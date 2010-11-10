using System;
using System.Collections.Generic;
using System.Text;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;

namespace Spring.Context.Annotation
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ImportResourceAttribute : Attribute
    {
        private IObjectDefinitionReader _objectDefinitionReader;

        private string[] _resources;

        /// <summary>
        /// Initializes a new instance of the ImportResource class.
        /// </summary>
        /// <param name="objectDefinitionReader"></param>
        /// <param name="resources"></param>
        public ImportResourceAttribute(IObjectDefinitionReader objectDefinitionReader, string[] resources)
        {
            _objectDefinitionReader = objectDefinitionReader;
            _resources = resources;
        }

        /// <summary>
        /// <see cref="IObjectDefinitionReader"/> implementation to use when processing resources specified
        /// by the <see cref="Resources"/> attribute.
        /// </summary>
        /// <value>The <see cref="IObjectDefinitionReader"/>.</value>
        public IObjectDefinitionReader DefinitionReader
        {
            get { return _objectDefinitionReader; }
            set
            {
                _objectDefinitionReader = value;
            }
        }

        /// <summary>
        /// Resource paths to import.  Resource-loading prefixes such as <code>assembly://</code> and
        /// <code>file://</code>, etc may be used.
        /// </summary>
        /// <value>The resources.</value>
        public string[] Resources
        {
            get { return _resources; }
            set
            {
                _resources = value;
            }
        }

    }
}
