using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TestImpactAnalyzer.Lib;

namespace TestImpactAnalyzer.Console
{
    class Program
    {
        static void Main()
        {
            //var className = "Class1";
            //var solutionPath = "C:\\Projects\\unity3d\\tia\\TestImpactAnalyzer.sln";

            var workingFolder = "C:\\Projects\\unity3d\\unity";
            var solutionPath = $"{workingFolder}\\Projects\\CSharp\\Unity.CSharpProjects.gen.sln";

            var mercurialClient = new MercurialClient(workingFolder);
            var changes = mercurialClient.GetChangedFiles();

            var classPath = changes[0];
            var className = Path.GetFileNameWithoutExtension(classPath);

            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Searching for class \"{0}\" reference in solution {1} ", className, Path.GetFileName(solutionPath));

            var finder = new ReferenceFinder(solutionPath);
            //var locations = finder.FindClassUsages($"C:\\Projects\\unity3d\\tia\\TestImpactAnalyzer.Lib.Samples\\{className}.cs", $"{className}");
            var locations = finder.FindClassUsages(classPath, className);
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Found references in files.");
            System.Console.ResetColor();
            foreach (var location in locations)
            {
                System.Console.WriteLine("File: " + location.Document.Name);
            }

            var unitTests = ReferenceFinder.GetUnitTestLocations(locations);
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Found references in unit tests.");
            System.Console.ResetColor();
            var testsAssemlblies = new List<string>();
            var testsNames = new List<string>();
            foreach (var location in unitTests)
            {
                System.Console.WriteLine("File: " + location.Document.Name);
                testsAssemlblies.Add(location.Document.Project.OutputFilePath);
                testsNames.Add($"test =~ /{Path.GetFileNameWithoutExtension(location.Document.Name)}/");
            }

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
        }
    }
}
