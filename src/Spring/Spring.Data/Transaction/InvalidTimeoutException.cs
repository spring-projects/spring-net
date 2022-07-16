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
	/// Exception that gets thrown when an invalid timeout is specified,
	/// for example when the transaction manager implementation doesn't support timeouts.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class InvalidTimeoutException : TransactionUsageException, ISerializable
	{
		/// <summary>
		/// Invalid timeout value.
		/// </summary>
		private int _timeout = -1;

		/// <summary>
		/// Returns the invalid timeout for this exception.
		/// </summary>
		public int Timeout
		{
			get
			{
				return _timeout;
			}
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.InvalidTimeoutException"/> class
		/// with the specified message and timeout value.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="timeout">The (possibly invalid) timeout value.</param>
		public InvalidTimeoutException(string message, int timeout):base(message)
		{
			_timeout = timeout;
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.InvalidTimeoutException"/> class.
		/// </summary>
		public InvalidTimeoutException( ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.InvalidTimeoutException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public InvalidTimeoutException( String message ) : base(message) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.InvalidTimeoutException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
		public InvalidTimeoutException(string message, Exception rootCause)
			: base(message, rootCause) {}

		/// <inheritdoc />
		protected InvalidTimeoutException(
			SerializationInfo info, StreamingContext context ) : base( info, context )
		{
			_timeout = info.GetInt32( "timeout" );
		}

		/// <inheritdoc />
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue( "timeout", _timeout );
			base.GetObjectData( info, context );
		}
	}
}
