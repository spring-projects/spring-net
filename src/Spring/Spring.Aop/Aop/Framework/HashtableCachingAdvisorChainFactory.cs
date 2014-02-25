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
using System.Collections.Generic;
using System.Reflection;

#if NET_4_0
using System.Collections.Concurrent;
#else
using System.Runtime.Serialization;
using System.Threading;
#endif

#endregion

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
#if !NET_4_0
        private readonly IDictionary<MethodInfo, IList<object>> methodCache = new Dictionary<MethodInfo, IList<object>>();

        // ReaderWriterLockSlim is not serializable. Cannot set value using field initializer as it won't 
        // run on deserialization. Instead c'tor and OnDeserialized will take care of creating the lock instance.
        [NonSerialized]
        private ReaderWriterLockSlim cacheLock;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext c)
        {
            CreateCacheLock();
        }

        private void CreateCacheLock()
        {
            cacheLock = new ReaderWriterLockSlim();
        }
#else
        private readonly ConcurrentDictionary<MethodInfo, IList<object>> methodCache = new ConcurrentDictionary<MethodInfo, IList<object>>();
#endif

        /// <summary>
        /// Default c'tor
        /// </summary>
        public HashtableCachingAdvisorChainFactory()
        {
#if !NET_4_0
            CreateCacheLock();
#endif
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
#if !NET_4_0
            IList<object> cached;
            cacheLock.EnterReadLock();
            try {
                if (this.methodCache.TryGetValue(method, out cached)) 
                {
                    return cached;
                }
            } 
            finally
            {
                cacheLock.ExitReadLock();
            }
            // Apparently not in the cache - calculate the value outside of any locks then enter upgradeable read lock and check again
            IList<object> calculated = AdvisorChainFactoryUtils.CalculateInterceptors(advised, proxy, method, targetType);
            cacheLock.EnterUpgradeableReadLock();
            try 
            {
                if (!this.methodCache.TryGetValue(method, out cached)) 
                {
                    // Still not in the cache - enter write lock and add the pre-calculated value
                    cacheLock.EnterWriteLock();
                    try 
                    {
                        cached = calculated;
                        this.methodCache[method] = cached;
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                }
            } 
            finally 
            {
                cacheLock.ExitUpgradeableReadLock();
            }
            return cached;
#else
            return methodCache.GetOrAdd(method, m => AdvisorChainFactoryUtils.CalculateInterceptors(advised, proxy, m, targetType));
#endif
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
#if !NET_4_0
            cacheLock.EnterWriteLock();
            try
            {
#endif
            methodCache.Clear();
#if !NET_4_0
            } 
            finally
            {
                cacheLock.ExitWriteLock();
            }
#endif
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