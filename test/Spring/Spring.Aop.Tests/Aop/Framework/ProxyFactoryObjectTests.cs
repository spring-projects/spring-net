#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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
using System.IO;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Threading;

using AopAlliance.Aop;
using AopAlliance.Intercept;

using FakeItEasy;

using NUnit.Framework;
using Spring.Aop.Advice;
using Spring.Aop.Interceptor;
using Spring.Aop.Support;
using Spring.Aop.Target;
using Spring.Context;
using Spring.Core.IO;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Proxy;
using Spring.Threading;

namespace Spring.Aop.Framework
{
    /// <summary>
    /// Integration test cases for the ProxyFactoryObject, using an XML object factory.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Federico Spinazzi (.NET)</author>
    /// <author>Choy Rim (.NET)</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    [TestFixture]
    public sealed class ProxyFactoryObjectTests
    {
        private IObjectFactory factory;

        [SetUp]
        public void SetUp()
        {
            factory = new XmlObjectFactory(new ReadOnlyXmlTestResource("proxyFactoryTests.xml", GetType()));
        }

        [Test]
        public void TargetThrowsInvalidCastException()
        {
            Exception expectedException = new InvalidCastException();
            ITestObject test1 = (ITestObject)factory.GetObject("test1");
            try
            {
                test1.Exceptional(expectedException);
                Assert.Fail("Should have thrown exception raised by target");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(expectedException, ex, "exception matches");
                Assert.AreEqual(1, test1.ExceptionMethodCallCount);
            }
        }

        [Test]
        public void IsCompositionProxy()
        {
            ITestObject test1 = (ITestObject)factory.GetObject("test1");
            Assert.IsTrue(AopUtils.IsCompositionAopProxy(test1), "test1 is a composition proxy");
        }

        [Test]
        public void GetObjectTypeWithDirectTarget()
        {
            IObjectFactory bf = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("proxyFactoryTargetSourceTests.xml",
                                            GetType()));

            // We have a counting before advice here
            CountingBeforeAdvice cba = (CountingBeforeAdvice)bf.GetObject("countingBeforeAdvice");
            Assert.AreEqual(0, cba.GetCalls());

            ITestObject tb = (ITestObject)bf.GetObject("directTarget");
            Assert.IsTrue(tb.Name.Equals("Adam"));
            Assert.AreEqual(1, cba.GetCalls());

            ProxyFactoryObject pfb = (ProxyFactoryObject)bf.GetObject("&directTarget");
            Assert.IsTrue(typeof(ITestObject).IsAssignableFrom(pfb.ObjectType), "Has correct object type");
        }

