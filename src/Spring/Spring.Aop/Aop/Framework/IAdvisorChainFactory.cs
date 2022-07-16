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

#region Imports

using System.Reflection;

#endregion

namespace Spring.Aop.Framework
{
	/// <summary>
	/// Factory interface for advisor chains.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	public interface IAdvisorChainFactory : IAdvisedSupportListener
	{
	    /// <summary>
	    /// Gets the list of <see cref="AopAlliance.Intercept.IInterceptor"/> and
	    /// <see cref="Spring.Aop.Framework.InterceptorAndDynamicMethodMatcher"/>
	    /// instances for the supplied <paramref name="proxy"/>.
	    /// </summary>
	    /// <param name="advised">The proxy configuration object.</param>
	    /// <param name="proxy">The object proxy.</param>
	    /// <param name="method">
	    /// The method for which the interceptors are to be evaluated.
	    /// </param>
	    /// <param name="targetType">
	    /// The <see cref="System.Type"/> of the target object.
	    /// </param>
	    /// <returns>
	    /// The list of <see cref="AopAlliance.Intercept.IInterceptor"/> and
	    /// <see cref="Spring.Aop.Framework.InterceptorAndDynamicMethodMatcher"/>
	    /// instances for the supplied <paramref name="proxy"/>.
	    /// </returns>
	    IList<object> GetInterceptors(IAdvised advised, object proxy, MethodInfo method, Type targetType);
	}
}
