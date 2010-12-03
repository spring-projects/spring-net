#if NET_2_0


using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using Spring.Core.IO;

namespace Spring.Objects.Factory.Parsing
{
    [Serializable]
    public class ObjectDefinitionParsingException : ObjectDefinitionStoreException
    {
        protected ObjectDefinitionParsingException(SerializationInfo info, StreamingContext context)
        {
        }
        /// <summary>
        /// Initializes a new instance of the ObjectDefinitionParsingException class.
        /// </summary>
        public ObjectDefinitionParsingException(Problem problem)
            : base(problem.Location.Resource, problem.ResourceDescription, problem.Message)
        {

        }
        /// <summary>
        /// Creates a new instance of the ObjectDefinitionParsingException class.
        /// </summary>
        public ObjectDefinitionParsingException()
        {

        }
        /// <summary>
        /// Creates a new instance of the ObjectDefinitionParsingException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public ObjectDefinitionParsingException(string message)
            : base(message)
        {
            
        }
        /// <summary>
        /// Creates a new instance of the ObjectDefinitionParsingException class.
        /// </summary>
        /// <param name="resourceDescription">
        /// The description of the resource that the object definition came from
        /// </param>
        /// <param name="name">
        /// The name of the object that triggered the exception.
        /// </param>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public ObjectDefinitionParsingException(string resourceDescription, string name, string message)
            : base(resourceDescription, name, message)
        {
            
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDefinitionParsingException"></see> class.
        /// </summary>
        /// <param name="resourceDescription">
        /// The description of the resource that the object definition came from
        /// </param>
        /// <param name="msg">The detail message (used as exception message as-is)</param>
        /// <param name="cause">The root cause. (may be <code>null</code></param>
        public ObjectDefinitionParsingException(string resourceDescription, string msg, Exception cause)
            : base(resourceDescription, msg, cause)
        {
            
        }
        /// <summary>
        /// Creates a new instance of the ObjectDefinitionParsingException class.
        /// </summary>
        /// <param name="resourceLocation">
        /// The resource location (e.g. an XML object definition file) associated
        /// with the offending object definition.
        /// </param>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="name">
        /// The name of the object that triggered the exception.
        /// </param>
        public ObjectDefinitionParsingException(IResource resourceLocation, string name, string message)
            : base(resourceLocation, name, message)
        {
            
        }
        /// <summary>
        /// Creates a new instance of the ObjectDefinitionParsingException class.
        /// </summary>
        /// <param name="resourceLocation">
        /// The resource location (e.g. an XML object definition file) associated
        /// with the offending object definition.
        /// </param>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="name">
        /// The name of the object that triggered the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public ObjectDefinitionParsingException(IResource resourceLocation, string name, string message, Exception rootCause)
            : base(resourceLocation, name, message, rootCause)
        {
            
        }
        /// <summary>
        /// Creates a new instance of the ObjectDefinitionParsingException class.
        /// </summary>
        /// <param name="resourceDescription">
        /// The description of the resource that the object definition came from
        /// </param>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="name">
        /// The name of the object that triggered the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public ObjectDefinitionParsingException(string resourceDescription, string name, string message, Exception rootCause)
            : base(resourceDescription, name, message, rootCause)
        {
            
        }
        /// <summary>
        /// Creates a new instance of the ObjectDefinitionParsingException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public ObjectDefinitionParsingException(string message, Exception rootCause)
            : base(message, rootCause)
        {
            
        }

    }
}


#endif