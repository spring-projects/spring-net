#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Spring.Core.IO;
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
        #region Fields

        [NonSerialized]
        private XmlResolver resolver;

        private Type documentReaderType = typeof (DefaultObjectDefinitionDocumentReader);

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
        {}

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
        public XmlObjectDefinitionReader(IObjectDefinitionRegistry registry, XmlResolver resolver) : base(registry)
        {
            Resolver = resolver;
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
                XmlReader reader = null;

                if (SystemUtils.MonoRuntime)
                {
                    reader = XmlUtils.CreateReader(stream);
                }
                else
                {
                    reader =
                        XmlUtils.CreateValidatingReader(stream, Resolver, NamespaceParserRegistry.GetSchemas(),
                                                        new ValidationEventHandler(HandleValidation));
                }

                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug("Using the following XmlReader implementation : " + reader.GetType());
                }

                #endregion

                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                return RegisterObjectDefinitions(doc, resource);
            }
            catch (XmlException ex)
            {
                throw new XmlObjectDefinitionStoreException(resource.Description,
                                                            "Line " + ex.LineNumber + " in XML document from " +
                                                            resource + " is invalid.  " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new ObjectDefinitionStoreException("Unexpected exception parsing XML document from " + resource.Description + "Inner exception message= " + ex.Message, ex);
            }
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
#if !NET_1_0
                throw new XmlException(args.Message, args.Exception, args.Exception.LineNumber, args.Exception.LinePosition);
#else
				throw new XmlException(args.Message, args.Exception);
#endif
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
            documentReader.RegisterObjectDefinitions(doc, CreateReaderContext(resource));
            return Registry.ObjectDefinitionCount - countBefore;
        }

        /// <summary>
        /// Creates the <see cref="IObjectDefinitionDocumentReader"/> to use for actually
        /// reading object definitions from an XML document.
        /// </summary>
        /// <remarks>Default implementation instantiates the specified 'documentReaderType'.</remarks>
        /// <returns></returns>
        protected virtual IObjectDefinitionDocumentReader CreateObjectDefinitionDocumentReader()
        {
            return (IObjectDefinitionDocumentReader) ObjectUtils.InstantiateType(documentReaderType);
        }

		/// <summary>
		/// Creates the <see cref="XmlReaderContext"/> to be passed along 
		/// during the object definition reading process.
		/// </summary>
		/// <param name="resource">The underlying <see cref="IResource"/> that is currently processed.</param>
		/// <returns>A new <see cref="XmlReaderContext"/></returns>
        protected virtual XmlReaderContext CreateReaderContext(IResource resource)
        {
            return new XmlReaderContext(resource, this);
        }

        #endregion
    }
}