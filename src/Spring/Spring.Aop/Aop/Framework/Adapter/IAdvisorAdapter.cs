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
using AopAlliance.Intercept;

#endregion

namespace Spring.Aop.Framework.Adapter
{
	/// <summary>
	/// Permits the handling of new advisors and advice types as extensions to
	/// the Spring AOP framework.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Implementors can create AOP Alliance
	/// <see cref="AopAlliance.Intercept.IInterceptor"/>s from custom advice
	/// types, enabling these advice types to be used in the Spring.NET AOP
	/// framework, which uses interception under the covers.
	/// </p>
	/// <p>
	/// There is no need for most Spring.NET users to implement this interface;
	/// do so only if you need to introduce more
	/// <see cref="Spring.Aop.IAdvisor"/> or <see cref="AopAlliance.Aop.IAdvice"/>
	/// types to Spring.NET.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	public interface IAdvisorAdapter
	{
	    /// <summary>
	    /// Does this adapter understand the supplied <paramref name="advice"/>?
	    /// </summary>
	    /// <remarks>
	    /// <p>
	    /// Is it valid to invoke the
	    /// <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapterRegistry.Wrap"/>
	    /// method with the given advice as an argument?
	    /// </p>
	    /// </remarks>
	    /// <param name="advice">
	    /// <see cref="AopAlliance.Aop.IAdvice"/> such as
	    /// <see cref="Spring.Aop.IBeforeAdvice"/>.
	    /// </param>
	    /// <returns><see langword="true"/> if this adapter understands the
	    /// supplied <paramref name="advice"/>.
	    /// </returns>
	    bool SupportsAdvice(IAdvice advice);

	    /// <summary>
	    /// Return an AOP Alliance
	    /// <see cref="AopAlliance.Intercept.IInterceptor"/> exposing the
	    /// behaviour of the given advice to an interception-based AOP
	    /// framework.
	    /// </summary>
	    /// <remarks>
	    /// <p>
	    /// Don't worry about any <see cref="Spring.Aop.IPointcut"/>
	    /// contained in the supplied <see cref="Spring.Aop.IAdvisor"/>;
	    /// the AOP framework will take care of checking the pointcut.
	    /// </p>
	    /// </remarks>
	    /// <param name="advisor">
	    /// The advice. The
	    /// <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter.SupportsAdvice"/>
	    /// method must have previously returned <see langword="true"/> on the
	    /// supplied <paramref name="advisor"/>.
	    /// </param>
	    /// <returns>
	    /// An AOP Alliance
	    /// <see cref="AopAlliance.Intercept.IInterceptor"/> exposing the
	    /// behaviour of the given advice to an interception-based AOP
	    /// framework.
	    /// </returns>
	    IInterceptor GetInterceptor(IAdvisor advisor);
	}
}