#region License

/*
 * Copyright 2002-2010 the original author or authors.
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
using NUnit.Framework;

using Spring.Core.IO;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Threading;
using System.Threading;
using System.Collections;

#endregion

namespace Spring.Objects.Factory
{
    /// <summary>
    /// Subclasses must override SetUp () to initialize the object factory
    /// and any other variables they need.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Rick Evans (.NET)</author>
    public abstract class AbstractObjectFactoryTests
    {
        #region Properties

        protected internal abstract AbstractObjectFactory CreateObjectFactory(bool caseSensitive);

        private AbstractObjectFactory cachedFactory;

        protected AbstractObjectFactory ObjectFactory
        {
            get { return cachedFactory; }
            set { cachedFactory = value; }
        }

        #endregion

        #region Case Insensitive Tests

        [Test]
        public void RespectsCaseInsensitiveNamesAndAliases()
        {
            AbstractObjectFactory of = CreateObjectFactory(false);

            object testObject = new object();
            of.RegisterSingleton("name", testObject);
            of.RegisterAlias("NAME", "alias");

            try
            {
                of.RegisterAlias("NaMe", "AlIaS");
                Assert.Fail();
            }
            catch (ObjectDefinitionStoreException ex)
            {
                Assert.IsTrue(-1 < ex.Message.IndexOf("already registered"));
            }

            Assert.AreEqual(1, of.GetAliases("nAmE").Count);
            Assert.AreEqual(testObject, of.GetObject("nAmE"));
            Assert.AreEqual(testObject, of.GetObject("ALIAS"));
        }

        #endregion

        /// <summary>
        /// Roderick objects inherits from rod, overriding name only.
        /// </summary>
        [Test]
        public void Inheritance()
        {
            Assert.IsTrue(ObjectFactory.ContainsObject("rod"));
            Assert.IsTrue(ObjectFactory.ContainsObject("roderick"));
            TestObject rod = (TestObject)ObjectFactory["rod"];
            TestObject roderick = (TestObject)ObjectFactory["roderick"];
            Assert.IsTrue(rod != roderick, "not == ");
            Assert.IsTrue(rod.Name.Equals("Rod"), "rod.name is Rod");
            Assert.IsTrue(rod.Age == 31, "rod.age is 31");
            Assert.IsTrue(roderick.Name.Equals("Roderick"), "roderick.name is Roderick");
            Assert.IsTrue(roderick.Age == rod.Age, "roderick.age was inherited");
        }

        [Test]
        public virtual void GetObjectWithNullName()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectFactory.GetObject(null));
        }

        /// <summary>
        /// Test that InitializingObject objects receive the AfterPropertiesSet () callback.
        /// </summary>
        [Test]
        public void InitializingObjectCallback()
        {
            MustBeInitialized mbi =
                (MustBeInitialized)ObjectFactory["mustBeInitialized"];
            // The dummy business method will throw an exception if the
            // AfterPropertiesSet () callback wasn't invoked
            mbi.BusinessMethod();
        }

        /// <summary>
        /// Test that InitializingObject/ObjectFactoryAware/DisposableObject objects
        /// receive the AfterPropertiesSet () callback before ObjectFactoryAware
        /// callbacks.
        /// </summary>
        [Test]
        public void LifecycleCallbacks()
        {
            LifecycleObject lb = (LifecycleObject)ObjectFactory.GetObject("lifecycle");
            Assert.AreEqual("lifecycle", lb.ObjectName);
            // The dummy business method will throw an exception if the
            // necessary callbacks weren't invoked in the right order
            lb.BusinessMethod();
            Assert.IsFalse(lb.Destroyed, "Was destroyed");
        }

        [Test(Description = "SPRNET-1208")]
        public void AddObjectFactoryOnObjectFactoryAwareObjectPostProcessors()
        {
            AbstractObjectFactory aof = ObjectFactory;
            LifecycleObject.PostProcessor lb = new LifecycleObject.PostProcessor();
            aof.AddObjectPostProcessor(lb);
            Assert.AreSame(aof, lb.ObjectFactory);
        }

        [Test]
        public void FindsValidInstance()
        {
            object o = ObjectFactory.GetObject("rod");
            Assert.IsTrue(o is TestObject, "Rod object is a TestObject");
            TestObject rod = (TestObject)o;
            Assert.IsTrue(rod.Name.Equals("Rod"), "rod.name is Rod");
            Assert.IsTrue(rod.Age == 31, "rod.age is 31");
        }

        [Test]
        public void GetInstanceByMatchingClass()
        {
            object o = ObjectFactory.GetObject("rod", typeof(TestObject));
            Assert.IsTrue(o is TestObject, "Rod object is a TestObject");
        }

        [Test]
        public void GetInstanceByNonmatchingClass()
        {
            try
            {
                ObjectFactory.GetObject("rod", typeof(IObjectFactory));
                Assert.Fail("Rod object is not of type IObjectFactory; GetObjectInstance(rod, typeof (IObjectFactory)) should throw ObjectNotOfRequiredTypeException");
            }
            catch (ObjectNotOfRequiredTypeException ex)
            {
                Assert.IsTrue(ex.ObjectName.Equals("rod"), "Exception has correct object name");
                Assert.IsTrue(ex.RequiredType.Equals(typeof(IObjectFactory)), "Exception requiredType must be ObjectFactory.class");
                Assert.IsTrue(typeof(TestObject).IsAssignableFrom(ex.ActualType), "Exception actualType as TestObject.class");
                Assert.IsTrue(ex.ActualInstance == ObjectFactory.GetObject("rod"), "Actual instance is correct");
            }
        }

        [Test]
        public virtual void GetSharedInstanceByMatchingClass()
        {
            object o = ObjectFactory.GetObject("rod", typeof(TestObject));
            Assert.IsTrue(o is TestObject, "Rod object is a TestObject");
        }

        [Test]
        public virtual void GetSharedInstanceByMatchingClassNoCatch()
        {
            object o = ObjectFactory.GetObject("rod", typeof(TestObject));
            Assert.IsTrue(o is TestObject, "Rod object is a TestObject");
        }

        [Test]
        public void GetSharedInstanceByNonmatchingClass()
        {
            try
            {
                ObjectFactory.GetObject("rod", typeof(IObjectFactory));
                Assert.Fail("Rod object is not of type ObjectFactory; getObjectInstance(rod, ObjectFactory.class) should throw ObjectNotOfRequiredTypeException");
            }
            catch (ObjectNotOfRequiredTypeException ex)
            {
                // So far, so good
                Assert.IsTrue(ex.ObjectName.Equals("rod"), "Exception has correct object name");
                Assert.IsTrue(ex.RequiredType.Equals(typeof(IObjectFactory)), "Exception requiredType must be IObjectFactory class");
                Assert.IsTrue(typeof(TestObject).IsAssignableFrom(ex.ActualType), "Exception actualType as TestObject class");
            }
            catch (Exception ex)
            {
                Assert.Fail("Shouldn't throw exception on getting valid instance with matching class : " + ex.Message);
            }
        }

        [Test]
        public virtual void SharedInstancesAreEqual()
        {
            try
            {
                object o = ObjectFactory.GetObject("rod");
                Assert.IsTrue(o is TestObject, "Rod object1 is a TestObject");
                object o1 = ObjectFactory.GetObject("rod");
                Assert.IsTrue(o1 is TestObject, "Rod object2 is a TestObject");
                Assert.IsTrue(o == o1, "Object equals applies");
            }
            catch
            {
                Assert.Fail("Shouldn't throw exception on getting valid instance");
            }
        }

        [Test]
        public void NotThere()
        {
            Assert.IsFalse(ObjectFactory.ContainsObject("Mr Squiggle"));
            Assert.Throws<NoSuchObjectDefinitionException>(() => ObjectFactory.GetObject("Mr Squiggle"));
        }

        [Test]
        public void ValidEmpty()
        {
            try
            {
                object o = ObjectFactory.GetObject("validEmpty");
                Assert.IsTrue(o is TestObject, "validEmpty object is a TestObject");
                TestObject ve = (TestObject)o;
                Assert.IsTrue(ve.Name == null && ve.Age == 0 && ve.Spouse == null, "Valid empty has defaults");
            }
            catch
            {
                Assert.Fail("Shouldn't throw exception on valid empty");
            }
        }

        [Test]
        public void RegisterNullCustomTypeConverter()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectFactory.RegisterCustomConverter(null, null));
        }

        [Test]
        public virtual void TypeMismatch()
        {
            try
            {
                ObjectFactory.GetObject("typeMismatch");
                Assert.Fail("Shouldn't succeed with type mismatch");
            }
            catch (ObjectCreationException wex)
            {
                Assert.IsTrue(wex.InnerException is PropertyAccessExceptionsException);
                PropertyAccessExceptionsException ex = (PropertyAccessExceptionsException)wex.InnerException;
                // Furthers
                Assert.IsTrue(ex.ExceptionCount == 1, "Has one error");
                Assert.IsTrue(ex.GetPropertyAccessException("age") != null, "Error is for field age");

                TestObject tb = (TestObject)ex.ObjectWrapper.WrappedInstance;
                Assert.IsTrue(tb.Age == 0, "Age still has default");
                Assert.IsTrue(ex.GetPropertyAccessException("age").PropertyChangeArgs.NewValue.Equals("34x"), "We have rejected age in exception");
                Assert.IsTrue(tb.Name.Equals("typeMismatch"), "valid name stuck");
                Assert.IsTrue(tb.Spouse.Name.Equals("Rod"), "valid spouse stuck");
            }
        }

        [Test]
        public virtual void GrandParentDefinitionFoundInObjectFactory()
        {
            TestObject dad = (TestObject)ObjectFactory.GetObject("father");
            Assert.IsTrue(dad.Name.Equals("Albert"), "Dad has correct name");
        }

        [Test]
        public virtual void GrandParentDefinitionFoundInObjectFactoryWithType()
        {
            TestObject dad = (TestObject)ObjectFactory.GetObject("typedfather", typeof(TestObject));
            Assert.AreEqual(null, dad.Name, "Dad has not correct name");
        }

        [Test]
        public virtual void GrandParentDefinitionFoundInObjectFactoryWithArguments()
        {
            TestObject dad = (TestObject)ObjectFactory.GetObject("namedfather", new object[] { "Hugo", 65 });
            Assert.AreEqual("Hugo", dad.Name, "Dad has not correct name");
            Assert.AreEqual(65, dad.Age, "Dad has not correct age");
        }

        [Test]
        public virtual void GrandParentDefinitionFoundInObjectFactoryWithTypeAndArguments()
        {
            TestObject dad = (TestObject)ObjectFactory.GetObject("typedfather", typeof(TestObject), new object[] { "Chris", 66 });
            Assert.AreEqual("Chris", dad.Name, "Dad has not correct name");
            Assert.AreEqual(66, dad.Age, "Dad has not correct age");
        }

        [Test(Description = "Extra check that the type is really passed on to the parent factory")]
        public virtual void GrandParentDefinitionFoundInObjectFactoryWithTypeAndArgumentsWithWrongType()
        {
            try
            {
                TestObject dad = (TestObject)ObjectFactory.GetObject("typedfather", typeof(string), new object[] { "Chris", 66 });
                Assert.Fail("should throw ObjectNotOfRequiredTypeException");
            }
            catch (ObjectNotOfRequiredTypeException)
            {
            }
        }

        [Test]
        public virtual void FactorySingleton()
        {
            Assert.IsTrue(ObjectFactory.IsSingleton("&singletonFactory"));
            Assert.IsTrue(ObjectFactory.IsSingleton("singletonFactory"));
            TestObject tb = (TestObject)ObjectFactory.GetObject("singletonFactory");
            Assert.IsTrue(tb.Name.Equals(DummyFactory.SINGLETON_NAME), "Singleton from factory has correct name, not " + tb.Name);
            DummyFactory factory = (DummyFactory)ObjectFactory.GetObject("&singletonFactory");
            TestObject tb2 = (TestObject)ObjectFactory.GetObject("singletonFactory");
            Assert.IsTrue(tb == tb2, "Singleton references ==");
            Assert.IsTrue(factory.ObjectFactory != null, "FactoryObject is ObjectFactoryAware");
        }

        [Test]
        public virtual void FactoryPrototype()
        {
            Assert.IsTrue(ObjectFactory.IsSingleton("&prototypeFactory"));
            Assert.IsFalse(ObjectFactory.IsSingleton("prototypeFactory"));
            TestObject tb = (TestObject)ObjectFactory.GetObject("prototypeFactory");
            Assert.IsTrue(!tb.Name.Equals(DummyFactory.SINGLETON_NAME));
            TestObject tb2 = (TestObject)ObjectFactory.GetObject("prototypeFactory");
            Assert.IsTrue(tb != tb2, "Prototype references !=");
        }

        /// <summary>
        /// Check that we can get the factory object itself.
        /// This is only possible if we're dealing with a factory
        /// </summary>
        [Test]
        public virtual void GetFactoryItself()
        {
            DummyFactory factory = (DummyFactory)ObjectFactory.GetObject("&singletonFactory");
            Assert.IsTrue(factory != null);
        }

        /// <summary>Check that AfterPropertiesSet gets called on factory.</summary>
        [Test]
        public virtual void FactoryIsInitialized()
        {
            TestObject tb = (TestObject)ObjectFactory.GetObject("singletonFactory");
            DummyFactory factory = (DummyFactory)ObjectFactory.GetObject("&singletonFactory");
            Assert.IsTrue(factory.WasInitialized, "Factory was not initialized even though it implemented IInitializingObject");
        }

        /// <summary>
        /// It should be illegal to dereference a normal object as a factory.
        /// </summary>
        [Test]
        public virtual void RejectsFactoryGetOnNormalObject()
        {
            Assert.Throws<ObjectIsNotAFactoryException>(() => ObjectFactory.GetObject("&rod"));
        }

        [Test]
        public virtual void Aliasing()
        {
            string alias = "rods alias";
            try
            {
                ObjectFactory.GetObject(alias);
                Assert.Fail("Shouldn't permit factory get on normal object");
            }
            catch (NoSuchObjectDefinitionException ex)
            {
                Assert.IsTrue(alias.Equals(ex.ObjectName));
            }

            // Create alias
            ObjectFactory.RegisterAlias("rod", alias);
            object rod = ObjectFactory.GetObject("rod");
            object aliasRod = ObjectFactory.GetObject(alias);
            Assert.IsTrue(rod == aliasRod);
            Assert.Throws<ObjectDefinitionStoreException>(() => ObjectFactory.RegisterAlias("father", alias));
        }

        [Test]
        public void RegisterSingletonWithEmptyName()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectFactory.RegisterSingleton(Environment.NewLine, DBNull.Value));
        }

        [Test]
        public void RegisterSingletonWithNullName()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectFactory.RegisterSingleton(null, DBNull.Value));
        }

        [Test]
        public void GetSingletonNamesReflectsOrderOfRegistration()
        {
            AbstractObjectFactory of;
            of = CreateObjectFactory(true);
            of.RegisterSingleton("A", new object());
            of.RegisterSingleton("C", new object());
            of.RegisterSingleton("B", new object());
            Assert.AreEqual(new string[] { "A", "C", "B" }, of.GetSingletonNames());

            of = CreateObjectFactory(false);
            of.RegisterSingleton("A", new object());
            of.RegisterSingleton("C", new object());
            of.RegisterSingleton("B", new object());
            Assert.AreEqual(new string[] { "A", "C", "B" }, of.GetSingletonNames(typeof(object)));
        }

        [Test]
        public void ContainsSingletonWithEmptyName()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectFactory.ContainsSingleton(Environment.NewLine));
        }

        [Test]
        public void ContainsSingletonWithNullName()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectFactory.ContainsSingleton(null));
        }

        [Test]
        public void AliasWithEmptyName()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectFactory.RegisterAlias(Environment.NewLine, "the whipping boy"));
        }

        [Test]
        public void AliasWithEmptyAlias()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectFactory.RegisterAlias("rick", Environment.NewLine));
        }

        //		[Test]
        //		[ExpectedException(typeof(ObjectDefinitionStoreException))]
        //		public void ChokesIfNotGivenSupportedIObjectDefinitionImplementation() 
        //		{
        //			ObjectFactory.GetObject("unsupportedDefinition");
        //		}

        /// <summary>
        /// This test resembles a scenario that may happen e.g. using ProxyFactoryObject proxying sibling objects with cyclic dependencies
        /// </summary>
        [Test]
        public void CanResolveCyclicSingletonFactoryObjectProductDependencies()
        {
            AbstractObjectFactory of = this.CreateObjectFactory(true);

            GenericObjectDefinition od = new GenericObjectDefinition();
            od.ObjectTypeName = typeof(TestObject).FullName;
            od.IsSingleton = true;
            od.PropertyValues.Add(new PropertyValue("Spouse", new RuntimeObjectReference("product2")));
            of.RegisterObjectDefinition("product1Target", od);

            GenericObjectDefinition od2 = new GenericObjectDefinition();
            od2.ObjectTypeName = typeof(TestObject).FullName;
            od2.IsSingleton = true;
            od2.PropertyValues.Add(new PropertyValue("Sibling", new RuntimeObjectReference("product1")));
            of.RegisterObjectDefinition("product2Target", od2);

            of.RegisterSingleton("product1", new ObjectReferenceFactoryObject("product1Target", of));
            of.RegisterSingleton("product2", new ObjectReferenceFactoryObject("product2Target", of));

            TestObject to = (TestObject)of.GetObject("product1");
            Assert.NotNull(to);
            Assert.NotNull(to.Spouse);
            Assert.NotNull(((TestObject)to.Spouse).Sibling);
        }

        [Test]
        public void ThrowsOnCyclicDependenciesOnNonSingletons()
        {
            AbstractObjectFactory of = this.CreateObjectFactory(true);

            GenericObjectDefinition od = new GenericObjectDefinition();
            od.ObjectTypeName = typeof(TestObject).FullName;
            od.IsSingleton = false;
            od.PropertyValues.Add(new PropertyValue("Spouse", new RuntimeObjectReference("product2")));
            of.RegisterObjectDefinition("product1", od);

            GenericObjectDefinition od2 = new GenericObjectDefinition();
            od2.ObjectTypeName = typeof(TestObject).FullName;
            od2.IsSingleton = false;
            od2.PropertyValues.Add(new PropertyValue("Sibling", new RuntimeObjectReference("product1")));
            of.RegisterObjectDefinition("product2", od2);

            try
            {
                of.GetObject("product1");
                Assert.Fail();
            }
            catch (ObjectCurrentlyInCreationException ex)
            {
                Assert.AreEqual("product1", ex.ObjectName);
            }
        }

        private void GetTheTestObject()
        {
            if (DateTime.Now.Millisecond % 2 == 0)
            {
                ObjectFactory.GetObject("theObject");
            }
            else
            {
                ObjectFactory.GetObject("theSpouse");
            }
        }

        [Test]
        public void GetObjectIsThreadSafe()
        {
            ObjectFactory = CreateObjectFactory(true);

            GenericObjectDefinition theSpouse = new GenericObjectDefinition();
            theSpouse.ObjectTypeName = typeof(TestObject).FullName;
            theSpouse.IsSingleton = false;
            ObjectFactory.RegisterObjectDefinition("theSpouse", theSpouse);

            GenericObjectDefinition theObject = new GenericObjectDefinition();
            theObject.ObjectTypeName = typeof(TestObject).FullName;
            theObject.IsSingleton = false;
            theObject.PropertyValues.Add("Spouse", theSpouse);
            ObjectFactory.RegisterObjectDefinition("theObject", theObject);

            AsyncTestTask t1 = new AsyncTestMethod(20000, new ThreadStart(GetTheTestObject)).Start();
            AsyncTestTask t2 = new AsyncTestMethod(20000, new ThreadStart(GetTheTestObject)).Start();
            AsyncTestTask t3 = new AsyncTestMethod(20000, new ThreadStart(GetTheTestObject)).Start();
            AsyncTestTask t4 = new AsyncTestMethod(20000, new ThreadStart(GetTheTestObject)).Start();

            t1.AssertNoException();
            t2.AssertNoException();
            t3.AssertNoException();
            t4.AssertNoException();
        }

    }

    [TestFixture]
    public class SPRNET_1334
    {
        public static AbstractObjectFactory CreateObjectFactory(bool caseSensitive)
        {
            return new DefaultListableObjectFactory(caseSensitive);
        }

        [Test]
        public void CanDisposeFactoryWhenDependentObjectCallsFactoryInDispose()
        {
            AbstractObjectFactory factory = CreateObjectFactory(false);
            ConfigureObjectFactory(factory as IObjectDefinitionRegistry);

            ParentClass parent = (ParentClass)factory.GetObject("Parent");
            Assert.That(parent, Is.Not.Null);

            DisposableClass innerObject = (DisposableClass)parent.InnerObject;
            innerObject.ObjectFactory = factory;

            factory.Dispose();

            Assert.Pass("Test concluded successfully.");
        }

        private void ConfigureObjectFactory(IObjectDefinitionRegistry factory)
        {
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new StringResource(@"<?xml version='1.0' encoding='UTF-8' ?>
                <objects xmlns='http://www.springframework.net'
                          xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                          xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>

                      <object id='Parent' type='Spring.Objects.Factory.SPRNET_1334+ParentClass, Spring.Core.Tests'>
                        <property name='Name' value='Foo!'/>
			                <property name='InnerObject'>
				                <object type='Spring.Objects.Factory.SPRNET_1334+DisposableClass, Spring.Core.Tests'/>
			                </property>
                      </object>
<!--
                      <object id='Parent' type='Spring.Objects.Factory.SPRNET_1334+ParentClass, Spring.Core.Tests'>
                        <property name='Name' value='Foo!'/>
			                <property name='InnerObject' ref='Inner'/>
                      </object>

                      <object id='Inner' type='Spring.Objects.Factory.SPRNET_1334+DisposableClass, Spring.Core.Tests'/>
-->			          
                      
                </objects>
            "));
        }

        public class ParentClass
        {
            private string _name;

            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }

            private IDisposable _innerObject;

            public IDisposable InnerObject
            {
                get { return _innerObject; }
                set { _innerObject = value; }
            }
        }

        public class DisposableClass : IDisposable
        {
            private AbstractObjectFactory _objectFactory;
            public AbstractObjectFactory ObjectFactory
            {
                get { return _objectFactory; }
                set { _objectFactory = value; }
            }

            public void Dispose()
            {
                Console.WriteLine("DisposableClass.Dispose()");
                if (ObjectFactory == null)
                    return;

                object parent = ObjectFactory.GetObject("Parent");
                if (parent == null)
                    Console.WriteLine("parent == null");
            }

        }
    }

    [TestFixture]
    public class SPRNET_1338
    {
        private static AbstractObjectFactory _cachedFactory;

        protected static AbstractObjectFactory ObjectFactory
        {
            get { return _cachedFactory; }
            set { _cachedFactory = value; }
        }

        [SetUp]
        public void _SetUp()
        {
            ObjectFactory = CreateObjectFactory(true);

            GenericObjectDefinition threadCreatorInsideConstructor = new GenericObjectDefinition();
            threadCreatorInsideConstructor.ObjectTypeName = typeof(ThreadCreatorInsideConstructor).FullName;
            threadCreatorInsideConstructor.IsSingleton = true;
            ObjectFactory.RegisterObjectDefinition("threadCreatorInsideConstructor", threadCreatorInsideConstructor);

            GenericObjectDefinition threadCreatorInsideDispose = new GenericObjectDefinition();
            threadCreatorInsideDispose.ObjectTypeName = typeof(ThreadCreatorInsideDispose).FullName;
            threadCreatorInsideDispose.IsSingleton = true;
            ObjectFactory.RegisterObjectDefinition("threadCreatorInsideDispose", threadCreatorInsideDispose);
        }

        [Test]
        [Ignore("Test fails -- waiting for verification re: if bug exists in Java impl")]
        public void CanAvoidLockContentionDuringObjectFactoryDisposal()
        {
            Thread t = new Thread(CreateThreadContentionFromDispose);
            t.Start();
            t.Join(20000);

            if (t.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                Assert.Fail("Lock Contention Blocked Successful Dispose of ObjectFactory!");
        }

        [Test]
        public void CanAvoidLockContentionDuringObjectInstantiation()
        {
            Thread t = new Thread(CreateThreadContentionFromConstructor);
            t.Start();
            t.Join(20000);

            if (t.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                Assert.Fail("Lock Contention Blocked Successful Instantiation of Singleton Test Object!");
        }

        public static AbstractObjectFactory CreateObjectFactory(bool caseSensitive)
        {
            return new DefaultListableObjectFactory(caseSensitive);
        }

        private void CreateThreadContentionFromConstructor()
        {
            ObjectFactory.GetObject("threadCreatorInsideConstructor");
        }

        private void CreateThreadContentionFromDispose()
        {
            ObjectFactory.GetObject("threadCreatorInsideDispose");
            ObjectFactory.Dispose();
        }

        internal class ThreadCreatorInsideDispose : IDisposable
        {
            public ThreadCreatorInsideDispose()
            {
            }

            public void Dispose()
            {
                Thread t = new Thread(ConcurentThreadDisposeProc);
                t.Start();
                t.Join();
            }

            private static void ConcurentThreadDisposeProc()
            {
                ObjectFactory.GetObject("threadCreatorInsideConstructor");
            }
        }

        internal class ThreadCreatorInsideConstructor
        {
            public ThreadCreatorInsideConstructor()
            {
                Thread t = new Thread(ConcurentThreadProc);
                t.Start();
                t.Join();
            }

            private static void ConcurentThreadProc()
            {
                ObjectFactory.GetObject("threadCreatorInsideDispose");
            }
        }

    }

    [TestFixture]
    public class SPRNET_1315
    {
        private static AbstractObjectFactory _cachedFactory;

        private static int _childCounter;

        private static int _parentCounter;

        private static ArrayList invocationLog = new ArrayList();

        protected static AbstractObjectFactory ObjectFactory
        {
            get { return _cachedFactory; }
            set { _cachedFactory = value; }
        }

        [SetUp]
        public void _Setup()
        {
            ObjectFactory = new DefaultListableObjectFactory(true);
            invocationLog.Clear();
            _parentCounter = 0;
            _childCounter = 0;
        }

        [Test]
        public void When_ParentAndChildArePrototypes_ConstructorInjection_DoesNotEnforceDestructionOrder()
        {
            WireParentAndChildWWithImpliedDependencyByConstructorInjection(false, false);

            Parent theParent = ObjectFactory.GetObject("parent") as Parent;
            theParent.Dispose();

            Assert.AreEqual(0, _parentCounter, "Should have no remaining parent objects after dispose");
            Assert.AreEqual(1, _childCounter, "Should have exactly ONE child object");
            Assert.IsNotNull(theParent.InjectedChild, "Parent's child dependency not set as expected");
            Assert.AreEqual("Parent Destructor", invocationLog[2], "Parent Destructor wasn't called third!");
            Assert.AreEqual(3, invocationLog.Count, "Should have no further object lifecycle behavior after parent destruction!");
        }

        [Test]
        public void When_ParentAndChildArePrototypes_ConstructorInjection_EnforcesConstructionOrder()
        {
            WireParentAndChildWWithImpliedDependencyByConstructorInjection(false, false);

            Parent theParent = ObjectFactory.GetObject("parent") as Parent;

            Assert.AreEqual(1, _parentCounter, "Should have exactly ONE parent object");
            Assert.AreEqual(1, _childCounter, "Should have exactly ONE child object");
            Assert.IsNotNull(theParent.InjectedChild, "Parent's child dependency not set as expected");
            Assert.AreEqual("Child Constructor", invocationLog[0], "Child Constructor wasn't called first!");
            Assert.AreEqual("Parent Constructor", invocationLog[1], "Parent Constructor wasn't called second!");

        }

        [Test]
        public void When_ParentAndChildArePrototypes_DependsOn_DoesNotEnforceDestructionOrder()
        {
            WireParentAndChildWithDependsOnDeclarationDependency(false, false);

            Parent theParent = ObjectFactory.GetObject("parent") as Parent;
            theParent.Dispose();

            Assert.AreEqual(0, _parentCounter, "Should have no remaining parent objects after dispose");
            Assert.AreEqual(1, _childCounter, "Should have exactly ONE child object");

            Assert.AreEqual("Parent Destructor", invocationLog[2], "Parent Destructor wasn't called third!");
            Assert.AreEqual(3, invocationLog.Count, "Should have no further object lifecycle behavior after parent destruction!");
        }

        [Test]
        public void When_ParentAndChildArePrototypes_DependsOn_EnforcesConstructionOrder()
        {
            WireParentAndChildWithDependsOnDeclarationDependency(false, false);

            Parent theParent = ObjectFactory.GetObject("parent") as Parent;

            Assert.AreEqual(1, _parentCounter, "Should have exactly ONE parent object");
            Assert.AreEqual(1, _childCounter, "Should have exactly ONE child object");
            Assert.AreEqual("Child Constructor", invocationLog[0], "Child Constructor wasn't called first!");
            Assert.AreEqual("Parent Constructor", invocationLog[1], "Parent Constructor wasn't called second!");

        }

        [Test]
        public void When_ParentAndChildAreSingletons_ConstructorInjection_EnforcesDestructionOrder()
        {
            WireParentAndChildWWithImpliedDependencyByConstructorInjection(true, true);

            //triger the construction of the singletons
            ObjectFactory.GetObject("parent");

            //trigger the disposal of the singletons
            ObjectFactory.Dispose();

            Assert.AreEqual(0, _parentCounter, "Should have no remaining parent objects after dispose");
            Assert.AreEqual(0, _childCounter, "Should have no remaining child objects after dispose");
            Assert.AreEqual("Parent Destructor", invocationLog[2], "Parent Destructor wasn't called third!");
            Assert.AreEqual("Child Destructor", invocationLog[3], "Child Destructor wasn't called fourth!");
            Assert.AreEqual(4, invocationLog.Count, "Should have no further object lifecycle behavior after parent destruction!");
        }

        [Test]
        public void When_ParentAndChildAreSingletons_DependsOn_EnforcesDestructionOrder()
        {
            WireParentAndChildWithDependsOnDeclarationDependency(true, true);

            //triger the construction of the singletons
            ObjectFactory.GetObject("parent");

            //make certain they are created successfully
            Assert.AreEqual(1, _parentCounter, "Should have exactly ONE parent object");
            Assert.AreEqual(1, _childCounter, "Should have exactly ONE child object");

            //trigger the disposal of the singletons
            ObjectFactory.Dispose();

            Assert.AreEqual(0, _parentCounter, "Should have no remaining parent objects after dispose");
            Assert.AreEqual(0, _childCounter, "Should have no remaining child objects after dispose");
            Assert.AreEqual("Parent Destructor", invocationLog[2], "Parent Destructor wasn't called third!");
            Assert.AreEqual("Child Destructor", invocationLog[3], "Child Destructor wasn't called fourth!");
            Assert.AreEqual(4, invocationLog.Count, "Should have no further object lifecycle behavior after child destruction!");
        }

        [Test]
        public void When_ParentIsProttpyeAndChildIsSingleton_ConstructorInjection_DoesNotEnforcesDestructionOrder()
        {
            WireParentAndChildWWithImpliedDependencyByConstructorInjection(false, true);

            //triger the construction of the singletons
            ObjectFactory.GetObject("parent");

            //trigger the disposal of the singletons
            ObjectFactory.Dispose();

            Assert.AreEqual(1, _parentCounter, "Should have ONE remaining parent objects after dispose");
            Assert.AreEqual(0, _childCounter, "Should have no remaining child objects after dispose");
            Assert.AreEqual("Child Destructor", invocationLog[2], "Child Destructor wasn't called third!");
            Assert.AreEqual(3, invocationLog.Count, "Should have no further object lifecycle behavior after child destruction!");
        }

        [Test]
        public void When_ParentIsSingletonAndChildIsPrototype_ConstructorInjection_DoesNotEnforcesDestructionOrder()
        {
            WireParentAndChildWWithImpliedDependencyByConstructorInjection(true, false);

            //triger the construction of the singletons
            ObjectFactory.GetObject("parent");

            //trigger the disposal of the singletons
            ObjectFactory.Dispose();

            Assert.AreEqual(0, _parentCounter, "Should have no remaining parent objects after dispose");
            Assert.AreEqual(1, _childCounter, "Should have ONE remaining child object after dispose");
            Assert.AreEqual("Parent Destructor", invocationLog[2], "Child Destructor wasn't called third!");
            Assert.AreEqual(3, invocationLog.Count, "Should have no further object lifecycle behavior after parent destruction!");
        }

        [Test]
        public void When_ParentIsSingletonAndChildIsPrototype_DependsOn_EnforcesDestructionOrder()
        {
            WireParentAndChildWithDependsOnDeclarationDependency(true, false);

            //triger the construction of the singletons
            ObjectFactory.GetObject("parent");

            //make certain they are created successfully
            Assert.AreEqual(1, _parentCounter, "Should have exactly ONE parent object");
            Assert.AreEqual(1, _childCounter, "Should have exactly ONE child object");

            //trigger the disposal of the singletons
            ObjectFactory.Dispose();

            Assert.AreEqual(0, _parentCounter, "Should have no remaining parent objects after dispose");
            Assert.AreEqual(1, _childCounter, "Should have no remaining child objects after dispose");
            Assert.AreEqual("Parent Destructor", invocationLog[2], "Parent Destructor wasn't called third!");
            Assert.AreEqual(3, invocationLog.Count, "Should have no further object lifecycle behavior after parent destruction!");
        }

        private void WireParentAndChildWithDependsOnDeclarationDependency(bool parentIsSingleton, bool childIsSingleton)
        {
            GenericObjectDefinition child = new GenericObjectDefinition();
            child.ObjectTypeName = typeof(Child).FullName;
            child.IsSingleton = childIsSingleton;
            ObjectFactory.RegisterObjectDefinition("child", child);

            GenericObjectDefinition parent = new GenericObjectDefinition();
            parent.ObjectTypeName = typeof(Parent).FullName;
            parent.IsSingleton = parentIsSingleton;
            parent.DependsOn = new string[] { "child" };
            ObjectFactory.RegisterObjectDefinition("parent", parent);
        }

        private static void WireParentAndChildWWithImpliedDependencyByConstructorInjection(bool parentIsSingleton, bool childIsSingleton)
        {
            GenericObjectDefinition child = new GenericObjectDefinition();
            child.ObjectTypeName = typeof(Child).FullName;
            child.IsSingleton = childIsSingleton;
            ObjectFactory.RegisterObjectDefinition("child", child);

            GenericObjectDefinition parent = new GenericObjectDefinition();
            parent.ObjectTypeName = typeof(Parent).FullName;
            parent.IsSingleton = parentIsSingleton;
            parent.ConstructorArgumentValues.AddIndexedArgumentValue(0, new RuntimeObjectReference("child"));
            ObjectFactory.RegisterObjectDefinition("parent", parent);
        }

        public class Parent : IDisposable
        {

            private Child _child;

            public Parent()
                : this(null)
            {
            }

            public Parent(Child child)
            {
                _child = child;
                _parentCounter++;
                invocationLog.Add("Parent Constructor");
            }

            public Child InjectedChild
            {
                get { return _child; }
            }

            public void Dispose()
            {
                _parentCounter--;
                invocationLog.Add("Parent Destructor");
            }
        }

        public class Child : IDisposable
        {
            public Child()
            {
                _childCounter++;
                invocationLog.Add("Child Constructor");
            }

            public void Dispose()
            {
                _childCounter--;
                invocationLog.Add("Child Destructor");
            }
        }

    }
}
