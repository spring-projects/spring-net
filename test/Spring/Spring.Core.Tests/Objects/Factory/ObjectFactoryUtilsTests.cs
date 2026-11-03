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

using System.Collections;
using NUnit.Framework;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;

namespace Spring.Objects.Factory;

/// <summary>
/// Unit tests for the ObjectFactoryUtils class.
/// </summary>
/// <author>Rod Johnson</author>
/// <author>Simon White (.NET)</author>
/// <author>Rick Evans (.NET)</author>
[TestFixture]
public sealed class ObjectFactoryUtilsTests
{
    private IConfigurableListableObjectFactory _factory;

    [SetUp]
    public void SetUp()
    {
        IObjectFactory grandparent = new XmlObjectFactory(new ReadOnlyXmlTestResource("root.xml", GetType()));
        IObjectFactory parent = new XmlObjectFactory(new ReadOnlyXmlTestResource("middle.xml", GetType()), grandparent);
        IConfigurableListableObjectFactory child = new XmlObjectFactory(new ReadOnlyXmlTestResource("leaf.xml", GetType()), parent);
        _factory = child;
    }

    /// <summary>
    /// Check that override doesn't count as two separate objects.
    /// </summary>
    [Test]
    public void CountObjectsIncludingAncestors()
    {
        // leaf count...
        Assert.That(_factory.ObjectDefinitionCount, Is.EqualTo(1));
        // count minus duplicate...
        Assert.That(ObjectFactoryUtils.CountObjectsIncludingAncestors(_factory), Is.EqualTo(6),
            "Should count 6 objects, not " + ObjectFactoryUtils.CountObjectsIncludingAncestors(_factory));
    }

    [Test]
    public void ObjectNamesIncludingAncestors()
    {
        var names = ObjectFactoryUtils.ObjectNamesIncludingAncestors(_factory);
        Assert.That(names.Count, Is.EqualTo(6));
    }

    [Test]
    public void ObjectNamesForTypeIncludingAncestors()
    {
        var names = ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(_factory, typeof(ITestObject));
        // includes 2 TestObjects from IFactoryObjects (DummyFactory definitions)
        Assert.That(names.Count, Is.EqualTo(4));
        Assert.That(names.Contains("test"), Is.True);
        Assert.That(names.Contains("test3"), Is.True);
        Assert.That(names.Contains("testFactory1"), Is.True);
        Assert.That(names.Contains("testFactory2"), Is.True);
    }

    [Test]
    public void ObjectNamesForTypeIncludingAncestorsExcludesObjectsFromParentWhenLocalObjectDefined()
    {
        DefaultListableObjectFactory root = new DefaultListableObjectFactory();
        root.RegisterObjectDefinition("excludeLocalObject", new RootObjectDefinition(typeof(ArrayList)));
        DefaultListableObjectFactory child = new DefaultListableObjectFactory(root);
        child.RegisterObjectDefinition("excludeLocalObject", new RootObjectDefinition(typeof(Hashtable)));

        var names = ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(child, typeof(ArrayList));
        // "excludeLocalObject" matches on the parent, but not the local object definition
        Assert.That(names.Count, Is.EqualTo(0));

        names = ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(child, typeof(ArrayList), true, true);
        // "excludeLocalObject" matches on the parent, but not the local object definition
        Assert.That(names.Count, Is.EqualTo(0));
    }

    [Test]
    public void CountObjectsIncludingAncestorsWithNonHierarchicalFactory()
    {
        StaticListableObjectFactory lof = new StaticListableObjectFactory();
        lof.AddObject("t1", new TestObject());
        lof.AddObject("t2", new TestObject());
        Assert.That(ObjectFactoryUtils.CountObjectsIncludingAncestors(lof) == 2, Is.True);
    }

    [Test]
    public void HierarchicalResolutionWithOverride()
    {
        object test3 = _factory.GetObject("test3");
        object test = _factory.GetObject("test");
        object testFactory1 = _factory.GetObject("testFactory1");

        IDictionary<string, object> objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(_factory, typeof(ITestObject), true, false);
        Assert.That(objects.Count, Is.EqualTo(3));
        Assert.That(objects["test3"], Is.EqualTo(test3));
        Assert.That(objects["test"], Is.EqualTo(test));
        Assert.That(objects["testFactory1"], Is.EqualTo(testFactory1));
        objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(_factory, typeof(ITestObject), false, false);
        Assert.That(objects.Count, Is.EqualTo(2));
        Assert.That(objects["test"], Is.EqualTo(test));
        Assert.That(objects["testFactory1"], Is.EqualTo(testFactory1));
        objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(_factory, typeof(ITestObject), false, true);
        Assert.That(objects.Count, Is.EqualTo(2));
        Assert.That(objects["test"], Is.EqualTo(test));
        Assert.That(objects["testFactory1"], Is.EqualTo(testFactory1));
        objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(_factory, typeof(ITestObject), true, true);
        Assert.That(objects.Count, Is.EqualTo(4));
        Assert.That(objects["test3"], Is.EqualTo(test3));
        Assert.That(objects["test"], Is.EqualTo(test));
        Assert.That(objects["testFactory1"], Is.EqualTo(testFactory1));
        Assert.That(objects["testFactory2"] is ITestObject, Is.True);
        objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(_factory, typeof(DummyFactory), true, true);
        Assert.That(objects.Count, Is.EqualTo(2));
        Assert.That(objects["&testFactory1"], Is.EqualTo(_factory.GetObject("&testFactory1")));
        Assert.That(objects["&testFactory2"], Is.EqualTo(_factory.GetObject("&testFactory2")));
        objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(_factory, typeof(IFactoryObject), true, true);
        Assert.That(objects.Count, Is.EqualTo(2));
        Assert.That(objects["&testFactory1"], Is.EqualTo(_factory.GetObject("&testFactory1")));
        Assert.That(objects["&testFactory2"], Is.EqualTo(_factory.GetObject("&testFactory2")));
    }

