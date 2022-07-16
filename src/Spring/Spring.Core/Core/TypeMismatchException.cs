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
using System.Text;

#endregion

namespace Spring.Core
{
    /// <summary>
    /// Exception thrown on a <see cref="System.Type"/> mismatch when trying to set a property
    /// or resolve an argument to a method invocation.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    [Serializable]
    public class TypeMismatchException : PropertyAccessException
    {
        /// <summary>
        /// The string error code used to classify the exception.
        /// </summary>
        public override string ErrorCode
        {
            get { return "typeMismatch"; }
        }

        /// <summary>
        /// Creates a new instance of the TypeMismatchException class.
        /// </summary>
        public TypeMismatchException()
        {
        }

        /// <summary>
        /// Creates a new instance of the TypeMismatchException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public TypeMismatchException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the TypeMismatchException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public TypeMismatchException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the TypeMismatchException class describing the
        /// property and required type that could not used to set a property on the target object.
        /// </summary>
        /// <param name="propertyChangeEventArgs">
        /// The description of the property that was to be changed.
        /// </param>
        /// <param name="requiredType">The target conversion type.</param>
        public TypeMismatchException(
            PropertyChangeEventArgs propertyChangeEventArgs, Type requiredType) :
                base(BuildMessage(propertyChangeEventArgs, requiredType), propertyChangeEventArgs)
        {
        }

        /// <summary>
        /// Creates a new instance of the TypeMismatchException class describing the
        /// property, required type, and underlying exception that could not be used
        /// to set a property on the target object.
        /// </summary>
        /// <param name="propertyChangeEventArgs">
        /// The description of the property that was to be changed.
        /// </param>
        /// <param name="requiredType">The target conversion type.</param>
        /// <param name="rootCause">The underlying exception.</param>
        public TypeMismatchException(
            PropertyChangeEventArgs propertyChangeEventArgs, Type requiredType, Exception rootCause) :
                base(BuildMessage(propertyChangeEventArgs, requiredType), propertyChangeEventArgs, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the TypeMismatchException class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected TypeMismatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static string BuildMessage(PropertyChangeEventArgs propertyChangeEventArgs, Type requiredType)
        {
            StringBuilder message = new StringBuilder();
            message.Append("Cannot convert property value of type [");
            if (propertyChangeEventArgs != null && propertyChangeEventArgs.NewValue != null)
            {
                message.Append(propertyChangeEventArgs.NewValue.GetType().FullName);
            }
            else
            {
                message.Append("null");
            }
            message.Append("] to required type [");
            if (requiredType != null)
            {
                message.Append(requiredType.FullName);
            }
            else
            {
                message.Append("null");
            }
            message.Append("] for property '");
            if (propertyChangeEventArgs != null && propertyChangeEventArgs.PropertyName != null)
            {
                message.Append(propertyChangeEventArgs.PropertyName);
            }
            message.Append("'.");
            return message.ToString();
        }
    }
}
