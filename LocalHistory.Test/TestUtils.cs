using System;
using LOSTALLOY.LocalHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Globalization;
using System.Threading;
using Pri.LongPath;

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
            Utils.NormalizePath("aaa").Should().Be(Path.Combine(currentDir, "aaa"));

            //test empty string
            a = new Action(() => Utils.NormalizePath(""));
            a.Should().Throw<ArgumentException>().Where(e => e.Message.StartsWith("path is invalid"));

            //test trim
            Utils.NormalizePath("    C:\\aaa   ").Should().Be("C:\\aaa");
        }

        [TestMethod]
        public void TestGetRepositoryPathForFile()
        {
            //test null
            var a = new Action(() => Utils.GetRepositoryPathForFile(null, @"C:\solution"));
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "filePath");
            a = new Action(() => Utils.GetRepositoryPathForFile("file.txt", null));
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "solutionDirectory");

            //test invalid path
            a = new Action(() => Utils.GetRepositoryPathForFile(@"", @"C:\dir\solution"));
            a.Should().Throw<ArgumentException>().Where(e => e.Message.StartsWith("path is invalid") && e.ParamName == "filePath");
            a = new Action(() => Utils.GetRepositoryPathForFile(@"folder\file.txt", @""));
            a.Should().Throw<ArgumentException>().Where(e => e.Message.StartsWith("path is invalid") && e.ParamName == "solutionDirectory");
            a = new Action(() => Utils.GetRepositoryPathForFile(@"!£$%&/()=?'-_\|", @"C:\dir\solution"));
            a.Should().Throw<ArgumentException>().Where(e => e.Message.StartsWith("path is invalid") && e.ParamName == "filePath");
            a = new Action(() => Utils.GetRepositoryPathForFile(@"folder\file.txt", @"!£$%&/()=?'-_\|"));
            a.Should().Throw<ArgumentException>().Where(e => e.Message.StartsWith("path is invalid") && e.ParamName == "solutionDirectory");

            //test real case
            Utils.GetRepositoryPathForFile(@"folder\file.txt", @"C:\dir\solution").Should().Be(@"C:\dir\solution\.localhistory\folder");
            Utils.GetRepositoryPathForFile(@"aaa\folder\file.txt", @"C:\dir\solution").Should().Be(@"C:\dir\solution\.localhistory\aaa\folder");
            Utils.GetRepositoryPathForFile(@"file.txt", @"C:\dir\solution").Should().Be(@"C:\dir\solution\.localhistory");
            Utils.GetRepositoryPathForFile(@"C:\file.txt", @"C:\dir\solution").Should().Be(@"C:\dir\solution\.localhistory\C_\");
            Utils.GetRepositoryPathForFile(@"D:\folder\file.txt", @"C:\dir\solution").Should().Be(@"C:\dir\solution\.localhistory\D_\folder");
            Utils.GetRepositoryPathForFile(@"folderA\folderB\filenoext", @"C:\dir\solution").Should().Be(@"C:\dir\solution\.localhistory\folderA\folderB");
            Utils.GetRepositoryPathForFile(@"folderA\folderB\folderC", @"C:\dir\solution").Should().Be(@"C:\dir\solution\.localhistory\folderA\folderB");
            Utils.GetRepositoryPathForFile(@"folder\file.txt", @"dir\solution").Should().Be(@"dir\solution\.localhistory\folder");
            Utils.GetRepositoryPathForFile(@"folder\file.txt", @"dir\solution\directory.txt").Should().Be(@"dir\solution\directory.txt\.localhistory\folder");
        }

        [TestMethod]
        public void TestGetRootRepositoryPath()
        {
            //test null
            var a = new Action(() => Utils.GetRootRepositoryPath(null));
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "solutionDirectory");

            //test invalid path
            a = new Action(() => Utils.GetRootRepositoryPath(@""));
            a.Should().Throw<ArgumentException>().Where(e => e.Message.StartsWith("path is invalid") && e.ParamName == "solutionDirectory");
            a = new Action(() => Utils.GetRootRepositoryPath(@"!£$%&/()=?^-_"));
            a.Should().Throw<ArgumentException>().Where(e => e.Message.StartsWith("path is invalid") && e.ParamName == "solutionDirectory");
            //test real case
            Utils.GetRootRepositoryPath(@"solutionDir").Should().Be(@"solutionDir\.localhistory");
            Utils.GetRootRepositoryPath(@"C:\aaa\bbb\ccc").Should().Be(@"C:\aaa\bbb\ccc\.localhistory");
            Utils.GetRootRepositoryPath(@"aaa\bbb\ccc").Should().Be(@"aaa\bbb\ccc\.localhistory");
            Utils.GetRootRepositoryPath(@"C:\solution\directory.txt").Should().Be(@"C:\solution\directory.txt\.localhistory");
        }

        [TestMethod]
        public void TestToDateTime()
        {
            var utcMin = -62135510400; //0001-01-02 00:00:00 minimum DateTime that can be converted to any timezone
            var utcMax = 253402214399; //9999-12-30 23:59:59 maximum DateTime that can be converted to any timezone
            Utils.ToDateTime(0).Should().Be(new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime());
            var a = new Action(() => Utils.ToDateTime(long.MinValue));
            a.Should().Throw<ArgumentOutOfRangeException>().Where(e => e.ParamName == "unixTime" && e.Message.StartsWith("value is too low"));
            a = new Action(() => Utils.ToDateTime(utcMin - 1));
            a.Should().Throw<ArgumentOutOfRangeException>().Where(e => e.ParamName == "unixTime" && e.Message.StartsWith("value is too low"));
            Utils.ToDateTime(utcMin).Should().Be(new DateTime(1, 1, 2, 0, 0, 0).ToLocalTime());
            a = new Action(() => Utils.ToDateTime(long.MaxValue));
            a.Should().Throw<ArgumentOutOfRangeException>().Where(e => e.ParamName == "unixTime" && e.Message.StartsWith("value is too big"));
            a = new Action(() => Utils.ToDateTime(utcMax + 1));
            a.Should().Throw<ArgumentOutOfRangeException>().Where(e => e.ParamName == "unixTime" && e.Message.StartsWith("value is too big"));
            Utils.ToDateTime(utcMax).Should().Be(new DateTime(9999, 12, 30, 23, 59, 59).ToLocalTime());
            Utils.ToDateTime(1571833453).Should().Be(new DateTime(2019, 10, 23, 12, 24, 13, DateTimeKind.Utc).ToLocalTime());
        }

        [TestMethod]
        public void TestToUnixTime()
        {
            var a = new Action(() => Utils.ToUnixTime(DateTime.MinValue));
            a.Should().Throw<ArgumentOutOfRangeException>().Where(e => e.ParamName == "dateTime" && e.Message.StartsWith("value is lower than 0001-01-02 00:00:00"));
            a = new Action(() => Utils.ToUnixTime(new DateTime(1,1,1,23,59,59,999)));
            a.Should().Throw<ArgumentOutOfRangeException>().Where(e => e.ParamName == "dateTime" && e.Message.StartsWith("value is lower than 0001-01-02 00:00:00"));
            var min = new DateTime(1, 1, 2, 0, 0, 0);
            var minLocal = (min).ToLocalTime();
            var diff = min - minLocal;
            Utils.ToUnixTime(min).Should().Be(-62135510400 + (long) diff.TotalSeconds);
            a = new Action(() => Utils.ToUnixTime(DateTime.MaxValue));
            a.Should().Throw<ArgumentOutOfRangeException>().Where(e => e.ParamName == "dateTime" && e.Message.StartsWith("value is bigger than 9999-12-30 23:59:59"));
            a = new Action(() => Utils.ToUnixTime(new DateTime(9999, 12, 31, 0, 0, 0)));
            a.Should().Throw<ArgumentOutOfRangeException>().Where(e => e.ParamName == "dateTime" && e.Message.StartsWith("value is bigger than 9999-12-30 23:59:59"));
            var max = new DateTime(9999, 12, 30, 23, 59, 59);
            var maxLocal = max.ToLocalTime();
            diff = max - maxLocal;
            Utils.ToUnixTime(max).Should().Be(253402214399 + (long)diff.TotalSeconds);
            Utils.ToUnixTime(new DateTime(2019, 10, 23, 12, 24, 13, DateTimeKind.Utc).ToLocalTime()).Should().Be(1571833453);
        }

        [TestMethod]
        public void TestIsValidPath()
        {
            Utils.IsValidPath(null).Should().BeFalse();
            Utils.IsValidPath("").Should().BeFalse();
            Utils.IsValidPath(" ").Should().BeFalse();
            Utils.IsValidPath(@"C:\aaa\bbbb").Should().BeTrue();
            Utils.IsValidPath(@"\aaa\bbbb").Should().BeTrue();
            Utils.IsValidPath(@"\aaa\bbbb\C:\ccc").Should().BeFalse();
            Utils.IsValidPath(@"C:\aaa\bbbb\..\cc").Should().BeTrue();
            Utils.IsValidPath(@".\aaa\bbbb\..\cc").Should().BeTrue();
            Utils.IsValidPath(@"aaa").Should().BeTrue();
            Utils.IsValidPath(@".\aaa\b  bb\..\cc").Should().BeTrue();
            Utils.IsValidPath(@".\<\b  bb\>\cc").Should().BeFalse();
            Utils.IsValidPath(@"\\netfoldet.foo\bar").Should().BeTrue();
        }

        [TestMethod]
        public void TestIsValidFileName()
        {
            Utils.IsValidFileName(null).Should().BeFalse();
            Utils.IsValidFileName("").Should().BeFalse();
            Utils.IsValidFileName(" ").Should().BeFalse();
            Utils.IsValidFileName(@"C:\aaa\bbbb").Should().BeFalse();
            Utils.IsValidFileName(@"\aaa\bbbb").Should().BeFalse();
            Utils.IsValidFileName(@".").Should().BeFalse();
            Utils.IsValidFileName(@"..").Should().BeFalse();
            Utils.IsValidFileName(@"..aaa").Should().BeTrue();
            Utils.IsValidFileName(@"a.").Should().BeFalse();
            Utils.IsValidFileName(@"<aaa>").Should().BeFalse();
            Utils.IsValidFileName(@"aa?a").Should().BeFalse();
            Utils.IsValidFileName(@"aaa").Should().BeTrue();
            Utils.IsValidFileName(@"aaa.txt").Should().BeTrue();
            Utils.IsValidFileName(@".tmp").Should().BeTrue();
            Utils.IsValidFileName(@"a-b_c.dat").Should().BeTrue();
        }
    }
}
