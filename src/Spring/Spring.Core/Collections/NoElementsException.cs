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

namespace Spring.Collections
{
	/// <summary>
	/// Thrown when an element is requested from an empty <see cref="IQueue"/>.
	/// </summary>
	/// <author>Griffin Caprio</author>
	[Serializable]
	public class NoElementsException : ApplicationException
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="NoElementsException"/> class.
		/// </summary>
		public NoElementsException()
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="NoElementsException"/> class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
		protected NoElementsException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="NoElementsException"/> class with the
		/// specified message.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public NoElementsException(string message) : base(message)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="NoElementsException"/> class with the
		/// specified message.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
		public NoElementsException(string message, Exception rootCause)
			: base(message, rootCause)
		{
		}
	}
}
