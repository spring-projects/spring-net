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

#endregion

namespace Spring.Aop
{
	/// <summary>
	/// Advice executed before a method is invoked.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Such advice cannot prevent the method call proceeding, short of
	/// throwing an <see cref="System.Exception"/>.
	/// </p>
	/// <p>
	/// The main advantage of <c>before</c> advice is that there is no
	/// possibility of inadvertently failing to proceed down the interceptor
	/// chain, since there is no need (and indeed means) to invoke the next
	/// interceptor in the call chain.
	/// </p>
	/// <p>
	/// Possible uses for this type of advice would include performing class
	/// invariant checks prior to the actual method invocation, the ubiquitous
	/// logging of method invocations (useful during development), etc.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	/// <seealso cref="Spring.Aop.IBeforeAdvice"/>
	/// <seealso cref="Spring.Aop.IAfterReturningAdvice"/>
	/// <seealso cref="Spring.Aop.IThrowsAdvice"/>
	/// <seealso cref="AopAlliance.Intercept.IMethodInterceptor"/>
	public interface IMethodBeforeAdvice : IBeforeAdvice
	{
		/// <summary>
		/// The callback before a given method is invoked.
		/// </summary>
		/// <param name="method">The method being invoked.</param>
		/// <param name="args">The arguments to the method.</param>
		/// <param name="target">
		/// The target of the method invocation. May be <see langword="null"/>.
		/// </param>
		/// <exception cref="System.Exception">
		/// Thrown when and if this object wishes to abort the call. Any
		/// exception so thrown will be propagated to the caller.
		/// </exception>
		void Before(MethodInfo method, object[] args, object target);
	}
}