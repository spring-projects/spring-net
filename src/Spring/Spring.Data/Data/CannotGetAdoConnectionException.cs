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
using Spring.Dao;

namespace Spring.Data
{
	/// <summary>
    /// Fatal exception thrown when we can't connect to an RDBMS using ADO.NET
    /// </summary>
    /// <author>Rod Johnson</author>
	/// <author>Mark Pollack (.NET)</author>
	[Serializable]
	public class CannotGetAdoConnectionException : InvalidDataAccessResourceUsageException
	{

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="CannotGetAdoConnectionException"/> class.
        /// </summary>
		public CannotGetAdoConnectionException()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="CannotGetAdoConnectionException"/> class.
        /// </summary>
        /// <param name="message">A message about the exception.</param>
        public CannotGetAdoConnectionException(string message): base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CannotGetAdoConnectionException"/> class.
        /// </summary>
        /// <param name="message">A message about the exception.</param>
        /// <param name="inner">The inner exception.</param>
        public CannotGetAdoConnectionException(string message, Exception inner)
            : base(message, inner)
        {
        }

		/// <inheritdoc />
        protected CannotGetAdoConnectionException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		#endregion

    }
}
