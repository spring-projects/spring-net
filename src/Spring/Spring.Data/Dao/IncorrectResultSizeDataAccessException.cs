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
	/// Data access exception thrown when a result was not of the expected size,
	/// for example when expecting a single row but getting 0 or more than 1 rows.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class IncorrectResultSizeDataAccessException : InvalidDataAccessApiUsageException, ISerializable
	{
		private int _expectedSize;
		private int _actualSize;

		/// <summary>Return the expected result size.</summary>
		public virtual int ExpectedSize
		{
			get
			{
				return _expectedSize;
			}
		}

		/// <summary>Return the actual result size (or -1 if unknown).</summary>
		public virtual int ActualSize
		{
			get
			{
				return _actualSize;
			}
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.IncorrectResultSizeDataAccessException"/> class.
		/// </summary>
		public IncorrectResultSizeDataAccessException() : this( -1, -1 ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.IncorrectResultSizeDataAccessException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public IncorrectResultSizeDataAccessException( string message ) : this( message, -1, -1 )
		{
		}

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
		public IncorrectResultSizeDataAccessException( string message, Exception rootCause )
			: base( message, rootCause )
		{
			_expectedSize = -1;
			_actualSize = -1;
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.CleanupFailureDataAccessException"/> class.
		/// </summary>
		/// <param name="expectedSize">The expected result size.</param>
		/// <param name="actualSize">The actual result size (or -1 if unknown).</param>
		public IncorrectResultSizeDataAccessException( int expectedSize, int actualSize )
			: this ( "Incorrect result size: expected " + expectedSize + ", actual " + actualSize, expectedSize, actualSize)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Dao.CleanupFailureDataAccessException"/> class.
		/// </summary>>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="expectedSize">The expected result size.</param>
		/// <param name="actualSize">The actual result size (or -1 if unknown).</param>
		public IncorrectResultSizeDataAccessException( string message, int expectedSize, int actualSize )
			: base( message )
		{
			this._expectedSize = expectedSize;
			this._actualSize = actualSize;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="IncorrectResultSizeDataAccessException"/> class.
        /// </summary>
        /// <param name="message">A message about the exception</param>
        /// <param name="expectedSize">The expected result size.</param>
        public IncorrectResultSizeDataAccessException(string message, int expectedSize)
            : base(message)
        {
            this._expectedSize = expectedSize;
            this._actualSize = -1;
        }

		/// <inheritdoc />
		protected IncorrectResultSizeDataAccessException( SerializationInfo info, StreamingContext context ) : base( info, context )
		{
			_expectedSize = info.GetInt32( "expectedSize" );
			_actualSize = info.GetInt32( "actualSize" );
		}

		/// <inheritdoc />
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue( "expectedSize", _expectedSize );
			info.AddValue( "actualSize", _actualSize );
			base.GetObjectData( info, context );
		}
	}
}
