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

using System;
using System.Collections.Generic;
using System.Xml;
using NVelocity.Runtime;
using Spring.Core.TypeResolution;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Template.Velocity;
using Spring.Util;

#endregion

namespace Spring.Template.Velocity.Config {
    /// <summary>
    /// Implementation of the custom configuration parser for template configurations
    /// </summary>
    /// <author>Erez Mazor</author>
    [
        NamespaceParser(
            Namespace = "http://www.springframework.net/nvelocity",
            SchemaLocationAssemblyHint = typeof(TemplateNamespaceParser),
            SchemaLocation = "/Spring.Template.Velocity.Config/spring-nvelocity-1.3.xsd")
    ]
    public sealed class TemplateNamespaceParser : AbstractSingleObjectDefinitionParser {
        private const string TemplateTypePrefix = "template: ";

        static TemplateNamespaceParser() {
            TypeRegistry.RegisterType(TemplateTypePrefix + TemplateDefinitionConstants.NVelocityElement, typeof(VelocityEngineFactoryObject));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateNamespaceParser"/> class.
        /// </summary>
        public TemplateNamespaceParser() {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="parserContext"></param>
        /// <param name="builder"></param>
        protected override void DoParse(XmlElement element, ParserContext parserContext, ObjectDefinitionBuilder builder) {
            switch (element.LocalName) {
                case TemplateDefinitionConstants.NVelocityElement:
                    ParseNVelocityTemplate(element, parserContext, builder);
                    return;
                default:
                    throw new ArgumentException(string.Format("undefined element for templating namespace: {0}", element.LocalName));
            }
        }

        private void ParseNVelocityTemplate(XmlElement element, ParserContext parseContext, ObjectDefinitionBuilder builder) {
            string preferFileSystemAccess = GetAttributeValue(element, TemplateDefinitionConstants.AttributePreferFileSystemAccess);
            string overrideLogging = GetAttributeValue(element, TemplateDefinitionConstants.AttributeOverrideLogging);
            string configFile = GetAttributeValue(element, TemplateDefinitionConstants.AttributeConfigFile);

            if (StringUtils.HasText(preferFileSystemAccess)) {
                builder.AddPropertyValue(NVelocityEngineFactoryProperties.PropertyPreferFileSystemAccess, preferFileSystemAccess);
            }

            if (StringUtils.HasText(overrideLogging)) {
                builder.AddPropertyValue(NVelocityEngineFactoryProperties.PropertyOverrideLogging, overrideLogging);
            }

            if (StringUtils.HasText(configFile)) {
                builder.AddPropertyValue(NVelocityEngineFactoryProperties.PropertyConfigFile, configFile);
            }

            XmlNodeList childElements = element.ChildNodes;
            if (childElements.Count > 0) {
                ParseChildDefinitions(childElements, parseContext, builder);
            }
        }

        private void ParseChildDefinitions(XmlNodeList childElements, ParserContext context, ObjectDefinitionBuilder builder) {
            IDictionary<string, object> properties = new Dictionary<string, object>();

            foreach (XmlElement element in childElements) {
                switch (element.LocalName) {
                    case TemplateDefinitionConstants.ElementResourceLoader:
                        ParseResourceLoader(element, context, builder, properties);
                        break;
                    case TemplateDefinitionConstants.ElementNVelocityProperties:
                        ParseNVelocityProperties(element, properties);
                        break;
                }
            }

            if (properties.Count > 0) {
                builder.AddPropertyValue(NVelocityEngineFactoryProperties.PropertyVelocityProperties, properties);
            }
        }

        private void ParseResourceLoader(XmlElement element, ParserContext context, ObjectDefinitionBuilder builder, IDictionary<string, object> properties) {
            string loaderTypeDefinition = GetAttributeValue(element, TemplateDefinitionConstants.AttributeLoaderType);
            XmlNodeList loaderElements = element.ChildNodes;
            foreach (XmlElement loaderElement in loaderElements) {
                switch (loaderElement.LocalName) {
                    case TemplateDefinitionConstants.ElementResourceLoaderFile:
                        AppendResourceLoaderPaths(loaderElements, builder);
                        break;
                    case TemplateDefinitionConstants.ElementResourceLoaderAssembly:
                        AppendAssemblyLoaderProperties((XmlElement)loaderElements.Item(0), properties, loaderTypeDefinition);
                        break;
                }
            }

        }

        private void AppendAssemblyLoaderProperties(XmlElement element, IDictionary<string, object> properties, string loaderTypeDefinition) {
            string assemblyName = GetAttributeValue(element, TemplateDefinitionConstants.AttributeName);
            string loaderType = (StringUtils.HasText(loaderTypeDefinition))
                                    ? loaderTypeDefinition
                                    : "NVelocity.Runtime.Resource.Loader.AssemblyResourceLoader";
            properties.Add(RuntimeConstants.RESOURCE_LOADER, element.LocalName);
            properties.Add(element.LocalName + "." + RuntimeConstants.RESOURCE_LOADER + ".class", loaderType);
            properties.Add(element.LocalName + "." + RuntimeConstants.RESOURCE_LOADER + ".assembly", assemblyName);
        }

        private void AppendResourceLoaderPaths(XmlNodeList elements, ObjectDefinitionBuilder builder) {
            IList<string> paths = new List<string>();
            foreach (XmlElement element in elements) {
                string path = GetAttributeValue(element, TemplateDefinitionConstants.AttributePath);
                paths.Add(path);
            }
            builder.AddPropertyValue(NVelocityEngineFactoryProperties.PropertyResourceLoaderPaths, paths);
        }


        private void ParseNVelocityProperties(XmlElement element, IDictionary<string, object> dictionary) {
            // todo add impl
        }


        protected override string GetObjectTypeName(XmlElement element) {
            string typeName = GetAttributeValue(element, ObjectDefinitionConstants.TypeAttribute);
            if (StringUtils.IsNullOrEmpty(typeName)) {
                return TemplateTypePrefix + element.LocalName;
            }
            return typeName;
        }

        #region Element & Attribute Name Constants

        private class TemplateDefinitionConstants {
            public const string NVelocityElement = "engine";
            public const string AttributePreferFileSystemAccess = "prefer-file-system-access";
            public const string AttributeConfigFile = "config-file";
            public const string AttributeOverrideLogging = "override-logging";
            public const string AttributeLoaderType = "loader-type";
            public const string ElementResourceLoader = "resource-loader";
            public const string ElementResourceLoaderFile = "file";
            public const string ElementResourceLoaderAssembly = "assembly";
            public const string ElementNVelocityProperties = "nvelocity-properties";
            public const string AttributeName = "name";
            public const string AttributePath = "path";
        }

        private class NVelocityEngineFactoryProperties {
            public const string PropertyPreferFileSystemAccess = "PreferFileSystemAccess";
            public const string PropertyOverrideLogging = "OverrideLogging";
            public const string PropertyConfigFile = "ConfigLocation";
            public const string PropertyResourceLoaderPaths = "ResourceLoaderPaths";
            public const string PropertyVelocityProperties = "VelocityProperties";
        }
        #endregion
    }
}