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

#endregion

namespace Spring.Core
{
    /// <summary>
    /// Thrown when a method (typically a property getter or setter invoked via reflection)
    /// throws an exception, analogous to a <see cref="System.Reflection.TargetInvocationException"/>.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Mark Pollack (.NET)</author>
    [Serializable]
    public class MethodInvocationException : PropertyAccessException
    {
        /// <summary>
        /// The error code string for this exception.
        /// </summary>
        override public string ErrorCode
        {
            get
            {
                return "methodInvocation";
            }
        }

        #region Constructor (s) / Destructor
        /// <summary>
        /// Creates a new instance of the MethodInvocationException class.
        /// </summary>
        public MethodInvocationException ()
        {
        }

        /// <summary>
        /// Creates a new instance of the MethodInvocationException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public MethodInvocationException (string message)
            : base (message)
        {
        }

        /// <summary>
        /// Creates a new instance of the MethodInvocationException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public MethodInvocationException (string message, Exception rootCause)
            : base (message, rootCause)
        {
        }

        /// <summary>
        /// Constructor to use when an exception results from a
        /// <see cref="System.ComponentModel.PropertyChangedEventArgs"/>.
        /// </summary>
        /// <param name="ex">
        /// The <see cref="System.Exception"/> raised by the invoked property.
        /// </param>
        /// <param name="argument">
        /// The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> that
        /// resulted in an exception.
        /// </param>
        public MethodInvocationException (Exception ex, PropertyChangeEventArgs argument) :
            base ("Property '" + argument.PropertyName + "' threw exception.", argument, ex)
        {
        }

        /// <summary>
        /// Creates a new instance of the MethodInvocationException class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected MethodInvocationException (
            SerializationInfo info, StreamingContext context)
            : base (info, context)
        {
        }
        #endregion
    }
}
