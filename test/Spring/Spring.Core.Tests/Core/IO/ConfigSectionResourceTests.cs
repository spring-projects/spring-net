using System;
using System.Xml;
using NUnit.Framework;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Core.IO
{
    /// <summary>
    /// Summary description for ConfigSectionResourceTests.
    /// </summary>
    [TestFixture]
    public class ConfigSectionResourceTests
    {
        private ConfigSectionResource CreateConfigSectionResource(string filename)
        {
            ConfigXmlDocument xmlDoc = new ConfigXmlDocument();
            Uri testUri = TestResourceLoader.GetUri(this, filename);
            xmlDoc.Load( testUri.AbsoluteUri );
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("od", "http://www.springframework.net");
            XmlElement configElement = (XmlElement)xmlDoc.SelectSingleNode("//configuration/spring/od:objects", nsmgr);
            
            ConfigSectionResource csr = new ConfigSectionResource( configElement);
            return csr;
        }

        [Test]
        public void CanCreate()
        {
            ConfigSectionResource csr = CreateConfigSectionResource("_config1.xml");
            Assert.IsFalse(csr.Exists);
            Assert.IsNull(csr.File); // always null
            Assert.IsNull(csr.Uri);
            Assert.IsTrue(csr.Description.StartsWith("config [") );
            Assert.IsTrue(csr.Description.EndsWith("#objects]") );
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsOnNullSectionName()
        {
            new ConfigSectionResource((string)null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsOnNullConfigElement()
        {
            new ConfigSectionResource((XmlElement)null);
        }
    }
}