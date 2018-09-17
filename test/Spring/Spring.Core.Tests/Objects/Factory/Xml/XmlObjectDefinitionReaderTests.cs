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

using System;
using System.Reflection;
using System.Xml;
using NUnit.Framework;
using Spring.Aop.Config;
using Spring.Core.IO;
using Spring.Objects.Factory.Config;
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
        public void LoadObjectDefinitionsWithNullResource()
        {
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(new DefaultListableObjectFactory());
            Assert.Throws<ObjectDefinitionStoreException>(() => reader.LoadObjectDefinitions((string) null));
        }

        [Test]
        public void LoadObjectDefinitionsWithNonExistentResource()
        {
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(new DefaultListableObjectFactory());
            Assert.Throws<ObjectDefinitionStoreException>(() => reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("/dev/null")));
        }

        [Test]
        public void AutoRegistersAllWellknownNamespaceParsers_Common()
        {
            string[] namespaces =
            {
                "http://www.springframework.net/tx",
                "http://www.springframework.net/aop",
                "http://www.springframework.net/db",
                "http://www.springframework.net/database",
#if !NETCOREAPP
                "http://www.springframework.net/remoting",
                "http://www.springframework.net/nms",
                "http://www.springframework.net/nvelocity",
#endif
                "http://www.springframework.net/validation"
            };

            foreach (string ns in namespaces)
            {
                Assert.IsNotNull(NamespaceParserRegistry.GetParser(ns),
                    string.Format("Parser for Namespace {0} could not be auto-registered.", ns));
            }
        }

#if !NETCOREAPP
        [Test]
        public void AutoRegistersAllWellknownNamespaceParsers_3_0()
        {
            string[] namespaces = { "http://www.springframework.net/wcf" };

            foreach (string ns in namespaces)
            {
                Assert.IsNotNull(NamespaceParserRegistry.GetParser(ns),
                    string.Format("Parser for Namespace {0} could not be auto-registered.", ns));
            }
        }
