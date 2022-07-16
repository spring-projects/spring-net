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

#region Imports

using System.Globalization;
using System.Runtime.Serialization;

#endregion

namespace Spring.Core
{
    /// <summary>
    /// Thrown in response to a failed attempt to write a property.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    [Serializable]
    public class NotWritablePropertyException : InvalidPropertyException
    {
        /// <summary>
        /// Creates a new instance of the NotWritablePropertyException class.
        /// </summary>
        public NotWritablePropertyException()
        {
        }

        /// <summary>
        /// Creates a new instance of the NotWritablePropertyException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public NotWritablePropertyException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the NotWritablePropertyException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public NotWritablePropertyException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the NotWritablePropertyException class.
        /// </summary>
        /// <param name="offendingType">
        /// The <see cref="System.Type"/> that is (or rather was) the source of the
        /// offending property.
        /// </param>
        /// <param name="offendingProperty">
        /// The name of the offending property.
        /// </param>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public NotWritablePropertyException(
            Type offendingType, string offendingProperty, string message, Exception rootCause)
            : base(offendingType, offendingProperty, message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the NotWritablePropertyException class
        /// summarizing what property was not writable.
        /// </summary>
        /// <param name="offendingProperty">
        /// The name of the property that is not writable.
        /// </param>
        /// <param name="offendingType">
        /// The <see cref="System.Type"/> in which the property is not writable.
        /// </param>
        public NotWritablePropertyException(string offendingProperty, Type offendingType)
            : base(offendingType, offendingProperty,
                   string.Format(CultureInfo.InvariantCulture,
                                 "Property '{0}' is not writable in class [{1}].",
                                 offendingProperty, offendingType.FullName))
        {
        }

        /// <summary>
        /// Creates new NotWritablePropertyException with a root cause.
        /// </summary>
        /// <param name="offendingProperty">
        /// The name of the property that is not writable.
        /// </param>
        /// <param name="offendingType">
        /// The <see cref="System.Type"/> in which the property is not writable.
        /// </param>
        /// <param name="rootCause">
        /// The root cause indicating why the property was not writable.
        /// </param>
        public NotWritablePropertyException(string offendingProperty, Type offendingType, Exception rootCause)
            : base(offendingType, offendingProperty,
                   string.Format(CultureInfo.InvariantCulture,
                                 "Property '{0}' is not writable in class [{1}].",
                                 offendingProperty,
                                 offendingType != null ? offendingType.FullName : "null"),
                   rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the NotWritablePropertyException class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected NotWritablePropertyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
