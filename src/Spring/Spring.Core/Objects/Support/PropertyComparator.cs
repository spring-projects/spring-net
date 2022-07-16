#region License

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

#endregion

using System.Collections;
using System.Globalization;
using Common.Logging;
using Spring.Core;
using Spring.Util;

namespace Spring.Objects.Support
{
    /// <summary>
    /// Performs a comparison of two objects, using the specified object property via
    /// an <see cref="Spring.Objects.IObjectWrapper"/>.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Jean-Pierre Pawlak</author>
    /// <author>Simon White (.NET)</author>
    public class PropertyComparator : IComparer
    {
        private static readonly ILog logger
            = LogManager.GetLogger(typeof (PropertyComparator));

        private ISortDefinition sortDefinition;
        private readonly IDictionary cachedObjectWrappers = new Hashtable();

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Support.PropertyComparator"/> class.
        /// </summary>
        /// <param name="definition">
        /// The <see cref="Spring.Objects.Support.ISortDefinition"/> to use for any
        /// sorting.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="definition"/> is <cref lang="null"/>.
        /// </exception>
        public PropertyComparator(ISortDefinition definition)
        {
            AssertUtils.ArgumentNotNull(definition, "definition");
            this.sortDefinition = definition;
        }

        /// <summary>
        ///	Gets the <see cref="Spring.Objects.Support.ISortDefinition"/> to
        ///	use for any sorting.
        /// </summary>
        /// <value>
        /// The <see cref="Spring.Objects.Support.ISortDefinition"/> to	use for
        /// any sorting.
        /// </value>
        public ISortDefinition SortDefinition
        {
            get { return this.sortDefinition; }
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less
        /// than, equal to or greater than the other.
        /// </summary>
        /// <param name="o1">The first object to compare.</param>
        /// <param name="o2">The second object to compare.</param>
        /// <returns><see cref="System.Collections.IComparer.Compare"/></returns>
        public virtual int Compare(object o1, object o2)
        {
            object v1 = GetPropertyValue(o1);
            object v2 = GetPropertyValue(o2);
            if (this.sortDefinition.IgnoreCase
                && (v1 is string)
                && (v2 is string))
            {
                v1 = ((string) v1).ToLower(CultureInfo.CurrentCulture);
                v2 = ((string) v2).ToLower(CultureInfo.CurrentCulture);
            }
            int result = 0;
            try
            {
                if (v1 != null)
                {
                    if (v2 != null)
                    {
                        result = ((IComparable) v1).CompareTo(v2);
                    }
                    else
                    {
                        result = -1;
                    }
                }
                else
                {
                    if (v2 != null)
                    {
                        result = 1;
                    }
                    else
                    {
                        result = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                #region Instrumentation

                if (logger.IsWarnEnabled)
                {
                    logger.Warn("Could not sort objects [" + o1 + "] and [" + o2 + "]",
                                ex);
                }

                #endregion

                return 0;
            }
            return (this.sortDefinition.Ascending ? result : -result);
        }

        /// <summary>
        /// Get the <see cref="Spring.Objects.Support.ISortDefinition"/>'s property
        /// value for the given object.
        /// </summary>
        /// <param name="obj">The object to get the property value for.</param>
        /// <returns>The property value.</returns>
        private object GetPropertyValue(object obj)
        {
            object propertyValue = null;
            if (obj != null)
            {
                IObjectWrapper ow = (IObjectWrapper) this.cachedObjectWrappers[obj];
                if (ow == null)
                {
                    ow = new ObjectWrapper(obj);
                    this.cachedObjectWrappers.Add(obj, ow);
                }
                try
                {
                    propertyValue = ow.GetPropertyValue(this.sortDefinition.Property);
                }
                catch (InvalidPropertyException)
                {
                    // the property doesn't exist in the first place, so let exception through...
                    throw;
                }
                catch (ObjectsException ex)
                {
                    // if a nested property cannot be read, simply return null...
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug("Could not access property - treating as null for sorting.", ex);
                    }
                }
            }
            return propertyValue;
        }

        /// <summary>
        /// Sort the given <see cref="System.Collections.IList"/> according to the
        /// given sort definition.
        /// </summary>
        /// <param name="source">
        /// The <see cref="System.Collections.IList"/> to be sorted.
        /// </param>
        /// <param name="sortDefinition">The parameters to sort by.</param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of a missing property name.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="sortDefinition"/> is <cref lang="null"/>.
        /// </exception>
        public static void Sort(IList source, ISortDefinition sortDefinition)
        {
//            ArrayList.Adapter(source).Sort(new PropertyComparator(sortDefinition));
            ICollection coll = CollectionUtils.StableSort(source, new PropertyComparator(sortDefinition));
            int index = 0;
            IEnumerator it = coll.GetEnumerator();
            while(it.MoveNext())
            {
                source[index] = it.Current;
                index++;
            }
        }
    }
}
