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

using System.Reflection;
using Spring.Core;

namespace Spring.Objects
{
    /// <summary>
    /// The central interface of Spring.NET's low-level object infrastructure.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Typically not directly used by application code but rather implicitly
    /// via an <see cref="Spring.Objects.Factory.IObjectFactory"/>.
    /// </p>
    /// <p>
    /// Implementing classes have the ability to get and set property values
    /// (individually or in bulk), get property descriptors and query the
    /// readability and writability of properties.
    /// </p>
    /// <p>
    /// This interface supports <b>nested properties</b> enabling the setting
    /// of properties on subproperties to an unlimited depth.
    /// </p>
    /// <p>
    /// If a property update causes an exception, a
    /// <see cref="PropertyAccessException"/> will be thrown. Bulk
    /// updates continue after exceptions are encountered, throwing an exception
    /// wrapping <b>all</b> exceptions encountered during the update.
    /// </p>
    /// <p>
    /// <see cref="Spring.Objects.IObjectWrapper"/> implementations can be used
    /// repeatedly, with their "target" or wrapped object changed.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface IObjectWrapper
    {
        /// <summary>
        /// The object wrapped by the wrapper (cannot be <see lang="null"/>).
        /// </summary>
        /// <remarks>
        /// <p>
        /// Implementations are required to allow the type of the wrapped
        /// object to change.
        /// </p>
        /// </remarks>
        /// <returns>The object wrapped by this wrapper.</returns>
        object WrappedInstance { get; set; }

    	/// <summary>
    	/// Convenience method to return the <see cref="System.Type"/>
    	/// of the wrapped object.
    	/// </summary>
    	/// <returns>The <see cref="System.Type"/> of the wrapped object.</returns>
    	Type WrappedType { get; }

    	/// <summary>Get the value of a property.</summary>
    	/// <param name="theProperty">
    	/// The name of the property to get the value of. May be nested.
    	/// </param>
    	/// <returns>The value of the property.</returns>
    	/// <exception cref="Spring.Objects.FatalObjectException">
    	/// if the property isn't readable, or if the getting the value throws
    	/// an exception.
    	/// </exception>
    	object GetPropertyValue(string theProperty);

    	/// <summary>
    	/// Get the <see cref="System.Reflection.PropertyInfo"/> for a particular
    	/// property.
    	/// </summary>
    	/// <param name="theProperty">
    	/// The property to be retrieved.
    	/// </param>
    	/// <returns>
    	/// The <see cref="System.Reflection.PropertyInfo"/> for the particular
    	/// property.
    	/// </returns>
		PropertyInfo GetPropertyInfo(string theProperty);

		/// <summary>
		/// Get the <see cref="System.Type"/> for a particular property.
		/// </summary>
		/// <param name="theProperty">
		/// The property the <see cref="System.Type"/> of which is to be retrieved.
		/// </param>
		/// <returns>
		/// The <see cref="System.Type"/> for a particular property..
		/// </returns>
		Type GetPropertyType(string theProperty);

    	/// <summary>
    	/// Get all of the <see cref="System.Reflection.PropertyInfo"/> instances for
    	/// all of the properties of the wrapped object.
    	/// </summary>
    	/// <returns>
    	/// An array of <see cref="System.Reflection.PropertyInfo"/> instances.
    	/// </returns>
    	PropertyInfo[] GetPropertyInfos();

    	/// <summary>
    	/// Set a property value.
    	/// </summary>
    	/// <remarks>
    	/// <p>
    	/// <b>This is the preferred way to update an individual property.</b>
    	/// </p>
    	/// </remarks>
    	/// <param name="propertyValue">The new property value.</param>
    	void SetPropertyValue(PropertyValue propertyValue);

    	/// <summary>
    	/// Set a property value.
    	/// </summary>
    	/// <remarks>
    	/// <p>
    	/// This method is provided for convenience only. The
    	/// <see cref="Spring.Objects.IObjectWrapper.SetPropertyValue(PropertyValue)"/>
    	/// method is more powerful.
    	/// </p>
    	/// </remarks>
    	/// <param name="theProperty">
    	/// The name of the property to set value of.
    	/// </param>
    	/// <param name="propertyValue">The new property value.</param>
    	void SetPropertyValue(string theProperty, object propertyValue);

    	/// <summary>Set a number of property values in bulk.</summary>
    	/// <remarks>
    	/// <p>
    	/// This is the preferred way to perform a bulk update.
    	/// </p>
    	/// <p>
    	/// Note that performing a bulk update differs from performing a single update,
    	/// in that an implementation of this class will continue to update properties
    	/// if a <b>recoverable</b> error (such as a vetoed property change or a type
    	/// mismatch, but <b>not</b> an invalid property name or the like) is
    	/// encountered, throwing a
    	/// <see cref="Spring.Objects.PropertyAccessExceptionsException"/> containing
    	/// all the individual errors. This exception can be examined later to see all
    	/// binding errors. Properties that were successfully updated stay changed.
    	/// </p>
    	/// <p>
    	/// Does not allow the setting of unknown fields. Equivalent to
    	/// <see cref="Spring.Objects.IObjectWrapper.SetPropertyValues(IPropertyValues,bool)"/>
    	/// with an argument of <c>false</c> for the second parameter.
    	/// </p>
    	/// </remarks>
    	/// <param name="values">
    	/// The collection of <see cref="Spring.Objects.PropertyValue"/> instances to
    	/// set on the wrapped object.
    	/// </param>
    	void SetPropertyValues(IPropertyValues values);

    	/// <summary>
    	/// Set a number of property values in bulk with full control over behavior.
    	/// </summary>
    	/// <remarks>
    	/// <p>
    	/// Note that performing a bulk update differs from performing a single update,
    	/// in that an implementation of this class will continue to update properties
    	/// if a <b>recoverable</b> error (such as a vetoed property change or a type
    	/// mismatch, but <b>not</b> an invalid property name or the like) is
    	/// encountered, throwing a
    	/// <see cref="Spring.Objects.PropertyAccessExceptionsException"/> containing
    	/// all the individual errors. This exception can be examined later to see all
    	/// binding errors. Properties that were successfully updated stay changed.
    	/// </p>
    	/// <p>Does not allow the setting of unknown fields.
    	/// </p>
    	/// </remarks>
    	/// <param name="values">
    	/// The <see cref="Spring.Objects.IPropertyValues"/> to set on the target object
    	/// </param>
    	/// <param name="ignoreUnknown">
    	/// Should we ignore unknown values (not found in the object!?)
    	/// </param>
    	void SetPropertyValues(IPropertyValues values, bool ignoreUnknown);

    }
}
