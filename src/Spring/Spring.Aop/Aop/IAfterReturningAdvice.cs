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

using System.Reflection;

using AopAlliance.Aop;

#endregion

namespace Spring.Aop
{
	/// <summary>
	/// Advice that executes after a method returns <b>successfully</b>.
	/// </summary>
	/// <remarks>
	/// <p>
	/// <i>After</i> returning advice is invoked only on a normal method
	/// return, but <b>not</b> if an exception is thrown. Such advice can see
	/// the return value of the advised method invocation, but cannot change it.
	/// </p>
	/// <p>
	/// Possible uses for this type of advice would include performing access
	/// control checks on the return value of an advised method invocation, the
	/// ubiquitous logging of method invocation return values (useful during
	/// development), etc.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	/// <seealso cref="Spring.Aop.IMethodBeforeAdvice"/>
	/// <seealso cref="Spring.Aop.IThrowsAdvice"/>
	/// <seealso cref="AopAlliance.Intercept.IMethodInterceptor"/>
	public interface IAfterReturningAdvice : IAdvice
	{
		/// <summary>
		/// Executes after <paramref name="target"/> <paramref name="method"/>
		/// returns <b>successfully</b>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Note that the supplied <paramref name="returnValue"/> <b>cannot</b>
		/// be changed by this type of advice... use the around advice type
		/// (<see cref="AopAlliance.Intercept.IMethodInterceptor"/>) if you
		/// need to change the return value of an advised method invocation.
		/// The data encapsulated by the supplied <paramref name="returnValue"/>
		/// can of course be modified though.
		/// </p>
		/// </remarks>
		/// <param name="returnValue">
		/// The value returned by the <paramref name="target"/>.
		/// </param>
		/// <param name="method">The intecepted method.</param>
		/// <param name="args">The intercepted method's arguments.</param>
		/// <param name="target">The target object.</param>
		/// <seealso cref="AopAlliance.Intercept.IMethodInterceptor.Invoke"/>
		void AfterReturning(object returnValue, MethodInfo method, object[] args, object target);
	}
}