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

#region Imports

using System;
using System.Reflection;
using NUnit.Framework;
using Spring.Aop.Interceptor;
using Spring.Objects;

#endregion

namespace Spring.Aop.Framework.DynamicProxy
{
    /// <summary>
    /// Additional and overridden tests for the decorator-based proxy.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Bruno Baia (.NET)</author>
    [TestFixture]
    public class DecoratorAopProxyTests : AbstractAopProxyTests
    {
        protected override object CreateProxy(AdvisedSupport advisedSupport)
        {
            advisedSupport.ProxyTargetType = true;
            object proxy = CreateAopProxy(advisedSupport).GetProxy();
            Assert.IsTrue(AopUtils.IsDecoratorAopProxy(proxy));
            return proxy;
        }

        protected override Type CreateAopProxyType(AdvisedSupport advisedSupport)
        {
            return new DecoratorAopProxyTypeBuilder(advisedSupport).BuildProxyType(); 
        }

        [Test]
        [Ignore("TODO: doesn't work yet for decorator proxies")]
        public override void Equality()
        {
            // TODO - find a way to correctly handle decorator proxy equality
            base.Equality();
        }

        [Test]
        public void CannotProxySealedClass()
        {
            SealedTestObject target = new SealedTestObject();
            mockTargetSource.SetTarget(target);
            AdvisedSupport advised = new AdvisedSupport(new Type[] { });
            advised.TargetSource = mockTargetSource;

            Assert.Throws<AopConfigException>(() => CreateAopProxy(advised));
        }

        [Test]
        public void CannotProxyNonPublicClass()
        {
            NonPublicTestObject target = new NonPublicTestObject();
            mockTargetSource.SetTarget(target);
            AdvisedSupport advised = new AdvisedSupport(new Type[] { });
            advised.TargetSource = mockTargetSource;

            Assert.Throws<AopConfigException>(() => CreateAopProxy(advised));
        }

        [Test]
        public void ProxyCanBeClassAndInterface()
        {
            TestObject target = new TestObject();
            target.Age = 32;
            mockTargetSource.SetTarget(target);
            AdvisedSupport advised = new AdvisedSupport();
            advised.TargetSource = mockTargetSource;
            IAopProxy aop = CreateAopProxy(advised);

            object proxy = aop.GetProxy();
            Assert.IsTrue(AopUtils.IsDecoratorAopProxy(proxy), "Should be a decorator-based proxy");
            Assert.IsTrue(proxy is ITestObject);
            Assert.IsTrue(proxy is TestObject);

            ITestObject itb = (ITestObject)proxy;
            Assert.AreEqual(32, itb.Age, "Incorrect age");
            TestObject tb = (TestObject)proxy;
            Assert.AreEqual(32, tb.Age, "Incorrect age");
        }

        [Test]
        public void InterceptVirtualMethod()
        {
            DoesNotImplementInterfaceTestObject target = new DoesNotImplementInterfaceTestObject();
            target.Name = "Bruno";
            mockTargetSource.SetTarget(target);

            NopInterceptor ni = new NopInterceptor();

            AdvisedSupport advised = new AdvisedSupport();
            advised.TargetSource = mockTargetSource;
            advised.AddAdvice(ni);

            DoesNotImplementInterfaceTestObject proxy = CreateProxy(advised) as DoesNotImplementInterfaceTestObject;
            Assert.IsNotNull(proxy);

            Assert.AreEqual(target.Name, proxy.Name, "Incorrect name");
            proxy.Name = "Bruno Baia";
            Assert.AreEqual("Bruno Baia", proxy.Name, "Incorrect name");

            Assert.AreEqual(3, ni.Count);
        }

        [Test]
        public void InterceptProtectedVirtualMethod()
        {
            DoesNotImplementInterfaceTestObject target = new DoesNotImplementInterfaceTestObject();
            target.Name = "Bruno";
            mockTargetSource.SetTarget(target);

            NopInterceptor ni = new NopInterceptor();

            AdvisedSupport advised = new AdvisedSupport();
            advised.TargetSource = mockTargetSource;
            advised.AddAdvice(ni);

            DoesNotImplementInterfaceTestObject proxy = CreateProxy(advised) as DoesNotImplementInterfaceTestObject;
            Assert.IsNotNull(proxy);

            // GetName() calls underlying protected "GetNameInternal()" which calls get_Name
            Assert.AreEqual(target.Name, proxy.GetName(), "Incorrect name");
            target.Name = "Bruno Baia";
            Assert.AreEqual("Bruno Baia", proxy.GetName(), "Incorrect name");

            Assert.AreEqual(2, ni.Count);
        }

