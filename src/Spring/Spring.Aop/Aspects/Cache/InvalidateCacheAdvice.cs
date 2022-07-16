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

using System.Collections;
using System.Reflection;

using Spring.Aop;
using Spring.Caching;

#endregion

namespace Spring.Aspects.Cache
{
    /// <summary>
    /// Implementation of a cache invalidation advice.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This advice can be used to evict items from the cache.
    /// </p>
    /// <p>
    /// Information that determines which items should be evicted and from which cache
    /// are retrieved from the <see cref="InvalidateCacheAttribute"/>s  that are defined
    /// on the pointcut.
    /// </p>
    /// <p>
    /// Items are evicted *after* target method is invoked. Return value of the method,
    /// as well as method arguments, can be used to determine a list of keys for the items
    /// that should be evicted (return value will be passed as a context for
    /// <see cref="InvalidateCacheAttribute.Keys"/> expression evaluation, and method
    /// arguments will be passed as variables, keyed by argument name).
    /// </p>
    /// </remarks>
    /// <seealso cref="InvalidateCacheAttribute"/>
    /// <author>Aleksandar Seovic</author>
    public class InvalidateCacheAdvice : BaseCacheAdvice, IAfterReturningAdvice
    {
        #region InvalidateCacheAttribute caching

        private readonly Hashtable _invalidateCacheAttributeCache = new Hashtable();

        private InvalidateCacheAttribute[] GetInvalidateCacheInfo(MethodInfo method)
        {
            InvalidateCacheAttribute[] cacheInfoArray = (InvalidateCacheAttribute[])_invalidateCacheAttributeCache[method];
            if (cacheInfoArray == null)
            {
                cacheInfoArray = (InvalidateCacheAttribute[])GetCustomAttributes(method, typeof(InvalidateCacheAttribute));
                _invalidateCacheAttributeCache[method] = cacheInfoArray;
            }
            return cacheInfoArray;
        }

        #endregion

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
        /// <param name="arguments">The intercepted method's arguments.</param>
        /// <param name="target">The target object.</param>
        /// <seealso cref="AopAlliance.Intercept.IMethodInterceptor.Invoke"/>
        public void AfterReturning(object returnValue, MethodInfo method, object[] arguments, object target)
        {
            #region Instrumentation
            bool isLogDebugEnabled = logger.IsDebugEnabled;
            #endregion

            InvalidateCacheAttribute[] cacheInfoArray = GetInvalidateCacheInfo(method);

            if (cacheInfoArray.Length > 0)
            {
                IDictionary<string, object> vars = PrepareVariables(method, arguments);
                foreach (InvalidateCacheAttribute cacheInfo in cacheInfoArray)
                {
                    if (EvalCondition(cacheInfo.Condition, cacheInfo.ConditionExpression, returnValue, vars))
                    {
                        ICache cache = GetCache(cacheInfo.CacheName);

                        if (cacheInfo.KeysExpression != null)
                        {
                            object keys = cacheInfo.KeysExpression.GetValue(returnValue, vars);
                            if (keys is ICollection)
                            {
                                #region Instrumentation
                                if (isLogDebugEnabled)
                                {
                                    logger.Debug(string.Format("Removing objects for keys [{0}] from cache [{1}].", keys, cacheInfo.CacheName));
                                }
                                #endregion
                                cache.RemoveAll((ICollection) keys);
                            }
                            else
                            {
                                #region Instrumentation
                                if (isLogDebugEnabled)
                                {
                                    logger.Debug(string.Format("Removing object for key [{0}] from cache [{1}].", keys, cacheInfo.CacheName));
                                }
                                #endregion
                                cache.Remove(keys);
                            }
                        }
                        else
                        {
                            #region Instrumentation
                            if (isLogDebugEnabled)
                            {
                                logger.Debug(string.Format("Invalidate cache [{0}].", cacheInfo.CacheName));
                            }
                            #endregion
                            cache.Clear();
                        }
                    }
                }
            }
        }
    }
}
