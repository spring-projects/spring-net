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

using System.Runtime.Serialization;
using System.Security.Permissions;

using Spring.Util;

#endregion

namespace Spring.Core
{
    /// <summary>
    /// Superclass for exceptions related to a property access, such as a <see cref="System.Type"/>
    /// mismatch or a target invocation exception.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Mark Pollack (.NET)</author>
    [Serializable]
    public abstract class PropertyAccessException : ReflectionException, IErrorCoded
    {
        /// <summary>
        /// Returns the PropertyChangeEventArgs that resulted in the problem.
        /// </summary>
        public PropertyChangeEventArgs PropertyChangeArgs
        {
            get { return _propertyChangeEventArgs; }
        }

        /// <summary>
        /// The string error code used to classify the error.
        /// </summary>
        public abstract string ErrorCode { get; }

        private PropertyChangeEventArgs _propertyChangeEventArgs;

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
            info.AddValue("PropertyChangeArgs", PropertyChangeArgs);
        }

        #endregion

        /// <summary>
        /// Create a new instance of the PropertyAccessException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="propertyChangeEvent">Describes the change attempted on the property.</param>
        protected PropertyAccessException(string message, PropertyChangeEventArgs propertyChangeEvent) : base(message)
        {
            _propertyChangeEventArgs = propertyChangeEvent;
        }

        /// <summary>
        /// Create a new instance of the PropertyAccessException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="propertyChangeEvent">Describes the change attempted on the property.</param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        protected PropertyAccessException(string message, PropertyChangeEventArgs propertyChangeEvent, Exception rootCause)
            : base(message, rootCause)
        {
            _propertyChangeEventArgs = propertyChangeEvent;
        }

        /// <summary>
        /// Creates a new instance of the PropertyAccessException class.
        /// </summary>
        protected PropertyAccessException()
        {
        }

        /// <summary>
        /// Creates a new instance of the PropertyAccessException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        protected PropertyAccessException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the PropertyAccessExceptionsException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        protected PropertyAccessException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the PropertyAccessExceptionsException class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected PropertyAccessException(
            SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _propertyChangeEventArgs = info.GetValue("PropertyChangeArgs", typeof (object)) as PropertyChangeEventArgs;
        }
    }
}
