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

using NUnit.Framework;
using Spring.Objects.Factory.Support;
using System.Collections;
using Spring.Core.TypeConversion;
using System.Collections.Generic;
using System.Collections.Specialized;

#endregion

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Unit and integration tests for collection conversion support
    /// </summary>
    /// <author>Choy Rim</author>
    [TestFixture]
    [Description("SPRNET-1470 Setting property of type IList<T> using <list/> without the @element-type specified fails.")]
    public class CollectionConversionGenericTests
    {
        private DefaultListableObjectFactory objectFactory;

        [SetUp]
        public void SetUp()
        {
            this.objectFactory = new DefaultListableObjectFactory();
            IObjectDefinitionReader reader = new XmlObjectDefinitionReader(this.objectFactory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("collectionConversionGeneric.xml", GetType()));
        }

        [Test]
        public void ShouldConvertListToGenericIList()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("HasGenericIListProperty");
            IList<int> list = to.SomeGenericIListInt32;
            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[0], Is.EqualTo(123));
            Assert.That(list[1], Is.EqualTo(234));
            Assert.That(list[2], Is.EqualTo(345));
        }

        [Test]
        public void ShouldConvertDictionaryToGenericIDictionary()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("HasGenericIDictionaryProperty");
            IDictionary<string, int> dict = to.SomeGenericIDictionaryStringInt32;
            Assert.That(dict.Count, Is.EqualTo(3));
            Assert.That(dict["aaa"], Is.EqualTo(111));
            Assert.That(dict["bbb"], Is.EqualTo(222));
            Assert.That(dict["ccc"], Is.EqualTo(333));
        }

        [Test]
        public void ShouldConvertListToGenericIEnumerable()
        {
            TestObject to = (TestObject)this.objectFactory.GetObject("HasGenericIEnumerableProperty");
            IEnumerable<int> enumerable = to.SomeGenericIEnumerableInt32;
            int enumerableLength = 0;
            int first = 0;

            foreach (int i in enumerable)
            {
                enumerableLength += 1;
                if (enumerableLength == 1)
                {
                    first = i;
                }
            }

            Assert.That(enumerableLength, Is.EqualTo(1));
            Assert.That(first, Is.EqualTo(123));
        }

        [Test]
        public void ConvertArrayListToGenericIList()
        {
            ArrayList xs = new ArrayList();
            xs.Add("Mark Pollack");
            object ys = TypeConversionUtils.ConvertValueIfNecessary(typeof(IList<string>), xs, "ignored");
            Assert.That(ys as IList<string>, Is.Not.Null);
            IList<string> zs = (IList<string>)ys;
            Assert.That(zs[0], Is.EqualTo("Mark Pollack"));
        }

        [Test]
        public void ConvertHybridDictionaryToGenericIDictionary()
        {
            HybridDictionary xs = new HybridDictionary();
            xs.Add("first", 1);
            object ys = TypeConversionUtils.ConvertValueIfNecessary(typeof(IDictionary<string, int>), xs, "ignored");
            Assert.That(ys as IDictionary<string, int>, Is.Not.Null);
            IDictionary<string, int> zs = (IDictionary<string, int>)ys;
            Assert.That(zs["first"], Is.EqualTo(1));
        }

        [Test]
        public void ConvertArrayListToGenericIEnumerable()
        {
            ArrayList xs = new ArrayList();
            xs.Add("Mark Pollack");
            object ys = TypeConversionUtils.ConvertValueIfNecessary(typeof(IEnumerable<string>), xs, "ignored");
            Assert.That(ys as IEnumerable<string>, Is.Not.Null);
            IEnumerable<string> zs = (IEnumerable<string>)ys;
            IEnumerator<string> zse = zs.GetEnumerator();
            Assert.That(zse.MoveNext(), Is.True);
            Assert.That(zse.Current, Is.EqualTo("Mark Pollack"));
        }

    }
}
