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

using System.Reflection;
using System.Runtime.Remoting;
using System.Web;
using AopAlliance.Intercept;
using FakeItEasy;
using NUnit.Framework;
using Spring.Util;

namespace Spring.Aop.Framework.Adapter;

/// <summary>
/// Unit tests for the ThrowsAdviceInterceptor class.
/// </summary>
/// <author>Rod Johnson</author>
/// <author>Simon White (.NET)</author>
[TestFixture]
public sealed partial class ThrowsAdviceInterceptorTests
{
    [Test]
    public void NoHandlerMethods()
    {
        Assert.Throws<ArgumentException>(() => new ThrowsAdviceInterceptor(new object()));
    }

    [Test]
    public void PassNullAdviceToCtor()
    {
        Assert.Throws<ArgumentNullException>(() => new ThrowsAdviceInterceptor(null));
    }

    [Test]
    public void NotInvoked()
    {
        IMethodInvocation mi = A.Fake<IMethodInvocation>();

        MyThrowsHandler th = new MyThrowsHandler();
        ThrowsAdviceInterceptor ti = new ThrowsAdviceInterceptor(th);
        object ret = new object();

        A.CallTo(() => mi.Proceed()).Returns(ret);

        Assert.That(ti.Invoke(mi), Is.EqualTo(ret));
        Assert.That(th.GetCalls(), Is.EqualTo(0));
    }

    [Test]
    public void NoHandlerMethodForThrowable()
    {
        MyThrowsHandler th = new MyThrowsHandler();
        ThrowsAdviceInterceptor ti = new ThrowsAdviceInterceptor(th);
        Assert.That(ti.HandlerMethodCount, Is.EqualTo(2));
        Exception ex = new Exception();

        IMethodInvocation mi = A.Fake<IMethodInvocation>();
        A.CallTo(() => mi.Proceed()).Throws(ex);

        try
        {
            ti.Invoke(mi);
            Assert.Fail();
        }
        catch (Exception caught)
        {
            Assert.That(caught, Is.EqualTo(ex));
        }

        Assert.That(th.GetCalls(), Is.EqualTo(0));
    }

    [Test]
    public void CorrectHandlerUsed()
    {
        MyThrowsHandler th = new MyThrowsHandler();
        ThrowsAdviceInterceptor ti = new ThrowsAdviceInterceptor(th);
        HttpException ex = new HttpException();

        IMethodInvocation mi = A.Fake<IMethodInvocation>();

        A.CallTo(() => mi.Method).Returns(ReflectionUtils.GetMethod(typeof(object), "HashCode", new Type[] { }));
        A.CallTo(() => mi.Arguments).Returns(null);
        A.CallTo(() => mi.This).Returns(new object());
        A.CallTo(() => mi.Proceed()).Throws(ex);

        try
        {
            ti.Invoke(mi);
            Assert.Fail();
        }
        catch (Exception caught)
        {
            Assert.That(caught, Is.EqualTo(ex));
        }

        Assert.That(th.GetCalls(), Is.EqualTo(1));
        Assert.That(th.GetCalls("HttpException"), Is.EqualTo(1));
    }

    [Test]
    public void NestedInnerExceptionsAreNotPickedUp()
    {
        MyThrowsHandler throwsHandler = new MyThrowsHandler();
        ThrowsAdviceInterceptor throwsInterceptor = new ThrowsAdviceInterceptor(throwsHandler);
        // nest the exceptions; make sure the advice gets applied because of the inner exception...
        Exception exception = new FormatException("Parent", new HttpException("Inner"));

        IMethodInvocation invocation = A.Fake<IMethodInvocation>();
        A.CallTo(() => invocation.Proceed()).Throws(exception);

        try
        {
            throwsInterceptor.Invoke(invocation);
            Assert.Fail("Must have failed (by throwing an exception by this point - check the mock).");
        }
        catch (Exception caught)
        {
            Assert.That(caught, Is.EqualTo(exception));
        }

        Assert.That(throwsHandler.GetCalls(), Is.EqualTo(0),
            "Must NOT have been handled, 'cos the HttpException was wrapped by " +
            "another Exception that did not have a handler.");
        Assert.That(throwsHandler.GetCalls("HttpException"), Is.EqualTo(0),
            "Similarly, must NOT have been handled, 'cos the HttpException was wrapped by " +
            "another Exception that did not have a handler.");
    }

