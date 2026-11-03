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
using System.Collections.Specialized;
using NUnit.Framework;
using Spring.Collections;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory.Xml;

/// <summary>
/// Unit and integration tests for the collection merging support
/// </summary>
/// <author>Rod Johnson</author>
/// <author>Rick Evans</author>
/// <author>Mark Pollack (.NET)</author>
[TestFixture]
public class CollectionMergingTests
{
    private DefaultListableObjectFactory objectFactory;

    [SetUp]
    public void SetUp()
    {
        this.objectFactory = new DefaultListableObjectFactory();
        IObjectDefinitionReader reader = new XmlObjectDefinitionReader(this.objectFactory);
        reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("collectionMerging.xml", GetType()));
    }

    [TearDown]
    public void TearDown()
    {
        objectFactory?.Dispose();
    }

    [Test]
    public void MergeList()
    {
        TestObject to = (TestObject) this.objectFactory.GetObject("childWithList");
        IList list = to.SomeList;
        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list[0], Is.EqualTo("Rob Harrop"));
        Assert.That(list[1], Is.EqualTo("Rod Johnson"));
        Assert.That(list[2], Is.EqualTo("Juergen Hoeller"));
    }

    [Test]
    public void MergListWithInnerObjectAsListElement()
    {
        TestObject to = (TestObject) this.objectFactory.GetObject("childWithListOfRefs");
        IList list = to.SomeList;
        Assert.That(list, Is.Not.Null);
        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list[2], Is.Not.Null);
        Assert.That(list[2], Is.InstanceOf(typeof(TestObject)));
    }

    [Test]
    public void MergeSet()
    {
        TestObject to = (TestObject) this.objectFactory.GetObject("childWithSet");
        ISet set = to.SomeSet;
        Assert.That(set.Count, Is.EqualTo(2));
        Assert.That(set.Contains("Rob Harrop"), Is.True);
        Assert.That(set.Contains("Sally Greenwood"), Is.True);
    }

    [Test]
    public void MergeSetWithInnerObjectAsSetElement()
    {
        TestObject to = (TestObject) this.objectFactory.GetObject("childWithSetOfRefs");
        ISet set = to.SomeSet;
        Assert.That(set, Is.Not.Null);
        Assert.That(set.Count, Is.EqualTo(2));
        IEnumerator enumerator = set.GetEnumerator();
        enumerator.MoveNext();
        enumerator.MoveNext();
        object o = enumerator.Current;
        Assert.That(o, Is.Not.Null);
        Assert.That(o, Is.InstanceOf(typeof(TestObject)));
        Assert.That(((TestObject) o).Name, Is.EqualTo("Sally"));
    }

    [Test]
    public void MergeDictionary()
    {
        TestObject to = (TestObject) this.objectFactory.GetObject("childWithMap");
        IDictionary map = to.SomeMap;
        Assert.That(map.Count, Is.EqualTo(3));
        Assert.That(map["Rob"], Is.EqualTo("Sally"));
        Assert.That(map["Rod"], Is.EqualTo("Kerry"));
        Assert.That(map["Juergen"], Is.EqualTo("Eva"));
    }

    [Test]
    public void MergeMapWithInnerObjectAsMapEntryValue()
    {
        TestObject to = (TestObject) this.objectFactory.GetObject("childWithMapOfRefs");
        IDictionary map = to.SomeMap;
        Assert.That(map, Is.Not.Null);
        Assert.That(map.Count, Is.EqualTo(2));
        Assert.That(map["Rob"], Is.Not.Null);
        Assert.That(map["Rob"], Is.InstanceOf(typeof(TestObject)));
        Assert.That(((TestObject) map["Rob"]).Name, Is.EqualTo("Sally"));
    }

    [Test]
    public void MergeNameValueCollection()
    {
        TestObject to = (TestObject) this.objectFactory.GetObject("childWithNameValues");
        NameValueCollection map = to.SomeNameValueCollection;
        Assert.That(map.Count, Is.EqualTo(3));
        Assert.That(map["Rob"], Is.EqualTo("Sally"));
        Assert.That(map["Rod"], Is.EqualTo("Kerry"));
        Assert.That(map["Juergen"], Is.EqualTo("Eva"));
    }

    [Test]
    public void MergeListInConstructor()
    {
        TestObject to = (TestObject) this.objectFactory.GetObject("childWithListInConstructor");
        IList list = to.SomeList;
        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list[0], Is.EqualTo("Rob Harrop"));
        Assert.That(list[1], Is.EqualTo("Rod Johnson"));
        Assert.That(list[2], Is.EqualTo("Juergen Hoeller"));
    }

    [Test]
    public void MergeListWithInnerObjectAsListElementInConstructor()
    {
        TestObject to = (TestObject) this.objectFactory.GetObject("childWithListOfRefsInConstructor");
        IList list = to.SomeList;
        Assert.That(list, Is.Not.Null);
        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list[2], Is.Not.Null);
        Assert.That(list[2], Is.InstanceOf(typeof(TestObject)));
    }

    [Test]
    public void MergeSetInConstructor()
    {
        TestObject to = (TestObject) this.objectFactory.GetObject("childWithSetInConstructor");
        ISet set = to.SomeSet;
        Assert.That(set.Count, Is.EqualTo(2));
        Assert.That(set.Contains("Rob Harrop"), Is.True);
        Assert.That(set.Contains("Sally Greenwood"), Is.True);
    }

    [Test]
    public void MergeSetWithInnerObjectAsSetElementInConstructor()
    {
        TestObject to = (TestObject) this.objectFactory.GetObject("childWithSetOfRefsInConstructor");
        ISet set = to.SomeSet;
        Assert.That(set, Is.Not.Null);
        Assert.That(set.Count, Is.EqualTo(2));
        IEnumerator enumerator = set.GetEnumerator();
        enumerator.MoveNext();
        enumerator.MoveNext();
        object o = enumerator.Current;
        Assert.That(o, Is.Not.Null);
        Assert.That(o, Is.InstanceOf(typeof(TestObject)));
        Assert.That(((TestObject) o).Name, Is.EqualTo("Sally"));
    }

    [Test]
    public void MergeMapInConstructor()
    {
        TestObject to = (TestObject) this.objectFactory.GetObject("childWithMapInConstructor");
        IDictionary map = to.SomeMap;
        Assert.That(map.Count, Is.EqualTo(3));
        Assert.That(map["Rob"], Is.EqualTo("Sally"));
        Assert.That(map["Rod"], Is.EqualTo("Kerry"));
        Assert.That(map["Juergen"], Is.EqualTo("Eva"));
    }

    [Test]
    public void MergeMapWithInnerObjectgAsMapEntryValueInConstructor()
    {
        TestObject to = (TestObject) this.objectFactory.GetObject("childWithMapOfRefsInConstructor");
        IDictionary map = to.SomeMap;
        Assert.That(map, Is.Not.Null);
        Assert.That(map.Count, Is.EqualTo(2));
        Assert.That(map["Rob"], Is.Not.Null);
        Assert.That(map["Rob"], Is.InstanceOf(typeof(TestObject)));
        Assert.That(((TestObject) map["Rob"]).Name, Is.EqualTo("Sally"));
    }

    [Test]
    public void MergeNameValueCollectionInConstructor()
    {
        TestObject to = (TestObject) this.objectFactory.GetObject("childWithPropsInConstructor");
        NameValueCollection props = to.SomeNameValueCollection;
        Assert.That(props.Count, Is.EqualTo(3));
        Assert.That(props["Rob"], Is.EqualTo("Sally"));
        Assert.That(props["Rod"], Is.EqualTo("Kerry"));
        Assert.That(props["Juergen"], Is.EqualTo("Eva"));
    }
}
