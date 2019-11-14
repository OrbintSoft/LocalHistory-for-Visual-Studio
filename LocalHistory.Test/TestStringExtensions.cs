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
            test.Replace("abcdef", null, StringComparison.InvariantCulture).Should().Be("");
            test.Replace("abCDef", null, StringComparison.InvariantCultureIgnoreCase).Should().Be("");
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
            @"C:\parentdir\dir\solution".IsSubPathOf(@"C:\parentdir").Should().BeTrue();
            @"C:\parentdir".IsSubPathOf(@"C:\parentdir\dir\solution").Should().BeFalse();
            a = new Action(() => { test.IsSubPathOf(@"|-<>&$%&\!/"); });
            a.Should().Throw<ArgumentException>().Where(e => e.ParamName == "baseDirPath");
            test = @"!£|\/=/&-_<>";
            a = new Action(() => { test.IsSubPathOf(@"C:\parentdir"); });
            a.Should().Throw<ArgumentException>().Where(e => e.ParamName == "path");
            @"C:\parentdir\dir\solution\file.txt".IsSubPathOf(@"C:\parentdir").Should().BeTrue();
        }

        [TestMethod]
        public void TestWithEnding()
        {
            string test = null;
            test.WithEnding("").Should().Be("");
            Action a = new Action(() => { "sjidoi".WithEnding(null); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "ending");
            "abcdefg".WithEnding("hilmno").Should().Be("abcdefghilmno");
            "abcdefg".WithEnding("efg").Should().Be("abcdefg");
            "".WithEnding("efg").Should().Be("efg");
            "abcdefg".WithEnding("efghilm").Should().Be("abcdefghilm");
            "abcdefg".WithEnding("EFG").Should().Be("abcdefgEFG");
            "abcdefg".WithEnding("EFGHI").Should().Be("abcdefgEFGHI");
        }

        [TestMethod]
        public void TestRight()
        {
            string test = null;
            Action a = new Action(() => { test.Right(5); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "value");
            a = new Action(() => { "aaa".Right(-1); });
            a.Should().Throw<ArgumentOutOfRangeException>().Where(e => e.ParamName == "length"); ;
            "abcde".Right(0).Should().Be("");
            "abcde".Right(1).Should().Be("e");
            "abcde".Right(2).Should().Be("de");
            "abcde".Right(3).Should().Be("cde");
            "abcde".Right(4).Should().Be("bcde");
            "abcde".Right(5).Should().Be("abcde");
            "abcde".Right(6).Should().Be("abcde");
            "abcde".Right(int.MaxValue).Should().Be("abcde");
            "".Right(3).Should().Be("");
            "".Right(0).Should().Be("");
        }

        [TestMethod]
        public void TestEscapeFileVersionSeparator()
        {
            string test = null;
            Action a = new Action(() => { test.EscapeFileVersionSeparator(); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "value");
            "".EscapeFileVersionSeparator().Should().Be("");
            "$".EscapeFileVersionSeparator().Should().Be("$$");
            "$$".EscapeFileVersionSeparator().Should().Be("$$$$");
            "$$$".EscapeFileVersionSeparator().Should().Be("$$$$$$");
            "a$".EscapeFileVersionSeparator().Should().Be("a$$");
            "$a".EscapeFileVersionSeparator().Should().Be("$$a");
            "abcd".EscapeFileVersionSeparator().Should().Be("abcd");
        }

        [TestMethod]
        public void TestUnescapeFileVersionSeparator()
        {
            string test = null;
            Action a = new Action(() => { test.UnescapeFileVersionSeparator(); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "value");
            "".UnescapeFileVersionSeparator().Should().Be("");
            "$".UnescapeFileVersionSeparator().Should().Be("$");
            "$$".UnescapeFileVersionSeparator().Should().Be("$");
            "$$$".UnescapeFileVersionSeparator().Should().Be("$$");
            "$$$$".UnescapeFileVersionSeparator().Should().Be("$$");
            "$$$$$".UnescapeFileVersionSeparator().Should().Be("$$$");
            "$$$$$$".UnescapeFileVersionSeparator().Should().Be("$$$");
            "a$$".UnescapeFileVersionSeparator().Should().Be("a$");
            "$$a".UnescapeFileVersionSeparator().Should().Be("$a");
            "abcd".UnescapeFileVersionSeparator().Should().Be("abcd");
        }
    }
}
