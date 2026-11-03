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

using System.Reflection;
using NUnit.Framework;
using Spring.Aop.Interceptor;
using Spring.Objects;

namespace Spring.Aop.Framework.DynamicProxy;

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
        Assert.That(AopUtils.IsDecoratorAopProxy(proxy), Is.True);
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
        Assert.That(AopUtils.IsDecoratorAopProxy(proxy), Is.True, "Should be a decorator-based proxy");
        Assert.That(proxy is ITestObject, Is.True);
        Assert.That(proxy is TestObject, Is.True);

        ITestObject itb = (ITestObject) proxy;
        Assert.That(itb.Age, Is.EqualTo(32), "Incorrect age");
        TestObject tb = (TestObject) proxy;
        Assert.That(tb.Age, Is.EqualTo(32), "Incorrect age");
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
        Assert.That(proxy, Is.Not.Null);

        Assert.That(proxy.Name, Is.EqualTo(target.Name), "Incorrect name");
        proxy.Name = "Bruno Baia";
        Assert.That(proxy.Name, Is.EqualTo("Bruno Baia"), "Incorrect name");

        Assert.That(ni.Count, Is.EqualTo(3));
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
        Assert.That(proxy, Is.Not.Null);

        // GetName() calls underlying protected "GetNameInternal()" which calls get_Name
        Assert.That(proxy.GetName(), Is.EqualTo(target.Name), "Incorrect name");
        target.Name = "Bruno Baia";
        Assert.That(proxy.GetName(), Is.EqualTo("Bruno Baia"), "Incorrect name");

        Assert.That(ni.Count, Is.EqualTo(2));
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
        Assert.That(proxy, Is.Not.Null);

        // GetName() calls underlying protected "GetNameInternal()" which calls get_Name
        Assert.That(proxy.GetName(), Is.EqualTo(target.Name), "Incorrect name");
        proxy.Name = "Bruno Baia";
        Assert.That(proxy.Name, Is.EqualTo("Bruno Baia"), "Incorrect name");

        Assert.That(ni.Count, Is.EqualTo(3));
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
        Assert.That(proxy, Is.Not.Null);

        // Location is final and doesn't belong to an interface so can't proxy.
        // method call goes directly to the proxy
        // and will not have access to the valid _location field
        Assert.That(proxy.Location, Is.Null);

        Assert.That(ni.Count, Is.EqualTo(0));
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
        Assert.That(proxy, Is.Not.Null);

        Assert.That(proxy.Name, Is.EqualTo(target.Name), "Incorrect name");
        proxy.Name = "Bruno Baia";
        Assert.That(proxy.Name, Is.EqualTo("Bruno Baia"), "Incorrect name");

        Assert.That(ni.Count, Is.EqualTo(3));
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

    [Test]
    public void ProxyTargetVirtualMethodAttributes()
    {
        MarkerClass target = new MarkerClass();

        AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
        advised.Target = target;
        IAopProxy aopProxy = CreateAopProxy(advised);

        object proxy = aopProxy.GetProxy();
        Assert.That(proxy, Is.Not.Null, "The proxy generated by a (valid) call to GetProxy() was null.");

        MethodInfo method = proxy.GetType().GetMethod("MarkerVirtualMethod");
        Assert.That(method, Is.Not.Null);

        object[] attrs = method.GetCustomAttributes(false);
        Assert.That(attrs, Is.Not.Null, "Should have 1 attribute applied to the target method.");
        Assert.That(attrs.Length, Is.EqualTo(1), "Should have 1 attribute applied to the target method.");
        Assert.That(attrs[0].GetType(), Is.EqualTo(typeof(MarkerAttribute)), "Wrong System.Type of Attribute applied to the target method.");
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
        Assert.That(proxy, Is.Not.Null, "The proxy generated by a (valid) call to GetProxy() was null.");

        MethodInfo method = proxy.GetType().GetMethod("MarkerVirtualMethod");
        Assert.That(method, Is.Not.Null);

        object[] attrs = method.GetCustomAttributes(false);
        Assert.That(attrs, Is.Not.Null);
        Assert.That(attrs.Length, Is.EqualTo(0), "Should not have attribute applied to the target method.");
    }

    [Test]
    public void ProxyTargetVirtualMethodParameterAttributes()
    {
        MarkerClass target = new MarkerClass();

        AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
        advised.Target = target;
        IAopProxy aopProxy = CreateAopProxy(advised);

        object proxy = aopProxy.GetProxy();
        Assert.That(proxy, Is.Not.Null, "The proxy generated by a (valid) call to GetProxy() was null.");

        MethodInfo method = proxy.GetType().GetMethod("MarkerVirtualMethod");
        Assert.That(method, Is.Not.Null);

        object[] attrs = method.GetParameters()[1].GetCustomAttributes(false);
        Assert.That(attrs, Is.Not.Null, "Should have had 1 attribute applied to the method's parameter.");
        Assert.That(attrs.Length, Is.EqualTo(1), "Should have had 1 attribute applied to the method's parameter.");
        Assert.That(attrs[0].GetType(), Is.EqualTo(typeof(MarkerAttribute)), "Wrong System.Type of Attribute applied to the method's parameter.");
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
        Assert.That(proxy, Is.Not.Null, "The proxy generated by a (valid) call to GetProxy() was null.");

        MethodInfo method = proxy.GetType().GetMethod("MarkerVirtualMethod");
        Assert.That(method, Is.Not.Null);

        object[] attrs = method.GetParameters()[1].GetCustomAttributes(false);
        Assert.That(attrs, Is.Not.Null);
        Assert.That(attrs.Length, Is.EqualTo(0), "Should not have attribute applied to the method's parameter.");
    }

    [Test]
    public void ProxyTargetVirtualMethodReturnValueAttributes()
    {
        MarkerClass target = new MarkerClass();

        AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
        advised.Target = target;
        IAopProxy aopProxy = CreateAopProxy(advised);

        object proxy = aopProxy.GetProxy();
        Assert.That(proxy, Is.Not.Null, "The proxy generated by a (valid) call to GetProxy() was null.");

        MethodInfo method = proxy.GetType().GetMethod("MarkerVirtualMethod");
        Assert.That(method, Is.Not.Null);

        object[] attrs = method.ReturnTypeCustomAttributes.GetCustomAttributes(false);
        Assert.That(attrs, Is.Not.Null, "Should have had 1 attribute applied to the method's return value.");
        Assert.That(attrs.Length, Is.EqualTo(1), "Should have had 1 attribute applied to the method's return value.");
        Assert.That(attrs[0].GetType(), Is.EqualTo(typeof(MarkerAttribute)), "Wrong System.Type of Attribute applied to the method's return value.");
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
        Assert.That(proxy, Is.Not.Null, "The proxy generated by a (valid) call to GetProxy() was null.");

        MethodInfo method = proxy.GetType().GetMethod("MarkerVirtualMethod");
        Assert.That(method, Is.Not.Null);

        object[] attrs = method.ReturnTypeCustomAttributes.GetCustomAttributes(false);
        Assert.That(attrs, Is.Not.Null);
        Assert.That(attrs.Length, Is.EqualTo(0), "Should not have attribute applied to the method's return value.");
    }

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
    {
    }
}
