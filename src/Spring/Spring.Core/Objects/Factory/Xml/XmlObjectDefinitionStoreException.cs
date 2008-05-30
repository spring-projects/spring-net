

using System;
using System.Runtime.Serialization;
using System.Xml;

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// XML-specific ObjectDefinitionStoreException subclass that wraps a XmlException, which
    /// contains information about the error location.
    /// </summary>
    [Serializable]
    public class XmlObjectDefinitionStoreException : ObjectDefinitionStoreException
    {

        #region Required for Standards Compliance
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlObjectDefinitionStoreException"/> class.
        /// </summary>
        public XmlObjectDefinitionStoreException()
        {
        }

        /// <summary>
		/// Creates a new instance of the XmlObjectDefinitionStoreException class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
        public XmlObjectDefinitionStoreException(string message)
			: base(message)
		{
        }

        /// <summary>
		/// Creates a new instance of the XmlObjectDefinitionStoreException class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
        public XmlObjectDefinitionStoreException(string message, Exception rootCause)
			: base(message, rootCause)
		{
		}

        /// <summary>
		/// Creates a new instance of the XmlObjectDefinitionStoreException class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
        protected XmlObjectDefinitionStoreException(
		    SerializationInfo info, StreamingContext context)
			: base(info, context)
       	{
       	}

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlObjectDefinitionStoreException"/> class.
        /// </summary>
        /// <param name="resourceDescription">The description of the resource that the object definition came from</param>
        /// <param name="msg">The detail message (used as exception message as-is).</param>
        /// <param name="cause">The XmlException root cause.</param>
        public XmlObjectDefinitionStoreException(string resourceDescription, string msg, XmlException cause)
            : base(resourceDescription, msg, cause)
        {
            
        }

        /// <summary>
        /// Gets the line number in the XML resource that failed.
        /// </summary>
        /// <value>The line number if available (in case of a XmlException); -1 else.</value>
        public int LineNumber
        {
            get
            {
                XmlException cause = InnerException as XmlException;
                if (cause != null)
                {
                    return (cause.LineNumber);
                }
                else
                {
                    return -1;
                }
            }
        }

		/// <summary>
		/// Gets the line position in the XML resource that failed.
		/// </summary>
        /// <value>The line position if available (in case of a XmlException); -1 else.</value>
		public int LinePosition
        {
            get
            {
                XmlException cause = InnerException as XmlException;
                if (cause != null)
                {
                    return (cause.LinePosition);
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}