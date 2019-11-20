using LOSTALLOY.LocalHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using FluentAssertions;
using Pri.LongPath;

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
            a = new Action(() => { new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363224793", @"C:\"); });
            a.Should().Throw<ArgumentException>().Where(e => e.ParamName == "label");

            a = new Action(() => { new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", currentDate); });
            a.Should().NotThrow();
            a = new Action(() => { new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632"); });
            a.Should().NotThrow();
            a = new Action(() => { new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632", "label"); });
            a.Should().NotThrow();
        }

        [TestMethod]
        public void TestHasLabel()
        {
            var d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632", "label");
            d.HasLabel.Should().BeTrue();
            d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632", null);
            d.HasLabel.Should().BeFalse();
            d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", DateTime.Now);
            d.HasLabel.Should().BeFalse();
            d.AddLabel("aaa");
            d.HasLabel.Should().BeTrue();
            d.RemoveLabel();
            d.HasLabel.Should().BeFalse();
        }

        [TestMethod]
        public void TestVersionFileFullFilePath()
        {
            var d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632", "label");
            d.VersionFileFullFilePath.Should().Be(@"C:\dir\solution\.localhistory\1572363632$file.txt$label");
            d.VersionFileFullFilePath.Should().Be(Path.Combine(d.RepositoryPath, d.VersionFileName));
            d.RemoveLabel();
            d.VersionFileFullFilePath.Should().Be(@"C:\dir\solution\.localhistory\1572363632$file.txt");
            d = new DocumentNode(@"C:\dir\solution\.localhistory", @"E:\anotherDir\anotherFolder", @"file.txt", "1572363632", "label");
            d.VersionFileFullFilePath.Should().Be(@"C:\dir\solution\.localhistory\1572363632$file.txt$label");
        }

        [TestMethod]
        public void TestVersionFileName()
        {
            var d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632", "label");
            d.VersionFileName.Should().Be(@"1572363632$file.txt$label");
            d.RemoveLabel();
            d.VersionFileName.Should().Be(@"1572363632$file.txt");
            d.AddLabel("shhdud");
            d.VersionFileName.Should().Be(@"1572363632$file.txt$shhdud");
            d.AddLabel("shh$dud");
            d.VersionFileName.Should().Be(@"1572363632$file.txt$shh$$dud");
            d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @".fi$le", "1572363632", "lab$$$el");
            d.VersionFileName.Should().Be(@"1572363632$.fi$$le$lab$$$$$$el");
            var date = new DateTime(2019, 11, 15);
            d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @".fi$le", date);
            d.VersionFileName.Should().Be(Utils.ToUnixTime(date).ToString() + "$.fi$$le");
        }

        [TestMethod]
        public void TestRepositoryPath()
        {
            var d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632", "label");
            d.RepositoryPath.Should().Be(@"C:\dir\solution\.localhistory");
            d = new DocumentNode(@"\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632", "label");
            d.RepositoryPath.Should().Be(@"C:\dir\solution\.localhistory");
            d = new DocumentNode(@"C:\dir\solution\.localhistory   ", @"C:\dir\solution\folder", @"file.txt", "1572363632", "label");
            d.RepositoryPath.Should().Be(@"C:\dir\solution\.localhistory");
            d = new DocumentNode(@"\\networkpath-t\shdh\hhdu", @"C:\dir\solution\folder", @"file.txt", "1572363632", "label");
            d.RepositoryPath.Should().Be(@"\\networkpath-t\shdh\hhdu");
        }

        [TestMethod]
        public void TestOriginalPath()
        {
            var d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632", "label");
            d.OriginalPath.Should().Be(@"C:\dir\solution\folder");
            d = new DocumentNode(@"\dir\solution\.localhistory", @"\dir\solution\folder", @"file.txt", "1572363632", "label");
            d.OriginalPath.Should().Be(@"C:\dir\solution\folder");
            d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder    ", @"file.txt", "1572363632", "label");
            d.OriginalPath.Should().Be(@"C:\dir\solution\folder");
            d = new DocumentNode(@"C:\dir\solution\.localhistory", @"\\networkpath-t\shdh\hhdu", @"file.txt", "1572363632", "label");
            d.OriginalPath.Should().Be(@"\\networkpath-t\shdh\hhdu");
        }

        [TestMethod]
        public void TestOriginalFileName()
        {
            var d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632", "label");
            d.OriginalFileName.Should().Be(@"file.txt");
            d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @".file", "1572363632", "label");
            d.OriginalFileName.Should().Be(@".file");
            d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file$.dat", "1572363632", "label");
            d.OriginalFileName.Should().Be(@"file$.dat");
        }

        [TestMethod]
        public void TestLabel()
        {
            var d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632", "label");
            d.Label.Should().Be(@"label");
            d.RemoveLabel();
            d.Label.Should().Be(null);
            d.AddLabel("shhd$ghd");
            d.Label.Should().Be("shhd$ghd");
            d.AddLabel("aaa");
            d.Label.Should().Be("aaa");
            d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632");
            d.Label.Should().BeNull();
        }

        [TestMethod]
        public void TestTimestamp()
        {
            var d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", new DateTime(2019, 11, 15, 14, 26, 33));
            d.Timestamp.Should().Be("2019-11-15 14:26:33");
            d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", Utils.ToUnixTime(new DateTime(2019, 11, 15, 14, 26, 33)).ToString(), "label");
            d.Timestamp.Should().Be("2019-11-15 14:26:33");
        }

        [TestMethod]
        public void TestTimestampAndLabel()
        {
            var d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", new DateTime(2019, 11, 15, 14, 26, 33));
            d.TimestampAndLabel.Should().Be("2019-11-15 14:26:33");
            d.AddLabel("djdjj");
            d.TimestampAndLabel.Should().Be("2019-11-15 14:26:33 djdjj");
            d.RemoveLabel();
            d.TimestampAndLabel.Should().Be("2019-11-15 14:26:33");
            d = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", Utils.ToUnixTime(new DateTime(2019, 11, 15, 14, 26, 33)).ToString(), "label");
            d.TimestampAndLabel.Should().Be("2019-11-15 14:26:33 label");
        }

        [TestMethod]
        public void TestEquality()
        {
            var a = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", new DateTime(2019, 11, 15, 14, 26, 33));
            var b = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", new DateTime(2019, 11, 15, 14, 26, 33));
            (a == b).Should().BeTrue();
            (a != b).Should().BeFalse();
            a.Should().Equals(b);
            a.GetHashCode().Should().Be(b.GetHashCode());
            a = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632", "aaa");
            b = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632", "bbb");
            (a == b).Should().BeTrue();
            (a != b).Should().BeFalse();
            a.Should().Equals(b);
            a.GetHashCode().Should().Be(b.GetHashCode());
            a = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"aaa.txt", "1572363632");
            b = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"bbv.txt", "1572363632");
            (a == b).Should().BeFalse();
            (a != b).Should().BeTrue();
            a.Should().NotBeEquivalentTo(b);
            a.GetHashCode().Should().NotBe(b.GetHashCode());
            a = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\aaa", @"file.txt", "1572363632");
            b = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\bbb", @"file.txt", "1572363632");
            (a == b).Should().BeFalse();
            (a != b).Should().BeTrue();
            a.Should().NotBeEquivalentTo(b);
            a.GetHashCode().Should().NotBe(b.GetHashCode());
            a = new DocumentNode(@"C:\dir\aaa\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632");
            b = new DocumentNode(@"C:\dir\bbb\.localhistory", @"C:\dir\solution\folder", @"file.txt", "1572363632");
            (a == b).Should().BeTrue();
            (a != b).Should().BeFalse();
            a.Should().Equals(b);
            a.GetHashCode().Should().Be(b.GetHashCode());
            a = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"FILE.txt", new DateTime(2019, 11, 15, 14, 26, 33));
            b = new DocumentNode(@"C:\dir\solution\.localhistory", @"C:\dir\solution\folder", @"file.txt", new DateTime(2019, 11, 15, 14, 26, 33));
            (a == b).Should().BeTrue();
            (a != b).Should().BeFalse();
            a.Should().Equals(b);
            a.GetHashCode().Should().Be(b.GetHashCode());
            a = null;
            b = null;
            (a == b).Should().BeTrue();
            (a != b).Should().BeFalse();
        }

        [TestMethod]
        public void TestAddRemoveLabel()
        {            
            var originalPath = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath;
            var repository = Path.Combine(originalPath, ".localhistory");         
            var d = new DocumentNode(repository, originalPath, "file.txt", new DateTime(2019, 11, 15, 14, 26, 33));
            d.Label.Should().BeNull();
            var parentDir = Path.GetDirectoryName(d.VersionFileFullFilePath);
            if (Directory.Exists(parentDir))
            {
                Directory.Delete(parentDir, true);
            }
            Directory.CreateDirectory(parentDir);
            using (var s = File.Create(d.VersionFileFullFilePath))
            {
                s.Write(new byte[] { 00 }, 0, 1);
            }
            d.AddLabel("test");
            d.Label.Should().Be("test");
            d.VersionFileFullFilePath.Should().EndWith("test");
            File.Exists(d.VersionFileFullFilePath).Should().BeTrue();
            d.AddLabel("aaa");
            d.Label.Should().Be("aaa");
            d.VersionFileFullFilePath.Should().EndWith("aaa");
            File.Exists(d.VersionFileFullFilePath).Should().BeTrue();
            d.AddLabel("aa$a");
            d.Label.Should().Be("aa$a");
            d.VersionFileFullFilePath.Should().EndWith("aa$$a");
            File.Exists(d.VersionFileFullFilePath).Should().BeTrue();
            d.RemoveLabel();
            d.Label.Should().BeNull();
            d.VersionFileFullFilePath.Should().NotEndWith("aa$$a");
            File.Exists(d.VersionFileFullFilePath).Should().BeTrue();
            d.AddLabel("bbb");
            d.Label.Should().Be("bbb");
            d.VersionFileFullFilePath.Should().EndWith("bbb");
            File.Exists(d.VersionFileFullFilePath).Should().BeTrue();
            d.RemoveLabel();
            d.Label.Should().BeNull();
            d.VersionFileFullFilePath.Should().NotEndWith("bbb");
            File.Exists(d.VersionFileFullFilePath).Should().BeTrue();
        }
    }
}
