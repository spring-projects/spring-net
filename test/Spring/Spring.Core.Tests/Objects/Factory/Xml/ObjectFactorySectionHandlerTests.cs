#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

#region Imports

using System;
using System.IO;
using System.Xml;
using NUnit.Framework;

#endregion

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Test the usage of the ObjectFactorySectionHandler.
    /// </summary>
	[TestFixture]
    public class ObjectFactorySectionHandlerTests
	{
        private XmlElement _xmlElement;

        [SetUp]
        public void CreateXmlElement()
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlData = "<objects xmlns=\"http://www.springframework.net\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd\"><object name=\"TestVersion\"  type=\"System.Version, Mscorlib\"></object></objects>";
            xmlDoc.Load(new StringReader(xmlData));
            _xmlElement = xmlDoc.DocumentElement;
        }

        /// <summary>
        /// Test calling the section handler with null values.
        /// </summary>
		[Test]
		public void CreateFactoryUnSuccessful()
		{
		    ObjectFactorySectionHandler objHandler = new ObjectFactorySectionHandler();
		    Assert.Throws<ArgumentNullException>(() => objHandler.Create(null, null, null));
		}

        /// <summary>
        /// Test calling the section handler with valid XML and make sure no
        /// exception occurs and the factory has the right number of
        /// objects.
        /// </summary>
        [Test]
        public void CreateFactorySuccessful()
        {
            ObjectFactorySectionHandler objHandler = new ObjectFactorySectionHandler();
            IListableObjectFactory factory = (IListableObjectFactory)objHandler.Create( null, null, _xmlElement );
            Assert.AreEqual(1, factory.ObjectDefinitionCount);

        }
	}
}
