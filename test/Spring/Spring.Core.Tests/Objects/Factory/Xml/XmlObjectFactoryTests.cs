/*
 * Copyright � 2002-2011 the original author or authors.
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
#if !NETCOREAPP
using System.Web.Services;
#endif

using Common.Logging;
using Common.Logging.Simple;

using FakeItEasy;

using NUnit.Framework;

using Spring.Core.IO;
using Spring.Core.TypeResolution;
using Spring.Expressions;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Util;

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Unit tests for the XmlObjectFactory class.
    /// </summary>
    /// <remarks>
    /// <p>
    /// There are actually a lot of integration tests in this class too.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    [TestFixture]
    public sealed class XmlObjectFactoryTests
    {
        /// <summary>
        /// The setup logic executed before the execution of this test fixture.
        /// </summary>
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            // enable (null appender) logging, to ensure that the logging code is exercised...
            //XmlConfigurator.Configure();
            LogManager.Adapter = new NoOpLoggerFactoryAdapter();
        }

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void ReplacedMethodWithNoReplacerObjectNameSpecified()
        {
            Assert.Throws<ObjectDefinitionStoreException>(() => new StreamHelperDecorator(new StreamHelperCallback(_ReplacedMethodWithNoReplacerObjectNameSpecified)).Run());
        }

        private void _ReplacedMethodWithNoReplacerObjectNameSpecified(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='foo' type='Spring.Objects.TestObject, Spring.Core.Tests'>
		<!-- the 'name' attribute is required to be a non-null string -->
		<replaced-method name='bing' replacer=' '/>
	</object>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
        }

        [Test]
        public void ReplacedMethodWithNoMethodNameSpecified()
        {
            Assert.Throws<ObjectDefinitionStoreException>(() => new StreamHelperDecorator(new StreamHelperCallback(_ReplacedMethodWithNoMethodNameSpecified)).Run());
        }

        private void _ReplacedMethodWithNoMethodNameSpecified(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='foo' type='Spring.Objects.TestObject, Spring.Core.Tests'>
		<!-- the 'name' attribute is required to be a non-null string -->
		<replaced-method name=' ' replacer='target'/>
	</object>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
        }

        [Test]
        public void ValidatesReplacedMethodCorrectly()
        {
            Assert.Throws<ObjectDefinitionStoreException>(() => new StreamHelperDecorator(new StreamHelperCallback(_ValidatesReplacedMethodCorrectly)).Run());
        }

        private void _ValidatesReplacedMethodCorrectly(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='foo' type='Spring.Objects.TestObject, Spring.Core.Tests'>
		<!-- there is no such method 'hello' on the TestObject class -->
		<replaced-method name='hello' replacer='target'/>
	</object>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
        }

        [Test]
        public void LookupMethodIsParsedAndOperatesCorrectly_SunnyDay()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_LookupMethodIsParsedAndOperatesCorrectly_SunnyDay)).Run();
        }

        private void _LookupMethodIsParsedAndOperatesCorrectly_SunnyDay(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='factory' type='Spring.Objects.Factory.Support.TestObjectFactory, Spring.Core.Tests'>
		<lookup-method name='GetObject' object='target'/>
	</object>
	<object id='target' type='Spring.Objects.TestObject, Spring.Core.Tests' singleton='false'>
		<property name='name' value='Fiona Apple'/>
		<property name='age' value='47'/>
	</object>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            IObjectFactory factory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
            TestObjectFactory tof = (TestObjectFactory) factory["factory"];
            object to = tof.GetObject();
            Assert.IsNotNull(to);
            Assert.AreEqual(typeof(TestObject), to.GetType());
            TestObject target = (TestObject) to;
            Assert.AreEqual("Fiona Apple", target.Name);
            Assert.AreEqual(47, target.Age);
            // pull the prototype out again...
            TestObject target2 = (TestObject) tof.GetObject();
            Assert.IsFalse(ReferenceEquals(target, target2));
        }

        [Test]
        public void ValidatesLookupMethodCorrectly()
        {
            Assert.Throws<ObjectDefinitionStoreException>(() => new StreamHelperDecorator(new StreamHelperCallback(_ValidatesLookupMethodCorrectly)).Run());
        }

        private void _ValidatesLookupMethodCorrectly(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='foo' type='Spring.Objects.TestObject, Spring.Core.Tests'>
		<!-- there is no such method 'hello' on the TestObject class -->
		<lookup-method name='hello' object='target'/>
	</object>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
        }

        [Test]
        public void LookupMethodWithNoMethodNameSpecified()
        {
            Assert.Throws<ObjectDefinitionStoreException>(() => new StreamHelperDecorator(new StreamHelperCallback(_LookupMethodWithNoMethodNameSpecified)).Run());
        }

        private void _LookupMethodWithNoMethodNameSpecified(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='foo' type='Spring.Objects.TestObject, Spring.Core.Tests'>
		<!-- the 'name' attribute is required to be a non-null string -->
		<lookup-method name=' ' object='target'/>
	</object>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            IObjectFactory factory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
            factory.GetObject("foo");
        }

        [Test]
        public void LookupMethodWithNoTargetObjectNameSpecified()
        {
            Assert.Throws<ObjectDefinitionStoreException>(() => new StreamHelperDecorator(new StreamHelperCallback(_LookupMethodWithNoTargetObjectNameSpecified)).Run());
        }

        private void _LookupMethodWithNoTargetObjectNameSpecified(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='foo' type='Spring.Objects.TestObject, Spring.Core.Tests'>
		<!-- the 'name' attribute is required to be a non-null string -->
		<lookup-method name='bing' object=' '/>
	</object>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            IObjectFactory factory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
            factory.GetObject("foo");
        }

        [Test]
        public void NonChildObjectDefinitionWithoutATypeReturnsObjectDefinition()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_NonChildObjectDefinitionWithoutATypeReturnsObjectDefinition)).Run();
        }

        private void _NonChildObjectDefinitionWithoutATypeReturnsObjectDefinition(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='Applicationimpl'/>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            IObjectFactory factory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
            object obj = factory.GetObject("Applicationimpl");
            Assert.IsTrue(obj is IObjectDefinition);
        }

        [Test]
        public void RegisterAliasViaAliasElement()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_RegisterAliasViaAliasElement)).Run();
        }

        private void _RegisterAliasViaAliasElement(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
    <alias name='foo' alias='fooAlias'/>
	<object id='foo' type='Spring.Objects.TestObject,Spring.Core.Tests'/>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            IObjectFactory factory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
            // pull object out of the factory using the alias name...
            object foo = factory.GetObject("fooAlias");
            Assert.IsNotNull(foo, "Alias was obviously not registered otherwise we would not have got a null object back.");
        }

        [Test]
        public void RegisterAliasViaAliasElementOrderingIsUnimportant()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_RegisterAliasViaAliasElementOrderingIsUnimportant)).Run();
        }

        private void _RegisterAliasViaAliasElementOrderingIsUnimportant(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='foo' type='Spring.Objects.TestObject,Spring.Core.Tests'/>
    <alias name='foo' alias='fooAlias'/>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            IObjectFactory factory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
            // pull object out of the factory using the alias name...
            object foo = factory.GetObject("fooAlias");
            Assert.IsNotNull(foo, "Alias was obviously not registered otherwise we would not have got a null object back.");
        }

        [Test]
        public void IfTypeAttributeIsPresentItMustNotBeTheEmptyStringValue()
        {
            Assert.Throws<ObjectDefinitionStoreException>(() => new StreamHelperDecorator(new StreamHelperCallback(_IfTypeAttributeIsPresentItMustNotBeTheEmptyStringValue)).Run());
        }

        private void _IfTypeAttributeIsPresentItMustNotBeTheEmptyStringValue(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='foo' type=''/>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            IObjectFactory factory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
            factory.GetObject("Applicationimpl");
        }

        [Test]
        public void IfTypeAttributeIsPresentItMustNotBeAnOnlyWhitespaceStringValue()
        {
            Assert.Throws<ObjectDefinitionStoreException>(() => new StreamHelperDecorator(new StreamHelperCallback(_IfTypeAttributeIsPresentItMustNotBeAnOnlyWhitespaceStringValue)).Run());
        }

        private void _IfTypeAttributeIsPresentItMustNotBeAnOnlyWhitespaceStringValue(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='foo' type=' 
'/>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            IObjectFactory factory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
            factory.GetObject("Applicationimpl");
        }

        [Test]
        public void SimpleStringObject()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_SimpleStringObject)).Run();
        }

        private void _SimpleStringObject(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='myString' type='System.String'>
		<constructor-arg index='0' value='foo'/>
	</object>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            IObjectFactory factory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
            string myString = (string) factory["myString"];
            Assert.AreEqual("foo", myString);
        }

        [Test]
        public void MethodInvokingFactoryObjectRefsObject()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_MethodInvokingFactoryObjectRefsObject)).Run();
        }

        private void _MethodInvokingFactoryObjectRefsObject(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='myString' type='System.String'>
		<constructor-arg type='char[]' value='Bingo'/>
	</object>
    <object id='foo' type='Spring.Objects.Factory.Config.MethodInvokingFactoryObject'>
        <property name='TargetType' value='Spring.Objects.TestObject'/>
        <property name='TargetMethod' value='Create'/>
        <property name='Arguments'>
            <list>
                <ref local='myString'/>
            </list>
        </property>
    </object>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            IObjectFactory factory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
            TestObject foo = (TestObject) factory["foo"];
            Assert.AreEqual("Bingo", foo.Name);
        }

        [Test]
        public void BadParentReference()
        {
            IResource resource = new ReadOnlyXmlTestResource("wellformed-but-bad.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            Assert.Throws<ObjectCreationException>(() => xof.GetObject("no.parent.factory"));
        }

        [Test]
        public void TypedStringValueIsPickedUp()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_TypedStringValueIsPickedUp)).Run();
        }

        private void _TypedStringValueIsPickedUp(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='golyadkin' type='Spring.Objects.TestObject, Spring.Core.Tests'>
		<property name='Hats'><value type='string[]'>trilby,fedora</value></property>
	</object>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            XmlObjectFactory fac = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
            IObjectDefinition def = fac.GetObjectDefinition("golyadkin");
            PropertyValue value = def.PropertyValues.GetPropertyValue("Hats");
            Assert.AreEqual(typeof(TypedStringValue), value.Value.GetType());
            TestObject obj = (TestObject) fac.GetObject("golyadkin");
            Assert.IsTrue(ArrayUtils.AreEqual(new string[] {"trilby", "fedora"}, obj.Hats));
        }

        [Test]
        public void ChildDefinitionWithoutIdOrNameOrALiasGetsOneAutogenerated()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_ChildDefinitionWithoutIdOrNameOrALiasGetsOneAutogenerated)).Run();
        }

        private void _ChildDefinitionWithoutIdOrNameOrALiasGetsOneAutogenerated(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='mother' abstract='true' type='Spring.Objects.TestObject, Spring.Core.Tests'/>
	<object parent='mother'/>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            XmlObjectFactory factory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
            IList<string> names = factory.GetObjectDefinitionNames();
            // mmm, how is one to test this? I have no idea what the generated name is...
            Assert.AreEqual(2, names.Count, "Should have got two object names, one of which is autogenerated.");
        }

        /// <summary>
        /// Tests for the issue described at SPRNET-83. Schema modded to allow
        /// zero <object/> elements (where previously there HAD to be at least one).
        /// </summary>
        [Test]
        public void AnObjectsFileWithNoObjectsIsOk()
        {
            // test just makes sure that no exception is thrown and that
            // said container doesn't complain about having no objects.
            IResource resource = new ReadOnlyXmlTestResource("no-objects.xml", GetType());
            new XmlObjectFactory(resource);
        }

        [Test]
        public void ImportsExternalResourcesCorrectly()
        {
            IResource resource = new ReadOnlyXmlTestResource("external-resources.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            // object comes from imported file...
            ITestObject rick = xof["rick"] as ITestObject;
            Assert.IsNotNull(rick);
            Assert.AreEqual("Rick", rick.Name);
            // object comes from base file...
            ITestObject jenny = xof["jenny"] as ITestObject;
            Assert.IsNotNull(jenny);
            Assert.AreEqual("Jenny", jenny.Name);
        }

        [Test]
        public void ImportsExternalResourcesBailsOnNonExistentResource()
        {
            IResource resource = new ReadOnlyXmlTestResource("bad-external-resources.xml", GetType());
            Assert.Throws<ObjectDefinitionStoreException>(() => new XmlObjectFactory(resource));
        }

        [Test]
        public void PropertyInvokingFactoryObjectIsWiredCorrectly()
        {
            IResource resource = new ReadOnlyXmlTestResource("invoke-factory.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            TestObject actual = xof["culturalObject"] as TestObject;
            object expected = CultureInfo.InstalledUICulture;
            Assert.AreEqual(expected, actual.MyCulture);
        }

        [Test]
        public void LoadFromConfig()
        {
            IObjectFactory factory = ConfigurationUtils.GetSection("objects") as IObjectFactory;

            Assert.IsNotNull(factory, "Factory loaded from config was null.");
            Assert.IsTrue(factory is IConfigurableListableObjectFactory);
            IConfigurableListableObjectFactory clof
                = factory as IConfigurableListableObjectFactory;
            Assert.IsTrue(clof.ObjectDefinitionCount == 6);
            Assert.IsNotNull(factory["foo"]);
        }

        [Test]
        public void DescriptionButNoProperties()
        {
            DefaultListableObjectFactory xof = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(xof);
            reader.LoadObjectDefinitions(
                new ReadOnlyXmlTestResource("collections.xml", GetType()));
            TestObject validEmpty
                = (TestObject) xof.GetObject("validEmptyWithDescription");
            Assert.AreEqual(0, validEmpty.Age);
        }

        /// <summary>
        /// Uses a separate factory.
        /// </summary>
        [Test]
        public void RefToSeparatePrototypeInstances()
        {
            DefaultListableObjectFactory xof = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(xof);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("reftypes.xml", GetType()));
            Assert.IsTrue(xof.ObjectDefinitionCount == 9, "9 objects in reftypes, not " + xof.ObjectDefinitionCount);
            TestObject emma = (TestObject) xof.GetObject("emma");
            TestObject georgia = (TestObject) xof.GetObject("georgia");
            ITestObject emmasJenks = emma.Spouse;
            ITestObject georgiasJenks = georgia.Spouse;
            Assert.IsTrue(emmasJenks != georgiasJenks, "Emma and georgia think they have a different boyfriend.");
            Assert.IsTrue(emmasJenks.Name.Equals("Andrew"), "Emmas jenks has right name");
            Assert.IsTrue(emmasJenks != xof.GetObject("jenks"), "Emmas doesn't equal new ref.");
            Assert.IsTrue(emmasJenks.Name.Equals("Andrew"), "Georgias jenks has right name.");
            Assert.IsTrue(emmasJenks.Equals(georgiasJenks), "They are object equal.");
            Assert.IsTrue(emmasJenks.Equals(xof.GetObject("jenks")), "They object equal direct ref.");
        }

        [Test]
        public void RefToSingleton()
        {
            DefaultListableObjectFactory xof = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(xof);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("reftypes.xml", GetType()));
            Assert.IsTrue(xof.ObjectDefinitionCount == 9, "9 objects in reftypes, not " + xof.ObjectDefinitionCount);
            TestObject jen = (TestObject) xof.GetObject("jenny");
            TestObject dave = (TestObject) xof.GetObject("david");
            TestObject jenks = (TestObject) xof.GetObject("jenks");
            ITestObject davesJen = dave.Spouse;
            ITestObject jenksJen = jenks.Spouse;
            Assert.IsTrue(davesJen == jenksJen, "1 jen instance");
            Assert.IsTrue(davesJen == jen, "1 jen instance");
        }


        [Test]
        public void InnerObjects()
        {
            DefaultListableObjectFactory xof = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(xof);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("reftypes.xml", GetType()));

            // Let's create the outer bean named "innerObject",
            // to check whether it doesn't create any conflicts
            // with the actual inner object named "innerObject".
            xof.GetObject("innerObject");

            TestObject hasInnerObjects = (TestObject) xof.GetObject("hasInnerObjects");
            Assert.AreEqual(5, hasInnerObjects.Age);
            TestObject inner1 = (TestObject) hasInnerObjects.Spouse;
            Assert.IsNotNull(inner1);
            Assert.AreEqual("Spring.Objects.TestObject#", inner1.ObjectName.Substring(0, inner1.ObjectName.IndexOf("#")+1));
            Assert.AreEqual("inner1", inner1.Name);
            Assert.AreEqual(6, inner1.Age);


            Assert.IsNotNull(hasInnerObjects.Friends);
            IList friends = (IList) hasInnerObjects.Friends;
            Assert.AreEqual(2, friends.Count);
            DerivedTestObject inner2 = (DerivedTestObject) friends[0];
            Assert.AreEqual("inner2", inner2.Name);
            Assert.AreEqual(7, inner2.Age);
            Assert.AreEqual("Spring.Objects.DerivedTestObject#", inner2.ObjectName.Substring(0, inner2.ObjectName.IndexOf("#") + 1));
            TestObject innerFactory = (TestObject) friends[1];
            Assert.AreEqual(DummyFactory.SINGLETON_NAME, innerFactory.Name);


            Assert.IsNotNull(hasInnerObjects.SomeMap);
            Assert.IsFalse((hasInnerObjects.SomeMap.Count == 0));
            TestObject inner3 = (TestObject) hasInnerObjects.SomeMap["someKey"];
            Assert.AreEqual("Jenny", inner3.Name);
            Assert.AreEqual(30, inner3.Age);
            xof.Dispose();
            Assert.IsTrue(inner2.WasDestroyed());
            Assert.IsTrue(innerFactory.Name == null);
        }

        [Test]
        public void InnerObjectsInPrototype()
        {
            DefaultListableObjectFactory xof = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(xof);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("reftypes.xml", GetType()));
            TestObject hasInnerObjects = (TestObject) xof.GetObject("prototypeHasInnerObjects");
            Assert.AreEqual(5, hasInnerObjects.Age);

            Assert.IsNotNull(hasInnerObjects.Spouse);
            Assert.AreEqual("inner1", hasInnerObjects.Spouse.Name);
            Assert.AreEqual(6, hasInnerObjects.Spouse.Age);
            Assert.IsNotNull(hasInnerObjects.Friends);
            IList friends = (IList) hasInnerObjects.Friends;
            Assert.AreEqual(2, friends.Count);
            DerivedTestObject inner2 = (DerivedTestObject) friends[0];
            Assert.AreEqual("inner2", inner2.Name);
            Assert.AreEqual(7, inner2.Age);


            IList friendsOfInner = (IList) inner2.Friends;
            Assert.AreEqual(1, friendsOfInner.Count);
            DerivedTestObject innerFriendOfAFriend = (DerivedTestObject) friendsOfInner[0];
            Assert.AreEqual("innerFriendOfAFriend", innerFriendOfAFriend.Name);
            Assert.AreEqual(7, innerFriendOfAFriend.Age);
            TestObject innerFactory = (TestObject) friends[1];
            Assert.AreEqual(DummyFactory.SINGLETON_NAME, innerFactory.Name);
            Assert.IsNotNull(hasInnerObjects.SomeMap);
            Assert.IsFalse((hasInnerObjects.SomeMap.Count == 0));

            TestObject inner3 = (TestObject) hasInnerObjects.SomeMap["someKey"];
            Assert.AreEqual("inner3", inner3.Name);
            Assert.AreEqual(8, inner3.Age);
            xof.Dispose();

            Assert.IsFalse(inner2.WasDestroyed());
            Assert.IsFalse(innerFactory.Name == null);
            Assert.IsFalse(innerFriendOfAFriend.WasDestroyed());

        }

        [Test]
        public void SingletonInheritanceFromParentFactorySingleton()
        {
            XmlObjectFactory parent = new XmlObjectFactory(new ReadOnlyXmlTestResource("parent.xml", GetType()));
            XmlObjectFactory child = new XmlObjectFactory(new ReadOnlyXmlTestResource("child.xml", GetType()), parent);
            TestObject inherits = (TestObject) child.GetObject("inheritsFromParentFactory");
            // Name property value is overriden
            Assert.IsTrue(inherits.Name.Equals("override"));
            // Age property is inherited from object in parent factory
            Assert.IsTrue(inherits.Age == 1);
            TestObject inherits2 = (TestObject) child.GetObject("inheritsFromParentFactory");
            Assert.AreSame(inherits2,inherits);
        }

        [Test]
        public void SingletonInheritanceFromParentFactorySingletonUsingCtor()
        {
            XmlObjectFactory parent = new XmlObjectFactory(new ReadOnlyXmlTestResource("parent.xml", GetType()));
            XmlObjectFactory child = new XmlObjectFactory(new ReadOnlyXmlTestResource("child.xml", GetType()), parent);
            TestObject inherits = (TestObject)child.GetObject("inheritsFromParentFactoryUsingCtor");
            // Name property value is overriden
            Assert.IsTrue(inherits.Name.Equals("child-name"));
            // Age property is inherited from object in parent factory
            Assert.IsTrue(inherits.Age == 1);
            TestObject inherits2 = (TestObject)child.GetObject("inheritsFromParentFactoryUsingCtor");
            Assert.AreSame(inherits2,inherits);
        }

        [Test]
        public void PrototypeInheritanceFromParentFactoryPrototype()
        {
            XmlObjectFactory parent = new XmlObjectFactory(new ReadOnlyXmlTestResource("parent.xml", GetType()));
            XmlObjectFactory child = new XmlObjectFactory(new ReadOnlyXmlTestResource("child.xml", GetType()), parent);
            TestObject inherits = (TestObject) child.GetObject("prototypeInheritsFromParentFactoryPrototype");
            // Name property value is overridden
            Assert.IsTrue(inherits.Name.Equals("prototype-override"));
            // Age property is inherited from object in parent factory
            Assert.IsTrue(inherits.Age == 2);
            TestObject inherits2 = (TestObject) child.GetObject("prototypeInheritsFromParentFactoryPrototype");
            Assert.AreNotSame(inherits2,inherits);
            inherits2.Age = 13;
            Assert.IsTrue(inherits2.Age == 13);
            // Shouldn't have changed first instance
            Assert.IsTrue(inherits.Age == 2);
        }

        [Test]
        public void PrototypeInheritanceFromParentFactorySingleton()
        {
            XmlObjectFactory parent = new XmlObjectFactory(new ReadOnlyXmlTestResource("parent.xml", GetType()));
            XmlObjectFactory child = new XmlObjectFactory(new ReadOnlyXmlTestResource("child.xml", GetType()), parent);
            TestObject inherits = (TestObject) child.GetObject("protoypeInheritsFromParentFactorySingleton");
            // Name property value is overridden
            Assert.IsTrue(inherits.Name.Equals("prototypeOverridesInheritedSingleton"));
            // Age property is inherited from object in parent factory
            Assert.IsTrue(inherits.Age == 1);
            TestObject inherits2 = (TestObject) child.GetObject("protoypeInheritsFromParentFactorySingleton");
            Assert.AreNotSame(inherits2,inherits);
            inherits2.Age = 13;
            Assert.IsTrue(inherits2.Age == 13);
            // Shouldn't have changed first instance
            Assert.IsTrue(inherits.Age == 1);
        }

        [Test]
        public void DependenciesMaterializeThis()
        {
            IResource resource = new ReadOnlyXmlTestResource("dependenciesMaterializeThis.xml", GetType());
            XmlObjectFactory bf = new XmlObjectFactory(resource);
            DummyBo bos = (DummyBo) bf.GetObject("boSingleton");
            DummyBo bop = (DummyBo) bf.GetObject("boPrototype");
            Assert.IsFalse(bos == bop);
            Assert.AreEqual(bos.dao, bop.dao);
        }

        [Test]
        public void ChildOverridesParentObject()
        {
            XmlObjectFactory parent = new XmlObjectFactory(new ReadOnlyXmlTestResource("parent.xml", GetType()));
            XmlObjectFactory child = new XmlObjectFactory(new ReadOnlyXmlTestResource("child.xml", GetType()), parent);
            TestObject inherits = (TestObject) child.GetObject("inheritedTestObject");
            // Name property value is overridden
            Assert.IsTrue(inherits.Name.Equals("overrideParentObject"));
            // Age property is inherited from object in parent factory
            Assert.IsTrue(inherits.Age == 1);
            TestObject inherits2 = (TestObject) child.GetObject("inheritedTestObject");
            Assert.IsTrue(inherits2 == inherits);
        }

        /// <summary>
        /// Check that a prototype can't inherit from a bogus parent.
        /// If a singleton does this the factory will fail to load.
        /// </summary>
        [Test]
        public void BogusParentageFromParentFactory()
        {
            XmlObjectFactory parent = new XmlObjectFactory(new ReadOnlyXmlTestResource("parent.xml", GetType()));
            XmlObjectFactory child = new XmlObjectFactory(new ReadOnlyXmlTestResource("child.xml", GetType()), parent);
            Assert.Throws<NoSuchObjectDefinitionException>(() => child.GetObject("bogusParent"));
        }

        /// <summary>
        /// Note that prototype/singleton distinction is <b>not</b> inherited.
        /// It's possible for a subclass singleton not to return independent
        /// instances even if derived from a prototype
        /// </summary>
        [Test]
        public void SingletonInheritsFromParentFactoryPrototype()
        {
            XmlObjectFactory parent = new XmlObjectFactory(new ReadOnlyXmlTestResource("parent.xml", GetType()));
            XmlObjectFactory child = new XmlObjectFactory(new ReadOnlyXmlTestResource("child.xml", GetType()), parent);
            TestObject inherits = (TestObject) child.GetObject("singletonInheritsFromParentFactoryPrototype");
            // Name property value is overriden
            Assert.IsTrue(inherits.Name.Equals("prototype-override"));
            // Age property is inherited from object in parent factory
            Assert.IsTrue(inherits.Age == 2);
            TestObject inherits2 = (TestObject) child.GetObject("singletonInheritsFromParentFactoryPrototype");
            Assert.IsTrue(inherits2 == inherits);
        }

        [Test]
        public void AbstractParentObjects()
        {
            XmlObjectFactory parent = new XmlObjectFactory(new ReadOnlyXmlTestResource("parent.xml", GetType()));
            parent.PreInstantiateSingletons();
            Assert.IsTrue(parent.IsSingleton("inheritedTestObjectWithoutClass"));

            // abstract objects should not match
            //TODO add overloaded GetObjectOfType with 1 arg
            IDictionary<string, object> tbs = parent.GetObjectsOfType(typeof(TestObject), true, true);
            Assert.AreEqual(2, tbs.Count);
            Assert.IsTrue(tbs.ContainsKey("inheritedTestObjectPrototype"));
            Assert.IsTrue(tbs.ContainsKey("inheritedTestObjectSingleton"));

            // non-abstract object should work, even if it serves as parent
            object o1 = parent.GetObject("inheritedTestObjectPrototype");
            Assert.IsTrue(o1 is TestObject);

            // abstract object should return IObjectInstance itself
            object o2 = parent.GetObject("inheritedTestObjectWithoutClass");
            Assert.IsTrue(o2 is IObjectDefinition);
        }

        [Test]
        public void CircularReferences()
        {
            DefaultListableObjectFactory xof = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(xof);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("reftypes.xml", GetType()));
            TestObject jenny = (TestObject) xof.GetObject("jenny");
            TestObject david = (TestObject) xof.GetObject("david");
            TestObject ego = (TestObject) xof.GetObject("ego");
            Assert.IsTrue(jenny.Spouse == david, "Correct circular reference");
            Assert.IsTrue(david.Spouse == jenny, "Correct circular reference");
            Assert.IsTrue(ego.Spouse == ego, "Correct circular reference");
        }

        [Test]
        public void FactoryReferenceCircle()
        {
            IResource resource = new ReadOnlyXmlTestResource("factoryCircle.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            TestObject tb = (TestObject) xof.GetObject("singletonFactory");
            DummyFactory db = (DummyFactory) xof.GetObject("&singletonFactory");
            Assert.IsTrue(tb == db.OtherTestObject);
        }

        [Test]
        public void RefSubelement()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            //Assert.IsTrue ("5 objects in reftypes, not " + xof.GetObjectDefinitionCount(), xof.GetObjectDefinitionCount() == 5);
            TestObject jen = (TestObject) xof.GetObject("jenny");
            TestObject dave = (TestObject) xof.GetObject("david");
            Assert.IsTrue(jen.Spouse == dave);
        }

        [Test]
        public void PropertyWithLiteralValueSubelement()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            TestObject verbose = (TestObject) xof.GetObject("verbose");
            Assert.IsTrue(verbose.Name.Equals("verbose"));
        }

        [Test]
        public void PropertyWithIdRefLocalAttrSubelement()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            TestObject verbose = (TestObject) xof.GetObject("verbose2");
            Assert.IsTrue(verbose.Name.Equals("verbose"));
        }

        [Test]
        public void PropertyWithIdRefObjectAttrSubelement()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            TestObject verbose = (TestObject) xof.GetObject("verbose3");
            Assert.IsTrue(verbose.Name.Equals("verbose"));
        }

        [Test]
        public void EnumProperty()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("enums.xml", GetType()));
            TestObject obj = factory.GetObject("rod", typeof(TestObject)) as TestObject;
            Assert.IsNotNull(obj.FileMode);
            Assert.AreEqual(FileMode.Create, obj.FileMode);
        }

        [Test]
        public void InitMethodIsInvoked()
        {
            IResource resource = new ReadOnlyXmlTestResource("initializers.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            DoubleInitializer in_Renamed = (DoubleInitializer) xof.GetObject("init-method1");
            // Initializer should have doubled value
            Assert.AreEqual(14, in_Renamed.Num);
        }

        [Test]
        public void DefaultInitMethodIsInvoked()
        {
            IResource resource = new ReadOnlyXmlTestResource("default-initializers.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            DoubleInitializer in_Renamed = (DoubleInitializer)xof.GetObject("init-method1");
            // Initializer should have doubled value
            Assert.AreEqual(14, in_Renamed.Num);
        }

        [Test]
        public void DefaultInitMethodDisabled()
        {
            IResource resource = new ReadOnlyXmlTestResource("default-initializers.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            DoubleInitializer in_Renamed = (DoubleInitializer)xof.GetObject("init-method2");
            // Initializer should have doubled value
            Assert.AreEqual(7, in_Renamed.Num);
        }

        /// <summary>
        /// Test that if a custom initializer throws an exception, it's handled correctly.
        /// </summary>
        [Test]
        public void InitMethodThrowsException()
        {
            IResource resource = new ReadOnlyXmlTestResource("initializers.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            try
            {
                xof.GetObject("init-method2");
                Assert.Fail();
            }
            catch (ObjectCreationException ex)
            {
                Assert.IsTrue(ex.InnerException is FormatException);
            }
        }

        [Test]
        public void NoSuchInitMethod()
        {
            IResource resource = new ReadOnlyXmlTestResource("initializers.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            Assert.Throws<ObjectCreationException>(() => xof.GetObject("init-method3"));
        }

        /// <summary>
        /// Check that InitializingObject method is called first.
        /// </summary>
        [Test]
        public void InitializingObjectAndInitMethod()
        {
            InitAndIB.constructed = false;
            IResource resource = new ReadOnlyXmlTestResource("initializers.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            Assert.IsFalse(InitAndIB.constructed);
            xof.PreInstantiateSingletons();
            Assert.IsFalse(InitAndIB.constructed);
            InitAndIB iib = (InitAndIB) xof.GetObject("init-and-ib");
            Assert.IsTrue(InitAndIB.constructed);
            Assert.IsTrue(iib.afterPropertiesSetInvoked && iib.initMethodInvoked);
            Assert.IsTrue(!iib.destroyed && !iib.customDestroyed);
            xof.Dispose();
            Assert.IsTrue(iib.destroyed && iib.customDestroyed);
            xof.Dispose();
            Assert.IsTrue(iib.destroyed && iib.customDestroyed);
        }

        [Test]
        public void DefaultDestroyMethodInvoked()
        {
            IResource resource = new ReadOnlyXmlTestResource("default-destroy-methods.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            xof.PreInstantiateSingletons();
            DefaultDestroyer dd = (DefaultDestroyer)xof.GetObject("destroy-method1");
            Assert.IsTrue(!dd.customDestroyed);
            xof.Dispose();
            Assert.IsTrue(dd.customDestroyed);
        }

        [Test]
        public void DefaultDestroyMethodDisabled()
        {
            IResource resource = new ReadOnlyXmlTestResource("default-destroy-methods.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            xof.PreInstantiateSingletons();
            DefaultDestroyer dd = (DefaultDestroyer)xof.GetObject("destroy-method2");
            Assert.IsTrue(!dd.customDestroyed);
            xof.Dispose();
            Assert.IsTrue(!dd.customDestroyed);
        }

        [Test]
        public void MultiThreadedLazyInit()
        {
            IResource resource = new ReadOnlyXmlTestResource("lazy-init-multithreaded.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);

            LazyWorker lw1 = new LazyWorker(xof);
            LazyWorker lw2 = new LazyWorker(xof);
            Thread thread1 = new Thread(lw1.DoWork);
            Thread thread2 = new Thread(lw2.DoWork);

            thread1.Start();
            Thread.Sleep(1000);
            thread2.Start();
            thread1.Join();
            thread2.Join();
            Assert.AreEqual(typeof(LazyTestObject), lw1.ObjectFromContext.GetType());
            Assert.AreEqual(typeof(LazyTestObject), lw2.ObjectFromContext.GetType());
            Assert.AreEqual(1, LazyTestObject.Count);
        }

        /// <summary>
        /// Check that InitializingObject method is called first.
        /// </summary>
        [Test]
        public void DefaultLazyInit()
        {
            InitAndIB.constructed = false;
            IResource resource = new ReadOnlyXmlTestResource("default-lazy-init.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            Assert.IsFalse(InitAndIB.constructed);
            xof.PreInstantiateSingletons();
            Assert.IsTrue(InitAndIB.constructed);
            try
            {
                xof.GetObject("lazy-and-bad");
            }
            catch (ObjectCreationException ex)
            {
                Assert.IsTrue(ex.InnerException is FormatException);
            }
        }


        /// <summary>
        /// Check that InitializingObject method is called first.
        /// </summary>
        [Test]
        public void DefaultLazyInitNoInObjectDef()
        {
            InitAndIB.constructed = false;
            IResource resource = new ReadOnlyXmlTestResource("default-lazy-init.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            Assert.IsFalse(InitAndIB.constructed);
            xof.PreInstantiateSingletons();
            Assert.IsTrue(InitAndIB.constructed);
            try
            {
                xof.GetObject("init-and-ib-no-init-in-local-object-def");
            }
            catch (ObjectCreationException ex)
            {
                Assert.IsTrue(ex.InnerException is FormatException);
            }
        }

        [Test]
        public void NoSuchXmlFile()
        {
            Assert.Throws<ObjectDefinitionStoreException>(() => new XmlObjectFactory(new ReadOnlyXmlTestResource("missing.xml", GetType())));
        }

        [Test]
        public void InvalidXmlFile()
        {
            DefaultListableObjectFactory xof = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(xof);
            try
            {
                reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("invalid.xml", GetType()));
                Assert.Fail("Should have thrown XmlObjectDefinitionStoreException");
            }
            catch (ObjectDefinitionStoreException e)
            {
                Assert.AreEqual(0, e.Message.IndexOf("Line 21 in XML document"));
            }
        }

        [Test]
        public void DefaultXmlResolverIsUsedIfNullSuppliedOrSet()
        {
            DefaultListableObjectFactory xof = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(xof, null);
            try
            {
                reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("invalid.xml", GetType()));
                Assert.Fail("Should have thrown XmlObjectDefinitionStoreException");
            }
            catch (ObjectDefinitionStoreException e)
            {
                Assert.AreEqual(0, e.Message.IndexOf("Line 21 in XML document"));
            }
        }

        [Test]
        public void UnsatisfiedObjectDependencyCheck()
        {
            XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("unsatisfiedObjectDependencyCheck.xml", GetType()));
            Assert.Throws<UnsatisfiedDependencyException>(() => xof.GetObject("a", typeof(DependenciesObject)));
        }

        [Test]
        public void UnsatisfiedSimpleDependencyCheck()
        {
            XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("unsatisfiedSimpleDependencyCheck.xml", GetType()));
            Assert.Throws<UnsatisfiedDependencyException>(() => xof.GetObject("a", typeof(DependenciesObject)));
        }

        [Test]
        public void SatisfiedObjectDependencyCheck()
        {
            XmlObjectFactory xof
                = new XmlObjectFactory(
                    new ReadOnlyXmlTestResource("satisfiedObjectDependencyCheck.xml", GetType()));
            DependenciesObject a = (DependenciesObject) xof.GetObject("a");
            Assert.IsNotNull(a.Spouse);
        }

        [Test]
        public void SatisfiedSimpleDependencyCheck()
        {
            XmlObjectFactory xof =
                new XmlObjectFactory(
                    new ReadOnlyXmlTestResource(
                        "satisfiedSimpleDependencyCheck.xml", GetType()));
            DependenciesObject a = (DependenciesObject) xof.GetObject("a");
            Assert.AreEqual(a.Age, 33);
        }

        [Test]
        public void UnsatisfiedAllDependencyCheck()
        {
            XmlObjectFactory xof= new XmlObjectFactory(new ReadOnlyXmlTestResource("unsatisfiedAllDependencyCheckMissingObjects.xml", GetType()));
            Assert.Throws<UnsatisfiedDependencyException>(() => xof.GetObject("a", typeof(DependenciesObject)));
        }

        [Test]
        public void SatisfiedAllDependencyCheck()
        {
            XmlObjectFactory xof
                = new XmlObjectFactory(
                    new ReadOnlyXmlTestResource("satisfiedAllDependencyCheck.xml", GetType()));
            DependenciesObject a = (DependenciesObject) xof.GetObject("a");
            Assert.AreEqual(a.Age, 33);
            Assert.IsNotNull(a.Name);
            Assert.IsNotNull(a.Spouse);
        }

        [Test]
        public void Autowire()
        {
            XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("autowire.xml", GetType()));
            TestObject spouse = new TestObject("kerry", 0);
            xof.RegisterSingleton("Spouse", spouse);
            DoTestAutowire(xof);
        }

        [Test]
        public void AutowireWithCtorArrayArgs()
        {
            XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("array-autowire.xml", GetType()));
            TestObject spouse = new TestObject("kerry", 0);
            xof.RegisterSingleton("spouse", spouse);

            TestObject spouse2 = new TestObject("kerry2", 0);
            xof.RegisterSingleton("spouse2", spouse2);

            ITestObject kerry = (ITestObject) xof.GetObject("spouse");
            ITestObject kerry2 = (ITestObject)xof.GetObject("spouse2");
            ArrayCtorDependencyObject rod7 = (ArrayCtorDependencyObject) xof.GetObject("rod7");

            Assert.AreEqual(kerry, rod7.Spouse1);
            Assert.AreEqual(kerry2, rod7.Spouse2);
        }

        [Test]
        public void AutowireWithParent()
        {
            XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("autowire.xml", GetType()));
            DefaultListableObjectFactory lbf = new DefaultListableObjectFactory();
            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("Name", "kerry");
            lbf.RegisterObjectDefinition("Spouse", new RootObjectDefinition(typeof(TestObject), pvs));
            xof.ParentObjectFactory = lbf;
            DoTestAutowire(xof);
        }

        private void DoTestAutowire(XmlObjectFactory xof)
        {
            DependenciesObject rod1 = (DependenciesObject) xof.GetObject("rod1");
            TestObject kerry = (TestObject) xof.GetObject("Spouse");
            // Should have been autowired
            Assert.AreEqual(kerry, rod1.Spouse);

            DependenciesObject rod1a = (DependenciesObject) xof.GetObject("rod1a");
            // Should have been autowired
            Assert.AreEqual(kerry, rod1a.Spouse);

            DependenciesObject rod2 = (DependenciesObject) xof.GetObject("rod2");
            // Should have been autowired
            Assert.AreEqual(kerry, rod2.Spouse);

            ConstructorDependenciesObject rod3 = (ConstructorDependenciesObject) xof.GetObject("rod3");
            IndexedTestObject other = (IndexedTestObject) xof.GetObject("other");
            // Should have been autowired
            Assert.AreEqual(kerry, rod3.Spouse1);
            Assert.AreEqual(kerry, rod3.Spouse2);
            Assert.AreEqual(other, rod3.Other);

            ConstructorDependenciesObject rod3a = (ConstructorDependenciesObject) xof.GetObject("rod3a");
            // Should have been autowired
            Assert.AreEqual(kerry, rod3a.Spouse1);
            Assert.AreEqual(kerry, rod3a.Spouse2);
            Assert.AreEqual(other, rod3a.Other);

            try
            {
                xof.GetObject("rod4", typeof(ConstructorDependenciesObject));
                Assert.Fail("Should not have thrown FatalObjectException");
            }
            catch (FatalObjectException)
            {
                // expected
            }

            DependenciesObject rod5 = (DependenciesObject) xof.GetObject("rod5");
            // Should not have been autowired
            Assert.IsNull(rod5.Spouse);

            /* TODO include basc in
            IObjectFactory appCtx = (IObjectFactory) xof.GetObject("childAppCtx");
            Assert.IsTrue(appCtx.GetObject("rod1") != null);
            Assert.IsTrue(appCtx.GetObject("dependingObject") != null);
            Assert.IsTrue(appCtx.GetObject("jenny") != null);
             */
        }

        [Test]
        public void AutowireWithDefault()
        {
            XmlObjectFactory xof
                = new XmlObjectFactory(
                    new ReadOnlyXmlTestResource("default-autowire.xml", GetType()));
            DependenciesObject rod1 = (DependenciesObject) xof.GetObject("rod1");
            // Should have been autowired
            Assert.IsNotNull(rod1.Spouse);
            Assert.IsTrue(rod1.Spouse.Name.Equals("Kerry"));

            DependenciesObject rod2 = (DependenciesObject) xof.GetObject("rod2");
            // Should have been autowired
            Assert.IsNotNull(rod2.Spouse);
            Assert.IsTrue(rod2.Spouse.Name.Equals("Kerry"));
        }

        [Test]
        public void AutowireByConstructor()
        {
            XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
            ConstructorDependenciesObject rod1 = (ConstructorDependenciesObject) xof.GetObject("rod1");
            TestObject kerry = (TestObject) xof.GetObject("kerry2");
            // Should have been autowired
            Assert.AreEqual(kerry, rod1.Spouse1);
            Assert.AreEqual(0, rod1.Age);
            Assert.AreEqual(null, rod1.Name);

            ConstructorDependenciesObject rod2 = (ConstructorDependenciesObject) xof.GetObject("rod2");
            TestObject kerry1 = (TestObject) xof.GetObject("kerry1");
            TestObject kerry2 = (TestObject) xof.GetObject("kerry2");
            // Should have been autowired
            Assert.AreEqual(kerry2, rod2.Spouse1);
            Assert.AreEqual(kerry1, rod2.Spouse2);
            Assert.AreEqual(0, rod2.Age);
            Assert.AreEqual(null, rod2.Name);

            ConstructorDependenciesObject rod = (ConstructorDependenciesObject) xof.GetObject("rod3");
            IndexedTestObject other = (IndexedTestObject) xof.GetObject("other");
            // Should have been autowired
            Assert.AreEqual(kerry, rod.Spouse1);
            Assert.AreEqual(kerry, rod.Spouse2);
            Assert.AreEqual(other, rod.Other);
            Assert.AreEqual(0, rod.Age);
            Assert.AreEqual(null, rod.Name);
            xof.GetObject("rod4", typeof(ConstructorDependenciesObject));
            // Should have been autowired
            Assert.AreEqual(kerry, rod.Spouse1);
            Assert.AreEqual(kerry, rod.Spouse2);
            Assert.AreEqual(other, rod.Other);
            Assert.AreEqual(0, rod.Age);
            Assert.AreEqual(null, rod.Name);
        }

        [Test]
        public void AutowireByConstructorWithSimpleValues()
        {
            XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
            ConstructorDependenciesObject rod5 = (ConstructorDependenciesObject) xof.GetObject("rod5");
            TestObject kerry1 = (TestObject) xof.GetObject("kerry1");
            TestObject kerry2 = (TestObject) xof.GetObject("kerry2");
            IndexedTestObject other = (IndexedTestObject) xof.GetObject("other");
            // Should have been autowired
            Assert.AreEqual(kerry2, rod5.Spouse1);
            Assert.AreEqual(kerry1, rod5.Spouse2);
            Assert.AreEqual(other, rod5.Other);
            Assert.AreEqual(99, rod5.Age);
            Assert.AreEqual("myname", rod5.Name);
            ConstructorDependenciesObject rod6 = (ConstructorDependenciesObject) xof.GetObject("rod6");
            // Should have been autowired
            Assert.AreEqual(kerry2, rod6.Spouse1);
            Assert.AreEqual(kerry1, rod6.Spouse2);
            Assert.AreEqual(other, rod6.Other);
            Assert.AreEqual(0, rod6.Age);
            Assert.AreEqual(null, rod6.Name);
        }

        [Test]
        public void ConstructorArgResolution()
        {
            XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
            TestObject kerry1 = (TestObject) xof.GetObject("kerry1");
            TestObject kerry2 = (TestObject) xof.GetObject("kerry2");
            ConstructorDependenciesObject rod9 = (ConstructorDependenciesObject) xof.GetObject("rod9");
            Assert.AreEqual(99, rod9.Age);
            ConstructorDependenciesObject rod10 = (ConstructorDependenciesObject) xof.GetObject("rod10");
            Assert.AreEqual(null, rod10.Name);
            ConstructorDependenciesObject rod11 = (ConstructorDependenciesObject) xof.GetObject("rod11");
            Assert.AreEqual(kerry2, rod11.Spouse1);

            ConstructorDependenciesObject rod12 = (ConstructorDependenciesObject) xof.GetObject("rod12");
            Assert.AreEqual(kerry1, rod12.Spouse1);
            Assert.IsNull(rod12.Spouse2);

            ConstructorDependenciesObject rod13 = (ConstructorDependenciesObject) xof.GetObject("rod13");
            Assert.AreEqual(kerry1, rod13.Spouse1);
            Assert.AreEqual(kerry2, rod13.Spouse2);
        }

        [Test]
        public void ThrowsExceptionOnTooManyArguments()
        {
            XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
            Assert.Throws<ObjectCreationException>(() => xof.GetObject("rod7", typeof(ConstructorDependenciesObject)));
        }

        [Test]
        public void ThrowsExceptionOnAmbiguousResolution()
        {
            XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
            Assert.Throws<UnsatisfiedDependencyException>(() => xof.GetObject("rod8", typeof(ConstructorDependenciesObject)));
        }

        [Test]
        public void FactoryObjectDefinedAsPrototype()
        {
            Assert.Throws<ObjectDefinitionStoreException>(() => new XmlObjectFactory(new ReadOnlyXmlTestResource("invalid-factory.xml", GetType())));
        }

        [Test]
        public void DependsOn()
        {
            PreparingObject1.prepared = false;
            PreparingObject1.destroyed = false;
            PreparingObject2.prepared = false;
            PreparingObject2.destroyed = false;
            DependingObject.destroyed = false;
            IResource resource = new ReadOnlyXmlTestResource("initializers.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            xof.PreInstantiateSingletons();
            xof.Dispose();
            Assert.IsTrue(PreparingObject1.prepared);
            Assert.IsTrue(PreparingObject1.destroyed);
            Assert.IsTrue(PreparingObject2.prepared);
            Assert.IsTrue(PreparingObject2.destroyed);
            Assert.IsTrue(DependingObject.destroyed);
        }

        [Test]
        public void ClassNotFoundWithDefault()
        {
            try
            {
                new XmlObjectFactory(
                    new ReadOnlyXmlTestResource("classNotFound.xml", GetType()));
            }
            catch (ObjectDefinitionStoreException ex)
            {
                Assert.IsTrue(ex.InnerException is TypeLoadException);
            }
        }

#if !NETCOREAPP
        [Test]
        public void AnObjectCanBeIstantiatedWithANotFullySpecifiedAssemblyName()
        {
            IResource resource = new ReadOnlyXmlTestResource("notfullyspecified.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            IDbConnection connection = (IDbConnection) xof.GetObject("connectionNotFullySpecified");
            Assert.IsNotNull(connection);
        }
#endif

        [Test]
        public void ResourceAndInputStream()
        {
            //string filename = @"C:\temp\spring-test.properties";
            string filename = @"/temp/spring-test.properties";
            FileInfo file = new FileInfo(filename);
            bool tempDirWasCreated = false;
            try
            {
                if (file.Exists)
                {
                    file.Delete();
                }
                if (!file.Directory.Exists)
                {
                    file.Directory.Create();
                    tempDirWasCreated = true;
                }
                StreamWriter sw = file.CreateText();
                sw.WriteLine("test");
                sw.Close();

                IResource resource = new ReadOnlyXmlTestResource("resource.xml", GetType());
                XmlObjectFactory xof = new XmlObjectFactory(resource);

                ResourceTestObject resource1 = (ResourceTestObject) xof.GetObject("resource1");
                Assert.IsTrue(resource1.Resource is FileSystemResource);
                using (StreamReader reader = new StreamReader(resource1.Resource.InputStream))
                {
                    string fileText = reader.ReadLine();
                    Assert.AreEqual("test", fileText, "error using IResource to read file contents");
                }
                using (StreamReader reader = new StreamReader(resource1.InputStream))
                {
                    string fileText = reader.ReadLine();
                    Assert.AreEqual("test", fileText, "error using Stream to read file contents");
                }
            }
            finally
            {
                if (tempDirWasCreated)
                {
                    file.Delete();
                    // sure hope another process hasn't created the directory
                    file.Directory.Delete();
                }
            }
        }

        [Test]
        public void FactoryMethodsSingletonOnTargetClass()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("factory-methods.xml", GetType()));

            FactoryMethods fm = (FactoryMethods) factory["default"];
            Assert.AreEqual(0, fm.Number);
            Assert.AreEqual("default", fm.Name);
            Assert.AreEqual("defaultInstance", fm.Object.Name);
            Assert.AreEqual("setterString", fm.Value);

            fm = (FactoryMethods) factory["testObjectOnly"];
            Assert.AreEqual(0, fm.Number);
            Assert.AreEqual("default", fm.Name);
            // This comes from the test object
            Assert.AreEqual("Juergen", fm.Object.Name);

            fm = (FactoryMethods) factory["full"];
            Assert.AreEqual(27, fm.Number);
            Assert.AreEqual("gotcha", fm.Name);
            Assert.AreEqual("Juergen", fm.Object.Name);

            FactoryMethods fm2 = (FactoryMethods) factory["full"];
            Assert.AreSame(fm, fm2);
        }

        [Test]
        public void FactoryMethodsPrototypeOnTargetClass()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("factory-methods.xml", GetType()));
            FactoryMethods fm = (FactoryMethods) factory["defaultPrototype"];
            FactoryMethods fm2 = (FactoryMethods) factory["defaultPrototype"];
            Assert.AreEqual(0, fm.Number);
            Assert.AreEqual("default", fm.Name);
            Assert.AreEqual("defaultInstance", fm.Object.Name);
            Assert.AreEqual("setterString", fm.Value);
            Assert.AreEqual(fm.Number, fm2.Number);
            Assert.AreEqual(fm.Value, fm2.Value);
            // The TestObject is created separately for each object
            Assert.IsFalse(ReferenceEquals(fm.Object, fm2.Object));
            Assert.IsFalse(ReferenceEquals(fm, fm2));

            fm = (FactoryMethods) factory["testObjectOnlyPrototype"];
            fm2 = (FactoryMethods) factory["testObjectOnlyPrototype"];
            Assert.AreEqual(0, fm.Number);
            Assert.AreEqual("default", fm.Name);
            // This comes from the test object
            Assert.AreEqual("Juergen", fm.Object.Name);
            Assert.AreEqual(fm.Number, fm2.Number);
            Assert.AreEqual(fm.Value, fm2.Value);
            // The TestObject reference is resolved to a prototype in the factory
            Assert.AreSame(fm.Object, fm2.Object);
            Assert.IsFalse(ReferenceEquals(fm, fm2));

            fm = (FactoryMethods) factory["fullPrototype"];
            fm2 = (FactoryMethods) factory["fullPrototype"];
            Assert.AreEqual(27, fm.Number);
            Assert.AreEqual("gotcha", fm.Name);
            Assert.AreEqual("Juergen", fm.Object.Name);
            Assert.AreEqual(fm.Number, fm2.Number);
            Assert.AreEqual(fm.Value, fm2.Value);
            // The TestObject reference is resolved to a prototype in the factory
            Assert.AreSame(fm.Object, fm2.Object);
            Assert.IsFalse(ReferenceEquals(fm, fm2));
        }

        /// <summary>
        /// Tests where the static factory method is on a different class
        /// </summary>
        [Test]
        public void FactoryMethodsOnExternalClass()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("factory-methods.xml", GetType()));
            TestObject to = (TestObject) factory["externalFactoryMethodWithoutArgs"];
            Assert.AreEqual(2, to.Age);
            Assert.AreEqual("Tristan", to.Name);

            to = (TestObject) factory["externalFactoryMethodWithArgs"];
            Assert.AreEqual(33, to.Age);
            Assert.AreEqual("Rod", to.Name);
        }

        [Test]
        public void InstanceFactoryMethodWithoutArgs()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("factory-methods.xml", GetType()));
            FactoryMethods fm = (FactoryMethods) factory["instanceFactoryMethodWithoutArgs"];
            Assert.AreEqual("instanceFactory", fm.Object.Name);
        }

        [Test(Description = "http://opensource.atlassian.com/projects/spring/browse/SPRNET-492")]
        public void InstanceFactoryMethodWithOverloadedArgs()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("factory-methods.xml", GetType()));
            DataTable table = MakeNamesTable();
            DataRow row = table.NewRow();
            //FactoryMethods fm = (FactoryMethods) factory.GetObject("instanceFactoryMethodOverloads", new object[] {row});
            // Assert.AreEqual("DataRowCtor", fm.Name);

            IDataRecord dataRecord = A.Fake<IDataRecord>();
            FactoryMethods fm = (FactoryMethods)factory.GetObject("instanceFactoryMethodOverloads", new object[] { dataRecord });
            Assert.AreEqual("DataRecordCtor", fm.Name);
        }

        private DataTable MakeNamesTable()
        {
            // Create a new DataTable titled 'Names.'
            DataTable namesTable = new DataTable("Names");

            // Add three column objects to the table.
            DataColumn idColumn = new DataColumn();
            idColumn.DataType = System.Type.GetType("System.Int32");
            idColumn.ColumnName = "id";
            idColumn.AutoIncrement = true;
            namesTable.Columns.Add(idColumn);

            DataColumn fNameColumn = new DataColumn();
            fNameColumn.DataType = System.Type.GetType("System.String");
            fNameColumn.ColumnName = "Fname";
            fNameColumn.DefaultValue = "Fname";
            namesTable.Columns.Add(fNameColumn);

            DataColumn lNameColumn = new DataColumn();
            lNameColumn.DataType = System.Type.GetType("System.String");
            lNameColumn.ColumnName = "LName";
            namesTable.Columns.Add(lNameColumn);

            // Create an array for DataColumn objects.
            DataColumn[] keys = new DataColumn[1];
            keys[0] = idColumn;
            namesTable.PrimaryKey = keys;

            // Return the new DataTable.
            return namesTable;
        }

        [Test]
        public void FactoryMethodNoMatchingStaticMethod()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("factory-methods.xml", GetType()));
            Assert.Throws<ObjectCreationException>(() => factory.GetObject("noMatchPrototype"));
        }

        [Test]
        public void CanSpecifyFactoryMethodArgumentsOnFactoryMethodPrototype()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("factory-methods.xml", GetType()));
            TestObject toArg = new TestObject();
            toArg.Name = "arg1";
            TestObject toArg2 = new TestObject();
            toArg2.Name = "arg2";
            FactoryMethods fm1 = (FactoryMethods) factory.GetObject("testObjectOnlyPrototype", new object[] {toArg});
            FactoryMethods fm2 = (FactoryMethods) factory.GetObject("testObjectOnlyPrototype", new object[] {toArg2});

            Assert.AreEqual(0, fm1.Number);
            Assert.AreEqual("default", fm1.Name);
            // This comes from the test object
            Assert.AreEqual("arg1", fm1.Object.Name);
            Assert.AreEqual("arg2", fm2.Object.Name);
            Assert.AreEqual(fm1.Number, fm2.Number);
            Assert.AreEqual(fm2.Value, "testObjectOnlyPrototypeDISetterString");
            Assert.AreEqual(fm2.Value, fm2.Value);
            // The TestObject reference is resolved to a prototype in the factory
            Assert.AreSame(fm2.Object, fm2.Object);
            Assert.IsFalse(ReferenceEquals(fm1, fm2));
        }

        //[Test]
        //[ExpectedException(typeof(ObjectDefinitionStoreException))]
        //public void CannotSpecifyFactoryMethodArgumentsOnSingleton()
        //{
        //    DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
        //    XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
        //    reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("factory-methods.xml", GetType()));
        //    factory.GetObject("testObjectOnly", new object[] {new TestObject()});
        //}

        //[Test]
        //[ExpectedException(typeof(ObjectDefinitionStoreException))]
        //public void CannotSpecifyFactoryMethodArgumentsExceptWithFactoryMethod()
        //{
        //    DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
        //    XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
        //    reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("overrides.xml", GetType()));
        //    factory.GetObject("overrideOnPrototype", new object[] {new TestObject()});
        //}

        [Test]
        public void StaticPropertyRetrievingFactoryMethod()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("field-props-factory.xml", GetType()));
            MyTestObject obj = factory.GetObject("cultureAware", typeof(MyTestObject)) as MyTestObject;
            Assert.IsNotNull(obj.Culture);
            Assert.AreEqual(CultureInfo.CurrentUICulture, obj.Culture);
        }

        [Test]
        public void StaticFieldRetrievingFactoryMethod()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("field-props-factory.xml", GetType()));
            MyTestObject obj = factory.GetObject("withTypesField", typeof(MyTestObject)) as MyTestObject;
            Assert.IsNotNull(obj.Types);
            Assert.AreEqual(Type.EmptyTypes.Length, obj.Types.Length);
        }

        [Test]
        public void InstancePropertyRetrievingFactoryMethod()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("field-props-factory.xml", GetType()));
            MyTestObject obj = factory.GetObject("instancePropertyCultureAware", typeof(MyTestObject)) as MyTestObject;
            Assert.IsNotNull(obj.Culture);
            Assert.AreEqual(new MyTestObject().MyDefaultCulture, obj.Culture);
        }

        [Test]
        public void InstanceFieldRetrievingFactoryMethod()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("field-props-factory.xml", GetType()));
            MyTestObject obj = factory.GetObject("instanceCultureAware", typeof(MyTestObject)) as MyTestObject;
            Assert.IsNotNull(obj.Culture);
            Assert.AreEqual(new MyTestObject().Default, obj.Culture);
        }

        [Test]
        public void BailsOnRubbishFieldRetrievingFactoryMethod()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("field-props-factory.xml", GetType()));
            Assert.Throws<ObjectCreationException>(() => factory.GetObject("rubbishField", typeof(MyTestObject)));
        }

        [Test]
        public void BailsOnRubbishPropertyRetrievingFactoryMethod()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("field-props-factory.xml", GetType()));
            Assert.Throws<ObjectCreationException>(() => factory.GetObject("rubbishProperty", typeof(MyTestObject)));
        }

        /// <summary>
        /// Test creating an object using its 1 arg boolean constructor (amongst
        /// others) by specifying the constructor arguments type and
        /// not its index number.
        /// </summary>
        [Test]
        public void ConstructorArgWithSimpleTypeMatch()
        {
            XmlObjectFactory xof
                = new XmlObjectFactory(
                    new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
            SingleSimpleTypeConstructorObject obj =
                (SingleSimpleTypeConstructorObject) xof.GetObject("objectWithBoolean");
            Assert.IsTrue(obj.SingleBoolean);
        }

        /// <summary>
        /// Test creating an object using its two argument constructor (amongst
        /// others) by specifying both ctor arguments type and not their index
        /// numbers.
        /// </summary>
        [Test]
        public void ConstructorArgWithDoubleSimpleTypeMatch()
        {
            XmlObjectFactory xof
                = new XmlObjectFactory(
                    new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
            SingleSimpleTypeConstructorObject obj =
                (SingleSimpleTypeConstructorObject) xof.GetObject("objectWithBooleanAndString");
            Assert.IsTrue(obj.SecondBoolean);
            Assert.AreEqual("A String", obj.TestString);
        }

        [Test]
        public void DoubleBooleanAutowire()
        {
            XmlObjectFactory xof
                = new XmlObjectFactory(
                    new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
            DoubleBooleanConstructorObject obj =
                (DoubleBooleanConstructorObject) xof.GetObject("objectWithDoubleBoolean");
            Assert.IsTrue(obj.Boolean1);
            Assert.IsFalse(obj.Boolean2);
        }

        [Test]
        public void ParsesNamedCtorArgsCorrectly()
        {
            XmlObjectFactory fac = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
            IObjectDefinition obj = fac.GetObjectDefinition("ctorArgsAllNamed");
            Assert.AreEqual(2, obj.ConstructorArgumentValues.NamedArgumentValues.Count,
                            "Should have parsed 2 named ctor args.");
            ConstructorArgumentValues.ValueHolder nameArg = obj.ConstructorArgumentValues.GetNamedArgumentValue("name");
            Assert.IsNotNull(nameArg, "Should have parsed the 'name' ctor arg.");
            Assert.AreEqual("Isaac Newton", nameArg.Value);
            ConstructorArgumentValues.ValueHolder ageArg = obj.ConstructorArgumentValues.GetNamedArgumentValue("age");
            Assert.IsNotNull(ageArg, "Should have parsed the 'age' ctor arg.");
            Assert.AreEqual("87", ageArg.Value);
        }

        [Test]
        public void InstantiateObjectViaNamedArgsToInnerMethodInvokingFactoryObject()
        {
            XmlObjectFactory fac = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
            ITestObject obj = (ITestObject) fac.GetObject("grabCtorArgFromMethodInvokingFactoryObject");
            Assert.AreEqual("Mr Isaac Newton Phd PPQ MC DJ", obj.Name);
            Assert.AreEqual(198, obj.Age);
        }

        [Test]
        public void BailsWhenBothNameAndIndexAttributesAreAppliedToASingleCtorArg()
        {
            Assert.Throws<ObjectDefinitionStoreException>(() => new XmlObjectFactory(new ReadOnlyXmlTestResource("bad-named-constructor-arg.xml", GetType())));
        }

        [Test]
        public void GetObjectThatUsesCtorArgValueShortcuts()
        {
            XmlObjectFactory xof = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
            TestObject to = (TestObject) xof.GetObject("objectWithCtorArgValueShortcuts");
            Assert.AreEqual("Rick", to.Name);
            Assert.AreEqual(30, to.Age);
        }

        [Test]
        public void GetObjectThatUsesCtorArgRefShortcuts()
        {
            XmlObjectFactory xof = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
            TestObject to = (TestObject) xof.GetObject("objectWithCtorArgRefShortcuts");
            ITestObject spouse = to.Spouse;
            Assert.IsNotNull(spouse, "Dependency not wired in when using 'ref' attribute shortcut.");
            Assert.AreEqual("Kerry2", spouse.Name);
        }

        [Test(Description="SPR-1313")]
        public void UseStaticFactoryMethodInnerObjectDefinition()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_UseStaticFactoryMethodInnerObjectDefinition)).Run();
        }

        private void _UseStaticFactoryMethodInnerObjectDefinition(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='foo' type='Spring.Objects.TestObject, Spring.Core.Tests'>
        <property name='spouse'>
            <object
                factory-method='CreateTestObject'
                type='Spring.Objects.Factory.Xml.TestObjectCreator, Spring.Core.Tests'/>
        </property>
    </object>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            IObjectFactory factory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
            ITestObject to = (ITestObject) factory.GetObject("foo");
            Assert.IsNotNull( to.Spouse );
        }

        [Test(Description="SPR-1313")]
        public void UseInstanceFactoryMethodInnerObjectDefinition()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_UseInstanceFactoryMethodInnerObjectDefinition)).Run();
        }

        private void _UseInstanceFactoryMethodInnerObjectDefinition(out Stream stream)
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='foo' type='Spring.Objects.TestObject, Spring.Core.Tests'>
        <property name='spouse'>
            <object
                factory-method='InstanceCreateTestObject' factory-object='creator'/>
        </property>
    </object>
    <object id='creator' type='Spring.Objects.Factory.Xml.TestObjectCreator, Spring.Core.Tests'/>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            IObjectFactory factory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
            factory.GetObject("foo");
        }

