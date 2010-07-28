using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Spring.Context.Support
{
    [TestFixture]
    public class XmlApplicationContentArgsTests
    {
        [Test]
        public void Default_CaseSensitivity_isTrue()
        {
            XmlApplicationContextArgs args = new XmlApplicationContextArgs();
            Assert.True(args.CaseSensitive);
        }

        [Test]
        public void Default_AutoRefresh_isTrue()
        {
            XmlApplicationContextArgs args = new XmlApplicationContextArgs();
            Assert.True(args.Refresh);
        }
    }
}
