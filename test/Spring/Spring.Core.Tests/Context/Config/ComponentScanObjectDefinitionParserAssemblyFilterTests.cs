using NUnit.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory.Xml;

namespace Spring.Context.Config
{
    [TestFixture]
    public class ComponentScanObjectDefinitionParserAssemblyFilterTests
    {
        private IApplicationContext _applicationContext;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void BaseAssembliesAttributeRequired()
        {
            Assert.That(() => { _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.BaseAssemblyTestWithout.xml", GetType())); },
                        Throws.Exception);
        }

        [Test]
        public void SingleAssemblyNameProvided()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.BaseAssemblyTestSingle.xml", GetType()));

            Assert.That(_applicationContext.GetObjectDefinitionNames().Count, Is.GreaterThan(0));
        }

        [Test]
        public void MultipleAssemblyNameProvided()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.BaseAssemblyTestMultiple.xml", GetType()));

            Assert.That(_applicationContext.GetObjectDefinitionNames().Count, Is.GreaterThan(0));
        }
        [Test]
        public void NegativeAssemblyNameProvided()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.BaseAssemblyTestNegative.xml", GetType()));

            Assert.That(_applicationContext.GetObjectDefinitionNames().Count, Is.EqualTo(4));
        }

    }
}
