using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;

namespace TestImpactAnalyzer.Lib
{
    public class ReferenceFinder
    {
        private readonly string _solutionPath;

        public ReferenceFinder(string solutionPath)
        {
            _solutionPath = solutionPath;
            Workspace = MSBuildWorkspace.Create();
        }

        public ReferenceFinder(Workspace workspace)
        {
            Workspace = workspace;
        }

        public Workspace Workspace { get; private set; }

        public IEnumerable<ReferenceLocation> FindClassUsages(string filePath, string className)
        {
            var solution = Workspace.CurrentSolution;
            if (!string.IsNullOrEmpty(_solutionPath))
            {
                solution = ((MSBuildWorkspace)Workspace).OpenSolutionAsync(_solutionPath).Result;
            }
            var document = FindDocument(solution, filePath);

            if (document == null)
            {
                throw new InvalidOperationException();
            }

            var model = document.GetSemanticModelAsync().Result;
            var syntaxRoot = document.GetSyntaxRootAsync().Result;
            ClassDeclarationSyntax node = syntaxRoot.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(x => x.Identifier.Text == className);
            var classSymbol = model.GetDeclaredSymbol(node);
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
            return visited.Values;
        }

        public IEnumerable<ReferenceLocation> FindAffectedTests(string classPath, string className)
        {
            var classReferences = FindClassUsages(classPath, className);
            var unitTests = GetUnitTestLocations(classReferences);
            return unitTests;
        }

        public static IEnumerable<ReferenceLocation> GetUnitTestLocations(IEnumerable<ReferenceLocation> locations)
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
        
        private static Document FindDocument(Solution solution, string filePath)
        {
            Document document = null;
            foreach (var solutionProject in solution.Projects)
            {
                foreach (var projectDocument in solutionProject.Documents)
                {
                    if (projectDocument.FilePath == filePath)
                    {
                        document = projectDocument;
                    }
                }
            }
            return document;
        }

        private static bool IsUnitTestsFile(ReferenceLocation referenceLocation)
        {
            var classNode = GetClassNode(referenceLocation);
            var childNodes = classNode.ChildNodes();
            var attributesNode = childNodes.OfType<AttributeListSyntax>().FirstOrDefault();
            return attributesNode != null && attributesNode.Attributes.Any(a => a.ToString().Contains("Test"));
        }

        private static ISymbol GetClassSymbol(ReferenceLocation location)
        {
            SyntaxNode referencedClassNode = GetClassNode(location);
            var semanticModel = location.Document.GetSemanticModelAsync().Result;
            var referencedClassSymbol = semanticModel.GetDeclaredSymbol(referencedClassNode);
            return referencedClassSymbol;
        }

        private static SyntaxNode GetClassNode(ReferenceLocation location)
        {
            var referenceLocation = location.Location;
            var lineSpan = referenceLocation.SourceSpan;
            var root = location.Document.GetSyntaxRootAsync().Result;
            var locationNode = root.DescendantNodes(lineSpan).First(n => lineSpan.Contains(n.Span));
            SyntaxNode referencedClassNode = FindParentFor(locationNode, SyntaxKind.ClassDeclaration);
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