        [Test]
        public void GetObjectTypeWithTargetViaTargetSource()
        {
            IObjectFactory bf = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("proxyFactoryTargetSourceTests.xml",
                                            GetType()));
            ITestObject tb = (ITestObject)bf.GetObject("viaTargetSource");
            Assert.IsTrue(tb.Name.Equals("Adam"));
            ProxyFactoryObject pfb = (ProxyFactoryObject)bf.GetObject("&viaTargetSource");
            Assert.IsTrue(typeof(ITestObject).IsAssignableFrom(pfb.ObjectType), "Has correct object type");
        }

        [Test]
        public void GetObjectTypeWithNoTargetOrTargetSource()
        {
            IObjectFactory bf =
				new XmlObjectFactory(new ReadOnlyXmlTestResource("proxyFactoryTargetSourceTests.xml", GetType()));
            bf.GetObject("noTarget");
            IFactoryObject pfb = (ProxyFactoryObject)bf.GetObject("&noTarget");
            Assert.IsTrue(typeof(ITestObject).IsAssignableFrom(pfb.ObjectType), "Has correct object type");
        }

        /// <summary>
        /// The instances are equal, but do not have object identity.
        /// Interceptors and interfaces and the target are the same.
        /// </summary>
        [Test]
        public void SingletonInstancesAreEqual()
        {
            ITestObject test1 = (ITestObject)factory.GetObject("test1");
            ITestObject test1_1 = (ITestObject)factory.GetObject("test1");
            Assert.AreEqual(test1, test1_1, "Singleton instances ==");
            test1.Age = 25;
            Assert.AreEqual(test1.Age, test1_1.Age);
            test1.Age = 250;
            Assert.AreEqual(test1.Age, test1_1.Age);
            IAdvised pc1 = (IAdvised)test1;
            IAdvised pc2 = (IAdvised)test1_1;
            Assert.AreEqual(pc1.Advisors, pc2.Advisors);
            int oldLength = pc1.Advisors.Count;
            NopInterceptor di = new NopInterceptor();
            pc1.AddAdvice(1, di);
            Assert.AreEqual(pc1.Advisors, pc2.Advisors);
            Assert.AreEqual(oldLength + 1, pc2.Advisors.Count, "Now have one more advisor");
            Assert.AreEqual(di.Count, 0);
            test1.Age = (5);
            Assert.AreEqual(test1_1.Age, test1.Age);
            Assert.AreEqual(3, di.Count);
        }

        [Test]
        public void PrototypeInstancesAreNotEqual()
        {
            ITestObject test2 = (ITestObject)factory.GetObject("prototype");
            ITestObject test2_1 = (ITestObject)factory.GetObject("prototype");
            Assert.IsTrue(test2 != test2_1, "Prototype instances !=");
            Assert.IsTrue(TestObject.Equals(test2, test2_1), "Prototype instances equal");
        }

        [Test]
        public void PrototypeInstancesAreIndependent()
        {
            IObjectFactory objectFactory = new XmlObjectFactory(new ReadOnlyXmlTestResource("prototypeTests.xml", GetType()));
            // Initial count value set in object factory XML
            int INITIAL_COUNT = 10;


            // Check it works without AOP
            ISideEffectObject raw = (ISideEffectObject)objectFactory.GetObject("prototypeTarget");
            Assert.AreEqual(INITIAL_COUNT, raw.Count);
            raw.doWork();
            Assert.AreEqual(INITIAL_COUNT + 1, raw.Count);
            raw = (ISideEffectObject)objectFactory.GetObject("prototypeTarget");
            Assert.AreEqual(INITIAL_COUNT, raw.Count);

            // Now try with advised instances
            ISideEffectObject prototype2FirstInstance = (ISideEffectObject)objectFactory.GetObject("prototype");
            Assert.AreEqual(INITIAL_COUNT, prototype2FirstInstance.Count);
            prototype2FirstInstance.doWork();
            Assert.AreEqual(INITIAL_COUNT + 1, prototype2FirstInstance.Count);

            ISideEffectObject prototype2SecondInstance = (ISideEffectObject)objectFactory.GetObject("prototype");
            Assert.IsFalse(prototype2FirstInstance == prototype2SecondInstance, "Prototypes are not ==");
            Assert.AreEqual(INITIAL_COUNT, prototype2SecondInstance.Count);
            Assert.AreEqual(INITIAL_COUNT + 1, prototype2FirstInstance.Count);


        }


        /// <summary> Test invoker is automatically added to manipulate target</summary>
        [Test]
        public void AutoInvoker()
        {
            String name = "Hieronymous";
            TestObject target = (TestObject)factory.GetObject("test");
            target.Name = name;
            ITestObject autoInvoker = (ITestObject)factory.GetObject("autoInvoker");
            Assert.IsTrue(autoInvoker.Name.Equals(name));
        }

        [Test]
        public void CanGetFactoryReferenceAndManipulate()
        {
            ITestObject to = (ITestObject)factory.GetObject("test1");
            // no exception
            string dummy = to.Name;

            IAdvised config = (IAdvised)to;
            Assert.AreEqual(1, config.Advisors.Count, "Object should have only one advisors");

            Exception ex = new NotSupportedException("Invoke");
            // Add evil interceptor to head of list
            config.AddAdvice(0, new EvilMethodInterceptor(ex));
            Assert.AreEqual(2, config.Advisors.Count, "The advisor count is wrong after adding an advisor programmatically.");

            try
            {
                // evil interceptor should throw exception
                dummy = to.Name;
                Assert.Fail("Evil interceptor added programmatically should fail all method calls, but it didn't");
            }
            catch (Exception thrown)
            {
                Assert.AreEqual(thrown, ex, "The thrown exception is not the one we were looking for.");
            }
        }

        /// <summary>
        /// Must see effect immediately on behaviour.
        /// TODO (EE): Note that we can't add or remove interfaces without reconfiguring the singleton.
        /// </summary>
        [Test, Ignore("change according to ProxyFactoryBeanTests.canAddAndRemoveAdvicesOnSingleton")]
        public void CanAddAndRemoveIntroductionsOnSingleton()
        {
            try
            {
                ITimeStamped ts = (ITimeStamped)factory.GetObject("test1");
                Assert.Fail("Shouldn't implement ITimeStamped before manipulation");
            }
            catch (InvalidCastException)
            {
            }

            ProxyFactoryObject config = (ProxyFactoryObject)factory.GetObject("&test1");
            long time = 666L;
            TimestampIntroductionInterceptor ti = new TimestampIntroductionInterceptor();
            ti.TimeStamp = new DateTime(time);
            IIntroductionAdvisor advisor = new DefaultIntroductionAdvisor(ti, typeof(ITimeStamped));

            // add to front of introduction chain
            int oldCount = config.Introductions.Count;
            config.AddIntroduction(0, advisor);
            Assert.IsTrue(config.Introductions.Count == oldCount + 1);

            ITimeStamped ts2 = (ITimeStamped)factory.GetObject("test1");
            Assert.IsTrue(ts2.TimeStamp == new DateTime(time));

            // Can remove
            config.RemoveIntroduction(advisor);
            Assert.IsTrue(config.Introductions.Count == oldCount);

            // Existing reference will still work
            object o = ts2.TimeStamp;

            // But new proxies should not implement ITimeStamped
            try
            {
                ts2 = (ITimeStamped)factory.GetObject("test1");
                Assert.Fail("Should no longer implement ITimeStamped");
            }
            catch (InvalidCastException)
            {
                // expected...
            }

            // Now check non-effect of removing interceptor that isn't there
            oldCount = config.Advisors.Count;
            config.RemoveAdvice(new DebugAdvice());
            Assert.IsTrue(config.Advisors.Count == oldCount);

            ITestObject it = (ITestObject)ts2;
            DebugAdvice debugInterceptor = new DebugAdvice();
            config.AddAdvice(0, debugInterceptor);
            object foo = it.Spouse;
            Assert.AreEqual(1, debugInterceptor.Count);
            config.RemoveAdvice(debugInterceptor);
            foo = it.Spouse;
            // not invoked again
            Assert.IsTrue(debugInterceptor.Count == 1);
        }

        /// <summary> Try adding and removing interfaces and interceptors on prototype.
        /// Changes will only affect future references obtained from the factory.
        /// Each instance will be independent.
        /// </summary>
        [Test]
        public void CanAddAndRemoveAspectInterfacesOnPrototype()
        {
            try
            {
                ITimeStamped ts = (ITimeStamped)factory.GetObject("test2");
                Assert.Fail("Shouldn't implement ITimeStamped before manipulation");
            }
            catch (InvalidCastException)
            {
            }

            IAdvised config = (IAdvised)factory.GetObject("&test2");
            long time = 666L;
            TimestampIntroductionInterceptor ti = new TimestampIntroductionInterceptor();
            ti.TimeStamp = new DateTime(time);
            IIntroductionAdvisor advisor = new DefaultIntroductionAdvisor(ti, typeof(ITimeStamped));

            // add to front of introduction chain
            int oldCount = config.Introductions.Count;
            config.AddIntroduction(0, advisor);
            Assert.IsTrue(config.Introductions.Count == oldCount + 1);

            ITimeStamped ts2 = (ITimeStamped)factory.GetObject("test2");
            Assert.IsTrue(ts2.TimeStamp == new DateTime(time));

            // Can remove
            config.RemoveIntroduction(advisor);
            Assert.IsTrue(config.Introductions.Count == oldCount);

            // Existing reference will still work
            object o = ts2.TimeStamp;

            // But new proxies should not implement ITimeStamped
            try
            {
                ts2 = (ITimeStamped)factory.GetObject("test2");
                Assert.Fail("Should no longer implement ITimeStamped");
            }
            catch (InvalidCastException)
            {
            }

            // Now check non-effect of removing interceptor that isn't there
            ITestObject it = (ITestObject)factory.GetObject("test2");
            config = (IAdvised)it;

            oldCount = config.Advisors.Count;
            config.RemoveAdvice(new DebugAdvice());
            Assert.IsTrue(config.Advisors.Count == oldCount);

            DebugAdvice debugInterceptor = new DebugAdvice();
            config.AddAdvice(0, debugInterceptor);
            object foo = it.Spouse;
            Assert.AreEqual(1, debugInterceptor.Count);
            config.RemoveAdvice(debugInterceptor);
            foo = it.Spouse;
            // not invoked again
            Assert.IsTrue(debugInterceptor.Count == 1);
        }

        /// <summary>
        /// Note that we can't add or remove interfaces without reconfiguring the
        /// singleton.
        /// </summary>
        [Test]
        public void CanAddAndRemoveAspectInterfacesOnSingletonByCasting()
        {
            ITestObject it = (ITestObject)factory.GetObject("test1");
            IAdvised pc = (IAdvised)it;
            object name = it.Age;
            NopInterceptor di = new NopInterceptor();
            pc.AddAdvice(0, di);
            Assert.AreEqual(0, di.Count);
            it.Age = 25;
            Assert.AreEqual(25, it.Age);
            Assert.AreEqual(2, di.Count);
        }

        [Test]
        public void MethodPointcuts()
        {
            ITestObject tb = (ITestObject)factory.GetObject("pointcuts");
            PointcutForVoid.Reset();
            Assert.IsTrue((PointcutForVoid.methodNames.Count == 0), "No methods intercepted");
            object o = tb.Age;
            Assert.IsTrue((PointcutForVoid.methodNames.Count == 0), "Not void: shouldn't have intercepted");
            tb.Age = 1;
            o = tb.Age;
            tb.Name = "Tristan";
            tb.ToString();
            Assert.AreEqual(2, PointcutForVoid.methodNames.Count, "Recorded wrong number of invocations");
            Assert.AreEqual("set_Age", PointcutForVoid.methodNames[0]);
            Assert.AreEqual("set_Name", PointcutForVoid.methodNames[1]);
        }

