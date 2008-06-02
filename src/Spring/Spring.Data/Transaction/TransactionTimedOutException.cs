using System;
using System.Runtime.Serialization;

namespace Spring.Transaction
{
	/// <summary>
	/// Exception to be thrown when a transaction has timed out.
	/// </summary>
    [Serializable]
	public class TransactionTimedOutException : TransactionException
	{
        /// <summary>
        /// Create a new instance
        /// </summary>
        public TransactionTimedOutException() : base()
        {
            
        }
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Transaction.TransactionTimedOutException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public TransactionTimedOutException( String message ) : base(message) {}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Transaction.TransactionTimedOutException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public TransactionTimedOutException(string message, Exception rootCause)
            : base(message, rootCause) {}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Transaction.TransactionTimedOutException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected TransactionTimedOutException(
            SerializationInfo info, StreamingContext context ) : base( info, context ) {}
	}
}
