using LOSTALLOY.LocalHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using FluentAssertions;
using System.IO;

namespace LocalHistory.Test
{
    [TestClass]
    public class TestDocumentRepository
    {
        [TestMethod]
        public void TestConstructor()
        {
            var solution = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath;
            var directory = Path.Combine(solution, ".localhistory");
            var a = new Action(() => { new DocumentRepository(null, directory); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "solutionDirectory");
            a = new Action(() => { new DocumentRepository(solution, null); });
            a.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "repositoryDirectory");
            a = new Action(() => { new DocumentRepository("\\|!££$&//()'?^ì-_<>:;", directory); });
            a.Should().Throw<ArgumentException>().Where(e => e.ParamName == "solutionDirectory");
            a = new Action(() => { new DocumentRepository(solution, "\\|!££$&//()'?^ì-_<>:;"); });
            a.Should().Throw<ArgumentException>().Where(e => e.ParamName == "repositoryDirectory");

        }
    }
}
