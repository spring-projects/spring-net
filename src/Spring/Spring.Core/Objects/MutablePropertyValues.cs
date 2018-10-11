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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Spring.Util;

namespace Spring.Objects
{
    /// <summary>
    /// Default implementation of the <see cref="Spring.Objects.IPropertyValues"/>
    /// interface.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Allows simple manipulation of properties, and provides constructors to
    /// support deep copy and construction from a number of collection types such as
    /// <see cref="System.Collections.IDictionary"/> and
    /// <see cref="System.Collections.IList"/>.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class MutablePropertyValues : IPropertyValues
    {
        private static readonly IReadOnlyList<PropertyValue> emptyPropertyValuesList = new List<PropertyValue>();
        private List<PropertyValue> propertyValuesList;

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.Objects.MutablePropertyValues"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The returned instance is initially empty...
        /// <see cref="Spring.Objects.PropertyValue"/>s can be added with the various
        /// overloaded <see cref="Spring.Objects.MutablePropertyValues.Add(PropertyValue)"/>,
        /// <see cref="Spring.Objects.MutablePropertyValues.Add(string, object)"/>,
        /// <see cref="Spring.Objects.MutablePropertyValues.AddAll(IDictionary{string, object})"/>,
        /// and <see cref="Spring.Objects.MutablePropertyValues.AddAll(IList{PropertyValue})"/>
        /// methods.
        /// </p>
        /// </remarks>
        /// <seealso cref="Spring.Objects.MutablePropertyValues.Add (PropertyValue)"/>
        /// <seealso cref="Spring.Objects.MutablePropertyValues.Add (string, object)"/>
        public MutablePropertyValues()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.Objects.MutablePropertyValues"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Deep copy constructor. Guarantees <see cref="Spring.Objects.PropertyValue"/>
        /// references are independent, although it can't deep copy objects currently
        /// referenced by individual <see cref="Spring.Objects.PropertyValue"/> objects.
        /// </p>
        /// </remarks>
        public MutablePropertyValues(IPropertyValues other)
        {
            if (other != null)
            {
                AddAll(other.PropertyValues);
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.Objects.MutablePropertyValues"/>
        /// class.
        /// </summary>
        /// <param name="map">
        /// The <see cref="System.Collections.IDictionary"/> with property values
        /// keyed by property name, which must be a <see cref="System.String"/>.
        /// </param>
        public MutablePropertyValues(IReadOnlyDictionary<string, object> map)
        {
            AddAll(map);
        }

        /// <summary>
        /// Property to retrieve the array of property values.
        /// </summary>
        public IReadOnlyList<PropertyValue> PropertyValues => propertyValuesList ?? emptyPropertyValuesList;

        /// <summary>
        /// Overloaded version of <c>Add</c> that takes a property name and a property value.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="propertyValue">
        /// The value of the property.
        /// </param>
        public void Add(string propertyName, object propertyValue)
        {
            Add(new PropertyValue(propertyName, propertyValue));
        }

        /// <summary>
        /// Add the supplied <see cref="Spring.Objects.PropertyValue"/> object,
        /// replacing any existing one for the respective property.
        /// </summary>
        /// <param name="pv">
        /// The <see cref="Spring.Objects.PropertyValue"/> object to add.
        /// </param>
        public void Add(PropertyValue pv)
        {
            propertyValuesList = propertyValuesList ?? new List<PropertyValue>();

            for (int i = 0; i < propertyValuesList.Count; ++i)
            {
                PropertyValue currentPv = propertyValuesList[i];
                if (currentPv.Name == pv.Name)
                {
                    pv = MergeIfRequired(pv, currentPv);
                    propertyValuesList[i] = pv;
                    return;
                }
            }

            propertyValuesList.Add(pv);
        }

        /// <summary>
        /// Merges the value of the supplied 'new' <see cref="PropertyValue"/> with that of
        /// the current <see cref="PropertyValue"/> if merging is supported and enabled.
        /// </summary>
        /// <see cref="IMergable"/>
        /// <param name="newPv">The new pv.</param>
        /// <param name="currentPv">The current pv.</param>
        /// <returns>The possibly merged PropertyValue</returns>
        private PropertyValue MergeIfRequired(PropertyValue newPv, PropertyValue currentPv)
        {
            object val = newPv.Value;
            if (val is IMergable mergable)
            {
                if (mergable.MergeEnabled)
                {
                    object merged = mergable.Merge(currentPv.Value);
                    return new PropertyValue(newPv.Name, merged);
                }
            }

            return newPv;
        }

        /// <summary>
        /// Add all property values from the given
        /// <see cref="System.Collections.IDictionary"/>.
        /// </summary>
        /// <param name="map">
        /// The map of property values, the keys of which must be
        /// <see cref="System.String"/>s.
        /// </param>
        public void AddAll(IReadOnlyDictionary<string, object> map)
        {
            if (map != null)
            {
                foreach (KeyValuePair<string, object> pair in map)
                {
                    Add(new PropertyValue(pair.Key, pair.Value));
                }
            }
        }
        
        /// <summary>
        /// Add all property values from the given
        /// <see cref="System.Collections.IDictionary"/>.
        /// </summary>
        /// <param name="map">
        /// The map of property values, the keys of which must be
        /// <see cref="System.String"/>s.
        /// </param>
        public void AddAll(IDictionary<string, object> map)
        {
            if (map != null)
            {
                foreach (KeyValuePair<string, object> pair in map)
                {
                    Add(new PropertyValue(pair.Key, pair.Value));
                }
            }
        }


        /// <summary>
        /// Add all property values from the given
        /// <see cref="System.Collections.IList"/>.
        /// </summary>
        /// <param name="values">
        /// The list of <see cref="Spring.Objects.PropertyValue"/>s to be added.
        /// </param>
        public void AddAll(IReadOnlyList<PropertyValue> values)
        {
            if (values != null)
            {
                for (var i = 0; i < values.Count; i++)
                {
                    Add(values[i]);
                }
            }
        }

        /// <summary>
        /// Remove the given <see cref="Spring.Objects.PropertyValue"/>, if contained.
        /// </summary>
        /// <param name="pv">
        /// The <see cref="Spring.Objects.PropertyValue"/> to remove.
        /// </param>
        public void Remove(PropertyValue pv)
        {
            propertyValuesList?.Remove(pv);
        }

        /// <summary>
        /// Removes the named <see cref="Spring.Objects.PropertyValue"/>, if contained.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        public void Remove(string propertyName)
        {
            Remove(GetPropertyValue(propertyName));
        }

        /// <summary>
        /// Modify a <see cref="Spring.Objects.PropertyValue"/> object held in this object. Indexed from 0.
        /// </summary>
        public void SetPropertyValueAt(PropertyValue pv, int i)
        {
            propertyValuesList = propertyValuesList ?? new List<PropertyValue>();
            propertyValuesList[i] = pv;
        }

        /// <summary>
        /// Return the property value given the name.
        /// </summary>
        /// <remarks>
        /// The property name is checked in a <c>case-insensitive</c> fashion.
        /// </remarks>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <returns>
        /// The property value.
        /// </returns>
        public PropertyValue GetPropertyValue(string propertyName)
        {
            if (propertyValuesList == null)
            {
                return null;
            }
            
            string propertyNameLowered = propertyName.ToLower(CultureInfo.CurrentCulture);
            for (var i = 0; i < propertyValuesList.Count; i++)
            {
                PropertyValue pv = propertyValuesList[i];
                if (pv.Name.ToLower(CultureInfo.CurrentCulture).Equals(propertyNameLowered))
                {
                    return pv;
                }
            }

            return null;
        }

        /// <summary>
        /// Does the container of properties contain one of this name.
        /// </summary>
        /// <param name="propertyName">The name of the property to search for.</param>
        /// <returns>
        /// True if the property is contained in this collection, false otherwise.
        /// </returns>
        public bool Contains(string propertyName)
        {
            return GetPropertyValue(propertyName) != null;
        }

        /// <summary>
        /// Return the difference (changes, additions, but not removals) of
        /// property values between the supplied argument and the values
        /// contained in the collection.
        /// </summary>
        /// <param name="old">Another property values collection.</param>
        /// <returns>
        /// The collection of property values that are different than the supplied one.
        /// </returns>
        public IPropertyValues ChangesSince(IPropertyValues old)
        {
            var changes = new MutablePropertyValues();
            if (old == this || propertyValuesList == null)
            {
                return changes;
            }

            // for each property value in this (the newer set)
            foreach (PropertyValue newProperty in propertyValuesList)
            {
                PropertyValue oldProperty = old.GetPropertyValue(newProperty.Name);
                if (oldProperty == null)
                {
                    // if there wasn't an old one, add it
                    changes.Add(newProperty);
                }
                else if (!oldProperty.Equals(newProperty))
                {
                    // it's changed
                    changes.Add(newProperty);
                }
            }

            return changes;
        }

        /// <summary>
        /// Returns an <see cref="System.Collections.IEnumerator"/> that can iterate
        /// through a collection.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The returned <see cref="System.Collections.IEnumerator"/> is the
        /// <see cref="System.Collections.IEnumerator"/> exposed by the
        /// <see cref="Spring.Objects.MutablePropertyValues.PropertyValues"/>
        /// property.
        /// </p>
        /// </remarks>
        /// <returns>
        /// An <see cref="System.Collections.IEnumerator"/> that can iterate through a
        /// collection.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return PropertyValues.GetEnumerator();
        }

        // CLOVER:OFF

        /// <summary>
        /// Convert the object to a string representation.
        /// </summary>
        /// <returns>
        /// A string representation of the object.
        /// </returns>
        public override string ToString()
        {
            var pvs = PropertyValues;
            StringBuilder sb
                = new StringBuilder(
                    "MutablePropertyValues: length=").Append(pvs.Count).Append("; ");
            sb.Append(StringUtils.ArrayToDelimitedString(pvs, ","));
            return sb.ToString();
        }

        // CLOVER:ON
    }
}