using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.CSharp;

namespace TestImpactAnalyzer.Console
{
    class ReferenceFinder
    {
        public void Find(string solutionPath, string methodName)
        {
            var msWorkspace = MSBuildWorkspace.Create();

            System.Console.WriteLine("Searching for class \"{0}\" reference in solution {1} ", "Class1", Path.GetFileName(solutionPath));
            ISymbol classSymbol = null;
            bool found = false;

            var solution = msWorkspace.OpenSolutionAsync(solutionPath).Result;
            foreach (var project in solution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    var model = document.GetSemanticModelAsync().Result;

                    var syntaxRoot = document.GetSyntaxRootAsync().Result;
                    InvocationExpressionSyntax node;
                    try
                    {
                        node = syntaxRoot.DescendantNodes().OfType<InvocationExpressionSyntax>()
                            .Where(x => ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == methodName)
                            .FirstOrDefault();

                        if (node == null)
                            continue;
                    }
                    catch(Exception)
                    {
                        // Swallow the exception of type cast. 
                        // Could be avoided by a better filtering on above linq.
                        continue;
                    }

                    var methodSymbol = model.GetSymbolInfo(node).Symbol;
                    classSymbol = methodSymbol.ContainingSymbol;
                    found = true;
                    break;
                }

                if (found) break;
            }

            var referencedSymbols = SymbolFinder.FindReferencesAsync(classSymbol, solution).Result;

            var queue = new Queue<ReferencedSymbol>();
            var visited = new Dictionary<string, ReferenceLocation>();

            foreach (var referencedSymbol in referencedSymbols)
            {
                queue.Enqueue(referencedSymbol);
            }

            while (queue.Count != 0)
            {
                var referencedSymbol = queue.Dequeue();
                foreach (var location in referencedSymbol.Locations)
                {
                    if (!visited.ContainsKey(location.Document.FilePath))
                    {
                        visited.Add(location.Document.FilePath, location);
                    }

                    var referencedClassSymbol = GetClassSymbol(location);
                    var symbols = SymbolFinder.FindReferencesAsync(referencedClassSymbol, solution).Result;
                    foreach (var symbol in symbols)
                    {
                        var allLocationsExist = true;
                        foreach (var symbolLocation in symbol.Locations)
                        {
                            allLocationsExist &= visited.ContainsKey(symbolLocation.Document.FilePath);
                            if (!allLocationsExist)
                            {
                                break;
                            }
                        }

                        if (!allLocationsExist)
                        {
                            queue.Enqueue(symbol);
                        }
                    }
                }
            }

            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Found references in files.");
            System.Console.ResetColor();
            foreach (var location in visited.Values)
            {
                System.Console.WriteLine("File: " + location.Document.Name);
            }

            var unitTests = GetUnitTestLocations(visited.Values);
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Found references in unit tests.");
            System.Console.ResetColor();
            foreach (var location in unitTests)
            {
                System.Console.WriteLine("File: " + location.Document.Name);
            }

            System.Console.WriteLine("Finished searching. Press any key to continue....");
        }

        private static IEnumerable<ReferenceLocation> GetUnitTestLocations(IEnumerable<ReferenceLocation> locations)
        {
            var unitTests = new List<ReferenceLocation>();
            foreach (var fileLocation in locations)
            {
                if (IsUnitTestsFile(fileLocation))
                {
                    unitTests.Add(fileLocation);
                }
            }
            return unitTests;
        }

        private static bool IsUnitTestsFile(ReferenceLocation referenceLocation)
        {
            var classNode = GetClassNode(referenceLocation);
            var childNodes = classNode.ChildNodes();
            var attributesNode = childNodes.OfType<AttributeListSyntax>().FirstOrDefault();
            return attributesNode != null && attributesNode.Attributes.Any(a => a.ToString() == "TestClass");
        }

        private static SyntaxNode GetClassNode(ReferenceLocation referenceLocation)
        {
            var methodNode = GetMethodNode(referenceLocation);
            return methodNode.Parent;
        }

        private static ISymbol GetClassSymbol(ReferenceLocation location)
        {
            SyntaxNode referencedClassNode = GetMethodNode(location);
            var semanticModel = location.Document.GetSemanticModelAsync().Result;
            var referencedClassSymbol = semanticModel.GetDeclaredSymbol(referencedClassNode);
            return referencedClassSymbol.ContainingSymbol;
        }

        private static SyntaxNode GetMethodNode(ReferenceLocation location)
        {
            var referenceLocation = location.Location;
            var lineSpan = referenceLocation.SourceSpan;
            var root = location.Document.GetSyntaxRootAsync().Result;
            var locationNode = root.DescendantNodes(lineSpan).First(n => lineSpan.Contains(n.Span));
            SyntaxNode referencedClassNode = FindParentFor(locationNode, SyntaxKind.MethodDeclaration);
            return referencedClassNode;
        }

        private static SyntaxNode FindParentFor(SyntaxNode node, SyntaxKind parentSyntaxKind)
        {
            SyntaxNode parentNode = node;
            while (parentNode != null)
            {
                if (parentNode.Kind() == parentSyntaxKind)
                {
                    break;
                }
                parentNode = parentNode.Parent;
            }
            return parentNode;
        }
    }
}
