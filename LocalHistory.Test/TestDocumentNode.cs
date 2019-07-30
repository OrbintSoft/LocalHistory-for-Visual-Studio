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
        public void Constructor()
        {
            //Test Null
            var a = new Action(() => { new DocumentNode(null, "C:\\", "D:\\", DateTime.Now); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "repositoryPath");
            a = new Action(() => { new DocumentNode("A:\\", null, "C:\\", DateTime.Now); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "originalPath");
            a = new Action(() => { new DocumentNode("A", "B", null, DateTime.Now); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "originalFileName");
            a = new Action(() => { new DocumentNode("A:\\", "B:\\", "C:\\", null); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "unixTime");

            //Test Valid Path



            //Test parameters
            var currentDate = DateTime.Now;
            var d = new DocumentNode("A", "B", "C", DateTime.MinValue);
            d.RepositoryPath.Should().Be("A");
            d.OriginalPath.Should().Be("B");
            d.RepositoryPath.Should().Be("C");

            // d = new DocumentNode("D", "E", "F", "G");

        }
    }
}
