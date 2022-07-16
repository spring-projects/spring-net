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

using System.Runtime.Serialization;

namespace Spring.Objects.Factory
{
    /// <summary>
    /// Exception that an object implementation is suggested to throw if its own
    /// factory-aware initialization code fails.
    /// <see cref="Spring.Objects.ObjectsException"/> thrown by object factory methods
    /// themselves should simply be propagated as-is.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Note that non-factory-aware initialization methods like AfterPropertiesSet ()
    /// or a custom "init-method" can throw any exception.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class ObjectInitializationException : FatalObjectException
    {
        #region Constructor (s) / Destructor
        /// <summary>
        /// Creates a new instance of the ObjectInitializationException class.
        /// </summary>
        public ObjectInitializationException ()
        {
        }

        /// <summary>
        /// Creates a new instance of the ObjectInitializationException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public ObjectInitializationException (string message)
            : base (message)
        {
        }

        /// <summary>
        /// Creates a new instance of the ObjectInitializationException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public ObjectInitializationException (string message, Exception rootCause)
            : base (message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the ObjectInitializationException class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected ObjectInitializationException (
            SerializationInfo info, StreamingContext context)
            : base (info, context)
        {
        }
        #endregion
    }
}
