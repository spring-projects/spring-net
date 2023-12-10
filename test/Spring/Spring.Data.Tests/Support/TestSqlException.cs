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

using System;
using System.Runtime.Serialization;

#pragma warning disable CS0672 // Member overrides obsolete member
#pragma warning disable SYSLIB0050
#pragma warning disable SYSLIB0051

namespace Spring.Support
{
	/// <summary>
	/// A simple exception class to test exception translation functionality 
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
    [Serializable] 
    public class TestSqlException : Exception
    {


        private string errorNumber;
        
        public TestSqlException() {}
        
        
        public TestSqlException( string message ) : base( message ) {}
        
        public TestSqlException(string message, string errorNumber) :base (message)
        {
            ErrorNumber = errorNumber;
        }

        public TestSqlException( string message, Exception rootCause)
            : base( message , rootCause ) {}

        protected TestSqlException(
            SerializationInfo info, StreamingContext context ) : base( info, context )
        {
            errorNumber = info.GetString( "errorNumber" );
        }
        
        public string ErrorNumber
        {
            get { return errorNumber; }
            set { errorNumber = value; }
        }
        /// <summary>
        /// Override of <see cref="System.Exception.GetObjectData(SerializationInfo, StreamingContext)"/>
        /// to allow for private serialization.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue( "errorNumber", errorNumber );
            base.GetObjectData( info, context );
        }
    }
}
