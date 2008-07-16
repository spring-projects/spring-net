using System;
using System.Runtime.Serialization;

namespace Spring.Messaging
{
    public class MessagingException : ApplicationException
    {
        #region Constructor (s) / Destructor

        /// <summary>Creates a new instance of the MessagingException class.</summary>
        public MessagingException()
        {
        }

        /// <summary>
        /// Creates a new instance of the MessagingException class. with the specified message.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public MessagingException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the MessagingException class with the specified message
        /// and root cause.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public MessagingException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the MessagingException class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected MessagingException(
            SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}