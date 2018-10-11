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

#region Imports

using System.Collections;
using System.Collections.Generic;

#endregion

namespace Spring.Objects
{
    /// <summary>
    ///     A collection style container for <see cref="Spring.Objects.PropertyValue" />
    ///     instances.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Mark Pollack (.NET) </author>
    public interface IPropertyValues : IEnumerable
    {
        /// <summary>
        ///     Return an array of the <see cref="Spring.Objects.PropertyValue" /> objects
        ///     held in this object.
        /// </summary>
        /// <returns>
        ///     An array of the <see cref="Spring.Objects.PropertyValue" /> objects held
        ///     in this object.
        /// </returns>
        IReadOnlyList<PropertyValue> PropertyValues { get; }

        /// <summary>
        ///     Return the <see cref="Spring.Objects.PropertyValue" /> instance with the
        ///     given name.
        /// </summary>
        /// <param name="propertyName">The name to search for.</param>
        /// <returns>
        ///     the <see cref="Spring.Objects.PropertyValue" />, or null if a
        ///     the <see cref="Spring.Objects.PropertyValue" /> with the supplied
        ///     <paramref name="propertyName" /> did not exist in this collection.
        /// </returns>
        PropertyValue GetPropertyValue(string propertyName);

        /// <summary>
        ///     Is there a <see cref="Spring.Objects.PropertyValue" /> instance for this
        ///     property name?
        /// </summary>
        /// <param name="propertyName">The name to search for.</param>
        /// <returns>
        ///     True if there is a <see cref="Spring.Objects.PropertyValue" /> instance for
        ///     the supplied <paramref name="propertyName" />.
        /// </returns>
        bool Contains(string propertyName);

        /// <summary>
        ///     Return the difference (changes, additions, but not removals) of
        ///     property values between the supplied argument and the values
        ///     contained in the collection.
        /// </summary>
        /// <remarks>
        ///     <p>
        ///         Subclasses should also override <c>Equals</c>.
        ///     </p>
        /// </remarks>
        /// <param name="old">The old property values.</param>
        /// <returns>
        ///     An <see cref="Spring.Objects.IPropertyValues" /> containing any changes, or
        ///     an empty <see cref="Spring.Objects.IPropertyValues" /> instance if there were
        ///     no changes.
        /// </returns>
        IPropertyValues ChangesSince(IPropertyValues old);
    }
}