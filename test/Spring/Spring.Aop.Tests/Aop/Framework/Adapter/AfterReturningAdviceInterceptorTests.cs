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
using AopAlliance.Intercept;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Util;

#endregion

namespace Spring.Aop.Framework.Adapter
{
	/// <summary>
	/// Unit tests for the AfterReturningAdviceInterceptor class.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Simon White (.NET)</author>
	[TestFixture]
	public sealed class AfterReturningAdviceInterceptorTests
	{
		[Test]
		public void PassNullAdviceToCtor()
		{
            Assert.Throws<ArgumentNullException>(() => new AfterReturningAdviceInterceptor(null));
		}

		[Test]
		public void IsNotInvokedIfServiceObjectThrowsException()
		{
            MockRepository repository = new MockRepository();
            IMethodInvocation mockInvocation = (IMethodInvocation)repository.CreateMock(typeof(IMethodInvocation));
            IAfterReturningAdvice mockAdvice = (IAfterReturningAdvice)repository.CreateMock(typeof(IAfterReturningAdvice));
            mockAdvice.AfterReturning(null, null, null, null);
            LastCall.IgnoreArguments();
            LastCall.Throw(new FormatException());

            Expect.Call(mockInvocation.Method).Return(ReflectionUtils.GetMethod(typeof(object), "HashCode", new Type[] { }));
            Expect.Call(mockInvocation.Arguments).Return(null);
            Expect.Call(mockInvocation.This).Return(new object());
            Expect.Call(mockInvocation.Proceed()).Return(null);

            repository.ReplayAll();

            try
            {
                AfterReturningAdviceInterceptor interceptor = new AfterReturningAdviceInterceptor(mockAdvice);
                interceptor.Invoke(mockInvocation);
                Assert.Fail("Must have thrown a FormatException by this point.");
            }
            catch (FormatException)
            {
            }

            repository.VerifyAll();

		}

		[Test]
		public void JustPassesAfterReturningAdviceExceptionUpWithoutAnyWrapping()
		{

            MockRepository repository = new MockRepository();
            IMethodInvocation mockInvocation = (IMethodInvocation)repository.CreateMock(typeof(IMethodInvocation));
		    IAfterReturningAdvice mockAdvice = (IAfterReturningAdvice) repository.CreateMock(typeof (IAfterReturningAdvice));
		    mockAdvice.AfterReturning(null,null,null,null);
		    LastCall.IgnoreArguments();
		    LastCall.Throw(new FormatException());

            Expect.Call(mockInvocation.Method).Return(ReflectionUtils.GetMethod(typeof(object), "HashCode", new Type[] { }));
            Expect.Call(mockInvocation.Arguments).Return(null);
            Expect.Call(mockInvocation.This).Return(new object());           
		    Expect.Call(mockInvocation.Proceed()).Return(null);

            repository.ReplayAll();

            try
            {
                AfterReturningAdviceInterceptor interceptor = new AfterReturningAdviceInterceptor(mockAdvice);
                interceptor.Invoke(mockInvocation);
                Assert.Fail("Must have thrown a FormatException by this point.");
            }
            catch (FormatException)
            {
            }
            repository.VerifyAll();





		}
	}
}