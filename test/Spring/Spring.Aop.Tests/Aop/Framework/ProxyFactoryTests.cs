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
using System.Runtime.Serialization;
using AopAlliance.Aop;
using AopAlliance.Intercept;

using FakeItEasy;

using NUnit.Framework;
using Spring.Aop.Interceptor;
using Spring.Aop.Support;
using Spring.Objects;
using Spring.Util;

namespace Spring.Aop.Framework
{
    /// <summary>
    /// Unit tests for the ProxyFactory class.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Choy Rim (.NET)</author>
    /// <author>Rick Evans (.NET)</author>
    [TestFixture]
    public sealed class ProxyFactoryTests
    {
        public interface IDoubleClickable
        {
            event EventHandler DoubleClick;

            void FireDoubleClickEvent();
        }

        public interface IDoubleClickableIntroduction :
            IDoubleClickable, IAdvice
        {
        }

        private class DoubleClickableIntroduction :
            IDoubleClickableIntroduction
        {
            public event EventHandler DoubleClick;

            public void FireDoubleClickEvent()
            {
                if (DoubleClick != null)
                {
                    DoubleClick(this, EventArgs.Empty);
                }
            }
        }

        [Test]
        public void AddAndRemoveEventHandlerThroughIntroduction()
        {
            TestObject target = new TestObject();
            DoubleClickableIntroduction dci = new DoubleClickableIntroduction();
            DefaultIntroductionAdvisor advisor = new DefaultIntroductionAdvisor(dci);
            CountingBeforeAdvice countingBeforeAdvice = new CountingBeforeAdvice();
            target.Name = "SOME-NAME";
            ProxyFactory pf = new ProxyFactory(target);
            pf.AddIntroduction(advisor);
            pf.AddAdvisor(new DefaultPointcutAdvisor(countingBeforeAdvice));
            object proxy = pf.GetProxy();
            ITestObject to = proxy as ITestObject;
            Assert.IsNotNull(to);
            Assert.AreEqual("SOME-NAME", to.Name);
            IDoubleClickable doubleClickable = proxy as IDoubleClickable;
            // add event handler through introduction
            doubleClickable.DoubleClick += new EventHandler(OnClick);
            OnClickWasCalled = false;
            doubleClickable.FireDoubleClickEvent();
            Assert.IsTrue(OnClickWasCalled);
            Assert.AreEqual(3, countingBeforeAdvice.GetCalls());
            // remove event handler through introduction
            doubleClickable.DoubleClick -= new EventHandler(OnClick);
            OnClickWasCalled = false;
            doubleClickable.FireDoubleClickEvent();
            Assert.IsFalse(OnClickWasCalled);
            Assert.AreEqual(5, countingBeforeAdvice.GetCalls());
        }

        private bool OnClickWasCalled = false;

        private void OnClick(object sender, EventArgs e)
        {
            OnClickWasCalled = true;
        }

        [Test]
        public void CacheTest()
        {

            for (int i = 0; i < 2; i++)
            {
                TestObject target = new TestObject();
                NopInterceptor nopInterceptor = new NopInterceptor();
                CountingBeforeAdvice countingBeforeAdvice = new CountingBeforeAdvice();
                ProxyFactory pf = new ProxyFactory();
                pf.Target = target;
                pf.AddAdvice(nopInterceptor);
                pf.AddAdvisor(new DefaultPointcutAdvisor(countingBeforeAdvice));
                object proxy = pf.GetProxy();
            }

            // fails when running in resharper/testdriven.net
            // DynamicProxyManager.SaveAssembly();

        }

