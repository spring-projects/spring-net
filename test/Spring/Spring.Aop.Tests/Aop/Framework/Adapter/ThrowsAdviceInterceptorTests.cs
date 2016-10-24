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
using System.Runtime.Remoting;
using System.Web;
using AopAlliance.Intercept;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Util;

#endregion

namespace Spring.Aop.Framework.Adapter
{
	/// <summary>
	/// Unit tests for the ThrowsAdviceInterceptor class.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Simon White (.NET)</author>
	[TestFixture]
	public sealed class ThrowsAdviceInterceptorTests
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
            MockRepository repository = new MockRepository();
		    IMethodInvocation mi = (IMethodInvocation) repository.CreateMock(typeof (IMethodInvocation));
            
            MyThrowsHandler th = new MyThrowsHandler();
            ThrowsAdviceInterceptor ti = new ThrowsAdviceInterceptor(th);
            object ret = new object();

		    Expect.Call(mi.Proceed()).Return(ret);
            repository.ReplayAll();
            Assert.AreEqual(ret, ti.Invoke(mi));
            Assert.AreEqual(0, th.GetCalls());
            repository.VerifyAll();

            

		}

		[Test]
		public void NoHandlerMethodForThrowable()
		{   
            MyThrowsHandler th = new MyThrowsHandler();
            ThrowsAdviceInterceptor ti = new ThrowsAdviceInterceptor(th);
            Assert.AreEqual(2, ti.HandlerMethodCount);
            Exception ex = new Exception();

            MockRepository repository = new MockRepository();
            IMethodInvocation mi = (IMethodInvocation)repository.CreateMock(typeof(IMethodInvocation));
		    Expect.Call(mi.Proceed()).Throw(ex);
            repository.ReplayAll();
            try
            {
                ti.Invoke(mi);
                Assert.Fail();
            }
            catch (Exception caught)
            {
                Assert.AreEqual(ex, caught);
            }
            Assert.AreEqual(0, th.GetCalls());
            repository.VerifyAll();

		}

		[Test]
		public void CorrectHandlerUsed()
		{
 
            MyThrowsHandler th = new MyThrowsHandler();
            ThrowsAdviceInterceptor ti = new ThrowsAdviceInterceptor(th);
            HttpException ex = new HttpException();

            MockRepository repository = new MockRepository();
            IMethodInvocation mi = (IMethodInvocation)repository.CreateMock(typeof(IMethodInvocation));

		    Expect.Call(mi.Method).Return(ReflectionUtils.GetMethod(typeof (object), "HashCode", new Type[] {}));
		    Expect.Call(mi.Arguments).Return(null);
		    Expect.Call(mi.This).Return(new object());
            Expect.Call(mi.Proceed()).Throw(ex);
            repository.ReplayAll();
            try
            {
                ti.Invoke(mi);
                Assert.Fail();
            }
            catch (Exception caught)
            {
                Assert.AreEqual(ex, caught);
            }
            Assert.AreEqual(1, th.GetCalls());
            Assert.AreEqual(1, th.GetCalls("HttpException"));

            repository.VerifyAll();

        }

        [Test]
        public void NestedInnerExceptionsAreNotPickedUp()
        {
            MyThrowsHandler throwsHandler = new MyThrowsHandler();
            ThrowsAdviceInterceptor throwsInterceptor = new ThrowsAdviceInterceptor(throwsHandler);
            // nest the exceptions; make sure the advice gets applied because of the inner exception...
            Exception exception = new FormatException("Parent", new HttpException("Inner"));
            MockRepository repository = new MockRepository();
            IMethodInvocation invocation = (IMethodInvocation)repository.CreateMock(typeof(IMethodInvocation));
            Expect.Call(invocation.Proceed()).Throw(exception);
            repository.ReplayAll();
            try
            {
                throwsInterceptor.Invoke(invocation);
                Assert.Fail("Must have failed (by throwing an exception by this point - check the mock).");
            }
            catch (Exception caught)
            {
                Assert.AreEqual(exception, caught);
            }
            Assert.AreEqual(0, throwsHandler.GetCalls(),
                "Must NOT have been handled, 'cos the HttpException was wrapped by " +
                "another Exception that did not have a handler.");
            Assert.AreEqual(0, throwsHandler.GetCalls("HttpException"),
                "Similarly, must NOT have been handled, 'cos the HttpException was wrapped by " +
                "another Exception that did not have a handler.");
            repository.VerifyAll();
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

            MockRepository repository = new MockRepository();
            IMethodInvocation mi = (IMethodInvocation)repository.CreateMock(typeof(IMethodInvocation));
		    Expect.Call(mi.Proceed()).Throw(ex);
            repository.ReplayAll();
            try
            {
                ti.Invoke(mi);
                Assert.Fail();
            }
            catch (Exception caught)
            {
                Assert.AreEqual(ex, caught);
            }
            Assert.AreEqual(1, th.GetCalls());
            Assert.AreEqual(1, th.GetCalls("RemotingException"));

            repository.VerifyAll();
		}

		[Test]
		public void HandlerMethodThrowsException()
		{   
            Exception exception = new Exception();
            MyThrowsHandler handler = new ThrowingMyHandler(exception);
            ThrowsAdviceInterceptor interceptor = new ThrowsAdviceInterceptor(handler);
            // extends RemotingException...
            RemotingTimeoutException ex = new RemotingTimeoutException();

            MockRepository repository = new MockRepository();
            IMethodInvocation mi = (IMethodInvocation)repository.CreateMock(typeof(IMethodInvocation));
		    Expect.Call(mi.Proceed()).Throw(ex);
            repository.ReplayAll();
            try
            {
                interceptor.Invoke(mi);
                Assert.Fail("Should not have reached this point, should have thrown an exception.");
            }
            catch (Exception caught)
            {
                Assert.AreEqual(exception, caught);
            }
            Assert.AreEqual(1, handler.GetCalls());
            Assert.AreEqual(1, handler.GetCalls("RemotingException"));
            repository.VerifyAll();

		}

        #region Helper Classes

        private sealed class MultipleMethodsAreApplicableThrowsHandler
        {
            public void AfterThrowing(
                MethodInfo method, object[] args, object target, RemotingException ex)
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

	    #endregion
	}
}