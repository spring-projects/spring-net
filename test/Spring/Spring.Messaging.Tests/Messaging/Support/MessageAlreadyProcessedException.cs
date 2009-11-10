using System;
using System.Runtime.Serialization;

namespace Spring.Messaging
{
    /// <summary>
    /// Indicates that the received message has already been processed. Will typically be used
    /// in an IMessageTransactionExceptionHandler to commit (i.e. remove from the queue) a 
    /// message that has been processed by the database but not acknowledged to MSMQ
    /// due to an application failure.
    /// </summary>
    [Serializable]
    public class MessageAlreadyProcessedException : MessagingException
    {
        #region Constructor (s) / Destructor

        /// <summary>Creates a new instance of the MessageAlreadyProcessedException class.</summary>
        public MessageAlreadyProcessedException()
        {
        }

        /// <summary>
        /// Creates a new instance of the MessageAlreadyProcessedException class. with the specified message.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public MessageAlreadyProcessedException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the MessageAlreadyProcessedException class with the specified message
        /// and root cause.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public MessageAlreadyProcessedException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the MessageAlreadyProcessedException class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected MessageAlreadyProcessedException(
            SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}