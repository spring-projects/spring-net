#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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
    /// Data access exception thrown when a result was not of the expected size,
    /// for example when expecting a single row but getting 0 or more than 1 rows.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	/// <author>Juergen Hoeller</author>
    [Serializable]
    public class EmptyResultDataAccessException : IncorrectResultSizeDataAccessException
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyResultDataAccessException"/> class.
        /// </summary>
        public EmptyResultDataAccessException() : base ()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyResultDataAccessException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public EmptyResultDataAccessException(string message) : base (message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyResultDataAccessException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public EmptyResultDataAccessException(string message, Exception innerException) : base (message, innerException)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Dao.EmptyResultDataAccessException"/> class.
        /// </summary>
        /// <param name="expectedSize">The expected size.</param>
        public EmptyResultDataAccessException(int expectedSize) : base (expectedSize, 0)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Dao.EmptyResultDataAccessException"/> class.
        /// </summary>
        /// <param name="message">A message about the exception.</param>
        /// <param name="expectedSize">The expected size.</param>
        public EmptyResultDataAccessException( string message, int expectedSize ) : base( message, expectedSize, 0 ) {}


        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyResultDataAccessException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is <see langword="null"/>.</exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is <see langword="null"/> or <see cref="P:System.Exception.HResult"/> is zero (0).</exception>
        protected EmptyResultDataAccessException( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
        }
    }
}
