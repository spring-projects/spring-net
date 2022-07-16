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

using AopAlliance.Intercept;

using Spring.Caching;
using Spring.Util;
using System.Collections.Concurrent;

#endregion

namespace Spring.Aspects.Cache
{
    /// <summary>
    /// Implementation of a result caching advice.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This advice can be used to cache the return value of the method.
    /// </p>
    /// <p>
    /// Parameters that determine where, how and for how long the return value
    /// will be cached are retrieved from the <see cref="CacheResultAttribute"/> and/or
    /// <see cref="CacheResultItemsAttribute"/> that are defined on the pointcut.
    /// </p>
    /// </remarks>
    /// <seealso cref="CacheResultAttribute"/>
    /// <seealso cref="CacheResultItemsAttribute"/>
    /// <author>Aleksandar Seovic</author>
    public class CacheResultAdvice : BaseCacheAdvice, IMethodInterceptor
    {
        #region CacheResultAttribute & CacheResultItemsAttribute caching

        private class CacheResultInfo
        {
            public readonly CacheResultAttribute ResultInfo;
            public readonly CacheResultItemsAttribute[] ItemInfoArray;

            public CacheResultInfo(CacheResultAttribute resultInfo, CacheResultItemsAttribute[] itemInfoArray)
            {
                ResultInfo = resultInfo;
                ItemInfoArray = itemInfoArray;
            }
        }

        private readonly ConcurrentDictionary<MethodInfo, CacheResultInfo> _cacheResultAttributeCache = new();

        private CacheResultInfo GetCacheResultInfo(MethodInfo method)
        {
            var cacheResultInfo = _cacheResultAttributeCache.GetOrAdd(method, _ =>
            {
                var resultInfo = (CacheResultAttribute) GetCustomAttribute(method, typeof(CacheResultAttribute));
                var itemInfoArray = (CacheResultItemsAttribute[]) GetCustomAttributes(method, typeof(CacheResultItemsAttribute));

                return new CacheResultInfo(resultInfo, itemInfoArray);
            });

            return cacheResultInfo;
        }

        #endregion

        /// <summary>
        /// Inner class to help cache null values.
        /// </summary>
        [Serializable] public sealed class NullValueMarkerType
        {
            /// <returns>true when other object is of same type.</returns>
            public override bool Equals(object obj)
            {
                return obj is NullValueMarkerType;
            }

            /// <returns>13</returns>
            public override int GetHashCode()
            {
                return 13;
            }
        }

        // NullValue
        private static readonly object NullValue = new NullValueMarkerType();

        /// <summary>
        /// Applies caching around a method invocation.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method tries to retrieve an object from the cache, using the supplied
        /// <paramref name="invocation"/> to generate a cache key. If an object is found
        /// in the cache, the cached value is returned and the method call does not
        /// proceed any further down the invocation chain.
        /// </p>
        /// <p>
        /// If object does not exist in the cache, the advised method is called (using
        /// <see cref="AopAlliance.Intercept.IJoinpoint.Proceed()"/>)
        /// and any return value is cached for the next method invocation.
        /// </p>
        /// </remarks>
        /// <param name="invocation">
        /// The method invocation that is being intercepted.
        /// </param>
        /// <returns>
        /// A cached object or the result of the
        /// <see cref="AopAlliance.Intercept.IJoinpoint.Proceed()"/> call.
        /// </returns>
        /// <exception cref="System.Exception">
        /// If any of the interceptors in the chain or the target object itself
        /// throws an exception.
        /// </exception>
        public object Invoke(IMethodInvocation invocation)
        {
            CacheResultInfo cacheResultInfo = GetCacheResultInfo(invocation.Method);

            // prepare variables for SpEL expressions
            IDictionary<string, object> vars = PrepareVariables(invocation.Method, invocation.Arguments);

            bool cacheHit = false;
            object returnValue = GetReturnValue(invocation, cacheResultInfo.ResultInfo, vars, out cacheHit);

            if (!cacheHit && cacheResultInfo.ItemInfoArray.Length > 0 && returnValue is IEnumerable)
            {
                CacheResultItems((IEnumerable)returnValue, cacheResultInfo.ItemInfoArray, vars);
            }

            return returnValue;
        }

