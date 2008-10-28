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
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;
using Spring.Core;
using Spring.Core.IO;
using Spring.Core.TypeResolution;
using Spring.Util;
using Spring.Validation;

#endregion

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
        /// Name of the .Net config section that contains definitions 
        /// for custom config parsers.
        /// </summary>
        private const string ConfigParsersSectionName = "spring/parsers";

        #region Fields

        private static IDictionary parsers = new HybridDictionary();

#if !NET_2_0
        private static XmlSchemaCollection schemas = new XmlSchemaCollection();
#else
        private static XmlSchemaSet schemas = new XmlSchemaSet();
#endif

        #endregion

        /// <summary>
        /// Creates a new instance of the NamespaceParserRegistry class.
        /// </summary>
        static NamespaceParserRegistry()
        {
            lock (parsers.SyncRoot)
            lock (schemas)
            {
                //TODO - externalize default list of parsers.
                RegisterParser(new ObjectsNamespaceParser());
                //This is done simple as a means to avoid cyclic dependencies with Factory.Xml
                //which implementations of parsers typically use.
                RegisterParser(ObjectUtils.InstantiateType(typeof(NamespaceParserRegistry).Assembly, 
                    "Spring.Validation.Config.ValidationNamespaceParser") as INamespaceParser);

                // register custom config parsers
                ConfigurationUtils.GetSection(ConfigParsersSectionName);
            }
        }

        /// <summary>
        /// Constructs a "assembly://..." qualified schemaLocation url using the given type
        /// to obtain the assembly name.
        /// </summary>
        public static string GetAssemblySchemaLocation( Type schemaLocationAssemblyHint, string schemaLocation)
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
            return (INamespaceParser) parsers[namespaceURI];
        }

        /// <summary>
        /// Returns a schema collection containing validation schemas for all registered parsers.
        /// </summary>
        /// <returns>
        /// A schema collection containing validation schemas for all registered parsers.
        /// </returns>
#if !NET_2_0
        public static XmlSchemaCollection GetSchemas()
#else
        public static XmlSchemaSet GetSchemas()
#endif
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

            if (!(typeof(INamespaceParser)).IsAssignableFrom(parserType))
            {
                throw new ArgumentException(string.Format("The [{0}] Type must implement the IXmlObjectDefinitionParser interface.",
                                                          parserType.Name), "parserType");
            }

            RegisterParser((INamespaceParser) ObjectUtils.InstantiateType(parserType), namespaceUri, schemaLocation);
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
                NamespaceParserAttribute defaults = GetDefaults(parser);
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
            try {
                schemas.Add(namespaceUri, new XmlTextReader(schema.InputStream));
            } catch (Exception e)
            {
                throw new ArgumentException("Could not load schema from resource = " + schema, e);
            }
        }

        /// <summary>
        /// Returns default values for the parser namespace and schema location as 
        /// defined by the <see cref="NamespaceParserAttribute"/>.
        /// </summary>
        /// <param name="parser">
        /// A parser instance.
        /// </param>
        /// <returns>
        /// A <see cref="NamespaceParserAttribute"/> instance containing
        /// default values for the parser namsepace and schema location
        /// </returns>
        private static NamespaceParserAttribute GetDefaults(INamespaceParser parser)
        {
            object[] attrs = parser.GetType().GetCustomAttributes(typeof(NamespaceParserAttribute), true);
            if (attrs.Length > 0)
            {
                return (NamespaceParserAttribute)attrs[0];
            }
            return null;
        }
    }
}