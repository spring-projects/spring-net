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

using Spring.Collections;
using Spring.Core;
using Spring.Core.TypeConversion;
using Spring.Core.TypeResolution;
using Spring.Util;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Tag subclass used to hold a set of managed elements.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class ManagedSet : HybridSet, IManagedCollection, IMergable
    {
        private string elementTypeName;

        private bool mergeEnabled;


        /// <summary>
        /// Creates a new set instance based on either a list or a hash table,
        /// depending on which will be more efficient based on the data-set
        /// size.
        /// </summary>
        public ManagedSet()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HybridSet"/> class with a given capacity
        /// </summary>
        /// <param name="size">The size.</param>
        public ManagedSet(int size) : base(size)
        {
        }

        /// <summary>
        /// Gets or sets the unresolved name for the <see cref="System.Type"/>
        /// of the elements of this managed set.
        /// </summary>
        /// <value>The unresolved name for the type of the elements of this managed set.</value>
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
            ISet set = new HybridSet();

            Type elementType = null;
            if (StringUtils.HasText(this.elementTypeName))
            {
                elementType = TypeResolutionUtils.ResolveType(this.elementTypeName);
            }

            string elementName = propertyName + "[(set-element)]";
            foreach (object element in this)
            {
                object resolvedElement = resolver(objectName, definition, elementName, element);

                if (elementType != null)
                {
                    try
                    {
                        resolvedElement = TypeConversionUtils.ConvertValueIfNecessary(elementType, resolvedElement, propertyName);
                    }
                    catch (TypeMismatchException)
                    {
                        throw new TypeMismatchException(
                            String.Format(
                                    "Unable to convert managed set element '{0}' from [{1}] into [{2}] during initialization"
                                    + " of property '{3}' for object '{4}'. Do you have an appropriate type converter registered?",
                                    resolvedElement, resolvedElement.GetType(), elementType, propertyName, objectName));
                    }
                }

                set.Add(resolvedElement);
            }

            return set;
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
            ISet pSet = parent as ISet;
            if (pSet == null)
            {
                throw new InvalidOperationException("Cannot merge with object of type [" + parent.GetType() + "]");
            }
            ISet merged = new ManagedSet();
            foreach (object element in pSet)
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