        /// <summary>
        /// Obtains return value either from cache or by invoking target method
        /// and caches it if necessary.
        /// </summary>
        /// <param name="invocation">
        /// The method invocation that is being intercepted.
        /// </param>
        /// <param name="resultInfo">
        /// Attribute specifying where and how to cache return value. Can be <c>null</c>,
        /// in which case no caching of the result as a whole will be performed
        /// (if the result is collection, individual items could still be cached separately).
        /// </param>
        /// <param name="vars">
        /// Variables for expression evaluation.
        /// </param>
        /// <param name="cacheHit">
        /// Returns <c>true</c> if the return value was found in cache, <c>false</c> otherwise.
        /// </param>
        /// <returns>
        /// Return value for the specified <paramref name="invocation"/>.
        /// </returns>
        private object GetReturnValue(IMethodInvocation invocation, CacheResultAttribute resultInfo, IDictionary<string, object> vars, out bool cacheHit)
        {
            if (resultInfo != null)
            {
                #region Instrumentation
                bool isLogDebugEnabled = logger.IsDebugEnabled;
                #endregion

                AssertUtils.ArgumentNotNull(resultInfo.KeyExpression, "Key",
                    "The cache attribute is missing the key definition.");

                object returnValue = null;
                object resultKey = resultInfo.KeyExpression.GetValue(null, vars);

                ICache cache = GetCache(resultInfo.CacheName);
                returnValue = cache.Get(resultKey);
                cacheHit = (returnValue != null);

                if (NullValue.Equals(returnValue))
                {
                    returnValue = null;
                }

                Type returnType = invocation.Method.ReturnType;
                if (returnValue != null && !returnType.IsInstanceOfType(returnValue))
                {
                    #region Instrumentation
                    if (isLogDebugEnabled)
                    {
                        logger.Debug(String.Format("Object for key [{0}] was of type [{1}] which is not compatible with return type [{2}]. Proceeding...", resultKey, returnValue.GetType(), returnType));
                    }
                    #endregion

                    cacheHit = false;
                    returnValue = null;
                }

                if (!cacheHit)
                {
                    #region Instrumentation
                    if (isLogDebugEnabled)
                    {
                        logger.Debug(String.Format("Object for key [{0}] was not found in cache [{1}]. Proceeding...", resultKey, resultInfo.CacheName));
                    }
                    #endregion

                    returnValue = invocation.Proceed();
                    if (EvalCondition(resultInfo.Condition, resultInfo.ConditionExpression, returnValue, vars))
                    {
                        #region Instrumentation
                        if (isLogDebugEnabled)
                        {
                            logger.Debug(String.Format("Caching object for key [{0}] into cache [{1}].", resultKey, resultInfo.CacheName));
                        }
                        #endregion
                        cache.Insert(resultKey, (returnValue == null) ? NullValue : returnValue, resultInfo.TimeToLiveTimeSpan);
                    }
                }
                else
                {
                    #region Instrumentation
                    if (isLogDebugEnabled)
                    {
                        logger.Debug(String.Format("Object for key [{0}] found in cache [{1}]. Aborting invocation...", resultKey, resultInfo.CacheName));
                    }
                    #endregion
                }

                return returnValue;
            }

            cacheHit = false;
            return invocation.Proceed();
        }

        /// <summary>
        /// Caches each item from the collection returned by target method.
        /// </summary>
        /// <param name="items">
        /// A collection of items to cache.
        /// </param>
        /// <param name="itemInfoArray">
        /// Attributes specifying where and how to cache each item from the collection.
        /// </param>
        /// <param name="vars">
        /// Variables for expression evaluation.
        /// </param>
        private void CacheResultItems(IEnumerable items, CacheResultItemsAttribute[] itemInfoArray, IDictionary<string, object> vars)
        {
            foreach (CacheResultItemsAttribute itemInfo in itemInfoArray)
            {
                AssertUtils.ArgumentNotNull(itemInfo.KeyExpression, "Key",
                    "The cache attribute is missing the key definition.");

                ICache cache = GetCache(itemInfo.CacheName);

                #region Instrumentation
                bool isDebugEnabled = logger.IsDebugEnabled;
                #endregion
                foreach (object item in items)
                {
                    if (EvalCondition(itemInfo.Condition, itemInfo.ConditionExpression, item, vars))
                    {
                        object itemKey = itemInfo.KeyExpression.GetValue(item, vars);
                        #region Instrumentation
                        if (isDebugEnabled)
                        {
                            logger.Debug("Caching collection item for key [" + itemKey + "].");
                        }
                        #endregion
                        cache.Insert(itemKey, (item == null ? NullValue : item), itemInfo.TimeToLiveTimeSpan);
                    }
                }
            }
        }
    }
}
