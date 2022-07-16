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

using AopAlliance.Intercept;

using Spring.Aop.Framework.Adapter;

#endregion

namespace Spring.Aop.Framework
{
	/// <summary>
	/// Utility methods for use by
	/// <see cref="Spring.Aop.Framework.IAdvisorChainFactory"/> implementations.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Not intended to be used directly by applications.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	public sealed class AdvisorChainFactoryUtils
	{
		/// <summary>
		/// Gets the list of
		/// <see langword="static"/> interceptors and dynamic interception
		/// advice that may apply to the supplied <paramref name="method"/>
		/// invocation.
		/// </summary>
		/// <param name="config">The proxy configuration.</param>
		/// <param name="proxy">The object proxy.</param>
		/// <param name="method">
		/// The method to evaluate interceptors for.
		/// </param>
		/// <param name="targetType">
		/// The <see cref="System.Type"/> of the target object.
		/// </param>
		/// <returns>
		/// A <see cref="System.Collections.IList"/> of
		/// <see cref="AopAlliance.Intercept.IMethodInterceptor"/> (if there's
		/// a dynamic method matcher that needs evaluation at runtime).
		/// </returns>
		public static IList<object> CalculateInterceptors(
			IAdvised config, object proxy, MethodInfo method, Type targetType)
		{
            IList<object> interceptors = new List<object>(config.Advisors.Count);
			foreach (IAdvisor advisor in config.Advisors)
			{
				if (advisor is IPointcutAdvisor)
				{
					IPointcutAdvisor pointcutAdvisor = (IPointcutAdvisor) advisor;
					if (pointcutAdvisor.Pointcut.TypeFilter.Matches(targetType))
					{
						IMethodInterceptor interceptor =
							(IMethodInterceptor) GlobalAdvisorAdapterRegistry.Instance.GetInterceptor(advisor);
						IMethodMatcher mm = pointcutAdvisor.Pointcut.MethodMatcher;
						if (mm.Matches(method, targetType))
						{
							if (mm.IsRuntime)
							{
								// Creating a new object instance in the GetInterceptor() method
								// isn't a problem as we normally cache created chains...
								interceptors.Add(new InterceptorAndDynamicMethodMatcher(interceptor, mm));
							}
							else
							{
								interceptors.Add(interceptor);
							}
						}
					}
				}
			}
			return interceptors;
		}

		#region Constructor (s) / Destructor

		// CLOVER:OFF

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Framework.AdvisorChainFactoryUtils"/>
		/// class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a utility class, and as such has no publicly visible
		/// constructors.
		/// </p>
		/// </remarks>
		private AdvisorChainFactoryUtils()
		{
		}

		// CLOVER:ON

		#endregion
	}
}
