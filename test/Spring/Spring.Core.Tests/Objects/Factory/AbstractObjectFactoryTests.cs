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

using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Threading;
using System.Collections;

namespace Spring.Objects.Factory;

/// <summary>
/// Subclasses must override SetUp () to initialize the object factory
/// and any other variables they need.
/// </summary>
/// <author>Rod Johnson</author>
/// <author>Rick Evans (.NET)</author>
public abstract class AbstractObjectFactoryTests
{
    protected internal abstract AbstractObjectFactory CreateObjectFactory(bool caseSensitive);

    private AbstractObjectFactory cachedFactory;

    protected AbstractObjectFactory ObjectFactory
    {
        get { return cachedFactory; }
        set { cachedFactory = value; }
    }

    [TearDown]
    public void DisposeObjectFactory()
    {
        ObjectFactory?.Dispose();
        ObjectFactory = null;
    }

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
            Assert.That(-1 < ex.Message.IndexOf("already registered"), Is.True);
        }

        Assert.That(of.GetAliases("nAmE").Count, Is.EqualTo(1));
        Assert.That(of.GetObject("nAmE"), Is.EqualTo(testObject));
        Assert.That(of.GetObject("ALIAS"), Is.EqualTo(testObject));
    }

    /// <summary>
    /// Roderick objects inherits from rod, overriding name only.
    /// </summary>
    [Test]
    public void Inheritance()
    {
        Assert.That(ObjectFactory.ContainsObject("rod"), Is.True);
        Assert.That(ObjectFactory.ContainsObject("roderick"), Is.True);
        TestObject rod = (TestObject) ObjectFactory["rod"];
        TestObject roderick = (TestObject) ObjectFactory["roderick"];
        Assert.That(rod != roderick, Is.True, "not == ");
        Assert.That(rod.Name.Equals("Rod"), Is.True, "rod.name is Rod");
        Assert.That(rod.Age == 31, Is.True, "rod.age is 31");
        Assert.That(roderick.Name.Equals("Roderick"), Is.True, "roderick.name is Roderick");
        Assert.That(roderick.Age == rod.Age, Is.True, "roderick.age was inherited");
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
        Assert.That(lb.ObjectName, Is.EqualTo("lifecycle"));
        // The dummy business method will throw an exception if the
        // necessary callbacks weren't invoked in the right order
        lb.BusinessMethod();
        Assert.That(lb.Destroyed, Is.False, "Was destroyed");
    }

    [Test(Description = "SPRNET-1208")]
    public void AddObjectFactoryOnObjectFactoryAwareObjectPostProcessors()
    {
        AbstractObjectFactory aof = ObjectFactory;
        LifecycleObject.PostProcessor lb = new LifecycleObject.PostProcessor();
        aof.AddObjectPostProcessor(lb);
        Assert.That(lb.ObjectFactory, Is.SameAs(aof));
    }

    [Test]
    public void FindsValidInstance()
    {
        object o = ObjectFactory.GetObject("rod");
        Assert.That(o is TestObject, Is.True, "Rod object is a TestObject");
        TestObject rod = (TestObject) o;
        Assert.That(rod.Name.Equals("Rod"), Is.True, "rod.name is Rod");
        Assert.That(rod.Age == 31, Is.True, "rod.age is 31");
    }

    [Test]
    public void GetInstanceByMatchingClass()
    {
        object o = ObjectFactory.GetObject("rod", typeof(TestObject));
        Assert.That(o is TestObject, Is.True, "Rod object is a TestObject");
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
            Assert.That(ex.ObjectName.Equals("rod"), Is.True, "Exception has correct object name");
            Assert.That(ex.RequiredType.Equals(typeof(IObjectFactory)), Is.True, "Exception requiredType must be ObjectFactory.class");
            Assert.That(typeof(TestObject).IsAssignableFrom(ex.ActualType), Is.True, "Exception actualType as TestObject.class");
            Assert.That(ex.ActualInstance == ObjectFactory.GetObject("rod"), Is.True, "Actual instance is correct");
        }
    }

    [Test]
    public virtual void GetSharedInstanceByMatchingClass()
    {
        object o = ObjectFactory.GetObject("rod", typeof(TestObject));
        Assert.That(o is TestObject, Is.True, "Rod object is a TestObject");
    }

    [Test]
    public virtual void GetSharedInstanceByMatchingClassNoCatch()
    {
        object o = ObjectFactory.GetObject("rod", typeof(TestObject));
        Assert.That(o is TestObject, Is.True, "Rod object is a TestObject");
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
            Assert.That(ex.ObjectName.Equals("rod"), Is.True, "Exception has correct object name");
            Assert.That(ex.RequiredType.Equals(typeof(IObjectFactory)), Is.True, "Exception requiredType must be IObjectFactory class");
            Assert.That(typeof(TestObject).IsAssignableFrom(ex.ActualType), Is.True, "Exception actualType as TestObject class");
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
            Assert.That(o is TestObject, Is.True, "Rod object1 is a TestObject");
            object o1 = ObjectFactory.GetObject("rod");
            Assert.That(o1 is TestObject, Is.True, "Rod object2 is a TestObject");
            Assert.That(o == o1, Is.True, "Object equals applies");
        }
        catch
        {
            Assert.Fail("Shouldn't throw exception on getting valid instance");
        }
    }

    [Test]
    public void NotThere()
    {
        Assert.That(ObjectFactory.ContainsObject("Mr Squiggle"), Is.False);
        Assert.Throws<NoSuchObjectDefinitionException>(() => ObjectFactory.GetObject("Mr Squiggle"));
    }

    [Test]
    public void ValidEmpty()
    {
        try
        {
            object o = ObjectFactory.GetObject("validEmpty");
            Assert.That(o is TestObject, Is.True, "validEmpty object is a TestObject");
            TestObject ve = (TestObject) o;
            Assert.That(ve.Name == null && ve.Age == 0 && ve.Spouse == null, Is.True, "Valid empty has defaults");
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
            Assert.That(wex.InnerException is PropertyAccessExceptionsException, Is.True);
            PropertyAccessExceptionsException ex = (PropertyAccessExceptionsException) wex.InnerException;
            // Furthers
            Assert.That(ex.ExceptionCount == 1, Is.True, "Has one error");
            Assert.That(ex.GetPropertyAccessException("age") != null, Is.True, "Error is for field age");

            TestObject tb = (TestObject) ex.ObjectWrapper.WrappedInstance;
            Assert.That(tb.Age == 0, Is.True, "Age still has default");
            Assert.That(ex.GetPropertyAccessException("age").PropertyChangeArgs.NewValue.Equals("34x"), Is.True, "We have rejected age in exception");
            Assert.That(tb.Name.Equals("typeMismatch"), Is.True, "valid name stuck");
            Assert.That(tb.Spouse.Name.Equals("Rod"), Is.True, "valid spouse stuck");
        }
    }

    [Test]
    public virtual void GrandParentDefinitionFoundInObjectFactory()
    {
        TestObject dad = (TestObject) ObjectFactory.GetObject("father");
        Assert.That(dad.Name.Equals("Albert"), Is.True, "Dad has correct name");
    }

    [Test]
    public virtual void GrandParentDefinitionFoundInObjectFactoryWithType()
    {
        TestObject dad = (TestObject) ObjectFactory.GetObject("typedfather", typeof(TestObject));
        Assert.That(dad.Name, Is.EqualTo(null), "Dad has not correct name");
    }

    [Test]
    public virtual void GrandParentDefinitionFoundInObjectFactoryWithArguments()
    {
        TestObject dad = (TestObject) ObjectFactory.GetObject("namedfather", new object[] { "Hugo", 65 });
        Assert.That(dad.Name, Is.EqualTo("Hugo"), "Dad has not correct name");
        Assert.That(dad.Age, Is.EqualTo(65), "Dad has not correct age");
    }

    [Test]
    public virtual void GrandParentDefinitionFoundInObjectFactoryWithTypeAndArguments()
    {
        TestObject dad = (TestObject) ObjectFactory.GetObject("typedfather", typeof(TestObject), new object[] { "Chris", 66 });
        Assert.That(dad.Name, Is.EqualTo("Chris"), "Dad has not correct name");
        Assert.That(dad.Age, Is.EqualTo(66), "Dad has not correct age");
    }

    [Test(Description = "Extra check that the type is really passed on to the parent factory")]
    public virtual void GrandParentDefinitionFoundInObjectFactoryWithTypeAndArgumentsWithWrongType()
    {
        try
        {
            TestObject dad = (TestObject) ObjectFactory.GetObject("typedfather", typeof(string), new object[] { "Chris", 66 });
            Assert.Fail("should throw ObjectNotOfRequiredTypeException");
        }
        catch (ObjectNotOfRequiredTypeException)
        {
        }
    }

    [Test]
    public virtual void FactorySingleton()
    {
        Assert.That(ObjectFactory.IsSingleton("&singletonFactory"), Is.True);
        Assert.That(ObjectFactory.IsSingleton("singletonFactory"), Is.True);
        TestObject tb = (TestObject) ObjectFactory.GetObject("singletonFactory");
        Assert.That(tb.Name.Equals(DummyFactory.SINGLETON_NAME), Is.True, "Singleton from factory has correct name, not " + tb.Name);
        DummyFactory factory = (DummyFactory) ObjectFactory.GetObject("&singletonFactory");
        TestObject tb2 = (TestObject) ObjectFactory.GetObject("singletonFactory");
        Assert.That(tb == tb2, Is.True, "Singleton references ==");
        Assert.That(factory.ObjectFactory != null, Is.True, "FactoryObject is ObjectFactoryAware");
    }

    [Test]
    public virtual void FactoryPrototype()
    {
        Assert.That(ObjectFactory.IsSingleton("&prototypeFactory"), Is.True);
        Assert.That(ObjectFactory.IsSingleton("prototypeFactory"), Is.False);
        TestObject tb = (TestObject) ObjectFactory.GetObject("prototypeFactory");
        Assert.That(!tb.Name.Equals(DummyFactory.SINGLETON_NAME), Is.True);
        TestObject tb2 = (TestObject) ObjectFactory.GetObject("prototypeFactory");
        Assert.That(tb != tb2, Is.True, "Prototype references !=");
    }

    /// <summary>
    /// Check that we can get the factory object itself.
    /// This is only possible if we're dealing with a factory
    /// </summary>
    [Test]
    public virtual void GetFactoryItself()
    {
        DummyFactory factory = (DummyFactory) ObjectFactory.GetObject("&singletonFactory");
        Assert.That(factory != null, Is.True);
    }

    /// <summary>Check that AfterPropertiesSet gets called on factory.</summary>
    [Test]
    public virtual void FactoryIsInitialized()
    {
        TestObject tb = (TestObject) ObjectFactory.GetObject("singletonFactory");
        DummyFactory factory = (DummyFactory) ObjectFactory.GetObject("&singletonFactory");
        Assert.That(factory.WasInitialized, Is.True, "Factory was not initialized even though it implemented IInitializingObject");
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
            Assert.That(alias.Equals(ex.ObjectName), Is.True);
        }

        // Create alias
        ObjectFactory.RegisterAlias("rod", alias);
        object rod = ObjectFactory.GetObject("rod");
        object aliasRod = ObjectFactory.GetObject(alias);
        Assert.That(rod == aliasRod, Is.True);
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
        Assert.That(of.GetSingletonNames(), Is.EqualTo(new string[] { "A", "C", "B" }));

        of = CreateObjectFactory(false);
        of.RegisterSingleton("A", new object());
        of.RegisterSingleton("C", new object());
        of.RegisterSingleton("B", new object());
        Assert.That(of.GetSingletonNames(typeof(object)), Is.EqualTo(new string[] { "A", "C", "B" }));
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

        TestObject to = (TestObject) of.GetObject("product1");
        Assert.That(to, Is.Not.Null);
        Assert.That(to.Spouse, Is.Not.Null);
        Assert.That(((TestObject) to.Spouse).Sibling, Is.Not.Null);
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
            Assert.That(ex.ObjectName, Is.EqualTo("product1"));
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

        ParentClass parent = (ParentClass) factory.GetObject("Parent");
        Assert.That(parent, Is.Not.Null);

        DisposableClass innerObject = (DisposableClass) parent.InnerObject;
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

    [TearDown]
    public void _TearDown()
    {
        ObjectFactory?.Dispose();
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

    [TearDown]
    public void _TearDown()
    {
        ObjectFactory?.Dispose();
    }

    [Test]
    public void When_ParentAndChildArePrototypes_ConstructorInjection_DoesNotEnforceDestructionOrder()
    {
        WireParentAndChildWWithImpliedDependencyByConstructorInjection(false, false);

        Parent theParent = ObjectFactory.GetObject("parent") as Parent;
        theParent.Dispose();

        Assert.That(_parentCounter, Is.EqualTo(0), "Should have no remaining parent objects after dispose");
        Assert.That(_childCounter, Is.EqualTo(1), "Should have exactly ONE child object");
        Assert.That(theParent.InjectedChild, Is.Not.Null, "Parent's child dependency not set as expected");
        Assert.That(invocationLog[2], Is.EqualTo("Parent Destructor"), "Parent Destructor wasn't called third!");
        Assert.That(invocationLog.Count, Is.EqualTo(3), "Should have no further object lifecycle behavior after parent destruction!");
    }

    [Test]
    public void When_ParentAndChildArePrototypes_ConstructorInjection_EnforcesConstructionOrder()
    {
        WireParentAndChildWWithImpliedDependencyByConstructorInjection(false, false);

        Parent theParent = ObjectFactory.GetObject("parent") as Parent;

        Assert.That(_parentCounter, Is.EqualTo(1), "Should have exactly ONE parent object");
        Assert.That(_childCounter, Is.EqualTo(1), "Should have exactly ONE child object");
        Assert.That(theParent.InjectedChild, Is.Not.Null, "Parent's child dependency not set as expected");
        Assert.That(invocationLog[0], Is.EqualTo("Child Constructor"), "Child Constructor wasn't called first!");
        Assert.That(invocationLog[1], Is.EqualTo("Parent Constructor"), "Parent Constructor wasn't called second!");
    }

    [Test]
    public void When_ParentAndChildArePrototypes_DependsOn_DoesNotEnforceDestructionOrder()
    {
        WireParentAndChildWithDependsOnDeclarationDependency(false, false);

        Parent theParent = ObjectFactory.GetObject("parent") as Parent;
        theParent.Dispose();

        Assert.That(_parentCounter, Is.EqualTo(0), "Should have no remaining parent objects after dispose");
        Assert.That(_childCounter, Is.EqualTo(1), "Should have exactly ONE child object");

        Assert.That(invocationLog[2], Is.EqualTo("Parent Destructor"), "Parent Destructor wasn't called third!");
        Assert.That(invocationLog.Count, Is.EqualTo(3), "Should have no further object lifecycle behavior after parent destruction!");
    }

    [Test]
    public void When_ParentAndChildArePrototypes_DependsOn_EnforcesConstructionOrder()
    {
        WireParentAndChildWithDependsOnDeclarationDependency(false, false);

        Parent theParent = ObjectFactory.GetObject("parent") as Parent;

        Assert.That(_parentCounter, Is.EqualTo(1), "Should have exactly ONE parent object");
        Assert.That(_childCounter, Is.EqualTo(1), "Should have exactly ONE child object");
        Assert.That(invocationLog[0], Is.EqualTo("Child Constructor"), "Child Constructor wasn't called first!");
        Assert.That(invocationLog[1], Is.EqualTo("Parent Constructor"), "Parent Constructor wasn't called second!");
    }

    [Test]
    public void When_ParentAndChildAreSingletons_ConstructorInjection_EnforcesDestructionOrder()
    {
        WireParentAndChildWWithImpliedDependencyByConstructorInjection(true, true);

        //triger the construction of the singletons
        ObjectFactory.GetObject("parent");

        //trigger the disposal of the singletons
        ObjectFactory.Dispose();

        Assert.That(_parentCounter, Is.EqualTo(0), "Should have no remaining parent objects after dispose");
        Assert.That(_childCounter, Is.EqualTo(0), "Should have no remaining child objects after dispose");
        Assert.That(invocationLog[2], Is.EqualTo("Parent Destructor"), "Parent Destructor wasn't called third!");
        Assert.That(invocationLog[3], Is.EqualTo("Child Destructor"), "Child Destructor wasn't called fourth!");
        Assert.That(invocationLog.Count, Is.EqualTo(4), "Should have no further object lifecycle behavior after parent destruction!");
    }

    [Test]
    public void When_ParentAndChildAreSingletons_DependsOn_EnforcesDestructionOrder()
    {
        WireParentAndChildWithDependsOnDeclarationDependency(true, true);

        //triger the construction of the singletons
        ObjectFactory.GetObject("parent");

        //make certain they are created successfully
        Assert.That(_parentCounter, Is.EqualTo(1), "Should have exactly ONE parent object");
        Assert.That(_childCounter, Is.EqualTo(1), "Should have exactly ONE child object");

        //trigger the disposal of the singletons
        ObjectFactory.Dispose();

        Assert.That(_parentCounter, Is.EqualTo(0), "Should have no remaining parent objects after dispose");
        Assert.That(_childCounter, Is.EqualTo(0), "Should have no remaining child objects after dispose");
        Assert.That(invocationLog[2], Is.EqualTo("Parent Destructor"), "Parent Destructor wasn't called third!");
        Assert.That(invocationLog[3], Is.EqualTo("Child Destructor"), "Child Destructor wasn't called fourth!");
        Assert.That(invocationLog.Count, Is.EqualTo(4), "Should have no further object lifecycle behavior after child destruction!");
    }

    [Test]
    public void When_ParentIsProttpyeAndChildIsSingleton_ConstructorInjection_DoesNotEnforcesDestructionOrder()
    {
        WireParentAndChildWWithImpliedDependencyByConstructorInjection(false, true);

        //triger the construction of the singletons
        ObjectFactory.GetObject("parent");

        //trigger the disposal of the singletons
        ObjectFactory.Dispose();

        Assert.That(_parentCounter, Is.EqualTo(1), "Should have ONE remaining parent objects after dispose");
        Assert.That(_childCounter, Is.EqualTo(0), "Should have no remaining child objects after dispose");
        Assert.That(invocationLog[2], Is.EqualTo("Child Destructor"), "Child Destructor wasn't called third!");
        Assert.That(invocationLog.Count, Is.EqualTo(3), "Should have no further object lifecycle behavior after child destruction!");
    }

    [Test]
    public void When_ParentIsSingletonAndChildIsPrototype_ConstructorInjection_DoesNotEnforcesDestructionOrder()
    {
        WireParentAndChildWWithImpliedDependencyByConstructorInjection(true, false);

        //triger the construction of the singletons
        ObjectFactory.GetObject("parent");

        //trigger the disposal of the singletons
        ObjectFactory.Dispose();

        Assert.That(_parentCounter, Is.EqualTo(0), "Should have no remaining parent objects after dispose");
        Assert.That(_childCounter, Is.EqualTo(1), "Should have ONE remaining child object after dispose");
        Assert.That(invocationLog[2], Is.EqualTo("Parent Destructor"), "Child Destructor wasn't called third!");
        Assert.That(invocationLog.Count, Is.EqualTo(3), "Should have no further object lifecycle behavior after parent destruction!");
    }

    [Test]
    public void When_ParentIsSingletonAndChildIsPrototype_DependsOn_EnforcesDestructionOrder()
    {
        WireParentAndChildWithDependsOnDeclarationDependency(true, false);

        //triger the construction of the singletons
        ObjectFactory.GetObject("parent");

        //make certain they are created successfully
        Assert.That(_parentCounter, Is.EqualTo(1), "Should have exactly ONE parent object");
        Assert.That(_childCounter, Is.EqualTo(1), "Should have exactly ONE child object");

        //trigger the disposal of the singletons
        ObjectFactory.Dispose();

        Assert.That(_parentCounter, Is.EqualTo(0), "Should have no remaining parent objects after dispose");
        Assert.That(_childCounter, Is.EqualTo(1), "Should have no remaining child objects after dispose");
        Assert.That(invocationLog[2], Is.EqualTo("Parent Destructor"), "Parent Destructor wasn't called third!");
        Assert.That(invocationLog.Count, Is.EqualTo(3), "Should have no further object lifecycle behavior after parent destruction!");
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
