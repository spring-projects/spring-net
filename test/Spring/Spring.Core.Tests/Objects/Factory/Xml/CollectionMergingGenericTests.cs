#region License

/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
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
    [Description("SPRNET-1242 Support for collection merging with generic collections")]
	public class CollectionMergingGenericTests
	{
	    private DefaultListableObjectFactory objectFactory;

        [SetUp]
        public void SetUp()
        {
            this.objectFactory = new DefaultListableObjectFactory();
            IObjectDefinitionReader reader = new XmlObjectDefinitionReader(this.objectFactory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("collectionMergingGeneric.xml", GetType()));
        }

        [Test]
        public void MergeList()
        {
            TestObject to = (TestObject) this.objectFactory.GetObject("childWithList");
            System.Collections.Generic.List<string> list = to.SomeGenericStringList;
            Assert.That(3, Is.EqualTo(list.Count));
            Assert.That("Rob Harrop", Is.EqualTo(list[0]));
            Assert.That("Rod Johnson", Is.EqualTo(list[1]));
            Assert.That("Juergen Hoeller", Is.EqualTo(list[2]));
        }

	}
}
