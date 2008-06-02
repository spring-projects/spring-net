#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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
using Common.Logging;
using Spring.Caching;
using Spring.Expressions;
using Spring.Util;

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
        // shared logger instance
        private static readonly ILog logger = LogManager.GetLogger(typeof (CacheResultAdvice));

		// NullValue
		private static readonly object NullValue = new object();

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
            CacheResultAttribute resultInfo =
                (CacheResultAttribute) GetCustomAttribute(invocation.Method, typeof (CacheResultAttribute));
            CacheResultItemsAttribute[] itemInfoArray =
                (CacheResultItemsAttribute[])invocation.Method.GetCustomAttributes(typeof(CacheResultItemsAttribute), false);

            bool cacheHit = false;
            object returnValue = GetReturnValue(invocation, resultInfo, out cacheHit);

            if (!cacheHit && itemInfoArray.Length > 0 && returnValue is ICollection)
            {
                CacheResultItems((ICollection)returnValue, itemInfoArray);
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
        /// <param name="cacheHit">
        /// Returns <c>true</c> if the return value was found in cache, <c>false</c> otherwise.
        /// </param>
        /// <returns>
        /// Return value for the specified <paramref name="invocation"/>.
        /// </returns>
        private object GetReturnValue(IMethodInvocation invocation, CacheResultAttribute resultInfo, out bool cacheHit)
        {
            if (resultInfo != null)
            {
                object returnValue = null;
                bool isLogDebugEnabled = logger.IsDebugEnabled;

                IDictionary vars = PrepareVariables(invocation.Method, invocation.Arguments);

                object resultKey = resultInfo.KeyExpression.GetValue(null, vars);
                ICache cache = GetCache(resultInfo.CacheName);
                AssertUtils.ArgumentNotNull(cache, "CacheName",
                                            "Result cache with the specified name [" + resultInfo.CacheName +
                                            "] does not exist.");

                returnValue = cache.Get(resultKey);
                cacheHit = (returnValue != null);
                if (!cacheHit)
                {
                    #region Instrumentation

                    if (isLogDebugEnabled)
                    {
                        logger.Debug("Object for key [" + resultKey + "] was not found in cache. Proceeding...");
                    }

                    #endregion

                    returnValue = invocation.Proceed();
                    if (EvalCondition(resultInfo.Condition, resultInfo.ConditionExpression, returnValue, vars))
                    {
                        #region Instrumentation

                        if (isLogDebugEnabled)
                        {
                            logger.Debug("Caching object for key [" + resultKey + "].");
                        }

                        #endregion
                        cache.Insert(resultKey, (returnValue==null)?NullValue:returnValue, resultInfo.TimeToLiveTimeSpan);
                    }
                }
                else
                {
                    #region Instrumentation

                    if (isLogDebugEnabled)
                    {
                        logger.Debug("Cache hit for [" + resultKey + "]. Aborting invocation...");
                    }

                    #endregion
                }

                return (returnValue==NullValue)?null:returnValue;
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
        private void CacheResultItems(ICollection items, CacheResultItemsAttribute[] itemInfoArray)
        {
            foreach (CacheResultItemsAttribute itemInfo in itemInfoArray)
            {
                ICache cache = GetCache(itemInfo.CacheName);
                AssertUtils.ArgumentNotNull(cache, "CacheName",
                                            "Result item cache with the specified name [" + itemInfo.CacheName +
                                            "] does not exist.");

                bool isDebugEnabled = logger.IsDebugEnabled;
                foreach (object item in items)
                {
                    if (EvalCondition(itemInfo.Condition, itemInfo.ConditionExpression, item, null))
                    {
                        object itemKey = itemInfo.KeyExpression.GetValue(item);
                        #region Instrumentation

                        if (isDebugEnabled)
                        {
                            logger.Debug("Caching collection item for key [" + itemKey + "].");
                        }

                        #endregion
                        cache.Insert(itemKey, (item==null?NullValue:item), itemInfo.TimeToLiveTimeSpan);
                    }
                }
            }
        }
    }
}