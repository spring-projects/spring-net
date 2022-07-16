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

namespace Spring.Core
{
    /// <summary>
    /// Interface defining a generic contract for attaching and accessing metadata
    /// to/from arbitrary objects.
    /// </summary>
    public interface IAttributeAccessor
    {
        /// <summary>
        /// Set the attribute defined by <code>name</code> to the supplied	<code>value</code>.
        /// In general, users should take care to prevent overlaps with other
        /// metadata attributes by using fully-qualified names, perhaps using
        /// class or package names as prefix.
        /// </summary>
        /// <param name="name">the unique attribute key</param>
        /// <param name="value">the attribute value to be attached</param>
        void SetAttribute(string name, object value);

        /// <summary>
        /// Get the value of the attribute identified by <code>name</code>.
        /// Return <code>null</code> if the attribute doesn't exist.
        /// </summary>
        /// <param name="name">the unique attribute key</param>
        /// <returns>the current value of the attribute, if any</returns>
        object GetAttribute(string name);

        /// <summary>
        /// Remove the attribute identified by <code>name</code> and return its value.
        /// Return <code>null</code> if no attribute under <code>name</code> is found.
        /// </summary>
        /// <param name="name">the unique attribute key</param>
        /// <returns>The last value of the attribute, if any</returns>
        object RemoveAttribute(string name);

        /// <summary>
        /// Checks weather a specific attributes exists
        /// </summary>
        /// <param name="name">The unique attribute key</param>
        /// <returns>
        /// <code>true</code> if the attribute identified by <code>name</code> exists.
        /// Otherwise return <code>false</code>
        /// </returns>
        bool HasAttribute(string name);

        /// <summary>
        /// Return the names of all attributes.
        /// </summary>
        String[] AttributeNames { get; }

    }
}