#endif

        [Test]
        [Ignore("this test cannot co-exist with AutoRegistersAllWellknownNamespaceParsers b/c that test will have already loaded the Spring.Data ass'y")]
        public void AutoRegistersWellknownNamespaceParser()
        {
            try
            {
                Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in loadedAssemblies)
                {
                    if (assembly.GetName(true).Name.StartsWith("Spring.Data"))
                    {
                        Assert.Fail("Spring.Data is already loaded - this test checks if it gets loaded during xml parsing");
                    }
                }

                NamespaceParserRegistry.Reset();

                DefaultListableObjectFactory of = new DefaultListableObjectFactory();
                XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(of);
                reader.LoadObjectDefinitions(new StringResource(
                                                 @"<?xml version='1.0' encoding='UTF-8' ?>
                                                    <objects xmlns='http://www.springframework.net' 
                                                             xmlns:tx='http://www.springframework.net/tx'>  
                                                          <tx:attribute-driven />
                                                    </objects>
                                                    "));
                object apc = of.GetObject(AopNamespaceUtils.AUTO_PROXY_CREATOR_OBJECT_NAME);
                Assert.NotNull(apc);
            }
            finally
            {
                NamespaceParserRegistry.Reset();
            }
        }

        [Test]
        public void ThrowsOnUnknownNamespaceUri()
        {
            NamespaceParserRegistry.Reset();

            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(of);
            Assert.Throws<ObjectDefinitionStoreException>(() => reader.LoadObjectDefinitions(new StringResource(
                                             @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' 
     xmlns:x='http://www.springframework.net/XXXX'>  
  <x:group id='tripValidator' />
</objects>
")));
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
            Assert.AreEqual(" \n\r\t", ((TestObject)of.GetObject("test")).Name);
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
		<property name='name'><value xml:space='default'> &#x000a;&#x000d;&#x0009;</value></property>
	</object>
</objects>
"));
            Assert.AreEqual(string.Empty, ((TestObject)of.GetObject("test2")).Name);
            Assert.AreEqual(string.Empty, ((TestObject)of.GetObject("test3")).Name);
            Assert.AreEqual(string.Empty, ((TestObject)of.GetObject("test4")).Name);
        }


        [Test]
        public void WhitespaceValuesArePreservedForValueElementWhenSpaceIsSetToPreserve()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(of);
            reader.LoadObjectDefinitions(new StringResource(
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>  
	<object id='test4' type='Spring.Objects.TestObject, Spring.Core.Tests'>
		<property name='name'><value xml:space='preserve'> &#x000a;&#x000d;&#x0009;</value></property>
	</object>
</objects>
"));
            Assert.AreEqual(" \n\r\t", ((TestObject)of.GetObject("test4")).Name);
        }

        [Test]
        public void ThrowsObjectDefinitionStoreExceptionOnValidationError()
        {
            try
            {
                DefaultListableObjectFactory of = new DefaultListableObjectFactory();
                XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(of);
                reader.LoadObjectDefinitions(new StringResource(
                                                @"<?xml version='1.0' encoding='UTF-8' ?>
                                                <objects xmlns='http://www.springframework.net'>  
	                                                <INVALIDELEMENT id='test2' type='Spring.Objects.TestObject, Spring.Core.Tests' />
                                                </objects>
                                                "));
                Assert.Fail();
            }
            catch (ObjectDefinitionStoreException ex)
            {
                Assert.IsTrue(ex.Message.IndexOf("Line 3 in XML document from  violates the schema.") > -1);
            }
        }

        [Test]
        public void ThrowsObjectDefinitionStoreExceptionOnInvalidXml()
        {
            try
            {
                DefaultListableObjectFactory of = new DefaultListableObjectFactory();
                XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(of);
                reader.LoadObjectDefinitions(new StringResource(
                                                 @"<?xml version='1.0' encoding='UTF-8' ?>
                                                <objects xmlns='http://www.springframework.net'>  
	                                                <object id='test2' type='Spring.Objects.TestObject, Spring.Core.Tests'>
                                                </objects>
                                                "));
                Assert.Fail();
            }
            catch (ObjectDefinitionStoreException ex)
            {
                Assert.IsTrue(ex.Message.IndexOf("Line 4 in XML document from  is not well formed.") > -1);
            }
        }

        #region ThrowsObjectDefinitionStoreExceptionOnErrorDuringObjectDefinitionRegistration Helper

        private class TestXmlObjectDefinitionReader : XmlObjectDefinitionReader
        {
            public TestXmlObjectDefinitionReader(IObjectDefinitionRegistry registry)
                : base(registry)
            { }

            private class ThrowingObjectDefinitionDocumentReader : IObjectDefinitionDocumentReader
            {
                public void RegisterObjectDefinitions(XmlDocument doc, XmlReaderContext readerContext)
                {
                    throw new TestException("RegisterObjectDefinitions");
                }
            }

            protected override IObjectDefinitionDocumentReader CreateObjectDefinitionDocumentReader()
            {
                return new ThrowingObjectDefinitionDocumentReader();
            }

        }

        #endregion

        [Test]
        public void ThrowsObjectDefinitionStoreExceptionOnErrorDuringObjectDefinitionRegistration()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new TestXmlObjectDefinitionReader(of);
            Assert.Throws<ObjectDefinitionStoreException>(() => reader.LoadObjectDefinitions(new StringResource(
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>  
	<object id='test2' type='Spring.Objects.TestObject, Spring.Core.Tests' />
</objects>
")));
        }

        [Test]
        public void ParsesNonDefaultNamespace()
        {
            try
            {
                NamespaceParserRegistry.Reset();

                DefaultListableObjectFactory of = new DefaultListableObjectFactory();
                XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(of);
                reader.LoadObjectDefinitions(new StringResource(
@"<?xml version='1.0' encoding='UTF-8' ?>
<core:objects xmlns:core='http://www.springframework.net'>  
	<core:object id='test2' type='Spring.Objects.TestObject, Spring.Core.Tests'>
        <core:property name='Sibling'>
            <core:object type='Spring.Objects.TestObject, Spring.Core.Tests' />
        </core:property>
    </core:object>
</core:objects>
"));
                TestObject test2 = (TestObject)of.GetObject("test2");
                Assert.AreEqual(typeof(TestObject), test2.GetType());
                Assert.IsNotNull(test2.Sibling);
            }
            finally
            {
                NamespaceParserRegistry.Reset();
            }
        }

        [Test]
        public void ParsesObjectAttributes()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(of);
            reader.LoadObjectDefinitions(new StringResource(
@"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>  
	<object id='test1' type='Spring.Objects.TestObject, Spring.Core.Tests' singleton='false' abstract='true' />
	<object id='test2' type='Spring.Objects.TestObject, Spring.Core.Tests' singleton='true' abstract='false' lazy-init='true' 
        autowire='no' dependency-check='simple'
        depends-on='test1' 
        init-method='init'
        destroy-method='destroy'
    />
</objects>
"));
            AbstractObjectDefinition od1 = (AbstractObjectDefinition)of.GetObjectDefinition("test1");
            Assert.IsFalse(od1.IsSingleton);
            Assert.IsTrue(od1.IsAbstract);
            Assert.IsFalse(od1.IsLazyInit);

            AbstractObjectDefinition od2 = (AbstractObjectDefinition)of.GetObjectDefinition("test2");
            Assert.IsTrue(od2.IsSingleton);
            Assert.IsFalse(od2.IsAbstract);
            Assert.IsTrue(od2.IsLazyInit);
            Assert.AreEqual(AutoWiringMode.No, od2.AutowireMode);
            Assert.AreEqual("init", od2.InitMethodName);
            Assert.AreEqual("destroy", od2.DestroyMethodName);
            Assert.AreEqual(1, od2.DependsOn.Count);
            Assert.AreEqual("test1", od2.DependsOn[0]);
            Assert.AreEqual(DependencyCheckingMode.Simple, od2.DependencyCheck);
        }

        [Test]
        public void ParsesAutowireCandidate()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(of);
            reader.LoadObjectDefinitions(new StringResource(
@"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' default-autowire-candidates='test1*,test4*'>  
	<object id='test1' type='Spring.Objects.TestObject, Spring.Core.Tests' />
	<object id='test2' type='Spring.Objects.TestObject, Spring.Core.Tests' autowire-candidate='false' />
	<object id='test3' type='Spring.Objects.TestObject, Spring.Core.Tests' autowire-candidate='true' />
	<object id='test4' type='Spring.Objects.TestObject, Spring.Core.Tests' autowire-candidate='default' />
	<object id='test5' type='Spring.Objects.TestObject, Spring.Core.Tests' autowire-candidate='default' />
</objects>
"));
            var od = (AbstractObjectDefinition)of.GetObjectDefinition("test1");
            Assert.That(od.IsAutowireCandidate, Is.True, "No attribute set should default to true");

            od = (AbstractObjectDefinition)of.GetObjectDefinition("test2");
            Assert.That(od.IsAutowireCandidate, Is.False, "Specifically attribute set to false should set to false");

            od = (AbstractObjectDefinition)of.GetObjectDefinition("test3");
            Assert.That(od.IsAutowireCandidate, Is.True, "Specifically attribute set to true should set to false");

            od = (AbstractObjectDefinition)of.GetObjectDefinition("test4");
            Assert.That(od.IsAutowireCandidate, Is.True, "Attribute set to default should check pattern and return true");

            od = (AbstractObjectDefinition)of.GetObjectDefinition("test5");
            Assert.That(od.IsAutowireCandidate, Is.False, "Attribute set to default should check pattern and return false");
        }

    }
}