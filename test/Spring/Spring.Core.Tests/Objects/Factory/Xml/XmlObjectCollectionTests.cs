#region License

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

#endregion

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Common.Logging;
using Common.Logging.Simple;
using NUnit.Framework;
using Spring.Collections;
using Spring.Core.IO;
using Spring.Expressions;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Util;

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Tests for collections in XML object definitions.
    /// </summary>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class XmlObjectCollectionTests
    {
        /// <summary>
        /// The setup logic executed before the execution of this test fixture.
        /// </summary>
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            // enable (null appender) logging, to ensure that the logging code is exercised...
            LogManager.Adapter = new NoOpLoggerFactoryAdapter(); 
            //XmlConfigurator.Configure();
        }

        [Test]
        public void CanApplyConstructorArgsToAbstractType()
        {
            IResource resource = new ReadOnlyXmlTestResource("ctor-args.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            TestObject rod = (TestObject)xof.GetObject("rod");
            Assert.AreEqual(1, rod.Age);

            RootObjectDefinition def = (RootObjectDefinition) xof.GetObjectDefinition("rod");
            ConstructorResolver resolver = new ConstructorResolver(xof, xof, new SimpleInstantiationStrategy(), 
                                                    new ObjectDefinitionValueResolver(xof));
            
            ConstructorInstantiationInfo ci = resolver.GetConstructorInstantiationInfo("rod", def, null, null);

            AbstractObjectDefinition objDef = (AbstractObjectDefinition)xof.GetObjectDefinition("foo");
            objDef.IsAbstract = false;

            TestObject foo = (TestObject) xof.GetObject("foo");


            Assert.AreEqual(2, foo.Age);
        }


        [Test]
        public void RefSubelementsBuildCollection()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            //Assert.IsTrue ("5 objects in reftypes, not " + xof.GetObjectDefinitionCount(), xof.GetObjectDefinitionCount() == 5);
            TestObject jen = (TestObject) xof.GetObject("jenny");
            TestObject dave = (TestObject) xof.GetObject("david");
            TestObject rod = (TestObject) xof.GetObject("rod");

            // Must be a list to support ordering
            // Our object doesn't modify the collection:
            // of course it could be a different copy in a real object
            IList friends = (IList) rod.Friends;
            Assert.IsTrue(friends.Count == 2);
            Assert.IsTrue(friends[0] == jen, "First friend must be jen, not " + friends[0]);
            Assert.IsTrue(friends[1] == dave);
            // Should be ordered
        }

        [Test]
        public void RefSubelementsBuildCollectionWithPrototypes()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);

            TestObject jen = (TestObject) xof.GetObject("pJenny");
            TestObject dave = (TestObject) xof.GetObject("pDavid");
            TestObject rod = (TestObject) xof.GetObject("pRod");
            IList friends = (IList) rod.Friends;
            Assert.IsTrue(friends.Count == 2);
            Assert.IsTrue(friends[0].ToString().Equals(jen.ToString()), "First friend must be jen, not " + friends[0]);
            Assert.IsTrue(friends[0] != jen, "Jen not same instance");
            Assert.IsTrue(friends[1].ToString().Equals(dave.ToString()));
            Assert.IsTrue(friends[1] != dave, "Dave not same instance");

            TestObject rod2 = (TestObject) xof.GetObject("pRod");
            IList friends2 = (IList) rod2.Friends;
            Assert.IsTrue(friends2.Count == 2);
            Assert.IsTrue(friends2[0].ToString().Equals(jen.ToString()), "First friend must be jen, not " + friends2[0]);
            Assert.IsTrue(friends2[0] != friends[0], "Jen not same instance");
            Assert.IsTrue(friends2[1].ToString().Equals(dave.ToString()));
            Assert.IsTrue(friends2[1] != friends[1], "Dave not same instance");
        }

        [Test]
        public void RefSubelementsBuildCollectionFromSingleElement()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            TestObject loner = (TestObject) xof.GetObject("loner");
            TestObject dave = (TestObject) xof.GetObject("david");
            Assert.IsTrue(loner.Friends.Count == 1);
            bool contains = false;
            foreach (object friend in loner.Friends)
            {
                if (friend == dave)
                {
                    contains = true;
                    break;
                }
            }
            Assert.IsTrue(contains);
        }

        [Test]
        public void BuildCollectionFromMixtureOfReferencesAndValues()
        {
            MixedCollectionObject.ResetStaticState();
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            MixedCollectionObject jumble = (MixedCollectionObject) xof.GetObject("jumble");
            Assert.AreEqual(1, MixedCollectionObject.nrOfInstances);
            xof.GetObject("david", typeof (TestObject));
            Assert.IsTrue(jumble.Jumble.Count == 4, "Expected 3 elements, not " + jumble.Jumble.Count);
            IList l = (IList) jumble.Jumble;
            Assert.IsTrue(l[0].Equals(xof.GetObject("david")));
            Assert.IsTrue(l[1].Equals("literal"));
            Assert.IsTrue(l[2].Equals(xof.GetObject("jenny")));
            Assert.IsTrue(l[3].Equals("rod"));
        }

        /// <summary>
        /// Test to see if a type safe custom collection class can be set using normal
        /// list syntax.  
        /// </summary>
        [Test]
        public void CustomListCollection()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            TestObject objectWithTypedFriends = (TestObject) xof.GetObject("objectWithTypedFriends");
            TestObjectList tol = objectWithTypedFriends.TypedFriends;
            Assert.AreEqual(1, tol.Count);
            Assert.IsTrue(tol[0].Equals(xof.GetObject("david")));
        }

        /// <summary>
        /// Test to see if we can set values on a collection class that uses indexers
        /// </summary>
        [Test]
        public void ObjectWithIndexerProperty()
        {          
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            TestObject testObject = (TestObject) xof.GetObject("objectWithIndexer");
            Assert.IsNotNull(testObject);
            Assert.AreEqual("my string value", testObject[0]);
            
        }
        /// <summary>
        /// Test that properties with name as well as id creating an alias up front.
        /// </summary>
        [Test]
        public void AutoAliasing()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            var objectNames = xof.GetObjectDefinitionNames();
            TestObject tb1 = (TestObject) xof.GetObject("aliased");
            TestObject alias1 = (TestObject) xof.GetObject("myalias");
            Assert.IsTrue(tb1 == alias1);

            var tb1Aliases = xof.GetAliases("aliased");
            Assert.AreEqual(1, tb1Aliases.Count);
            Assert.IsTrue(tb1Aliases.Contains("myalias"));
            Assert.IsTrue(objectNames.Contains("aliased"));
            Assert.IsFalse(objectNames.Contains("myalias"));

            TestObject tb2 = (TestObject) xof.GetObject("multiAliased");
            TestObject alias2 = (TestObject) xof.GetObject("alias1");
            TestObject alias3 = (TestObject) xof.GetObject("alias2");
            Assert.IsTrue(tb2 == alias2);
            Assert.IsTrue(tb2 == alias3);
            var tb2Aliases = xof.GetAliases("multiAliased");
            Assert.AreEqual(2, tb2Aliases.Count);
            Assert.IsTrue(tb2Aliases.Contains("alias1"));
            Assert.IsTrue(tb2Aliases.Contains("alias2"));
            Assert.IsTrue(objectNames.Contains("multiAliased"));
            Assert.IsFalse(objectNames.Contains("alias1"));
            Assert.IsFalse(objectNames.Contains("alias2"));

            TestObject tb3 = (TestObject) xof.GetObject("aliasWithoutId1");
            TestObject alias4 = (TestObject) xof.GetObject("aliasWithoutId2");
            TestObject alias5 = (TestObject) xof.GetObject("aliasWithoutId3");
            Assert.IsTrue(tb3 == alias4);
            Assert.IsTrue(tb3 == alias5);

            var tb3Aliases = xof.GetAliases("aliasWithoutId1");
            Assert.AreEqual(2, tb2Aliases.Count);
            Assert.IsTrue(tb3Aliases.Contains("aliasWithoutId2"));
            Assert.IsTrue(tb3Aliases.Contains("aliasWithoutId3"));
            Assert.IsTrue(objectNames.Contains("aliasWithoutId1"));
            Assert.IsFalse(objectNames.Contains("aliasWithoutId2"));
            Assert.IsFalse(objectNames.Contains("aliasWithoutId3"));

            string className = typeof(TestObject).FullName;
            string targetName = className + ObjectDefinitionReaderUtils.GENERATED_OBJECT_NAME_SEPARATOR + "0";

            TestObject tb4 = (TestObject)xof.GetObject(targetName);
            Assert.AreEqual(null, tb4.Name);
        }

        /// <summary>
        /// Test that we can add elements to an IList that is exposed as a 
        /// read only property.  Many classes in the BCL only expose getters to
        /// modify the content of collections.
        /// </summary>
        [Test]
        public void AddElementsToReadOnlyList()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            //Stream stream = new FileSystemResource("file:///w:/collections.xml").InputStream;
            XmlObjectFactory xof = new XmlObjectFactory(resource);

            TestObject aleks = (TestObject) xof.GetObject("aleks");
            IList pets = (IList) aleks.Pets;
            Assert.IsTrue(pets.Count == 1);
            Assert.IsTrue(pets[0].Equals("Nadja"));
        }

        /// <summary>
        /// Test that we can add elements to an IDictionary that is exposed as a
        /// read-only property.
        /// </summary>
        public void AddElementsToReadOnlyDictionary()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);

            TestObject to = (TestObject) xof.GetObject("readOnlyDictionary");
            IDictionary table = to.PeriodicTable;
            Assert.IsTrue(table.Count == 2);
            Assert.AreEqual("1", table["hydrogen"], "Dictionary Value incorrect");
            Assert.AreEqual("12", table["carbon"], "Dictionary value incorrect");
        }

        /// <summary>
        /// Test that we can add elements to an ISet that is exposed as a 
        /// read-only property
        /// </summary>
        public void AddElementsToReadOnlySet()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);

            TestObject to = (TestObject) xof.GetObject("readOnlySet");
            ISet computers = to.Computers;
            Assert.IsTrue(computers.Count == 2);
            bool foundDell = false;
            bool foundIbm = false;
            foreach (string name in computers)
            {
                if (name.Equals("Dell"))
                {
                    foundDell = true;
                }
                if (name.Equals("IBM T41"))
                {
                    foundIbm = true;
                }
            }
            Assert.IsTrue(foundDell, "Dell string not found in ISet");
            Assert.IsTrue(foundIbm, "IBM T41 not found in ISet");
        }

        [Test]
        public void EmptyMap()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            HasMap hasMap = (HasMap) xof.GetObject("emptyMap");
            Assert.IsTrue(hasMap.Map.Count == 0);
        }

        [Test]
        public void MapWithLiteralsOnly()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            HasMap hasMap = (HasMap) xof.GetObject("literalMap");
            Assert.IsTrue(hasMap.Map.Count == 3);
            Assert.IsTrue(hasMap.Map["foo"].Equals("bar"));
            Assert.IsTrue(hasMap.Map["fi"].Equals("fum"));
            Assert.IsTrue(hasMap.Map["fa"] == null);
        }

        [Test]
        public void MapWithLiteralsAndReferences()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            HasMap hasMap = (HasMap) xof.GetObject("mixedMap");
            Assert.IsTrue(hasMap.Map.Count == 3);
            Assert.IsTrue(hasMap.Map["foo"].Equals("bar"));
            TestObject jenny = (TestObject) xof.GetObject("jenny");
            Assert.IsTrue(hasMap.Map["jenny"] == jenny);
            Assert.IsTrue(hasMap.Map["david"].Equals("david"));
        }

        [Test]
        public void MapWithLiteralsAndPrototypeReferences()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);

            TestObject jenny = (TestObject) xof.GetObject("pJenny");
            HasMap hasMap = (HasMap) xof.GetObject("pMixedMap");
            Assert.IsTrue(hasMap.Map.Count == 2);
            Assert.IsTrue(hasMap.Map["foo"].Equals("bar"));
            Assert.IsTrue(hasMap.Map["jenny"].ToString().Equals(jenny.ToString()));
            Assert.IsTrue(hasMap.Map["jenny"] != jenny, "Not same instance");

            HasMap hasMap2 = (HasMap) xof.GetObject("pMixedMap");
            Assert.IsTrue(hasMap2.Map.Count == 2);
            Assert.IsTrue(hasMap2.Map["foo"].Equals("bar"));
            Assert.IsTrue(hasMap2.Map["jenny"].ToString().Equals(jenny.ToString()));
            Assert.IsTrue(hasMap2.Map["jenny"] != hasMap.Map["jenny"], "Not same instance");
        }

        [Test]
        public void MapWithLiteralsReferencesAndList()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            HasMap hasMap = (HasMap) xof.GetObject("mixedMapWithList");
            Assert.IsTrue(hasMap.Map.Count == 4);
            Assert.IsTrue(hasMap.Map["foo"].Equals("bar"));
            TestObject jenny = (TestObject) xof.GetObject("jenny");
            Assert.IsTrue(hasMap.Map["jenny"].Equals(jenny));

            // Check list
            IList l = (IList) hasMap.Map["list"];
            Assert.IsNotNull(l);
            Assert.IsTrue(l.Count == 4);
            Assert.IsTrue(l[0].Equals("zero"));
            Assert.IsTrue(l[3] == null);

            // Check nested map in list
            IDictionary m = (IDictionary) l[1];
            Assert.IsNotNull(m);
            Assert.IsTrue(m.Count == 2);
            Assert.IsTrue(m["fo"].Equals("bar"));
            Assert.IsTrue(m["jen"].Equals(jenny), "Map element 'jenny' should be equal to jenny object, not " + m["jen"]);

            // Check nested list in list
            l = (IList) l[2];
            Assert.IsNotNull(l);
            Assert.IsTrue(l.Count == 2);
            Assert.IsTrue(l[0].Equals(jenny));
            Assert.IsTrue(l[1].Equals("ba"));

            // Check nested map
            m = (IDictionary) hasMap.Map["map"];
            Assert.IsNotNull(m);
            Assert.IsTrue(m.Count == 2);
            Assert.IsTrue(m["foo"].Equals("bar"));
            Assert.IsTrue(m["jenny"].Equals(jenny),
                "Map element 'jenny' should be equal to jenny object, not " + m["jenny"]);
        }

        [Test]
        public void EmptySet()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            HasMap hasMap = (HasMap) xof.GetObject("emptySet");
            Assert.IsTrue(hasMap.Set.Count == 0);
        }

        [Test]
        public void PopulatedSet()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            HasMap hasMap = (HasMap) xof.GetObject("set");
            Assert.IsTrue(hasMap.Set.Count == 3);
            Assert.IsTrue(hasMap.Set.Contains("bar"));
            TestObject jenny = (TestObject) xof.GetObject("jenny");
            Assert.IsTrue(hasMap.Set.Contains(jenny));
            Assert.IsTrue(hasMap.Set.Contains(null));
        }

        [Test]
        public void EmptyProps()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            HasMap hasMap = (HasMap) xof.GetObject("emptyProps");
            Assert.IsTrue(hasMap.Props.Count == 0);
        }

        /// <summary>
        /// Test that an empty string value can be placed in a name-value collection
        /// </summary>
        [Test]
        public void EmptyValue()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            HasMap hasMap = (HasMap) xof.GetObject("emptyValue");
            Assert.AreEqual("", hasMap.Props.Get("foo"), "Expected empty string");
        }

        [Test]
        public void PopulatedProps()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            HasMap hasMap = (HasMap) xof.GetObject("props");
            Assert.AreEqual(2, hasMap.Props.Count);
            Assert.AreEqual("bar", hasMap.Props["foo"]);
            Assert.AreEqual("TWO", hasMap.Props["2"]);
        }

        [Test]
        public void PopulatedPropsWithSameKey()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            HasMap hasMap = (HasMap) xof.GetObject("propsWithSameKey");
            Assert.AreEqual(2, hasMap.Props.Count);
            Assert.AreEqual("OnE,tWo", hasMap.Props["foo"]);
            Assert.AreEqual(2, hasMap.Props.GetValues("bar").Length);
            Assert.AreEqual("OnE", hasMap.Props.GetValues("bar")[0]);
            Assert.AreEqual("tWo", hasMap.Props.GetValues("bar")[1]);
        }

        [Test]
        public void DelimitedProps()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            HasMap hasMap = (HasMap)xof.GetObject("delimitedProps");
            Assert.AreEqual(2, hasMap.Props.Count);
            Assert.AreEqual(3, hasMap.Props.GetValues("foo").Length);
            Assert.AreEqual(7, hasMap.Props.GetValues("days").Length);
            Assert.AreEqual("wednesday", hasMap.Props.GetValues("days")[2]);
        }
        
        [Test]
        public void ObjectArray()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            HasMap hasMap = (HasMap) xof.GetObject("objectArray");
            Assert.IsTrue(hasMap.ObjectArray.Length == 2);
            Assert.IsTrue(hasMap.ObjectArray[0].Equals("one"));
            Assert.IsTrue(hasMap.ObjectArray[1].Equals(xof.GetObject("jenny")));
        }

        [Test]
        public void ClassArray()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            HasMap hasMap = (HasMap) xof.GetObject("classArray");
            Assert.IsTrue(hasMap.ClassArray.Length == 2);
            Assert.IsTrue(hasMap.ClassArray[0].Equals(typeof (String)));
            Assert.IsTrue(hasMap.ClassArray[1].Equals(typeof (Exception)));
        }

        [Test]
        public void GetDictionaryThatUsesEntryValueShortcut()
        {
            XmlObjectFactory xof = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("collections.xml", GetType()));
            IDictionary map = (IDictionary) xof.GetObject("mapWithEntryValueShortcut");
            Assert.AreEqual(2, map.Count);
            string v1 = (string) map["rob"];
            Assert.AreEqual("robert", v1);
            string v2 = (string) map["jen"];
            Assert.AreEqual("jenny", v2);
        }

        [Test]
        public void GetDictionaryThatUsesStringKeysSpecifiedAsElements()
        {
            XmlObjectFactory xof = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("collections.xml", GetType()));
            IDictionary map = (IDictionary) xof.GetObject("mapWithStringKeysSpecifiedAsElements");
            Assert.AreEqual(2, map.Count);
            string v1 = (string) map["rick"];
            Assert.AreEqual("Rick Evans", v1);
            string v2 = (string) map["uncleelvis"];
            Assert.AreEqual("Elvis Orten", v2);
        }

        [Test]
        public void GetDictionaryThatDoesntSpecifyAnyKeyForAnEntry()
        {
            Assert.Throws<ObjectDefinitionStoreException>(() => new StreamHelperDecorator(new StreamHelperCallback(_GetDictionaryThatDoesntSpecifyAnyKeyForAnEntry)).Run());
        }

        private void _GetDictionaryThatDoesntSpecifyAnyKeyForAnEntry(out Stream stream)
        {
            const string xml =
                      @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
	<object id='mapWithNoKeyForEntry' type='Spring.Objects.Factory.Config.DictionaryFactoryObject, Spring.Core'>
		<property name='SourceDictionary'>
			<dictionary>
				<entry>
					<value>Rick Evans</value>
				</entry>
			</dictionary>
		</property>
	</object>
</objects>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            new XmlObjectFactory(new InputStreamResource(stream, ""));
            Assert.Fail("Must have failed to parse <entry/> element with no key.");
        }

        [Test]
        public void GetDictionaryWithKeyRefAttributeShortcuts()
        {
            XmlObjectFactory xof = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("collections.xml", GetType()));
            IDictionary map = (IDictionary) xof.GetObject("mapWithKeyRefAttributeShortcuts");
            Assert.AreEqual(2, map.Count);
            string v1 = (string) map[new TestObject("Rick Evans", 30)];
            Assert.AreEqual("Rick Evans", v1);
            string v2 = (string) map[new TestObject("Uncle Elvis", 47)];
            Assert.AreEqual("Elvis Orten", v2);
        }

        [Test]
        public void GetDictionaryWithValueRefAttributeShortcuts()
        {
            XmlObjectFactory xof = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("collections.xml", GetType()));
            IDictionary map = (IDictionary) xof.GetObject("mapWithValueRefAttributeShortcuts");
            Assert.AreEqual(2, map.Count);
            foreach(string key in map.Keys)
            {
                object obj = map[key];
                Assert.IsNotNull(obj);
                Assert.AreEqual(typeof(TestObject), obj.GetType(), "Wrong value assigned to value of dictionary.");
                TestObject tob = (TestObject) obj;
                if(key == "rick")
                {
                    Assert.AreEqual("Rick Evans", tob.Name, "Wrong object value assigned to the 'rick' key.");
                }
                else if(key == "uncleelvis")
                {
                    Assert.AreEqual("Uncle Elvis", tob.Name, "Wrong object value assigned to the 'uncleelvis' key.");
                }
            }
        }

        [Test]
        public void CollectionFactoryDefaults()
        {
            ListFactoryObject listFactory = new ListFactoryObject();
            listFactory.SourceList = new ArrayList();
            listFactory.AfterPropertiesSet();
            Assert.IsTrue(listFactory.GetObject() is ArrayList);

            SetFactoryObject setFactory = new SetFactoryObject();
            setFactory.SourceSet = new HybridSet();
            setFactory.AfterPropertiesSet();
            Assert.IsTrue(setFactory.GetObject() is HybridSet);

            DictionaryFactoryObject dictionaryFactory = new DictionaryFactoryObject();
            dictionaryFactory.SourceDictionary = new Hashtable();
            dictionaryFactory.AfterPropertiesSet();
            Assert.IsTrue(dictionaryFactory.GetObject() is Hashtable);
        }

        [Test]
        public void ListFactory()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            IList list = (IList) xof.GetObject("listFactory");
            Assert.IsTrue(list is LinkedList);
            Assert.IsTrue(list.Count == 2);
            Assert.AreEqual("bar", list[0]);
            Assert.AreEqual("jenny", list[1]);
        }

        [Test]
        public void PrototypeListFactory()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            IList list = (IList) xof.GetObject("pListFactory");
            Assert.IsTrue(list is LinkedList);
            Assert.IsTrue(list.Count == 2);
            Assert.AreEqual("bar", list[0]);
            Assert.AreEqual("jenny", list[1]);
        }

        [Test]
        public void SetFactory()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            Set mySet = (Set) xof.GetObject("setFactory");
            Assert.IsTrue(mySet is HybridSet);
            Assert.IsTrue(mySet.Count == 2);
            Assert.IsTrue(mySet.Contains("bar"));
            Assert.IsTrue(mySet.Contains("jenny"));
        }

        [Test]
        public void PrototypeSetFactory()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            Set mySet = (Set) xof.GetObject("pSetFactory");
            Assert.IsTrue(mySet is HybridSet);
            Assert.IsTrue(mySet.Count == 2);
            Assert.IsTrue(mySet.Contains("bar"));
            Assert.IsTrue(mySet.Contains("jenny"));
        }

        [Test]
        public void DictionaryFactory()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            IDictionary map = (IDictionary) xof.GetObject("mapFactory");
            Assert.IsTrue(map.Count == 2);
            Assert.AreEqual("bar", map["foo"]);
            Assert.AreEqual("jenny", map["jen"]);
        }

        [Test]
        public void PrototypeDictionaryFactory()
        {
            IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
            XmlObjectFactory xof = new XmlObjectFactory(resource);
            IDictionary map = (IDictionary) xof.GetObject("pMapFactory");
            Assert.IsTrue(map.Count == 2);
            Assert.AreEqual("bar", map["foo"]);
            Assert.AreEqual("jenny", map["jen"]);
        }

        [Test]
        public void GetDictionaryThatUsesNonStringKeys()
        {
            XmlObjectFactory xof = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("collections.xml", GetType()));
            IDictionary map = (IDictionary) xof.GetObject("mapWithNonStringKeys");
            Assert.AreEqual(2, map.Count);
            string v1 = (string) map[new TestObject("Rick Evans", 30)];
            Assert.AreEqual("Rick Evans", v1);
            string v2 = (string) map[new TestObject("Uncle Elvis", 47)];
            Assert.AreEqual("Elvis Orten", v2);
        }
        
        [Test]
        public void TypedNonGenericList()
        {
            XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("collections.xml", GetType()));
            NonGenericExpressionHolder obj = (NonGenericExpressionHolder) xof.GetObject("nonGenericExpressionHolder");
            Assert.AreEqual(2, obj[0].GetValue());
            Assert.AreEqual(8, obj[1].GetValue());
            Assert.AreEqual("ALEKSANDAR SEOVIC", obj[2].GetValue());
            Assert.IsTrue((bool) obj[3].GetValue());
        }

        [Test]
        public void TypedNonGenericDictionary()
        {
            XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("collections.xml", GetType()));
            NonGenericExpressionHolder obj = (NonGenericExpressionHolder)xof.GetObject("nonGenericExpressionHolder");
            Assert.AreEqual(2, obj["0"].GetValue());
            Assert.AreEqual(8, obj["1"].GetValue());
            Assert.AreEqual("ALEKSANDAR SEOVIC", obj["2"].GetValue());
            Assert.IsTrue((bool)obj["3"].GetValue());
        }

        [Test]
        public void TypedGenericList()
        {
            XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("collections.xml", GetType()));
            GenericExpressionHolder obj = (GenericExpressionHolder)xof.GetObject("genericExpressionHolder");
            Assert.AreEqual(2, obj[0].GetValue());
            Assert.AreEqual(8, obj[1].GetValue());
            Assert.AreEqual("ALEKSANDAR SEOVIC", obj[2].GetValue());
            Assert.IsTrue((bool)obj[3].GetValue());
        }

        [Test]
        public void TypedGenericDictionary()
        {
            XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("collections.xml", GetType()));
            GenericExpressionHolder obj = (GenericExpressionHolder)xof.GetObject("genericExpressionHolder");
            Assert.AreEqual(2, obj["0"].GetValue());
            Assert.AreEqual(8, obj["1"].GetValue());
            Assert.AreEqual("ALEKSANDAR SEOVIC", obj["2"].GetValue());
            Assert.IsTrue((bool)obj["3"].GetValue());
        }
    }

    #region Helper classes
    
    public class NonGenericExpressionHolder
    {
        private IList expressionsList;
        private IDictionary expressionsDictionary;

        public IList ExpressionsList
        {
            set { this.expressionsList = value; }
        }

        public IDictionary ExpressionsDictionary
        {
            set { this.expressionsDictionary = value; }
        }

        public IExpression this[int index]
        {
            get { return (IExpression)this.expressionsList[index]; }
        }

        public IExpression this[string key]
        {
            get { return (IExpression)this.expressionsDictionary[key]; }
        }
    }

    public class GenericExpressionHolder
    {
        private System.Collections.Generic.IList<IExpression> expressionsList;
        private System.Collections.Generic.IDictionary<string, IExpression> expressionsDictionary;

        public System.Collections.Generic.IList<IExpression> ExpressionsList
        {
            set { this.expressionsList = value; }
        }

        public System.Collections.Generic.IDictionary<string,IExpression> ExpressionsDictionary
        {
            set { this.expressionsDictionary = value; }
        }

        public IExpression this[int index]
        {
            get { return this.expressionsList[index]; }
        }

        public IExpression this[string key]
        {
            get { return this.expressionsDictionary[key]; }
        }
    }
    
    #endregion
}
