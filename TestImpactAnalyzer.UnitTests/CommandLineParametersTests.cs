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
    }
}
