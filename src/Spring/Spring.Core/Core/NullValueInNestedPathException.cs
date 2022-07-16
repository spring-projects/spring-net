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
using System.Security.Permissions;
using Spring.Util;

#endregion

namespace Spring.Core
{
    /// <summary>
    /// Thrown in response to encountering a <see langword="null"/> value
    /// when traversing a nested path expression.
    /// </summary>
    [Serializable]
    public class NullValueInNestedPathException : FatalReflectionException
    {
        private string property;
        private Type type;

        /// <summary>
        /// The name of the offending property.
        /// </summary>
        public string PropertyName
        {
            get { return property; }
        }

        /// <summary>
        /// The <see cref="System.Type"/> of the class where the property was last looked for.
        /// </summary>
        public Type ObjectType
        {
            get { return type; }
        }

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="NullValueInNestedPathException"/> class.
        /// </summary>
        public NullValueInNestedPathException()
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="NullValueInNestedPathException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public NullValueInNestedPathException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="NullValueInNestedPathException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public NullValueInNestedPathException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="NullValueInNestedPathException"/> class.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the object where the property was not found.
        /// </param>
        /// <param name="theProperty">The name of the property not found.</param>
        public NullValueInNestedPathException(Type type, string theProperty)
            : this(type, theProperty, string.Format(CultureInfo.InvariantCulture,
                                                    "Value of nested property '{0}' is null in Type [{1}].", theProperty, type))
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="NullValueInNestedPathException"/> class.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the object where the property was not found.
        /// </param>
        /// <param name="theProperty">The name of the property not found.</param>
        /// <param name="message">A message about the exception.</param>
        public NullValueInNestedPathException(Type type, string theProperty, string message)
            : base(message)
        {
            property = theProperty;
            this.type = type;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="NullValueInNestedPathException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected NullValueInNestedPathException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            type = info.GetValue("ObjectType", typeof (Type)) as Type;
            property = info.GetString("PropertyName");
        }

        #endregion

        #region Methods

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
            info.AddValue("ObjectType", ObjectType);
            info.AddValue("PropertyName", PropertyName);
        }

        #endregion
    }
}
