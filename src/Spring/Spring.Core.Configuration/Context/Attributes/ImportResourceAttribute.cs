using System;
using System.Collections.Generic;
using System.Text;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ImportResourceAttribute : Attribute
    {
        private Type _objectDefinitionReader = typeof(XmlObjectDefinitionReader);

        private string[] _resources;

        /// <summary>
        /// Initializes a new instance of the ImportResourceAttribute class.
        /// </summary>
        /// <param name="resources"></param>
        public ImportResourceAttribute(string[] resources)
        {
            if (resources ==null || resources.Length ==0)
                throw new ArgumentException("resources cannot be null or empty!");

            _resources = resources;
        }


        /// <summary>
        /// Initializes a new instance of the ImportResourceAttribute class.
        /// </summary>
        /// <param name="resource"></param>
        public ImportResourceAttribute(string resource)
        {
            if (StringUtils.IsNullOrEmpty(resource))
                throw new ArgumentException("resource cannot be null or empty!");
            	
            _resources = new[] { resource };
        }

        /// <summary>
        /// <see cref="IObjectDefinitionReader"/> implementation to use when processing resources specified
        /// by the <see cref="Resources"/> attribute.
        /// </summary>
        /// <value>The <see cref="IObjectDefinitionReader"/>.</value>
        public Type DefinitionReader
        {
            get
            {
                return _objectDefinitionReader;
            }
            set
            {
                if (!((typeof(IObjectDefinitionReader).IsAssignableFrom(value))))
                    throw new ArgumentException(string.Format("DefinitionReader must be of type IObjectDefinitionReader but was of type {0}", value.Name));
                
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
