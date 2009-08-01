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
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using NVelocity.Runtime;
using NVelocity.Runtime.Resource.Loader;
using Spring.Core.TypeResolution;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
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
        private static readonly ObjectNamespaceParserHelper objectNamespaceParserHelper = new ObjectNamespaceParserHelper();

        static TemplateNamespaceParser() {
            TypeRegistry.RegisterType(TemplateTypePrefix + TemplateDefinitionConstants.NVelocityElement, typeof(VelocityEngineFactoryObject));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateNamespaceParser"/> class.
        /// </summary>
        public TemplateNamespaceParser() {
        }

        /// <summary>
        /// Parses single object definition for the templating namespace
        /// </summary>
        /// <param name="element">the current XmlElement to parse</param>
        /// <param name="parserContext">the parser context</param>
        /// <param name="builder">the ObjectDefinitionBuilder used to configure this object</param>
        /// <see cref="AbstractSingleObjectDefinitionParser"/>
        protected override void DoParse(XmlElement element, ParserContext parserContext, ObjectDefinitionBuilder builder) {
            switch (element.LocalName) {
                case TemplateDefinitionConstants.NVelocityElement:
                    ParseNVelocityEngine(element, parserContext, builder);
                    return;
                default:
                    throw new ArgumentException(string.Format("undefined element for templating namespace: {0}", element.LocalName));
            }
        }

        /// <summary>
        /// Parses the object definition for the engine object, configures a single NVelocity template engine based 
        /// on the element definitions.
        /// </summary>
        /// <param name="element">the root element defining the velocity engine</param>
        /// <param name="parserContext">the parser context</param>
        /// <param name="builder">the ObjectDefinitionBuilder used to configure this object</param>
        private void ParseNVelocityEngine(XmlElement element, ParserContext parserContext, ObjectDefinitionBuilder builder) {
            string preferFileSystemAccess = GetAttributeValue(element, TemplateDefinitionConstants.AttributePreferFileSystemAccess);
            string overrideLogging = GetAttributeValue(element, TemplateDefinitionConstants.AttributeOverrideLogging);
            string configFile = GetAttributeValue(element, TemplateDefinitionConstants.AttributeConfigFile);

            if (StringUtils.HasText(preferFileSystemAccess)) {
                builder.AddPropertyValue(TemplateDefinitionConstants.PropertyPreferFileSystemAccess, preferFileSystemAccess);
            }

            if (StringUtils.HasText(overrideLogging)) {
                builder.AddPropertyValue(TemplateDefinitionConstants.PropertyOverrideLogging, overrideLogging);
            }

            if (StringUtils.HasText(configFile)) {
                builder.AddPropertyValue(TemplateDefinitionConstants.PropertyConfigFile, configFile);
            }

            XmlNodeList childElements = element.ChildNodes;
            if (childElements.Count > 0) {
                ParseChildDefinitions(childElements, parserContext, builder);
            }
        }

        /// <summary>
        /// Parses child element definitions for the NVelocity engine. Typically resource loaders and locally defined properties are parsed here
        /// </summary>
        /// <param name="childElements">the XmlNodeList representing the child configuration of the root NVelocity engine element</param>
        /// <param name="parserContext">the parser context</param>
        /// <param name="builder">the ObjectDefinitionBuilder used to configure this object</param>
        private void ParseChildDefinitions(XmlNodeList childElements, ParserContext parserContext, ObjectDefinitionBuilder builder) {
            IDictionary<string, object> properties = new Dictionary<string, object>();

            foreach (XmlElement element in childElements) {
                switch (element.LocalName) {
                    case TemplateDefinitionConstants.ElementResourceLoader:
                        ParseResourceLoader(element, builder, properties);
                        break;
                    case TemplateDefinitionConstants.ElementNVelocityProperties:
                        ParseNVelocityProperties(element, parserContext, properties);
                        break;
                }
            }

            if (properties.Count > 0) {
                builder.AddPropertyValue(TemplateDefinitionConstants.PropertyVelocityProperties, properties);
            }
        }

        /// <summary>
        /// Configures the NVelocity resource loader definitions from the xml definition
        /// </summary>
        /// <param name="element">the root resource loader element</param>
        /// <param name="builder">the ObjectDefinitionBuilder used to configure this object</param>
        /// <param name="properties">the properties used to initialize the velocity engine</param>
        private void ParseResourceLoader(XmlElement element, ObjectDefinitionBuilder builder, IDictionary<string, object> properties) {
            string caching = GetAttributeValue(element, TemplateDefinitionConstants.AttributeTemplateCaching);
            string defaultCacheSize = GetAttributeValue(element, TemplateDefinitionConstants.AttributeDefaultCacheSize);
            string modificationCheckInterval = GetAttributeValue(element, TemplateDefinitionConstants.AttributeModificationCheckInterval);

            if (!string.IsNullOrEmpty(defaultCacheSize)) {
                properties.Add(RuntimeConstants.RESOURCE_MANAGER_DEFAULTCACHE_SIZE, defaultCacheSize);
            }

            XmlNodeList loaderElements = element.ChildNodes;
            switch (loaderElements[0].LocalName) {
                case VelocityConstants.File:
                    AppendFileLoaderProperties(loaderElements, properties);
                    AppendResourceLoaderGlobalProperties(properties, VelocityConstants.File, caching,
                                                         modificationCheckInterval);
                    break;
                case VelocityConstants.Assembly:
                    AppendAssemblyLoaderProperties(loaderElements, properties);
                    AppendResourceLoaderGlobalProperties(properties, VelocityConstants.Assembly, caching, null);
                    break;
                case TemplateDefinitionConstants.Spring:
                    AppendResourceLoaderPaths(loaderElements, builder);
                    AppendResourceLoaderGlobalProperties(properties, TemplateDefinitionConstants.Spring, caching, null);
                    break;
                case TemplateDefinitionConstants.Custom:
                    XmlElement firstElement = (XmlElement)loaderElements.Item(0);
                    AppendCustomLoaderProperties(firstElement, properties);
                    AppendResourceLoaderGlobalProperties(properties, firstElement.LocalName, caching, modificationCheckInterval);
                    break;
                default:
                    throw new ArgumentException(string.Format("undefined element for resource loadre type: {0}", element.LocalName));
            }
        }


        /// <summary>
        /// Set the caching and modification interval checking properties of a resource loader of a given type
        /// </summary>
        /// <param name="properties">the properties used to initialize the velocity engine</param>
        /// <param name="type">type of the resource loader</param>
        /// <param name="caching">caching flag</param>
        /// <param name="modificationInterval">modification interval value</param>
        private void AppendResourceLoaderGlobalProperties(IDictionary<string, object> properties, string type, string caching, string modificationInterval) {
            AppendResourceLoaderGlobalProperty(properties, type,
                                                 TemplateDefinitionConstants.PropertyResourceLoaderCaching,
                                                 Convert.ToBoolean(caching));
            AppendResourceLoaderGlobalProperty
                (properties, type, TemplateDefinitionConstants.PropertyResourceLoaderModificationCheckInterval, Convert.ToInt64(modificationInterval));
        }

        /// <summary>
        /// Set global velocity resource loader properties (caching, modification interval etc.)
        /// </summary>
        /// <param name="properties">the properties used to initialize the velocity engine</param>
        /// <param name="type">the type of resource loader</param>
        /// <param name="property">the suffix property</param>
        /// <param name="value">the value of the property</param>
        private void AppendResourceLoaderGlobalProperty(IDictionary<string, object> properties, string type, string property, object value) {
            if (null != value) {
                properties.Add(type + VelocityConstants.Separator + property, value);
            }
        }

        /// <summary>
        /// Creates a nvelocity file based resource loader by setting the required properties
        /// </summary>
        /// <param name="elements">a list of nv:file elements defining the paths to template files</param>
        /// <param name="properties">the properties used to initialize the velocity engine</param>
        private void AppendFileLoaderProperties(XmlNodeList elements, IDictionary<string, object> properties) {
            IList paths = new List<string>(elements.Count);
            foreach (XmlElement element in elements) {
                paths.Add(GetAttributeValue(element, VelocityConstants.Path));
            }
            properties.Add(RuntimeConstants.RESOURCE_LOADER, VelocityConstants.File);
            properties.Add(getResourceLoaderProperty(VelocityConstants.File, VelocityConstants.Class), TemplateDefinitionConstants.FileResourceLoaderClass);
            properties.Add(getResourceLoaderProperty(VelocityConstants.File, VelocityConstants.Path), StringUtils.CollectionToCommaDelimitedString(paths));
        }

        /// <summary>
        /// Creates a nvelocity assembly based resource loader by setting the required properties
        /// </summary>
        /// <param name="elements">a list of nv:assembly elements defining the assemblies</param>
        /// <param name="properties">the properties used to initialize the velocity engine</param>
        private void AppendAssemblyLoaderProperties(XmlNodeList elements, IDictionary<string, object> properties) {
            IList assemblies = new List<string>(elements.Count);
            foreach (XmlElement element in elements) {
                assemblies.Add(GetAttributeValue(element, VelocityConstants.Name));
            }
            properties.Add(RuntimeConstants.RESOURCE_LOADER, VelocityConstants.Assembly);
            properties.Add(getResourceLoaderProperty(VelocityConstants.Assembly, VelocityConstants.Class), TemplateDefinitionConstants.AssemblyResourceLoaderClass);
            properties.Add(getResourceLoaderProperty(VelocityConstants.Assembly, VelocityConstants.Assembly), StringUtils.CollectionToCommaDelimitedString(assemblies));
        }

        /// <summary>
        /// Creates a spring resource loader by setting the ResourceLoaderPaths of the
        /// engine factory (the resource loader itself will be created internally either as 
        /// spring or as file resource loader based on the value of prefer-file-system-access
        /// attribute).
        /// </summary>
        /// <param name="elements">list of resource loader path elements</param>
        /// <param name="builder">the object definiton builder to set the property for the engine factory</param>
        private void AppendResourceLoaderPaths(XmlNodeList elements, ObjectDefinitionBuilder builder) {
            IList<string> paths = new List<string>();
            foreach (XmlElement element in elements) {
                string path = GetAttributeValue(element, TemplateDefinitionConstants.AttributeUri);
                paths.Add(path);
            }
            builder.AddPropertyValue(TemplateDefinitionConstants.PropertyResourceLoaderPaths, paths);
        }

        /// <summary>
        /// Create a custom resource loader from an nv:custom element 
        /// generates the 4 required nvelocity props for a resource loader (name, description, class and path).
        /// </summary>
        /// <param name="element">the nv:custom xml definition element</param>
        /// <param name="properties">the properties used to initialize the velocity engine</param>
        private void AppendCustomLoaderProperties(XmlElement element, IDictionary<string, object> properties) {
            string name = GetAttributeValue(element, VelocityConstants.Name);
            string description = GetAttributeValue(element, VelocityConstants.Description);
            string type = GetAttributeValue(element, VelocityConstants.Type);
            string path = GetAttributeValue(element, VelocityConstants.Path);
            properties.Add(RuntimeConstants.RESOURCE_LOADER, name);
            properties.Add(getResourceLoaderProperty(name, VelocityConstants.Description), description);
            properties.Add(getResourceLoaderProperty(name, VelocityConstants.Class), type.Replace(',', ';'));
            properties.Add(getResourceLoaderProperty(name, VelocityConstants.Path), path);
        }

        /// <summary>
        /// Parses the nvelocity properties map using <code>ObjectNamespaceParserHelper</code> 
        /// and appends it to the properties dictionary
        /// </summary>
        /// <param name="element">root element of the map element</param>
        /// <param name="parserContext">the parser context</param>
        /// <param name="properties">the properties used to initialize the velocity engine</param>
        private void ParseNVelocityProperties(XmlElement element, ParserContext parserContext, IDictionary<string, object> properties) {
            IDictionary parsedProperties = objectNamespaceParserHelper.ParseDictionaryElementInternal(element, 
                TemplateDefinitionConstants.ElementNVelocityProperties, parserContext);
            foreach (DictionaryEntry entry in parsedProperties) {
                properties.Add(Convert.ToString(entry.Key), entry.Value);
            }
        }

        /// <summary>
        /// construct the element type name (e.g, nv:engine, nv:resource-loader)
        /// </summary>
        /// <param name="element">the current xml element</param>
        protected override string GetObjectTypeName(XmlElement element) {
            return TemplateTypePrefix + element.LocalName;
        }

        /// <summary>
        /// constructs an nvelocity style resource loader property in the format:
        /// prefix.resource.loader.suffix
        /// </summary>
        /// <param name="type">the prefix</param>
        /// <param name="suffix">the suffix</param>
        /// <returns>a concatenated string like: prefix.resource.loader.suffix</returns>
        public static string getResourceLoaderProperty(string type, string suffix) {
            return type + VelocityConstants.Separator + RuntimeConstants.RESOURCE_LOADER + VelocityConstants.Separator +
                   suffix;
        }
    }



    /// <summary>
    /// Helper class to access the ParseDictionaryElement of the ObjectsNamespaceParser
    /// todo: remove this and externalize the logic in ObjectsNamespaceParser.ParseDictionaryElement
    /// </summary>
    internal class ObjectNamespaceParserHelper : ObjectsNamespaceParser {
        /// <summary>
        /// Gets a dictionary definition.
        /// </summary>
        /// <param name="mapEle">The element describing the dictionary definition.</param>
        /// <param name="name">The name of the object (definition) associated with the dictionary definition.</param>
        /// <param name="parserContext">The namespace-aware parser.</param>
        /// <returns>The dictionary definition.</returns>
        public IDictionary ParseDictionaryElementInternal(XmlElement mapEle, string name, ParserContext parserContext) {
            ManagedDictionary dictionary = new ManagedDictionary();
            string keyTypeName = GetAttributeValue(mapEle, "key-type");
            string valueTypeName = GetAttributeValue(mapEle, "value-type");
            if (StringUtils.HasText(keyTypeName)) {
                dictionary.KeyTypeName = keyTypeName;
            }
            if (StringUtils.HasText(valueTypeName)) {
                dictionary.ValueTypeName = valueTypeName;
            }
            dictionary.MergeEnabled = ParseMergeAttribute(mapEle, parserContext.ParserHelper);

            XmlNodeList entryElements = SelectNodes(mapEle, ObjectDefinitionConstants.EntryElement);
            foreach (XmlElement entryEle in entryElements) {
                #region Key

                object key = null;

                XmlAttribute keyAtt = entryEle.Attributes[ObjectDefinitionConstants.KeyAttribute];
                if (keyAtt != null) {
                    key = keyAtt.Value;
                } else {
                    // ok, we're not using the 'key' attribute; lets check for the ref shortcut...
                    XmlAttribute keyRefAtt = entryEle.Attributes[ObjectDefinitionConstants.DictionaryKeyRefShortcutAttribute];
                    if (keyRefAtt != null) {
                        key = new RuntimeObjectReference(keyRefAtt.Value);
                    } else {
                        // so check for the 'key' element...
                        XmlNode keyNode = SelectSingleNode(entryEle, ObjectDefinitionConstants.KeyElement);
                        if (keyNode == null) {
                            throw new ObjectDefinitionStoreException(
                                parserContext.ReaderContext.Resource, name,
                                string.Format("One of either the '{0}' element, or the the '{1}' or '{2}' attributes " +
                                              "is required for the <{3}/> element.",
                                              ObjectDefinitionConstants.KeyElement,
                                              ObjectDefinitionConstants.KeyAttribute,
                                              ObjectDefinitionConstants.DictionaryKeyRefShortcutAttribute,
                                              ObjectDefinitionConstants.EntryElement));
                        }
                        XmlElement keyElement = (XmlElement)keyNode;
                        XmlNodeList keyNodes = keyElement.GetElementsByTagName("*");
                        if (keyNodes == null || keyNodes.Count == 0) {
                            throw new ObjectDefinitionStoreException(
                                parserContext.ReaderContext.Resource, name,
                                string.Format("Malformed <{0}/> element... the value of the key must be " +
                                    "specified as a child value-style element.",
                                              ObjectDefinitionConstants.KeyElement));
                        }
                        key = ParsePropertySubElement((XmlElement)keyNodes.Item(0), name, parserContext);
                    }
                }

                #endregion

                #region Value

                XmlAttribute inlineValueAtt = entryEle.Attributes[ObjectDefinitionConstants.ValueAttribute];
                if (inlineValueAtt != null) {
                    // ok, we're using the value attribute shortcut...
                    dictionary[key] = inlineValueAtt.Value;
                } else if (entryEle.Attributes[ObjectDefinitionConstants.DictionaryValueRefShortcutAttribute] != null) {
                    // ok, we're using the value-ref attribute shortcut...
                    XmlAttribute inlineValueRefAtt = entryEle.Attributes[ObjectDefinitionConstants.DictionaryValueRefShortcutAttribute];
                    RuntimeObjectReference ror = new RuntimeObjectReference(inlineValueRefAtt.Value);
                    dictionary[key] = ror;
                } else if (entryEle.Attributes[ObjectDefinitionConstants.ExpressionAttribute] != null) {
                    // ok, we're using the expression attribute shortcut...
                    XmlAttribute inlineExpressionAtt = entryEle.Attributes[ObjectDefinitionConstants.ExpressionAttribute];
                    ExpressionHolder expHolder = new ExpressionHolder(inlineExpressionAtt.Value);
                    dictionary[key] = expHolder;
                } else {
                    XmlNode keyNode = SelectSingleNode(entryEle, ObjectDefinitionConstants.KeyElement);
                    if (keyNode != null) {
                        entryEle.RemoveChild(keyNode);
                    }
                    // ok, we're using the original full-on value element...
                    XmlNodeList valueElements = entryEle.GetElementsByTagName("*");
                    if (valueElements == null || valueElements.Count == 0) {
                        throw new ObjectDefinitionStoreException(
                            parserContext.ReaderContext.Resource, name,
                            string.Format("One of either the '{0}' or '{1}' attributes, or a value-style element " +
                                "is required for the <{2}/> element.",
                                          ObjectDefinitionConstants.ValueAttribute, ObjectDefinitionConstants.DictionaryValueRefShortcutAttribute, ObjectDefinitionConstants.EntryElement));
                    }
                    dictionary[key] = ParsePropertySubElement((XmlElement)valueElements.Item(0), name, parserContext);
                }

                #endregion
            }
            return dictionary;
        }
        private bool ParseMergeAttribute(XmlElement collectionElement, ObjectDefinitionParserHelper helper) {
            string val = collectionElement.GetAttribute(ObjectDefinitionConstants.MergeAttribute);
            if (ObjectDefinitionConstants.DefaultValue.Equals(val)) {
                val = helper.Defaults.Merge;
            }
            return ObjectDefinitionConstants.TrueValue.Equals(val);
        }

       /// <summary>
        ///  This method overrides SelectNodes from ObjectsNamespaceParser because the original method 
        /// picks up the NamespaceURI from the nvelocity which causes element.SelectNodes to return an empty 
        /// list. Consider removing. 
       /// </summary>
        protected new XmlNodeList SelectNodes(XmlElement element, string childElementName) {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(new NameTable());
            nsManager.AddNamespace(GetNamespacePrefix(element), Namespace);
            return element.SelectNodes(GetNamespacePrefix(element) + ":" + childElementName, nsManager);
        }


        private string GetNamespacePrefix(XmlElement element) {
            return StringUtils.HasText(element.Prefix) ? element.Prefix : "spring";
        }
    }
    
    #region Element & Attribute Name Constants

        /// <summary>
        /// Template definition constants
        /// </summary>
        public class TemplateDefinitionConstants {
            /// <summary>
            /// Engine element definition
            /// </summary>
            public const string NVelocityElement = "engine";

            /// <summary>
            /// Spring resource loader element definition
            /// </summary>
            public const string Spring = "spring";

            /// <summary>
            /// Custom resource loader element definition
            /// </summary>
            public const string Custom = "custom";

            /// <summary>
            /// uri attribute of the spring element
            /// </summary>
            public const string AttributeUri = "uri";

            /// <summary>
            /// prefer-file-system-access attribute of the engine factory
            /// </summary>
            public const string AttributePreferFileSystemAccess = "prefer-file-system-access";

            /// <summary>
            /// config-file attribute of the engine factory
            /// </summary>
            public const string AttributeConfigFile = "config-file";

            /// <summary>
            /// override-logging attribute of the engine factory
            /// </summary>
            public const string AttributeOverrideLogging = "override-logging";

            /// <summary>
            /// template-caching attribute of the nvelocity engine
            /// </summary>
            public const string AttributeTemplateCaching = "template-caching";

            /// <summary>
            /// default-cache-size attribute of the nvelocity engine resource manager
            /// </summary>
            public const string AttributeDefaultCacheSize = "default-cache-size";

            /// <summary>
            /// modification-check-interval attribute of the nvelocity engine resource loader
            /// </summary>
            public const string AttributeModificationCheckInterval = "modification-check-interval";

            /// <summary>
            /// resource loader element
            /// </summary>
            public const string ElementResourceLoader = "resource-loader";

            /// <summary>
            /// nvelocity propeties element (map)
            /// </summary>
            public const string ElementNVelocityProperties = "nvelocity-properties";

            /// <summary>
            /// PreferFileSystemAccess property of the engine factory
            /// </summary>
            public const string PropertyPreferFileSystemAccess = "PreferFileSystemAccess";

            /// <summary>
            /// OverrideLogging property of the engine factory
            /// </summary>
            public const string PropertyOverrideLogging = "OverrideLogging";

            /// <summary>
            /// ConfigLocation property of the engine factory
            /// </summary>
            public const string PropertyConfigFile = "ConfigLocation";

            /// <summary>
            /// ResourceLoaderPaths property of the engine factory
            /// </summary>
            public const string PropertyResourceLoaderPaths = "ResourceLoaderPaths";

            /// <summary>
            /// VelocityProperties property of the engine factory
            /// </summary>
            public const string PropertyVelocityProperties = "VelocityProperties";

            /// <summary>
            /// resource.loader.cache property of the resource loader configuration 
            /// </summary>
            public const string PropertyResourceLoaderCaching = "resource.loader.cache";

            /// <summary>
            /// resource.loader.modificationCheckInterval property of the resource loader configuration 
            /// </summary>
            public const string PropertyResourceLoaderModificationCheckInterval = "resource.loader.modificationCheckInterval";

            /// <summary>
            /// the type used for file resource loader
            /// </summary>
            public const string FileResourceLoaderClass = "NVelocity.Runtime.Resource.Loader.FileResourceLoader; NVelocity";

            /// <summary>
            /// the type used for assembly resource loader
            /// </summary>
            public const string AssemblyResourceLoaderClass = "NVelocity.Runtime.Resource.Loader.AssemblyResourceLoader; NVelocity";

            /// <summary>
            /// the type used for spring resource loader
            /// </summary>
            public const string SpringResourceLoaderClass = "Spring.Template.Velocity.SpringResourceLoader; Spring.Template.Velocity";
        }
        #endregion
}