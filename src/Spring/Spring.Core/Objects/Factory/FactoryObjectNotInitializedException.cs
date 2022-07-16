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
	/// Exception thrown if an
	/// <see cref="Spring.Objects.Factory.IFactoryObject"/> is not fully
	/// initialized, for example if it is involved in a circular reference.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This is usually indicated by any of the variants of the
	/// <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/>
	/// method returning <see langword="null"/>.
	/// </p>
	/// <p>
	/// A circular reference with an <see cref="Spring.Objects.Factory.IFactoryObject"/>
	/// cannot be solved by eagerly caching singleton instances (as is the
	/// case with normal objects. The reason is that every
	/// <see cref="Spring.Objects.Factory.IFactoryObject"/> needs to be fully
	/// initialized before it can return the created object, while only specific
	/// normal objects need to be initialized - that is, if a collaborating object
	/// actually invokes them on initialization instead of just storing the reference.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	[Serializable]
	public class FactoryObjectNotInitializedException : ObjectCreationException
	{
		/// <summary>
		/// Creates a new instance of the
		/// FactoryObjectNotInitializedException class.
		/// </summary>
		public FactoryObjectNotInitializedException()
		{
		}

		/// <summary>
		/// Creates a new instance of the FactoryObjectNotInitializedException class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public FactoryObjectNotInitializedException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Creates a new instance of the FactoryObjectNotInitializedException class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
		public FactoryObjectNotInitializedException(string message, Exception rootCause)
			: base(message, rootCause)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// FactoryObjectCircularReferenceException class.
		/// </summary>
        /// <param name="objectName">
		/// The name of the object that triggered the exception.
		/// </param>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public FactoryObjectNotInitializedException(string objectName, string message)
            : base(objectName, StringUtils.HasText(message) ? message :
				"Circular dependency chain detected for factory object.")
		{
		}

		/// <summary>
		/// Creates a new instance of the FactoryObjectCircularReferenceException class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
		protected FactoryObjectNotInitializedException(
			SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
