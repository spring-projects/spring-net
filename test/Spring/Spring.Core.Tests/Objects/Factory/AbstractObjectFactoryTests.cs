#region License

/*
 * Copyright 2002-2004 the original author or authors.
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
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

#endregion

namespace Spring.Objects.Factory
{
    public class TestAbstractObjectFactory : AbstractObjectFactory
    {
        protected override void DestroyObject(string name, object target)
        {
            throw new System.NotImplementedException();
        }

        public override bool ContainsObjectDefinition(string name)
        {
            throw new System.NotImplementedException();
        }

        public override IObjectDefinition GetObjectDefinition(string name)
        {
            throw new System.NotImplementedException();
        }

        public override IObjectDefinition GetObjectDefinition(string name, bool includeAncestors)
        {
            throw new System.NotImplementedException();
        }

        public override object ConfigureObject(object target, string name)
        {
            throw new System.NotImplementedException();
        }

        public override object ConfigureObject(object target, string name, IObjectDefinition definition)
        {
            throw new System.NotImplementedException();
        }

        protected override object InstantiateObject(string name, RootObjectDefinition definition, object[] arguments,
                                               bool allowEagerCaching, bool suppressConfigure)
        {
            throw new NotImplementedException();
        }
    }

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
                Assert.IsTrue( -1<ex.Message.IndexOf("already registered") );
            }

            Assert.AreEqual(1, of.GetAliases("nAmE").Length);
            Assert.AreEqual(testObject, of.GetObject("nAmE"));
            Assert.AreEqual(testObject, of.GetObject("ALIAS"));
        }

        #endregion

        #region Tests

        /// <summary>
		/// Roderick objects inherits from rod, overriding name only.
		/// </summary>
		[Test]
		public void Inheritance()
		{
			Assert.IsTrue(ObjectFactory.ContainsObject("rod"));
			Assert.IsTrue(ObjectFactory.ContainsObject("roderick"));
			TestObject rod = (TestObject) ObjectFactory["rod"];
			TestObject roderick = (TestObject) ObjectFactory["roderick"];
			Assert.IsTrue(rod != roderick, "not == ");
			Assert.IsTrue(rod.Name.Equals("Rod"), "rod.name is Rod");
			Assert.IsTrue(rod.Age == 31, "rod.age is 31");
			Assert.IsTrue(roderick.Name.Equals("Roderick"), "roderick.name is Roderick");
			Assert.IsTrue(roderick.Age == rod.Age, "roderick.age was inherited");
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public virtual void GetObjectWithNullName()
		{
			ObjectFactory.GetObject(null);
		}

		/// <summary>
		/// Test that InitializingObject objects receive the AfterPropertiesSet () callback.
		/// </summary>
		[Test]
		public void InitializingObjectCallback()
		{
			MustBeInitialized mbi =
				(MustBeInitialized) ObjectFactory["mustBeInitialized"];
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
			LifecycleObject lb = (LifecycleObject) ObjectFactory.GetObject("lifecycle");
			Assert.AreEqual("lifecycle", lb.ObjectName);
			// The dummy business method will throw an exception if the
			// necessary callbacks weren't invoked in the right order
			lb.BusinessMethod();
			Assert.IsFalse(lb.Destroyed, "Was destroyed");
		}

		[Test]
		public void FindsValidInstance()
		{
			object o = ObjectFactory.GetObject("rod");
			Assert.IsTrue(o is TestObject, "Rod object is a TestObject");
			TestObject rod = (TestObject) o;
			Assert.IsTrue(rod.Name.Equals("Rod"), "rod.name is Rod");
			Assert.IsTrue(rod.Age == 31, "rod.age is 31");
		}

		[Test]
		public void GetInstanceByMatchingClass()
		{
			object o = ObjectFactory.GetObject("rod", typeof (TestObject));
			Assert.IsTrue(o is TestObject, "Rod object is a TestObject");
		}

		[Test]
		public void GetInstanceByNonmatchingClass()
		{
			try
			{
				ObjectFactory.GetObject("rod", typeof (IObjectFactory));
				Assert.Fail("Rod object is not of type IObjectFactory; GetObjectInstance(rod, typeof (IObjectFactory)) should throw ObjectNotOfRequiredTypeException");
			}
			catch (ObjectNotOfRequiredTypeException ex)
			{
				Assert.IsTrue(ex.ObjectName.Equals("rod"), "Exception has correct object name");
				Assert.IsTrue(ex.RequiredType.Equals(typeof (IObjectFactory)), "Exception requiredType must be ObjectFactory.class");
				Assert.IsTrue(typeof (TestObject).IsAssignableFrom(ex.ActualType), "Exception actualType as TestObject.class");
				Assert.IsTrue(ex.ActualInstance == ObjectFactory.GetObject("rod"), "Actual instance is correct");
			}
		}

		[Test]
		public virtual void GetSharedInstanceByMatchingClass()
		{
			object o = ObjectFactory.GetObject("rod", typeof (TestObject));
			Assert.IsTrue(o is TestObject, "Rod object is a TestObject");
		}

		[Test]
		public virtual void GetSharedInstanceByMatchingClassNoCatch()
		{
			object o = ObjectFactory.GetObject("rod", typeof (TestObject));
			Assert.IsTrue(o is TestObject, "Rod object is a TestObject");
		}

		[Test]
		public void GetSharedInstanceByNonmatchingClass()
		{
			try
			{
				ObjectFactory.GetObject("rod", typeof (IObjectFactory));
				Assert.Fail("Rod object is not of type ObjectFactory; getObjectInstance(rod, ObjectFactory.class) should throw ObjectNotOfRequiredTypeException");
			}
			catch (ObjectNotOfRequiredTypeException ex)
			{
				// So far, so good
				Assert.IsTrue(ex.ObjectName.Equals("rod"), "Exception has correct object name");
				Assert.IsTrue(ex.RequiredType.Equals(typeof (IObjectFactory)), "Exception requiredType must be IObjectFactory class");
				Assert.IsTrue(typeof (TestObject).IsAssignableFrom(ex.ActualType), "Exception actualType as TestObject class");
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
		[ExpectedException(typeof (NoSuchObjectDefinitionException))]
		public void NotThere()
		{
			Assert.IsFalse(ObjectFactory.ContainsObject("Mr Squiggle"));
			ObjectFactory.GetObject("Mr Squiggle");
		}

		[Test]
		public void ValidEmpty()
		{
			try
			{
				object o = ObjectFactory.GetObject("validEmpty");
				Assert.IsTrue(o is TestObject, "validEmpty object is a TestObject");
				TestObject ve = (TestObject) o;
				Assert.IsTrue(ve.Name == null && ve.Age == 0 && ve.Spouse == null, "Valid empty has defaults");
			}
			catch
			{
				Assert.Fail("Shouldn't throw exception on valid empty");
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RegisterNullCustomTypeConverter()
		{
			AbstractObjectFactory fac = ObjectFactory as AbstractObjectFactory;
			if(fac != null) 
			{
				fac.RegisterCustomConverter(null, null);
			}
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
				PropertyAccessExceptionsException ex = (PropertyAccessExceptionsException) wex.InnerException;
				// Furthers
				Assert.IsTrue(ex.ExceptionCount == 1, "Has one error");
				Assert.IsTrue(ex.GetPropertyAccessException("age") != null, "Error is for field age");

				TestObject tb = (TestObject) ex.ObjectWrapper.WrappedInstance;
				Assert.IsTrue(tb.Age == 0, "Age still has default");
				Assert.IsTrue(ex.GetPropertyAccessException("age").PropertyChangeArgs.NewValue.Equals("34x"), "We have rejected age in exception");
				Assert.IsTrue(tb.Name.Equals("typeMismatch"), "valid name stuck");
				Assert.IsTrue(tb.Spouse.Name.Equals("Rod"), "valid spouse stuck");
			}
		}

		[Test]
		public virtual void GrandParentDefinitionFoundInObjectFactory()
		{
			TestObject dad = (TestObject) ObjectFactory.GetObject("father");
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
            TestObject dad = (TestObject)ObjectFactory.GetObject("namedfather", new object[] { "Hugo", 65 } );
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

        [Test(Description="Extra check that the type is really passed on to the parent factory")]
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
			TestObject tb = (TestObject) ObjectFactory.GetObject("singletonFactory");
			Assert.IsTrue(tb.Name.Equals(DummyFactory.SINGLETON_NAME), "Singleton from factory has correct name, not " + tb.Name);
			DummyFactory factory = (DummyFactory) ObjectFactory.GetObject("&singletonFactory");
			TestObject tb2 = (TestObject) ObjectFactory.GetObject("singletonFactory");
			Assert.IsTrue(tb == tb2, "Singleton references ==");
			Assert.IsTrue(factory.ObjectFactory != null, "FactoryObject is ObjectFactoryAware");
		}

		[Test]
		public virtual void FactoryPrototype()
		{
			Assert.IsTrue(ObjectFactory.IsSingleton("&prototypeFactory"));
			Assert.IsFalse(ObjectFactory.IsSingleton("prototypeFactory"));
			TestObject tb = (TestObject) ObjectFactory.GetObject("prototypeFactory");
			Assert.IsTrue(!tb.Name.Equals(DummyFactory.SINGLETON_NAME));
			TestObject tb2 = (TestObject) ObjectFactory.GetObject("prototypeFactory");
			Assert.IsTrue(tb != tb2, "Prototype references !=");
		}

		/// <summary>
		/// Check that we can get the factory object itself.
		/// This is only possible if we're dealing with a factory
		/// </summary>
		[Test]
		public virtual void GetFactoryItself()
		{
			DummyFactory factory = (DummyFactory) ObjectFactory.GetObject("&singletonFactory");
			Assert.IsTrue(factory != null);
		}

		/// <summary>Check that AfterPropertiesSet gets called on factory.</summary>
		[Test]
		public virtual void FactoryIsInitialized()
		{
			TestObject tb = (TestObject) ObjectFactory.GetObject("singletonFactory");
			DummyFactory factory = (DummyFactory) ObjectFactory.GetObject("&singletonFactory");
			Assert.IsTrue(factory.WasInitialized, "Factory was not initialized even though it implemented IInitializingObject");
		}

		/// <summary>
		/// It should be illegal to dereference a normal object as a factory.
		/// </summary>
		[Test]
		[ExpectedException(typeof (ObjectIsNotAFactoryException))]
		public virtual void RejectsFactoryGetOnNormalObject()
		{
			ObjectFactory.GetObject("&rod");
		}

		[Test]
		[ExpectedException(typeof (ObjectDefinitionStoreException))]
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
			((AbstractObjectFactory) ObjectFactory).RegisterAlias("rod", alias);
			object rod = ObjectFactory.GetObject("rod");
			object aliasRod = ObjectFactory.GetObject(alias);
			Assert.IsTrue(rod == aliasRod);
			((AbstractObjectFactory) ObjectFactory).RegisterAlias("father", alias);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RegisterSingletonWithEmptyName()
		{
			((AbstractObjectFactory) ObjectFactory)
				.RegisterSingleton(Environment.NewLine, DBNull.Value);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RegisterSingletonWithNullName()
		{
			((AbstractObjectFactory) ObjectFactory)
				.RegisterSingleton(null, DBNull.Value);
		}

#if NET_2_0
        [Test]
        public void GetSingletonNamesReflectsOrderOfRegistration()
        {
            AbstractObjectFactory of;
            of = CreateObjectFactory(true);
            of.RegisterSingleton("A", new object());
            of.RegisterSingleton("C", new object());
            of.RegisterSingleton("B", new object());
            Assert.AreEqual(new string[] { "A", "C", "B"}, of.GetSingletonNames());

            of = CreateObjectFactory(false);
            of.RegisterSingleton("A", new object());
            of.RegisterSingleton("C", new object());
            of.RegisterSingleton("B", new object());
            Assert.AreEqual(new string[] { "A", "C", "B"}, of.GetSingletonNames(typeof(object)));
        }
#endif

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ContainsSingletonWithEmptyName()
		{
			((AbstractObjectFactory) ObjectFactory)
				.ContainsSingleton(Environment.NewLine);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ContainsSingletonWithNullName()
		{
			((AbstractObjectFactory) ObjectFactory)
				.ContainsSingleton(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AliasWithEmptyName()
		{
			((AbstractObjectFactory) ObjectFactory).RegisterAlias(Environment.NewLine, "the whipping boy");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AliasWithEmptyAlias()
		{
			((AbstractObjectFactory) ObjectFactory).RegisterAlias("rick", Environment.NewLine);
		}

		[Test]
		[ExpectedException(typeof(ObjectDefinitionStoreException))]
		public void ChokesIfNotGivenSupportedIObjectDefinitionImplementation() 
		{
			ObjectFactory.GetObject("unsupportedDefinition");
		}

		#endregion
	}
}