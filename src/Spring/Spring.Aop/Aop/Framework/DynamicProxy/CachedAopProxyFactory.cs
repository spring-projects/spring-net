#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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
using System.Text;
using System.Collections;

using Common.Logging;
using Spring.Proxy;

#endregion

namespace Spring.Aop.Framework.DynamicProxy
{
    /// <summary>
    /// Implementation of the <see cref="Spring.Aop.Framework.IAopProxyFactory"/> 
    /// interface that caches the AOP proxy <see cref="System.Type"/> instance.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Caches against a key based on :
    /// - the base type
    /// - the target type
    /// - the interfaces to proxy
    /// </p>
    /// </remarks>
    /// <author>Bruno Baia</author>
    /// <author>Erich Eichinger</author>
    /// <seealso cref="Spring.Aop.Framework.DynamicProxy.DefaultAopProxyFactory"/>
    /// <seealso cref="Spring.Aop.Framework.IAopProxyFactory"/>
    [Serializable]
    public class CachedAopProxyFactory : DefaultAopProxyFactory
    {
        /// <summary>
        /// The shared <see cref="Common.Logging.ILog"/> instance for this class.
        /// </summary>
        private static readonly ILog logger = LogManager.GetLogger(typeof(CachedAopProxyFactory));

        private static readonly Hashtable typeCache = new Hashtable();

        /// <summary>
        /// Returns the number of proxy types in the cache
        /// </summary>
        public static int CountCachedTypes
        {
            get { return typeCache.Count; }
        }

        /// <summary>
        /// Clears the type cache
        /// </summary>
        public static void ClearCache()
        {
            typeCache.Clear();
        }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public CachedAopProxyFactory()
        {}

        /// <summary>
        /// Generates the proxy type and caches the <see cref="System.Type"/>
        /// instance against the base type and the interfaces to proxy.
        /// </summary>
        /// <param name="typeBuilder">
        /// The <see cref="Spring.Proxy.IProxyTypeBuilder"/> to use
        /// </param>
        /// <returns>The generated or cached proxy class.</returns>
        protected override Type BuildProxyType(IProxyTypeBuilder typeBuilder)
        {
            ProxyTypeCacheKey cacheKey = new ProxyTypeCacheKey(
                typeBuilder.BaseType, typeBuilder.TargetType, typeBuilder.Interfaces);
            Type proxyType = null;
            lock (typeCache)
            {
                proxyType = typeCache[cacheKey] as Type;
                if (proxyType == null)
                {
                    proxyType = typeBuilder.BuildProxyType();
                    typeCache[cacheKey] = proxyType;
                }
                else
                {
                    #region Instrumentation

                    if (logger.IsInfoEnabled)
                    {
                        logger.Info(String.Format(
                                        "AOP proxy type found in cache for '{0}'.", cacheKey));
                    }

                    #endregion
                }
            }
            return proxyType;
        }

        #region ProxyTypeCacheKey inner class implementation

        /// <summary>
        /// Uniquely identifies a proxytype in the cache
        /// </summary>
        private sealed class ProxyTypeCacheKey
        {
            private sealed class HashCodeComparer : IComparer
            {
                public int Compare(object x, object y)
                {
                    return x.GetHashCode().CompareTo(y.GetHashCode());
                }
            }

            private static IComparer interfaceComparer = new HashCodeComparer();

            private Type baseType;
            private Type targetType;
            private Type[] interfaceTypes;

            public ProxyTypeCacheKey(Type baseType, Type targetType, Type[] interfaceTypes)
            {
                this.baseType = baseType;
                this.targetType = targetType;
                Array.Sort(interfaceTypes, interfaceComparer); // sort by GetHashcode()? to have a defined order
                this.interfaceTypes = interfaceTypes;
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }
                ProxyTypeCacheKey proxyTypeCacheKey = obj as ProxyTypeCacheKey;
                if (proxyTypeCacheKey == null)
                {
                    return false;
                }
                if (!Equals(targetType, proxyTypeCacheKey.targetType))
                {
                    return false;
                }
                if (!Equals(baseType, proxyTypeCacheKey.baseType))
                {
                    return false;
                }
                for (int i = 0; i < interfaceTypes.Length; i++)
                {
                    if (!Equals(interfaceTypes[i], proxyTypeCacheKey.interfaceTypes[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                int result = baseType.GetHashCode();
                result = 29*result + targetType.GetHashCode();
                for (int i = 0; i < interfaceTypes.Length; i++)
                {
                    result = 29 * result + interfaceTypes[i].GetHashCode();
                }
                return result;
            }

            public override string ToString()
            {
                StringBuilder buffer = new StringBuilder();
                buffer.Append("baseType=" + baseType + "; ");
                buffer.Append("targetType=" + targetType + "; ");
                buffer.Append("interfaceTypes=[");
                foreach (Type intf in interfaceTypes)
                {
                    buffer.Append(intf + ";");
                }
                buffer.Append("]; ");
                return buffer.ToString();
            }
        }

        #endregion
    }
}