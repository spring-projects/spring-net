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
	/// Exception thrown on an optimistic locking violation.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This exception will be thrown either by O/R mapping tools or by custom DAO
	/// implementations.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class OptimisticLockingFailureException : ConcurrencyFailureException
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.OptimisticLockingFailureException"/> class.
		/// </summary>
		public OptimisticLockingFailureException() {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.OptimisticLockingFailureException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public OptimisticLockingFailureException( string message ) : base( message ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.OptimisticLockingFailureException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception (from the underlying data access API, such as ADO.NET).
		/// </param>
		public OptimisticLockingFailureException( string message, Exception rootCause)
			: base( message , rootCause ) {}

		/// <inheritdoc />
		protected OptimisticLockingFailureException(
			SerializationInfo info, StreamingContext context ) : base( info, context ) {}
	}
}
