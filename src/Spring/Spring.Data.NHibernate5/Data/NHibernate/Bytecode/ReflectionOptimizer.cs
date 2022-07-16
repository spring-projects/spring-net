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

using NHibernate.Properties;

using Spring.Objects.Factory;

namespace Spring.Data.NHibernate.Bytecode
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Fabio Maulo</author>
    public class ReflectionOptimizer : global::NHibernate.Bytecode.Lightweight.ReflectionOptimizer
    {
        private readonly IListableObjectFactory listableObjectFactory;

        ///<summary>
        ///</summary>
        ///<param name="listableObjectFactory"></param>
        ///<param name="mappedType"></param>
        ///<param name="getters"></param>
        ///<param name="setters"></param>
        public ReflectionOptimizer(IListableObjectFactory listableObjectFactory, Type mappedType, IGetter[] getters,
                                   ISetter[] setters)
            : base(mappedType, getters, setters)
        {
            this.listableObjectFactory = listableObjectFactory;
        }

        /// <summary>
        /// Perform instantiation of an instance of the underlying class.
        /// </summary>
        /// <returns>The new instance.</returns>
        public override object CreateInstance()
        {
            var namesForType = listableObjectFactory.GetObjectNamesForType(mappedType);
            if (namesForType.Count > 0)
            {
                return listableObjectFactory.GetObject(namesForType[0], mappedType);
            }
            else
            {
                return base.CreateInstance();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        protected override void ThrowExceptionForNoDefaultCtor(Type type)
        {
        }
    }
}
