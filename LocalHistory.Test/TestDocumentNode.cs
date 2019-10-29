using LOSTALLOY.LocalHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using FluentAssertions;

namespace LocalHistory.Test
{
    [TestClass]
    public class TestDocumentNode
    {
        [TestMethod]
        public void TestConstructor()
        {
            var currentDate = DateTime.Now;
            //Test Null
            var a = new Action(() => { new DocumentNode(null, @"C:\dir\solution\folder", "file.txt", currentDate); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "repositoryPath");
            a = new Action(() => { new DocumentNode(@"C:\dir\solution\.localhistory", null, "file.txt", currentDate); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "originalPath");
            a = new Action(() => { new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", null, currentDate); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "originalFileName");
            a = new Action(() => { new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", "file.txt", null); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "unixTime");

            //Test bad parameters            
            a = new Action(() => { new DocumentNode("?", @"C:\dir\solution\folder", "file.txt", currentDate); });
            a.Should().Throw<ArgumentException>().Where(e => e.ParamName == "repositoryPath");
            a = new Action(() => { new DocumentNode(@"C:\dir\solution\.localhistory", @"?", "file.txt", currentDate); });
            a.Should().Throw<ArgumentException>().Where(e => e.ParamName == "originalPath");
            a = new Action(() => { new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"C:\folder", currentDate); });
            a.Should().Throw<ArgumentException>().Where(e => e.ParamName == "originalFileName");
            a = new Action(() => { new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", DateTime.MinValue); });
            a.Should().Throw<ArgumentOutOfRangeException>();
            a = new Action(() => { new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "AAA"); });
            a.Should().Throw<ArgumentException>().Where(e => e.ParamName == "unixTime");
            a = new Action(() => { new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", long.MinValue.ToString()); });
            a.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
