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

using AopAlliance.Aop;

#endregion

namespace Spring.Aop
{
	/// <summary>
	/// Base interface holding AOP advice and a filter determining the
	/// applicability of the advice (such as a pointcut).
	/// </summary>
	/// <remarks>
	/// <note>
	/// This interface is not for use by Spring.NET users, but exists rather to
	/// allow for commonality in the support for different types of advice
	/// within the framework.
	/// </note>
	/// <p>
	/// Spring.NET AOP is centered on <b>around advice</b> delivered via method
	/// <b>interception</b>, compliant with the AOP Alliance interception API. 
	/// The <see cref="Spring.Aop.IAdvisor"/> interface allows support for
	/// different types of advice, such as <b>before</b> and <b>after</b>
	/// advice, which need not be implemented using interception.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	/// <seealso cref="Spring.Aop.IMethodBeforeAdvice"/>
	/// <seealso cref="Spring.Aop.IAfterReturningAdvice"/>
	/// <seealso cref="Spring.Aop.IThrowsAdvice"/>
	/// <seealso cref="AopAlliance.Intercept.IMethodInterceptor"/>
	public interface IAdvisor
	{
		/// <summary>
		/// Is this advice associated with a particular instance?
		/// </summary>
		/// <remarks>
		/// <p>
		/// An advisor that was creating a mixin would be a per instance
		/// operation and would thus return <see langword="true"/>. If the
		/// advisor is not per instance, it is shared with all instances of the
		/// advised class obtained from the same Spring.NET IoC container.
		/// </p>
		/// <p>
		/// Use <c>singleton</c> and <c>prototype</c> object definitions or
		/// appropriate programmatic proxy creation to ensure that
		/// <see cref="Spring.Aop.IAdvisor"/>s have the correct lifecycle model. 
		/// </p>
		/// <note>
		/// This method is not currently used by the framework.
		/// </note>
		/// </remarks>
		/// <value>
		/// <see langword="true"/> if this advice is associated with a
		/// particular instance.
		/// </value>
		bool IsPerInstance { get; }

		/// <summary>
		/// Return the advice part of this aspect.
		/// </summary>
		/// <remarks>
		/// <p>
		/// An advice may be an interceptor, a throws advice, before advice,
		/// introduction etc.
		/// </p>
		/// </remarks>
		/// <returns>
		/// The advice that should apply if the pointcut matches.
		/// </returns>
		IAdvice Advice { get; }
	}
}