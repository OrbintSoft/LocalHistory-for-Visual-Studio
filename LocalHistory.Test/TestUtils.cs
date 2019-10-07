using System;
using LOSTALLOY.LocalHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace LocalHistory.Test
{
    [TestClass]
    public class TestUtils
    {
        [TestMethod]
        public void TestNormalizePath()
        {
            //test null
            var a = new Action(() => Utils.NormalizePath(null));
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "path");

            //test slashes normalization
            Utils.NormalizePath("C:/aaa/bbb").Should().Be(@"C:\aaa\bbb");

            //test invalid path            
            a = new Action(() => Utils.NormalizePath("!\"£$%&/()=?'.;\\|:-_+*[]<>"));
            a.Should().Throw<ArgumentException>().Where(e => e.Message.StartsWith("path is invalid"));

            //test relative path to full
            var currentDir = Environment.CurrentDirectory;
            Utils.NormalizePath(".").Should().Be(currentDir);
        }
    }
}
