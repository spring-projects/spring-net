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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using Spring.Core.TypeConversion;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Objects.Support;

namespace Spring.Objects.Factory
{
    /// <summary>
    /// Unit tests for the DefaultListableObjectFactory class.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Simon White (.NET)</author>
    [TestFixture]
    public sealed class DefaultListableObjectFactoryTests
    {
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// The setup logic executed before the execution of this test fixture.
        /// </summary>
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            // enable (null appender) logging, just to ensure that the logging code is correct :D
            //XmlConfigurator.Configure();
        }

        interface ICollaborator {}
        interface IStrategy {}

        class Collaborator : ICollaborator {}
        class Strategy1 : IStrategy {}
        class Strategy2 : IStrategy {}

        class Class1
        {
            public readonly IStrategy TheStrategy;

            public Class1( ICollaborator collaborator, IStrategy strategy)
            {
                TheStrategy = strategy;
            }
        }

        class Class2
        {
            private IStrategy _strategy;
            private ICollaborator _collaborator;

            public Class2()
            {}

            public IStrategy Strategy
            {
                get { return _strategy; }
                set { _strategy = value; }
            }

            public ICollaborator Collaborator
            {
                get { return _collaborator; }
                set { _collaborator = value; }
            }
        }


        [Test(Description = "http://jira.springframework.org/browse/SPRNET-985")]
        public void AutowireConstructorHonoresOverridesBeforeThrowingUnsatisfiedDependencyException()
        {
            RootObjectDefinition def = new RootObjectDefinition(typeof(Class1));
            def.AutowireMode = AutoWiringMode.AutoDetect;
            def.ConstructorArgumentValues.AddNamedArgumentValue("strategy", new RuntimeObjectReference("Strategy1") );

            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            lof.RegisterObjectDefinition("Class", def);
            lof.RegisterObjectDefinition("ICollaborator", new RootObjectDefinition(typeof(Collaborator)));
            lof.RegisterObjectDefinition("Strategy1", new RootObjectDefinition(typeof(Strategy1)));
            lof.RegisterObjectDefinition("Strategy2", new RootObjectDefinition(typeof(Strategy1)));

            Class1 c1 = (Class1) lof.GetObject("Class");
            Assert.IsNotNull(c1);
            Assert.AreEqual( typeof(Strategy1), c1.TheStrategy.GetType() );
        }

        [Test(Description = "http://jira.springframework.org/browse/SPRNET-985")]
        public void AutowireByTypeHonoresOverridesBeforeThrowingUnsatisfiedDependencyException()
        {
            RootObjectDefinition def = new RootObjectDefinition(typeof(Class2));
            def.AutowireMode = AutoWiringMode.AutoDetect;
            def.PropertyValues.Add("strategy", new RuntimeObjectReference("Strategy1") );

            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            lof.RegisterObjectDefinition("Class", def);
            lof.RegisterObjectDefinition("ICollaborator", new RootObjectDefinition(typeof(Collaborator)));
            lof.RegisterObjectDefinition("Strategy1", new RootObjectDefinition(typeof(Strategy1)));
            lof.RegisterObjectDefinition("Strategy2", new RootObjectDefinition(typeof(Strategy1)));

            Class2 c2 = (Class2) lof.GetObject("Class");
            Assert.IsNotNull(c2);
            Assert.AreEqual( typeof(Strategy1), c2.Strategy.GetType() );
        }

        [Test(Description = "http://jira.springframework.org/browse/SPRNET-112")]
        public void ObjectCreatedViaStaticFactoryMethodUsesReturnTypeOfFactoryMethodAsTheObjectType()
        {
            RootObjectDefinition def
                = new RootObjectDefinition(typeof(TestObjectCreator));
            def.FactoryMethodName = "CreateTestObject";
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            lof.RegisterObjectDefinition("factoryObject", def);
            var objs = lof.GetObjects<TestObject>();
            Assert.AreEqual(1, objs.Count);
        }

        [Test(Description = "http://opensource2.atlassian.com/projects/spring/browse/SPRNET-112")]
        public void ObjectCreatedViaInstanceFactoryMethodUsesReturnTypeOfFactoryMethodAsTheObjectType()
        {
            RootObjectDefinition def
                = new RootObjectDefinition(typeof(TestObjectCreator));
            def.FactoryMethodName = "InstanceCreateTestObject";
            def.FactoryObjectName = "target";
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            lof.RegisterObjectDefinition("factoryObject", def);
            lof.RegisterObjectDefinition("target", new RootObjectDefinition(typeof(TestObjectCreator)));
            var objs = lof.GetObjects<TestObject>();
            Assert.AreEqual(1, objs.Count);
        }

