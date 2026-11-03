using System.Xml;
using NUnit.Framework;
using Spring.Util;

namespace Spring.Core.IO;

/// <summary>
/// Summary description for ConfigSectionResourceTests.
/// </summary>
[TestFixture]
public class ConfigSectionResourceTests
{
    private class TestXmlUrlResolver : XmlUrlResolver
    {
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            object result = base.GetEntity(absoluteUri, role, ofObjectToReturn);
            return result;
        }

        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            Console.WriteLine("baseUri=" + baseUri);
            Console.WriteLine("relativeUri=" + relativeUri);
            Uri result = base.ResolveUri(baseUri, relativeUri);
            return result;
        }
    }

    private ConfigSectionResource CreateConfigSectionResource(string filename)
    {
        ConfigXmlDocument xmlDoc = new ConfigXmlDocument();

        Uri testUri = TestResourceLoader.GetUri(this, filename);

        xmlDoc.Load("test config section", testUri.AbsoluteUri);
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("od", "http://www.springframework.net");
        XmlElement configElement = (XmlElement) xmlDoc.SelectSingleNode("//configuration/spring/od:objects", nsmgr);

        ConfigSectionResource csr = new ConfigSectionResource(configElement);
        return csr;
    }

#if NETFRAMEWORK
    [Test]
    public void CanCreate()
    {
        ConfigSectionResource csr = CreateConfigSectionResource("_config1.xml");
        Assert.That(csr.Exists, Is.False);
        Assert.That(csr.File, Is.Null); // always null
        Assert.That(csr.Uri, Is.Null);
        Assert.That(csr.Description.StartsWith("config ["), Is.True);
        Assert.That(csr.Description.EndsWith("#objects]"), Is.True);
    }
#endif

    [Test]
    public void ThrowsOnNullSectionName()
    {
        Assert.Throws<ArgumentNullException>(() => new ConfigSectionResource((string) null));
    }

    [Test]
    public void ThrowsOnNullConfigElement()
    {
        Assert.Throws<ArgumentNullException>(() => new ConfigSectionResource((XmlElement) null));
    }

    [Test]
    public void ThrowsIoExceptionIfConfigSectionDoesNotExist()
    {
        IResource res = new ConfigSectionResource(Guid.NewGuid().ToString());

        try
        {
            Stream istm = res.InputStream;
            Assert.Fail("Did not receive expected IOException!");
        }
        catch (IOException)
        {
        }
    }
}
