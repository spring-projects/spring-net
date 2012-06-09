using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Spring.ConversationWA
{
    [TestFixture]
    public class SimpleTest
    {
        [Test]
        public void Test()
        {
            Assert.AreEqual(2, 1 + 1, "2 == 1 + 1");
        }
    }
}
