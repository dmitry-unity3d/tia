using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TestImpactAnalyzer.Lib;

namespace TestImpactAnalyzer.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //var className = "Class1";
            //var solutionPath = "C:\\Projects\\unity3d\\tia\\TestImpactAnalyzer.sln";

            //var workingFolder = "C:\\Projects\\unity3d\\unity";
            var workingFolder = args[0];
            var solutionPath = $"{workingFolder}\\Projects\\CSharp\\Unity.CSharpProjects.gen.sln";

            System.Console.WriteLine("Getting changed file(s)...");
            var mercurialClient = new MercurialClient(workingFolder);
            var changes = mercurialClient.GetChangedFiles();
            foreach (var changedFile in changes)
            {
                System.Console.WriteLine(changedFile);
            }
            System.Console.WriteLine();

            var classPath = changes[0];
            var className = Path.GetFileNameWithoutExtension(classPath);

            System.Console.WriteLine("Searching for references in solution...");
            var finder = new ReferenceFinder(solutionPath);
            //var locations = finder.FindClassUsages($"C:\\Projects\\unity3d\\tia\\TestImpactAnalyzer.Lib.Samples\\{className}.cs", $"{className}");
            var locations = finder.FindClassUsages(classPath, className);
            System.Console.WriteLine("Affected file(s):");
            foreach (var location in locations)
            {
                System.Console.WriteLine(location.Document.Name);
            }
            System.Console.WriteLine();

            System.Console.WriteLine("Affected unit test(s):");
            var unitTests = ReferenceFinder.GetUnitTestLocations(locations);
            var testsAssemlblies = new List<string>();
            var testsNames = new List<string>();
            foreach (var location in unitTests)
            {
                System.Console.WriteLine(location.Document.Name);
                testsAssemlblies.Add(location.Document.Project.OutputFilePath);
                testsNames.Add($"test =~ /{Path.GetFileNameWithoutExtension(location.Document.Name)}/");
            }
            System.Console.WriteLine();

            System.Console.WriteLine("Running affected unit test(s)...");
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
                System.Console.WriteLine(line);
            }
            System.Console.ReadLine();
        }
    }
}
