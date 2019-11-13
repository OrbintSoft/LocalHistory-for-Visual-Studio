using LOSTALLOY.LocalHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using FluentAssertions;

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
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "str");
            test = "abcdef";
            a = new Action(() => { test.Replace(null, "", StringComparison.Ordinal); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "oldValue");
            a = new Action(() => { test.Replace("", "", StringComparison.Ordinal); });
            a.Should().Throw<ArgumentException>().Where(e => e.Message == "String cannot be of zero length.");
            test.Replace("bcd", "ghi", StringComparison.InvariantCulture).Should().Be("aghief");
            test.Replace("BCD", "ghi", StringComparison.InvariantCulture).Should().Be("abcdef");
            test.Replace("BCD", "ghi", StringComparison.InvariantCultureIgnoreCase).Should().Be("aghief");
            test.Replace("abcdef", "abcdef", StringComparison.InvariantCultureIgnoreCase).Should().Be("abcdef");
            test.Replace("abcdef", "ABCDEF", StringComparison.InvariantCultureIgnoreCase).Should().Be("ABCDEF");
            test.Replace("bcd", "GHI", StringComparison.InvariantCultureIgnoreCase).Should().Be("aGHIef");
            test.Replace("abcdef", "", StringComparison.InvariantCultureIgnoreCase).Should().Be("");
            test.Replace("bcd", null, StringComparison.InvariantCulture).Should().Be("aef");
            test = "";
            test.Replace("a", "a", StringComparison.Ordinal).Should().Be("");
        }

        [TestMethod]
        public void TestIsSubPathOf()
        {
            string test = null;
            Action a = new Action(() => { test.IsSubPathOf(""); });
            a.Should().Throw<ArgumentNullException>().Where( e => e.ParamName == "path");
            test = @"C:\parentdir\dir\solution";
            a = new Action(() => { test.IsSubPathOf(null); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "baseDirPath");
            @"C:\parentdir\dir\solution".IsSubPathOf(@"C:\parentdir\dir\solution\folder\file.txt").Should().BeTrue();
        }
    }
}
