using LOSTALLOY.LocalHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LocalHistory.Test
{
    [TestClass]
    public class TestDocumentRepository
    {
        [TestMethod]
        public void TestConstructor()
        {
            var repository = new DocumentRepository(null, null);
        }
    }
}