        [Test]
        public void InterceptInheritedVirtualMethods()
        {
            DoesNotImplementInterfaceTestObject target = new DerivedDoesNotImplementInterfaceTestObject();
            target.Name = "Bruno";
            mockTargetSource.SetTarget(target);

            NopInterceptor ni = new NopInterceptor();

            AdvisedSupport advised = new AdvisedSupport();
            advised.TargetSource = mockTargetSource;
            advised.AddAdvice(ni);

            DoesNotImplementInterfaceTestObject proxy = CreateProxy(advised) as DoesNotImplementInterfaceTestObject;
            Assert.IsNotNull(proxy);

            // GetName() calls underlying protected "GetNameInternal()" which calls get_Name
            Assert.AreEqual(target.Name, proxy.GetName(), "Incorrect name");
            proxy.Name = "Bruno Baia";
            Assert.AreEqual("Bruno Baia", proxy.Name, "Incorrect name");

            Assert.AreEqual(3, ni.Count);
        }

        [Test]
        public void CannotInterceptFinalMethodThatDoesNotBelongToAnInterface()
        {
            DoesNotImplementInterfaceTestObject target = new DoesNotImplementInterfaceTestObject();
            target.Location = "Paris";
            mockTargetSource.SetTarget(target);

            NopInterceptor ni = new NopInterceptor();

            AdvisedSupport advised = new AdvisedSupport();
            advised.TargetSource = mockTargetSource;
            advised.AddAdvice(ni);

            DoesNotImplementInterfaceTestObject proxy = CreateProxy(advised) as DoesNotImplementInterfaceTestObject;
            Assert.IsNotNull(proxy);

            // Location is final and doesn't belong to an interface so can't proxy.
            // method call goes directly to the proxy 
            // and will not have access to the valid _location field
            Assert.IsNull(proxy.Location);

            Assert.AreEqual(0, ni.Count);
        }

        [Test]
        public void InterceptFinalMethodThatBelongsToAnInterface()
        {
            TestObject target = new TestObject();
            target.Name = "Bruno";
            mockTargetSource.SetTarget(target);

            NopInterceptor ni = new NopInterceptor();

            AdvisedSupport advised = new AdvisedSupport(mockTargetSource);
            advised.AddAdvice(ni);

            // Cast to the interface that method belongs to
            ITestObject proxy = CreateProxy(advised) as ITestObject;
            Assert.IsNotNull(proxy);

            Assert.AreEqual(target.Name, proxy.Name, "Incorrect name");
            proxy.Name = "Bruno Baia";
            Assert.AreEqual("Bruno Baia", proxy.Name, "Incorrect name");

            Assert.AreEqual(3, ni.Count);
        }

        [Test]
        public override void ProxyMethodWithRefOutParametersWithStandardReflection()
        {
            Assert.Throws<AopConfigException>(() => ProxyMethodWithRefOutParametersWithStandardReflectionWrapped(), "Cannot create decorator-based IAopProxy for a non visible class [Spring.Aop.Framework.DynamicProxy.AbstractAopProxyTests+InternalRefOutTestObject]");
        }

        private void ProxyMethodWithRefOutParametersWithStandardReflectionWrapped()
        {
            base.ProxyMethodWithRefOutParametersWithStandardReflection();
        }

        [Test]
        public override void ProxyGenericMethodWithRefOutParametersWithStandardReflection()
        {
            Assert.Throws<AopConfigException>(() => ProxyGenericMethodWithRefOutParametersWithStandardReflectionWrapped(), "Cannot create decorator-based IAopProxy for a non visible class [Spring.Aop.Framework.DynamicProxy.AbstractAopProxyTests+InternalRefOutGenericTestObject]");
        }

        private void ProxyGenericMethodWithRefOutParametersWithStandardReflectionWrapped()
        {
            base.ProxyGenericMethodWithRefOutParametersWithStandardReflection();
        }

        #region Attributes

        [Test]
        public void ProxyTargetVirtualMethodAttributes()
        {
            MarkerClass target = new MarkerClass();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
            advised.Target = target;
            IAopProxy aopProxy = CreateAopProxy(advised);

            object proxy = aopProxy.GetProxy();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to GetProxy() was null.");

            MethodInfo method = proxy.GetType().GetMethod("MarkerVirtualMethod");
            Assert.IsNotNull(method);

            object[] attrs = method.GetCustomAttributes(false);
            Assert.IsNotNull(attrs, "Should have 1 attribute applied to the target method.");
            Assert.AreEqual(1, attrs.Length, "Should have 1 attribute applied to the target method.");
            Assert.AreEqual(typeof(MarkerAttribute), attrs[0].GetType(), "Wrong System.Type of Attribute applied to the target method.");
        }

