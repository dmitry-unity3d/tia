using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestImpactAnalyzer.Lib.Utils;

namespace TestImpactAnalyzer.UnitTests
{
    [TestClass]
    public class CommandLineParametersTests
    {
        [TestMethod]
        public void Parse_RunTestsParameterExists_RunTestsIsTrue()
        {
            var actualParameters = CommandLineParameters.Parse(new[] { "--run-tests" });

            Assert.IsTrue(actualParameters.RunTests);
        }

        [TestMethod]
        public void Parse_RunTestsParameterNotExists_RunTestsIsFalse()
        {
            var actualParameters = CommandLineParameters.Parse(new[] { "--solution-path", "c:\\aaa\\a.sln", "--revision", "aa23d54f" });

            Assert.IsFalse(actualParameters.RunTests);
        }

        [TestMethod]
        public void Parse_WaitOnFinishParameterExists_WaitOnFinishIsTrue()
        {
            var actualParameters = CommandLineParameters.Parse(new[] { "--wait" });

            Assert.IsTrue(actualParameters.WaitOnFinish);
        }

        [TestMethod]
        public void Parse_WaitOnFinishParameterNotExists_WaitOnFinishIsFalse()
        {
            var actualParameters = CommandLineParameters.Parse(new[] { "--solution-path", "c:\\aaa\\a.sln", "--revision", "aa23d54f" });

            Assert.IsFalse(actualParameters.WaitOnFinish);
        }

        [TestMethod]
        public void Parse_SolutionPathParameterNotExists_SolutionPathIsEmpty()
        {
            var actualParameters = CommandLineParameters.Parse(new[] { "--run-tests", "--revision", "aa23d54f" });

            Assert.IsTrue(string.IsNullOrEmpty(actualParameters.SolutionPath));
        }

        [TestMethod]
        public void Parse_SolutionPathParameterExists_SolutionPathIsSetProperly()
        {
            var expectedSolutionPath = "c:\\aaa\\a.sln";
            var actualParameters = CommandLineParameters.Parse(new[] { "--run-tests", "--solution-path", $"{expectedSolutionPath}" });

            Assert.AreEqual(expectedSolutionPath, actualParameters.SolutionPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Parse_SolutionPathParameterExistsInTheEndOfTheList_ArgumentExceptionIsThrown()
        {
            CommandLineParameters.Parse(new[] { "--run-tests", "--solution-path" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Parse_SolutionPathParameterExistsWithoutValue_ArgumentExceptionIsThrown()
        {
            CommandLineParameters.Parse(new[] { "--solution-path", "--run-tests" });
        }

        [TestMethod]
        public void Parse_RevisionParameterNotExists_RevisionIsEmpty()
        {
            var actualParameters = CommandLineParameters.Parse(new[] { "--run-tests", "--solution-path", "c:\\aaa\\a.sln" });

            Assert.IsTrue(string.IsNullOrEmpty(actualParameters.Revision));
        }

        [TestMethod]
        public void Parse_RevisionParameterExists_RevisionIsSetProperly()
        {
            var expectedRevision = "aa23d54f";
            var actualParameters = CommandLineParameters.Parse(new[] { "--run-tests", "--revision", $"{expectedRevision}" });

            Assert.AreEqual(expectedRevision, actualParameters.Revision);
        }

        [TestMethod]
        public void Parse_WorkingFolderParameterNotExists_WorkingFolderIsEmpty()
        {
            var actualParameters = CommandLineParameters.Parse(new[] { "--run-tests", "--solution-path", "c:\\aaa\\a.sln" });

            Assert.IsTrue(string.IsNullOrEmpty(actualParameters.WorkingFolder));
        }

        [TestMethod]
        public void Parse_WorkingFolderParameterExists_WorkingFolderIsSetProperly()
        {
            var workingFolder = "c:\\aaa\\";
            var actualParameters = CommandLineParameters.Parse(new[] { "--run-tests", "--working-folder", $"{workingFolder}" });

            Assert.AreEqual(workingFolder, actualParameters.WorkingFolder);
        }
        
        [TestMethod]
        public void Parse_OutputFormattingParameterNotExists_OutputFormattingIsJson()
        {
            var actualParameters = CommandLineParameters.Parse(new[] { "--run-tests", "--solution-path", "c:\\aaa\\a.sln" });

            Assert.AreEqual(FormattingType.Json, actualParameters.OutputFormatting);
        }

        [TestMethod]
        public void Parse_OutputFormattingParameterExists_OutputFormattingIsSetProperly()
        {
            var expectedFormatting = FormattingType.Text;
            var formatting = expectedFormatting.ToString().ToLower();
            var actualParameters = CommandLineParameters.Parse(new[] { "--run-tests", "--formatting", $"{formatting}" });

            Assert.AreEqual(expectedFormatting, actualParameters.OutputFormatting);
        }

        [TestMethod]
        public void TryParse_WrongParametersSpecified_ReturnsFalse()
        {
            CommandLineParameters parameters;
            var parsed = CommandLineParameters.TryParse(new[] { "--run-tests", "--solution-path" }, out parameters);

            Assert.IsFalse(parsed);
        }

        [TestMethod]
        public void TryParse_CorrectParametersSpecified_ReturnsTrue()
        {
            CommandLineParameters parameters;
            var parsed = CommandLineParameters.TryParse(new[] { "--run-tests", "--solution-path", "c:\\aaa\\a.sln" }, out parameters);

            Assert.IsTrue(parsed);
        }
    }
}