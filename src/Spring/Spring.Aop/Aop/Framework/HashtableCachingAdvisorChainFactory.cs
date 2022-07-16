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

using System.Reflection;
using System.Runtime.Serialization;

namespace Spring.Aop.Framework
{
    /// <summary>
    /// <see cref="Spring.Aop.Framework.IAdvisorChainFactory"/> implementation
    /// that caches advisor chains on a per-advised-method basis.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    [Serializable]
    public sealed class HashtableCachingAdvisorChainFactory : IAdvisorChainFactory
    {
        [NonSerialized]
        private Dictionary<MethodInfo, IList<object>> methodCache = new Dictionary<MethodInfo, IList<object>>();

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            methodCache = new Dictionary<MethodInfo, IList<object>>();
        }

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
        public IList<object> GetInterceptors(IAdvised advised, object proxy, MethodInfo method, Type targetType)
        {
            if (!methodCache.TryGetValue(method, out var interceptors))
            {
                lock (methodCache)
                {
                    if (!methodCache.TryGetValue(method, out interceptors))
                    {
                        interceptors = AdvisorChainFactoryUtils.CalculateInterceptors(advised, proxy, method, targetType);
                        methodCache[method] = interceptors;
                    }
                }
            }
            return interceptors;
        }

        /// <summary>
        /// Invoked when the first proxy is created.
        /// </summary>
        /// <param name="source">
        /// The relevant <see cref="Spring.Aop.Framework.AdvisedSupport"/> source.
        /// </param>
        public void Activated(AdvisedSupport source)
        {
        }

        /// <summary>
        /// Invoked when advice is changed after a proxy is created.
        /// </summary>
        /// <param name="source">
        /// The relevant <see cref="Spring.Aop.Framework.AdvisedSupport"/> source.
        /// </param>
        public void AdviceChanged(AdvisedSupport source)
        {
            methodCache.Clear();
        }

        /// <summary>
        /// Invoked when interfaces are changed after a proxy is created.
        /// </summary>
        /// <param name="source">
        /// The relevant <see cref="Spring.Aop.Framework.AdvisedSupport"/> source.
        /// </param>
        public void InterfacesChanged(AdvisedSupport source)
        {
        }
    }
}
