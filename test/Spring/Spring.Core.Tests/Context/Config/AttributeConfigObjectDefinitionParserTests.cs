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

using System.Linq;
using NUnit.Framework;
using Spring.Context.Attributes;
using Spring.Context.Support;
using Spring.Objects.Factory.Xml;

namespace Spring.Context.Config
{
    [TestFixture]
    public class AttributeConfigObjectDefinitionParserTests
    {
        private XmlApplicationContext _applicationContext;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void RegisteredComponents()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.AttributeConfigParser.xml", GetType()));
            var objectDefintionNames = _applicationContext.ObjectFactory.GetObjectDefinitionNames();

            Assert.That(objectDefintionNames.Contains(AttributeConfigUtils.CONFIGURATION_ATTRIBUTE_PROCESSOR_OBJECT_NAME), Is.True);
            Assert.That(objectDefintionNames.Contains(AttributeConfigUtils.AUTOWIRED_ATTRIBUTE_PROCESSOR_OBJECT_NAME), Is.True);
            Assert.That(objectDefintionNames.Contains(AttributeConfigUtils.REQUIRED_ATTRIBUTE_PROCESSOR_OBJECT_NAME), Is.True);
            Assert.That(objectDefintionNames.Contains(AttributeConfigUtils.INITDESTROY_ATTRIBUTE_PROCESSOR_OBJECT_NAME), Is.True);
        }
    }
}
