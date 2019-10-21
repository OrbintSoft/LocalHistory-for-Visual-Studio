using System;
using LOSTALLOY.LocalHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.IO;

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

            //test wrong path
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
        }
    }
}
