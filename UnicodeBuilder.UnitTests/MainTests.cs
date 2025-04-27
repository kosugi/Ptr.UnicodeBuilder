using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnicodeBuilder.UnitTests
{
    [TestClass]
    public class MainTests
    {
        private Main main;

        [TestInitialize]
        public void TestInitialize()
        {
            main = new Main();
            main.SetupConnection();
        }

        [TestMethod]
        public void Query_should_return_results()
        {
            var result = main.Query(new(""));
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Query_0021()
        {
            var result = main.Query(new("21"));
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("! (U+0021)", result[0].Title);
            Assert.AreEqual("EXCLAMATION MARK", result[0].SubTitle);
        }

        [TestMethod]
        public void Query_3042()
        {
            var result = main.Query(new("3042"));
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("あ (U+3042)", result[0].Title);
            Assert.AreEqual("HIRAGANA LETTER A", result[0].SubTitle);
        }

        [TestMethod]
        public void Query_1F9E1()
        {
            var result = main.Query(new("1f9E1"));
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("🧡 (U+1F9E1)", result[0].Title);
            Assert.AreEqual("ORANGE HEART", result[0].SubTitle);
        }
    }
}