#if !NETCOREAPP
        [Test]
        public void TestExpressionAttribute()
        {
            TypeRegistry.RegisterType("WebMethod", typeof(WebMethodAttribute));

            IResource resource = new ReadOnlyXmlTestResource("expressions.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);

            ExpressionTestObject eto = (ExpressionTestObject) xof.GetObject("to1");
            Assert.AreEqual(new DateTime(1974, 8, 24).ToString("m"), eto.SomeString);
            Assert.AreEqual(new DateTime(2004, 8, 14), eto.SomeDate);
            Assert.IsInstanceOf(typeof(IExpression), eto.ExpressionOne);
            Assert.IsInstanceOf(typeof(IExpression), eto.ExpressionTwo);
            Assert.AreEqual(DateTime.Today, eto.ExpressionOne.GetValue());
            Assert.AreEqual(String.Empty, eto.ExpressionTwo.GetValue());
            Assert.IsInstanceOf(typeof(WebMethodAttribute), eto.SomeDictionary["method1"]);
            Assert.IsInstanceOf(typeof(WebMethodAttribute), eto.SomeDictionary["method2"]);
            Assert.AreEqual("My First Web Method", ((WebMethodAttribute) eto.SomeDictionary["method1"]).Description);
            Assert.AreEqual("My Second Web Method", ((WebMethodAttribute) eto.SomeDictionary["method2"]).Description);
        }

        [Test]
        public void TestExpressionElement()
        {
            TypeRegistry.RegisterType("WebMethod", typeof(WebMethodAttribute));

            IResource resource = new ReadOnlyXmlTestResource("expressions.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);

            ExpressionTestObject eto = (ExpressionTestObject)xof.GetObject("to2");
            Assert.AreEqual(new DateTime(1974, 8, 24).ToString("m"), eto.SomeString);
            Assert.AreEqual(new DateTime(2004, 8, 14), eto.SomeDate);
            Assert.IsInstanceOf(typeof(IExpression), eto.ExpressionOne);
            Assert.IsInstanceOf(typeof(IExpression), eto.ExpressionTwo);
            Assert.AreEqual(DateTime.Today, eto.ExpressionOne.GetValue());
            Assert.AreEqual(String.Empty, eto.ExpressionTwo.GetValue());
            Assert.IsInstanceOf(typeof(WebMethodAttribute), eto.SomeDictionary["method1"]);
            Assert.IsInstanceOf(typeof(WebMethodAttribute), eto.SomeDictionary["method2"]);
            Assert.AreEqual("My First Web Method", ((WebMethodAttribute)eto.SomeDictionary["method1"]).Description);
            Assert.AreEqual("My Second Web Method", ((WebMethodAttribute)eto.SomeDictionary["method2"]).Description);
        }
#endif

        public class LazyWorker
        {
            private XmlObjectFactory xof;
            private Object objectFromContext;
            public LazyWorker(XmlObjectFactory xof)
            {
               this.xof = xof;
            }
            public void DoWork()
            {
                objectFromContext = xof.GetObject("lazyObject");
            }

            public Object ObjectFromContext => objectFromContext;
        }
        public sealed class MyTestObject
        {
            public readonly CultureInfo Default = new CultureInfo("af-ZA");

            public Type[] Types
            {
                get => _types;
                set => _types = value;
            }

            public CultureInfo Culture
            {
                get => _culture;
                set => _culture = value;
            }

            public CultureInfo MyDefaultCulture => Default;

            private Type[] _types;
            private CultureInfo _culture;
        }

        public class BadInitializer
        {
            public void Init2()
            {
                throw new FormatException();
            }
        }

        public class DoubleInitializer
        {
            public int Num
            {
                get => num;
                set => num = value;
            }

            private int num;

            public void Init()
            {
                this.num *= 2;
            }
        }

        public class DefaultDestroyer
        {
            public bool customDestroyed;

            public void CustomDestroy()
            {
                customDestroyed = true;
            }
        }

        public class InitAndIB : IInitializingObject, IDisposable
        {
            public static bool constructed;
            public bool afterPropertiesSetInvoked, initMethodInvoked, destroyed, customDestroyed;

            public InitAndIB()
            {
                constructed = true;
            }

            public void AfterPropertiesSet()
            {
                if (this.initMethodInvoked)
                {
                    Assert.Fail();
                }
                this.afterPropertiesSetInvoked = true;
            }

            public void CustomInit()
            {
                if (!this.afterPropertiesSetInvoked)
                {
                    Assert.Fail();
                }
                this.initMethodInvoked = true;
            }

            public void Dispose()
            {
                if (this.customDestroyed)
                {
                    Assert.Fail();
                }
                if (this.destroyed)
                {
                    throw new ApplicationException("Already destroyed");
                }
                this.destroyed = true;
            }

            public void CustomDestroy()
            {
                if (!this.destroyed)
                {
                    Assert.Fail();
                }
                if (this.customDestroyed)
                {
                    throw new ApplicationException("Already customDestroyed");
                }
                this.customDestroyed = true;
            }
        }

        public class PreparingObject1 : IDisposable
        {
            public static bool prepared = false;
            public static bool destroyed = false;

            public PreparingObject1()
            {
                prepared = true;
            }

            public void Dispose()
            {
                destroyed = true;
            }
        }

        public class PreparingObject2 : IDisposable
        {
            public static bool prepared = false;
            public static bool destroyed = false;

            public PreparingObject2()
            {
                prepared = true;
            }

            public void Dispose()
            {
                destroyed = true;
            }
        }

        public class DependingObject : IDisposable
        {
            public static bool destroyed = false;

            public DependingObject()
            {
                if (!(PreparingObject1.prepared && PreparingObject2.prepared))
                {
                    throw new ApplicationException("Need prepared PreparedObjects!");
                }
            }

            public void Dispose()
            {
                if (PreparingObject1.destroyed || PreparingObject2.destroyed)
                {
                    throw new ApplicationException("Should not be destroyed before PreparedObjects");
                }
                destroyed = true;
            }
        }
    }


}