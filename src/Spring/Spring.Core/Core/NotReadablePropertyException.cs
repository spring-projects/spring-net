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
    /// Thrown in response to a failed attempt to read a property.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Typically thrown when attempting to read the value of a write-only
    /// property via reflection.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class NotReadablePropertyException : InvalidPropertyException
    {
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Core.NotReadablePropertyException"/> class.
        /// </summary>
        public NotReadablePropertyException()
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Core.NotReadablePropertyException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public NotReadablePropertyException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Core.NotReadablePropertyException"/> class.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> that is (or rather was) the source of the
        /// offending property.
        /// </param>
        /// <param name="propertyName">
        /// The name of the offending property.
        /// </param>
        public NotReadablePropertyException(Type type, string propertyName)
            : base(type, propertyName, string.Format(
                                           CultureInfo.InvariantCulture,
                                           "Cannot read the value of the '{0}' property " +
                                           "declared on the [{1}] class : property is read-only.", propertyName, type.FullName))
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Core.NotReadablePropertyException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public NotReadablePropertyException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Core.NotReadablePropertyException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected NotReadablePropertyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
