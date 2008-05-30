#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using DotNetMock.Dynamic;
using NUnit.Framework;

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
		[ExpectedException(typeof (ArgumentNullException))]
		public void PassNullAdviceToCtor()
		{
			new AfterReturningAdviceInterceptor(null);
		}

		[Test]
		public void IsNotInvokedIfServiceObjectThrowsException()
		{
			IDynamicMock mockAdvice = new DynamicMock(typeof (IAfterReturningAdvice));
			IAfterReturningAdvice afterAdvice = (IAfterReturningAdvice) mockAdvice.Object;

			IDynamicMock mockInvocation = new DynamicMock(typeof (IMethodInvocation));
			IMethodInvocation invocation = (IMethodInvocation) mockInvocation.Object;
			mockInvocation.ExpectAndThrow("Proceed", new FormatException(), null);

			try
			{
				AfterReturningAdviceInterceptor interceptor = new AfterReturningAdviceInterceptor(afterAdvice);
				interceptor.Invoke(invocation);
				Assert.Fail("Must have thrown a FormatException by this point.");
			}
			catch (FormatException)
			{
			}

			mockAdvice.Verify(); // must not have been called...
			mockInvocation.Verify();
		}

		[Test]
		public void JustPassesAfterReturningAdviceExceptionUpWithoutAnyWrapping()
		{
			IDynamicMock mockAdvice = new DynamicMock(typeof (IAfterReturningAdvice));
			IAfterReturningAdvice afterAdvice = (IAfterReturningAdvice) mockAdvice.Object;
			mockAdvice.ExpectAndThrow("AfterReturning", new FormatException(), new object[] { null, null, null, null});

			IDynamicMock mockInvocation = new DynamicMock(typeof (IMethodInvocation));
			IMethodInvocation invocation = (IMethodInvocation) mockInvocation.Object;
			mockInvocation.ExpectAndReturn("Proceed", null);

			try
			{
				AfterReturningAdviceInterceptor interceptor = new AfterReturningAdviceInterceptor(afterAdvice);
				interceptor.Invoke(invocation);
				Assert.Fail("Must have thrown a FormatException by this point.");
			}
			catch (FormatException)
			{
			}

			mockAdvice.Verify();
			mockInvocation.Verify();
		}
	}
}