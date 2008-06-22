#region License

/*
 * Copyright 2004 the original author or authors.
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

using NUnit.Framework;

using Spring.Objects.Factory.Support;

#endregion

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Unit tests for the XmlObjectDefinitionReader class.
    /// </summary>
    /// <author>Rick Evans (.NET)</author>
    [TestFixture]
    public class XmlObjectDefinitionReaderTests
    {
        [Test]
        public void Instantiation()
        {
            XmlObjectDefinitionReader reader
                = new XmlObjectDefinitionReader(
                    new DefaultListableObjectFactory());
        }

        [Test]
        [ExpectedException(typeof(ObjectDefinitionStoreException))]
        public void LoadObjectDefinitionsWithNullResource()
        {
            XmlObjectDefinitionReader reader
                = new XmlObjectDefinitionReader(
                    new DefaultListableObjectFactory());
            reader.LoadObjectDefinitions((string)null);
        }

        [Test]
        [ExpectedException(typeof(ObjectDefinitionStoreException))]
        public void LoadObjectDefinitionsWithNonExistentResource()
        {
            XmlObjectDefinitionReader reader
                = new XmlObjectDefinitionReader(
                    new DefaultListableObjectFactory());
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("/dev/null"));
        }

        [Test]
        public void WhitespaceValuesArePreservedForValueAttribute()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(of);
            reader.LoadObjectDefinitions(new StringResource(
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>  
	<object id='test' type='Spring.Objects.TestObject, Spring.Core.Tests'>
		<property name='name' value=' &#x000a;&#x000d;&#x0009;' />
	</object>
</objects>
"));
            Assert.AreEqual(" \n\r\t", ((TestObject) of.GetObject("test")).Name);
        }

        [Test]
        public void WhitespaceValuesResultInEmptyStringForValueElement()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(of);
            reader.LoadObjectDefinitions(new StringResource(
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>  
	<object id='test2' type='Spring.Objects.TestObject, Spring.Core.Tests'>
		<property name='name'><value /></property>
	</object>
	<object id='test3' type='Spring.Objects.TestObject, Spring.Core.Tests'>
		<property name='name'><value></value></property>
	</object>
	<object id='test4' type='Spring.Objects.TestObject, Spring.Core.Tests'>
		<property name='name'><value> &#x000a;&#x000d;&#x0009;</value></property>
	</object>
</objects>
"));
            Assert.AreEqual(string.Empty, ((TestObject) of.GetObject("test2")).Name);
            Assert.AreEqual(string.Empty, ((TestObject) of.GetObject("test3")).Name);
            Assert.AreEqual(string.Empty, ((TestObject) of.GetObject("test4")).Name);
        }
    }
}