#region License

/*
 * Copyright © 2010-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
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
                attrib.DefinitionReader = typeof(Object);// <--need to pass *anything* ensured *not* to implement IObjectDefinitionReader
                Assert.Fail("Expected Exception of type ArgumentException not thrown!");
            }
            catch (ArgumentException)
            {
                //swallow the expected exception
            }
           
        }
    }
}