#if !NETCOREAPP
        [Test]
        public void CanAddThrowsAdviceWithoutAdvisor()
        {
            IObjectFactory f = new XmlObjectFactory(new ReadOnlyXmlTestResource("throwsAdvice.xml", GetType()));
            var th = (Spring.Aop.Framework.Adapter.ThrowsAdviceInterceptorTests.MyThrowsHandler) f.GetObject("throwsAdvice");
            CountingBeforeAdvice cba = (CountingBeforeAdvice)f.GetObject("countingBeforeAdvice");
            Assert.AreEqual(0, cba.GetCalls());
            Assert.AreEqual(0, th.GetCalls());
            var echo = (Spring.Aop.Framework.Adapter.ThrowsAdviceInterceptorTests.IEcho) f.GetObject("throwsAdvised");
            echo.A = 12;
            Assert.AreEqual(1, cba.GetCalls());
            Assert.AreEqual(0, th.GetCalls());
            Exception expected = new Exception();
            try
            {
                echo.EchoException(1, expected);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.AreEqual(expected, ex);
            }
            // No throws handler method: count should still be 0
            Assert.AreEqual(0, th.GetCalls());

            // Handler knows how to handle this exception
            expected = new System.Web.HttpException();
            try
            {
                echo.EchoException(1, expected);
                Assert.Fail();
            }
            catch (System.Web.HttpException ex)
            {
                Assert.AreEqual(expected, ex);
            }
            // One match
            Assert.AreEqual(1, th.GetCalls("HttpException"));
        }