    [Test]
    public void ChokesOnHandlerWhereMultipleMethodsAreApplicable()
    {
        object throwsHandler = new MultipleMethodsAreApplicableThrowsHandler();
        Assert.Throws<ArgumentException>(() => new ThrowsAdviceInterceptor(throwsHandler));
    }

    [Test]
    public void CorrectHandlerUsedForSubclass()
    {
        MyThrowsHandler th = new MyThrowsHandler();
        ThrowsAdviceInterceptor ti = new ThrowsAdviceInterceptor(th);
        // Extends RemotingException
        RemotingTimeoutException ex = new RemotingTimeoutException();

        IMethodInvocation mi = A.Fake<IMethodInvocation>();
        A.CallTo(() => mi.Proceed()).Throws(ex);

        try
        {
            ti.Invoke(mi);
            Assert.Fail();
        }
        catch (Exception caught)
        {
            Assert.That(caught, Is.EqualTo(ex));
        }

        Assert.That(th.GetCalls(), Is.EqualTo(1));
        Assert.That(th.GetCalls("RemotingException"), Is.EqualTo(1));
    }

    [Test]
    public void HandlerMethodThrowsException()
    {
        Exception exception = new Exception();
        MyThrowsHandler handler = new ThrowingMyHandler(exception);
        ThrowsAdviceInterceptor interceptor = new ThrowsAdviceInterceptor(handler);

        IMethodInvocation mi = A.Fake<IMethodInvocation>();
        A.CallTo(() => mi.Proceed()).Throws(new RemotingTimeoutException());

        try
        {
            interceptor.Invoke(mi);
            Assert.Fail("Should not have reached this point, should have thrown an exception.");
        }
        catch (Exception caught)
        {
            Assert.That(caught, Is.EqualTo(exception));
        }

        Assert.That(handler.GetCalls(), Is.EqualTo(1));
        Assert.That(handler.GetCalls("RemotingException"), Is.EqualTo(1));
    }

    private sealed class MultipleMethodsAreApplicableThrowsHandler
    {
        public void AfterThrowing(MethodInfo method, object[] args, object target, RemotingException ex)
        {
        }

        public void AfterThrowing(RemotingException ex)
        {
        }
    }

    private class ThrowingMyHandler : MyThrowsHandler
    {
        private Exception exception;

        public ThrowingMyHandler(Exception ex)
        {
            this.exception = ex;
        }

        public override void AfterThrowing(RemotingException ex)
        {
            base.AfterThrowing(ex);
            throw exception;
        }
    }

    public class MyThrowsHandler : MethodCounter, IThrowsAdvice
    {
        public void AfterThrowing(
            MethodInfo m, object[] args, object target, HttpException ex)
        {
            Count("HttpException");
        }

        public virtual void AfterThrowing(RemotingException ex)
        {
            Count("RemotingException");
        }

        // not valid, wrong number of arguments...
        public void AfterThrowing(MethodInfo m, Exception ex)
        {
            throw new NotSupportedException("Shouldn't be called");
        }
    }

    public interface IEcho
    {
        int A { get; set; }

        int EchoException(int i, Exception t);
    }

    public class Echo : IEcho
    {
        private int a;

        public int A
        {
            get { return a; }
            set { a = value; }
        }

        public virtual int EchoException(int i, Exception ex)
        {
            if (ex != null)
            {
                throw ex;
            }

            return i;
        }
    }
}
