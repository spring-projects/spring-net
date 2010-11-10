using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

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
    }
}
