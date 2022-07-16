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
	/// Data access exception thrown when something unintended appears to have
	/// happened with an update, but the transaction hasn't already been rolled back.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Thrown, for example, when we wanted to update 1 row in an RDBMS but actually
	/// updated 3.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public abstract class IncorrectUpdateSemanticsDataAccessException
		: InvalidDataAccessResourceUsageException
	{
		/// <summary>Return whether or not data was updated.</summary>
		/// <returns>
		/// <b>True</b> if data was updated (as opposed to being incorrectly
		/// updated). If this property returns false, there's nothing to roll back.
		/// </returns>
		public abstract bool DataWasUpdated { get; }

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.IncorrectUpdateSemanticsDataAccessException"/> class.
		/// </summary>
		protected IncorrectUpdateSemanticsDataAccessException() {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.IncorrectUpdateSemanticsDataAccessException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		protected IncorrectUpdateSemanticsDataAccessException( string message ) : base( message ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.IncorrectUpdateSemanticsDataAccessException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception (from the underlying data access API, such as ADO.NET).
		/// </param>
		protected IncorrectUpdateSemanticsDataAccessException( string message, Exception rootCause)
			: base( message , rootCause ) {}

		/// <inheritdoc />
		protected IncorrectUpdateSemanticsDataAccessException(
			SerializationInfo info, StreamingContext context ) : base( info, context ) {}
	}
}
