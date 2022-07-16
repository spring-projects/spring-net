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
using Spring.Util;

#endregion

namespace Spring.Aspects.Cache
{
    /// <summary>
    /// Implementation of a parameter caching advice.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This advice can be used to cache the parameter of the method.
    /// </p>
    /// <p>
    /// Information that determines where, how and for how long the return value
    /// will be cached are retrieved from the <see cref="CacheParameterAttribute"/>s
    /// that are defined on the pointcut.
    /// </p>
    /// <p>
    /// Parameter values are cached *after* the target method is invoked in order to
    /// capture any parameter state changes it might make (for example, it is common
    /// to set an object identifier within the save method for the persistent entity).
    /// </p>
    /// </remarks>
    /// <seealso cref="CacheParameterAttribute"/>
    /// <author>Aleksandar Seovic</author>
    public class CacheParameterAdvice : BaseCacheAdvice, IAfterReturningAdvice
    {
        #region CacheParameterAttribute caching

        private class CacheParameterInfo
        {
            public readonly ParameterInfo[] Parameters;
            public readonly CacheParameterAttribute[][] CacheParameterAttributes;

            public CacheParameterInfo(ParameterInfo[] parameters, CacheParameterAttribute[][] cacheParameterAttributes)
            {
                Parameters = parameters;
                CacheParameterAttributes = cacheParameterAttributes;
            }
        }

        private readonly Hashtable _cacheParameterInfoCache = new Hashtable();

        private CacheParameterInfo GetCacheParameterInfo(MethodInfo method)
        {
            CacheParameterInfo cpi = (CacheParameterInfo)_cacheParameterInfoCache[method];
            if (cpi == null)
            {
                ParameterInfo[] parameters = method.GetParameters();
                CacheParameterAttribute[][] parameterInfos = new CacheParameterAttribute[parameters.Length][];
                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo p = parameters[i];
                    CacheParameterAttribute[] paramInfoArray = (CacheParameterAttribute[])GetCustomAttributes(p, typeof(CacheParameterAttribute));
                    parameterInfos[i] = paramInfoArray;
                }
                cpi = new CacheParameterInfo(parameters, parameterInfos);
                _cacheParameterInfoCache[method] = cpi;
            }
            return cpi;
        }

        #endregion

        /// <summary>
        /// Executes after target <paramref name="method"/>
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

            CacheParameterInfo cpi = GetCacheParameterInfo(method);
            CacheParameterAttribute[][] cacheParameterAttributes = cpi.CacheParameterAttributes;

            if (cacheParameterAttributes.Length > 0)
            {
                IDictionary<string, object> vars = PrepareVariables(method, arguments);
                for (int i = 0; i < cacheParameterAttributes.Length; i++)
                {
                    foreach (CacheParameterAttribute paramInfo in cacheParameterAttributes[i])
                    {
                        AssertUtils.ArgumentNotNull(paramInfo.KeyExpression, "Key",
                            "The cache attribute is missing the key definition.");

                        if (EvalCondition(paramInfo.Condition, paramInfo.ConditionExpression, arguments[i], vars))
                        {
                            ICache cache = GetCache(paramInfo.CacheName);

                            object key = paramInfo.KeyExpression.GetValue(arguments[i], vars);

                            #region Instrumentation
                            if (isLogDebugEnabled)
                            {
                                logger.Debug(string.Format("Caching parameter for key [{0}] into cache [{1}].", key, paramInfo.CacheName));
                            }
                            #endregion

                            cache.Insert(key, arguments[i], paramInfo.TimeToLiveTimeSpan);
                        }
                    }
                }
            }
        }
    }
}
