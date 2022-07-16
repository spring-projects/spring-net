#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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

using Spring.Core.IO;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Supports providing one or more <see cref="IResource"/> implementations to import when creating <see cref="RootObjectDefinition"/>s.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
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
