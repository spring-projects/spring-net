#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Runtime.Serialization;

namespace Spring.Transaction
{
	/// <summary>
	/// Exception thrown when attempting to suspend an existing transaction
	/// but transaction suspension is not supported by the underlying backend.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class TransactionSuspensionNotSupportedException : CannotCreateTransactionException
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.TransactionSuspensionNotSupportedException"/> class.
		/// </summary>
		public TransactionSuspensionNotSupportedException( ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.TransactionSuspensionNotSupportedException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public TransactionSuspensionNotSupportedException( String message ) : base(message) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.TransactionSuspensionNotSupportedException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
		public TransactionSuspensionNotSupportedException(string message, Exception rootCause)
			: base(message, rootCause) {}

		/// <inheritdoc />
		protected TransactionSuspensionNotSupportedException(
			SerializationInfo info, StreamingContext context ) : base( info, context ) {}
	}
}