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

namespace Spring.Context
{
	/// <summary>
	/// Thrown when a message cannot be resolved.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Mark Pollack (.NET)</author>
	/// <seealso cref="Spring.Context.IMessageSource"/>
	/// <seealso cref="Spring.Context.IMessageSourceResolvable"/>
	[Serializable]
	public class NoSuchMessageException : ApplicationException
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Context.NoSuchMessageException"/> class.
		/// </summary>
		public NoSuchMessageException()
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Context.NoSuchMessageException"/> class with the
		/// specified message.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
		public NoSuchMessageException(string message, Exception rootCause)
			: base(message, rootCause)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Context.NoSuchMessageException"/> class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being
		/// thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or
		/// destination.
		/// </param>
		protected NoSuchMessageException(
			SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Context.NoSuchMessageException"/> class.
		/// </summary>
		/// <param name="code">
		/// The code that could not be resolved for given culture.
		/// </param>
		/// <param name="culture">
		/// The <see cref="System.Globalization.CultureInfo"/> that was used
		/// to search for the code.
		/// </param>
		public NoSuchMessageException(string code, CultureInfo culture)
			: base(string.Format("No message found under code '{0}' for locale '{1}'.",
			code, culture))
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Context.NoSuchMessageException"/> class.
		/// </summary>
		/// <param name="code">
		/// The code that could not be resolved for the current UI culture.
		/// </param>
		public NoSuchMessageException(string code)
			: this(code, CultureInfo.CurrentUICulture)
		{
		}
	}
}
