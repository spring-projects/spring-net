#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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
using DotNetMock.Dynamic;
using NUnit.Framework;
using Spring.Collections;
using Spring.Context.Support;
using Spring.Core.TypeResolution;
using Spring.Util;
using Spring.Objects.Factory.Support;
using Spring.Context;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Unit tests for the TypeAliasConfigurer class
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class TypeAliasConfigurerTests
    {
        [Test]
        public void Serialization()
        {
            IDictionary typeAliases = new Hashtable();
            typeAliases.Add("LinkedList", typeof(LinkedList));

            TypeAliasConfigurer typeAliasConfigurer = new TypeAliasConfigurer();
            typeAliasConfigurer.TypeAliases = typeAliases;

            typeAliasConfigurer.Order = 1;

            SerializationTestUtils.SerializeAndDeserialize(typeAliasConfigurer);
        }

        [Test]
		[ExpectedException(typeof(ObjectInitializationException))]
        public void UseInvalidTypeForDictionaryValue()
        {
            IDictionary typeAliases = new Hashtable();
            typeAliases.Add("LinkedList", new LinkedList());

            TypeAliasConfigurer typeAliasConfigurer = new TypeAliasConfigurer();
            typeAliasConfigurer.TypeAliases = typeAliases;

            typeAliasConfigurer.PostProcessObjectFactory((IConfigurableListableObjectFactory)new DynamicMock(typeof(IConfigurableListableObjectFactory)).Object);
        }

        [Test]
        [ExpectedException(typeof(ObjectInitializationException))]
        public void UseNonResolvableTypeForDictionaryValue()
        {
            IDictionary typeAliases = new Hashtable();
            typeAliases.Add("LinkedList", "Spring.Collections.LinkkkedList");

            TypeAliasConfigurer typeAliasConfigurer = new TypeAliasConfigurer();
            typeAliasConfigurer.TypeAliases = typeAliases;

            typeAliasConfigurer.PostProcessObjectFactory((IConfigurableListableObjectFactory)new DynamicMock(typeof(IConfigurableListableObjectFactory)).Object);
        }

        [Test]
        public void SunnyDayScenarioUsingType()
        {
            IDictionary typeAliases = new Hashtable();
            typeAliases.Add("LinkedList", typeof(LinkedList));

            CreateConfigurerAndTestLinkedList(typeAliases);
        }

        [Test]
        public void SunnyDayScenarioUsingTypeString()
        {
            IDictionary typeAliases = new Hashtable();
            typeAliases.Add("LinkedList", "Spring.Collections.LinkedList, Spring.Core");
            CreateConfigurerAndTestLinkedList(typeAliases);
        }

        [Test]
        public void WithinApplicationContext()
        {
            IApplicationContext ctx = new XmlApplicationContext(
                "file://Spring/Objects/Factory/Config/typeAliases.xml");

            object obj1 = ctx.GetObject("testObject1");
            Assert.IsNotNull(obj1);
            Assert.AreEqual(typeof(TestObject), obj1.GetType());

            object obj2 = ctx.GetObject("testObject2");
            Assert.IsNotNull(obj2);
            Assert.AreEqual(typeof(TestObject), obj2.GetType());

            object obj3 = ctx.GetObject("testObject3");
            Assert.IsNotNull(obj3);
            Assert.AreEqual(typeof(TestObject), obj3.GetType());
            Assert.AreEqual("Bruno", ((TestObject)obj3).Name);
            Assert.AreEqual(26, ((TestObject)obj3).Age);
        }

        private static void CreateConfigurerAndTestLinkedList(IDictionary typeAliases)
        {
            TypeAliasConfigurer typeAliasConfigurer = new TypeAliasConfigurer();
            typeAliasConfigurer.TypeAliases = typeAliases;

            typeAliasConfigurer.Order = 1;
            

            typeAliasConfigurer.PostProcessObjectFactory((IConfigurableListableObjectFactory)new DynamicMock(typeof(IConfigurableListableObjectFactory)).Object);

            //todo investigate mocking the typeregistry, for now ask the actual one for information.
            Assert.IsTrue(TypeRegistry.ContainsAlias("LinkedList"),
                          "TypeAliasConfigurer did not register a type alias with the TypeRegistry");

            Type linkedListType = TypeRegistry.ResolveType("LinkedList");
            Assert.IsTrue(linkedListType.Equals(typeof(LinkedList)), "Incorrect type resolved.");
            Assert.AreEqual(1,typeAliasConfigurer.Order);
        }
    }
}
