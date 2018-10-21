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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Objects.Factory
{
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
            Assert.AreEqual(1, _factory.ObjectDefinitionCount);
            // count minus duplicate...
            Assert.AreEqual(6, ObjectFactoryUtils.CountObjectsIncludingAncestors(_factory),
                            "Should count 6 objects, not " + ObjectFactoryUtils.CountObjectsIncludingAncestors(_factory));
        }

        [Test]
        public void ObjectNamesIncludingAncestors()
        {
            var names = ObjectFactoryUtils.ObjectNamesIncludingAncestors(_factory);
            Assert.AreEqual(6, names.Count);
        }

        [Test]
        public void ObjectNamesForTypeIncludingAncestors()
        {
            var names = ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(_factory, typeof(ITestObject));
            // includes 2 TestObjects from IFactoryObjects (DummyFactory definitions)
            Assert.AreEqual(4, names.Count);
            Assert.IsTrue(names.Contains("test"));
            Assert.IsTrue(names.Contains("test3"));
            Assert.IsTrue(names.Contains("testFactory1"));
            Assert.IsTrue(names.Contains("testFactory2"));
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
            Assert.AreEqual(0, names.Count);

            names = ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(child, typeof(ArrayList), true, true);
            // "excludeLocalObject" matches on the parent, but not the local object definition
            Assert.AreEqual(0, names.Count);
        }

        [Test]
        public void CountObjectsIncludingAncestorsWithNonHierarchicalFactory()
        {
            StaticListableObjectFactory lof = new StaticListableObjectFactory();
            lof.AddObject("t1", new TestObject());
            lof.AddObject("t2", new TestObject());
            Assert.IsTrue(ObjectFactoryUtils.CountObjectsIncludingAncestors(lof) == 2);
        }

        [Test]
        public void HierarchicalResolutionWithOverride()
        {
            object test3 = _factory.GetObject("test3");
            object test = _factory.GetObject("test");
            object testFactory1 = _factory.GetObject("testFactory1");

            IDictionary<string, object> objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(_factory, typeof(ITestObject), true, false);
            Assert.AreEqual(3, objects.Count);
            Assert.AreEqual(test3, objects["test3"]);
            Assert.AreEqual(test, objects["test"]);
            Assert.AreEqual(testFactory1, objects["testFactory1"]);
            objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(_factory, typeof(ITestObject), false, false);
            Assert.AreEqual(2, objects.Count);
            Assert.AreEqual(test, objects["test"]);
            Assert.AreEqual(testFactory1, objects["testFactory1"]);
            objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(_factory, typeof(ITestObject), false, true);
            Assert.AreEqual(2, objects.Count);
            Assert.AreEqual(test, objects["test"]);
            Assert.AreEqual(testFactory1, objects["testFactory1"]);
            objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(_factory, typeof(ITestObject), true, true);
            Assert.AreEqual(4, objects.Count);
            Assert.AreEqual(test3, objects["test3"]);
            Assert.AreEqual(test, objects["test"]);
            Assert.AreEqual(testFactory1, objects["testFactory1"]);
            Assert.IsTrue(objects["testFactory2"] is ITestObject);
            objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(_factory, typeof(DummyFactory), true, true);
            Assert.AreEqual(2, objects.Count);
            Assert.AreEqual(_factory.GetObject("&testFactory1"), objects["&testFactory1"]);
            Assert.AreEqual(_factory.GetObject("&testFactory2"), objects["&testFactory2"]);
            objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(_factory, typeof(IFactoryObject), true, true);
            Assert.AreEqual(2, objects.Count);
            Assert.AreEqual(_factory.GetObject("&testFactory1"), objects["&testFactory1"]);
            Assert.AreEqual(_factory.GetObject("&testFactory2"), objects["&testFactory2"]);
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
            Assert.AreEqual(0, objectEntries.Count);
        }

        [Test]
        public void NoObjectsOfTypeIncludingAncestors()
        {
            StaticListableObjectFactory lof = new StaticListableObjectFactory();
            lof.AddObject("foo", new object());
            IDictionary<string, object> objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(lof, typeof(ITestObject), true, false);
            Assert.IsTrue(objects.Count == 0);
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
            Assert.AreEqual(2, objects.Count);
            Assert.AreEqual(t1, objects["t1"]);
            Assert.AreEqual(t2, objects["t2"]);
            objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(lof, typeof(ITestObject), false, true);
            Assert.AreEqual(3, objects.Count);
            Assert.AreEqual(t1, objects["t1"]);
            Assert.AreEqual(t2, objects["t2"]);
            Assert.AreEqual(t3.GetObject(), objects["t3"]);
            objects = ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(lof, typeof(ITestObject), true, true);
            Assert.AreEqual(4, objects.Count);
            Assert.AreEqual(t1, objects["t1"]);
            Assert.AreEqual(t2, objects["t2"]);
            Assert.AreEqual(t3.GetObject(), objects["t3"]);
            Assert.IsTrue(objects["t4"] is TestObject);
        }

        [Test]
        public void IsFactoryDereferenceWithNonFactoryObjectName()
        {
            Assert.IsFalse(ObjectFactoryUtils.IsFactoryDereference("roob"),
                "Name that didn't start with the factory object prefix is being reported " +
                "(incorrectly) as a factory object dereference.");
        }

        [Test]
        public void IsFactoryDereferenceWithNullName()
        {
            Assert.IsFalse(ObjectFactoryUtils.IsFactoryDereference(null),
                "Null name that (obviously) didn't start with the factory object prefix is being reported " +
                "(incorrectly) as a factory object dereference.");
        }

        [Test]
        public void IsFactoryDereferenceWithEmptyName()
        {
            Assert.IsFalse(ObjectFactoryUtils.IsFactoryDereference(string.Empty),
                "String.Empty name that (obviously) didn't start with the factory object prefix is being reported " +
                "(incorrectly) as a factory object dereference.");
        }

        [Test]
        public void IsFactoryDereferenceWithJustTheFactoryObjectPrefixCharacter()
        {
            Assert.IsFalse(ObjectFactoryUtils.IsFactoryDereference(
                ObjectFactoryUtils.FactoryObjectPrefix),
                "Name that consisted solely of the factory object prefix is being reported " +
                "(incorrectly) as a factory object dereference.");
        }

        [Test]
        public void IsFactoryDereferenceSunnyDay()
        {
            Assert.IsTrue(ObjectFactoryUtils.IsFactoryDereference(
                ObjectFactoryUtils.FactoryObjectPrefix + "roob"),
                "Name that did start with the factory object prefix is not being reported " +
                "(incorrectly) as a factory object dereference.");
        }
    }
}