#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using System.Collections;
#if NET_2_0
using System.Collections.Generic;
#endif
using System.Collections.Specialized;
using System.Globalization;

using Spring.Core;
using Spring.Core.TypeConversion;
using Spring.Core.TypeResolution;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Tag subclass used to hold a dictionary of managed elements.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class ManagedDictionary : Hashtable, IManagedCollection
	{
        private string keyTypeName;
        private string valueTypeName;

        /// <summary>
        /// Gets or sets the unresolved name for the <see cref="System.Type"/> 
        /// of the keys of this managed dictionary.
        /// </summary>
        /// <value>The unresolved name for the type of the keys of this managed dictionary.</value>
        public string KeyTypeName
        {
            get { return this.keyTypeName; }
            set { this.keyTypeName = value; }
        }

        /// <summary>
        /// Gets or sets the unresolved name for the <see cref="System.Type"/> 
        /// of the values of this managed dictionary.
        /// </summary>
        /// <value>The unresolved name for the type of the values of this managed dictionary.</value>
        public string ValueTypeName
        {
            get { return this.valueTypeName; }
            set { this.valueTypeName = value; }
        }
	    
		/// <summary>
		/// Resolves this managed collection at runtime.
		/// </summary>
		/// <param name="objectName">
		/// The name of the top level object that is having the value of one of it's
		/// collection properties resolved.
		/// </param>
		/// <param name="definition">
		/// The definition of the named top level object.
		/// </param>
		/// <param name="propertyName">
		/// The name of the property the value of which is being resolved.
		/// </param>
		/// <param name="resolver">
		/// The callback that will actually do the donkey work of resolving
		/// this managed collection.
		/// </param>
		/// <returns>A fully resolved collection.</returns>
		public ICollection Resolve(
			string objectName, RootObjectDefinition definition,
			string propertyName, ManagedCollectionElementResolver resolver)
		{
            IDictionary dictionary;

            Type keyType = null;
            if (StringUtils.HasText(this.keyTypeName))
            {
                keyType = TypeResolutionUtils.ResolveType(this.keyTypeName);
            }

            Type valueType = null;
            if (StringUtils.HasText(this.valueTypeName))
            {
                valueType = TypeResolutionUtils.ResolveType(this.valueTypeName);
            }
#if NET_2_0
            if ((keyType == null) && (valueType == null))
            {
                dictionary = new HybridDictionary();
            }
            else
            {
                Type type = typeof(Dictionary<,>);
                Type[] genericArgs = new Type[2] { 
                    (keyType == null) ? typeof(object) : keyType,
                    (valueType == null) ? typeof(object) : valueType };
                type = type.MakeGenericType(genericArgs);

                dictionary = (IDictionary)ObjectUtils.InstantiateType(type);
            }
#else
            dictionary = new HybridDictionary();
#endif
            foreach (object key in this.Keys)
			{
				string elementName = string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", propertyName, key);
				object resolvedKey = resolver(objectName, definition, elementName, key);
				object resolvedValue = resolver(objectName, definition, elementName, this[key]);

                if (keyType != null)
                {
                    try
                    {
                        resolvedKey = TypeConversionUtils.ConvertValueIfNecessary(keyType, resolvedKey, propertyName);
                    }
                    catch (TypeMismatchException)
                    {
                        throw new TypeMismatchException(
                            String.Format(
                                    "Unable to convert managed dictionary key '{0}' from [{1}] into [{2}] during initialization"
                                    + " of property '{3}' for object '{4}'. Do you have an appropriate type converter registered?", 
                                    resolvedKey, resolvedKey.GetType(), keyType, propertyName, objectName));
                    }
                }

                if (valueType != null)
                {
                    try
                    {
                        resolvedValue = TypeConversionUtils.ConvertValueIfNecessary(valueType, resolvedValue, propertyName + "[" + resolvedKey + "]");
                    }
                    catch (TypeMismatchException)
                    {
                        throw new TypeMismatchException(
                           String.Format(
                                   "Unable to convert managed dictionary value '{0}' from [{1}] into [{2}] during initialization"
                                   + " of property '{3}' for object '{4}'. Do you have an appropriate type converter registered?", 
                                   resolvedValue, resolvedValue.GetType(), valueType, propertyName, objectName));
                    }
                }

                dictionary.Add(resolvedKey, resolvedValue);
			}

            return dictionary;
		}
	}
}