        [Test(Description = "http://opensource2.atlassian.com/projects/spring/browse/SPRNET-112")]
        public void ObjectCreatedViaStaticGenericFactoryMethodUsesReturnTypeOfGenericFactoryMethodAsTheObjectType()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            RootObjectDefinition def
                = new RootObjectDefinition(typeof(TestGenericObject<int, string>));
            def.FactoryMethodName = "CreateList<int>";
            lof.RegisterObjectDefinition("foo", def);
            var objs = lof.GetObjectsOfType(typeof(List<int>));
            Assert.AreEqual(1, objs.Count);
        }

        [Test(Description = "http://opensource2.atlassian.com/projects/spring/browse/SPRNET-112")]
        public void ObjectCreatedViaInstanceGenericFactoryMethodUsesReturnTypeOfGenericFactoryMethodAsTheObjectType()
        {
            RootObjectDefinition def
                = new RootObjectDefinition(typeof(TestObjectCreator));
            def.FactoryMethodName = "CreateInstance<string,int>";
            def.FactoryObjectName = "target";
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            lof.RegisterObjectDefinition("factoryObject", def);
            lof.RegisterObjectDefinition("target", new RootObjectDefinition(typeof(TestGenericObject<int, string>)));
            var objs = lof.GetObjectsOfType(typeof(TestGenericObject<string, int>));
            Assert.AreEqual(1, objs.Count);
        }

        /// <summary>
        /// Object instantiation through factory method should not require type attribute.
        /// </summary>
        [Test(Description = "http://opensource.atlassian.com/projects/spring/browse/SPRNET-130")]
        public void SPRNET_130()
        {
            const string factoryObjectName = "factoryObject";
            const string exampleObjectName = "exampleObject";

            RootObjectDefinition factoryObjectDefinition
                = new RootObjectDefinition(typeof(TestObjectFactory));
            RootObjectDefinition exampleObjectDefinition = new RootObjectDefinition();
            exampleObjectDefinition.FactoryObjectName = factoryObjectName;
            exampleObjectDefinition.FactoryMethodName = "GetObject";

            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            lof.RegisterObjectDefinition(factoryObjectName, factoryObjectDefinition);
            lof.RegisterObjectDefinition(exampleObjectName, exampleObjectDefinition);

            object exampleObject = lof.GetObject(exampleObjectName);
            Assert.IsNotNull(exampleObject);
            object factoryObject = lof.GetObject(factoryObjectName);
            Assert.IsNotNull(factoryObject);
        }

        [Test(Description = "http://opensource.atlassian.com/projects/spring/browse/SPR-1011")]
        public void SPR_1011()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            RootObjectDefinition def
                = new RootObjectDefinition(
                    typeof(StaticFactoryMethodObject));
            def.FactoryMethodName = "CreateObject";
            lof.RegisterObjectDefinition("foo", def);
            var objs = lof.GetObjectsOfType(typeof(DBNull));
            Assert.AreEqual(1, objs.Count,
                            "Must be looking at the RETURN TYPE of the factory method, " +
                                "and hence get one DBNull object back.");
        }

        private sealed class StaticFactoryMethodObject
        {
            private StaticFactoryMethodObject()
            {
            }

            public static DBNull CreateObject()
            {
                return DBNull.Value;
            }
        }

        [Test(Description = "http://opensource.atlassian.com/projects/spring/browse/SPR-1077")]
        public void SPR_1077()
        {
            DisposableTestObject sing = null;
            using (DefaultListableObjectFactory lof = new DefaultListableObjectFactory())
            {
                RootObjectDefinition singleton
                    = new RootObjectDefinition(typeof(DisposableTestObject));
                MutablePropertyValues sprops = new MutablePropertyValues();
                sprops.Add("name", "Rick");
                singleton.PropertyValues = sprops;
                lof.RegisterObjectDefinition("singleton", singleton);

                RootObjectDefinition prototype
                    = new RootObjectDefinition(typeof(TestObject));
                MutablePropertyValues pprops = new MutablePropertyValues();
                pprops.Add("name", "Jenny");
                // prototype has dependency on a singleton...
                pprops.Add("spouse", new RuntimeObjectReference("singleton"));
                prototype.PropertyValues = pprops;
                prototype.IsSingleton = false;
                lof.RegisterObjectDefinition("prototype", prototype);

                sing = (DisposableTestObject)lof.GetObject("singleton");

                lof.GetObject("prototype");
                lof.GetObject("prototype");
                lof.GetObject("prototype");
                lof.GetObject("prototype");
            }
            Assert.AreEqual(1, sing.NumTimesDisposed);
        }

        private sealed class DisposableTestObject : TestObject, IDisposable
        {
            private int _numTimesDisposed;

            public int NumTimesDisposed
            {
                get { return _numTimesDisposed; }
            }

            public void Dispose()
            {
                ++_numTimesDisposed;
            }
        }

        [Test]
        public void GetObjectPostProcessorCount()
        {
            IObjectPostProcessor proc1 = FakeItEasy.A.Fake<IObjectPostProcessor>();
            IObjectPostProcessor proc2 = FakeItEasy.A.Fake<IObjectPostProcessor>();

            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();

            const string errMsg = "Wrong number of IObjectPostProcessors being reported by the ObjectPostProcessorCount property.";
            Assert.AreEqual(0, lof.ObjectPostProcessorCount, errMsg);
            lof.AddObjectPostProcessor(proc1);
            Assert.AreEqual(1, lof.ObjectPostProcessorCount, errMsg);
            lof.AddObjectPostProcessor(proc2);
            Assert.AreEqual(2, lof.ObjectPostProcessorCount, errMsg);

        }

        /// <summary>
        /// The ObjectPostProcessorCount property must only return the count of
        /// processors registered with the current factory, and not
        /// surf up any hierarchy.
        /// </summary>
        [Test]
        public void GetObjectPostProcessorCountDoesntRespectHierarchy()
        {
            IObjectPostProcessor proc1 = FakeItEasy.A.Fake<IObjectPostProcessor>();
            IObjectPostProcessor proc2 = FakeItEasy.A.Fake<IObjectPostProcessor>();

            DefaultListableObjectFactory child = new DefaultListableObjectFactory();
            DefaultListableObjectFactory parent = new DefaultListableObjectFactory(child);

            const string errMsg = "Wrong number of IObjectPostProcessors being reported by the ObjectPostProcessorCount property.";
            Assert.AreEqual(0, child.ObjectPostProcessorCount, errMsg);
            Assert.AreEqual(0, parent.ObjectPostProcessorCount, errMsg);
            child.AddObjectPostProcessor(proc1);
            Assert.AreEqual(1, child.ObjectPostProcessorCount, errMsg);
            Assert.AreEqual(0, parent.ObjectPostProcessorCount, errMsg);
            parent.AddObjectPostProcessor(proc2);
            Assert.AreEqual(1, child.ObjectPostProcessorCount, errMsg);
            Assert.AreEqual(1, parent.ObjectPostProcessorCount, errMsg);
            child.AddObjectPostProcessor(proc2);
            Assert.AreEqual(2, child.ObjectPostProcessorCount, errMsg);
            Assert.AreEqual(1, parent.ObjectPostProcessorCount, errMsg);
        }

        [Test]
        public void TestIInstantiationAwareObjectPostProcessorsInterception()
        {
            ProxyingInstantiationAwareObjectPostProcessorStub proc
                = new ProxyingInstantiationAwareObjectPostProcessorStub("TheAgony");

            MutablePropertyValues props = new MutablePropertyValues();
            props.Add("Name", "Rick");
            RootObjectDefinition toBeProxied
                = new RootObjectDefinition(typeof(TestObject), props);

            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            lof.AddObjectPostProcessor(proc);
            lof.RegisterObjectDefinition("toBeProxied", toBeProxied);

            object proxy = lof["toBeProxied"];
            Assert.IsNotNull(proxy);
            Assert.AreEqual("TheAgony", proxy);
        }

        [Test]
        public void TestIInstantiationAwareObjectPostProcessorsPassThrough()
        {
            NullInstantiationAwareObjectPostProcessorStub proc
                = new NullInstantiationAwareObjectPostProcessorStub();

            MutablePropertyValues props = new MutablePropertyValues();
            props.Add("Name", "Rick");
            RootObjectDefinition not
                = new RootObjectDefinition(typeof(TestObject), props);

            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            lof.AddObjectPostProcessor(proc);
            lof.RegisterObjectDefinition("notToBeProxied", not);

            object foo = lof["notToBeProxied"];
            Assert.IsNotNull(foo);
            Assert.AreEqual(typeof(TestObject), foo.GetType());
            TestObject to = (TestObject)foo;
            Assert.AreEqual("Rick", to.Name);
        }

        private sealed class NullInstantiationAwareObjectPostProcessorStub
            : IInstantiationAwareObjectPostProcessor
        {
            public NullInstantiationAwareObjectPostProcessorStub()
            {
            }

            public object PostProcessBeforeInitialization(object obj, string name)
            {
                return obj;
            }

            public object PostProcessBeforeInstantiation(Type objectType, string objectName)
            {
                //proceed with default instantiation
                return null;
            }

            public bool PostProcessAfterInstantiation(object objectInstance, string objectName)
            {
                //proceed to set properties on the object
                return true;
            }

            public IPropertyValues PostProcessPropertyValues(IPropertyValues pvs, IList<PropertyInfo> pis, object objectInstance, string objectName)
            {
                return pvs;
            }

            public object PostProcessAfterInitialization(object obj, string objectName)
            {
                return obj;
            }
        }

        private sealed class ProxyingInstantiationAwareObjectPostProcessorStub
            : IInstantiationAwareObjectPostProcessor
        {
            public ProxyingInstantiationAwareObjectPostProcessorStub()
            {
            }

            public ProxyingInstantiationAwareObjectPostProcessorStub(object proxy)
            {
                _proxy = proxy;
            }

            private object _proxy;

            public object Proxy
            {
                get { return _proxy; }
                set { _proxy = value; }
            }

            public object PostProcessBeforeInitialization(object obj, string name)
            {
                throw new NotImplementedException();
            }

            public object PostProcessBeforeInstantiation(Type objectType, string objectName)
            {
                return _proxy;
            }

            public bool PostProcessAfterInstantiation(object objectInstance, string objectName)
            {
                return true;
            }

            public IPropertyValues PostProcessPropertyValues(IPropertyValues pvs, IList<PropertyInfo> pis, object objectInstance, string objectName)
            {
                return pvs;
            }

            public object PostProcessAfterInitialization(object obj, string objectName)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void PreInstantiateSingletonsMustNotIgnoreObjectsWithUnresolvedObjectTypes()
        {
            KnowsIfInstantiated.ClearInstantiationRecord();
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            Assert.IsTrue(!KnowsIfInstantiated.WasInstantiated, "Singleton appears to be instantiated before the test is even run.");
            RootObjectDefinition def = new RootObjectDefinition();
            def.ObjectTypeName = typeof(KnowsIfInstantiated).FullName;
            lof.RegisterObjectDefinition("x1", def);
            Assert.IsTrue(!KnowsIfInstantiated.WasInstantiated, "Singleton appears to be instantiated before PreInstantiateSingletons() is invoked.");
            lof.PreInstantiateSingletons();
            Assert.IsTrue(KnowsIfInstantiated.WasInstantiated, "Singleton was not instantiated by the container (it must be).");
        }

        [Test]
        public void LazyInitialization()
        {
            KnowsIfInstantiated.ClearInstantiationRecord();
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();

            RootObjectDefinition def = new RootObjectDefinition();
            def.ObjectTypeName = typeof(KnowsIfInstantiated).FullName;
            def.IsLazyInit = true;
            lof.RegisterObjectDefinition("x1", def);

            Assert.IsTrue(!KnowsIfInstantiated.WasInstantiated, "Singleton appears to be instantiated before the test is even run.");
            lof.RegisterObjectDefinition("x1", def);
            Assert.IsTrue(!KnowsIfInstantiated.WasInstantiated, "Singleton appears to be instantiated before PreInstantiateSingletons() is invoked.");
            lof.PreInstantiateSingletons();
            Assert.IsFalse(KnowsIfInstantiated.WasInstantiated, "Singleton was instantiated by the container (it must NOT be 'cos LazyInit was set to TRUE).");
            lof.GetObject("x1");
            Assert.IsTrue(KnowsIfInstantiated.WasInstantiated, "Singleton was not instantiated by the container (it must be).");
        }

        [Test]
        public void SingletonFactoryObjectMustNotCreatePrototypeOnPreInstantiateSingletonsCall()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();

            RootObjectDefinition def = new RootObjectDefinition();
            def.ObjectType = typeof(DummyFactory);
            def.IsSingleton = true;
            def.PropertyValues.Add("IsSingleton", false);

            DummyFactory.Reset();

            Assert.IsFalse(DummyFactory.WasPrototypeCreated,
                           "Prototype appears to be instantiated before the test is even run.");
            lof.RegisterObjectDefinition("x1", def);
            Assert.IsFalse(DummyFactory.WasPrototypeCreated,
                           "Prototype instantiated after object definition registration (must NOT be).");
            lof.PreInstantiateSingletons();
            Assert.IsFalse(DummyFactory.WasPrototypeCreated,
                           "Prototype instantiated after call to PreInstantiateSingletons(); must NOT be.");
            lof.GetObject("x1");
            Assert.IsTrue(DummyFactory.WasPrototypeCreated, "Prototype was not instantiated.");
        }

        [Test]
        public void Empty()
        {
            IListableObjectFactory lof = new DefaultListableObjectFactory();
            Assert.IsTrue(lof.GetObjectDefinitionNames() != null, "No objects defined --> array != null");
            Assert.IsTrue(lof.GetObjectDefinitionNames().Count == 0, "No objects defined after no arg constructor");
            Assert.IsTrue(lof.ObjectDefinitionCount == 0, "No objects defined after no arg constructor");
        }

        [Test]
        public void ObjectDefinitionCountIsZeroBeforeAnythingIsRegistered()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            Assert.AreEqual(0, lof.ObjectDefinitionCount, "No objects must be defined straight off the bat.");
        }

        [Test]
        public void ObjectDefinitionOverriding()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            lof.RegisterObjectDefinition("test", new RootObjectDefinition(typeof(TestObject), null));
            lof.RegisterObjectDefinition("test", new RootObjectDefinition(typeof(NestedTestObject), null));
            Assert.IsTrue(lof.GetObject("test") is NestedTestObject);
        }

        [Test]
        public void ObjectDefinitionOverridingNotAllowed()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            lof.AllowObjectDefinitionOverriding = false;
            lof.RegisterObjectDefinition("test", new RootObjectDefinition(typeof(TestObject), null));
            Assert.Throws<ObjectDefinitionStoreException>(() => lof.RegisterObjectDefinition("test", new RootObjectDefinition(typeof(NestedTestObject), null)));
        }

        [Test]
        public void CustomEditor()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            NumberFormatInfo nfi = new CultureInfo("en-GB", false).NumberFormat;
            lof.RegisterCustomConverter(typeof(Single), new CustomNumberConverter(typeof(Single), nfi, true));
            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("myFloat", "1.1");
            lof.RegisterObjectDefinition("testObject", new RootObjectDefinition(typeof(TestObject), pvs));
            TestObject testObject = (TestObject)lof.GetObject("testObject");
            Assert.IsTrue(testObject.MyFloat == 1.1f);
        }

        [Test]
        public void RegisterExistingSingletonWithReference()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();

            RootObjectDefinition def = new RootObjectDefinition();
            def.ObjectType = typeof(TestObject);
            def.PropertyValues.Add("Name", "Rick");
            def.PropertyValues.Add("Age", 30);
            def.PropertyValues.Add("Spouse", new RuntimeObjectReference("singletonObject"));
            lof.RegisterObjectDefinition("test", def);

            object singletonObject = new TestObject();
            lof.RegisterSingleton("singletonObject", singletonObject);
            Assert.IsTrue(lof.IsSingleton("singletonObject"));
            TestObject test = (TestObject)lof.GetObject("test");
            Assert.AreEqual(singletonObject, lof.GetObject("singletonObject"));
            Assert.AreEqual(singletonObject, test.Spouse);
            var objectsOfType = lof.GetObjectsOfType(typeof(TestObject), false, true);
            Assert.AreEqual(2, objectsOfType.Count);
            Assert.IsTrue(objectsOfType.Values.Contains(test));
            Assert.IsTrue(objectsOfType.Values.Contains(singletonObject));
        }

        [Test]
        public void ApplyPropertyValues()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            MutablePropertyValues properties = new MutablePropertyValues();
            properties.Add("age", "99");
            factory.RegisterObjectDefinition("test", new RootObjectDefinition(typeof(TestObject), properties));
            TestObject obj = new TestObject();
            Assert.AreEqual(0, obj.Age);
            factory.ApplyObjectPropertyValues(obj, "test");
            Assert.AreEqual(99, obj.Age, "Property values were not applied to the existing instance.");
        }

        [Test]
        public void ApplyPropertyValuesWithIncompleteDefinition()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            MutablePropertyValues properties = new MutablePropertyValues();
            properties.Add("age", "99");
            factory.RegisterObjectDefinition("test", new RootObjectDefinition(null, properties));
            TestObject obj = new TestObject();
            Assert.AreEqual(0, obj.Age);
            factory.ApplyObjectPropertyValues(obj, "test");
            Assert.AreEqual(99, obj.Age, "Property values were not applied to the existing instance.");
        }

        [Test]
        public void RegisterExistingSingletonWithAutowire()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("name", "Tony");
            pvs.Add("age", "48");
            RootObjectDefinition rod = new RootObjectDefinition(typeof(DependenciesObject), pvs, true);
            rod.DependencyCheck = DependencyCheckingMode.Objects;
            rod.AutowireMode = AutoWiringMode.ByType;
            lof.RegisterObjectDefinition("test", rod);
            object singletonObject = new TestObject();
            lof.RegisterSingleton("singletonObject", singletonObject);
            Assert.IsTrue(lof.ContainsObject("singletonObject"));
            Assert.IsTrue(lof.IsSingleton("singletonObject"));
            Assert.AreEqual(0, lof.GetAliases("singletonObject").Count);
            DependenciesObject test = (DependenciesObject)lof.GetObject("test");
            Assert.AreEqual(singletonObject, lof.GetObject("singletonObject"));
            Assert.AreEqual(singletonObject, test.Spouse);
        }

        [Test]
        public void RegisterExistingSingletonWithAlreadyBound()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            object singletonObject = new TestObject();
            lof.RegisterSingleton("singletonObject", singletonObject);
            Assert.Throws<ObjectDefinitionStoreException>(() => lof.RegisterSingleton("singletonObject", singletonObject));
        }

        [Test]
        public void AutowireConstructor()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            RootObjectDefinition rod = new RootObjectDefinition(typeof(TestObject));
            lof.RegisterObjectDefinition("spouse", rod);
            ConstructorDependenciesObject cdo = (ConstructorDependenciesObject)lof.Autowire(typeof(ConstructorDependenciesObject),
                                                                                             AutoWiringMode.Constructor, true);
            object spouse = lof.GetObject("spouse");
            Assert.IsTrue(cdo.Spouse1 == spouse);
            Assert.IsTrue(ObjectFactoryUtils.ObjectOfType(lof, typeof(TestObject)) == spouse);
        }

        [Test]
        public void AutowireObjectByName()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            RootObjectDefinition rodDefinition = new RootObjectDefinition(typeof(TestObject));
            rodDefinition.PropertyValues.Add("name", "Rod");
            rodDefinition.AutowireMode = AutoWiringMode.ByName;
            RootObjectDefinition kerryDefinition = new RootObjectDefinition(typeof(TestObject));
            kerryDefinition.PropertyValues.Add("name", "Kerry");
            lof.RegisterObjectDefinition("rod", rodDefinition);
            lof.RegisterObjectDefinition("Spouse", kerryDefinition);
            DependenciesObject obj = (DependenciesObject)lof.Autowire(typeof(DependenciesObject),
                                                                       AutoWiringMode.ByName, true);
            TestObject objRod = (TestObject)lof.GetObject("rod");
            Assert.AreEqual(obj.Spouse, objRod.Spouse);
        }

        [Test]
        public void AutowireObjectByNameIsNotCaseInsensitive()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            RootObjectDefinition rodDefinition = new RootObjectDefinition(typeof(TestObject));
            rodDefinition.PropertyValues.Add("name", "Rod");
            rodDefinition.AutowireMode = AutoWiringMode.ByName;
            RootObjectDefinition kerryDefinition = new RootObjectDefinition(typeof(TestObject));
            kerryDefinition.PropertyValues.Add("name", "Kerry");
            lof.RegisterObjectDefinition("rod", rodDefinition);
            lof.RegisterObjectDefinition("spouse", kerryDefinition); // property name is Spouse (capital S)
            TestObject objRod = (TestObject)lof.GetObject("rod");
            Assert.IsNull(objRod.Spouse, "Mmm, Spouse property appears to have been autowired by name, even though there is no object in the factory with a name 'Spouse'.");
        }

        [Test]
        public void AutowireObjectByNameWithDependencyCheck()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            RootObjectDefinition rod = new RootObjectDefinition(typeof(TestObject));
            lof.RegisterObjectDefinition("Spous", rod);
            Assert.Throws<UnsatisfiedDependencyException>(() => lof.Autowire(typeof(DependenciesObject), AutoWiringMode.ByName, true));
        }

        [Test]
        public void AutowireObjectByNameWithNoDependencyCheck()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            RootObjectDefinition rod = new RootObjectDefinition(typeof(TestObject));
            lof.RegisterObjectDefinition("Spous", rod);
            DependenciesObject obj = (DependenciesObject)lof.Autowire(typeof(DependenciesObject), AutoWiringMode.ByName, false);
            Assert.IsNull(obj.Spouse);
        }

        [Test]
        public void AutowireObjectByType()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            RootObjectDefinition rod = new RootObjectDefinition(typeof(TestObject));
            lof.RegisterObjectDefinition("test", rod);
            DependenciesObject obj = (DependenciesObject)lof.Autowire(typeof(DependenciesObject), AutoWiringMode.ByType, true);
            TestObject test = (TestObject)lof.GetObject("test");
            Assert.AreEqual(obj.Spouse, test);
        }

        [Test]
        public void AutowireObjectByTypeWithDependencyCheck()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            Assert.Throws<UnsatisfiedDependencyException>(() => lof.Autowire(typeof(DependenciesObject), AutoWiringMode.ByType, true));
        }

        [Test]
        public void AutowireObjectByTypeWithNoDependencyCheck()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            DependenciesObject obj = (DependenciesObject)lof.Autowire(typeof(DependenciesObject), AutoWiringMode.ByType, false);
            Assert.IsNull(obj.Spouse);
        }

        [Test]
        public void AutowireExistingObjectByName()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            RootObjectDefinition rod = new RootObjectDefinition(typeof(TestObject));
            lof.RegisterObjectDefinition("Spouse", rod);
            DependenciesObject existingObj = new DependenciesObject();
            lof.AutowireObjectProperties(existingObj, AutoWiringMode.ByName, true);
            TestObject spouse = (TestObject)lof.GetObject("Spouse");
            Assert.AreEqual(existingObj.Spouse, spouse);
            Assert.IsTrue(ObjectFactoryUtils.ObjectOfType(lof, typeof(TestObject)) == spouse);
        }

        [Test]
        public void AutowireExistingObjectByNameWithDependencyCheck()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            RootObjectDefinition rod = new RootObjectDefinition(typeof(TestObject));
            lof.RegisterObjectDefinition("Spous", rod);
            DependenciesObject existingObj = new DependenciesObject();
            Assert.Throws<UnsatisfiedDependencyException>(() => lof.AutowireObjectProperties(existingObj, AutoWiringMode.ByName, true));
        }

        [Test]
        public void AutowireExistingObjectByNameWithNoDependencyCheck()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            RootObjectDefinition rod = new RootObjectDefinition(typeof(TestObject));
            lof.RegisterObjectDefinition("Spous", rod);
            DependenciesObject existingObj = new DependenciesObject();
            lof.AutowireObjectProperties(existingObj, AutoWiringMode.ByName, false);
            Assert.IsNull(existingObj.Spouse);
        }

        [Test]
        public void AutowireExistingObjectByType()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            RootObjectDefinition rod = new RootObjectDefinition(typeof(TestObject));
            lof.RegisterObjectDefinition("test", rod);
            DependenciesObject existingObj = new DependenciesObject();
            lof.AutowireObjectProperties(existingObj, AutoWiringMode.ByType, true);
            TestObject test = (TestObject)lof.GetObject("test");
            Assert.AreEqual(existingObj.Spouse, test);
        }

        [Test]
        public void AutowireByTypeWithInvalidAutowireMode()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            DependenciesObject obj = new DependenciesObject();
            Assert.Throws<ArgumentException>(() => lof.AutowireObjectProperties(obj, AutoWiringMode.Constructor, true));
        }

        [Test]
        public void AutowireExistingObjectByTypeWithDependencyCheck()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            DependenciesObject existingObj = new DependenciesObject();
            Assert.Throws<UnsatisfiedDependencyException>(() => lof.AutowireObjectProperties(existingObj, AutoWiringMode.ByType, true));
        }

        [Test]
        public void AutowireExistingObjectByTypeWithNoDependencyCheck()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            DependenciesObject existingObj = new DependenciesObject();
            lof.AutowireObjectProperties(existingObj, AutoWiringMode.ByType, false);
            Assert.IsNull(existingObj.Spouse);
        }

        [Test]
        public void AutowireWithNoDependencies()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            RootObjectDefinition rod = new RootObjectDefinition(typeof(TestObject));
            lof.RegisterObjectDefinition("rod", rod);
            Assert.AreEqual(1, lof.ObjectDefinitionCount);
            object registered = lof.Autowire(typeof(NoDependencies), AutoWiringMode.AutoDetect, false);
            Assert.AreEqual(1, lof.ObjectDefinitionCount);
            Assert.IsTrue(registered is NoDependencies);
        }

        [Test]
        public void AutowireWithSatisfiedObjectDependency()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add(new PropertyValue("name", "Rod"));
            RootObjectDefinition rood = new RootObjectDefinition(typeof(TestObject), pvs);
            lof.RegisterObjectDefinition("rod", rood);
            Assert.AreEqual(1, lof.ObjectDefinitionCount);
            // Depends on age, name and spouse (TestObject)
            object registered = lof.Autowire(typeof(DependenciesObject), AutoWiringMode.AutoDetect, true);
            Assert.AreEqual(1, lof.ObjectDefinitionCount);
            DependenciesObject kerry = (DependenciesObject)registered;
            TestObject rod = (TestObject)lof.GetObject("rod");
            Assert.AreSame(rod, kerry.Spouse);
        }

        [Test]
        public void AutowireWithSatisfiedConstructorDependency()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add(new PropertyValue("name", "Rod"));
            RootObjectDefinition rood = new RootObjectDefinition(typeof(TestObject), pvs);
            lof.RegisterObjectDefinition("rod", rood);
            Assert.AreEqual(1, lof.ObjectDefinitionCount);
            object registered = lof.Autowire(typeof(ConstructorDependency), AutoWiringMode.AutoDetect, false);
            Assert.AreEqual(1, lof.ObjectDefinitionCount);
            ConstructorDependency kerry = (ConstructorDependency)registered;
            TestObject rod = (TestObject)lof.GetObject("rod");
            Assert.AreSame(rod, kerry._spouse);
        }

        [Test]
        public void AutowireWithUnsatisfiedConstructorDependency()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add(new PropertyValue("name", "Rod"));
            RootObjectDefinition rod = new RootObjectDefinition(typeof(TestObject), pvs);
            lof.RegisterObjectDefinition("rod", rod);
            Assert.AreEqual(1, lof.ObjectDefinitionCount);
            Assert.Throws<UnsatisfiedDependencyException>(() => lof.Autowire(typeof(UnsatisfiedConstructorDependency), AutoWiringMode.AutoDetect, true));
        }

        [Test]
        public void ExtensiveCircularReference()
        {
            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();
            for (int i = 0; i < 1000; i++)
            {
                MutablePropertyValues pvs = new MutablePropertyValues();
                pvs.Add(new PropertyValue("Spouse", new RuntimeObjectReference("object" + (i < 99 ? i + 1 : 0))));
                RootObjectDefinition rod = new RootObjectDefinition(typeof(TestObject), pvs);
                lof.RegisterObjectDefinition("object" + i, rod);
            }
            lof.PreInstantiateSingletons();
            for (int i = 0; i < 1000; i++)
            {
                TestObject obj = (TestObject)lof.GetObject("object" + i);
                TestObject otherObj = (TestObject)lof.GetObject("object" + (i < 99 ? i + 1 : 0));
                Assert.IsTrue(obj.Spouse == otherObj);
            }
        }

        [Test]
        public void PullingObjectWithFactoryMethodAlsoInjectsDependencies()
        {
            string expectedName = "Terese Raquin";
            MutablePropertyValues props = new MutablePropertyValues();
            props.Add(new PropertyValue("Name", expectedName));

            RootObjectDefinition def = new RootObjectDefinition(typeof(MySingleton), props);
            def.FactoryMethodName = "GetInstance";

            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            fac.RegisterObjectDefinition("foo", def);

            object foo = fac["foo"];
            Assert.IsNotNull(foo, "Couldn't pull manually registered instance out of the factory using factory method instantiation.");
            MySingleton sing = (MySingleton)foo;
            Assert.AreEqual(expectedName, sing.Name, "Dependency was not resolved pulling manually registered instance out of the factory using factory method instantiation.");
        }

        [Test]
        public void GetObjectWithArgsOnFactoryObject()
        {
            using (DefaultListableObjectFactory lof = new DefaultListableObjectFactory())
            {
                // DummyFactory produces a TestObject
                RootObjectDefinition factoryObjectDef = new RootObjectDefinition(typeof(DummyFactory));
                factoryObjectDef.IsSingleton = true;
                lof.RegisterObjectDefinition("factoryObject", factoryObjectDef);

                // verify preconditions
                TestObject to = lof.GetObject("factoryObject", null, null) as TestObject;
                Assert.IsNotNull(to);
                Assert.AreEqual(25, to.Age);
                Assert.AreEqual(DummyFactory.SINGLETON_NAME, to.Name);

                try
                {
                    to = lof.GetObject("factoryObject", new object[] {}) as TestObject;
                    Assert.Fail("should throw ObjectDefinitionStoreException");
                }
                catch (ObjectDefinitionStoreException ex)
                {
                    Assert.IsTrue( ex.Message.IndexOf("Cannot specify arguments in the GetObject () method when referring to a factory object definition") > -1);
                }

                try
                {
                    to = lof.GetObject("factoryObject", new object[] { "Mark", "35" }) as TestObject;
                    Assert.Fail("should throw ObjectDefinitionStoreException");
                }
                catch (ObjectDefinitionStoreException ex)
                {
                    Assert.IsTrue( ex.Message.IndexOf("Cannot specify arguments in the GetObject () method when referring to a factory object definition") > -1);
                }
            }
        }

        [Test(Description = "http://opensource.atlassian.com/projects/spring/browse/SPRNET-368")]
        public void GetObjectWithCtorArgsAndCtorAutowiring()
        {
            using (DefaultListableObjectFactory lof = new DefaultListableObjectFactory())
            {
                RootObjectDefinition prototype
                    = new RootObjectDefinition(typeof(TestObject));
                prototype.IsSingleton = false;
                lof.RegisterObjectDefinition("prototype", prototype);

                TestObject to = lof.GetObject("prototype", new object[] { "Bruno", 26, new NestedTestObject("Home") }) as TestObject;
                Assert.IsNotNull(to);
                Assert.AreEqual(26, to.Age);
                Assert.AreEqual("Bruno", to.Name);
                Assert.AreEqual("Home", to.Doctor.Company);
            }
        }

        [Test]
        public void GetObjectWithCtorArgsOnPrototype()
        {
            using (DefaultListableObjectFactory lof = new DefaultListableObjectFactory())
            {
                RootObjectDefinition prototype
                    = new RootObjectDefinition(typeof(TestObject));
                prototype.IsSingleton = false;
                lof.RegisterObjectDefinition("prototype", prototype);

                TestObject to = lof.GetObject("prototype", new object[] { "Mark", 35 }) as TestObject;
                Assert.IsNotNull(to);
                Assert.AreEqual(35, to.Age);
                Assert.AreEqual("Mark", to.Name);
            }
        }

        [Test]
        [Ignore("Ordering must now be strict when providing array of arguments for ctors")]
        public void GetObjectWithCtorArgsOnPrototypeOutOfOrderArgs()
        {
            using (DefaultListableObjectFactory lof = new DefaultListableObjectFactory())
            {
                RootObjectDefinition prototype
                    = new RootObjectDefinition(typeof(TestObject));
                prototype.IsSingleton = false;
                lof.RegisterObjectDefinition("prototype", prototype);

                try
                {
                    TestObject to2 = lof.GetObject("prototype", new object[] { 35, "Mark" }) as TestObject;
                    Assert.IsNotNull(to2);
                    Assert.AreEqual(35, to2.Age);
                    Assert.AreEqual("Mark", to2.Name);
                }
                catch (ObjectCreationException ex)
                {
                    Assert.IsTrue(ex.Message.IndexOf("'Object of type 'System.Int32' cannot be converted to type 'System.String'") >= 0);
                }
            }
        }

        [Test]
        public void GetObjectWithCtorArgsOnSingleton()
        {
            using (DefaultListableObjectFactory lof = new DefaultListableObjectFactory())
            {
                RootObjectDefinition singleton
                    = new RootObjectDefinition(typeof(TestObject));
                singleton.IsSingleton = true;
                lof.RegisterObjectDefinition("singleton", singleton);

                TestObject to = lof.GetObject("singleton", new object[] { "Mark", 35 }) as TestObject;
                Assert.IsNotNull(to);
                Assert.AreEqual(35, to.Age);
                Assert.AreEqual("Mark", to.Name);
            }
        }

        [Test]
        public void GetObjectWithCtorArgsOverrided()
        {
            using (DefaultListableObjectFactory lof = new DefaultListableObjectFactory())
            {
                ConstructorArgumentValues arguments = new ConstructorArgumentValues();
                arguments.AddNamedArgumentValue("age", 27);
                arguments.AddNamedArgumentValue("name", "Bruno");
                RootObjectDefinition singleton
                    = new RootObjectDefinition(typeof(TestObject), arguments, new MutablePropertyValues());
                singleton.IsSingleton = true;
                lof.RegisterObjectDefinition("singleton", singleton);

                TestObject to = lof.GetObject("singleton", new object[] { "Mark", 35 }) as TestObject;
                Assert.IsNotNull(to);
                Assert.AreEqual(35, to.Age);
                Assert.AreEqual("Mark", to.Name);

                TestObject to2 = lof.GetObject("singleton") as TestObject;
                Assert.IsNotNull(to2);
                Assert.AreEqual(27, to2.Age);
                Assert.AreEqual("Bruno", to2.Name);
            }
        }

        [Test(Description = "http://opensource.atlassian.com/projects/spring/browse/SPRNET-368")]
        public void CreateObjectWithCtorArgsAndCtorAutowiring()
        {
            using (DefaultListableObjectFactory lof = new DefaultListableObjectFactory())
            {
                RootObjectDefinition prototype = new RootObjectDefinition(typeof(TestObject));
                prototype.IsSingleton = false;
                lof.RegisterObjectDefinition("prototype", prototype);

                TestObject to = lof.CreateObject("prototype", typeof(TestObject), new object[] { "Bruno", 26, new NestedTestObject("Home") }) as TestObject;
                Assert.IsNotNull(to);
                Assert.AreEqual(26, to.Age);
                Assert.AreEqual("Bruno", to.Name);
                Assert.AreEqual("Home", to.Doctor.Company);
            }
        }

        [Test]
        public void CreateObjectWithCtorArgsOnPrototype()
        {
            using (DefaultListableObjectFactory lof = new DefaultListableObjectFactory())
            {
                RootObjectDefinition prototype = new RootObjectDefinition(typeof(TestObject));
                prototype.IsSingleton = false;
                lof.RegisterObjectDefinition("prototype", prototype);

                TestObject to = lof.CreateObject("prototype", typeof(TestObject), new object[] { "Mark", 35 }) as TestObject;
                Assert.IsNotNull(to);
                Assert.AreEqual(35, to.Age);
                Assert.AreEqual("Mark", to.Name);
            }
        }

        [Test]
        [Ignore("Ordering must now be strict when providing array of arguments for ctors")]
        public void CreateObjectWithCtorArgsOnPrototypeOutOfOrderArgs()
        {
            using (DefaultListableObjectFactory lof = new DefaultListableObjectFactory())
            {
                RootObjectDefinition prototype
                    = new RootObjectDefinition(typeof(TestObject));
                prototype.IsSingleton = false;
                lof.RegisterObjectDefinition("prototype", prototype);

                try
                {
                    TestObject to2 = lof.CreateObject("prototype", typeof(TestObject), new object[] { 35, "Mark" }) as TestObject;
                    Assert.IsNotNull(to2);
                    Assert.AreEqual(35, to2.Age);
                    Assert.AreEqual("Mark", to2.Name);
                }
                catch (ObjectCreationException ex)
                {
                    Assert.IsTrue(ex.Message.IndexOf("'Object of type 'System.Int32' cannot be converted to type 'System.String'") >= 0);
                }
            }
        }

        [Test]
        public void CreateObjectWithCtorArgsOnSingleton()
        {
            using (DefaultListableObjectFactory lof = new DefaultListableObjectFactory())
            {
                RootObjectDefinition singleton
                    = new RootObjectDefinition(typeof(TestObject));
                singleton.IsSingleton = true;
                lof.RegisterObjectDefinition("singleton", singleton);

                TestObject to = lof.CreateObject("singleton", typeof(TestObject), new object[] { "Mark", 35 }) as TestObject;
                Assert.IsNotNull(to);
                Assert.AreEqual(35, to.Age);
                Assert.AreEqual("Mark", to.Name);
            }
        }

        [Test]
        public void CreateObjectWithCtorArgsOverrided()
        {
            using (DefaultListableObjectFactory lof = new DefaultListableObjectFactory())
            {
                ConstructorArgumentValues arguments = new ConstructorArgumentValues();
                arguments.AddNamedArgumentValue("age", 27);
                arguments.AddNamedArgumentValue("name", "Bruno");
                RootObjectDefinition singleton = new RootObjectDefinition(typeof(TestObject), arguments, new MutablePropertyValues());
                singleton.IsSingleton = true;
                lof.RegisterObjectDefinition("singleton", singleton);

                TestObject to = lof.CreateObject("singleton", typeof(TestObject), new object[] { "Mark", 35 }) as TestObject;
                Assert.IsNotNull(to);
                Assert.AreEqual(35, to.Age);
                Assert.AreEqual("Mark", to.Name);

                TestObject to2 = lof.CreateObject("singleton", null, null) as TestObject;
                Assert.IsNotNull(to2);
                Assert.AreEqual(27, to2.Age);
                Assert.AreEqual("Bruno", to2.Name);
            }
        }

        [Test]
        public void CreateObjectDoesNotConfigure()
        {
            using (DefaultListableObjectFactory lof = new DefaultListableObjectFactory())
            {
                MutablePropertyValues properties = new MutablePropertyValues();
                properties.Add(new PropertyValue("Age", 27));
                properties.Add(new PropertyValue("Name", "Bruno"));
                RootObjectDefinition singleton = new RootObjectDefinition(typeof(TestObject), null, properties);
                singleton.IsSingleton = true;
                lof.RegisterObjectDefinition("singleton", singleton);

                TestObject to2 = lof.CreateObject("singleton", typeof(TestObject), null) as TestObject;
                Assert.IsNotNull(to2);
                // no props set
                Assert.AreEqual(0, to2.Age);
                Assert.AreEqual(null, to2.Name);
                // no object postprocessors executed!
                Assert.AreEqual(null, to2.ObjectName);
                Assert.AreEqual(null, to2.ObjectFactory);
                Assert.AreEqual(null, to2.SharedState);
            }
        }

        [Test]
        public void CreateObjectDoesAffectContainerManagedSingletons()
        {
            using (DefaultListableObjectFactory lof = new DefaultListableObjectFactory())
            {
                lof.AddObjectPostProcessor(new SharedStateAwareProcessor(new ISharedStateFactory[] { new ByTypeSharedStateFactory() }, Int32.MaxValue ));

                MutablePropertyValues properties = new MutablePropertyValues();
                properties.Add(new PropertyValue("Age", 27));
                properties.Add(new PropertyValue("Name", "Bruno"));
                RootObjectDefinition singleton = new RootObjectDefinition(typeof(TestObject), null, properties);
                singleton.IsSingleton = true;
                lof.RegisterObjectDefinition("singleton", singleton);

                // a call to GetObject() results in a normally configured instance
                TestObject to = lof.GetObject("singleton") as TestObject;
                Assert.IsNotNull(to);
                // props set
                Assert.AreEqual(27, to.Age);
                Assert.AreEqual("Bruno", to.Name);
                // object postprocessors executed!
                Assert.AreEqual("singleton", to.ObjectName);
                Assert.AreEqual(lof, to.ObjectFactory);
                Assert.IsNotNull(to.SharedState);

                // altough configured as singleton, calling CreateObject prevents the instance from being cached
                // otherwise the container could not guarantee the results of calling GetObject() to other clients.
                TestObject to2 = lof.CreateObject("singleton", typeof(TestObject), null) as TestObject;
                Assert.IsNotNull(to2);
                // no props set
                Assert.AreEqual(0, to2.Age);
                Assert.AreEqual(null, to2.Name);
                // no object postprocessors executed!
                Assert.AreEqual(null, to2.ObjectName);
                Assert.AreEqual(null, to2.ObjectFactory);
                Assert.AreEqual(null, to2.SharedState);

                // a call to GetObject() results in a normally configured instance
                TestObject to3 = lof.GetObject("singleton") as TestObject;
                Assert.IsNotNull(to3);
                // props set
                Assert.AreEqual(27, to3.Age);
                Assert.AreEqual("Bruno", to3.Name);
                // object postprocessors executed!
                Assert.AreEqual("singleton", to3.ObjectName);
                Assert.AreEqual(lof, to3.ObjectFactory);
                Assert.IsNotNull(to3.SharedState);
            }
        }

        [Test]
        public void CreateObjectWithAllNamedCtorArguments()
        {
            string expectedName = "Bingo";
            int expectedAge = 1023;
            ConstructorArgumentValues values = new ConstructorArgumentValues();
            values.AddNamedArgumentValue("age", expectedAge);
            values.AddNamedArgumentValue("name", expectedName);
            RootObjectDefinition def = new RootObjectDefinition(typeof(TestObject), values, new MutablePropertyValues());
            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            fac.RegisterObjectDefinition("foo", def);

            ITestObject foo = fac["foo"] as ITestObject;
            Assert.IsNotNull(foo, "Couldn't pull manually registered instance out of the factory.");
            Assert.AreEqual(expectedName, foo.Name, "Dependency 'name' was not resolved using a named ctor arg.");
            Assert.AreEqual(expectedAge, foo.Age, "Dependency 'age' was not resolved using a named ctor arg.");
        }

        [Test]
        public void CreateObjectWithAllNamedCtorArgumentsIsCaseInsensitive()
        {
            string expectedName = "Bingo";
            int expectedAge = 1023;
            ConstructorArgumentValues values = new ConstructorArgumentValues();
            values.AddNamedArgumentValue("aGe", expectedAge);
            values.AddNamedArgumentValue("naME", expectedName);
            RootObjectDefinition def = new RootObjectDefinition(typeof(TestObject), values, new MutablePropertyValues());
            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            fac.RegisterObjectDefinition("foo", def);

            ITestObject foo = fac["foo"] as ITestObject;
            Assert.IsNotNull(foo, "Couldn't pull manually registered instance out of the factory.");
            Assert.AreEqual(expectedName, foo.Name, "Dependency 'name' was not resolved using a named ctor arg.");
            Assert.AreEqual(expectedAge, foo.Age, "Dependency 'age' was not resolved using a named ctor arg.");
        }

        [Test]
        public void CreateObjectWithMixOfNamedAndIndexedCtorArguments()
        {
            string expectedName = "Bingo";
            int expectedAge = 1023;
            ConstructorArgumentValues values = new ConstructorArgumentValues();
            values.AddNamedArgumentValue("age", expectedAge);
            values.AddIndexedArgumentValue(0, expectedName);
            RootObjectDefinition def = new RootObjectDefinition(typeof(TestObject), values, new MutablePropertyValues());
            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            fac.RegisterObjectDefinition("foo", def);

            ITestObject foo = fac["foo"] as ITestObject;
            Assert.IsNotNull(foo, "Couldn't pull manually registered instance out of the factory.");
            Assert.AreEqual(expectedName, foo.Name, "Dependency 'name' was not resolved an indexed ctor arg.");
            Assert.AreEqual(expectedAge, foo.Age, "Dependency 'age' was not resolved using a named ctor arg.");
        }

        [Test]
        public void CreateObjectWithMixOfNamedAndIndexedAndAutowiredCtorArguments()
        {
            string expectedCompany = "Griffin's Foosball Arcade";
            MutablePropertyValues autoProps = new MutablePropertyValues();
            autoProps.Add(new PropertyValue("Company", expectedCompany));
            RootObjectDefinition autowired = new RootObjectDefinition(typeof(NestedTestObject), autoProps);

            string expectedName = "Bingo";
            int expectedAge = 1023;
            ConstructorArgumentValues values = new ConstructorArgumentValues();
            values.AddNamedArgumentValue("age", expectedAge);
            values.AddIndexedArgumentValue(0, expectedName);
            RootObjectDefinition def = new RootObjectDefinition(typeof(TestObject), values, new MutablePropertyValues());
            def.AutowireMode = AutoWiringMode.Constructor;
            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            fac.RegisterObjectDefinition("foo", def);
            fac.RegisterObjectDefinition("doctor", autowired);

            ITestObject foo = fac["foo"] as ITestObject;
            Assert.IsNotNull(foo, "Couldn't pull manually registered instance out of the factory.");
            Assert.AreEqual(expectedName, foo.Name, "Dependency 'name' was not resolved an indexed ctor arg.");
            Assert.AreEqual(expectedAge, foo.Age, "Dependency 'age' was not resolved using a named ctor arg.");
            Assert.AreEqual(expectedCompany, foo.Doctor.Company, "Dependency 'doctor.Company' was not resolved using autowiring.");
        }

        [Test]
        public void CreateObjectWithMixOfIndexedAndTwoNamedSameTypeCtorArguments()
        {
            // this object will be passed in as a named constructor argument
            string expectedCompany = "Griffin's Foosball Arcade";
            MutablePropertyValues autoProps = new MutablePropertyValues();
            autoProps.Add(new PropertyValue("Company", expectedCompany));
            RootObjectDefinition autowired = new RootObjectDefinition(typeof(NestedTestObject), autoProps);

            // this object will be passed in as a named constructor argument
            string expectedLawyersCompany = "Pollack, Pounce, & Pulverise";
            MutablePropertyValues lawyerProps = new MutablePropertyValues();
            lawyerProps.Add(new PropertyValue("Company", expectedLawyersCompany));
            RootObjectDefinition lawyer = new RootObjectDefinition(typeof(NestedTestObject), lawyerProps);

            // this simple string object will be passed in as an indexed constructor argument
            string expectedName = "Bingo";

            // this simple integer object will be passed in as a named constructor argument
            int expectedAge = 1023;

            ConstructorArgumentValues values = new ConstructorArgumentValues();

            // lets mix the order up a little...
            values.AddNamedArgumentValue("age", expectedAge);
            values.AddIndexedArgumentValue(0, expectedName);
            values.AddNamedArgumentValue("doctor", new RuntimeObjectReference("a_doctor"));
            values.AddNamedArgumentValue("lawyer", new RuntimeObjectReference("a_lawyer"));

            RootObjectDefinition def = new RootObjectDefinition(typeof(TestObject), values, new MutablePropertyValues());

            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            // the object we're attempting to resolve...
            fac.RegisterObjectDefinition("foo", def);
            // the object that will be looked up and passed as a named parameter to a ctor call...
            fac.RegisterObjectDefinition("a_doctor", autowired);
            // another object that will be looked up and passed as a named parameter to a ctor call...
            fac.RegisterObjectDefinition("a_lawyer", lawyer);

            ITestObject foo = fac["foo"] as ITestObject;
            Assert.IsNotNull(foo, "Couldn't pull manually registered instance out of the factory.");
            Assert.AreEqual(expectedName, foo.Name, "Dependency 'name' was not resolved an indexed ctor arg.");
            Assert.AreEqual(expectedAge, foo.Age, "Dependency 'age' was not resolved using a named ctor arg.");
            Assert.AreEqual(expectedCompany, foo.Doctor.Company, "Dependency 'doctor.Company' was not resolved using autowiring.");
            Assert.AreEqual(expectedLawyersCompany, foo.Lawyer.Company, "Dependency 'lawyer.Company' was not resolved using another named ctor arg.");
        }

        [Test]
        public void CircularDependencyIsCorrectlyDetected()
        {
            RootObjectDefinition foo = new RootObjectDefinition(typeof(TestObject));
            foo.ConstructorArgumentValues.AddNamedArgumentValue("spouse", new RuntimeObjectReference("bar"));
            RootObjectDefinition bar = new RootObjectDefinition(typeof(TestObject));
            bar.ConstructorArgumentValues.AddNamedArgumentValue("spouse", new RuntimeObjectReference("foo"));

            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            fac.RegisterObjectDefinition("foo", foo);
            fac.RegisterObjectDefinition("bar", bar);

            try
            {
                fac.GetObject("foo");
            }
            catch (ObjectCreationException ex)
            {
                Assert.AreEqual(typeof(ObjectCurrentlyInCreationException), ex.GetBaseException().GetType(),
                                "Circular dependency was set up; should have caught an ObjectCurrentlyInCreationException instance.");
            }
        }

        [Test]
        public void ConfigurableFactoryObjectInline()
        {
            DefaultListableObjectFactory dlof = new DefaultListableObjectFactory();

            RootObjectDefinition everyman = new RootObjectDefinition();
            everyman.PropertyValues = new MutablePropertyValues();
            everyman.PropertyValues.Add("name", "Noone");
            everyman.PropertyValues.Add("age", 9781);

            RootObjectDefinition factory = new RootObjectDefinition();
            factory.ObjectType = typeof(DummyConfigurableFactory);
            factory.PropertyValues = new MutablePropertyValues();
            factory.PropertyValues.Add("ProductTemplate", everyman);
            dlof.RegisterObjectDefinition("factory", factory);

            TestObject instance = dlof.GetObject("factory") as TestObject;
            Assert.IsNotNull(instance);

            Assert.AreEqual("Noone", instance.Name, "Name dependency injected via IConfigurableObjectFactory (instance) failed (was 'Factory singleton').");
            Assert.AreEqual(9781, instance.Age, "Age dependency injected via IObjectFactory.ConfigureObject(instance) failed (was 25).");
        }

        [Test]
        public void ConfigurableFactoryObjectReference()
        {
            DefaultListableObjectFactory dlof = new DefaultListableObjectFactory();

            RootObjectDefinition everyman = new RootObjectDefinition();
            everyman.IsAbstract = true;
            everyman.PropertyValues = new MutablePropertyValues();
            everyman.PropertyValues.Add("name", "Noone");
            everyman.PropertyValues.Add("age", 9781);
            dlof.RegisterObjectDefinition("everyman", everyman);

            RootObjectDefinition factory = new RootObjectDefinition();
            factory.ObjectType = typeof(DummyConfigurableFactory);
            factory.PropertyValues = new MutablePropertyValues();
            factory.PropertyValues.Add("ProductTemplate", new RuntimeObjectReference("everyman"));
            dlof.RegisterObjectDefinition("factory", factory);

            TestObject instance = dlof.GetObject("factory") as TestObject;
            Assert.IsNotNull(instance);

            Assert.AreEqual("Noone", instance.Name, "Name dependency injected via IConfigurableObjectFactory (instance) failed (was 'Factory singleton').");
            Assert.AreEqual(9781, instance.Age, "Age dependency injected via IObjectFactory.ConfigureObject(instance) failed (was 25).");
        }

        [Test]
        public void ConfigureObject()
        {
            TestObject instance = new TestObject();
            RootObjectDefinition everyman = new RootObjectDefinition();
            everyman.IsAbstract = true;
            everyman.PropertyValues = new MutablePropertyValues();
            everyman.PropertyValues.Add("name", "Noone");
            everyman.PropertyValues.Add("age", 9781);
            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            fac.RegisterObjectDefinition(instance.GetType().FullName, everyman);
            fac.ConfigureObject(instance, instance.GetType().FullName);
            Assert.AreEqual("Noone", instance.Name, "Name dependency injected via IObjectFactory.ConfigureObject(instance) failed (was null).");
            Assert.AreEqual(9781, instance.Age, "Age dependency injected via IObjectFactory.ConfigureObject(instance) failed (was null).");
        }

        [Test]
        public void ConfigureObjectViaExplicitName()
        {
            TestObject instance = new TestObject();
            RootObjectDefinition everyman = new RootObjectDefinition();
            everyman.IsAbstract = true;
            everyman.PropertyValues = new MutablePropertyValues();
            everyman.PropertyValues.Add("name", "Noone");
            everyman.PropertyValues.Add("age", 9781);
            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            fac.RegisterObjectDefinition("everyman", everyman);
            fac.ConfigureObject(instance, "everyman");
            Assert.AreEqual(true, instance.InitCompleted, "AfterPropertiesSet() was not invoked by IObjectFactory.ConfigureObject(instance).");
            Assert.AreEqual("Noone", instance.Name, "Name dependency injected via IObjectFactory.ConfigureObject(instance) failed (was null).");
            Assert.AreEqual(9781, instance.Age, "Age dependency injected via IObjectFactory.ConfigureObject(instance) failed (was null).");
        }

        [Test]
        public void ConfigureObjectViaNullName()
        {
            TestObject instance = new TestObject();
            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            Assert.Throws<ArgumentNullException>(() => fac.ConfigureObject(instance, null));
        }

        [Test]
        public void ConfigureObjectViaLoadOfOldWhitespaceName()
        {
            TestObject instance = new TestObject();
            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            Assert.Throws<ArgumentException>(() => fac.ConfigureObject(instance, "        \t"));
        }

        [Test]
        public void ConfigureObjectViaEmptyName()
        {
            TestObject instance = new TestObject();
            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            Assert.Throws<ArgumentException>(() => fac.ConfigureObject(instance, string.Empty));
        }

        [Test]
        public void DisposeCyclesThroughAllSingletonsEvenIfTheirDisposeThrowsAnException()
        {
            RootObjectDefinition foo = new RootObjectDefinition(typeof(GoodDisposable));
            foo.IsSingleton = true;
            RootObjectDefinition bar = new RootObjectDefinition(typeof(BadDisposable));
            bar.IsSingleton = true;
            RootObjectDefinition baz = new RootObjectDefinition(typeof(GoodDisposable));
            baz.IsSingleton = true;

            using (DefaultListableObjectFactory fac = new DefaultListableObjectFactory())
            {
                fac.RegisterObjectDefinition("foo", foo);
                fac.RegisterObjectDefinition("bar", bar);
                fac.RegisterObjectDefinition("baz", baz);
                fac.PreInstantiateSingletons();
            }
            Assert.AreEqual(2, GoodDisposable.DisposeCount, "All IDisposable singletons must have their Dispose() method called... one of them bailed, and as a result the rest were (apparently) not Dispose()d.");
            GoodDisposable.DisposeCount = 0;
        }

        [Test]
        public void StaticInitializationViaDependsOnSingletonMethodInvokingFactoryObject()
        {
            RootObjectDefinition initializer = new RootObjectDefinition(typeof(MethodInvokingFactoryObject));
            initializer.PropertyValues.Add("TargetMethod", "Init");
            initializer.PropertyValues.Add("TargetType", typeof(StaticInitializer).AssemblyQualifiedName);

            RootObjectDefinition foo = new RootObjectDefinition(typeof(TestObject));
            foo.DependsOn = new string[] { "force-init" };

            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            fac.RegisterObjectDefinition("foo", foo);
            fac.RegisterObjectDefinition("force-init", initializer);

            fac.GetObject("foo");
            Assert.IsTrue(StaticInitializer.InitWasCalled, "Boing");
        }

        /// <summary>
        /// There is a similar test in XmlObjectFactoryTests that actually supplies another boolean
        /// object in the factory that is used to autowire the object; this test puts no such second
        /// object in the factory, so when the factory tries to autowire the second (missing) argument
        /// to the ctor, it should (must) choke.
        /// </summary>
        [Test]
        public void DoubleBooleanAutowire()
        {
            RootObjectDefinition def = new RootObjectDefinition(typeof(DoubleBooleanConstructorObject));
            ConstructorArgumentValues args = new ConstructorArgumentValues();
            args.AddGenericArgumentValue(true, "bool");
            def.ConstructorArgumentValues = args;
            def.AutowireMode = AutoWiringMode.Constructor;
            def.IsSingleton = true;

            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            fac.RegisterObjectDefinition("foo", def);

            Assert.Throws<UnsatisfiedDependencyException>(() => fac.GetObject("foo"),
                "Error creating object with name 'foo' : Unsatisfied dependency " +
                "expressed through constructor argument with index 1 of type [System.Boolean] : " +
                "No unique object of type [System.Boolean] is defined : Unsatisfied dependency of type [System.Boolean]: expected at least 1 matching object to wire the [b2] parameter on the constructor of object [foo]");
        }

        [Test]
        public void CanSetPropertyThatUsesNewModifierOnDerivedClass()
        {
            string nick = "Banjo";
            string expectedNickname = DerivedTestObject.NicknamePrefix + nick;

            RootObjectDefinition def = new RootObjectDefinition(typeof(DerivedTestObject));
            def.PropertyValues.Add("Nickname", nick);

            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            fac.RegisterObjectDefinition("foo", def);

            DerivedTestObject tob = (DerivedTestObject)fac.GetObject("foo");
            Assert.AreEqual(expectedNickname, tob.Nickname,
                "Property is not being set to the NEWed property on the subclass.");
        }

        [Test]
        public void CanSetPropertyThatUsesOddNewModifierOnDerivedClass()
        {
            RootObjectDefinition def = new RootObjectDefinition(typeof(DerivedFoo));
            def.PropertyValues.Add("Bar", new DerivedBar());
            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();
            fac.RegisterObjectDefinition("foo", def);
            DerivedFoo foo = (DerivedFoo)fac.GetObject("foo");
            Assert.AreEqual(typeof(DerivedBar), foo.Bar.GetType());
        }

        [Test]
        public void ChildReferencesParentByAnAliasOfTheParent()
        {
            const string TheParentsAlias = "theParentsAlias";
            const int ExpectedAge = 31;
            const string ExpectedName = "Rick Evans";

            RootObjectDefinition parentDef = new RootObjectDefinition(typeof(TestObject));
            parentDef.IsAbstract = true;
            parentDef.PropertyValues.Add("name", ExpectedName);
            parentDef.PropertyValues.Add("age", ExpectedAge);

            ChildObjectDefinition childDef = new ChildObjectDefinition(TheParentsAlias);

            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();

            fac.RegisterObjectDefinition("parent", parentDef);
            fac.RegisterAlias("parent", TheParentsAlias);
            fac.RegisterObjectDefinition("child", childDef);

            TestObject child = (TestObject)fac.GetObject("child");
            Assert.AreEqual(ExpectedName, child.Name);
            Assert.AreEqual(ExpectedAge, child.Age);
        }

        [Test]
        public void GetObjectByTypeWithAmbiguity()
        {
		    DefaultListableObjectFactory lbf = new DefaultListableObjectFactory();
		    RootObjectDefinition bd1 = new RootObjectDefinition(typeof(TestObject));
            RootObjectDefinition bd2 = new RootObjectDefinition(typeof(TestObject));
		    lbf.RegisterObjectDefinition("bd1", bd1);
		    lbf.RegisterObjectDefinition("bd2", bd2);

            Assert.That(() => lbf.GetObject<TestObject>(), Throws.Exception.TypeOf<NoSuchObjectDefinitionException>());
        }

        [Test]
        public void GetObjectByTypeFiltersOutNonAutowireCandidates()
        {
		    DefaultListableObjectFactory lbf = new DefaultListableObjectFactory();
		    RootObjectDefinition bd1 = new RootObjectDefinition(typeof(TestObject));
		    RootObjectDefinition bd2 = new RootObjectDefinition(typeof(TestObject));
		    RootObjectDefinition na1 = new RootObjectDefinition(typeof(TestObject));
		    na1.IsAutowireCandidate = false;

		    lbf.RegisterObjectDefinition("bd1", bd1);
		    lbf.RegisterObjectDefinition("na1", na1);
		    TestObject actual = lbf.GetObject<TestObject>(); // na1 was filtered
            Assert.That(lbf.GetObject("bd1", typeof(TestObject)), Is.SameAs(actual));

		    lbf.RegisterObjectDefinition("bd2", bd2);
            Assert.That(() => lbf.GetObject<TestObject>(), Throws.Exception.TypeOf<NoSuchObjectDefinitionException>());
        }

        [Test]
        public void GetObjectDefinitionResolvesAliases()
        {
            const string TheParentsAlias = "theParentsAlias";
            const int ExpectedAge = 31;
            const string ExpectedName = "Rick Evans";

            RootObjectDefinition parentDef = new RootObjectDefinition(typeof(TestObject));
            parentDef.IsAbstract = true;
            parentDef.PropertyValues.Add("name", ExpectedName);
            parentDef.PropertyValues.Add("age", ExpectedAge);

            ChildObjectDefinition childDef = new ChildObjectDefinition(TheParentsAlias);

            DefaultListableObjectFactory fac = new DefaultListableObjectFactory();

            fac.RegisterObjectDefinition("parent", parentDef);
            fac.RegisterAlias("parent", TheParentsAlias);

            IObjectDefinition od = fac.GetObjectDefinition(TheParentsAlias);
            Assert.IsNotNull(od);
        }

        [Test]
        public void IgnoreObjectPostProcessorDuplicates()
        {
            IObjectPostProcessor proc1 = FakeItEasy.A.Fake<IObjectPostProcessor>();

            DefaultListableObjectFactory lof = new DefaultListableObjectFactory();

            const string errMsg = "Wrong number of IObjectPostProcessors being reported by the ObjectPostProcessorCount property.";
            Assert.AreEqual(0, lof.ObjectPostProcessorCount, errMsg);
            lof.AddObjectPostProcessor(proc1);
            Assert.AreEqual(1, lof.ObjectPostProcessorCount, errMsg);
            lof.AddObjectPostProcessor(proc1);
            Assert.AreEqual(1, lof.ObjectPostProcessorCount, errMsg);
        }

        [Test]
        public void ConfigureObjectReturnsOriginalInstanceIfNoDefinitionFound()
        {
            IConfigurableObjectFactory of = new DefaultListableObjectFactory();
            object testObject = new object();
            object resultObject = of.ConfigureObject(testObject, "non-existing object definition name");
            Assert.AreSame(testObject, resultObject);
        }

        private class TestObjectPostProcessor : IObjectPostProcessor
        {
            private object other;

            public TestObjectPostProcessor(object other)
            {
                this.other = other;
            }

            public object PostProcessBeforeInitialization(object instance, string name)
            {
                return instance;
            }

            public object PostProcessAfterInitialization(object instance, string objectName)
            {
                return other;
            }
        }

        [Test]
        public void ConfigureObjectAppliesObjectPostProcessorsUsingDefinition()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            object wrapperObject = "WrapperObject";
            of.AddObjectPostProcessor( new TestObjectPostProcessor(wrapperObject));
            of.RegisterObjectDefinition("myObjectDefinition", new RootObjectDefinition());

            object testObject = "TestObject";
            object resultObject = of.ConfigureObject(testObject, "myObjectDefinition");
            Assert.AreSame(wrapperObject, resultObject);
        }

        [Test]
        public void ConfigureObjectDoesNotApplyObjectPostProcessorsIfNoDefinition()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            object wrapperObject = "WrapperObject";
            of.AddObjectPostProcessor( new TestObjectPostProcessor(wrapperObject));

            object testObject = "TestObject";
            object resultObject = of.ConfigureObject(testObject, "non-existant definition");
            Assert.AreSame(testObject, resultObject);
        }

        [Test]
        public void HierarchicalObjectFactoryChildParentResolution()
        {
            DefaultListableObjectFactory parent = new DefaultListableObjectFactory();
            DefaultListableObjectFactory child = new DefaultListableObjectFactory(parent);
            parent.RegisterSingleton("parent", new Parent());
            child.RegisterObjectDefinition("child", new RootObjectDefinition(typeof (Child), AutoWiringMode.AutoDetect));
            Child c = (Child) child.GetObject("child");
            Assert.IsNotNull(c);
        }

        private class A : IFactoryObject, ISerializable
        {
            public object GetObject()
            {
                throw new NotImplementedException();
            }

            public Type ObjectType
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsSingleton
            {
                get { throw new NotImplementedException(); }
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void GetObjectDefinitionNamesOnlyFromChild()
        {
            DefaultListableObjectFactory parent = new DefaultListableObjectFactory();
            parent.RegisterObjectDefinition("testChild", new RootObjectDefinition(typeof(TestObject), null));
            DefaultListableObjectFactory child = new DefaultListableObjectFactory(parent);
            child.RegisterObjectDefinition("testParent", new RootObjectDefinition(typeof(NestedTestObject), null));

            var names = child.GetObjectDefinitionNames();

            Assert.That(names, Has.Count.EqualTo(1), "GetObjectDefinitionNames() should only return object definition names from this factory");

            names = child.GetObjectDefinitionNames(false);

            Assert.That(names, Has.Count.EqualTo(1), "GetObjectDefinitionNames(false) should only return object definition names from this factory");
        }

        [Test]
        public void GetObjectDefinitionNamesIncludingParent()
        {
            DefaultListableObjectFactory parent = new DefaultListableObjectFactory();
            parent.RegisterObjectDefinition("testChild", new RootObjectDefinition(typeof(TestObject), null));
            DefaultListableObjectFactory child = new DefaultListableObjectFactory(parent);
            child.RegisterObjectDefinition("testParent", new RootObjectDefinition(typeof(NestedTestObject), null));

            var names = child.GetObjectDefinitionNames(true);

            Assert.That(names, Has.Count.EqualTo(2), "GetObjectDefinitionNames(true) should return object definition names from this factory and parents");
        }

        [Test]
        public void GetObjectDefinitionNamesByTypeExcludingParent()
        {
            DefaultListableObjectFactory parent = new DefaultListableObjectFactory();
            parent.RegisterObjectDefinition("testChild", new RootObjectDefinition(typeof(TestObject), null));
            DefaultListableObjectFactory child = new DefaultListableObjectFactory(parent);
            child.RegisterObjectDefinition("testParent", new RootObjectDefinition(typeof(NestedTestObject), null));

            var names1 = child.GetObjectDefinitionNames(typeof(NestedTestObject));
            var names2 = child.GetObjectDefinitionNames(typeof(TestObject));

            Assert.That(names1, Has.Count.EqualTo(1), "Should return only child object definitions");
            Assert.That(names2, Has.Count.EqualTo(0), "Should not return the parent object definitions");
        }

        [Test]
        public void GetObjectDefinitionNamesByTypeIncludingParent()
        {
            DefaultListableObjectFactory parent = new DefaultListableObjectFactory();
            parent.RegisterObjectDefinition("testChild", new RootObjectDefinition(typeof(TestObject), null));
            DefaultListableObjectFactory child = new DefaultListableObjectFactory(parent);
            child.RegisterObjectDefinition("testParent", new RootObjectDefinition(typeof(NestedTestObject), null));

            var names1 = child.GetObjectDefinitionNames(typeof(NestedTestObject), true);
            var names2 = child.GetObjectDefinitionNames(typeof(TestObject), true);

            Assert.That(names1, Has.Count.EqualTo(1), "Should return child object definitions");
            Assert.That(names2, Has.Count.EqualTo(1), "Should return the parent object definitions");
        }

        [Test]
        public void GetObjectNamesForTypeFindsFactoryObjects()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            of.RegisterObjectDefinition("mod", new RootObjectDefinition(typeof(A)));

            var names = of.GetObjectNamesForType(typeof (ISerializable), false, false);
            Assert.IsNotEmpty((ICollection) names);
            Assert.AreEqual("&mod", names[0]);
        }
        
        [Test, MaxTime(1000)]
        public void TestRegistrationOfManyBeanDefinitionsIsFastEnough()
        {
            var bf = new DefaultListableObjectFactory();
            bf.RegisterObjectDefinition("b", new RootObjectDefinition(typeof(B)));
            
            for (int i = 0; i < 100_000; i++) {
                bf.RegisterObjectDefinition("a" + i, new RootObjectDefinition(typeof(A)));
            }
        }

        [Test, MaxTime(1000)]
        public void TestRegistrationOfManySingletonsIsFastEnough()
        {
            // Assume.group(TestGroup.PERFORMANCE);
            var bf = new DefaultListableObjectFactory();
            bf.RegisterObjectDefinition("b", new RootObjectDefinition(typeof(B)));
            // bf.getBean("b");

            for (int i = 0; i < 100000; i++) {
                bf.RegisterSingleton("a" + i, new A());
            }
        }

        [Test, MaxTime(3000)]
        public void TestPrototypeCreationIsFastEnough()
        {
            var lbf = new DefaultListableObjectFactory();
            var rbd = new RootObjectDefinition(typeof(TestObject))
            {
                Scope = "prototype"
            };
            lbf.RegisterObjectDefinition("test", rbd);
            //lbf.FreezeConfiguration();

            for (int i = 0; i < 100_000; i++)
            {
                lbf.GetObject("test");
            }
        }

        public class B
        {
        }
        
        public interface IParent
        {

        }

        public class Parent : IParent
        {

        }

        public interface IChild
        {

        }

        public class Child : IChild
        {
            public Child(Parent parent) { }
        }
        public class GoodDisposable : IDisposable
        {
            public static int DisposeCount = 0;

            public void Dispose()
            {
                ++DisposeCount;
            }
        }

        public class BadDisposable : IDisposable
        {
            public void Dispose()
            {
                throw new FormatException();
            }
        }

        public sealed class MySingleton
        {
            private MySingleton()
            {
            }

            private static MySingleton _instance = new MySingleton();

            public static MySingleton GetInstance()
            {
                return _instance;
            }

            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }

            private string _name;
        }

        public class NoDependencies
        {
        }

        public class ConstructorDependency
        {
            public TestObject _spouse;

            public ConstructorDependency(TestObject spouse)
            {
                this._spouse = spouse;
            }
        }

        public class UnsatisfiedConstructorDependency
        {
            public UnsatisfiedConstructorDependency(TestObject to, SideEffectObject seo)
            {
                _to = to;
                _seo = seo;
            }

            public object Seo
            {
                get { return _seo; }
            }

            public TestObject To
            {
                get { return _to; }
            }

            private object _seo;
            private TestObject _to;
        }

        private sealed class StaticInitializer
        {
            public static bool InitWasCalled = false;

            public static void Init()
            {
                InitWasCalled = true;
            }
        }
    }

    public class Foo
    {
        private IBar bar;

        public IBar Bar
        {
            get { return bar; }
            set { bar = value; }
        }
    }

    public class DerivedFoo : Foo
    {
        public new IDerivedBar Bar
        {
            get { return (IDerivedBar)base.Bar; }
            set { base.Bar = value; }
        }
    }

    public interface IBar { }

    public interface IDerivedBar : IBar { }

    public class Bar : IBar { }

    public class DerivedBar : IDerivedBar { }
}