        [Test]
        public void DoesNotProxyTargetVirtualMethodAttributesWithProxyTargetAttributesEqualsFalse()
        {
            MarkerClass target = new MarkerClass();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
            advised.Target = target;
            advised.ProxyTargetAttributes = false;
            IAopProxy aopProxy = CreateAopProxy(advised);

            object proxy = aopProxy.GetProxy();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to GetProxy() was null.");

            MethodInfo method = proxy.GetType().GetMethod("MarkerVirtualMethod");
            Assert.IsNotNull(method);

            object[] attrs = method.GetCustomAttributes(false);
            Assert.IsNotNull(attrs);
            Assert.AreEqual(0, attrs.Length, "Should not have attribute applied to the target method.");
        }

        [Test]
        public void ProxyTargetVirtualMethodParameterAttributes()
        {
            MarkerClass target = new MarkerClass();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
            advised.Target = target;
            IAopProxy aopProxy = CreateAopProxy(advised);

            object proxy = aopProxy.GetProxy();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to GetProxy() was null.");

            MethodInfo method = proxy.GetType().GetMethod("MarkerVirtualMethod");
            Assert.IsNotNull(method);

            object[] attrs = method.GetParameters()[1].GetCustomAttributes(false);
            Assert.IsNotNull(attrs, "Should have had 1 attribute applied to the method's parameter.");
            Assert.AreEqual(1, attrs.Length, "Should have had 1 attribute applied to the method's parameter.");
            Assert.AreEqual(typeof(MarkerAttribute), attrs[0].GetType(), "Wrong System.Type of Attribute applied to the method's parameter.");
        }

        [Test]
        public void DoesNotProxyTargetVirtualMethodParameterAttributesWithProxyTargetAttributesEqualsFalse()
        {
            MarkerClass target = new MarkerClass();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
            advised.Target = target;
            advised.ProxyTargetAttributes = false;
            IAopProxy aopProxy = CreateAopProxy(advised);

            object proxy = aopProxy.GetProxy();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to GetProxy() was null.");

            MethodInfo method = proxy.GetType().GetMethod("MarkerVirtualMethod");
            Assert.IsNotNull(method);

            object[] attrs = method.GetParameters()[1].GetCustomAttributes(false);
            Assert.IsNotNull(attrs);
            Assert.AreEqual(0, attrs.Length, "Should not have attribute applied to the method's parameter.");
        }

        [Test]
        public void ProxyTargetVirtualMethodReturnValueAttributes()
        {
            MarkerClass target = new MarkerClass();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
            advised.Target = target;
            IAopProxy aopProxy = CreateAopProxy(advised);

            object proxy = aopProxy.GetProxy();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to GetProxy() was null.");

            MethodInfo method = proxy.GetType().GetMethod("MarkerVirtualMethod");
            Assert.IsNotNull(method);

            object[] attrs = method.ReturnTypeCustomAttributes.GetCustomAttributes(false);
            Assert.IsNotNull(attrs, "Should have had 1 attribute applied to the method's return value.");
            Assert.AreEqual(1, attrs.Length, "Should have had 1 attribute applied to the method's return value.");
            Assert.AreEqual(typeof(MarkerAttribute), attrs[0].GetType(), "Wrong System.Type of Attribute applied to the method's return value.");
        }

        [Test]
        public void DoesNotProxyTargetVirtualMethodReturnValueAttributesWithProxyTargetAttributesEqualsFalse()
        {
            MarkerClass target = new MarkerClass();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
            advised.Target = target;
            advised.ProxyTargetAttributes = false;
            IAopProxy aopProxy = CreateAopProxy(advised);

            object proxy = aopProxy.GetProxy();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to GetProxy() was null.");

            MethodInfo method = proxy.GetType().GetMethod("MarkerVirtualMethod");
            Assert.IsNotNull(method);

            object[] attrs = method.ReturnTypeCustomAttributes.GetCustomAttributes(false);
            Assert.IsNotNull(attrs);
            Assert.AreEqual(0, attrs.Length, "Should not have attribute applied to the method's return value.");
        }

        #endregion

        #region Helper classes definitions

        internal class NonPublicTestObject
        {
        }

        public sealed class SealedTestObject
        {
        }

        public class DoesNotImplementInterfaceTestObject
        {
            // virtual property
            private string _name;
            public virtual string Name
            {
                get { return _name; }
                set { _name = value; }
            }

            // final method
            private string _location;
            public string Location
            {
                get { return _location; }
                set { _location = value; }
            }

            // protected virtual method
            protected virtual string GetNameInternal()
            {
                return this.Name;
            }

            // public final method calling protected
            public string GetName()
            {
                return GetNameInternal();
            }
        }

        public class DerivedDoesNotImplementInterfaceTestObject : DoesNotImplementInterfaceTestObject
        {}

        #endregion
    }
}
