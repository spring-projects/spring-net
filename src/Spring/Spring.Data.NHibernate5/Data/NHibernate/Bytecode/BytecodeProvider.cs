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
using NHibernate.Properties;
using NHibernate.Type;
using Spring.Objects.Factory;

namespace Spring.Data.NHibernate.Bytecode
{
    /// <summary>
    /// The Spring for .NET-backed ByteCodeprovider for NHibernate
    /// </summary>
    /// <author>Fabio Maulo</author>
    public class BytecodeProvider : IBytecodeProvider
    {
		private readonly IListableObjectFactory listableObjectFactory;
		private readonly IObjectsFactory objectsFactory;
		private readonly DefaultCollectionTypeFactory collectionTypefactory;
        private readonly IProxyFactoryFactory proxyFactoryFactory;

        ///<summary>
        /// Creates a new bytecode Provider instance using the specified object factory
        ///</summary>
        ///<param name="listableObjectFactory"></param>
        public BytecodeProvider(IListableObjectFactory listableObjectFactory)
		{
			this.listableObjectFactory = listableObjectFactory;
			this.objectsFactory = new ObjectsFactory(listableObjectFactory);
			this.collectionTypefactory = new DefaultCollectionTypeFactory();
            this.proxyFactoryFactory = new ProxyFactoryFactory();
		}

        /// <summary>
        /// Retrieve the <see cref="T:NHibernate.Bytecode.IReflectionOptimizer"/> delegate for this provider
        ///             capable of generating reflection optimization components.
        /// </summary>
        /// <param name="clazz">The class to be reflected upon.</param><param name="getters">All property getters to be accessed via reflection.</param><param name="setters">All property setters to be accessed via reflection.</param>
        /// <returns>The reflection optimization delegate.</returns>
        public IReflectionOptimizer GetReflectionOptimizer(Type clazz, IGetter[] getters, ISetter[] setters)
		{
            return new ReflectionOptimizer(listableObjectFactory, clazz, getters, setters);
		}

        /// <summary>
        /// The specific factory for this provider capable of 
        /// generating run-time proxies for lazy-loading purposes.
        /// </summary>
        public IProxyFactoryFactory ProxyFactoryFactory
		{
			get { return this.proxyFactoryFactory; }
		}

        /// <summary>
        /// NHibernate's object instaciator.
        /// </summary>
        /// <remarks>
        /// For entities <see cref="T:NHibernate.Bytecode.IReflectionOptimizer"/> and its implementations.
        /// </remarks>
        public IObjectsFactory ObjectsFactory
		{
			get { return this.objectsFactory; }
		}

        /// <summary>
        /// Instanciator of NHibernate's collections default types.
        /// </summary>
        public ICollectionTypeFactory CollectionTypeFactory
		{
			get { return this.collectionTypefactory; }
		}

    }
}
