using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestImpactAnalyzer.Lib;

namespace TestImpactAnalyzer.UnitTests
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestMethod1()
        {
            var c = new Class3();

            Assert.AreEqual(15, c.GetInt3());
        }
    }
}
