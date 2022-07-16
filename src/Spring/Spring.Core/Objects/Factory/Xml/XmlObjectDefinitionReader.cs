#region License

/*
 * Copyright  2002-2005 the original author or authors.
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

#region Imports

using System.Xml;
using System.Xml.Schema;
using Spring.Core.IO;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Object definition reader for Spring's default XML object definition format.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Typically applied to a
    /// <see cref="Spring.Objects.Factory.Support.DefaultListableObjectFactory"/> instance.
    /// </p>
    /// <p>
    /// This class registers each object definition with the given object factory superclass,
    /// and relies on the latter's implementation of the
    /// <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry"/> interface.
    /// </p>
    /// <p>
    /// It supports singletons, prototypes, and references to either of these kinds of object.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    public class XmlObjectDefinitionReader : AbstractObjectDefinitionReader
    {
        #region Utility Classes

        /// <summary>
        /// For retrying the parse process
        /// </summary>
        private class RetryParseException : Exception
        {
            public RetryParseException()
            { }
        }

        #endregion

        #region Fields

        [NonSerialized]
        private XmlResolver resolver;

        private Type documentReaderType;
        private INamespaceParserResolver namespaceParserResolver;
        private IObjectDefinitionFactory objectDefinitionFactory;

        #endregion

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Xml.XmlObjectDefinitionReader"/> class.
        /// </summary>
        /// <param name="registry">
        /// The <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry"/>
        /// instance that this reader works on.
        /// </param>
        public XmlObjectDefinitionReader(IObjectDefinitionRegistry registry)
            : this(registry, new XmlUrlResolver())
        { }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Xml.XmlObjectDefinitionReader"/> class.
        /// </summary>
        /// <param name="registry">
        /// The <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry"/>
        /// instance that this reader works on.
        /// </param>
        /// <param name="resolver">
        /// The <see cref="System.Xml.XmlResolver"/>to be used for parsing.
        /// </param>
        public XmlObjectDefinitionReader(IObjectDefinitionRegistry registry, XmlResolver resolver)
            : this(registry, resolver, new DefaultObjectDefinitionFactory())
        {
            Resolver = resolver;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Xml.XmlObjectDefinitionReader"/> class.
        /// </summary>
        /// <param name="registry">
        /// The <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry"/>
        /// instance that this reader works on.
        /// </param>
        /// <param name="resolver">
        /// The <see cref="System.Xml.XmlResolver"/>to be used for parsing.
        /// </param>
        /// <param name="objectDefinitionFactory">the <see cref="IObjectDefinitionFactory"/> to use for creating new <see cref="IObjectDefinition"/>s</param>
        protected XmlObjectDefinitionReader(IObjectDefinitionRegistry registry, XmlResolver resolver, IObjectDefinitionFactory objectDefinitionFactory)
            : base(registry)
        {
            Resolver = resolver;
            this.objectDefinitionFactory = objectDefinitionFactory;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The <see cref="System.Xml.XmlResolver"/>to be used for parsing.
        /// </summary>
        public XmlResolver Resolver
        {
            get { return resolver; }
            set { resolver = value; }
        }


        /// <summary>
        /// Sets the IObjectDefinitionDocumentReader implementation to use, responsible for
        /// the actual reading of the XML object definition document.stype of the document reader.
        /// </summary>
        /// <value>The type of the document reader.</value>
        public Type DocumentReaderType
        {
            set
            {
                if (value == null || !typeof(IObjectDefinitionDocumentReader).IsAssignableFrom(value))
                {
                    throw new ArgumentException(
                        "DocumentReaderType must be an implementation of the IObjectDefinitionReader interface.");
                }
                documentReaderType = value;
            }
        }

        /// <summary>
        /// Specify a <see cref="INamespaceParserResolver"/> to use. If none is specified a default
        /// instance will be created by <see cref="CreateDefaultNamespaceParserResolver"/>
        /// </summary>
        internal INamespaceParserResolver NamespaceParserResolver
        {
            get
            {
                if (this.namespaceParserResolver == null)
                {
                    this.namespaceParserResolver = CreateDefaultNamespaceParserResolver();
                }
                return this.namespaceParserResolver;
            }
            set
            {
                if (this.namespaceParserResolver != null)
                {
                    throw new InvalidOperationException("NamespaceParserResolver is already set");
                }
                this.namespaceParserResolver = value;
            }
        }

        /// <summary>
        /// Specify a <see cref="IObjectDefinitionFactory"/> for creating instances of <see cref="AbstractObjectDefinition"/>.
        /// </summary>
        protected IObjectDefinitionFactory ObjectDefinitionFactory
        {
            get
            {
                return this.objectDefinitionFactory;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load object definitions from the supplied XML <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">
        /// The XML resource for the object definitions that are to be loaded.
        /// </param>
        /// <returns>
        /// The number of object definitions that were loaded.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of loading or parsing errors.
        /// </exception>
        public override int LoadObjectDefinitions(IResource resource)
        {
            if (resource == null)
            {
                throw new ObjectDefinitionStoreException
                    ("Resource cannot be null: expected an XML resource.");
            }

            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug("Loading XML object definitions from " + resource);
            }

            #endregion

            try
            {
                Stream stream = resource.InputStream;
                if (stream == null)
                {
                    throw new ObjectDefinitionStoreException(
                        "InputStream is null from Resource = [" + resource + "]");
                }
                try
                {
                    return DoLoadObjectDefinitions(stream, resource);
                }
                finally
                {
                    #region Close stream
                    try
                    {
                        stream.Close();
                    }
                    catch (IOException ex)
                    {
                        #region Instrumentation

                        if (log.IsWarnEnabled)
                        {
                            log.Warn("Could not close stream.", ex);
                        }

                        #endregion
                    }
                    #endregion
                }
            }
            catch (IOException ex)
            {
                throw new ObjectDefinitionStoreException(
                    "IOException parsing XML document from " + resource.Description, ex);
            }

        }

        /// <summary>
        /// Actually load object definitions from the specified XML file.
        /// </summary>
        /// <param name="stream">The input stream to read from.</param>
        /// <param name="resource">The resource for the XML data.</param>
        /// <returns></returns>
        protected virtual int DoLoadObjectDefinitions(Stream stream, IResource resource)
        {
            try
            {
                // create local copy of data
                byte[] xmlData = IOUtils.ToByteArray(stream);

                XmlDocument doc;
                // loop until no unregistered, wellknown namespaces left
                while (true)
                {
                    XmlReader reader = null;
                    try
                    {
                        MemoryStream xmlDataStream = new MemoryStream(xmlData);
                        reader = CreateValidatingReader(xmlDataStream);
                        doc = new ConfigXmlDocument();
                        doc.Load(reader);
                        break;
                    }
                    catch (RetryParseException)
                    {
                        if (reader != null)
                            reader.Close();
                    }
                }
                return RegisterObjectDefinitions(doc, resource);
            }
            catch (XmlException ex)
            {
                throw new ObjectDefinitionStoreException(resource.Description,
                                                            "Line " + ex.LineNumber + " in XML document from " +
                                                            resource + " is not well formed.  " + ex.Message, ex);
            }
            catch (XmlSchemaException ex)
            {
                throw new ObjectDefinitionStoreException(resource.Description,
                                                            "Line " + ex.LineNumber + " in XML document from " +
                                                            resource + " violates the schema.  " + ex.Message, ex);
            }
            catch (ObjectDefinitionStoreException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ObjectDefinitionStoreException("Unexpected exception parsing XML document from " + resource.Description + "Inner exception message= " + ex.Message, ex);
            }
        }

        private XmlReader CreateValidatingReader(MemoryStream stream)
        {
            XmlReader reader;
            if (SystemUtils.MonoRuntime)
            {
                reader = XmlUtils.CreateReader(stream);
            }
            else
            {
                reader = XmlUtils.CreateValidatingReader(stream, Resolver, NamespaceParserRegistry.GetSchemas(), HandleValidation);
            }

            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug("Using the following XmlReader implementation : " + reader.GetType());
            }
            return reader;

            #endregion
        }

        /// <summary>
        /// Validation callback for a validating XML reader.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">Any data pertinent to the event.</param>
        private void HandleValidation(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Error)
            {
                XmlSchemaException ex = args.Exception;
                XmlReader xmlReader = (XmlReader)sender;
                if (!NamespaceParserRegistry.GetSchemas().Contains(xmlReader.NamespaceURI) && ex is XmlSchemaValidationException)
                {
                    // try wellknown parsers
                    bool registered = NamespaceParserRegistry.RegisterWellknownNamespaceParserType(xmlReader.NamespaceURI);
                    if (registered)
                    {
                        throw new RetryParseException();
                    }
                }
                throw ex;
            }
            else
            {
                #region Instrumentation

                if (log.IsWarnEnabled)
                {
                    log.Warn(
                        "Ignored XML validation warning: " + args.Message,
                        args.Exception);
                }

                #endregion
            }
        }

        /// <summary>
        /// Register the object definitions contained in the given DOM document.
        /// </summary>
        /// <param name="doc">The DOM document.</param>
        /// <param name="resource">
        /// The original resource from where the <see cref="System.Xml.XmlDocument"/>
        /// was read.
        /// </param>
        /// <returns>
        /// The number of object definitions that were registered.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of parsing errors.
        /// </exception>
        public int RegisterObjectDefinitions(
            XmlDocument doc, IResource resource)
        {
            IObjectDefinitionDocumentReader documentReader = CreateObjectDefinitionDocumentReader();

            //TODO make void return and get object count from registry.
            int countBefore = Registry.ObjectDefinitionCount;
            XmlReaderContext readerContext = CreateReaderContext(resource);
            readerContext.NamespaceParserResolver = this.NamespaceParserResolver;
            documentReader.RegisterObjectDefinitions(doc, readerContext);
            return Registry.ObjectDefinitionCount - countBefore;
        }

        /// <summary>
        /// Creates the <see cref="IObjectDefinitionDocumentReader"/> to use for actually
        /// reading object definitions from an XML document.
        /// </summary>
        /// <remarks>Default implementation instantiates the specified <see cref="DocumentReaderType"/>
        /// or <see cref="DefaultObjectDefinitionDocumentReader"/> if no reader type is specified.</remarks>
        /// <returns></returns>
        protected virtual IObjectDefinitionDocumentReader CreateObjectDefinitionDocumentReader()
        {
            if (documentReaderType == null)
            {
                return new DefaultObjectDefinitionDocumentReader();
            }
            return (IObjectDefinitionDocumentReader)ObjectUtils.InstantiateType(documentReaderType);
        }

        /// <summary>
        /// Creates the <see cref="XmlReaderContext"/> to be passed along
        /// during the object definition reading process.
        /// </summary>
        /// <param name="resource">The underlying <see cref="IResource"/> that is currently processed.</param>
        /// <returns>A new <see cref="XmlReaderContext"/></returns>
        protected virtual XmlReaderContext CreateReaderContext(IResource resource)
        {
            return new XmlReaderContext(resource, this, this.objectDefinitionFactory);
        }

        /// <summary>
        /// Create a <see cref="INamespaceParserResolver"/> instance for handling custom namespaces.
        /// </summary>
        /// <remarks>
        /// TODO (EE): make protected virtual, see remarks on <see cref="INamespaceParserResolver"/>
        /// </remarks>
        private INamespaceParserResolver CreateDefaultNamespaceParserResolver()
        {
            return new DefaultNamespaceHandlerResolver();
        }

        #endregion
    }
}
