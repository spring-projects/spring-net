#region License

/*
 * Copyright � 2002-2010 the original author or authors.
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

using System.Collections;
using Spring.Core.TypeResolution;

namespace Spring.Messaging.Ems.Support.Converter
{
    /// <summary>
    /// Provides a layer of indirection when adding the 'type' of the object as a message property.
    /// </summary>
    public class TypeMapper : ITypeMapper
    {
        private string defaultNamespace;
        private string defaultAssemblyName;

        //Generics not used to support both 1.1 and 2.0
        private IDictionary idTypeMapping;
        private IDictionary typeIdMapping;

        private string defaultHashtableTypeId = "Hashtable";

        private Type defaultHashtableClass = typeof(Hashtable);

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMapper"/>
        /// </summary>
        public TypeMapper()
        {
            idTypeMapping = new Hashtable();
            typeIdMapping = new Hashtable();
        }


        /// <summary>
        /// Gets or sets the id type mapping.
        /// </summary>
        /// <value>The id type mapping.</value>
        public IDictionary IdTypeMapping
        {
            get { return idTypeMapping; }
            set { idTypeMapping = value; }
        }

        /// <summary>
        /// Gets the name of the field in the message that has type information..
        /// </summary>
        /// <value>The name of the type id field.</value>
        public string TypeIdFieldName
        {
            get { return "__TypeId__"; }
        }


        /// <summary>
        /// Sets the default hashtable class.
        /// </summary>
        /// <value>The default hashtable class.</value>
        public Type DefaultHashtableClass
        {
            set { defaultHashtableClass = value; }
        }

        /// <summary>
        /// Convert from a type to a string.
        /// </summary>
        /// <param name="typeOfObjectToConvert">The type of object to convert.</param>
        /// <returns></returns>
        public string FromType(Type typeOfObjectToConvert)
        {

            if (typeIdMapping.Contains(typeOfObjectToConvert))
            {
                return typeIdMapping[typeOfObjectToConvert] as string;
            }
            else
            {
                if ( typeof (IDictionary).IsAssignableFrom(typeOfObjectToConvert))
                {
                    return defaultHashtableTypeId;
                }
                return typeOfObjectToConvert.Name;
            }
        }

        /// <summary>
        /// Convert from a string to a type
        /// </summary>
        /// <param name="typeId">The type id.</param>
        /// <returns></returns>
        public Type ToType(string typeId)
        {
            if (idTypeMapping.Contains(typeId))
            {
                return idTypeMapping[typeId] as Type;
            }
            else
            {
                if (typeId.Equals(defaultHashtableTypeId))
                {
                    return defaultHashtableClass;
                }
                string fullyQualifiedTypeName = defaultNamespace + "." +
                                 typeId + ", " +
                                 DefaultAssemblyName;
                return TypeResolutionUtils.ResolveType(fullyQualifiedTypeName);
            }

        }

        /// <summary>
        /// Gets or sets the default namespace.
        /// </summary>
        /// <value>The default namespace.</value>
        public string DefaultNamespace
        {
            get
            {
                return defaultNamespace;
            }
            set
            {
                defaultNamespace = value;
            }

        }

        /// <summary>
        /// Gets or sets the default name of the assembly.
        /// </summary>
        /// <value>The default name of the assembly.</value>
        public string DefaultAssemblyName
        {
            get
            {
                return defaultAssemblyName;
            }
            set
            {
                defaultAssemblyName = value;
            }
        }

        /// <summary>
        /// Afters the properties set.
        /// </summary>
        public void AfterPropertiesSet()
        {
            ValidateIdTypeMapping();
            if (DefaultAssemblyName != null && DefaultNamespace == null)
            {
                throw new ArgumentException("Default Namespace required when DefaultAssemblyName is set.");
            }
            if (DefaultNamespace != null && DefaultAssemblyName == null)
            {
                throw new ArgumentException("Default Assembly Name required when DefaultNamespace is set.");
            }
        }


        private void ValidateIdTypeMapping()
        {
            IDictionary finalIdTypeMapping = new Hashtable();
            foreach (DictionaryEntry entry in idTypeMapping)
            {
                string id = entry.Key.ToString();
                Type t = entry.Value as Type;
                if (t == null)
                {
                    //convert from string value.
                    string typeName = entry.Value.ToString();
                    t = TypeResolutionUtils.ResolveType(typeName);
                }
                finalIdTypeMapping.Add(id,t);
                typeIdMapping.Add(t,id);

            }
            idTypeMapping = finalIdTypeMapping;
        }
    }
}
