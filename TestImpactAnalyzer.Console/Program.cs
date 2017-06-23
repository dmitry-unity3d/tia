using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TestImpactAnalyzer.Lib;
using TestImpactAnalyzer.Lib.Utils;

namespace TestImpactAnalyzer.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            CommandLineParameters commandLineParamters;
            if (!CommandLineParameters.TryParse(args, out commandLineParamters))
            {
                return;
            }
            var workingFolder = commandLineParamters.WorkingFolder;
            var solutionPath = commandLineParamters.SolutionPath;
            var textOutput = commandLineParamters.OutputFormatting == FormattingType.Text;

            Trace.WriteLineIf(textOutput, "Getting changed file(s)...");
            var mercurialClient = new MercurialClient(workingFolder);
            var changes = mercurialClient.GetChangedFiles(commandLineParamters.Revision);
            foreach (var changedFile in changes)
            {
                Trace.WriteLineIf(textOutput, changedFile);
            }
            Trace.WriteLineIf(textOutput, "");

            var classPath = changes[0];
            var className = Path.GetFileNameWithoutExtension(classPath);

            Trace.WriteLineIf(textOutput, "Searching for references in solution...");
            var finder = new ReferenceFinder(solutionPath);
            var locations = finder.FindClassUsages(classPath, className);
            Trace.WriteLineIf(textOutput, "Affected file(s):");
            foreach (var location in locations)
            {
                Trace.WriteLineIf(textOutput, location.Document.Name);
            }
            Trace.WriteLineIf(textOutput, "");

            Trace.WriteLineIf(textOutput, "Affected unit test(s):");
            var unitTests = ReferenceFinder.GetUnitTestLocations(locations);
            var unitTestFiles = new List<string>(unitTests.Select(ul => ul.Document.FilePath));
            foreach (var unitTestFile in unitTestFiles)
            {
                Trace.WriteLineIf(!textOutput, unitTestFile);
            }

            if (!commandLineParamters.RunTests)
            {
                return;
            }
            var testsAssemlblies = new List<string>();
            var testsNames = new List<string>();
            foreach (var location in unitTests)
            {
                Trace.WriteLineIf(textOutput, location.Document.Name);
                testsAssemlblies.Add(location.Document.Project.OutputFilePath);
                testsNames.Add($"test =~ /{Path.GetFileNameWithoutExtension(location.Document.Name)}/");
            }
            Trace.WriteLineIf(textOutput, "");

            Trace.WriteLineIf(textOutput, "Running affected unit test(s)...");
            var testAssembliesCombined = string.Join(" ", testsAssemlblies);
            var testsExpression = string.Join(" || ", testsNames);

            var runTestCommandProcess = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = $"{workingFolder}\\External\\NUnit\\nunit-console.exe",
                    Arguments = $"{testAssembliesCombined} --where \"{testsExpression}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            runTestCommandProcess.Start();
            while (!runTestCommandProcess.StandardOutput.EndOfStream) {
                var line = runTestCommandProcess.StandardOutput.ReadLine();
                Trace.WriteLineIf(textOutput, line);
            }

            if (commandLineParamters.WaitOnFinish)
            {
                System.Console.ReadLine();   
            }
        }
    }
}
