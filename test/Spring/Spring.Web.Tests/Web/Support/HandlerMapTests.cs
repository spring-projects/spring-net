using NUnit.Framework;

namespace Spring.Web.Support
{
    [TestFixture]
    public class HandlerMapTests
    {
        [Test]
        public void CanMatch_EmbeddedLiteralsPattern()
        {
            HandlerMap map = new HandlerMap();
            map.Add("/*/test2/*", "theHandlerIWant");

            HandlerMapEntry handler = map.MapPath("/test1/test2/default.aspx");

            Assert.NotNull(handler);
            Assert.AreEqual("theHandlerIWant", handler.HandlerObjectName);
        }

        [Test]
        public void CanMatch_ExactStringPatterns()
        {
            HandlerMap map = new HandlerMap();
            map.Add("/test1/test2/default.aspx", "theHandlerIWant");

            HandlerMapEntry handler = map.MapPath("/test1/test2/default.aspx");

            Assert.NotNull(handler);
            Assert.AreEqual("theHandlerIWant", handler.HandlerObjectName);
        }

        [Test]
        public void CanMatch_LeadingLiteralsPattern()
        {
            HandlerMap map = new HandlerMap();
            map.Add("/test1/*", "theHandlerIWant");

            HandlerMapEntry handler = map.MapPath("/test1/test2/default.aspx");

            Assert.NotNull(handler);
            Assert.AreEqual("theHandlerIWant", handler.HandlerObjectName);
        }

    }
}
