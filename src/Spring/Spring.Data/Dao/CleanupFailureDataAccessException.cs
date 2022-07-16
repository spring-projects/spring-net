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

namespace Spring.Dao
{
	/// <summary>
	/// Exception thrown when we couldn't cleanup after a data access operation,
	/// but the actual operation went OK.
	/// </summary>
	/// <remarks>
	/// <p>
	/// For example, this exception or a subclass might be thrown if an ADO.NET
	/// connection couldn't be closed after it had been used successfully.
	/// </p>
	/// <p>
	/// Note that data access code might perform resource cleanup in a
	/// finally block and therefore log cleanup failure rather than rethrow it,
	/// to keep the original data access exception, if any.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
    public class CleanupFailureDataAccessException : NonTransientDataAccessException
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.CleanupFailureDataAccessException"/> class.
		/// </summary>
		public CleanupFailureDataAccessException() {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.CleanupFailureDataAccessException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public CleanupFailureDataAccessException( string message ) : base( message ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.CleanupFailureDataAccessException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception (from the underlying data access API, such as ADO.NET).
		/// </param>
		public CleanupFailureDataAccessException( string message, Exception rootCause)
			: base( message , rootCause ) {}

		/// <inheritdoc />
		protected CleanupFailureDataAccessException(
			SerializationInfo info, StreamingContext context ) : base( info, context ) {}
	}
}
