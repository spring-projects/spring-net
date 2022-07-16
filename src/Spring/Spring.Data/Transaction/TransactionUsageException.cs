#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

namespace Spring.Transaction
{
	/// <summary>
	/// Superclass for exceptions caused by inappropriate usage of
	/// a <a href="http://www.springframework.net/">Spring.NET</a> transaction API.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class TransactionUsageException : TransactionException
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.TransactionUsageException"/> class.
		/// </summary>
		public TransactionUsageException( ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.TransactionUsageException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public TransactionUsageException( String message ) : base(message) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.TransactionUsageException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
		public TransactionUsageException(string message, Exception rootCause)
			: base(message, rootCause) {}

		/// <inheritdoc />
		protected TransactionUsageException(
			SerializationInfo info, StreamingContext context ) : base( info, context ) {}
	}
}
