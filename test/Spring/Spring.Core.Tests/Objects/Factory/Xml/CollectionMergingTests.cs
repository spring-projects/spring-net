#region License

/*
 * Copyright © 2002-2009 the original author or authors.
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
        public void MergeSet()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("childWithSet");
            ISet set = to.SomeSet;
            Assert.AreEqual(2, set.Count);
            Assert.IsTrue(set.Contains("Rob Harrop"));
            Assert.IsTrue(set.Contains("Sally Greenwood"));
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
        public void MergeNameValueCollection()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("childWithNameValues");
            NameValueCollection map = to.SomeNameValueCollection;
            Assert.AreEqual(3, map.Count);
            Assert.AreEqual("Sally", map["Rob"]);
            Assert.AreEqual("Kerry", map["Rod"]);
            Assert.AreEqual("Eva", map["Juergen"]);
        }
	}
}