        [Test]
        public void AddAndRemoveEventHandlerThroughInterceptor()
        {
            TestObject target = new TestObject();
            NopInterceptor nopInterceptor = new NopInterceptor();
            CountingBeforeAdvice countingBeforeAdvice = new CountingBeforeAdvice();
            target.Name = "SOME-NAME";
            ProxyFactory pf = new ProxyFactory(target);
            pf.AddAdvice(nopInterceptor);
            pf.AddAdvisor(new DefaultPointcutAdvisor(countingBeforeAdvice));
            object proxy = pf.GetProxy();
            ITestObject to = proxy as ITestObject;
            // add event handler through proxy
            to.Click += new EventHandler(OnClick);
            OnClickWasCalled = false;
            to.OnClick();
            Assert.IsTrue(OnClickWasCalled);
            Assert.AreEqual(2, countingBeforeAdvice.GetCalls());
            // remove event handler through proxy
            to.Click -= new EventHandler(OnClick);
            OnClickWasCalled = false;
            to.OnClick();
            Assert.IsFalse(OnClickWasCalled);
            Assert.AreEqual(4, countingBeforeAdvice.GetCalls());
        }

        private class TestObject2 : TestObject
        {
            public bool EqualsOverrideWasCalled = false;

            public override bool Equals(object obj)
            {
                EqualsOverrideWasCalled = true;
                return true;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        [Test]
        public void CallsEqualsOverride()
        {
            TestObject2 target = new TestObject2();
            target.Name = "SOME-NAME";
            ProxyFactory pf = new ProxyFactory(target);
            object proxy = pf.GetProxy();
            ITestObject to = proxy as ITestObject;
            Assert.IsNotNull(to);
            Assert.AreEqual("SOME-NAME", to.Name);

            target.EqualsOverrideWasCalled = false;
            Assert.IsTrue(to.Equals(proxy));
            Assert.IsTrue(target.EqualsOverrideWasCalled);

            target.EqualsOverrideWasCalled = false;
            Assert.IsTrue(proxy.Equals(to));
            Assert.IsTrue(target.EqualsOverrideWasCalled);

            target.EqualsOverrideWasCalled = false;
            Assert.IsTrue(target.Equals(to));
            Assert.IsTrue(target.EqualsOverrideWasCalled);

            target.EqualsOverrideWasCalled = false;
            Assert.IsTrue(to.Equals(target));
            Assert.IsTrue(target.EqualsOverrideWasCalled);
        }

        [Test]
        public void CreateProxyFactoryWithoutTargetThenSetTarget()
        {
            TestObject target = new TestObject();
            target.Name = "Adam";
            NopInterceptor nopInterceptor = new NopInterceptor();
            CountingBeforeAdvice countingBeforeAdvice = new CountingBeforeAdvice();
            ProxyFactory pf = new ProxyFactory();
            pf.Target = target;
            pf.AddAdvice(nopInterceptor);
            pf.AddAdvisor(new DefaultPointcutAdvisor(countingBeforeAdvice));
            object proxy = pf.GetProxy();
            ITestObject to = (ITestObject)proxy;
            Assert.AreEqual("Adam", to.Name);
            Assert.AreEqual(1, countingBeforeAdvice.GetCalls());
        }

        [Test]
        public void InstantiateWithNullTarget()
        {
            Assert.Throws<AopConfigException>(() => new ProxyFactory((object) null));
        }

        [Test]
        public void AddNullInterface()
        {
            Assert.Throws<ArgumentNullException>(() => new ProxyFactory().AddInterface(null));
        }

        [Test]
        public void AddInterfaceWhenConfigurationIsFrozen()
        {
            ProxyFactory factory = new ProxyFactory();
            factory.IsFrozen = true;
            Assert.Throws<AopConfigException>(() => factory.AddInterface(typeof(ITestObject)));
        }

        [Test]
        public void IndexOfMethods()
        {
            TestObject target = new TestObject();
            ProxyFactory pf = new ProxyFactory(target);
            NopInterceptor nop = new NopInterceptor();
            IAdvisor advisor = new DefaultPointcutAdvisor(new CountingBeforeAdvice());
            IAdvised advised = (IAdvised)pf.GetProxy();
            // Can use advised and ProxyFactory interchangeably
            advised.AddAdvice(nop);
            pf.AddAdvisor(advisor);
            Assert.AreEqual(-1, pf.IndexOf((IInterceptor)null));
            Assert.AreEqual(-1, pf.IndexOf(new NopInterceptor()));
            Assert.AreEqual(0, pf.IndexOf(nop));
            Assert.AreEqual(-1, advised.IndexOf((IAdvisor)null));
            Assert.AreEqual(1, pf.IndexOf(advisor));
            Assert.AreEqual(-1, advised.IndexOf(new DefaultPointcutAdvisor(null)));
        }

        [Test]
        public void RemoveAdvisorByReference()
        {
            TestObject target = new TestObject();
            ProxyFactory pf = new ProxyFactory(target);
            NopInterceptor nop = new NopInterceptor();
            CountingBeforeAdvice cba = new CountingBeforeAdvice();
            IAdvisor advisor = new DefaultPointcutAdvisor(cba);
            pf.AddAdvice(nop);
            pf.AddAdvisor(advisor);
            ITestObject proxied = (ITestObject)pf.GetProxy();
            proxied.Age = 5;
            Assert.AreEqual(1, cba.GetCalls());
            Assert.AreEqual(1, nop.Count);
            Assert.IsFalse(pf.RemoveAdvisor(null));
            Assert.IsTrue(pf.RemoveAdvisor(advisor));
            Assert.AreEqual(5, proxied.Age);
            Assert.AreEqual(1, cba.GetCalls());
            Assert.AreEqual(2, nop.Count);
            Assert.IsFalse(pf.RemoveAdvisor(new DefaultPointcutAdvisor(null)));
        }

        [Test]
        public void RemoveAdvisorByIndex()
        {
            TestObject target = new TestObject();
            ProxyFactory pf = new ProxyFactory(target);
            NopInterceptor nop = new NopInterceptor();
            CountingBeforeAdvice cba = new CountingBeforeAdvice();
            IAdvisor advisor = new DefaultPointcutAdvisor(cba);
            pf.AddAdvice(nop);
            pf.AddAdvisor(advisor);
            NopInterceptor nop2 = new NopInterceptor(2); // make instance unique (see SPRNET-847)
            pf.AddAdvice(nop2);
            ITestObject proxied = (ITestObject)pf.GetProxy();
            proxied.Age = 5;
            Assert.AreEqual(1, cba.GetCalls());
            Assert.AreEqual(1, nop.Count);
            Assert.AreEqual(1, nop2.Count);
            // Removes counting before advisor
            pf.RemoveAdvisor(1);
            Assert.AreEqual(5, proxied.Age);
            Assert.AreEqual(1, cba.GetCalls());
            Assert.AreEqual(2, nop.Count);
            Assert.AreEqual(2, nop2.Count);
            // Removes Nop1
            pf.RemoveAdvisor(0);
            Assert.AreEqual(5, proxied.Age);
            Assert.AreEqual(1, cba.GetCalls());
            Assert.AreEqual(2, nop.Count);
            Assert.AreEqual(3, nop2.Count);

            // Check out of bounds
            try
            {
                pf.RemoveAdvisor(-1);
                Assert.Fail("Supposed to throw exception");
            }
            catch (AopConfigException)
            {
                // Ok
            }

            try
            {
                pf.RemoveAdvisor(2);
                Assert.Fail("Supposed to throw exception");
            }
            catch (AopConfigException)
            {
                // Ok
            }

            Assert.AreEqual(5, proxied.Age);
            Assert.AreEqual(4, nop2.Count);
        }

        [Test]
        public void TryRemoveNonProxiedInterface()
        {
            ProxyFactory factory = new ProxyFactory(new TestObject());
            Assert.IsFalse(factory.RemoveInterface(typeof(IServiceProvider)));
        }

        [Test]
        public void RemoveProxiedInterface()
        {
            ProxyFactory factory = new ProxyFactory(new TestObject());
            Assert.IsTrue(factory.RemoveInterface(typeof(ITestObject)));
        }

        [Test]
        public void ReplaceAdvisor()
        {
            TestObject target = new TestObject();
            ProxyFactory pf = new ProxyFactory(target);
            NopInterceptor nop = new NopInterceptor();
            CountingBeforeAdvice cba1 = new CountingBeforeAdvice();
            CountingBeforeAdvice cba2 = new CountingBeforeAdvice();
            IAdvisor advisor1 = new DefaultPointcutAdvisor(cba1);
            IAdvisor advisor2 = new DefaultPointcutAdvisor(cba2);
            pf.AddAdvisor(advisor1);
            pf.AddAdvice(nop);
            ITestObject proxied = (ITestObject)pf.GetProxy();
            // Use the type cast feature
            // Replace etc methods on advised should be same as on ProxyFactory
            IAdvised advised = (IAdvised)proxied;
            proxied.Age = 5;
            Assert.AreEqual(1, cba1.GetCalls());
            Assert.AreEqual(0, cba2.GetCalls());
            Assert.AreEqual(1, nop.Count);
            Assert.IsFalse(advised.ReplaceAdvisor(null, null));
            Assert.IsFalse(advised.ReplaceAdvisor(null, advisor2));
            Assert.IsFalse(advised.ReplaceAdvisor(advisor1, null));
            Assert.IsTrue(advised.ReplaceAdvisor(advisor1, advisor2));
            Assert.AreEqual(advisor2, pf.Advisors[0]);
            Assert.AreEqual(5, proxied.Age);
            Assert.AreEqual(1, cba1.GetCalls());
            Assert.AreEqual(2, nop.Count);
            Assert.AreEqual(1, cba2.GetCalls());
            Assert.IsFalse(pf.ReplaceAdvisor(new DefaultPointcutAdvisor(null), advisor1));
        }

        [Test]
        public void IgnoresAdvisorDuplicates()
        {
            CountingBeforeAdvice cba1 = new CountingBeforeAdvice();
            IAdvisor advisor1 = new DefaultPointcutAdvisor(cba1);

            AdvisedSupport advSup = new AdvisedSupport();
            advSup.AddAdvisor(advisor1);
            advSup.AddAdvisor(advisor1);

            Assert.AreEqual(1, advSup.Advisors.Count);
        }

        private class AnonymousClassTimeStamped : ITimeStamped
        {
            public AnonymousClassTimeStamped(ProxyFactoryTests enclosingInstance)
            {
                InitBlock(enclosingInstance);
            }

            private void InitBlock(ProxyFactoryTests enclosingInstance)
            {
                this.enclosingInstance = enclosingInstance;
            }

            private ProxyFactoryTests enclosingInstance;

            public ProxyFactoryTests Enclosing_Instance
            {
                get { return enclosingInstance; }

            }

            public DateTime TimeStamp
            {
                get { throw new NotSupportedException("TimeStamp"); }
            }
        }

        [Test]
        public void AddRepeatedInterface()
        {
            ITimeStamped tst = new AnonymousClassTimeStamped(this);
            ProxyFactory pf = new ProxyFactory(tst);
            // We've already implicitly added this interface.
            // This call should be ignored without error
            pf.AddInterface(typeof(ITimeStamped));
            // All cool
            ITimeStamped ts = (ITimeStamped)pf.GetProxy();
        }

        internal class TestObjectSubclass : TestObject, IComparable
        {
            public override int CompareTo(Object arg0)
            {
                throw new NotSupportedException("compareTo");
            }
        }

        [Test]
        public void GetsAllInterfaces()
        {
            // Extend to get new interface
            TestObjectSubclass raw = new TestObjectSubclass();
            ProxyFactory factory = new ProxyFactory(raw);
            Assert.AreEqual(8, factory.Interfaces.Count, "Found correct number of interfaces");
            //System.out.println("Proxied interfaces are " + StringUtils.arrayToDelimitedString(factory.getProxiedInterfaces(), ","));
            ITestObject tb = (ITestObject)factory.GetProxy();
            Assert.IsTrue(tb is IOther, "Picked up secondary interface");

            raw.Age = 25;
            Assert.IsTrue(tb.Age == raw.Age);

            DateTime t = new DateTime(2004, 8, 1);
            TimestampIntroductionInterceptor ti = new TimestampIntroductionInterceptor(t);

            Console.WriteLine(StringUtils.CollectionToDelimitedString(factory.Interfaces, "/"));

            //factory.addAdvisor(0, new DefaultIntroductionAdvisor(ti, typeof(ITimeStamped)));
            factory.AddIntroduction(
                new DefaultIntroductionAdvisor(ti, typeof(ITimeStamped))
                );

            Console.WriteLine(StringUtils.CollectionToDelimitedString(factory.Interfaces, "/"));

            ITimeStamped ts = (ITimeStamped)factory.GetProxy();
            Assert.IsTrue(ts.TimeStamp == t);
            // Shouldn't fail;
            ((IOther)ts).Absquatulate();
        }

        private class AnonymousClassInterceptor : IInterceptor
        {
        }

        [Test]
        public void CanOnlyAddMethodInterceptors()
        {
            ProxyFactory factory = new ProxyFactory(new TestObject());
            factory.AddAdvice(0, new NopInterceptor());
            try
            {
                factory.AddAdvice(0, new AnonymousClassInterceptor());
                Assert.Fail("Should only be able to add MethodInterceptors");
            }
            catch (AopConfigException)
            {
            }

            // Check we can still use it
            IOther other = (IOther)factory.GetProxy();
            other.Absquatulate();
        }

        [Test]
        public void InterceptorInclusionMethods()
        {
            NopInterceptor di = new NopInterceptor();
            NopInterceptor diUnused = new NopInterceptor(1); // // make instance unique (see SPRNET-847)
            ProxyFactory factory = new ProxyFactory(new TestObject());
            factory.AddAdvice(0, di);
            ITestObject tb = (ITestObject)factory.GetProxy();
            Assert.IsTrue(factory.AdviceIncluded(di));
            Assert.IsTrue(!factory.AdviceIncluded(diUnused));
            Assert.IsTrue(factory.CountAdviceOfType(typeof(NopInterceptor)) == 1);

            factory.AddAdvice(0, diUnused);
            Assert.IsTrue(factory.AdviceIncluded(diUnused));
            Assert.IsTrue(factory.CountAdviceOfType(typeof(NopInterceptor)) == 2);
        }

        [Test]
        public void AddAdvisedSupportListener()
        {
            //MLP SPRNET-1367
            //IDynamicMock mock = new DynamicMock(typeof(IAdvisedSupportListener));
            //IAdvisedSupportListener listener = (IAdvisedSupportListener)mock.Object;
            IAdvisedSupportListener listener = A.Fake<IAdvisedSupportListener>();
            //listener.Activated();
            //mock.Expect("Activated");

            ProxyFactory factory = new ProxyFactory(new TestObject());
            factory.AddListener(listener);
            factory.GetProxy();

            A.CallTo(() => listener.Activated(A<AdvisedSupport>._)).MustHaveHappened();
        }

        [Test]
        public void AdvisedSupportListenerMethodsAreCalledAppropriately()
        {
            IAdvisedSupportListener listener = A.Fake<IAdvisedSupportListener>();

            ProxyFactory factory = new ProxyFactory(new TestObject());
            factory.AddListener(listener);

            // must fire the Activated callback...
            factory.GetProxy();
            // must fire the AdviceChanged callback...
            factory.AddAdvice(new NopInterceptor());
            // must fire the InterfacesChanged callback...
            factory.AddInterface(typeof(ISerializable));

            A.CallTo(() => listener.Activated(A<AdvisedSupport>.That.Not.IsNull())).MustHaveHappened();
            A.CallTo(() => listener.AdviceChanged(A<AdvisedSupport>.That.Not.IsNull())).MustHaveHappened();
            A.CallTo(() => listener.InterfacesChanged(A<AdvisedSupport>.That.Not.IsNull())).MustHaveHappened();
        }

        [Test]
        public void AdvisedSupportListenerMethodsAre_NOT_CalledIfProxyHasNotBeenCreated()
        {
            IAdvisedSupportListener listener = A.Fake<IAdvisedSupportListener>();

            ProxyFactory factory = new ProxyFactory(new TestObject());
            factory.AddListener(listener);

            // must not fire the AdviceChanged callback...
            factory.AddAdvice(new NopInterceptor());
            // must not fire the InterfacesChanged callback...
            factory.AddInterface(typeof(ISerializable));

            A.CallTo(() => listener.AdviceChanged(A<AdvisedSupport>._)).MustNotHaveHappened();
            A.CallTo(() => listener.InterfacesChanged(A<AdvisedSupport>._)).MustNotHaveHappened();
        }

        [Test]
        public void AddNullAdvisedSupportListenerIsOk()
        {
            ProxyFactory factory = new ProxyFactory(new TestObject());
            factory.AddListener(null);
        }

        [Test]
        public void RemoveNullAdvisedSupportListenerIsOk()
        {
            ProxyFactory factory = new ProxyFactory(new TestObject());
            factory.RemoveListener(null);
        }

        [Test]
        public void RemoveAdvisedSupportListener()
        {
            IAdvisedSupportListener listener = A.Fake<IAdvisedSupportListener>();

            ProxyFactory factory = new ProxyFactory(new TestObject());
            factory.AddListener(listener);
            factory.RemoveListener(listener);

            factory.GetProxy();

            // check that no lifecycle callback methods were invoked on the listener...
            A.CallTo(() => listener.Activated(null)).WithAnyArguments().MustNotHaveHappened();
            A.CallTo(() => listener.AdviceChanged(null)).WithAnyArguments().MustNotHaveHappened();
            A.CallTo(() => listener.InterfacesChanged(null)).WithAnyArguments().MustNotHaveHappened();
        }

        [Test]
        public void Frozen_RemoveAdvisor()
        {
            ProxyFactory factory = new ProxyFactory();
            factory.IsFrozen = true;
            Assert.Throws<AopConfigException>(() => factory.RemoveAdvisor(null));
        }

        public interface IMultiProxyingTestInterface
        {
            string TestMethod(string arg);
        }

        public interface IMultiProxyingTestInterface2 : IMultiProxyingTestInterface { }

        public class MultiProxyingTestClass : IMultiProxyingTestInterface2
        {
            public int InvocationCounter;

            public string TestMethod(string arg)
            {
                InvocationCounter++;
                return arg + "|" + arg;
            }
        }

        public interface ICountingIntroduction
        {
            void Inc();
        }

        public class TestCountingIntroduction : ICountingIntroduction, IAdvice
        {
            public int Counter;

            public void Inc()
            {
                Counter++;
            }
        }

        [Test]
        public void NestedProxiesDontInvokeSameAdviceOrIntroductionTwice()
        {
            MultiProxyingTestClass testObj = new MultiProxyingTestClass();
            ProxyFactory pf1 = new ProxyFactory();
            pf1.Target = testObj;

            NopInterceptor di = new NopInterceptor();
            NopInterceptor diUnused = new NopInterceptor(1); // // make instance unique (see SPRNET-847)
            TestCountingIntroduction countingMixin = new TestCountingIntroduction();

            pf1.AddAdvice(diUnused);
            pf1.AddAdvisor(new DefaultPointcutAdvisor(di));
            pf1.AddIntroduction(new DefaultIntroductionAdvisor(countingMixin));

            object innerProxy = pf1.GetProxy();
            ProxyFactory pf2 = new ProxyFactory();
            pf2.Target = innerProxy;
            pf2.AddAdvice(diUnused);
            pf2.AddAdvisor(new DefaultPointcutAdvisor(di));
            pf2.AddIntroduction(new DefaultIntroductionAdvisor(countingMixin));

            object outerProxy = pf2.GetProxy();

            // any advice instance is invoked once only
            string result = ((IMultiProxyingTestInterface)outerProxy).TestMethod("arg");
            Assert.AreEqual(1, testObj.InvocationCounter);
            Assert.AreEqual("arg|arg", result);
            Assert.AreEqual(1, di.Count);

            // any introduction instance is invoked once only
            ((ICountingIntroduction)outerProxy).Inc();
            Assert.AreEqual(1, countingMixin.Counter);
        }
    }
}