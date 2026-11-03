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

using NUnit.Framework;
using Spring.Objects;

namespace Spring.Aop.Framework.DynamicProxy;

/// <summary>
/// Additional and overridden tests for the composition-based proxy.
/// </summary>
/// <author>Rod Johnson</author>
/// <author>Juergen Hoeller</author>
/// <author>Bruno Baia (.NET)</author>
[TestFixture]
public class CompositionAopProxyTests : AbstractAopProxyTests
{
    protected override object CreateProxy(AdvisedSupport advisedSupport)
    {
        Assert.That(advisedSupport.ProxyTargetType, Is.False, "Not forcible decorator-based proxy");
        object proxy = CreateAopProxy(advisedSupport).GetProxy();
        Assert.That(AopUtils.IsCompositionAopProxy(proxy), Is.True, "Should be a composition-based proxy: " + proxy.GetType());
        return proxy;
    }

    protected override Type CreateAopProxyType(AdvisedSupport advisedSupport)
    {
        return new CompositionAopProxyTypeBuilder(advisedSupport).BuildProxyType();
    }

    [Test]
    public override void Equality()
    {
        base.Equality();
    }

    [Test]
    public void ProxyIsJustInterface()
    {
        TestObject target = new TestObject();
        target.Age = 32;

        AdvisedSupport advised = new AdvisedSupport();
        advised.Target = target;
        advised.Interfaces = new Type[] { typeof(ITestObject) };

        object proxy = CreateProxy(advised);

        Assert.That(proxy is ITestObject, Is.True);
        Assert.That(proxy is TestObject, Is.False);
    }

    [Test]
    public void ReturnsThisWhenProxyIsIncompatible()
    {
        FooBar obj = new FooBar();

        AdvisedSupport advised = new AdvisedSupport();
        advised.Target = obj;
        advised.Interfaces = new Type[] { typeof(IFoo) };

        IFoo proxy = (IFoo) CreateProxy(advised);

        Assert.That(proxy.GetBarThis(), Is.SameAs(obj),
            "Target should be returned when return types are incompatible");
        Assert.That(proxy.GetFooThis(), Is.SameAs(proxy),
            "Proxy should be returned when return types are compatible");
    }

    public interface IFoo
    {
        IBar GetBarThis();
        IFoo GetFooThis();
    }

    public interface IBar
    {
    }

    public class FooBar : IFoo, IBar
    {
        public IBar GetBarThis()
        {
            return this;
        }

        public IFoo GetFooThis()
        {
            return this;
        }
    }
}