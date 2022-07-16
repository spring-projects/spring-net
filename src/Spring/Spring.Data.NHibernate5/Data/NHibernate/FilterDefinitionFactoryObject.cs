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

using System.Collections;
using NHibernate.Engine;
using NHibernate.Type;

using Spring.Objects.Factory;

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// Convenient FactoryObject for defining Hibernate FilterDefinitions.
    /// Exposes a corresponding Hibernate FilterDefinition object.
    /// </summary>
    /// <remarks>
    /// 
    /// <p>
    /// Typically defined as an inner object within a LocalSessionFactoryObject
    /// definition, as the list element for the "filterDefinitions" object property.
    /// For example:
    /// </p>
    /// 
    /// <pre>
    /// &lt;objectn id="sessionFactory" type="Spring.Data.NHibernate.LocalSessionFactoryObject, Spring.Data.NHibernate"&gt;
    ///   ...
    ///   &lt;property name="FilterDefinitions"&gt;
    ///    &lt;list&gt;
    ///       &lt;object type="Spring.Data.NHibernate.FilterDefinitionFactoryObject, Spring.Data.NHibernate"&gt;
    ///         &lt;property name="FilterName" value="myFilter"/&gt;
    ///         &lt;property name="ParameterTypes"&gt;
    ///           &lt;props&gt;
    ///             &lt;prop key="MyParam"&gt;string&lt;/prop&gt;
    ///             &lt;prop key="MyOtherParam"&gt;long&lt;/prop&gt;
    ///           &lt;/props&gt;
    ///         &lt;/property&gt;
    ///       &lt;/object&gt;
    ///     &lt;/list&gt;
    ///   &lt;/property&gt;
    ///   ...
    /// &lt;/object&gt;
    /// </pre>
    /// <p>
    /// Alternatively, specify an object id (or name) attribute for the inner object,
    /// instead of the "FilterName" property.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Marko Lahma (.NET)</author>
    /// <see cref="FilterDefinition" />
    /// <see cref="LocalSessionFactoryObject.FilterDefinitions" />
    /// <version>$Id: FilterDefiniitionFactoryObject.cs,v 1.1 2008/04/07 20:12:53 lahma Exp $</version>
    public class FilterDefinitionFactoryObject : IFactoryObject, IObjectNameAware, IInitializingObject
    {
        private string filterName;

        private IDictionary<string, IType> parameterTypeMap = new Dictionary<string, IType>();

        private string defaultFilterCondition;

        private FilterDefinition filterDefinition;

        /// <summary>
        /// Set the name of the filter.
        /// </summary>
        public string FilterName
        {
            set { this.filterName = value; }
        }

        /// <summary>
        /// Set the parameter types for the filter,
        /// with parameter names as keys and type names as values.
        /// <see cref="TypeFactory.HeuristicType(string)" />
        /// </summary>
        public IDictionary ParameterTypes
        {
            set
            {
                if (value != null)
                {
                    this.parameterTypeMap = new Dictionary<string, IType>(value.Count);
                    foreach (DictionaryEntry entry in value)
                    {
                        string paramName = (string) entry.Key;
                        string typeName = (string) entry.Value;
                        this.parameterTypeMap.Add(paramName, TypeFactory.HeuristicType(typeName));
                    }
                }
                else
                {
                    this.parameterTypeMap = new Dictionary<string, IType>();
                }
            }
        }

        /// <summary>
        /// Specify a default filter condition for the filter, if any.
        /// </summary>
        public string DefaultFilterCondition
        {
            set { this.defaultFilterCondition = value; }
        }

        /// <summary>
        /// If no explicit filter name has been specified, the object name of
        /// the FilterDefinitionFactoryObject will be used.
        /// <see cref="FilterName" />
        /// </summary>
        public string ObjectName
        {
            set
            {
                if (this.filterName == null)
                {
                    this.filterName = value;
                }
            }
        }

        /// <summary>
        /// Initializes the filter definitions.
        /// </summary>
        public void AfterPropertiesSet()
        {
            this.filterDefinition = new FilterDefinition(this.filterName, this.defaultFilterCondition, this.parameterTypeMap, true);
        }

        /// <summary>
        /// Returns the singleton filter definition.
        /// </summary>
        /// <returns></returns>
        public object GetObject()
        {
            return this.filterDefinition;
        }

        /// <summary>
        /// Returns the type of the object this factory produces.
        /// </summary>
        public Type ObjectType
        {
            get { return typeof(FilterDefinition); }
        }

        /// <summary>
        /// Returns whether this factory produces singletons, always true.
        /// </summary>
        public bool IsSingleton
        {
            get { return true; }
        }

    }
}
