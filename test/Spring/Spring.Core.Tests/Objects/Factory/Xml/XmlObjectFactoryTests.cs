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

using System.Collections;
using System.Data;
using System.Globalization;
using System.Text;
#if !NETCOREAPP
using System.Web.Services;
#endif

using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Util;

namespace Spring.Objects.Factory.Xml;

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
        LogManager.LoggerFactory = NullLoggerFactory.Instance;
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
        Assert.That(to, Is.Not.Null);
        Assert.That(to.GetType(), Is.EqualTo(typeof(TestObject)));
        TestObject target = (TestObject) to;
        Assert.That(target.Name, Is.EqualTo("Fiona Apple"));
        Assert.That(target.Age, Is.EqualTo(47));
        // pull the prototype out again...
        TestObject target2 = (TestObject) tof.GetObject();
        Assert.That(ReferenceEquals(target, target2), Is.False);
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
        Assert.That(obj is IObjectDefinition, Is.True);
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
        Assert.That(foo, Is.Not.Null, "Alias was obviously not registered otherwise we would not have got a null object back.");
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
        Assert.That(foo, Is.Not.Null, "Alias was obviously not registered otherwise we would not have got a null object back.");
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
        Assert.That(myString, Is.EqualTo("foo"));
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
        Assert.That(foo.Name, Is.EqualTo("Bingo"));
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
        Assert.That(value.Value.GetType(), Is.EqualTo(typeof(TypedStringValue)));
        TestObject obj = (TestObject) fac.GetObject("golyadkin");
        Assert.That(ArrayUtils.AreEqual(new string[] { "trilby", "fedora" }, obj.Hats), Is.True);
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
        var names = factory.GetObjectDefinitionNames();
        // mmm, how is one to test this? I have no idea what the generated name is...
        Assert.That(names.Count, Is.EqualTo(2), "Should have got two object names, one of which is autogenerated.");
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
        Assert.That(rick, Is.Not.Null);
        Assert.That(rick.Name, Is.EqualTo("Rick"));
        // object comes from base file...
        ITestObject jenny = xof["jenny"] as ITestObject;
        Assert.That(jenny, Is.Not.Null);
        Assert.That(jenny.Name, Is.EqualTo("Jenny"));
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
        Assert.That(actual.MyCulture, Is.EqualTo(expected));
    }

    [Test]
    public void LoadFromConfig()
    {
        IObjectFactory factory = ConfigurationUtils.GetSection("objects") as IObjectFactory;

        Assert.That(factory, Is.Not.Null, "Factory loaded from config was null.");
        Assert.That(factory is IConfigurableListableObjectFactory, Is.True);
        IConfigurableListableObjectFactory clof
            = factory as IConfigurableListableObjectFactory;
        Assert.That(clof.ObjectDefinitionCount == 6, Is.True);
        Assert.That(factory["foo"], Is.Not.Null);
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
        Assert.That(validEmpty.Age, Is.EqualTo(0));
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
        Assert.That(xof.ObjectDefinitionCount == 9, Is.True, "9 objects in reftypes, not " + xof.ObjectDefinitionCount);
        TestObject emma = (TestObject) xof.GetObject("emma");
        TestObject georgia = (TestObject) xof.GetObject("georgia");
        ITestObject emmasJenks = emma.Spouse;
        ITestObject georgiasJenks = georgia.Spouse;
        Assert.That(emmasJenks != georgiasJenks, Is.True, "Emma and georgia think they have a different boyfriend.");
        Assert.That(emmasJenks.Name.Equals("Andrew"), Is.True, "Emmas jenks has right name");
        Assert.That(emmasJenks != xof.GetObject("jenks"), Is.True, "Emmas doesn't equal new ref.");
        Assert.That(emmasJenks.Name.Equals("Andrew"), Is.True, "Georgias jenks has right name.");
        Assert.That(emmasJenks.Equals(georgiasJenks), Is.True, "They are object equal.");
        Assert.That(emmasJenks.Equals(xof.GetObject("jenks")), Is.True, "They object equal direct ref.");
    }

    [Test]
    public void RefToSingleton()
    {
        DefaultListableObjectFactory xof = new DefaultListableObjectFactory();
        XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(xof);
        reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("reftypes.xml", GetType()));
        Assert.That(xof.ObjectDefinitionCount == 9, Is.True, "9 objects in reftypes, not " + xof.ObjectDefinitionCount);
        TestObject jen = (TestObject) xof.GetObject("jenny");
        TestObject dave = (TestObject) xof.GetObject("david");
        TestObject jenks = (TestObject) xof.GetObject("jenks");
        ITestObject davesJen = dave.Spouse;
        ITestObject jenksJen = jenks.Spouse;
        Assert.That(davesJen == jenksJen, Is.True, "1 jen instance");
        Assert.That(davesJen == jen, Is.True, "1 jen instance");
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
        Assert.That(hasInnerObjects.Age, Is.EqualTo(5));
        TestObject inner1 = (TestObject) hasInnerObjects.Spouse;
        Assert.That(inner1, Is.Not.Null);
        Assert.That(inner1.ObjectName.Substring(0, inner1.ObjectName.IndexOf("#") + 1), Is.EqualTo("Spring.Objects.TestObject#"));
        Assert.That(inner1.Name, Is.EqualTo("inner1"));
        Assert.That(inner1.Age, Is.EqualTo(6));

        Assert.That(hasInnerObjects.Friends, Is.Not.Null);
        IList friends = (IList) hasInnerObjects.Friends;
        Assert.That(friends.Count, Is.EqualTo(2));
        DerivedTestObject inner2 = (DerivedTestObject) friends[0];
        Assert.That(inner2.Name, Is.EqualTo("inner2"));
        Assert.That(inner2.Age, Is.EqualTo(7));
        Assert.That(inner2.ObjectName.Substring(0, inner2.ObjectName.IndexOf("#") + 1), Is.EqualTo("Spring.Objects.DerivedTestObject#"));
        TestObject innerFactory = (TestObject) friends[1];
        Assert.That(innerFactory.Name, Is.EqualTo(DummyFactory.SINGLETON_NAME));

        Assert.That(hasInnerObjects.SomeMap, Is.Not.Null);
        Assert.That((hasInnerObjects.SomeMap.Count == 0), Is.False);
        TestObject inner3 = (TestObject) hasInnerObjects.SomeMap["someKey"];
        Assert.That(inner3.Name, Is.EqualTo("Jenny"));
        Assert.That(inner3.Age, Is.EqualTo(30));
        xof.Dispose();
        Assert.That(inner2.WasDestroyed(), Is.True);
        Assert.That(innerFactory.Name == null, Is.True);
    }

    [Test]
    public void InnerObjectsInPrototype()
    {
        DefaultListableObjectFactory xof = new DefaultListableObjectFactory();
        XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(xof);
        reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("reftypes.xml", GetType()));
        TestObject hasInnerObjects = (TestObject) xof.GetObject("prototypeHasInnerObjects");
        Assert.That(hasInnerObjects.Age, Is.EqualTo(5));

        Assert.That(hasInnerObjects.Spouse, Is.Not.Null);
        Assert.That(hasInnerObjects.Spouse.Name, Is.EqualTo("inner1"));
        Assert.That(hasInnerObjects.Spouse.Age, Is.EqualTo(6));
        Assert.That(hasInnerObjects.Friends, Is.Not.Null);
        IList friends = (IList) hasInnerObjects.Friends;
        Assert.That(friends.Count, Is.EqualTo(2));
        DerivedTestObject inner2 = (DerivedTestObject) friends[0];
        Assert.That(inner2.Name, Is.EqualTo("inner2"));
        Assert.That(inner2.Age, Is.EqualTo(7));

        IList friendsOfInner = (IList) inner2.Friends;
        Assert.That(friendsOfInner.Count, Is.EqualTo(1));
        DerivedTestObject innerFriendOfAFriend = (DerivedTestObject) friendsOfInner[0];
        Assert.That(innerFriendOfAFriend.Name, Is.EqualTo("innerFriendOfAFriend"));
        Assert.That(innerFriendOfAFriend.Age, Is.EqualTo(7));
        TestObject innerFactory = (TestObject) friends[1];
        Assert.That(innerFactory.Name, Is.EqualTo(DummyFactory.SINGLETON_NAME));
        Assert.That(hasInnerObjects.SomeMap, Is.Not.Null);
        Assert.That((hasInnerObjects.SomeMap.Count == 0), Is.False);

        TestObject inner3 = (TestObject) hasInnerObjects.SomeMap["someKey"];
        Assert.That(inner3.Name, Is.EqualTo("inner3"));
        Assert.That(inner3.Age, Is.EqualTo(8));
        xof.Dispose();

        Assert.That(inner2.WasDestroyed(), Is.False);
        Assert.That(innerFactory.Name == null, Is.False);
        Assert.That(innerFriendOfAFriend.WasDestroyed(), Is.False);
    }

    [Test]
    public void SingletonInheritanceFromParentFactorySingleton()
    {
        XmlObjectFactory parent = new XmlObjectFactory(new ReadOnlyXmlTestResource("parent.xml", GetType()));
        XmlObjectFactory child = new XmlObjectFactory(new ReadOnlyXmlTestResource("child.xml", GetType()), parent);
        TestObject inherits = (TestObject) child.GetObject("inheritsFromParentFactory");
        // Name property value is overriden
        Assert.That(inherits.Name.Equals("override"), Is.True);
        // Age property is inherited from object in parent factory
        Assert.That(inherits.Age == 1, Is.True);
        TestObject inherits2 = (TestObject) child.GetObject("inheritsFromParentFactory");
        Assert.That(inherits, Is.SameAs(inherits2));
    }

    [Test]
    public void SingletonInheritanceFromParentFactorySingletonUsingCtor()
    {
        XmlObjectFactory parent = new XmlObjectFactory(new ReadOnlyXmlTestResource("parent.xml", GetType()));
        XmlObjectFactory child = new XmlObjectFactory(new ReadOnlyXmlTestResource("child.xml", GetType()), parent);
        TestObject inherits = (TestObject) child.GetObject("inheritsFromParentFactoryUsingCtor");
        // Name property value is overriden
        Assert.That(inherits.Name.Equals("child-name"), Is.True);
        // Age property is inherited from object in parent factory
        Assert.That(inherits.Age == 1, Is.True);
        TestObject inherits2 = (TestObject) child.GetObject("inheritsFromParentFactoryUsingCtor");
        Assert.That(inherits, Is.SameAs(inherits2));
    }

    [Test]
    public void PrototypeInheritanceFromParentFactoryPrototype()
    {
        XmlObjectFactory parent = new XmlObjectFactory(new ReadOnlyXmlTestResource("parent.xml", GetType()));
        XmlObjectFactory child = new XmlObjectFactory(new ReadOnlyXmlTestResource("child.xml", GetType()), parent);
        TestObject inherits = (TestObject) child.GetObject("prototypeInheritsFromParentFactoryPrototype");
        // Name property value is overridden
        Assert.That(inherits.Name.Equals("prototype-override"), Is.True);
        // Age property is inherited from object in parent factory
        Assert.That(inherits.Age == 2, Is.True);
        TestObject inherits2 = (TestObject) child.GetObject("prototypeInheritsFromParentFactoryPrototype");
        Assert.That(inherits, Is.Not.SameAs(inherits2));
        inherits2.Age = 13;
        Assert.That(inherits2.Age == 13, Is.True);
        // Shouldn't have changed first instance
        Assert.That(inherits.Age == 2, Is.True);
    }

    [Test]
    public void PrototypeInheritanceFromParentFactorySingleton()
    {
        XmlObjectFactory parent = new XmlObjectFactory(new ReadOnlyXmlTestResource("parent.xml", GetType()));
        XmlObjectFactory child = new XmlObjectFactory(new ReadOnlyXmlTestResource("child.xml", GetType()), parent);
        TestObject inherits = (TestObject) child.GetObject("protoypeInheritsFromParentFactorySingleton");
        // Name property value is overridden
        Assert.That(inherits.Name.Equals("prototypeOverridesInheritedSingleton"), Is.True);
        // Age property is inherited from object in parent factory
        Assert.That(inherits.Age == 1, Is.True);
        TestObject inherits2 = (TestObject) child.GetObject("protoypeInheritsFromParentFactorySingleton");
        Assert.That(inherits, Is.Not.SameAs(inherits2));
        inherits2.Age = 13;
        Assert.That(inherits2.Age == 13, Is.True);
        // Shouldn't have changed first instance
        Assert.That(inherits.Age == 1, Is.True);
    }

    [Test]
    public void DependenciesMaterializeThis()
    {
        IResource resource = new ReadOnlyXmlTestResource("dependenciesmaterializethis.xml", GetType());
        XmlObjectFactory bf = new XmlObjectFactory(resource);
        DummyBo bos = (DummyBo) bf.GetObject("boSingleton");
        DummyBo bop = (DummyBo) bf.GetObject("boPrototype");
        Assert.That(bos == bop, Is.False);
        Assert.That(bop.dao, Is.EqualTo(bos.dao));
    }

    [Test]
    public void ChildOverridesParentObject()
    {
        XmlObjectFactory parent = new XmlObjectFactory(new ReadOnlyXmlTestResource("parent.xml", GetType()));
        XmlObjectFactory child = new XmlObjectFactory(new ReadOnlyXmlTestResource("child.xml", GetType()), parent);
        TestObject inherits = (TestObject) child.GetObject("inheritedTestObject");
        // Name property value is overridden
        Assert.That(inherits.Name.Equals("overrideParentObject"), Is.True);
        // Age property is inherited from object in parent factory
        Assert.That(inherits.Age == 1, Is.True);
        TestObject inherits2 = (TestObject) child.GetObject("inheritedTestObject");
        Assert.That(inherits2 == inherits, Is.True);
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
        Assert.That(inherits.Name.Equals("prototype-override"), Is.True);
        // Age property is inherited from object in parent factory
        Assert.That(inherits.Age == 2, Is.True);
        TestObject inherits2 = (TestObject) child.GetObject("singletonInheritsFromParentFactoryPrototype");
        Assert.That(inherits2 == inherits, Is.True);
    }

    [Test]
    public void AbstractParentObjects()
    {
        XmlObjectFactory parent = new XmlObjectFactory(new ReadOnlyXmlTestResource("parent.xml", GetType()));
        parent.PreInstantiateSingletons();
        Assert.That(parent.IsSingleton("inheritedTestObjectWithoutClass"), Is.True);

        // abstract objects should not match
        //TODO add overloaded GetObjectOfType with 1 arg
        var tbs = parent.GetObjectsOfType(typeof(TestObject), true, true);
        Assert.That(tbs.Count, Is.EqualTo(2));
        Assert.That(tbs.ContainsKey("inheritedTestObjectPrototype"), Is.True);
        Assert.That(tbs.ContainsKey("inheritedTestObjectSingleton"), Is.True);

        // non-abstract object should work, even if it serves as parent
        object o1 = parent.GetObject("inheritedTestObjectPrototype");
        Assert.That(o1 is TestObject, Is.True);

        // abstract object should return IObjectInstance itself
        object o2 = parent.GetObject("inheritedTestObjectWithoutClass");
        Assert.That(o2 is IObjectDefinition, Is.True);
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
        Assert.That(jenny.Spouse == david, Is.True, "Correct circular reference");
        Assert.That(david.Spouse == jenny, Is.True, "Correct circular reference");
        Assert.That(ego.Spouse == ego, Is.True, "Correct circular reference");
    }

    [Test]
    public void FactoryReferenceCircle()
    {
        IResource resource = new ReadOnlyXmlTestResource("factorycircle.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        TestObject tb = (TestObject) xof.GetObject("singletonFactory");
        DummyFactory db = (DummyFactory) xof.GetObject("&singletonFactory");
        Assert.That(tb == db.OtherTestObject, Is.True);
    }

    [Test]
    public void RefSubelement()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        //Assert.IsTrue ("5 objects in reftypes, not " + xof.GetObjectDefinitionCount(), xof.GetObjectDefinitionCount() == 5);
        TestObject jen = (TestObject) xof.GetObject("jenny");
        TestObject dave = (TestObject) xof.GetObject("david");
        Assert.That(jen.Spouse == dave, Is.True);
    }

    [Test]
    public void PropertyWithLiteralValueSubelement()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        TestObject verbose = (TestObject) xof.GetObject("verbose");
        Assert.That(verbose.Name.Equals("verbose"), Is.True);
    }

    [Test]
    public void PropertyWithIdRefLocalAttrSubelement()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        TestObject verbose = (TestObject) xof.GetObject("verbose2");
        Assert.That(verbose.Name.Equals("verbose"), Is.True);
    }

    [Test]
    public void PropertyWithIdRefObjectAttrSubelement()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        TestObject verbose = (TestObject) xof.GetObject("verbose3");
        Assert.That(verbose.Name.Equals("verbose"), Is.True);
    }

    [Test]
    public void EnumProperty()
    {
        DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
        XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
        reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("enums.xml", GetType()));
        TestObject obj = factory.GetObject("rod", typeof(TestObject)) as TestObject;
        Assert.That((object) obj.FileMode, Is.Not.Null);
        Assert.That(obj.FileMode, Is.EqualTo(FileMode.Create));
    }

    [Test]
    public void InitMethodIsInvoked()
    {
        IResource resource = new ReadOnlyXmlTestResource("initializers.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        DoubleInitializer in_Renamed = (DoubleInitializer) xof.GetObject("init-method1");
        // Initializer should have doubled value
        Assert.That(in_Renamed.Num, Is.EqualTo(14));
    }

    [Test]
    public void DefaultInitMethodIsInvoked()
    {
        IResource resource = new ReadOnlyXmlTestResource("default-initializers.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        DoubleInitializer in_Renamed = (DoubleInitializer) xof.GetObject("init-method1");
        // Initializer should have doubled value
        Assert.That(in_Renamed.Num, Is.EqualTo(14));
    }

    [Test]
    public void DefaultInitMethodDisabled()
    {
        IResource resource = new ReadOnlyXmlTestResource("default-initializers.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        DoubleInitializer in_Renamed = (DoubleInitializer) xof.GetObject("init-method2");
        // Initializer should have doubled value
        Assert.That(in_Renamed.Num, Is.EqualTo(7));
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
            Assert.That(ex.InnerException is FormatException, Is.True);
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
        Assert.That(InitAndIB.constructed, Is.False);
        xof.PreInstantiateSingletons();
        Assert.That(InitAndIB.constructed, Is.False);
        InitAndIB iib = (InitAndIB) xof.GetObject("init-and-ib");
        Assert.That(InitAndIB.constructed, Is.True);
        Assert.That(iib.afterPropertiesSetInvoked && iib.initMethodInvoked, Is.True);
        Assert.That(!iib.destroyed && !iib.customDestroyed, Is.True);
        xof.Dispose();
        Assert.That(iib.destroyed && iib.customDestroyed, Is.True);
        xof.Dispose();
        Assert.That(iib.destroyed && iib.customDestroyed, Is.True);
    }

    [Test]
    public void DefaultDestroyMethodInvoked()
    {
        IResource resource = new ReadOnlyXmlTestResource("default-destroy-methods.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        xof.PreInstantiateSingletons();
        DefaultDestroyer dd = (DefaultDestroyer) xof.GetObject("destroy-method1");
        Assert.That(!dd.customDestroyed, Is.True);
        xof.Dispose();
        Assert.That(dd.customDestroyed, Is.True);
    }

    [Test]
    public void DefaultDestroyMethodDisabled()
    {
        IResource resource = new ReadOnlyXmlTestResource("default-destroy-methods.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        xof.PreInstantiateSingletons();
        DefaultDestroyer dd = (DefaultDestroyer) xof.GetObject("destroy-method2");
        Assert.That(!dd.customDestroyed, Is.True);
        xof.Dispose();
        Assert.That(!dd.customDestroyed, Is.True);
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
        Assert.That(lw1.ObjectFromContext.GetType(), Is.EqualTo(typeof(LazyTestObject)));
        Assert.That(lw2.ObjectFromContext.GetType(), Is.EqualTo(typeof(LazyTestObject)));
        Assert.That(LazyTestObject.Count, Is.EqualTo(1));
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
        Assert.That(InitAndIB.constructed, Is.False);
        xof.PreInstantiateSingletons();
        Assert.That(InitAndIB.constructed, Is.True);
        try
        {
            xof.GetObject("lazy-and-bad");
        }
        catch (ObjectCreationException ex)
        {
            Assert.That(ex.InnerException is FormatException, Is.True);
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
        Assert.That(InitAndIB.constructed, Is.False);
        xof.PreInstantiateSingletons();
        Assert.That(InitAndIB.constructed, Is.True);
        try
        {
            xof.GetObject("init-and-ib-no-init-in-local-object-def");
        }
        catch (ObjectCreationException ex)
        {
            Assert.That(ex.InnerException is FormatException, Is.True);
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
            Assert.That(e.Message.IndexOf("Line 21 in XML document"), Is.EqualTo(0));
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
            Assert.That(e.Message.IndexOf("Line 21 in XML document"), Is.EqualTo(0));
        }
    }

    [Test]
    public void UnsatisfiedObjectDependencyCheck()
    {
        XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("unsatisfiedobjectdependencycheck.xml", GetType()));
        Assert.Throws<UnsatisfiedDependencyException>(() => xof.GetObject("a", typeof(DependenciesObject)));
    }

    [Test]
    public void UnsatisfiedSimpleDependencyCheck()
    {
        XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("unsatisfiedsimpledependencycheck.xml", GetType()));
        Assert.Throws<UnsatisfiedDependencyException>(() => xof.GetObject("a", typeof(DependenciesObject)));
    }

    [Test]
    public void SatisfiedObjectDependencyCheck()
    {
        XmlObjectFactory xof
            = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("satisfiedobjectdependencycheck.xml", GetType()));
        DependenciesObject a = (DependenciesObject) xof.GetObject("a");
        Assert.That(a.Spouse, Is.Not.Null);
    }

    [Test]
    public void SatisfiedSimpleDependencyCheck()
    {
        XmlObjectFactory xof =
            new XmlObjectFactory(
                new ReadOnlyXmlTestResource(
                    "satisfiedsimpledependencycheck.xml", GetType()));
        DependenciesObject a = (DependenciesObject) xof.GetObject("a");
        Assert.That(a.Age, Is.EqualTo(33));
    }

    [Test]
    public void UnsatisfiedAllDependencyCheck()
    {
        XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("unsatisfiedalldependencycheckmissingobjects.xml", GetType()));
        Assert.Throws<UnsatisfiedDependencyException>(() => xof.GetObject("a", typeof(DependenciesObject)));
    }

    [Test]
    public void SatisfiedAllDependencyCheck()
    {
        XmlObjectFactory xof
            = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("satisfiedalldependencycheck.xml", GetType()));
        DependenciesObject a = (DependenciesObject) xof.GetObject("a");
        Assert.That(a.Age, Is.EqualTo(33));
        Assert.That(a.Name, Is.Not.Null);
        Assert.That(a.Spouse, Is.Not.Null);
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
        ITestObject kerry2 = (ITestObject) xof.GetObject("spouse2");
        ArrayCtorDependencyObject rod7 = (ArrayCtorDependencyObject) xof.GetObject("rod7");

        Assert.That(rod7.Spouse1, Is.EqualTo(kerry));
        Assert.That(rod7.Spouse2, Is.EqualTo(kerry2));
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
        Assert.That(rod1.Spouse, Is.EqualTo(kerry));

        DependenciesObject rod1a = (DependenciesObject) xof.GetObject("rod1a");
        // Should have been autowired
        Assert.That(rod1a.Spouse, Is.EqualTo(kerry));

        DependenciesObject rod2 = (DependenciesObject) xof.GetObject("rod2");
        // Should have been autowired
        Assert.That(rod2.Spouse, Is.EqualTo(kerry));

        ConstructorDependenciesObject rod3 = (ConstructorDependenciesObject) xof.GetObject("rod3");
        IndexedTestObject other = (IndexedTestObject) xof.GetObject("other");
        // Should have been autowired
        Assert.That(rod3.Spouse1, Is.EqualTo(kerry));
        Assert.That(rod3.Spouse2, Is.EqualTo(kerry));
        Assert.That(rod3.Other, Is.EqualTo(other));

        ConstructorDependenciesObject rod3a = (ConstructorDependenciesObject) xof.GetObject("rod3a");
        // Should have been autowired
        Assert.That(rod3a.Spouse1, Is.EqualTo(kerry));
        Assert.That(rod3a.Spouse2, Is.EqualTo(kerry));
        Assert.That(rod3a.Other, Is.EqualTo(other));

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
        Assert.That(rod5.Spouse, Is.Null);

        /* TODO include basc in
        IObjectFactory appCtx = (IObjectFactory) xof.GetObject("childAppCtx");
        Assert.That(appCtx.GetObject("rod1") != null, Is.True);
        Assert.That(appCtx.GetObject("dependingObject") != null, Is.True);
        Assert.That(appCtx.GetObject("jenny") != null, Is.True);
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
        Assert.That(rod1.Spouse, Is.Not.Null);
        Assert.That(rod1.Spouse.Name.Equals("Kerry"), Is.True);

        DependenciesObject rod2 = (DependenciesObject) xof.GetObject("rod2");
        // Should have been autowired
        Assert.That(rod2.Spouse, Is.Not.Null);
        Assert.That(rod2.Spouse.Name.Equals("Kerry"), Is.True);
    }

    [Test]
    public void AutowireByConstructor()
    {
        XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
        ConstructorDependenciesObject rod1 = (ConstructorDependenciesObject) xof.GetObject("rod1");
        TestObject kerry = (TestObject) xof.GetObject("kerry2");
        // Should have been autowired
        Assert.That(rod1.Spouse1, Is.EqualTo(kerry));
        Assert.That(rod1.Age, Is.EqualTo(0));
        Assert.That(rod1.Name, Is.EqualTo(null));

        ConstructorDependenciesObject rod2 = (ConstructorDependenciesObject) xof.GetObject("rod2");
        TestObject kerry1 = (TestObject) xof.GetObject("kerry1");
        TestObject kerry2 = (TestObject) xof.GetObject("kerry2");
        // Should have been autowired
        Assert.That(rod2.Spouse1, Is.EqualTo(kerry2));
        Assert.That(rod2.Spouse2, Is.EqualTo(kerry1));
        Assert.That(rod2.Age, Is.EqualTo(0));
        Assert.That(rod2.Name, Is.EqualTo(null));

        ConstructorDependenciesObject rod = (ConstructorDependenciesObject) xof.GetObject("rod3");
        IndexedTestObject other = (IndexedTestObject) xof.GetObject("other");
        // Should have been autowired
        Assert.That(rod.Spouse1, Is.EqualTo(kerry));
        Assert.That(rod.Spouse2, Is.EqualTo(kerry));
        Assert.That(rod.Other, Is.EqualTo(other));
        Assert.That(rod.Age, Is.EqualTo(0));
        Assert.That(rod.Name, Is.EqualTo(null));
        xof.GetObject("rod4", typeof(ConstructorDependenciesObject));
        // Should have been autowired
        Assert.That(rod.Spouse1, Is.EqualTo(kerry));
        Assert.That(rod.Spouse2, Is.EqualTo(kerry));
        Assert.That(rod.Other, Is.EqualTo(other));
        Assert.That(rod.Age, Is.EqualTo(0));
        Assert.That(rod.Name, Is.EqualTo(null));
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
        Assert.That(rod5.Spouse1, Is.EqualTo(kerry2));
        Assert.That(rod5.Spouse2, Is.EqualTo(kerry1));
        Assert.That(rod5.Other, Is.EqualTo(other));
        Assert.That(rod5.Age, Is.EqualTo(99));
        Assert.That(rod5.Name, Is.EqualTo("myname"));
        ConstructorDependenciesObject rod6 = (ConstructorDependenciesObject) xof.GetObject("rod6");
        // Should have been autowired
        Assert.That(rod6.Spouse1, Is.EqualTo(kerry2));
        Assert.That(rod6.Spouse2, Is.EqualTo(kerry1));
        Assert.That(rod6.Other, Is.EqualTo(other));
        Assert.That(rod6.Age, Is.EqualTo(0));
        Assert.That(rod6.Name, Is.EqualTo(null));
    }

    [Test]
    public void ConstructorArgResolution()
    {
        XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
        TestObject kerry1 = (TestObject) xof.GetObject("kerry1");
        TestObject kerry2 = (TestObject) xof.GetObject("kerry2");
        ConstructorDependenciesObject rod9 = (ConstructorDependenciesObject) xof.GetObject("rod9");
        Assert.That(rod9.Age, Is.EqualTo(99));
        ConstructorDependenciesObject rod10 = (ConstructorDependenciesObject) xof.GetObject("rod10");
        Assert.That(rod10.Name, Is.EqualTo(null));
        ConstructorDependenciesObject rod11 = (ConstructorDependenciesObject) xof.GetObject("rod11");
        Assert.That(rod11.Spouse1, Is.EqualTo(kerry2));

        ConstructorDependenciesObject rod12 = (ConstructorDependenciesObject) xof.GetObject("rod12");
        Assert.That(rod12.Spouse1, Is.EqualTo(kerry1));
        Assert.That(rod12.Spouse2, Is.Null);

        ConstructorDependenciesObject rod13 = (ConstructorDependenciesObject) xof.GetObject("rod13");
        Assert.That(rod13.Spouse1, Is.EqualTo(kerry1));
        Assert.That(rod13.Spouse2, Is.EqualTo(kerry2));
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
        Assert.That(PreparingObject1.prepared, Is.True);
        Assert.That(PreparingObject1.destroyed, Is.True);
        Assert.That(PreparingObject2.prepared, Is.True);
        Assert.That(PreparingObject2.destroyed, Is.True);
        Assert.That(DependingObject.destroyed, Is.True);
    }

    [Test]
    public void ClassNotFoundWithDefault()
    {
        try
        {
            new XmlObjectFactory(
                new ReadOnlyXmlTestResource("classnotfound.xml", GetType()));
        }
        catch (ObjectDefinitionStoreException ex)
        {
            Assert.That(ex.InnerException is TypeLoadException, Is.True);
        }
    }

#if !NETCOREAPP
        [Test]
        [Platform("Win")]
        public void AnObjectCanBeIstantiatedWithANotFullySpecifiedAssemblyName()
        {
            IResource resource = new ReadOnlyXmlTestResource("notfullyspecified.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            IDbConnection connection = (IDbConnection) xof.GetObject("connectionNotFullySpecified");
        Assert.That(connection, Is.Not.Null);
        }
#endif

    [Test]
    public void ResourceAndInputStream()
    {
        string filename = "./temp/spring-test.properties";
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
            Assert.That(resource1.Resource is FileSystemResource, Is.True);
            using (StreamReader reader = new StreamReader(resource1.Resource.InputStream))
            {
                string fileText = reader.ReadLine();
                Assert.That(fileText, Is.EqualTo("test"), "error using IResource to read file contents");
            }

            using (StreamReader reader = new StreamReader(resource1.InputStream))
            {
                string fileText = reader.ReadLine();
                Assert.That(fileText, Is.EqualTo("test"), "error using Stream to read file contents");
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
        Assert.That(fm.Number, Is.EqualTo(0));
        Assert.That(fm.Name, Is.EqualTo("default"));
        Assert.That(fm.Object.Name, Is.EqualTo("defaultInstance"));
        Assert.That(fm.Value, Is.EqualTo("setterString"));

        fm = (FactoryMethods) factory["testObjectOnly"];
        Assert.That(fm.Number, Is.EqualTo(0));
        Assert.That(fm.Name, Is.EqualTo("default"));
        // This comes from the test object
        Assert.That(fm.Object.Name, Is.EqualTo("Juergen"));

        fm = (FactoryMethods) factory["full"];
        Assert.That(fm.Number, Is.EqualTo(27));
        Assert.That(fm.Name, Is.EqualTo("gotcha"));
        Assert.That(fm.Object.Name, Is.EqualTo("Juergen"));

        FactoryMethods fm2 = (FactoryMethods) factory["full"];
        Assert.That(fm2, Is.SameAs(fm));
    }

    [Test]
    public void FactoryMethodsPrototypeOnTargetClass()
    {
        DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
        XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
        reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("factory-methods.xml", GetType()));
        FactoryMethods fm = (FactoryMethods) factory["defaultPrototype"];
        FactoryMethods fm2 = (FactoryMethods) factory["defaultPrototype"];
        Assert.That(fm.Number, Is.EqualTo(0));
        Assert.That(fm.Name, Is.EqualTo("default"));
        Assert.That(fm.Object.Name, Is.EqualTo("defaultInstance"));
        Assert.That(fm.Value, Is.EqualTo("setterString"));
        Assert.That(fm2.Number, Is.EqualTo(fm.Number));
        Assert.That(fm2.Value, Is.EqualTo(fm.Value));
        // The TestObject is created separately for each object
        Assert.That(ReferenceEquals(fm.Object, fm2.Object), Is.False);
        Assert.That(ReferenceEquals(fm, fm2), Is.False);

        fm = (FactoryMethods) factory["testObjectOnlyPrototype"];
        fm2 = (FactoryMethods) factory["testObjectOnlyPrototype"];
        Assert.That(fm.Number, Is.EqualTo(0));
        Assert.That(fm.Name, Is.EqualTo("default"));
        // This comes from the test object
        Assert.That(fm.Object.Name, Is.EqualTo("Juergen"));
        Assert.That(fm2.Number, Is.EqualTo(fm.Number));
        Assert.That(fm2.Value, Is.EqualTo(fm.Value));
        // The TestObject reference is resolved to a prototype in the factory
        Assert.That(fm2.Object, Is.SameAs(fm.Object));
        Assert.That(ReferenceEquals(fm, fm2), Is.False);

        fm = (FactoryMethods) factory["fullPrototype"];
        fm2 = (FactoryMethods) factory["fullPrototype"];
        Assert.That(fm.Number, Is.EqualTo(27));
        Assert.That(fm.Name, Is.EqualTo("gotcha"));
        Assert.That(fm.Object.Name, Is.EqualTo("Juergen"));
        Assert.That(fm2.Number, Is.EqualTo(fm.Number));
        Assert.That(fm2.Value, Is.EqualTo(fm.Value));
        // The TestObject reference is resolved to a prototype in the factory
        Assert.That(fm2.Object, Is.SameAs(fm.Object));
        Assert.That(ReferenceEquals(fm, fm2), Is.False);
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
        Assert.That(to.Age, Is.EqualTo(2));
        Assert.That(to.Name, Is.EqualTo("Tristan"));

        to = (TestObject) factory["externalFactoryMethodWithArgs"];
        Assert.That(to.Age, Is.EqualTo(33));
        Assert.That(to.Name, Is.EqualTo("Rod"));
    }

    [Test]
    public void InstanceFactoryMethodWithoutArgs()
    {
        DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
        XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
        reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("factory-methods.xml", GetType()));
        FactoryMethods fm = (FactoryMethods) factory["instanceFactoryMethodWithoutArgs"];
        Assert.That(fm.Object.Name, Is.EqualTo("instanceFactory"));
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
        // Assert.That(fm.Name, Is.EqualTo("DataRowCtor"));

        IDataRecord dataRecord = A.Fake<IDataRecord>();
        FactoryMethods fm = (FactoryMethods) factory.GetObject("instanceFactoryMethodOverloads", new object[] { dataRecord });
        Assert.That(fm.Name, Is.EqualTo("DataRecordCtor"));
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
        FactoryMethods fm1 = (FactoryMethods) factory.GetObject("testObjectOnlyPrototype", new object[] { toArg });
        FactoryMethods fm2 = (FactoryMethods) factory.GetObject("testObjectOnlyPrototype", new object[] { toArg2 });

        Assert.That(fm1.Number, Is.EqualTo(0));
        Assert.That(fm1.Name, Is.EqualTo("default"));
        // This comes from the test object
        Assert.That(fm1.Object.Name, Is.EqualTo("arg1"));
        Assert.That(fm2.Object.Name, Is.EqualTo("arg2"));
        Assert.That(fm2.Number, Is.EqualTo(fm1.Number));
        Assert.That(fm2.Value, Is.EqualTo("testObjectOnlyPrototypeDISetterString"));
        Assert.That(fm2.Value, Is.EqualTo(fm1.Value));
        // The TestObject reference is resolved to a prototype in the factory
        var firstObject = fm2.Object;
        Assert.That(fm2.Object, Is.SameAs(firstObject));
        Assert.That(ReferenceEquals(fm1, fm2), Is.False);
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
        Assert.That(obj.Culture, Is.Not.Null);
        Assert.That(obj.Culture, Is.EqualTo(CultureInfo.CurrentUICulture));
    }

    [Test]
    public void StaticFieldRetrievingFactoryMethod()
    {
        DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
        XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
        reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("field-props-factory.xml", GetType()));
        MyTestObject obj = factory.GetObject("withTypesField", typeof(MyTestObject)) as MyTestObject;
        Assert.That(obj.Types, Is.Not.Null);
        Assert.That(obj.Types.Length, Is.EqualTo(Type.EmptyTypes.Length));
    }

    [Test]
    public void InstancePropertyRetrievingFactoryMethod()
    {
        DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
        XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
        reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("field-props-factory.xml", GetType()));
        MyTestObject obj = factory.GetObject("instancePropertyCultureAware", typeof(MyTestObject)) as MyTestObject;
        Assert.That(obj.Culture, Is.Not.Null);
        Assert.That(obj.Culture, Is.EqualTo(new MyTestObject().MyDefaultCulture));
    }

    [Test]
    public void InstanceFieldRetrievingFactoryMethod()
    {
        DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
        XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
        reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("field-props-factory.xml", GetType()));
        MyTestObject obj = factory.GetObject("instanceCultureAware", typeof(MyTestObject)) as MyTestObject;
        Assert.That(obj.Culture, Is.Not.Null);
        Assert.That(obj.Culture, Is.EqualTo(new MyTestObject().Default));
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
        Assert.That(obj.SingleBoolean, Is.True);
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
        Assert.That(obj.SecondBoolean, Is.True);
        Assert.That(obj.TestString, Is.EqualTo("A String"));
    }

    [Test]
    public void DoubleBooleanAutowire()
    {
        XmlObjectFactory xof
            = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
        DoubleBooleanConstructorObject obj =
            (DoubleBooleanConstructorObject) xof.GetObject("objectWithDoubleBoolean");
        Assert.That(obj.Boolean1, Is.True);
        Assert.That(obj.Boolean2, Is.False);
    }

    [Test]
    public void ParsesNamedCtorArgsCorrectly()
    {
        XmlObjectFactory fac = new XmlObjectFactory(
            new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
        IObjectDefinition obj = fac.GetObjectDefinition("ctorArgsAllNamed");
        Assert.That(obj.ConstructorArgumentValues.NamedArgumentValues.Count, Is.EqualTo(2),
            "Should have parsed 2 named ctor args.");
        ConstructorArgumentValues.ValueHolder nameArg = obj.ConstructorArgumentValues.GetNamedArgumentValue("name");
        Assert.That(nameArg, Is.Not.Null, "Should have parsed the 'name' ctor arg.");
        Assert.That(nameArg.Value, Is.EqualTo("Isaac Newton"));
        ConstructorArgumentValues.ValueHolder ageArg = obj.ConstructorArgumentValues.GetNamedArgumentValue("age");
        Assert.That(ageArg, Is.Not.Null, "Should have parsed the 'age' ctor arg.");
        Assert.That(ageArg.Value, Is.EqualTo("87"));
    }

    [Test]
    public void InstantiateObjectViaNamedArgsToInnerMethodInvokingFactoryObject()
    {
        XmlObjectFactory fac = new XmlObjectFactory(
            new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
        ITestObject obj = (ITestObject) fac.GetObject("grabCtorArgFromMethodInvokingFactoryObject");
        Assert.That(obj.Name, Is.EqualTo("Mr Isaac Newton Phd PPQ MC DJ"));
        Assert.That(obj.Age, Is.EqualTo(198));
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
        Assert.That(to.Name, Is.EqualTo("Rick"));
        Assert.That(to.Age, Is.EqualTo(30));
    }

    [Test]
    public void GetObjectThatUsesCtorArgRefShortcuts()
    {
        XmlObjectFactory xof = new XmlObjectFactory(
            new ReadOnlyXmlTestResource("constructor-arg.xml", GetType()));
        TestObject to = (TestObject) xof.GetObject("objectWithCtorArgRefShortcuts");
        ITestObject spouse = to.Spouse;
        Assert.That(spouse, Is.Not.Null, "Dependency not wired in when using 'ref' attribute shortcut.");
        Assert.That(spouse.Name, Is.EqualTo("Kerry2"));
    }

    [Test(Description = "SPR-1313")]
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
        Assert.That(to.Spouse, Is.Not.Null);
    }

    [Test(Description = "SPR-1313")]
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
            Spring.Core.TypeResolution.TypeRegistry.RegisterType("WebMethod", typeof(WebMethodAttribute));

            IResource resource = new ReadOnlyXmlTestResource("expressions.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);

            ExpressionTestObject eto = (ExpressionTestObject) xof.GetObject("to1");
        Assert.That(eto.SomeString, Is.EqualTo(new DateTime(1974, 8, 24).ToString("m")));
        Assert.That(eto.SomeDate, Is.EqualTo(new DateTime(2004, 8, 14)));
        Assert.That(eto.ExpressionOne, Is.InstanceOf(typeof(Spring.Expressions.IExpression)));
        Assert.That(eto.ExpressionTwo, Is.InstanceOf(typeof(Spring.Expressions.IExpression)));
        Assert.That(eto.ExpressionOne.GetValue(), Is.EqualTo(DateTime.Today));
        Assert.That(eto.ExpressionTwo.GetValue(), Is.Empty);
        Assert.That(eto.SomeDictionary["method1"], Is.InstanceOf(typeof(WebMethodAttribute)));
        Assert.That(eto.SomeDictionary["method2"], Is.InstanceOf(typeof(WebMethodAttribute)));
        Assert.That(((WebMethodAttribute) eto.SomeDictionary["method1"]).Description, Is.EqualTo("My First Web Method"));
        Assert.That(((WebMethodAttribute) eto.SomeDictionary["method2"]).Description, Is.EqualTo("My Second Web Method"));
        }

        [Test]
        public void TestExpressionElement()
        {
            Spring.Core.TypeResolution.TypeRegistry.RegisterType("WebMethod", typeof(WebMethodAttribute));

            IResource resource = new ReadOnlyXmlTestResource("expressions.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);

            ExpressionTestObject eto = (ExpressionTestObject)xof.GetObject("to2");
        Assert.That(eto.SomeString, Is.EqualTo(new DateTime(1974, 8, 24).ToString("m")));
        Assert.That(eto.SomeDate, Is.EqualTo(new DateTime(2004, 8, 14)));
        Assert.That(eto.ExpressionOne, Is.InstanceOf(typeof(Spring.Expressions.IExpression)));
        Assert.That(eto.ExpressionTwo, Is.InstanceOf(typeof(Spring.Expressions.IExpression)));
        Assert.That(eto.ExpressionOne.GetValue(), Is.EqualTo(DateTime.Today));
        Assert.That(eto.ExpressionTwo.GetValue(), Is.Empty);
        Assert.That(eto.SomeDictionary["method1"], Is.InstanceOf(typeof(WebMethodAttribute)));
        Assert.That(eto.SomeDictionary["method2"], Is.InstanceOf(typeof(WebMethodAttribute)));
        Assert.That(((WebMethodAttribute) eto.SomeDictionary["method1"]).Description, Is.EqualTo("My First Web Method"));
        Assert.That(((WebMethodAttribute) eto.SomeDictionary["method2"]).Description, Is.EqualTo("My Second Web Method"));
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
