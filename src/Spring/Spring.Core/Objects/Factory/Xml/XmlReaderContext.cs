#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using System.Xml;
using Spring.Core.IO;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Parsing;
using Spring.Objects.Factory.Support;
using Spring.Util;

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Extension of <see cref="ReaderContext"/> specific to use with an XmlObjectDefinitionReader.
    /// Provides access to <see cref="NamespaceParserResolver"/> configured in <see cref="XmlObjectDefinitionReader"/>
    /// </summary>
    public class XmlReaderContext : ReaderContext
    {
        private readonly IObjectDefinitionReader reader;
        private readonly IObjectDefinitionFactory objectDefinitionFactory;
        private INamespaceParserResolver namespaceParserResolver;

        /// <summary>
        /// The maximum length of any XML fragment displayed in the error message
        /// reporting.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Hopefully this will display enough context so that a user
        /// can pinpoint the cause of the error.
        /// </p>
        /// </remarks>
        private const int MaxXmlErrorFragmentLength = 255;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlReaderContext"/> class.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="reader">The reader.</param>
        public XmlReaderContext(IResource resource, IObjectDefinitionReader reader)
            : this(resource, reader, new DefaultObjectDefinitionFactory())
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlReaderContext"/> class.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="reader">The reader.</param>
        /// <param name="objectDefinitionFactory">The factory to use for creating new <see cref="IObjectDefinition"/> instances.</param>
        internal XmlReaderContext(IResource resource, IObjectDefinitionReader reader, IObjectDefinitionFactory objectDefinitionFactory)
            : base(resource)
        {
            this.reader = reader;
            if (reader is XmlObjectDefinitionReader)
            {
                this.namespaceParserResolver = ((XmlObjectDefinitionReader) reader).NamespaceParserResolver;
            }
            this.objectDefinitionFactory = objectDefinitionFactory;
        }

        /// <summary>
        /// Gets the reader.
        /// </summary>
        /// <value>The reader.</value>
        public IObjectDefinitionReader Reader
        {
            get { return reader; }
        }

        /// <summary>
        /// Gets the resource loader.
        /// </summary>
        /// <value>The resource loader.</value>
        public IResourceLoader ResourceLoader
        {
            get { return reader.ResourceLoader;  }
        }

        /// <summary>
        /// Gets the registry.
        /// </summary>
        /// <value>The registry.</value>
        public IObjectDefinitionRegistry Registry
        {
            get
            {
                return reader.Registry;
            }
        }

        /// <summary>
        /// Gets or sets the object definition factory.
        /// </summary>
        /// <value>The object definition factory.</value>
        public IObjectDefinitionFactory ObjectDefinitionFactory
        {
            get { return objectDefinitionFactory; }
        }

        /// <summary>
        /// Get the <see cref="INamespaceParserResolver"/> instance to lookup parsers for custom namespaces.
        /// </summary>
        internal INamespaceParserResolver NamespaceParserResolver
        {
            get { return namespaceParserResolver; }
            set { namespaceParserResolver = value; }
        }

        /// <summary>
        /// Generates the name of the object.
        /// </summary>
        /// <param name="objectDefinition">The object definition.</param>
        /// <returns>the generated object name</returns>
        public string GenerateObjectName(IObjectDefinition objectDefinition)
        {
            return reader.ObjectNameGenerator.GenerateObjectName(objectDefinition, Registry);
        }

        /// <summary>
        /// Registers the name of the with generated.
        /// </summary>
        /// <param name="objectDefinition">The object definition.</param>
        /// <returns>the generated object name</returns>
        public string RegisterWithGeneratedName(IObjectDefinition objectDefinition)
        {
            string generatedName = GenerateObjectName(objectDefinition);
            Registry.RegisterObjectDefinition(generatedName, objectDefinition);
            return generatedName;
        }

        /// <summary>
        /// Reports a parse error by loading a
        /// <see cref="Spring.Objects.ObjectsException"/> with helpful contextual
        /// information and throwing said exception.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Derived classes can of course override this method in order to implement
        /// validators capable of displaying a full list of errors found in the
        /// definition.
        /// </p>
        /// </remarks>
        /// <param name="node">
        /// The node that triggered the parse error.
        /// </param>
        /// <param name="name">
        /// The name of the object that triggered the exception.
        /// </param>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// <b>Always</b> throws an instance of this exception class, that will
        /// contain helpful contextual infomation about the parse error.
        /// </exception>
        /// <seealso cref="ReportException(XmlNode,string,string,Exception)"/>
        public void ReportException(XmlNode node, string name, string message)
        {
            ReportException(node, name, message, null);
        }

        /// <summary>
        /// Reports a parse error by loading a
        /// <see cref="Spring.Objects.ObjectsException"/> with helpful contextual
        /// information and throwing said exception.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Derived classes can of course override this method in order to implement
        /// validators capable of displaying a full list of errors found in the
        /// definition.
        /// </p>
        /// </remarks>
        /// <param name="node">
        /// The node that triggered the parse error.
        /// </param>
        /// <param name="name">
        /// The name of the object that triggered the exception.
        /// </param>
        /// <param name="message">
        /// A message about the error.
        /// </param>
        /// <param name="cause">
        /// The root cause of the parse error (if any - may be <see langword="null"/>).
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// <b>Always</b> throws an instance of this exception class, that will
        /// contain helpful contextual infomation about the parse error.
        /// </exception>
        public virtual void ReportException(
            XmlNode node, string name, string message, Exception cause)
        {
            string xmlFragment;

            if (node is XmlAttribute)
            {
                xmlFragment = ((XmlAttribute)node).OwnerElement.OuterXml;
            }
            else
            {
                xmlFragment = node.OuterXml;
            }
            if (xmlFragment.Length > MaxXmlErrorFragmentLength)
            {
                xmlFragment = xmlFragment.Substring(0, MaxXmlErrorFragmentLength) + "...";
            }

            string resourceDescription = Resource.Description;
            int line = ConfigurationUtils.GetLineNumber(node);
            if (line > 0)
            {
                string atLine = " at line " + line;
                resourceDescription += atLine;
            }
            throw new ObjectDefinitionStoreException(
                resourceDescription, name, message + Environment.NewLine + xmlFragment, cause);
        }

        /// <summary>
        /// This method can be overwritten in order to implement validators
        /// capable of displaying a full list of errors found in the definition.
        /// </summary>
        /// <param name="node">
        /// The node that triggered the parse error.
        /// </param>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public virtual void ReportFatalException(XmlNode node, string message)
        {
            throw new FatalObjectException(message);
        }

    }
}
