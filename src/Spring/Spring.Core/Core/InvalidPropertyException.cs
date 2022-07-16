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

using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Spring.Util;

namespace Spring.Core
{
    /// <summary>
    /// Thrown in response to referring to an invalid property (most often via reflection).
    /// </summary>
    /// <author>Rick Evans</author>
    [Serializable]
    public class InvalidPropertyException : FatalReflectionException
    {
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="InvalidPropertyException"/> class.
        /// </summary>
        public InvalidPropertyException()
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="InvalidPropertyException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public InvalidPropertyException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="InvalidPropertyException"/> class.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> that is (or rather was) the source of the
        /// offending property.
        /// </param>
        /// <param name="propertyName">
        /// The name of the offending property.
        /// </param>
        public InvalidPropertyException(Type type, string propertyName)
            : this(type, propertyName, string.Format(
                                           CultureInfo.InvariantCulture,
                                           "Invalid property '{0}' in class [{1}].", propertyName, type.FullName))
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="InvalidPropertyException"/> class.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> that is (or rather was) the source of the
        /// offending property.
        /// </param>
        /// <param name="propertyName">
        /// The name of the offending property.
        /// </param>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public InvalidPropertyException(Type type, string propertyName, string message)
            : base(message)
        {
            offendingObjectType = type;
            offendingPropertyName = propertyName;
        }

        /// <summary>
        /// Creates a new instance of the InvalidPropertyException class.
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
        public InvalidPropertyException (
            Type offendingType, string offendingProperty, string message, Exception rootCause)
            : base (message, rootCause)
        {
            offendingObjectType = offendingType;
            offendingPropertyName = offendingProperty;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="InvalidPropertyException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public InvalidPropertyException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="InvalidPropertyException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected InvalidPropertyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var typeName = info.GetString("ObjectTypeName");
            offendingObjectType = typeName != null ? Type.GetType(typeName) : null;
            offendingPropertyName = info.GetString("OffendingPropertyName");
        }

        /// <summary>
        /// The <see cref="System.Type"/> that is (or rather was) the source of the
        /// offending property.
        /// </summary>
        public Type ObjectType
        {
            get { return offendingObjectType; }
        }

        /// <summary>
        /// The name of the offending property.
        /// </summary>
        public string OffendingPropertyName
        {
            get { return offendingPropertyName; }
        }

        /// <summary>
        /// Populates a <see cref="System.Runtime.Serialization.SerializationInfo"/> with
        /// the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/> to populate
        /// with data.
        /// </param>
        /// <param name="context">
        /// The destination (see <see cref="System.Runtime.Serialization.StreamingContext"/>)
        /// for this serialization.
        /// </param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(
            SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ObjectTypeName", ObjectType?.AssemblyQualifiedNameWithoutVersion());
            info.AddValue("OffendingPropertyName", OffendingPropertyName);
        }

        private Type offendingObjectType;
        private string offendingPropertyName;
    }
}
