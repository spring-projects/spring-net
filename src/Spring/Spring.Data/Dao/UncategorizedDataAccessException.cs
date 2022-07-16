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
	/// Normal superclass when we can't distinguish anything more specific
	/// than "something went wrong with the underlying resource": for example,
	/// a SQLException from Sql Server that we can't pinpoint more precisely.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
    public abstract class UncategorizedDataAccessException : NonTransientDataAccessException
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.UncategorizedDataAccessException"/> class.
		/// </summary>
		public UncategorizedDataAccessException() {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.UncategorizedDataAccessException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public UncategorizedDataAccessException( string message ) : base( message ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.UncategorizedDataAccessException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception (from the underlying data access API, such as ADO.NET).
		/// </param>
		public UncategorizedDataAccessException( string message, Exception rootCause)
			: base( message , rootCause ) {}

		/// <inheritdoc />
		protected UncategorizedDataAccessException(
			SerializationInfo info, StreamingContext context ) : base( info, context ) {}
	}
}
