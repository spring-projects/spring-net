#region License

/*
 * Copyright 2004 the original author or authors.
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

using System.Collections.Generic;

using NUnit.Framework;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

#endregion

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Unit tests for the XmlListableObjectFactory class.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    [TestFixture]
    public class XmlListableObjectFactoryTests : AbstractListableObjectFactoryTests
    {
        #region Inner Class : AnonymousClassObjectPostProcessor

        private class AnonymousClassObjectPostProcessor : IObjectPostProcessor
        {
            public AnonymousClassObjectPostProcessor()
            {
            }

            public virtual object PostProcessBeforeInitialization(
                object obj, string name)
            {
                if (obj is TestObject)
                {
                    ((TestObject)obj).PostProcessed = true;
                }
                if (obj is DummyFactory)
                {
                    ((DummyFactory)obj).PostProcessed = true;
                }
                return obj;
            }

            public virtual object PostProcessAfterInitialization(
                object obj, string objectName)
            {
                return obj;
            }
        }

        #endregion

        protected internal override AbstractObjectFactory CreateObjectFactory(bool caseSensitive)
        {
            return new DefaultListableObjectFactory(caseSensitive);
        }

        private DefaultListableObjectFactory parent;
        //		private XmlObjectFactory factory;

        #region Test SetUp

        [SetUp]
        protected void SetUp()
        {
            parent = new DefaultListableObjectFactory();
            var m = new Dictionary<string, object>();
            m["name"] = "Albert";
            parent.RegisterObjectDefinition("father", new RootObjectDefinition(typeof(TestObject), new MutablePropertyValues(m)));
            m = new Dictionary<string, object>();
            m["name"] = "Roderick";
            parent.RegisterObjectDefinition("rod", new RootObjectDefinition(typeof(TestObject), new MutablePropertyValues(m)));

            // for testing dynamic ctor arguments + parent.GetObject() call propagation 
            parent.RegisterObjectDefinition("namedfather", new RootObjectDefinition(typeof(TestObject), false));
            parent.RegisterObjectDefinition("typedfather", new RootObjectDefinition(typeof(TestObject), false));

            // add unsupported IObjectDefinition implementation...
            //UnsupportedObjectDefinitionImplementation unsupportedDefinition = new UnsupportedObjectDefinitionImplementation();
            //parent.RegisterObjectDefinition("unsupportedDefinition", unsupportedDefinition);

            XmlObjectFactory factory;
            factory = new XmlObjectFactory(new ReadOnlyXmlTestResource("test.xml", GetType()), parent);

            // TODO: should this be allowed?
            //this.factory.RegisterObjectDefinition("typedfather", new RootObjectDefinition(typeof(object), false));
            factory.AddObjectPostProcessor(new AnonymousClassObjectPostProcessor());
            factory.AddObjectPostProcessor(new LifecycleObject.PostProcessor());

            factory.PreInstantiateSingletons();
            base.ObjectFactory = factory;
        }

        #endregion

        [Test]
        public virtual void FactoryNesting()
        {
            ITestObject father = (ITestObject)ObjectFactory.GetObject("father");
            Assert.IsTrue(father != null, "Object from root context");

            ITestObject rod = (ITestObject)ObjectFactory.GetObject("rod");
            Assert.IsTrue("Rod".Equals(rod.Name), "Object from child context");
            Assert.IsTrue(rod.Spouse == father, "Object has external reference");

            rod = (ITestObject)parent.GetObject("rod");
            Assert.IsTrue("Roderick".Equals(rod.Name), "Object from root context");
        }

        [Test]
        public virtual void FactoryReferences()
        {
            DummyReferencer dref = (DummyReferencer)ObjectFactory.GetObject("factoryReferencer");
            Assert.IsTrue(dref.TestObject1 == dref.TestObject2);
        }

        [Test]
        public virtual void PrototypeReferences()
        {
            // check that not broken by circular reference resolution mechanism
            DummyReferencer ref1 = (DummyReferencer)ObjectFactory.GetObject("prototypeReferencer");
            Assert.IsTrue(ref1.TestObject1 != ref1.TestObject2, "Not referencing same Object twice");
            DummyReferencer ref2 = (DummyReferencer)ObjectFactory.GetObject("prototypeReferencer");
            Assert.IsTrue(ref1 != ref2, "Not the same referencer");
            Assert.IsTrue(ref2.TestObject1 != ref2.TestObject2, "Not referencing same Object twice");
            Assert.IsTrue(ref1.TestObject1 != ref2.TestObject1, "Not referencing same Object twice");
            Assert.IsTrue(ref1.TestObject2 != ref2.TestObject2, "Not referencing same Object twice");
            Assert.IsTrue(ref1.TestObject1 != ref2.TestObject2, "Not referencing same Object twice");
        }

        [Test]
        public virtual void ObjectPostProcessor()
        {
            TestObject kerry = (TestObject)ObjectFactory.GetObject("kerry");
            TestObject kathy = (TestObject)ObjectFactory.GetObject("kathy");
            DummyFactory factory = (DummyFactory)ObjectFactory.GetObject("&singletonFactory");
            TestObject factoryCreated = (TestObject)ObjectFactory.GetObject("singletonFactory");
            Assert.IsTrue(kerry.PostProcessed);
            Assert.IsTrue(kathy.PostProcessed);
            Assert.IsTrue(factory.PostProcessed);
            Assert.IsTrue(factoryCreated.PostProcessed);
        }

        /// <summary>
        /// Test the number of singletons in test.xml
        /// </summary>
        [Test]
        public virtual void CountSingletons()
        {
            Assert.AreEqual(13, ObjectFactory.GetSingletonCount(), "Number of singletons incorrect");
        }

        [Test]
        public void CanRetrieveByType_Using_GetObjects_T_Method()
        {
            var objsByGenericMethod = ((DefaultListableObjectFactory)ObjectFactory).GetObjects<NameIdTestObject>();
            Assert.That(objsByGenericMethod.Count, Is.EqualTo(3));
        }

        [Test]
        public void CanRetrieveByType_Using_GetObjectsOfType_Method()
        {
            var objsByOldMethod = ((DefaultListableObjectFactory)ObjectFactory).GetObjectsOfType(typeof(NameIdTestObject));
            Assert.That(objsByOldMethod.Count, Is.EqualTo(3));
        }

        [Test]
        public void CanRetrieveAllNameIdObjectsByName()
        {
            Assert.That(ObjectFactory.GetObject("object1-with-same-id-and-name"), Is.Not.Null);
            Assert.That(ObjectFactory.GetObject("object2-with-same-id-and-name"), Is.Not.Null);
            Assert.That(ObjectFactory.GetObject("name-id-test-object-name"), Is.Not.Null);
        }

    }
}

namespace Spring.Objects
{
    public class NameIdTestObject
    {

    }

}