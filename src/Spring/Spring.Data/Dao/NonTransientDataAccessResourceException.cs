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
    /// Data access exception thrown when a resource fails completely and the failure is permanent.
    /// </summary>
    /// <author>Thomas Risberg</author>
    /// <author>Mark Pollack (.NET)</author>
	[Serializable]
    public abstract class NonTransientDataAccessResourceException : NonTransientDataAccessException
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.NonTransientDataAccessResourceException"/> class.
		/// </summary>
		public NonTransientDataAccessResourceException() : base("No Exception Message") {}

		/// <summary>
		/// Creates a new instance of the
        /// <see cref="Spring.Dao.NonTransientDataAccessResourceException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public NonTransientDataAccessResourceException( string message ) : base( message ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.NonTransientDataAccessResourceException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception (from the underlying data access API, such as ADO.NET).
		/// </param>
		public NonTransientDataAccessResourceException( string message, Exception rootCause)
			: base( message , rootCause ) {}

		/// <summary>
		/// Creates a new instance of the
        /// <see cref="Spring.Dao.NonTransientDataAccessResourceException"/> class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
        protected NonTransientDataAccessResourceException(
			SerializationInfo info, StreamingContext context ) : base( info, context ) {}
	}
}