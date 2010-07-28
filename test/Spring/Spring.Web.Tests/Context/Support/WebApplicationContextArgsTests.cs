using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Spring.Context.Support
{
    [TestFixture]
    public class WebApplicationContextArgsTests
    {
        [Test]
        public void Default_CaseSensitivity_isFalse()
        {
            WebApplicationContextArgs args = new WebApplicationContextArgs(string.Empty, null, null, null);
            Assert.False(args.CaseSensitive);
        }
    }
}
