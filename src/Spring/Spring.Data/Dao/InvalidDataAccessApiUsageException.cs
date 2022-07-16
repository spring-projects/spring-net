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
	/// Exception thrown on incorrect usage of the API, such as failing to "compile" a query
	/// object that needed compilation before execution.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This represents a problem in our data access framework, not the underlying data access
	/// infrastructure.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
    public class InvalidDataAccessApiUsageException : NonTransientDataAccessException
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.InvalidDataAccessApiUsageException"/> class.
		/// </summary>
		public InvalidDataAccessApiUsageException() {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.InvalidDataAccessApiUsageException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public InvalidDataAccessApiUsageException( string message ) : base( message ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.InvalidDataAccessApiUsageException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception (from the underlying data access API, such as ADO.NET).
		/// </param>
		public InvalidDataAccessApiUsageException( string message, Exception rootCause)
			: base( message , rootCause ) {}

		/// <inheritdoc />
		protected InvalidDataAccessApiUsageException(
			SerializationInfo info, StreamingContext context ) : base( info, context ) {}
	}
}
