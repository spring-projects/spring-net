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
    /// Exception thrown when we can't classify a SQLException into
    /// one of our generic data access exceptions.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	[Serializable]
	public class UncategorizedAdoException : UncategorizedDataAccessException, ISerializable
	{
		#region Fields

        /// <summary>
        /// SQL that led to the problem
        /// </summary>
        private readonly string sql;

	    /// <summary>
	    /// The error code, if available, that was unable to be categorized.
	    /// </summary>
        private readonly string errorCode;

		#endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="UncategorizedAdoException"/> class.
        /// </summary>
		public UncategorizedAdoException()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="UncategorizedAdoException"/> class.
        /// </summary>
        /// <param name="message">A message about the exception.</param>
        public UncategorizedAdoException(string message): base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UncategorizedAdoException"/> class.
        /// </summary>
        /// <param name="message">A message about the exception.</param>
        /// <param name="inner">The inner exception.</param>
        public UncategorizedAdoException(string message, Exception inner): base(message, inner)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="UncategorizedAdoException"/> class.
        /// </summary>
        /// <param name="task">name of the current task.</param>
        /// <param name="errorCode">the underlying error code that could not be translated</param>
        /// <param name="sql">The offending SQL statment</param>
        /// <param name="ex">The root cause.</param>
        public UncategorizedAdoException(string task, string sql, string errorCode, Exception ex) : base(task + "; uncategorized DataException for SQL [" + sql + "]; " + "ErrorCode [" + errorCode + "]; " + ex.Message, ex)
        {
             this.sql = sql;
             this.errorCode = errorCode;
        }

		/// <inheritdoc />
        protected UncategorizedAdoException( SerializationInfo info, StreamingContext context ) : base( info, context ) {}

		#endregion

		#region Properties

        /// <summary>
        /// Return the underlying error code if available from the underlying provider.
        /// </summary>
	    public string ErrorCode
	    {
	        get { return errorCode; }
	    }

        /// <summary>
        /// Return the SQL that resulted in this exception.
        /// </summary>
	    public string Sql
	    {
	        get { return sql; }
	    }

	    #endregion

		#region Methods

		#endregion

        #region ISerializable Members

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/>
        /// with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference (<see langword="Nothing"/> in Visual Basic).</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue( "sql", sql );
            info.AddValue( "errorCode", errorCode);
            base.GetObjectData( info, context );
        }

        #endregion
    }
}
