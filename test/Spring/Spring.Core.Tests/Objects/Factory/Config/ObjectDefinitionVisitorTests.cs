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

using System;
using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Unit tests for the ObjectDefinitionVisitor class
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class ObjectDefinitionVisitorTests
    {
        private Hashtable properties;

        [SetUp]
        public void SetUp()
        {
            properties = CollectionsUtil.CreateCaseInsensitiveHashtable();
            properties.Add("Property", "Value");
        }

        private string ParseAndResolveVariables(string rawText)
        {
            if (rawText.StartsWith("$"))
            {
                return (string) properties[rawText.Substring(1)];
            }
            return rawText;
        }

        [Test]
        public void BadConstructorCall()
        {
            Assert.Throws<ArgumentNullException>(() => new ObjectDefinitionVisitor(null));
        }

        [Test]
        public void VisitObjectTypeName()
        {
            IObjectDefinition od = new RootObjectDefinition();
            od.ObjectTypeName = "$Property";

            ObjectDefinitionVisitor odv = new ObjectDefinitionVisitor(new ObjectDefinitionVisitor.ResolveHandler(ParseAndResolveVariables));
            odv.VisitObjectDefinition(od);

            Assert.AreEqual("Value", od.ObjectTypeName);
        }

        [Test]
        public void VisitPropertyValues()
        {
            IObjectDefinition od = new RootObjectDefinition();
            od.PropertyValues.Add("PropertyName", "$Property");

            ObjectDefinitionVisitor odv = new ObjectDefinitionVisitor(new ObjectDefinitionVisitor.ResolveHandler(ParseAndResolveVariables));
            odv.VisitObjectDefinition(od);

            Assert.AreEqual("Value", od.PropertyValues.GetPropertyValue("PropertyName").Value);
        }


        [Test]
        public void VisitManagedList()
        {
            IObjectDefinition od = new RootObjectDefinition();
            ManagedList ml = new ManagedList();
            ml.ElementTypeName = "$Property";
            ml.Add("$Property");
            od.PropertyValues.Add("PropertyName", ml);

            ObjectDefinitionVisitor odv = new ObjectDefinitionVisitor(new ObjectDefinitionVisitor.ResolveHandler(ParseAndResolveVariables));
            odv.VisitObjectDefinition(od);

            ManagedList list = od.PropertyValues.GetPropertyValue("PropertyName").Value as ManagedList;

            Assert.IsNotNull(list, "Property value is not of type ManagedList.  Type = [" +
                od.PropertyValues.GetPropertyValue("PropertyName").Value.GetType() + "]");
            Assert.AreEqual("Value", list.ElementTypeName);
            Assert.AreEqual("Value", list[0]);
        }

        [Test]
        public void VisitManagedSet()
        {
            IObjectDefinition od = new RootObjectDefinition();
            ManagedSet ms = new ManagedSet();
            ms.ElementTypeName = "$Property";
            ms.Add("$Property");
            od.PropertyValues.Add("PropertyName", ms);

            ObjectDefinitionVisitor odv = new ObjectDefinitionVisitor(new ObjectDefinitionVisitor.ResolveHandler(ParseAndResolveVariables));
            odv.VisitObjectDefinition(od);

            ManagedSet set = od.PropertyValues.GetPropertyValue("PropertyName").Value as ManagedSet;

            Assert.AreEqual("Value", set.ElementTypeName);
            IEnumerator enumerator = set.GetEnumerator();
            enumerator.MoveNext();
            Assert.AreEqual("Value", enumerator.Current);
        }

        [Test]
        public void VisitManagedDictionary()
        {
            IObjectDefinition od = new RootObjectDefinition();
            ManagedDictionary md = new ManagedDictionary();
            md.KeyTypeName = "$Property";
            md.ValueTypeName = "$Property";
            md.Add("Key", "$Property");
            od.PropertyValues.Add("PropertyName", md);

            ObjectDefinitionVisitor odv = new ObjectDefinitionVisitor(new ObjectDefinitionVisitor.ResolveHandler(ParseAndResolveVariables));
            odv.VisitObjectDefinition(od);

            ManagedDictionary dictionary = od.PropertyValues.GetPropertyValue("PropertyName").Value as ManagedDictionary;

            Assert.AreEqual("Value", dictionary.KeyTypeName);
            Assert.AreEqual("Value", dictionary.ValueTypeName);
            Assert.AreEqual("Value", dictionary["Key"]);
        }

        [Test]
        public void VisitNameValueCollection()
        {
            IObjectDefinition od = new RootObjectDefinition();
            NameValueCollection nvc = new NameValueCollection();
            nvc["Key"] = "$Property";
            od.PropertyValues.Add("PropertyName", nvc);

            ObjectDefinitionVisitor odv = new ObjectDefinitionVisitor(new ObjectDefinitionVisitor.ResolveHandler(ParseAndResolveVariables));
            odv.VisitObjectDefinition(od);

            NameValueCollection visitedNvc =
                od.PropertyValues.GetPropertyValue("PropertyName").Value as NameValueCollection;

            Assert.AreEqual("Value", visitedNvc["Key"]);
        }
    }
}