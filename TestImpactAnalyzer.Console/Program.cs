using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            var writeOutput = commandLineParamters.OutputFormatting == FormattingType.Text;

            Trace.WriteLineIf(writeOutput, "Getting changed file(s)...");
            var mercurialClient = new MercurialClient(workingFolder);
            var changes = mercurialClient.GetChangedFiles();
            foreach (var changedFile in changes)
            {
                Trace.WriteLineIf(writeOutput, changedFile);
            }
            Trace.WriteLineIf(writeOutput, "");

            var classPath = changes[0];
            var className = Path.GetFileNameWithoutExtension(classPath);

            Trace.WriteLineIf(writeOutput, "Searching for references in solution...");
            var finder = new ReferenceFinder(solutionPath);
            var locations = finder.FindClassUsages(classPath, className);
            Trace.WriteLineIf(writeOutput, "Affected file(s):");
            foreach (var location in locations)
            {
                Trace.WriteLineIf(writeOutput, location.Document.Name);
            }
            Trace.WriteLineIf(writeOutput, "");

            if (!commandLineParamters.RunTests)
            {
                return;
            }
            Trace.WriteLineIf(writeOutput, "Affected unit test(s):");
            var unitTests = ReferenceFinder.GetUnitTestLocations(locations);
            var testsAssemlblies = new List<string>();
            var testsNames = new List<string>();
            foreach (var location in unitTests)
            {
                System.Console.WriteLine(location.Document.Name);
                testsAssemlblies.Add(location.Document.Project.OutputFilePath);
                testsNames.Add($"test =~ /{Path.GetFileNameWithoutExtension(location.Document.Name)}/");
            }
            Trace.WriteLineIf(writeOutput, "");

            Trace.WriteLineIf(writeOutput, "Running affected unit test(s)...");
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
                Trace.WriteLineIf(writeOutput, line);
            }
            System.Console.ReadLine();
        }
    }
}
