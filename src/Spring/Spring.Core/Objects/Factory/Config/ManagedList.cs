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
using System.Globalization;

using Spring.Core;
using Spring.Core.TypeConversion;
using Spring.Core.TypeResolution;
using Spring.Util;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Tag subclass used to hold a list of managed elements.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class ManagedList : ArrayList, IManagedCollection, IMergable
    {
        private string elementTypeName;
        private bool mergeEnabled;

        /// <summary>
        /// Initializes a new instance of the ManagedList class that is empty and has the default initial capacity.
        /// </summary>
        public ManagedList()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ManagedList class that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store. </param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="capacity"/> is less than zero. </exception>
        public ManagedList(int capacity)
            : base(capacity)
        {
        }


        /// <summary>
        /// Gets or sets the unresolved name for the <see cref="System.Type"/>
        /// of the elements of this managed list.
        /// </summary>
        /// <value>The unresolved name for the type of the elements of this managed list.</value>
        public string ElementTypeName
        {
            get { return this.elementTypeName; }
            set { this.elementTypeName = value; }
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
        public ICollection Resolve(string objectName, IObjectDefinition definition, string propertyName, ManagedCollectionElementResolver resolver)
        {
            IList list;

            Type elementType = null;
            if (StringUtils.HasText(this.elementTypeName))
            {
                elementType = TypeResolutionUtils.ResolveType(this.elementTypeName);
            }

            if (elementType == null)
            {
                list = new ArrayList();
            }
            else
            {
                // CLOVER:ON
                Type type = typeof(List<>);
                Type[] genericArgs = new Type[1] { elementType };
                type = type.MakeGenericType(genericArgs);

                list = (IList)ObjectUtils.InstantiateType(type);
                // CLOVER:OFF
            }

            for (int i = 0; i < Count; ++i)
            {
                object element = this[i];
                object resolvedElement =
                        resolver(objectName, definition, String.Format(CultureInfo.InvariantCulture, "{0}[{1}]", propertyName, i), element);

                if (elementType != null)
                {
                    try
                    {
                        resolvedElement = TypeConversionUtils.ConvertValueIfNecessary(elementType, resolvedElement, propertyName + "[" + i + "]");
                    }
                    catch (TypeMismatchException)
                    {
                        throw new TypeMismatchException(
                            String.Format(
                                    "Unable to convert managed list element '{0}' from [{1}] into [{2}] during initialization"
                                    + " of property '{3}' for object '{4}'. Do you have an appropriate type converter registered?",
                                    resolvedElement, resolvedElement.GetType(), elementType, propertyName, objectName));
                    }
                }

                list.Add(resolvedElement);
            }

            return list;
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
            IList plist = parent as IList;
            if (plist == null)
            {
                throw new InvalidOperationException("Cannot merge with object of type [" + parent.GetType() + "]");
            }
            IList merged = new ManagedList();
            foreach (object element in plist)
            {
                merged.Add(element);
            }
            foreach (object o in this)
            {
                merged.Add(o);
            }
            return merged;
        }
    }
}
