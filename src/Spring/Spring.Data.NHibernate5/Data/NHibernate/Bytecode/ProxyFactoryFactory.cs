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

using NHibernate.Bytecode;
using NHibernate.Proxy;

namespace Spring.Data.NHibernate.Bytecode
{
    /// <summary>
    /// Creates a Spring for .NET backed <see cref="IProxyFactory"/> instance.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class ProxyFactoryFactory : IProxyFactoryFactory
    {
        /// <summary>
        /// Build a proxy factory specifically for handling runtime lazy loading. 
        /// </summary>
        /// <returns>The lazy-load proxy factory.</returns>
        public IProxyFactory BuildProxyFactory()
        {
            return new ProxyFactory();
        }

        /// <summary>
        /// </summary>
        public bool IsInstrumented(Type entityClass)
        {
            return false;
        }

        /// <summary>
        /// </summary>
        public bool IsProxy(object entity)
        {
            return (entity is INHibernateProxy);
        }

        ///<summary>
        ///</summary>
        public IProxyValidator ProxyValidator
        {
            get { return new DynProxyTypeValidator(); }
        }
    }
}