    [Test]
    public void ObjectOfTypeIncludingAncestorsWithMoreThanOneObjectOfType()
    {
        Assert.Throws<NoSuchObjectDefinitionException>(
            () => ObjectFactoryUtils.ObjectOfTypeIncludingAncestors(_factory, typeof(ITestObject), true, true),
            "No unique object of type [Spring.Objects.ITestObject] is defined : Expected single object but found 4");
    }

    [Test]
    public void ObjectOfTypeIncludingAncestorsExcludesObjectsFromParentWhenLocalObjectDefined()
    {
        DefaultListableObjectFactory root = new DefaultListableObjectFactory();
        root.RegisterObjectDefinition("excludeLocalObject", new RootObjectDefinition(typeof(ArrayList)));
        DefaultListableObjectFactory child = new DefaultListableObjectFactory(root);
        child.RegisterObjectDefinition("excludeLocalObject", new RootObjectDefinition(typeof(Hashtable)));

        IDictionary<string, object> objectEntries = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(child, typeof(ArrayList), true, true);
        // "excludeLocalObject" matches on the parent, but not the local object definition
        Assert.That(objectEntries.Count, Is.EqualTo(0));
    }

    [Test]
    public void NoObjectsOfTypeIncludingAncestors()
    {
        StaticListableObjectFactory lof = new StaticListableObjectFactory();
        lof.AddObject("foo", new object());
        IDictionary<string, object> objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(lof, typeof(ITestObject), true, false);
        Assert.That(objects.Count == 0, Is.True);
    }

    [Test]
    public void ObjectsOfTypeIncludingAncestorsWithStaticFactory()
    {
        StaticListableObjectFactory lof = new StaticListableObjectFactory();
        TestObject t1 = new TestObject();
        TestObject t2 = new TestObject();
        DummyFactory t3 = new DummyFactory();
        DummyFactory t4 = new DummyFactory();
        t4.IsSingleton = false;
        lof.AddObject("t1", t1);
        lof.AddObject("t2", t2);
        lof.AddObject("t3", t3);
        t3.AfterPropertiesSet(); // StaticListableObjectFactory does support lifecycle calls.
        lof.AddObject("t4", t4);
        t4.AfterPropertiesSet(); // StaticListableObjectFactory does support lifecycle calls.
        IDictionary<string, object> objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(lof, typeof(ITestObject), true, false);
        Assert.That(objects.Count, Is.EqualTo(2));
        Assert.That(objects["t1"], Is.EqualTo(t1));
        Assert.That(objects["t2"], Is.EqualTo(t2));
        objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(lof, typeof(ITestObject), false, true);
        Assert.That(objects.Count, Is.EqualTo(3));
        Assert.That(objects["t1"], Is.EqualTo(t1));
        Assert.That(objects["t2"], Is.EqualTo(t2));
        Assert.That(objects["t3"], Is.EqualTo(t3.GetObject()));
        objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(lof, typeof(ITestObject), true, true);
        Assert.That(objects.Count, Is.EqualTo(4));
        Assert.That(objects["t1"], Is.EqualTo(t1));
        Assert.That(objects["t2"], Is.EqualTo(t2));
        Assert.That(objects["t3"], Is.EqualTo(t3.GetObject()));
        Assert.That(objects["t4"] is TestObject, Is.True);
    }

    [Test]
    public void IsFactoryDereferenceWithNonFactoryObjectName()
    {
        Assert.That(ObjectFactoryUtils.IsFactoryDereference("roob"),
            Is.False,
            "Name that didn't start with the factory object prefix is being reported " +
            "(incorrectly) as a factory object dereference.");
    }

    [Test]
    public void IsFactoryDereferenceWithNullName()
    {
        Assert.That(ObjectFactoryUtils.IsFactoryDereference(null),
            Is.False,
            "Null name that (obviously) didn't start with the factory object prefix is being reported " +
            "(incorrectly) as a factory object dereference.");
    }

    [Test]
    public void IsFactoryDereferenceWithEmptyName()
    {
        Assert.That(ObjectFactoryUtils.IsFactoryDereference(string.Empty),
            Is.False,
            "String.Empty name that (obviously) didn't start with the factory object prefix is being reported " +
            "(incorrectly) as a factory object dereference.");
    }

    [Test]
    public void IsFactoryDereferenceWithJustTheFactoryObjectPrefixCharacter()
    {
        Assert.That(ObjectFactoryUtils.IsFactoryDereference(
                ObjectFactoryUtils.FactoryObjectPrefix),
            Is.False,
            "Name that consisted solely of the factory object prefix is being reported " +
            "(incorrectly) as a factory object dereference.");
    }

    [Test]
    public void IsFactoryDereferenceSunnyDay()
    {
        Assert.That(ObjectFactoryUtils.IsFactoryDereference(
                ObjectFactoryUtils.FactoryObjectPrefix + "roob"),
            Is.True,
            "Name that did start with the factory object prefix is not being reported " +
            "(incorrectly) as a factory object dereference.");
    }
}
