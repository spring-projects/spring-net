using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Spring.Objects.Factory.Xml;
using Spring.Objects.Factory.Support;

namespace Spring.Context.Attributes
{
    [TestFixture]
    public class ImportResourceAttributeTests
    {
        [Test]
        public void Uses_XmlObjectDefinitionReader_By_Default()
        {
            var attrib = new ImportResourceAttribute("the resource");

            Assert.That(attrib.DefinitionReader, Is.EqualTo(typeof(XmlObjectDefinitionReader)));
        }

        [Test]
        public void Can_Assign_NonDefault_DefinitionReader()
        {
            var attrib = new ImportResourceAttribute("the resource");
            attrib.DefinitionReader = typeof(AbstractObjectDefinitionReader);

            Assert.That(attrib.DefinitionReader, Is.EqualTo(typeof(AbstractObjectDefinitionReader)));
        }

        [Test]
        public void DefinitionReader_Can_Prevent_Improper_Types()
        {
            ImportResourceAttribute attrib = new ImportResourceAttribute("the resource");

            try
            {
                attrib.DefinitionReader = typeof(Object);// <--need to use *anything* ensured *not* to implement IObjectDefinitionReader
                Assert.Fail("Expected Exception of type ArgumentException not thrown!");
            }
            catch (ArgumentException)
            {
                //swallow the expected exception
            }
            
           
        }
    }
}
