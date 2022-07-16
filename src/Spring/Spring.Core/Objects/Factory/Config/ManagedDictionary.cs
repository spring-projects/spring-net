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

using System.Collections;
using System.Collections.Specialized;
using System.Globalization;

using Spring.Core;
using Spring.Core.TypeConversion;
using Spring.Core.TypeResolution;
using Spring.Util;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Tag subclass used to hold a dictionary of managed elements.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class ManagedDictionary : Hashtable, IManagedCollection, IMergable
	{
        private string keyTypeName;
        private string valueTypeName;
	    private bool mergeEnabled;

	    /// <summary>
	    /// Initializes a new, empty instance of the <see cref="T:System.Collections.Hashtable"/> class using the default initial capacity, load factor, hash code provider, and comparer.
	    /// </summary>
	    public ManagedDictionary()
	    {
	    }

	    /// <summary>
	    /// Initializes a new, empty instance of the <see cref="T:System.Collections.Hashtable"/> class using the specified initial capacity, and the default load factor, hash code provider, and comparer.
	    /// </summary>
	    /// <param name="capacity">The approximate number of elements that the <see cref="T:System.Collections.Hashtable"/> object can initially contain. </param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="capacity"/> is less than zero. </exception>
	    public ManagedDictionary(int capacity) : base(capacity)
	    {
	    }

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
			string objectName, IObjectDefinition definition,
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

	    /// <summary>
	    /// Gets a value indicating whether this instance is merge enabled for this instance
	    /// </summary>
	    /// <value>
	    /// 	<c>true</c> if this instance is merge enabled; otherwise, <c>false</c>.
	    /// </value>
	    public bool MergeEnabled
	    {
            get { return this.mergeEnabled; }
            set { this.mergeEnabled = value; }
	    }

	    /// <summary>
	    /// Merges the current value set with that of the supplied object.
	    /// </summary>
	    /// <remarks>The supplied object is considered the parent, and values in the
	    /// callee's value set must override those of the supplied object.
	    /// </remarks>
	    /// <param name="parent">The parent object to merge with</param>
	    /// <returns>The result of the merge operation</returns>
	    /// <exception cref="ArgumentNullException">If the supplied parent is <code>null</code></exception>
	    /// <exception cref="InvalidOperationException">If merging is not enabled for this instance,
	    /// (i.e. <code>MergeEnabled</code> equals <code>false</code>.</exception>
	    public object Merge(object parent)
	    {
	    	if (!this.mergeEnabled)
            	{
            		throw new InvalidOperationException(
                    		"Not allowed to merge when the 'MergeEnabled' property is set to 'false'");
            	}
            	if (parent == null)
            	{
                	return this;
            	}
            	var pDict = parent as IDictionary;
            	if (pDict == null)
            	{
            		throw new InvalidOperationException("Cannot merge with object of type [" + parent.GetType() + "]");
            	}
            	var merged = new ManagedDictionary();
            	var pManagedDict = pDict as ManagedDictionary;
            	if (pManagedDict != null)
            	{
            		merged.KeyTypeName = pManagedDict.keyTypeName;
            		merged.valueTypeName = pManagedDict.valueTypeName;
            	}
	    	foreach (DictionaryEntry dictionaryEntry in pDict)
	    	{
	            merged[dictionaryEntry.Key] = dictionaryEntry.Value;
	        }
	        foreach (DictionaryEntry entry in this)
	        {
	            merged[entry.Key] = entry.Value;
	        }
            	return merged;
	    }
	}
}
