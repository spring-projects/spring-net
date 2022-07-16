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
	/// Exception thrown when an operation is attempted that relies on an existing
	/// transaction (such as setting rollback status) and there is no existing transaction.
	/// This represents an illegal usage of the transaction API.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class NoTransactionException : TransactionUsageException
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.NoTransactionException"/> class.
		/// </summary>
		public NoTransactionException( ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.NoTransactionException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public NoTransactionException( String message ) : base(message) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.NoTransactionException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
		public NoTransactionException(string message, Exception rootCause)
			: base(message, rootCause) {}

		/// <inheritdoc />
		protected NoTransactionException(
			SerializationInfo info, StreamingContext context ) : base( info, context ) {}
	}
}
