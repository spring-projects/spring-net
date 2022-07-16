#region License

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

#endregion

using System;
using AopAlliance.Intercept;

namespace Spring.Aop.Advice
{
	/// <summary>
	/// Convenience <see cref="AopAlliance.Intercept.IMethodInterceptor"/>
	/// implementation that displays verbose information about intercepted
	/// invocations to the system console.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Can be introduced into an interceptor chain to serve as a useful low
	/// level debugging aid.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Federico Spinazzi (.NET)</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	/// <seealso cref="System.Console"/>
	public sealed class DebugAdvice : IMethodInterceptor
	{
		private int _count;

		/// <summary>
		/// Gets the count of the number of times this interceptor has been
		/// invoked.
		/// </summary>
		/// <returns>
		/// The count of the number of times this interceptor has been invoked.
		/// </returns>
		public int Count
		{
			get { return _count; }
		}

		/// <summary>
		/// Displays verbose information about intercepted invocations to the
		/// system console.
		/// </summary>
		/// <param name="invocation">
		/// The method invocation that is being intercepted.
		/// </param>
		/// <returns>
		/// The result of the call to the
		/// <see cref="AopAlliance.Intercept.IJoinpoint.Proceed"/> method of
		/// the supplied <paramref name="invocation"/>; this return value may
		/// well have been intercepted by the interceptor.
		/// </returns>
		/// <exception cref="System.Exception">
		/// If any of the interceptors in the chain or the target object itself
		/// throws an exception.
		/// </exception>
		/// <seealso cref="System.Console"/>
		/// <seealso cref="AopAlliance.Intercept.IMethodInterceptor.Invoke(IMethodInvocation)"/>
		public object Invoke(IMethodInvocation invocation)
		{
			++_count;
			Console.Out.WriteLine("{0} [count={1}, invocation='{2}']",
				typeof(DebugAdvice).Name, _count, invocation);
			object returnValue = invocation.Proceed();
			Console.Out.WriteLine("{0} ['{1}' invocation returned '{2}']",
				typeof(DebugAdvice).Name, invocation.Method.Name, returnValue);
			return returnValue;
		}
	}
}
