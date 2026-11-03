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

using AopAlliance.Aop;
using NUnit.Framework;
using AopAlliance.Intercept;
using Spring.Threading;

namespace Spring.Aop.Framework;

/// <summary>
/// Unit tests for the AopContext class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class AopContextTests
{
    [SetUp]
    public void SetUp()
    {
        // makes sure the context is always empty before any unit test...
        try
        {
            do
            {
                AopContext.PopProxy();
            } while (true);
        }
        catch (AopConfigException)
        {
        }
    }

    [Test]
    public void CurrentProxyChokesIfNoAopProxyIsOnTheStack()
    {
        Assert.Throws<AopConfigException>(() => AopContext.CurrentProxy.ToString());
    }

    [Test]
    public void CurrentProxyStackJustPeeksItDoesntPop()
    {
        string foo = "Foo";
        AopContext.PushProxy(foo);
        object fooref = AopContext.CurrentProxy;
        Assert.That(ReferenceEquals(foo, fooref),
            Is.True,
            "Not the exact same instance (must be).");
        // must not have been popped off the stack by looking at it...
        object foorefref = AopContext.CurrentProxy;
        Assert.That(ReferenceEquals(fooref, foorefref),
            Is.True,
            "Not the exact same instance (must be).");
    }

    [Test]
    public void PopProxyWithNothingOnStack()
    {
        Assert.Throws<AopConfigException>(() => AopContext.PopProxy());
    }

    [Test(Description = "SPRNET-1158")]
    public void IsActiveMatchesStackState()
    {
        Assert.That(AopContext.IsActive, Is.False);
        AopContext.PushProxy("Foo");
        Assert.That(AopContext.IsActive, Is.True);
        AopContext.PopProxy();
        Assert.That(AopContext.IsActive, Is.False);
    }

    [Test, Explicit]
    public void ProxyPerformanceTests()
    {
        int runs = 5000000;
        StopWatch watch = new StopWatch();

        ITestObject testObject = new ChainableTestObject(null);
        using (watch.Start("Naked Duration: {0}"))
        {
            for (int i = 0; i < runs; i++)
            {
                object result = testObject.DoSomething(this);
            }
        }

        ITestObject hardcodedWrapper = new ChainableTestObject(testObject);
        using (watch.Start("Hardcoded Wrapper Duration: {0}"))
        {
            for (int i = 0; i < runs; i++)
            {
                object result = hardcodedWrapper.DoSomething(this);
            }
        }

        PeformanceTestAopContextInterceptor interceptor = new PeformanceTestAopContextInterceptor();
        ITestObject proxy = CreateProxy(testObject, interceptor, false);
        using (watch.Start("Proxy Duration ('ExposeProxy'==false): {0}"))
        {
            for (int i = 0; i < runs; i++)
            {
                object result = proxy.DoSomething(this);
            }
        }

        Assert.That(interceptor.Calls, Is.EqualTo(runs));

        interceptor = new PeformanceTestAopContextInterceptor();
        proxy = CreateProxy(testObject, interceptor, true);
        using (watch.Start("Proxy Duration ('ExposeProxy'==true): {0}"))
        {
            for (int i = 0; i < runs; i++)
            {
                object result = proxy.DoSomething(this);
            }
        }

        Assert.That(interceptor.Calls, Is.EqualTo(runs));
    }

    private class PeformanceTestAopContextInterceptor : IMethodInterceptor
    {
        public int Calls = 0;

        public object Invoke(IMethodInvocation invocation)
        {
            Calls++;
            Object ret = invocation.Proceed();
            return ret;
        }
    }

    [Test(Description = "http://opensource.atlassian.com/projects/spring/browse/SPRNET-341")]
    public void CurrentProxyIsThreadSafe()
    {
        ProxyTestObjectAndExposeProxy();

        AsyncTestMethod t1 = new AsyncTestMethod(1000, new ThreadStart(ProxyTestObjectAndExposeProxy));
        AsyncTestMethod t2 = new AsyncTestMethod(1000, new ThreadStart(ProxyTestObjectAndExposeProxy));
        t1.Start();
        t2.Start();

        t1.AssertNoException();
        t2.AssertNoException();
    }

    public interface ITestObject
    {
        object DoSomething(object arg);
    }

    private class ChainableTestObject : ITestObject
    {
        private readonly ITestObject next;

        public ChainableTestObject(ITestObject next)
        {
            this.next = next;
        }

        public virtual object DoSomething(object arg)
        {
            if (next != null)
            {
                return next.DoSomething(arg);
            }

            // simulate some sensible work
            string rep = string.Format("{0} {1}", this.GetType(), arg.GetHashCode());
            return arg;
        }
    }

    private void ProxyTestObjectAndExposeProxy()
    {
        TestAopContextInterceptor interceptor = new TestAopContextInterceptor();

        ITestObject proxy = CreateProxyChain(interceptor, true);

        Assert.That(AopContext.IsActive, Is.False);
        Assert.That(proxy.DoSomething(this), Is.EqualTo(this), "Incorrect return value");
        Assert.That(AopContext.IsActive, Is.False);
        Assert.That(interceptor.Calls, Is.EqualTo(2)); // 2 interceptions on the way
    }

    private ITestObject CreateProxyChain(IAdvice interceptor, bool exposeProxy)
    {
        ITestObject first = new ChainableTestObject(null);
        ITestObject firstProxy = CreateProxy(first, interceptor, exposeProxy);
        Assert.That(firstProxy, Is.Not.Null);

        ITestObject second = new ChainableTestObject(firstProxy);
        ITestObject secondProxy = CreateProxy(second, interceptor, exposeProxy);
        Assert.That(secondProxy, Is.Not.Null);
        return secondProxy;
    }

    private ITestObject CreateProxy(object target, IAdvice interceptor, bool exposeProxy)
    {
        ProxyFactory pf = new ProxyFactory(target);
        pf.ExposeProxy = exposeProxy;
//            pf.Target = target;
        pf.AddAdvice(interceptor);

        return pf.GetProxy() as ITestObject;
    }

    private class TestAopContextInterceptor : IMethodInterceptor
    {
        public int Calls = 0;

        public object Invoke(IMethodInvocation invocation)
        {
            Calls++;
            Assert.That(AopContext.IsActive, Is.True);
            Assert.That(AopContext.CurrentProxy, Is.Not.Null);
            Assert.That(AopContext.CurrentProxy, Is.SameAs(invocation.Proxy));

            Object ret = invocation.Proceed();

            Assert.That(AopContext.CurrentProxy, Is.Not.Null);
            Assert.That(AopContext.IsActive, Is.True);
            return ret;
        }
    }
}
