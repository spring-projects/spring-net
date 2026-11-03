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
using Spring.Aop.Interceptor;
using Spring.Aop.Target;
using Spring.Objects.Factory.Support;

namespace Spring.Aop.Framework.DynamicProxy;

/// <summary>
/// Proxies borrow their target from the <see cref="ITargetSource"/> for the duration of a call,
/// so they have to give it back on every path out of the call, including when it throws.
/// </summary>
[TestFixture]
public class ReleaseTargetTests
{
    public struct ProbeStruct
    {
        public int Value;
    }

    public interface IReleaseProbe
    {
        void DoVoid();
        string GetString();
        int GetInt();
        ProbeStruct GetStruct();
        void RefOut(ref int counter, out string text);
        void Throw();
        int ThrowInt();
        ProbeStruct ThrowStruct();
        string ThrowString();
    }

    public class ReleaseProbe : IReleaseProbe
    {
        public int VoidCalls;

        public virtual void DoVoid() => VoidCalls++;

        public virtual string GetString() => "text";

        public virtual int GetInt() => 42;

        public virtual ProbeStruct GetStruct() => new ProbeStruct { Value = 7 };

        public virtual void RefOut(ref int counter, out string text)
        {
            counter += 10;
            text = "out:" + counter;
        }

        public virtual void Throw() => throw new InvalidOperationException("boom");

        public virtual int ThrowInt() => throw new InvalidOperationException("boom");

        public virtual ProbeStruct ThrowStruct() => throw new InvalidOperationException("boom");

        public virtual string ThrowString() => throw new InvalidOperationException("boom");

        public bool ThrowFromObjectMethods;

        public override bool Equals(object obj) => ThrowFromObjectMethods ? throw new InvalidOperationException("boom") : base.Equals(obj);

        public override int GetHashCode() => ThrowFromObjectMethods ? throw new InvalidOperationException("boom") : base.GetHashCode();

        public override string ToString() => ThrowFromObjectMethods ? throw new InvalidOperationException("boom") : base.ToString();
    }

    private sealed class ThrowingInterceptor : AopAlliance.Intercept.IMethodInterceptor
    {
        public object Invoke(AopAlliance.Intercept.IMethodInvocation invocation)
        {
            throw new InvalidOperationException("advice boom");
        }
    }

    private static IReleaseProbe CreateProxy(ITargetSource targetSource, bool decorator, bool advised, AopAlliance.Aop.IAdvice advice = null)
    {
        ProxyFactory factory = new ProxyFactory();
        factory.TargetSource = targetSource;
        if (decorator)
        {
            factory.ProxyTargetType = true;
        }
        else
        {
            factory.AddInterface(typeof(IReleaseProbe));
        }

        if (advice != null)
        {
            factory.AddAdvice(advice);
        }
        else if (advised)
        {
            factory.AddAdvice(new NopInterceptor());
        }

        object proxy = factory.GetProxy();
        Assert.That(AopUtils.IsDecoratorAopProxy(proxy), Is.EqualTo(decorator));
        return (IReleaseProbe) proxy;
    }

    private static MockTargetSource CreateCountingTargetSource(out ReleaseProbe probe)
    {
        probe = new ReleaseProbe();
        MockTargetSource targetSource = new MockTargetSource();
        targetSource.SetTarget(probe);
        return targetSource;
    }

    private static void AssertBalanced(MockTargetSource targetSource, int expectedCalls)
    {
        Assert.That(targetSource.gets, Is.EqualTo(expectedCalls));
        Assert.That(targetSource.releases, Is.EqualTo(targetSource.gets));
    }