#endif

        /// <summary> Checks that globals get invoked,
        /// and that they can add aspect interfaces unavailable
        /// to other objects. These interfaces don't need
        /// to be included in proxiedInterface [].
        /// </summary>
        [Test]
        public void GlobalsCanAddAspectInterfaces()
        {
            IAddedGlobalInterface agi = (IAddedGlobalInterface) factory.GetObject("autoInvoker");
            Assert.IsTrue(agi.GlobalsAdded == -1);

            ProxyFactoryObject pfb = (ProxyFactoryObject) factory.GetObject("&validGlobals");
            pfb.GetObject(); // for creation
            Assert.AreEqual(2, pfb.Advisors.Count, "Proxy should have 1 global and 1 explicit advisor");
            Assert.AreEqual(1, pfb.Introductions.Count, "Proxy should have 1 global introduction");

            agi.GlobalsAdded = ((IAdvised) agi).Introductions.Count;
            Assert.IsTrue(agi.GlobalsAdded == 1);

            IApplicationEventListener l = (IApplicationEventListener) factory.GetObject("validGlobals");
            agi = (IAddedGlobalInterface) l;
            Assert.IsTrue(agi.GlobalsAdded == -1);
            Assert.Throws<InvalidCastException>(() => factory.GetObject<IAddedGlobalInterface>("test1"));
        }

        [Test]
        public void IsSingletonFalseReturnsNew_ProxyInstance_NotNewProxyTargetSource()
        {
            GoodCommand target = new GoodCommand();
            IObjectFactory mock = A.Fake<IObjectFactory>();
            A.CallTo(() => mock.GetObject("singleton")).Returns(target).Twice();

            ProxyFactoryObject fac = new ProxyFactoryObject();
            fac.ProxyInterfaces = new string[] { typeof(ICommand).FullName };
            fac.IsSingleton = false;
            fac.TargetName = "singleton";
            fac.ObjectFactory = mock;
            fac.AddAdvice(new NopInterceptor());

            ICommand one = (ICommand)fac.GetObject();
            ICommand two = (ICommand)fac.GetObject();
            Assert.IsFalse(ReferenceEquals(one, two));
        }

        [Test]
        public void IsSingletonTrueReturnsNew_ProxyInstance_NotNewProxyTargetSource()
        {
            GoodCommand target = new GoodCommand();
            IObjectFactory mock = A.Fake<IObjectFactory>();
            A.CallTo(() => mock.GetObject("singleton")).Returns(target);

            ProxyFactoryObject fac = new ProxyFactoryObject();
            fac.ProxyInterfaces = new string[] { typeof(ICommand).FullName };
            fac.IsSingleton = true; // default, just being explicit...
            fac.TargetName = "singleton";
            fac.ObjectFactory = mock;
            fac.AddAdvice(new NopInterceptor());

            ICommand one = (ICommand)fac.GetObject();
            ICommand two = (ICommand)fac.GetObject();
            Assert.IsTrue(ReferenceEquals(one, two));
        }

        private ProxyFactoryObject CreateFrozenProxyFactory()
        {
            ProxyFactoryObject fac = new ProxyFactoryObject();
            fac.AddInterface(typeof(ITestObject));
            fac.IsFrozen = true;
            fac.AddAdvisor(new PointcutForVoid()); // this is ok, no proxy created yet
            fac.GetObject();
            return fac;
        }

        [Test]
        public void AddAdvisorWhenConfigIsFrozen()
        {
            ProxyFactoryObject fac = CreateFrozenProxyFactory();
            try
            {
                fac.AddAdvisor(new PointcutForVoid()); // not ok
                Assert.Fail("changing a frozen config must throw AopConfigException");
            }
            catch (AopConfigException)
            {}
        }

        [Test]
        public void RemoveAdvisorWhenConfigIsFrozen()
        {
            ProxyFactoryObject fac = CreateFrozenProxyFactory();
            fac.IsFrozen = true;
            Assert.Throws<AopConfigException>(() => fac.RemoveAdvisor(new PointcutForVoid()));
        }

        [Test]
        public void ReplaceAdvisorWhenConfigIsFrozen()
        {
            ProxyFactoryObject fac = CreateFrozenProxyFactory();
            fac.IsFrozen = true;
            Assert.Throws<AopConfigException>(() => fac.ReplaceAdvisor(new PointcutForVoid(), new PointcutForVoid()));
        }

        [Test]
        public void TargetAtEndOfInterceptorList()
        {
            GoodCommand target = new GoodCommand();
            NopInterceptor advice = new NopInterceptor();

            IObjectFactory mock = A.Fake<IObjectFactory>();
            A.CallTo(() => mock.GetObject("advice")).Returns(advice);
            A.CallTo(() => mock.GetObject("singleton")).Returns(target);
            A.CallTo(() => mock.GetType("singleton")).Returns(typeof(GoodCommand));

            ProxyFactoryObject fac = new ProxyFactoryObject();
            fac.ProxyInterfaces = new string[] { typeof(ICommand).FullName };
            fac.IsSingleton = true; // default, just being explicit...
            fac.InterceptorNames = new string[] { "advice", "singleton" };
            fac.ObjectFactory = mock;

            ICommand one = (ICommand)fac.GetObject();
            ICommand two = (ICommand)fac.GetObject();
            Assert.IsTrue(ReferenceEquals(one, two));
            one.Execute();
            Assert.AreEqual(1, advice.Count);
            two.Execute();
            Assert.AreEqual(2, advice.Count);
        }

        [Test]
        public void MakeSurePrototypeTargetIsNotNeedlesslyCreatedDuringInitialization_Unit()
        {
            GoodCommand target = new GoodCommand();
            NopInterceptor advice = new NopInterceptor();

            IObjectFactory factory = A.Fake<IObjectFactory>();

            ProxyFactoryObject fac = new ProxyFactoryObject();
            fac.ProxyInterfaces = new[] {typeof(ICommand).FullName};
            fac.IsSingleton = false;
            fac.InterceptorNames = new[] {"advice", "prototype"};
            fac.ObjectFactory = factory;

            A.CallTo(() => factory.IsSingleton("advice")).Returns(true);
            A.CallTo(() => factory.GetObject("advice")).Returns(advice);
            A.CallTo(() => factory.GetType("prototype")).Returns(target.GetType());
            A.CallTo(() => factory.GetObject("prototype")).Returns(target);

            fac.GetObject();
        }

        [Test]
        public void MakeSurePrototypeTargetIsNotNeedlesslyCreatedDuringInitialization_Integration()
        {
            try
            {
                RootObjectDefinition advice = new RootObjectDefinition(typeof(NopInterceptor));
                // prototype target...
                RootObjectDefinition target = new RootObjectDefinition(typeof(InstantiationCountingCommand), false);

                DefaultListableObjectFactory ctx = new DefaultListableObjectFactory();
                ctx.RegisterObjectDefinition("advice", advice);
                ctx.RegisterObjectDefinition("prototype", target);

                ProxyFactoryObject fac = new ProxyFactoryObject();
                fac.ProxyInterfaces = new string[] { typeof(ICommand).FullName };
                fac.IsSingleton = false;
                fac.InterceptorNames = new string[] { "advice", "prototype" };
                fac.ObjectFactory = ctx;

                Assert.AreEqual(0, InstantiationCountingCommand.NumberOfInstantiations,
                                "Prototype target instance is being (needlessly) created during PFO initialization.");
                fac.GetObject();
                Assert.AreEqual(1, InstantiationCountingCommand.NumberOfInstantiations, "Expected 1 inst");
                fac.GetObject();
                Assert.AreEqual(2, InstantiationCountingCommand.NumberOfInstantiations);
            }
            finally
            {
                InstantiationCountingCommand.NumberOfInstantiations = 0;
            }
        }

        [Test]
        public void SingletonProxyWithPrototypeTargetCreatesTargetOnlyOnce()
        {
            try
            {
                RootObjectDefinition advice = new RootObjectDefinition(typeof(NopInterceptor));
                // prototype target...
                RootObjectDefinition target = new RootObjectDefinition(typeof(InstantiationCountingCommand), false);

                DefaultListableObjectFactory ctx = new DefaultListableObjectFactory();
                ctx.RegisterObjectDefinition("advice", advice);
                ctx.RegisterObjectDefinition("prototype", target);

                ProxyFactoryObject fac = new ProxyFactoryObject();
                fac.ProxyInterfaces = new string[] { typeof(ICommand).FullName };
                fac.IsSingleton = true;
                fac.InterceptorNames = new string[] { "advice", "prototype" };
                fac.ObjectFactory = ctx;

                Assert.AreEqual(0, InstantiationCountingCommand.NumberOfInstantiations, "First Call");
                fac.GetObject();
                Assert.AreEqual(1, InstantiationCountingCommand.NumberOfInstantiations, "Second Call");
                fac.GetObject();
                Assert.AreEqual(1, InstantiationCountingCommand.NumberOfInstantiations, "Third Call");
            }

            finally
            {
                InstantiationCountingCommand.NumberOfInstantiations = 0;
            }
        }

        public class InstantiationCountingCommand : ICommand
        {
            private static int numberOfInstantiations = 0;

            public InstantiationCountingCommand()
            {
                ++numberOfInstantiations;
            }

            public static int NumberOfInstantiations
            {
                get { return numberOfInstantiations; }
                set { numberOfInstantiations = value; }
            }

            public void Execute()
            {
            }
        }

        [Test]
        public void NullNameInInterceptorNamesArrayThrowAopConfigException()
        {
            IObjectFactory factory = A.Fake<IObjectFactory>();

            ProxyFactoryObject fac = new ProxyFactoryObject();
            fac.ProxyInterfaces = new string[] { typeof(ICommand).FullName };
            fac.IsSingleton = false;
            fac.InterceptorNames = new string[] { null, null };
            fac.ObjectFactory = factory;
            Assert.Throws<AopConfigException>(() => fac.GetObject());
        }

        [Test]
        public void PassEmptyInterceptorNamesArray_WithTargetThatImplementsAnInterfaceCanBeCastToSaidInterface()
        {
            IObjectFactory factory = A.Fake<IObjectFactory>();

            ProxyFactoryObject fac = new ProxyFactoryObject();
            fac.ProxyInterfaces = new string[] { };
            fac.Target = new GoodCommand();
            fac.ObjectFactory = factory;

            IAdvised advised = fac.GetObject() as IAdvised;
            Assert.IsNotNull(advised);

            ICommand cmd = fac.GetObject() as ICommand;
            Assert.IsNotNull(cmd);

            DoesntImplementAnyInterfaces obj = fac.GetObject() as DoesntImplementAnyInterfaces;
            Assert.IsNull(obj);
        }

        [Test]
        public void PassNullToTheProxyInterfacesProperty()
        {
            ProxyFactoryObject fac = new ProxyFactoryObject();
            Assert.Throws<AopConfigException>(() => fac.ProxyInterfaces = null);
        }

        [Test]
        public void PassClassNotInterfaceNameToTheProxyInterfacesProperty()
        {
            ProxyFactoryObject fac = new ProxyFactoryObject();
            Assert.Throws<AopConfigException>(() => fac.ProxyInterfaces = new string[] { typeof(GoodCommand).FullName });
        }

        [Test]
        public void PassRubbishNameToTheProxyInterfacesProperty()
        {
            ProxyFactoryObject fac = new ProxyFactoryObject();
            Assert.Throws<AopConfigException>(() => fac.ProxyInterfaces = new string[] { "Hey" });
        }

        [Test]
        public void PassNullElementListToTheProxyInterfacesProperty()
        {
            ProxyFactoryObject fac = new ProxyFactoryObject();
            Assert.Throws<AopConfigException>(() => fac.ProxyInterfaces = new string[] { null });
        }

        [Test]
        public void ProxiedObjectUnwrapsTargetInvocationException()
        {
            ProxyFactoryObject fac = new ProxyFactoryObject();
            fac.AddInterface(typeof(ICommand));
            fac.AddAdvice(new NopInterceptor());
            fac.Target = new BadCommand();

            ICommand cmd = (ICommand)fac.GetObject();
            try
            {
                cmd.Execute();
            }
            catch (NotImplementedException)
            {
                // this is good, we want this exception to bubble up...
            }
            catch (TargetInvocationException)
            {
                Assert.Fail("Must have unwrapped this.");
            }
        }

        [Test]
        public void FactoryWrapsObjectInSingletonTargetSource()
        {
            IObjectFactory bf = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("proxyFactoryTargetSourceTests.xml",
                                            GetType()));
            ITestObject tb = (ITestObject)bf.GetObject("viaTargetSource");
            Assert.IsTrue(tb.Name.Equals("Adam"));
            ProxyFactoryObject pfb = (ProxyFactoryObject)bf.GetObject("&viaTargetSource");
            Assert.IsTrue(typeof(ITestObject).IsAssignableFrom(pfb.ObjectType), "Has correct object type");
            Assert.AreEqual(typeof(SingletonTargetSource), pfb.TargetSource.GetType(), "Incorrect target source, expected singleton");
        }

