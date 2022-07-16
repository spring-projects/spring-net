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
using Spring.Util;

namespace Spring.Objects.Factory
{
	/// <summary>
	/// Thrown in case of a reference to an object that is currently in creation.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Typically happens when constructor autowiring matches the currently
	/// constructed object.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans</author>
	[Serializable]
	public class ObjectCurrentlyInCreationException : ObjectCreationException
	{
        /// <summary>
        /// The default error message text to be used, if none is specified.
        /// </summary>
        public const string DEFAULTMESSAGE = "Requested object is currently in creation: Is there an unresolvable circular reference?";

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.ObjectCurrentlyInCreationException"/> class.
        /// </summary>
		public ObjectCurrentlyInCreationException()
		{
		}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.ObjectCurrentlyInCreationException"/> class.
        /// </summary>
		/// <param name="objectName">
		/// The name of the object that triggered the exception.
		/// </param>
		public ObjectCurrentlyInCreationException(string objectName)
            : this(null, objectName, null, null)
		{
		}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.ObjectCurrentlyInCreationException"/> class.
        /// </summary>
        /// <param name="objectName">
		/// The name of the object that triggered the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public ObjectCurrentlyInCreationException(string objectName, Exception rootCause)
            : this(null, objectName, null, rootCause)
        {
        }

        /// <summary>
		/// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.ObjectCurrentlyInCreationException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
        /// <param name="objectName">
		/// The name of the object that triggered the exception.
		/// </param>
        public ObjectCurrentlyInCreationException(string objectName, string message)
            : this(null, objectName, message, null)
		{
		}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.ObjectCurrentlyInCreationException"/> class.
        /// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
        /// <param name="objectName">
		/// The name of the object that triggered the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
        public ObjectCurrentlyInCreationException(string objectName, string message, Exception rootCause)
            : this(null, objectName, message, rootCause)
		{
		}

        /// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.ObjectCurrentlyInCreationException"/> class.
		/// </summary>
		/// <param name="resourceDescription">
		/// The description of the resource associated with the object.
		/// </param>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
        /// <param name="objectName">
		/// The name of the object that triggered the exception.
		/// </param>
        public ObjectCurrentlyInCreationException(
			string resourceDescription,
            string objectName,
			string message)
            : this(resourceDescription, objectName, message, null)
		{
		}

        /// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.ObjectCurrentlyInCreationException"/> class.
		/// </summary>
		/// <param name="resourceDescription">
		/// The description of the resource associated with the object.
		/// </param>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
        /// <param name="objectName">
		/// The name of the object that triggered the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
        public ObjectCurrentlyInCreationException(
			string resourceDescription,
            string objectName,
			string message,
			Exception rootCause)
            : base(resourceDescription,
                objectName,
                StringUtils.HasText(message) ? message : DEFAULTMESSAGE,
                rootCause)
		{
		}

		/// <summary>
		/// Creates a new instance of the ObjectCurrentlyInCreationException class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
		protected ObjectCurrentlyInCreationException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
