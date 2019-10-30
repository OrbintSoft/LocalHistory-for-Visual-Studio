using LOSTALLOY.LocalHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LocalHistory.Test
{
    [TestClass]
    public class TestStringExtensions
    {
        [TestMethod]
        public void TestReplace()
        {
            string test = null;
            Action a = new Action(() => { test.Replace("", "", StringComparison.Ordinal); });
        }
    }
}
