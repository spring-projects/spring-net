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

using Common.Logging;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Proxy;

namespace Spring.Data.NHibernate.Bytecode
{
    /// <summary>
    /// A Spring for .NET backed <see cref="IProxyFactory"/> implementation for creating
    /// NHibernate proxies.
    /// </summary>
    /// <seealso cref="ProxyFactoryFactory"/>
    /// <author>Erich Eichinger</author>
    public class ProxyFactory : AbstractProxyFactory
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProxyFactory));

        [Serializable]
        private class SerializableProxyFactory : global::Spring.Aop.Framework.ProxyFactory
        {
            // ensure proxy types are generated as Serializable
            public override bool IsSerializable
            {
                get { return true; }
            }
        }

        /// <summary>
        /// Creates a new proxy.
        /// </summary>
        /// <param name="id">The id value for the proxy to be generated.</param>
        /// <param name="session">The session to which the generated proxy will be associated.</param>
        /// <returns>The generated proxy.</returns>
        /// <exception cref="T:NHibernate.HibernateException">Indicates problems generating requested proxy.</exception>
        public override INHibernateProxy GetProxy(object id, ISessionImplementor session)
        {
            try
            {
                // PersistentClass = PersistentClass.IsInterface ? typeof(object) : PersistentClass
                LazyInitializer initializer = new LazyInitializer(EntityName, PersistentClass,
                                                      id, GetIdentifierMethod, SetIdentifierMethod, ComponentIdType, session);

                SerializableProxyFactory proxyFactory = new SerializableProxyFactory();
                proxyFactory.Interfaces = Interfaces;
                proxyFactory.TargetSource = initializer;
                proxyFactory.ProxyTargetType = IsClassProxy;
                proxyFactory.AddAdvice(initializer);

                object proxyInstance = proxyFactory.GetProxy();
                return (INHibernateProxy)proxyInstance;
            }
            catch (Exception ex)
            {
                log.Error("Creating a proxy instance failed", ex);
                throw new HibernateException("Creating a proxy instance failed", ex);
            }
        }
    }
}