    // decorator, advised
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void ReleasesTargetAfterVoidCall(bool decorator, bool advised)
    {
        MockTargetSource targetSource = CreateCountingTargetSource(out ReleaseProbe probe);
        IReleaseProbe proxy = CreateProxy(targetSource, decorator, advised);

        proxy.DoVoid();

        Assert.That(probe.VoidCalls, Is.EqualTo(1));
        AssertBalanced(targetSource, 1);
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void ReleasesTargetAfterReferenceTypeReturn(bool decorator, bool advised)
    {
        MockTargetSource targetSource = CreateCountingTargetSource(out _);
        IReleaseProbe proxy = CreateProxy(targetSource, decorator, advised);

        Assert.That(proxy.GetString(), Is.EqualTo("text"));

        AssertBalanced(targetSource, 1);
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void ReleasesTargetAfterValueTypeReturn(bool decorator, bool advised)
    {
        MockTargetSource targetSource = CreateCountingTargetSource(out _);
        IReleaseProbe proxy = CreateProxy(targetSource, decorator, advised);

        Assert.That(proxy.GetInt(), Is.EqualTo(42));
        Assert.That(proxy.GetStruct().Value, Is.EqualTo(7));

        AssertBalanced(targetSource, 2);
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void ReleasesTargetAfterRefAndOutParameters(bool decorator, bool advised)
    {
        MockTargetSource targetSource = CreateCountingTargetSource(out _);
        IReleaseProbe proxy = CreateProxy(targetSource, decorator, advised);

        int counter = 5;
        proxy.RefOut(ref counter, out string text);

        Assert.That(counter, Is.EqualTo(15));
        Assert.That(text, Is.EqualTo("out:15"));
        AssertBalanced(targetSource, 1);
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void ReleasesTargetWhenTargetMethodThrows(bool decorator, bool advised)
    {
        MockTargetSource targetSource = CreateCountingTargetSource(out _);
        IReleaseProbe proxy = CreateProxy(targetSource, decorator, advised);

        Assert.That(() => proxy.Throw(), Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("boom"));
        AssertBalanced(targetSource, 1);

        Assert.That(() => proxy.ThrowInt(), Throws.TypeOf<InvalidOperationException>());
        AssertBalanced(targetSource, 2);

        Assert.That(() => proxy.ThrowStruct(), Throws.TypeOf<InvalidOperationException>());
        AssertBalanced(targetSource, 3);

        Assert.That(() => proxy.ThrowString(), Throws.TypeOf<InvalidOperationException>());
        AssertBalanced(targetSource, 4);
    }

    [Test]
    public void CompositionProxyReleasesTargetWhenObjectMethodsThrow()
    {
        MockTargetSource targetSource = CreateCountingTargetSource(out ReleaseProbe probe);
        IReleaseProbe proxy = CreateProxy(targetSource, decorator: false, advised: true);
        MockTargetSource otherTargetSource = CreateCountingTargetSource(out _);
        IReleaseProbe otherProxy = CreateProxy(otherTargetSource, decorator: false, advised: true);
        probe.ThrowFromObjectMethods = true;

        Assert.That(() => proxy.Equals(new object()), Throws.TypeOf<InvalidOperationException>());
        AssertBalanced(targetSource, 1);

        Assert.That(() => proxy.Equals(otherProxy), Throws.TypeOf<InvalidOperationException>());
        AssertBalanced(targetSource, 2);
        AssertBalanced(otherTargetSource, 1);

        Assert.That(() => proxy.GetHashCode(), Throws.TypeOf<InvalidOperationException>());
        AssertBalanced(targetSource, 3);

        Assert.That(() => proxy.ToString(), Throws.TypeOf<InvalidOperationException>());
        AssertBalanced(targetSource, 4);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ReleasesTargetWhenAdviceThrows(bool decorator)
    {
        MockTargetSource targetSource = CreateCountingTargetSource(out ReleaseProbe probe);
        IReleaseProbe proxy = CreateProxy(targetSource, decorator, advised: true, new ThrowingInterceptor());

        Assert.That(() => proxy.DoVoid(), Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("advice boom"));
        Assert.That(() => proxy.GetInt(), Throws.TypeOf<InvalidOperationException>());

        Assert.That(probe.VoidCalls, Is.EqualTo(0));
        AssertBalanced(targetSource, 2);
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void PoolIsNotExhaustedByCallsThatThrow(bool decorator, bool advised)
    {
        DefaultListableObjectFactory objectFactory = new DefaultListableObjectFactory();
        objectFactory.RegisterObjectDefinition("probe", new RootObjectDefinition(typeof(ReleaseProbe), false));

        SimplePoolTargetSource pool = new SimplePoolTargetSource();
        pool.TargetObjectName = "probe";
        pool.MaxSize = 1;
        pool.ObjectFactory = objectFactory;
        try
        {
            IReleaseProbe proxy = CreateProxy(pool, decorator, advised);

            // an exhausted pool of one blocks the next borrower, so every call
            // has to hand its target back before the next one is made
            for (int i = 0; i < 2; i++)
            {
                Assert.That(() => proxy.Throw(), Throws.TypeOf<InvalidOperationException>());
                Assert.That(pool.Active, Is.EqualTo(0), "target was not released after call " + i);
            }

            proxy.DoVoid();
            Assert.That(pool.Active, Is.EqualTo(0));
        }
        finally
        {
            pool.Dispose();
        }
    }
}
