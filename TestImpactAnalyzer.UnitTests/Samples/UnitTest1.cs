﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestImpactAnalyzer.Lib.Samples;

namespace TestImpactAnalyzer.UnitTests.Samples
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var c = new Class2();

            Assert.AreEqual(15, c.GetInt2());
        }
    }
}
