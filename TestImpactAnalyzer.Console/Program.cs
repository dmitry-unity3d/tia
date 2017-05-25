using System;
using System.IO;
using TestImpactAnalyzer.Lib;

namespace TestImpactAnalyzer.Console
{
    class Program
    {
        static void Main()
        {
            var className = "Class1";
            var solutionPath = "C:\\Projects\\unity3d\\tia\\TestImpactAnalyzer.sln";

            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Searching for class \"{0}\" reference in solution {1} ", className, Path.GetFileName(solutionPath));

            var finder = new ReferenceFinder(solutionPath);
            var locations = finder.FindClassUsages($"C:\\Projects\\unity3d\\tia\\TestImpactAnalyzer.Lib.Samples\\{className}.cs", $"{className}");
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
            foreach (var location in unitTests)
            {
                System.Console.WriteLine("File: " + location.Document.Name);
            }
        }
    }
}
