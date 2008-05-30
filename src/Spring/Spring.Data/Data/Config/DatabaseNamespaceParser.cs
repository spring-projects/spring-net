#region License

/*
 * Copyright 2002-2004 the original author or authors.
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

using System.Globalization;
using System.Xml;

using Spring.Data.Common;
using Spring.Core.TypeResolution;
using Spring.Objects;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

#endregion

namespace Spring.Data.Config
{
    /// <summary>
    /// Implementation of the custom configuration parser for database definitions.
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id: DatabaseNamespaceParser.cs,v 1.3 2007/08/08 00:34:33 bbaia Exp $</version>
    [
        NamespaceParser(
            Namespace = "http://www.springframework.net/database",
            SchemaLocationAssemblyHint = typeof(DatabaseNamespaceParser),
            SchemaLocation = "/Spring.Data.Config/spring-database-1.1.xsd")
    ]
    public class DatabaseNamespaceParser : ObjectsNamespaceParser
    {

        private const string DatabaseTypePrefix = "database: ";
        
        static DatabaseNamespaceParser()
        {
            TypeRegistry.RegisterType(
                DatabaseTypePrefix + DbProviderFactoryObjectConstants.DbProviderFactoryObjectElement,
                typeof (DbProviderFactoryObject));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseNamespaceParser"/> class.
        /// </summary>
        public DatabaseNamespaceParser()
        {            
        }




        /// <summary>
        /// Parse the specified element and register any resulting
        /// IObjectDefinitions with the IObjectDefinitionRegistry that is
        /// embedded in the supplied ParserContext.
        /// </summary>
        /// <param name="element">The element to be parsed into one or more IObjectDefinitions</param>
        /// <param name="parserContext">The object encapsulating the current state of the parsing
        /// process.</param>
        /// <returns>
        /// The primary IObjectDefinition (can be null as explained above)
        /// </returns>
        /// <remarks>
        /// Implementations should return the primary IObjectDefinition
        /// that results from the parse phase if they wish to used nested
        /// inside (for example) a <code>&lt;property&gt;</code> tag.
        /// <para>Implementations may return null if they will not
        /// be used in a nested scenario.
        /// </para>
        /// </remarks>
        public override IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
        {
            string id = element.GetAttribute(ObjectDefinitionConstants.IdAttribute);
            IConfigurableObjectDefinition databaseConfiguration = ParseDatabaseDefinition(element, id, parserContext);
            if (!StringUtils.HasText(id))
            {
                id = ObjectDefinitionReaderUtils.GenerateObjectName(databaseConfiguration, parserContext.Registry);
            }
            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Registering object definition with id '{0}'.", id));
            }

            #endregion
            parserContext.Registry.RegisterObjectDefinition(id, databaseConfiguration);

            return null;
        }

        private IConfigurableObjectDefinition ParseDatabaseDefinition(XmlElement element, string name, ParserContext parserContext)
        {
            switch (element.LocalName)
            {
                case DbProviderFactoryObjectConstants.DbProviderFactoryObjectElement:
                    return ParseDatabaseConfigurer(element, name, parserContext);
            }
            return null;
        }

        private IConfigurableObjectDefinition ParseDatabaseConfigurer(XmlElement element, string name, ParserContext parserContext)
        {
            string typeName = GetTypeName(element);
            string providerNameAttribute = element.GetAttribute(DbProviderFactoryObjectConstants.ProviderNameAttribute);
            string connectionString = element.GetAttribute(DbProviderFactoryObjectConstants.ConnectionStringAttribute);

            MutablePropertyValues properties = new MutablePropertyValues();
            if (StringUtils.HasText(providerNameAttribute))
            {
                properties.Add("Provider", providerNameAttribute);
            }
            if (StringUtils.HasText(connectionString))
            {
                properties.Add("ConnectionString", connectionString);
            }

            IConfigurableObjectDefinition cod = parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(
                typeName, null, parserContext.ReaderContext.Reader.Domain);
            cod.PropertyValues = properties;

            return cod;
        }

        /// <summary>
        /// Gets the name of the object type for the specified element.  This has already been aliased
        /// in the static constructor.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The name of the object type.</returns>
        private string GetTypeName(XmlElement element)
        {
            string typeName = element.GetAttribute(ObjectDefinitionConstants.TypeAttribute);
            if (StringUtils.IsNullOrEmpty(typeName))
            {
                return DatabaseTypePrefix + element.LocalName;
            }
            return typeName;
        }

        private class DbProviderFactoryObjectConstants
        {
            public const string DbProviderFactoryObjectElement = "provider";
            public const string ProviderNameAttribute = "provider";
            public const string ConnectionStringAttribute = "connectionString";
        }
    }
}
