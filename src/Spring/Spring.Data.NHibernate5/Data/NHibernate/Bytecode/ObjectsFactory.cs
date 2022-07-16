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

using NHibernate.Bytecode;

using Spring.Objects.Factory;

namespace Spring.Data.NHibernate.Bytecode
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Fabio Maulo</author>
    public class ObjectsFactory : IObjectsFactory
    {
        private readonly IListableObjectFactory listableObjectFactory;

        ///<summary>
        ///</summary>
        ///<param name="listableObjectFactory"></param>
        public ObjectsFactory(IListableObjectFactory listableObjectFactory)
		{
			this.listableObjectFactory = listableObjectFactory;
		}

        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <returns>A reference to the created object.</returns>
        public object CreateInstance(Type type)
		{
			var namesForType = listableObjectFactory.GetObjectNamesForType(type);
			return namesForType.Count > 0 ? listableObjectFactory.GetObject(namesForType[0], type) : Activator.CreateInstance(type);
		}

        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <param name="type">The type of object to create.</param><param name="nonPublic">true if a public or nonpublic default constructor can match; false if only a public default constructor can match.</param>
        /// <returns>A reference to the created object </returns>
        public object CreateInstance(Type type, bool nonPublic)
		{
			var namesForType = listableObjectFactory.GetObjectNamesForType(type);
			return namesForType.Count > 0 ? listableObjectFactory.GetObject(namesForType[0], type) : Activator.CreateInstance(type);
		}

        /// <summary>
        /// Creates an instance of the specified type using the constructor that best matches the specified parameters.
        /// </summary>
        /// <param name="type">The type of object to create.</param><param name="ctorArgs">An array of constructor arguments.</param>
        /// <returns>A reference to the created object.</returns>
        public object CreateInstance(Type type, params object[] ctorArgs)
		{
			return Activator.CreateInstance(type, ctorArgs);
		}

    }
}
