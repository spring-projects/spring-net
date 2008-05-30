using System;
using NUnit.Framework;

namespace Spring.Services.WindowsService.Common
{
    [TestFixture]
	public class Localizer_ForProcessTest
	{
        [Test]
        public void ByDefaultIgnoreUnresolvablePlaceholders ()
        {
            Assert.IsTrue(new Localizer.ForProcess().IgnoreUnresolvablePlaceholders);
        }

        [Test]
		public void YouCanDefineACustomPrefix()
		{
            string prefix = Guid.NewGuid().ToString();

            Localizer.ForProcess localizer = new Localizer.ForProcess();

            Assert.AreEqual(Localizer.DefaultPrefix, localizer.Prefix);
            
            localizer.Prefix = prefix;
            foreach (string property in localizer.Properties)
            {
                Assert.IsTrue(property.StartsWith(prefix));
            }
		}
	}

    [TestFixture]
	public class Localizer_ForApplicationTest
	{
        [Test]
        public void ByDefaultIgnoreUnresolvablePlaceholders ()
        {
            Assert.IsTrue(new Localizer.ForApplication().IgnoreUnresolvablePlaceholders);
        }

        [Test]
		public void YouCanDefineACustomPrefix()
		{
            string prefix = "FOO";

            Localizer.ForApplication localizer = new Localizer.ForApplication();

            Assert.AreEqual(Localizer.DefaultPrefix, localizer.Prefix);

            localizer.Prefix = prefix;
            foreach (string property in localizer.Properties)
            {
                Assert.IsTrue(property.StartsWith(prefix));
            }
        }
	}
}