#if !NETCOREAPP
        [Test(Description = "http://opensource.atlassian.com/projects/spring/browse/SPRNET-293")]
        public void SupportsTransparentProxyAsTarget()
        {
            AppDomain domain = null;
            try
            {
                AppDomainSetup setup = new AppDomainSetup();
                setup.ApplicationBase = Environment.CurrentDirectory;
                domain = AppDomain.CreateDomain("Spring", new System.Security.Policy.Evidence(AppDomain.CurrentDomain.Evidence), setup);
                object command = domain.CreateInstanceAndUnwrap(GetType().Assembly.FullName, typeof(RemotableCommand).FullName);

                ProxyFactoryObject fac = new ProxyFactoryObject();
                fac.AddInterface(typeof(ICommand));
                fac.AddAdvice(new NopInterceptor());
                fac.Target = command;

                IAdvised advised = fac.GetObject() as IAdvised;
                Assert.IsNotNull(advised);

                ICommand cmd = fac.GetObject() as ICommand;
                Assert.IsNotNull(cmd);

                cmd.Execute();
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }
#endif

        [Test(Description = "http://opensource.atlassian.com/projects/spring/browse/SPRNET-500")]
        public void NotAccessibleInterfaceProxying()
        {
                const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
                <objects xmlns='http://www.springframework.net'>
                <object id='MyProxy' type='Spring.Aop.Framework.ProxyFactoryObject, Spring.Aop'>
                    <property name='ProxyInterfaces'>
                        <list>
                            <value>Spring.Aop.Framework.HelperInterface2, Spring.Aop.Tests</value>
                        </list>
                    </property>
                    <property name='TargetName' value='MyTarget' />
                    <property name='InterceptorNames'>
                        <list>
                            <value>MyInterceptor</value>
                        </list>
                    </property>
                </object>
                <object id='MyTarget' type='Spring.Aop.Framework.HelperClassForNotAccessibleInterfaceProxyingTest, Spring.Aop.Tests'/>
	            <object id='MyInterceptor' type='Spring.Aop.Interceptor.NopInterceptor'/>
               </objects>";


                MemoryStream stream = new MemoryStream(new UTF8Encoding().GetBytes(xml));
                IResource resource = new InputStreamResource(stream, "Test ProxyFactoryObject");
                XmlObjectFactory objectFactory = new XmlObjectFactory(resource, null);

                HelperInterface2 hc = (HelperInterface2)objectFactory.GetObject("MyProxy");
                Console.WriteLine(hc.SecondDoSomething());
        }

        [Test]
        public void ProxyFactoryObjectIsThreadSafe()
        {
            const int WORKERS = 10;

            AsyncTestMethod[] workers = new AsyncTestMethod[WORKERS];
            for(int i=0;i<WORKERS;i++)
            {
                workers[i] = new AsyncTestMethod( 1, new ThreadStart( CallGetObject ) );
            }

            for(int i=0;i<WORKERS;i++)
            {
                workers[i].Start();
            }

            for(int i=0;i<WORKERS;i++)
            {
                workers[i].AssertNoException();
            }
        }

        private void CallGetObject()
        {
            ProxyFactoryObject proxyFactory = (ProxyFactoryObject) this.factory.GetObject("&concurrentPrototype");

            ITestObject prevTestObject = null;

            for(int i=0;i<20;i++)
            {
                ITestObject o = proxyFactory.GetObject() as ITestObject;
                Assert.IsNotNull(o);
                Assert.AreNotSame( prevTestObject, o );
                prevTestObject = o;
                Thread.Sleep( 0 );
            }
        }

        [Test]
        public void ProxyTypeDoesntChangeIfSameConfig()
        {
            ProxyFactoryObject factoryObject = (ProxyFactoryObject) this.factory.GetObject( "&concurrentPrototype" );
            Type testObjectType1 = factoryObject.GetObject().GetType();
            Type testObjectType2 = factoryObject.GetObject().GetType();

            Assert.AreSame(testObjectType1, testObjectType2);
        }

        [Test]
        public void ProxyTypeChangesIfConfigChanges()
        {
            ProxyFactoryObject factoryObject = (ProxyFactoryObject) this.factory.GetObject( "&concurrentPrototype" );
            Type testObjectType1 = factoryObject.GetObject().GetType();

            factoryObject.Interfaces = new Type[] {};
            Type testObjectType2 = factoryObject.GetObject().GetType();

            Assert.AreNotSame( testObjectType1,testObjectType2 );
        }

        #region WorkerThread Class

        public class WorkerThread
        {
            private Exception _exception;
            private Thread _thread;
            private WaitCallback _callback;
            private object _arg;

            public WorkerThread(string name, WaitCallback callback,object arg)
            {
                this._arg = arg;
                this._callback = callback;
                _thread = new Thread( new ThreadStart( Run ) );
                _thread.IsBackground = true;
                _thread.Name = name;
            }

            public void Start()
            {
                _thread.Start();
            }

            public void Join()
            {
                if (_thread.IsAlive)
                {
                    _thread.Join();
                }
            }

            public Exception Exception
            {
                get { return this._exception; }
            }

            private void Run()
            {
                try
                {
                    _callback( _arg );
                }
                catch(Exception ex)
                {
                    _exception = ex;
                }
            }
        }

        #endregion WorkerThread Class

        #region Helper Classes

        public interface ICommand
        {
            void Execute();
        }

        public sealed class BadCommand : ICommand
        {
            public void Execute()
            {
                throw new NotImplementedException();
            }
        }

        public sealed class GoodCommand : ICommand
        {
            public void Execute()
            {
            }
        }

        public sealed class RemotableCommand : MarshalByRefObject, ICommand
        {
            public void Execute()
            {
            }
        }

        /// <summary>
        /// Fires only on void methods. Saves list of methods intercepted.
        /// </summary>
        public class PointcutForVoid : DynamicMethodMatcherPointcutAdvisor
        {
            private class AnonymousClassMethodInterceptor1 : IMethodInterceptor
            {
                public Object Invoke(IMethodInvocation invocation)
                {
                    methodNames.Add(invocation.Method.Name);
                    return invocation.Proceed();
                }
            }

            public static IList methodNames;

            public static void Reset()
            {
                methodNames.Clear();
            }

            public PointcutForVoid()
                : base(new AnonymousClassMethodInterceptor1())
            {
            }

            /// <summary>
            /// Must fire only if it returns void.
            /// </summary>
            public override bool Matches(MethodInfo method, Type targetType, object[] args)
            {
                return method.ReturnType == typeof(void);
            }

            static PointcutForVoid()
            {
                methodNames = new ArrayList();
            }
        }

        /// <summary> Aspect interface</summary>
        public interface IAddedGlobalInterface
        {
            int GlobalsAdded { get; set; }
        }

        /// <summary> Use as a global interceptor. Checks that
        /// global interceptors can add aspect interfaces.
        /// NB: Add only via global interceptors in XML file.
        /// </summary>
        public class GlobalIntroduction : IAdvice, IAddedGlobalInterface
        {
            private int globalsAdded = -1;

            public int GlobalsAdded
            {
                get { return globalsAdded; }
                set { globalsAdded = value; }
            }
        }

        private class EvilMethodInterceptor : IMethodInterceptor
        {
            private Exception ex;

            public EvilMethodInterceptor(Exception ex)
            {
                this.ex = ex;
            }

            public object Invoke(IMethodInvocation invocation)
            {
                throw ex;
            }
        }

        #endregion
    }

    #region HelpersForNotAccessibleInterfaceProxyingTest

    public class HelperClassForNotAccessibleInterfaceProxyingTest : HelperInterface1, HelperInterface2
    {
        public string FirstDoSomething()
        {
            return "FirstDoSomething (internal interface implementation)";
        }

        public string SecondDoSomething()
        {
            return "SecondDoSomething (public interface implementation)";
        }
    }

    internal interface HelperInterface1
    {
        string FirstDoSomething();
    }

    public interface HelperInterface2
    {
        string SecondDoSomething();
    }

    #endregion


}
