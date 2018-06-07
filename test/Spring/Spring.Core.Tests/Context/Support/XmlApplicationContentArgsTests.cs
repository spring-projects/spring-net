using NUnit.Framework;

namespace Spring.Context.Support
{
    [TestFixture]
    public class XmlApplicationContentArgsTests
    {
        [Test]
        public void Default_CaseSensitivity_isTrue()
        {
            XmlApplicationContextArgs args = new XmlApplicationContextArgs(string.Empty,null,null,null);
            Assert.True(args.CaseSensitive);
        }

        [Test]
        public void Default_AutoRefresh_isTrue()
        {
            XmlApplicationContextArgs args = new XmlApplicationContextArgs(string.Empty, null, null, null);
            Assert.True(args.Refresh);
        }
    }
}
