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

using FakeItEasy;

using NUnit.Framework;
using Spring.Collections;
using Spring.Context.Support;
using Spring.Core.TypeResolution;
using Spring.Util;
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
        private IConfigurableListableObjectFactory factory;

        [SetUp]
        public void SetUp()
        {
            factory = A.Fake<IConfigurableListableObjectFactory>();
        }

        [Test]
        public void Serialization()
        {
            IDictionary typeAliases = new Hashtable();
            typeAliases.Add("LinkedList", typeof(LinkedList).AssemblyQualifiedName);

            TypeAliasConfigurer typeAliasConfigurer = new TypeAliasConfigurer();
            typeAliasConfigurer.TypeAliases = typeAliases;

            typeAliasConfigurer.Order = 1;

            SerializationTestUtils.SerializeAndDeserialize(typeAliasConfigurer);
        }

        [Test]
        public void UseInvalidTypeForDictionaryValue()
        {
            IDictionary typeAliases = new Hashtable();
            typeAliases.Add("LinkedList", new LinkedList());

            TypeAliasConfigurer typeAliasConfigurer = new TypeAliasConfigurer();
            typeAliasConfigurer.TypeAliases = typeAliases;

            Assert.Throws<ObjectInitializationException>(() => typeAliasConfigurer.PostProcessObjectFactory(factory));
        }

        [Test]
        public void UseNonResolvableTypeForDictionaryValue()
        {
            IDictionary typeAliases = new Hashtable();
            typeAliases.Add("LinkedList", "Spring.Collections.LinkkkedList");

            TypeAliasConfigurer typeAliasConfigurer = new TypeAliasConfigurer();
            typeAliasConfigurer.TypeAliases = typeAliases;

            Assert.Throws<ObjectInitializationException>(() => typeAliasConfigurer.PostProcessObjectFactory(factory));
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
            IApplicationContext ctx = new XmlApplicationContext("file://Spring/Objects/Factory/Config/TypeAliases.xml");

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

            // SPRNET-1119
            object obj4 = ctx.GetObject("testObject4");
            Assert.IsNotNull(obj4);
            Assert.AreEqual(typeof(TestObject), obj4.GetType());
            Assert.AreEqual("Bruno", ((TestObject)obj4).Name);
            Assert.AreEqual(30, ((TestObject)obj4).Age);


            object obj6 = ctx.GetObject("testObject6");
            Assert.IsNotNull(obj6);
            Assert.AreEqual(typeof(TestObject), obj6.GetType());
            Assert.AreEqual("name from section", ((TestObject)obj6).Name);
            Assert.AreEqual(27, ((TestObject)obj6).Age);


            object obj5 = ctx.GetObject("testObject5");
            Assert.IsNotNull(obj5);
            Assert.AreEqual(typeof(TestObject), obj5.GetType());
            Assert.AreEqual("overide-name", ((TestObject)obj5).Name);
            Assert.AreEqual(26, ((TestObject)obj5).Age);

            object vpc = ctx.GetObject("vpc");
            Assert.IsNotNull(vpc);
            Assert.AreEqual(typeof(VariablePlaceholderConfigurer), vpc.GetType());


        }

        private void CreateConfigurerAndTestLinkedList(IDictionary typeAliases)
        {
            TypeAliasConfigurer typeAliasConfigurer = new TypeAliasConfigurer();
            typeAliasConfigurer.TypeAliases = typeAliases;

            typeAliasConfigurer.Order = 1;


            typeAliasConfigurer.PostProcessObjectFactory(factory);

            //todo investigate mocking the typeregistry, for now ask the actual one for information.
            Assert.IsTrue(TypeRegistry.ContainsAlias("LinkedList"),
                          "TypeAliasConfigurer did not register a type alias with the TypeRegistry");

            Type linkedListType = TypeRegistry.ResolveType("LinkedList");
            Assert.IsTrue(linkedListType.Equals(typeof(LinkedList)), "Incorrect type resolved.");
            Assert.AreEqual(1,typeAliasConfigurer.Order);
        }
    }
}
