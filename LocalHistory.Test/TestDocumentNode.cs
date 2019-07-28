using LOSTALLOY.LocalHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LocalHistory.Test
{
    [TestClass]
    public class TestDocumentNode
    {
        [TestMethod]
        public void Constructor()
        {
            var documentNode = new DocumentNode(null, null, null, DateTime.Now);
            
        }
    }
}
