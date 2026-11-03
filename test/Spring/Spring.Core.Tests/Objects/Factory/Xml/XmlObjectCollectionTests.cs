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
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Spring.Collections;
using Spring.Core.IO;
using Spring.Expressions;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Util;

namespace Spring.Objects.Factory.Xml;

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
        LogManager.LoggerFactory = NullLoggerFactory.Instance;
        //XmlConfigurator.Configure();
    }

    [Test]
    public void CanApplyConstructorArgsToAbstractType()
    {
        IResource resource = new ReadOnlyXmlTestResource("ctor-args.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        TestObject rod = (TestObject) xof.GetObject("rod");
        Assert.That(rod.Age, Is.EqualTo(1));

        RootObjectDefinition def = (RootObjectDefinition) xof.GetObjectDefinition("rod");
        ConstructorResolver resolver = new ConstructorResolver(xof, xof, new SimpleInstantiationStrategy(),
            new ObjectDefinitionValueResolver(xof));

        ConstructorInstantiationInfo ci = resolver.GetConstructorInstantiationInfo("rod", def, null, null);

        AbstractObjectDefinition objDef = (AbstractObjectDefinition) xof.GetObjectDefinition("foo");
        objDef.IsAbstract = false;

        TestObject foo = (TestObject) xof.GetObject("foo");

        Assert.That(foo.Age, Is.EqualTo(2));
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
        Assert.That(friends.Count == 2, Is.True);
        Assert.That(friends[0] == jen, Is.True, "First friend must be jen, not " + friends[0]);
        Assert.That(friends[1] == dave, Is.True);
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
        Assert.That(friends.Count == 2, Is.True);
        Assert.That(friends[0].ToString().Equals(jen.ToString()), Is.True, "First friend must be jen, not " + friends[0]);
        Assert.That(friends[0] != jen, Is.True, "Jen not same instance");
        Assert.That(friends[1].ToString().Equals(dave.ToString()), Is.True);
        Assert.That(friends[1] != dave, Is.True, "Dave not same instance");

        TestObject rod2 = (TestObject) xof.GetObject("pRod");
        IList friends2 = (IList) rod2.Friends;
        Assert.That(friends2.Count == 2, Is.True);
        Assert.That(friends2[0].ToString().Equals(jen.ToString()), Is.True, "First friend must be jen, not " + friends2[0]);
        Assert.That(friends2[0] != friends[0], Is.True, "Jen not same instance");
        Assert.That(friends2[1].ToString().Equals(dave.ToString()), Is.True);
        Assert.That(friends2[1] != friends[1], Is.True, "Dave not same instance");
    }

    [Test]
    public void RefSubelementsBuildCollectionFromSingleElement()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        TestObject loner = (TestObject) xof.GetObject("loner");
        TestObject dave = (TestObject) xof.GetObject("david");
        Assert.That(loner.Friends.Count == 1, Is.True);
        bool contains = false;
        foreach (object friend in loner.Friends)
        {
            if (friend == dave)
            {
                contains = true;
                break;
            }
        }

        Assert.That(contains, Is.True);
    }

    [Test]
    public void BuildCollectionFromMixtureOfReferencesAndValues()
    {
        MixedCollectionObject.ResetStaticState();
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        MixedCollectionObject jumble = (MixedCollectionObject) xof.GetObject("jumble");
        Assert.That(MixedCollectionObject.nrOfInstances, Is.EqualTo(1));
        xof.GetObject("david", typeof(TestObject));
        Assert.That(jumble.Jumble.Count == 4, Is.True, "Expected 3 elements, not " + jumble.Jumble.Count);
        IList l = (IList) jumble.Jumble;
        Assert.That(l[0].Equals(xof.GetObject("david")), Is.True);
        Assert.That(l[1].Equals("literal"), Is.True);
        Assert.That(l[2].Equals(xof.GetObject("jenny")), Is.True);
        Assert.That(l[3].Equals("rod"), Is.True);
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
        Assert.That(tol.Count, Is.EqualTo(1));
        Assert.That(tol[0].Equals(xof.GetObject("david")), Is.True);
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
        Assert.That(testObject, Is.Not.Null);
        Assert.That(testObject[0], Is.EqualTo("my string value"));
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
        Assert.That(tb1 == alias1, Is.True);

        var tb1Aliases = xof.GetAliases("aliased");
        Assert.That(tb1Aliases.Count, Is.EqualTo(1));
        Assert.That(tb1Aliases.Contains("myalias"), Is.True);
        Assert.That(objectNames.Contains("aliased"), Is.True);
        Assert.That(objectNames.Contains("myalias"), Is.False);

        TestObject tb2 = (TestObject) xof.GetObject("multiAliased");
        TestObject alias2 = (TestObject) xof.GetObject("alias1");
        TestObject alias3 = (TestObject) xof.GetObject("alias2");
        Assert.That(tb2 == alias2, Is.True);
        Assert.That(tb2 == alias3, Is.True);
        var tb2Aliases = xof.GetAliases("multiAliased");
        Assert.That(tb2Aliases.Count, Is.EqualTo(2));
        Assert.That(tb2Aliases.Contains("alias1"), Is.True);
        Assert.That(tb2Aliases.Contains("alias2"), Is.True);
        Assert.That(objectNames.Contains("multiAliased"), Is.True);
        Assert.That(objectNames.Contains("alias1"), Is.False);
        Assert.That(objectNames.Contains("alias2"), Is.False);

        TestObject tb3 = (TestObject) xof.GetObject("aliasWithoutId1");
        TestObject alias4 = (TestObject) xof.GetObject("aliasWithoutId2");
        TestObject alias5 = (TestObject) xof.GetObject("aliasWithoutId3");
        Assert.That(tb3 == alias4, Is.True);
        Assert.That(tb3 == alias5, Is.True);

        var tb3Aliases = xof.GetAliases("aliasWithoutId1");
        Assert.That(tb2Aliases.Count, Is.EqualTo(2));
        Assert.That(tb3Aliases.Contains("aliasWithoutId2"), Is.True);
        Assert.That(tb3Aliases.Contains("aliasWithoutId3"), Is.True);
        Assert.That(objectNames.Contains("aliasWithoutId1"), Is.True);
        Assert.That(objectNames.Contains("aliasWithoutId2"), Is.False);
        Assert.That(objectNames.Contains("aliasWithoutId3"), Is.False);

        string className = typeof(TestObject).FullName;
        string targetName = className + ObjectDefinitionReaderUtils.GENERATED_OBJECT_NAME_SEPARATOR + "0";

        TestObject tb4 = (TestObject) xof.GetObject(targetName);
        Assert.That(tb4.Name, Is.EqualTo(null));
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
        Assert.That(pets.Count == 1, Is.True);
        Assert.That(pets[0].Equals("Nadja"), Is.True);
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
        Assert.That(table.Count == 2, Is.True);
        Assert.That(table["hydrogen"], Is.EqualTo("1"), "Dictionary Value incorrect");
        Assert.That(table["carbon"], Is.EqualTo("12"), "Dictionary value incorrect");
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
        Assert.That(computers.Count == 2, Is.True);
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

        Assert.That(foundDell, Is.True, "Dell string not found in ISet");
        Assert.That(foundIbm, Is.True, "IBM T41 not found in ISet");
    }

    [Test]
    public void EmptyMap()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        HasMap hasMap = (HasMap) xof.GetObject("emptyMap");
        Assert.That(hasMap.Map.Count == 0, Is.True);
    }

    [Test]
    public void MapWithLiteralsOnly()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        HasMap hasMap = (HasMap) xof.GetObject("literalMap");
        Assert.That(hasMap.Map.Count == 3, Is.True);
        Assert.That(hasMap.Map["foo"].Equals("bar"), Is.True);
        Assert.That(hasMap.Map["fi"].Equals("fum"), Is.True);
        Assert.That(hasMap.Map["fa"] == null, Is.True);
    }

    [Test]
    public void MapWithLiteralsAndReferences()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        HasMap hasMap = (HasMap) xof.GetObject("mixedMap");
        Assert.That(hasMap.Map.Count == 3, Is.True);
        Assert.That(hasMap.Map["foo"].Equals("bar"), Is.True);
        TestObject jenny = (TestObject) xof.GetObject("jenny");
        Assert.That(hasMap.Map["jenny"] == jenny, Is.True);
        Assert.That(hasMap.Map["david"].Equals("david"), Is.True);
    }

    [Test]
    public void MapWithLiteralsAndPrototypeReferences()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);

        TestObject jenny = (TestObject) xof.GetObject("pJenny");
        HasMap hasMap = (HasMap) xof.GetObject("pMixedMap");
        Assert.That(hasMap.Map.Count == 2, Is.True);
        Assert.That(hasMap.Map["foo"].Equals("bar"), Is.True);
        Assert.That(hasMap.Map["jenny"].ToString().Equals(jenny.ToString()), Is.True);
        Assert.That(hasMap.Map["jenny"] != jenny, Is.True, "Not same instance");

        HasMap hasMap2 = (HasMap) xof.GetObject("pMixedMap");
        Assert.That(hasMap2.Map.Count == 2, Is.True);
        Assert.That(hasMap2.Map["foo"].Equals("bar"), Is.True);
        Assert.That(hasMap2.Map["jenny"].ToString().Equals(jenny.ToString()), Is.True);
        Assert.That(hasMap2.Map["jenny"] != hasMap.Map["jenny"], Is.True, "Not same instance");
    }

    [Test]
    public void MapWithLiteralsReferencesAndList()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        HasMap hasMap = (HasMap) xof.GetObject("mixedMapWithList");
        Assert.That(hasMap.Map.Count == 4, Is.True);
        Assert.That(hasMap.Map["foo"].Equals("bar"), Is.True);
        TestObject jenny = (TestObject) xof.GetObject("jenny");
        Assert.That(hasMap.Map["jenny"].Equals(jenny), Is.True);

        // Check list
        IList l = (IList) hasMap.Map["list"];
        Assert.That(l, Is.Not.Null);
        Assert.That(l.Count == 4, Is.True);
        Assert.That(l[0].Equals("zero"), Is.True);
        Assert.That(l[3] == null, Is.True);

        // Check nested map in list
        IDictionary m = (IDictionary) l[1];
        Assert.That(m, Is.Not.Null);
        Assert.That(m.Count == 2, Is.True);
        Assert.That(m["fo"].Equals("bar"), Is.True);
        Assert.That(m["jen"].Equals(jenny), Is.True, "Map element 'jenny' should be equal to jenny object, not " + m["jen"]);

        // Check nested list in list
        l = (IList) l[2];
        Assert.That(l, Is.Not.Null);
        Assert.That(l.Count == 2, Is.True);
        Assert.That(l[0].Equals(jenny), Is.True);
        Assert.That(l[1].Equals("ba"), Is.True);

        // Check nested map
        m = (IDictionary) hasMap.Map["map"];
        Assert.That(m, Is.Not.Null);
        Assert.That(m.Count == 2, Is.True);
        Assert.That(m["foo"].Equals("bar"), Is.True);
        Assert.That(m["jenny"].Equals(jenny),
            Is.True,
            "Map element 'jenny' should be equal to jenny object, not " + m["jenny"]);
    }

    [Test]
    public void EmptySet()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        HasMap hasMap = (HasMap) xof.GetObject("emptySet");
        Assert.That(hasMap.Set.Count == 0, Is.True);
    }

    [Test]
    public void PopulatedSet()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        HasMap hasMap = (HasMap) xof.GetObject("set");
        Assert.That(hasMap.Set.Count == 3, Is.True);
        Assert.That(hasMap.Set.Contains("bar"), Is.True);
        TestObject jenny = (TestObject) xof.GetObject("jenny");
        Assert.That(hasMap.Set.Contains(jenny), Is.True);
        Assert.That(hasMap.Set.Contains(null), Is.True);
    }

    [Test]
    public void EmptyProps()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        HasMap hasMap = (HasMap) xof.GetObject("emptyProps");
        Assert.That(hasMap.Props.Count == 0, Is.True);
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
        Assert.That(hasMap.Props.Get("foo"), Is.Empty, "Expected empty string");
    }

    [Test]
    public void PopulatedProps()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        HasMap hasMap = (HasMap) xof.GetObject("props");
        Assert.That(hasMap.Props.Count, Is.EqualTo(2));
        Assert.That(hasMap.Props["foo"], Is.EqualTo("bar"));
        Assert.That(hasMap.Props["2"], Is.EqualTo("TWO"));
    }

    [Test]
    public void PopulatedPropsWithSameKey()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        HasMap hasMap = (HasMap) xof.GetObject("propsWithSameKey");
        Assert.That(hasMap.Props.Count, Is.EqualTo(2));
        Assert.That(hasMap.Props["foo"], Is.EqualTo("OnE,tWo"));
        Assert.That(hasMap.Props.GetValues("bar").Length, Is.EqualTo(2));
        Assert.That(hasMap.Props.GetValues("bar")[0], Is.EqualTo("OnE"));
        Assert.That(hasMap.Props.GetValues("bar")[1], Is.EqualTo("tWo"));
    }

    [Test]
    public void DelimitedProps()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        HasMap hasMap = (HasMap) xof.GetObject("delimitedProps");
        Assert.That(hasMap.Props.Count, Is.EqualTo(2));
        Assert.That(hasMap.Props.GetValues("foo").Length, Is.EqualTo(3));
        Assert.That(hasMap.Props.GetValues("days").Length, Is.EqualTo(7));
        Assert.That(hasMap.Props.GetValues("days")[2], Is.EqualTo("wednesday"));
    }

    [Test]
    public void ObjectArray()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        HasMap hasMap = (HasMap) xof.GetObject("objectArray");
        Assert.That(hasMap.ObjectArray.Length == 2, Is.True);
        Assert.That(hasMap.ObjectArray[0].Equals("one"), Is.True);
        Assert.That(hasMap.ObjectArray[1].Equals(xof.GetObject("jenny")), Is.True);
    }

    [Test]
    public void ClassArray()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        HasMap hasMap = (HasMap) xof.GetObject("classArray");
        Assert.That(hasMap.ClassArray.Length == 2, Is.True);
        Assert.That(hasMap.ClassArray[0].Equals(typeof(String)), Is.True);
        Assert.That(hasMap.ClassArray[1].Equals(typeof(Exception)), Is.True);
    }

    [Test]
    public void GetDictionaryThatUsesEntryValueShortcut()
    {
        XmlObjectFactory xof = new XmlObjectFactory(
            new ReadOnlyXmlTestResource("collections.xml", GetType()));
        IDictionary map = (IDictionary) xof.GetObject("mapWithEntryValueShortcut");
        Assert.That(map.Count, Is.EqualTo(2));
        string v1 = (string) map["rob"];
        Assert.That(v1, Is.EqualTo("robert"));
        string v2 = (string) map["jen"];
        Assert.That(v2, Is.EqualTo("jenny"));
    }

    [Test]
    public void GetDictionaryThatUsesStringKeysSpecifiedAsElements()
    {
        XmlObjectFactory xof = new XmlObjectFactory(
            new ReadOnlyXmlTestResource("collections.xml", GetType()));
        IDictionary map = (IDictionary) xof.GetObject("mapWithStringKeysSpecifiedAsElements");
        Assert.That(map.Count, Is.EqualTo(2));
        string v1 = (string) map["rick"];
        Assert.That(v1, Is.EqualTo("Rick Evans"));
        string v2 = (string) map["uncleelvis"];
        Assert.That(v2, Is.EqualTo("Elvis Orten"));
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
        Assert.That(map.Count, Is.EqualTo(2));
        string v1 = (string) map[new TestObject("Rick Evans", 30)];
        Assert.That(v1, Is.EqualTo("Rick Evans"));
        string v2 = (string) map[new TestObject("Uncle Elvis", 47)];
        Assert.That(v2, Is.EqualTo("Elvis Orten"));
    }

    [Test]
    public void GetDictionaryWithValueRefAttributeShortcuts()
    {
        XmlObjectFactory xof = new XmlObjectFactory(
            new ReadOnlyXmlTestResource("collections.xml", GetType()));
        IDictionary map = (IDictionary) xof.GetObject("mapWithValueRefAttributeShortcuts");
        Assert.That(map.Count, Is.EqualTo(2));
        foreach (string key in map.Keys)
        {
            object obj = map[key];
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.GetType(), Is.EqualTo(typeof(TestObject)), "Wrong value assigned to value of dictionary.");
            TestObject tob = (TestObject) obj;
            if (key == "rick")
            {
                Assert.That(tob.Name, Is.EqualTo("Rick Evans"), "Wrong object value assigned to the 'rick' key.");
            }
            else if (key == "uncleelvis")
            {
                Assert.That(tob.Name, Is.EqualTo("Uncle Elvis"), "Wrong object value assigned to the 'uncleelvis' key.");
            }
        }
    }

    [Test]
    public void CollectionFactoryDefaults()
    {
        ListFactoryObject listFactory = new ListFactoryObject();
        listFactory.SourceList = new ArrayList();
        listFactory.AfterPropertiesSet();
        Assert.That(listFactory.GetObject() is ArrayList, Is.True);

        SetFactoryObject setFactory = new SetFactoryObject();
        setFactory.SourceSet = new HybridSet();
        setFactory.AfterPropertiesSet();
        Assert.That(setFactory.GetObject() is HybridSet, Is.True);

        DictionaryFactoryObject dictionaryFactory = new DictionaryFactoryObject();
        dictionaryFactory.SourceDictionary = new Hashtable();
        dictionaryFactory.AfterPropertiesSet();
        Assert.That(dictionaryFactory.GetObject() is Hashtable, Is.True);
    }

    [Test]
    public void ListFactory()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        IList list = (IList) xof.GetObject("listFactory");
        Assert.That(list is LinkedList, Is.True);
        Assert.That(list.Count == 2, Is.True);
        Assert.That(list[0], Is.EqualTo("bar"));
        Assert.That(list[1], Is.EqualTo("jenny"));
    }

    [Test]
    public void PrototypeListFactory()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        IList list = (IList) xof.GetObject("pListFactory");
        Assert.That(list is LinkedList, Is.True);
        Assert.That(list.Count == 2, Is.True);
        Assert.That(list[0], Is.EqualTo("bar"));
        Assert.That(list[1], Is.EqualTo("jenny"));
    }

    [Test]
    public void SetFactory()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        Set mySet = (Set) xof.GetObject("setFactory");
        Assert.That(mySet is HybridSet, Is.True);
        Assert.That(mySet.Count == 2, Is.True);
        Assert.That(mySet.Contains("bar"), Is.True);
        Assert.That(mySet.Contains("jenny"), Is.True);
    }

    [Test]
    public void PrototypeSetFactory()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        Set mySet = (Set) xof.GetObject("pSetFactory");
        Assert.That(mySet is HybridSet, Is.True);
        Assert.That(mySet.Count == 2, Is.True);
        Assert.That(mySet.Contains("bar"), Is.True);
        Assert.That(mySet.Contains("jenny"), Is.True);
    }

    [Test]
    public void DictionaryFactory()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        IDictionary map = (IDictionary) xof.GetObject("mapFactory");
        Assert.That(map.Count == 2, Is.True);
        Assert.That(map["foo"], Is.EqualTo("bar"));
        Assert.That(map["jen"], Is.EqualTo("jenny"));
    }

    [Test]
    public void PrototypeDictionaryFactory()
    {
        IResource resource = new ReadOnlyXmlTestResource("collections.xml", GetType());
        XmlObjectFactory xof = new XmlObjectFactory(resource);
        IDictionary map = (IDictionary) xof.GetObject("pMapFactory");
        Assert.That(map.Count == 2, Is.True);
        Assert.That(map["foo"], Is.EqualTo("bar"));
        Assert.That(map["jen"], Is.EqualTo("jenny"));
    }

    [Test]
    public void GetDictionaryThatUsesNonStringKeys()
    {
        XmlObjectFactory xof = new XmlObjectFactory(
            new ReadOnlyXmlTestResource("collections.xml", GetType()));
        IDictionary map = (IDictionary) xof.GetObject("mapWithNonStringKeys");
        Assert.That(map.Count, Is.EqualTo(2));
        string v1 = (string) map[new TestObject("Rick Evans", 30)];
        Assert.That(v1, Is.EqualTo("Rick Evans"));
        string v2 = (string) map[new TestObject("Uncle Elvis", 47)];
        Assert.That(v2, Is.EqualTo("Elvis Orten"));
    }

    [Test]
    public void TypedNonGenericList()
    {
        XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("collections.xml", GetType()));
        NonGenericExpressionHolder obj = (NonGenericExpressionHolder) xof.GetObject("nonGenericExpressionHolder");
        Assert.That(obj[0].GetValue(), Is.EqualTo(2));
        Assert.That(obj[1].GetValue(), Is.EqualTo(8));
        Assert.That(obj[2].GetValue(), Is.EqualTo("ALEKSANDAR SEOVIC"));
        Assert.That((bool) obj[3].GetValue(), Is.True);
    }

    [Test]
    public void TypedNonGenericDictionary()
    {
        XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("collections.xml", GetType()));
        NonGenericExpressionHolder obj = (NonGenericExpressionHolder) xof.GetObject("nonGenericExpressionHolder");
        Assert.That(obj["0"].GetValue(), Is.EqualTo(2));
        Assert.That(obj["1"].GetValue(), Is.EqualTo(8));
        Assert.That(obj["2"].GetValue(), Is.EqualTo("ALEKSANDAR SEOVIC"));
        Assert.That((bool) obj["3"].GetValue(), Is.True);
    }

    [Test]
    public void TypedGenericList()
    {
        XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("collections.xml", GetType()));
        GenericExpressionHolder obj = (GenericExpressionHolder) xof.GetObject("genericExpressionHolder");
        Assert.That(obj[0].GetValue(), Is.EqualTo(2));
        Assert.That(obj[1].GetValue(), Is.EqualTo(8));
        Assert.That(obj[2].GetValue(), Is.EqualTo("ALEKSANDAR SEOVIC"));
        Assert.That((bool) obj[3].GetValue(), Is.True);
    }

    [Test]
    public void TypedGenericDictionary()
    {
        XmlObjectFactory xof = new XmlObjectFactory(new ReadOnlyXmlTestResource("collections.xml", GetType()));
        GenericExpressionHolder obj = (GenericExpressionHolder) xof.GetObject("genericExpressionHolder");
        Assert.That(obj["0"].GetValue(), Is.EqualTo(2));
        Assert.That(obj["1"].GetValue(), Is.EqualTo(8));
        Assert.That(obj["2"].GetValue(), Is.EqualTo("ALEKSANDAR SEOVIC"));
        Assert.That((bool) obj["3"].GetValue(), Is.True);
    }
}

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
        get { return (IExpression) this.expressionsList[index]; }
    }

    public IExpression this[string key]
    {
        get { return (IExpression) this.expressionsDictionary[key]; }
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

    public System.Collections.Generic.IDictionary<string, IExpression> ExpressionsDictionary
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