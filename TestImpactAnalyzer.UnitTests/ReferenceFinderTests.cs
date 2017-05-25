using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestImpactAnalyzer.Lib;

namespace TestImpactAnalyzer.UnitTests
{
    [TestClass]
    public class ReferenceFinderTests
    {
        [TestInitialize]
        public void SetUp()
        {
            var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);
        }

        [TestMethod]
        public void Constructor_UsingSolutionPath_MSBuildWorkspaceCreated()
        {
            var solutionPath = "C:\\Projects\\unity3d\\tia\\TestImpactAnalyzer.sln";

            var finder = new ReferenceFinder(solutionPath);

            Assert.IsInstanceOfType(finder.Workspace, typeof(MSBuildWorkspace));
        }

        [TestMethod]
        public void Constructor_UsingSolutionInfo_AdhocWorkspaceCreated()
        {
            var finder = new ReferenceFinder(new AdhocWorkspace());

            Assert.IsInstanceOfType(finder.Workspace, typeof(AdhocWorkspace));
        }

        [TestMethod]
        public void FindClassUsages_ForTheClassFromTheSameProject_ReturnsSingleLocation()
        {
            var workspace = CreateWorkspace();

            var finder = new ReferenceFinder(workspace);

            var references = finder.FindClassUsages("Class1.cs", "Class1");

            Assert.AreEqual(1, references.Count());
        }

        [TestMethod]
        public void FindAffectedMsTests_ForTheClassFromTheSameProject_ReturnsSingleLocation()
        {
            var workspace = CreateWorkspace("[TestClass]");

            var finder = new ReferenceFinder(workspace);

            var references = finder.FindAffectedTests("Class1.cs", "Class1");

            Assert.AreEqual(1, references.Count());            
        }
 
        [TestMethod]
        public void FindAffectedNUnitTests_ForTheClassFromTheSameProject_ReturnsSingleLocation()
        {
            var workspace = CreateWorkspace("[Test]");

            var finder = new ReferenceFinder(workspace);

            var references = finder.FindAffectedTests("Class1.cs", "Class1");

            Assert.AreEqual(1, references.Count());            
        }
               
        private static AdhocWorkspace CreateWorkspace(string testAttribute = "")
        {
            var workspace = new AdhocWorkspace();
            var projectName = "Project1";
            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();
            var projectInfo = ProjectInfo.Create(projectId, versionStamp, projectName, projectName, LanguageNames.CSharp);
            var newProject = workspace.AddProject(projectInfo);
            var class1SourceText = SourceText.From("class Class1 {}");
            var textLoader1 = TextLoader.From(TextAndVersion.Create(class1SourceText, VersionStamp.Create()));
            var documentInfo1 = DocumentInfo.Create(DocumentId.CreateNewId(projectId), "Class1.cs", null, SourceCodeKind.Regular, textLoader1, "Class1.cs");
            workspace.AddDocument(documentInfo1);
            var class2SourceText = SourceText.From($"{testAttribute}class  Class2 {{ Class2() {{ var c1 = new Class1(); }} }}");
            var textLoader2 = TextLoader.From(TextAndVersion.Create(class2SourceText, VersionStamp.Create()));
            var documentInfo2 = DocumentInfo.Create(DocumentId.CreateNewId(projectId), "Class2.cs", null, SourceCodeKind.Regular, textLoader2, "Class2.cs");
            workspace.AddDocument(documentInfo2);
            return workspace;
        }
    }
}
