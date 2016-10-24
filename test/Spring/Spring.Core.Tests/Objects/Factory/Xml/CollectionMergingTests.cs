#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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
using System.Collections.Specialized;
using NUnit.Framework;
using Spring.Collections;
using Spring.Objects.Factory.Support;

#endregion

namespace Spring.Objects.Factory.Xml
{
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

        [Test]
        public void MergeList()
        {
            TestObject to = (TestObject) this.objectFactory.GetObject("childWithList");
            IList list = to.SomeList;
            Assert.That(3, Is.EqualTo(list.Count));
            Assert.That("Rob Harrop", Is.EqualTo(list[0]));
            Assert.That("Rod Johnson", Is.EqualTo(list[1]));
            Assert.That("Juergen Hoeller", Is.EqualTo(list[2]));
        }

        [Test]
        public void MergListWithInnerObjectAsListElement()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("childWithListOfRefs");
            IList list = to.SomeList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.IsNotNull(list[2]);
            Assert.That(list[2], Is.InstanceOf(typeof (TestObject)));
        }

        [Test]
        public void MergeSet()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("childWithSet");
            ISet set = to.SomeSet;
            Assert.AreEqual(2, set.Count);
            Assert.IsTrue(set.Contains("Rob Harrop"));
            Assert.IsTrue(set.Contains("Sally Greenwood"));
        }

        [Test]
        public void MergeSetWithInnerObjectAsSetElement()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("childWithSetOfRefs");
            ISet set = to.SomeSet;
            Assert.IsNotNull(set);
            Assert.AreEqual(2, set.Count);
            IEnumerator enumerator = set.GetEnumerator();
            enumerator.MoveNext();
            enumerator.MoveNext();
            object o = enumerator.Current;
            Assert.IsNotNull(o);
            Assert.That(o, Is.InstanceOf(typeof(TestObject)));
            Assert.AreEqual("Sally", ((TestObject) o).Name);
        }

        [Test]
        public void MergeDictionary()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("childWithMap");
            IDictionary map = to.SomeMap;
            Assert.AreEqual(3, map.Count);
            Assert.AreEqual("Sally", map["Rob"]);
            Assert.AreEqual("Kerry", map["Rod"]);
            Assert.AreEqual("Eva", map["Juergen"]);
        }

        [Test]
        public void MergeMapWithInnerObjectAsMapEntryValue()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("childWithMapOfRefs");
            IDictionary map = to.SomeMap;
            Assert.NotNull(map);
            Assert.AreEqual(2, map.Count);
            Assert.NotNull(map["Rob"]);
            Assert.That(map["Rob"], Is.InstanceOf(typeof(TestObject)));
            Assert.AreEqual("Sally", ((TestObject) map["Rob"]).Name);
        }

        [Test]
        public void MergeNameValueCollection()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("childWithNameValues");
            NameValueCollection map = to.SomeNameValueCollection;
            Assert.AreEqual(3, map.Count);
            Assert.AreEqual("Sally", map["Rob"]);
            Assert.AreEqual("Kerry", map["Rod"]);
            Assert.AreEqual("Eva", map["Juergen"]);
        }

        [Test]
        public void MergeListInConstructor()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("childWithListInConstructor");
            IList list = to.SomeList;
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual("Rob Harrop", list[0]);
            Assert.AreEqual("Rod Johnson", list[1]);
            Assert.AreEqual("Juergen Hoeller", list[2]);
        }

        [Test]
        public void MergeListWithInnerObjectAsListElementInConstructor()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("childWithListOfRefsInConstructor");
            IList list = to.SomeList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.IsNotNull(list[2]);
            Assert.That(list[2], Is.InstanceOf(typeof(TestObject)));
        }

        [Test]
        public void MergeSetInConstructor()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("childWithSetInConstructor");
            ISet set = to.SomeSet;
            Assert.AreEqual(2, set.Count);
            Assert.IsTrue(set.Contains("Rob Harrop"));
            Assert.IsTrue(set.Contains("Sally Greenwood"));            
        }

        [Test]
        public void MergeSetWithInnerObjectAsSetElementInConstructor()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("childWithSetOfRefsInConstructor");
            ISet set = to.SomeSet;
            Assert.IsNotNull(set);
            Assert.AreEqual(2, set.Count);
            IEnumerator enumerator = set.GetEnumerator();
            enumerator.MoveNext();
            enumerator.MoveNext();
            object o = enumerator.Current;
            Assert.IsNotNull(o);
            Assert.That(o, Is.InstanceOf(typeof(TestObject)));
            Assert.AreEqual("Sally", ((TestObject)o).Name);
        }

        [Test]
        public void MergeMapInConstructor()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("childWithMapInConstructor");
            IDictionary map = to.SomeMap;
            Assert.AreEqual(3, map.Count);
            Assert.AreEqual("Sally", map["Rob"]);
            Assert.AreEqual("Kerry", map["Rod"]);
            Assert.AreEqual("Eva", map["Juergen"]);           
        }

        [Test]
        public void MergeMapWithInnerObjectgAsMapEntryValueInConstructor()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("childWithMapOfRefsInConstructor");
            IDictionary map = to.SomeMap;
            Assert.IsNotNull(map);
            Assert.AreEqual(2, map.Count);
            Assert.IsNotNull(map["Rob"]);
            Assert.That(map["Rob"], Is.InstanceOf(typeof(TestObject)));
            Assert.AreEqual("Sally", ((TestObject) map["Rob"]).Name);
            
        }

        [Test]
        public void MergeNameValueCollectionInConstructor()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("childWithPropsInConstructor");
            NameValueCollection props = to.SomeNameValueCollection;
            Assert.AreEqual(3, props.Count);
            Assert.AreEqual("Sally", props["Rob"]);
            Assert.AreEqual("Kerry", props["Rod"]);
            Assert.AreEqual("Eva", props["Juergen"]);
            
        }
	}
}