#region License

/*
 * Copyright 2002-2004 the original author or authors.
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

using System;
using System.Runtime.Serialization;

#endregion

namespace Spring.Dao
{
	/// <summary> 
	/// Exception thrown on a pessimistic locking violation.
	/// </summary>
	/// <remarks>
	/// <para>Serves as a superclass for more specific exceptions, like 
	/// CannotAcquireLockException and DeadlockLoserDataAccessException
	/// </para>
	/// <para>
	/// This exception will be thrown either by O/R mapping tools or by custom DAO
	/// implementations.
	/// </para>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Mark Pollack (.NET)</author>
	[Serializable]
	public class PessimisticLockingFailureException : ConcurrencyFailureException
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.PessimisticLockingFailureException"/> class.
		/// </summary>
		public PessimisticLockingFailureException() {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.PessimisticLockingFailureException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public PessimisticLockingFailureException( string message ) : base( message ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.PessimisticLockingFailureException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception (from the underlying data access API, such as ADO.NET).
		/// </param>
		public PessimisticLockingFailureException( string message, Exception rootCause)
			: base( message , rootCause ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.CannotAcquireLockException"/> class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
        protected PessimisticLockingFailureException(
			SerializationInfo info, StreamingContext context ) : base( info, context ) {}
	}
}