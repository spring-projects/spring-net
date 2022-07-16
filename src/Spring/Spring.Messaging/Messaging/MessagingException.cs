using System.Runtime.Serialization;

namespace Spring.Messaging
{
    /// <summary>
    /// Base exception class for exceptions thrown by Spring in Spring.Messaging
    /// </summary>
    /// <author>Mark Pollack</author>
    [Serializable]
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

        /// <inheritdoc />
        protected MessagingException(
            SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
