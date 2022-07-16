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

using Spring.Data.Common;
using Spring.Core.TypeResolution;
using Spring.Objects;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Data.Config
{
    /// <summary>
    /// Implementation of the custom configuration parser for database definitions.
    /// </summary>
    /// <author>Mark Pollack</author>
    [
        NamespaceParser(
            Namespace = "http://www.springframework.net/database",
            SchemaLocationAssemblyHint = typeof(DatabaseNamespaceParser),
            SchemaLocation = "/Spring.Data.Config/spring-database-1.3.xsd")
    ]
    public class DatabaseNamespaceParser : ObjectsNamespaceParser
    {

        private const string DatabaseTypePrefix = "database: ";

        static DatabaseNamespaceParser()
        {
            TypeRegistry.RegisterType(
                DatabaseTypePrefix + DbProviderConfigurerConstants.DbProviderConfigurerElement,
                typeof(DbProviderConfigurer));
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
            string name = element.GetAttribute(ObjectDefinitionConstants.IdAttribute);
            IConfigurableObjectDefinition remotingDefinition = ParseDbProviderDefinition(element, name, parserContext);
            if (!StringUtils.HasText(name))
            {
                name = ObjectDefinitionReaderUtils.GenerateObjectName(remotingDefinition, parserContext.Registry);
            }
            parserContext.Registry.RegisterObjectDefinition(name, remotingDefinition);

            return null;
        }


        /// <summary>
        /// Parses database provider definitions.
        /// </summary>
        /// <param name="element">Validator XML element.</param>
        /// <param name="name">The name of the object definition.</param>
        /// <param name="parserContext">The parser context.</param>
        /// <returns>A database provider object definition.</returns>
        private IConfigurableObjectDefinition ParseDbProviderDefinition(
            XmlElement element, string name, ParserContext parserContext)
        {
            switch (element.LocalName)
            {
                case DbProviderConfigurerConstants.DbProviderConfigurerElement:
                    return ParseDbProviderConfigurer(element, name, parserContext);
                case DbProviderFactoryObjectConstants.DbProviderFactoryObjectElement:
                    return ParseDbProviderFactoryObject(element, name, parserContext);
            }

            return null;
        }

        private IConfigurableObjectDefinition ParseDbProviderConfigurer(XmlElement element, string name, ParserContext parserContext)
        {
            string typeName = GetTypeName(element);
            string resource = GetAttributeValue(element, DbProviderConfigurerConstants.ResourceAttribute);

            MutablePropertyValues propertyValues = new MutablePropertyValues();
            if (StringUtils.HasText(resource))
            {
                propertyValues.Add("ProviderResource", resource);
            }
            IConfigurableObjectDefinition cod = parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(
                typeName, null, parserContext.ReaderContext.Reader.Domain);
            cod.PropertyValues = propertyValues;
            return cod;
        }

        private IConfigurableObjectDefinition ParseDbProviderFactoryObject(XmlElement element, string name, ParserContext parserContext)
        {
            string typeName = GetTypeName(element);

            string providerNameAttribute = GetAttributeValue(element, DbProviderFactoryObjectConstants.ProviderNameAttribute);
            string connectionString = GetAttributeValue(element, DbProviderFactoryObjectConstants.ConnectionStringAttribute);

            MutablePropertyValues propertyValues = new MutablePropertyValues();
            if (StringUtils.HasText(providerNameAttribute))
            {
                propertyValues.Add("Provider", providerNameAttribute);
            }
            if (StringUtils.HasText(connectionString))
            {
                propertyValues.Add("ConnectionString", connectionString);
            }
            IConfigurableObjectDefinition cod = parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(
                typeName, null, parserContext.ReaderContext.Reader.Domain);
            cod.PropertyValues = propertyValues;
            return cod;

        }

        /*

        protected override void DoParse(XmlElement element, ParserContext parserContext, ObjectDefinitionBuilder builder)
        {
            switch (element.LocalName)
            {
                case DbProviderFactoryObjectConstants.DbProviderFactoryObjectElement:
                    {
                        ParseDatabaseConfigurer(element, parserContext, builder);
                        return;
                    }
            }
        }
        */

        /*
        private void ParseDatabaseConfigurer(XmlElement element, ParserContext parserContext, ObjectDefinitionBuilder builder)
        {
            string providerNameAttribute = GetAttributeValue(element, DbProviderFactoryObjectConstants.ProviderNameAttribute);
            string connectionString = GetAttributeValue(element, DbProviderFactoryObjectConstants.ConnectionStringAttribute);

            if (StringUtils.HasText(providerNameAttribute))
            {
                builder.AddPropertyValue("Provider", providerNameAttribute);
            }
            if (StringUtils.HasText(connectionString))
            {
                builder.AddPropertyValue("ConnectionString", connectionString);
            }
        }
        */

        /// <summary>
        /// Gets the name of the object type for the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The name of the object type.</returns>
        private string GetTypeName(XmlElement element)
        {
            string typeName = GetAttributeValue(element, ObjectDefinitionConstants.TypeAttribute);
            if (StringUtils.IsNullOrEmpty(typeName))
            {
                return DatabaseTypePrefix + element.LocalName;
            }
            return typeName;
        }

        /*
        protected override string GetObjectTypeName(XmlElement element)
        {
            string typeName = GetAttributeValue(element, ObjectDefinitionConstants.TypeAttribute);
            if (StringUtils.IsNullOrEmpty(typeName))
            {
                return DatabaseTypePrefix + element.LocalName;
            }
            return typeName;
        }*/

        private class DbProviderFactoryObjectConstants
        {
            public const string DbProviderFactoryObjectElement = "provider";
            public const string ProviderNameAttribute = "provider";
            public const string ConnectionStringAttribute = "connectionString";

        }

        private class DbProviderConfigurerConstants
        {
            public const string DbProviderConfigurerElement = "additionalProviders";
            public const string ResourceAttribute = "resource";
        }
    }
}
