/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;
using Spring.Collections;
using Spring.Core.IO;
using Spring.Objects.Factory.Config;
using Spring.Util;

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Provides a resolution mechanism for configuration parsers.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The <see cref="DefaultObjectDefinitionDocumentReader"/> uses this registry
    /// class to find the parser handling a specific namespace.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class NamespaceParserRegistry
    {
        /// <summary>
        /// Resolves xml entities by using the <see cref="IResourceLoader"/> infrastructure.
        /// </summary>
        private class XmlResourceUrlResolver : XmlUrlResolver
        {
            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                IResourceLoader resourceLoader = new ConfigurableResourceLoader();
                IResource resource = resourceLoader.GetResource(absoluteUri.AbsoluteUri);
                return resource.InputStream;
                //return base.GetEntity( absoluteUri, role, ofObjectToReturn );
            }

            public override Uri ResolveUri(Uri baseUri, string relativeUri)
            {
                // TODO: resolve Uri using IResource instance
                return base.ResolveUri(baseUri, relativeUri);
            }
        }

        /// <summary>
        /// Name of the .Net config section that contains definitions
        /// for custom config parsers.
        /// </summary>
        private const string ConfigParsersSectionName = "spring/parsers";

        private static IDictionary parsers;
        private readonly static IDictionary wellknownNamespaceParserTypeNames;
        private static XmlSchemaSet schemas;

        /// <summary>
        /// Creates a new instance of the NamespaceParserRegistry class.
        /// </summary>
        static NamespaceParserRegistry()
        {
            wellknownNamespaceParserTypeNames = new CaseInsensitiveHashtable();
            wellknownNamespaceParserTypeNames["http://www.springframework.net/tx"] =
                "Spring.Transaction.Config.TxNamespaceParser, Spring.Data";
            wellknownNamespaceParserTypeNames["http://www.springframework.net/aop"] =
                "Spring.Aop.Config.AopNamespaceParser, Spring.Aop";
            wellknownNamespaceParserTypeNames["http://www.springframework.net/context"] =
                "Spring.Context.Config.ContextNamespaceParser, Spring.Core";
            wellknownNamespaceParserTypeNames["http://www.springframework.net/db"] =
                "Spring.Data.Config.DatabaseNamespaceParser, Spring.Data";
            wellknownNamespaceParserTypeNames["http://www.springframework.net/database"] =
                "Spring.Data.Config.DatabaseNamespaceParser, Spring.Data";
            wellknownNamespaceParserTypeNames["http://www.springframework.net/remoting"] =
                "Spring.Remoting.Config.RemotingNamespaceParser, Spring.Services";
            wellknownNamespaceParserTypeNames["http://www.springframework.net/wcf"] =
                "Spring.ServiceModel.Config.WcfNamespaceParser, Spring.Services";
            wellknownNamespaceParserTypeNames["http://www.springframework.net/nms"] =
                "Spring.Messaging.Nms.Config.NmsNamespaceParser, Spring.Messaging.Nms";
            wellknownNamespaceParserTypeNames["http://www.springframework.net/ems"] =
                "Spring.Messaging.Ems.Config.EmsNamespaceParser, Spring.Messaging.Ems";
            wellknownNamespaceParserTypeNames["http://www.springframework.net/validation"] =
                "Spring.Validation.Config.ValidationNamespaceParser, Spring.Core";
            wellknownNamespaceParserTypeNames["http://www.springframework.net/nvelocity"] =
                "Spring.Template.Velocity.Config.TemplateNamespaceParser, Spring.Template.Velocity";

            Reset();
        }

        /// <summary>
        /// Reset the list of registered parsers to "factory"-setting
        /// </summary>
        /// <remarks>use for unit tests only</remarks>
        public static void Reset()
        {
            parsers = new HybridDictionary();
            schemas = new XmlSchemaSet();
            schemas.XmlResolver = new XmlResourceUrlResolver();

            RegisterParser(new ObjectsNamespaceParser());
            // register custom config parsers
            ConfigurationUtils.GetSection(ConfigParsersSectionName);
        }

        /// <summary>
        /// Registers the <see cref="INamespaceParser"/> type for wellknown namespaces
        /// </summary>
        /// <returns><c>true</c> if the parser could be registered, <c>false</c> otherwise</returns>
        internal static bool RegisterWellknownNamespaceParserType(string namespaceUri)
        {
            if (parsers[namespaceUri] != null) return true;

            if (wellknownNamespaceParserTypeNames.Contains(namespaceUri))
            {
                string parserTypeName = (string) wellknownNamespaceParserTypeNames[namespaceUri];
                // assume, that all Spring.XXX assemblies have same version + public key
                // get the ", Version=x.x.x.x, Culture=neutral, PublicKeyToken=65e474d141e25e07" part of Spring.Core and append it
                string name = typeof(NamespaceParserRegistry).Assembly.GetName().Name;
                string fullname = typeof(NamespaceParserRegistry).Assembly.GetName().FullName;
                string versionCulturePublicKey = fullname.Substring(name.Length);

                parserTypeName = parserTypeName + versionCulturePublicKey;
                Type parserType = Type.GetType(parserTypeName, true);
                RegisterParser(parserType);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Constructs a "assembly://..." qualified schemaLocation url using the given type
        /// to obtain the assembly name.
        /// </summary>
        public static string GetAssemblySchemaLocation(Type schemaLocationAssemblyHint, string schemaLocation)
        {
            if (schemaLocationAssemblyHint != null)
            {
                return "assembly://" + schemaLocationAssemblyHint.Assembly.FullName + schemaLocation;
            }

            return schemaLocation;
        }

        /// <summary>
        /// Returns a parser for the given namespace.
        /// </summary>
        /// <param name="namespaceURI">
        /// The namespace for which to lookup the parser implementation.
        /// </param>
        /// <returns>
        /// A parser for a given <paramref name="namespaceURI"/>, or
        /// <see langword="null"/> if no parser was found.
        /// </returns>
        public static INamespaceParser GetParser(string namespaceURI)
        {
            INamespaceParser parser = (INamespaceParser) parsers[namespaceURI];
            if (parser == null)
            {
                bool ok = RegisterWellknownNamespaceParserType(namespaceURI);
                if (ok)
                {
                    parser = (INamespaceParser) parsers[namespaceURI];

                    //work-around for SPRNET-1277 where we're inconsistent re: exposing /db or /database as the final namespace element
                    if (parser == null && namespaceURI == "http://www.springframework.net/db")
                    {
                        parser = (INamespaceParser) parsers["http://www.springframework.net/database"];
                    }
                }
            }

            return parser;
        }

        /// <summary>
        /// Returns a schema collection containing validation schemas for all registered parsers.
        /// </summary>
        /// <returns>
        /// A schema collection containing validation schemas for all registered parsers.
        /// </returns>
        public static XmlSchemaSet GetSchemas()
        {
            return schemas;
        }

        /// <summary>
        /// Pegisters parser, using default namespace and schema location
        /// as defined by the <see cref="NamespaceParserAttribute"/>.
        /// </summary>
        /// <param name="parserType">
        /// The <see cref="System.Type"/> of the parser that will be activated
        /// when an element in its default namespace is encountered.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="parserType"/> is <see langword="null"/>.
        /// </exception>
        public static void RegisterParser(Type parserType)
        {
            RegisterParser(parserType, null, null);
        }

        /// <summary>
        /// Associates a parser with a namespace.
        /// </summary>
        /// <remarks>
        /// <note>
        /// Parsers registered with the same <paramref name="namespaceUri"/> as that
        /// of a parser that has previously been registered will overwrite the existing
        /// parser.
        /// </note>
        /// </remarks>
        /// <param name="parserType">
        /// The <see cref="System.Type"/> of the parser that will be activated
        /// when the attendant <paramref name="namespaceUri"/> is
        /// encountered.
        /// </param>
        /// <param name="namespaceUri">
        /// The namespace with which to associate instance of the parser.
        /// </param>
        /// <param name="schemaLocation">
        /// The location of the XML schema that should be used for validation
        /// of the XML elements that belong to the specified namespace
        /// (can be any valid Spring.NET resource URI).
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// If the <paramref name="parserType"/> is not a <see cref="System.Type"/>
        /// that implements the <see cref="INamespaceParser"/>
        /// interface.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="parserType"/> is <see langword="null"/>.
        /// </exception>
        public static void RegisterParser(Type parserType, string namespaceUri, string schemaLocation)
        {
            AssertUtils.ArgumentNotNull(parserType, "parserType");

            INamespaceParser np = null;

            if ((typeof(INamespaceParser)).IsAssignableFrom(parserType))
            {
                np = (INamespaceParser) ObjectUtils.InstantiateType(parserType);
            }
            // TODO (EE): workaround to enable smooth transition between 1.x and 2.0 style namespace handling
            else if (typeof(IObjectDefinitionParser).IsAssignableFrom(parserType))
            {
                // determine and use defaults for the namespace and schema location, if necessary
                if (StringUtils.IsNullOrEmpty(namespaceUri) || StringUtils.IsNullOrEmpty(schemaLocation))
                {
                    NamespaceParserAttribute defaults = GetDefaults(parserType);
                    if (defaults == null)
                    {
                        throw new ArgumentNullException(
                            "Either default or an explicit namespace value must be specified for a configuration parser.");
                    }

                    if (StringUtils.IsNullOrEmpty(namespaceUri))
                    {
                        namespaceUri = defaults.Namespace;
                    }

                    if (StringUtils.IsNullOrEmpty(schemaLocation))
                    {
                        schemaLocation = defaults.SchemaLocation;
                        if (defaults.SchemaLocationAssemblyHint != null)
                        {
                            schemaLocation =
                                GetAssemblySchemaLocation(defaults.SchemaLocationAssemblyHint, schemaLocation);
                        }
                    }
                }

                IObjectDefinitionParser odParser = (IObjectDefinitionParser) ObjectUtils.InstantiateType(parserType);
                np = new ObjectDefinitionParserNamespaceParser(odParser);
            }
            else
            {
                throw new ArgumentException(
                    string.Format("The [{0}] Type must implement the INamespaceParser interface.", parserType.Name)
                    , "parserType");
            }

            RegisterParser(np, namespaceUri, schemaLocation);
        }

        /// <summary>
        /// Pegisters parser, using default namespace and schema location
        /// as defined by the <see cref="NamespaceParserAttribute"/>.
        /// </summary>
        /// <param name="parser">
        /// The parser instance.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="parser"/> is <see langword="null"/>.
        /// </exception>
        public static void RegisterParser(INamespaceParser parser)
        {
            RegisterParser(parser, null, null);
        }

        /// <summary>
        /// Associates a parser with a namespace.
        /// </summary>
        /// <remarks>
        /// <note>
        /// Parsers registered with the same <paramref name="namespaceUri"/> as that
        /// of a parser that has previously been registered will overwrite the existing
        /// parser.
        /// </note>
        /// </remarks>
        /// <param name="namespaceUri">
        /// The namespace with which to associate instance of the parser.
        /// </param>
        /// <param name="parser">
        /// The parser instance.
        /// </param>
        /// <param name="schemaLocation">
        /// The location of the XML schema that should be used for validation
        /// of the XML elements that belong to the specified namespace
        /// (can be any valid Spring.NET resource URI).
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="parser"/> is <see langword="null"/>, or if
        /// <paramref name="namespaceUri"/> is not specified and parser class
        /// does not have default value defined using <see cref="NamespaceParserAttribute"/>.
        /// </exception>
        public static void RegisterParser(INamespaceParser parser, string namespaceUri, string schemaLocation)
        {
            AssertUtils.ArgumentNotNull(parser, "parser");

            // determine and use defaults for the namespace and schema location, if necessary
            if (StringUtils.IsNullOrEmpty(namespaceUri) || StringUtils.IsNullOrEmpty(schemaLocation))
            {
                NamespaceParserAttribute defaults = GetDefaults(parser.GetType());
                if (defaults == null)
                {
                    throw new ArgumentNullException(
                        "Either default or an explicit namespace value must be specified for a configuration parser.");
                }

                if (StringUtils.IsNullOrEmpty(namespaceUri))
                {
                    namespaceUri = defaults.Namespace;
                }

                if (StringUtils.IsNullOrEmpty(schemaLocation))
                {
                    schemaLocation = defaults.SchemaLocation;
                    if (defaults.SchemaLocationAssemblyHint != null)
                    {
                        schemaLocation = GetAssemblySchemaLocation(defaults.SchemaLocationAssemblyHint, schemaLocation);
                    }
                }
            }

            // initialize the parser
            parser.Init();

            // register parser
            lock (parsers.SyncRoot)
            lock (schemas)
            {
                parsers[namespaceUri] = parser;
                if (StringUtils.HasText(schemaLocation) && !schemas.Contains(namespaceUri))
                {
                    RegisterSchema(namespaceUri, schemaLocation);
                }
            }
        }

        /// <summary>
        /// Register a schema as well-known
        /// </summary>
        /// <param name="namespaceUri"></param>
        /// <param name="schemaLocation"></param>
        private static void RegisterSchema(string namespaceUri, string schemaLocation)
        {
            IResourceLoader resourceLoader = new ConfigurableResourceLoader();
            IResource schema = resourceLoader.GetResource(schemaLocation);
            try
            {
                XmlTextReader schemaDocument = new XmlTextReader(schema.Uri.AbsoluteUri, schema.InputStream);
                schemas.Add(namespaceUri, schemaDocument);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Could not load schema from resource = " + schema, e);
            }
        }

        /// <summary>
        /// Returns default values for the parser namespace and schema location as
        /// defined by the <see cref="NamespaceParserAttribute"/>.
        /// </summary>
        /// <param name="parserType">
        /// A type of the parser.
        /// </param>
        /// <returns>
        /// A <see cref="NamespaceParserAttribute"/> instance containing
        /// default values for the parser namsepace and schema location
        /// </returns>
        private static NamespaceParserAttribute GetDefaults(Type parserType)
        {
            object[] attrs = parserType.GetCustomAttributes(typeof(NamespaceParserAttribute), true);
            if (attrs.Length > 0)
            {
                return (NamespaceParserAttribute) attrs[0];
            }

            return null;
        }

        /// <summary>
        /// Adapts the <see cref="IObjectDefinitionParser"/> interface to <see cref="INamespaceParser"/>.
        /// Only for smooth transition between 1.x and 2.0 style namespace handling, will be dropped for 2.0
        /// </summary>
        private class ObjectDefinitionParserNamespaceParser : INamespaceParser
        {
            private readonly IObjectDefinitionParser odParser;

            public ObjectDefinitionParserNamespaceParser(IObjectDefinitionParser odParser)
            {
                this.odParser = odParser;
            }

            public void Init()
            {
                // noop
            }

            public IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
            {
                return odParser.ParseElement(element, parserContext);
            }

            public ObjectDefinitionHolder Decorate(XmlNode node, ObjectDefinitionHolder definition,
                ParserContext parserContext)
            {
                return null;
            }
        }
    }
}
