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

#region Imports

using System.Collections;
using System.Xml;

using NVelocity.Runtime;

using Spring.Core.TypeResolution;
using Spring.Objects;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

#endregion

namespace Spring.Template.Velocity.Config {
    /// <summary>
    /// Implementation of the custom configuration parser for template configurations
    /// based on
    /// <see cref="ObjectsNamespaceParser"/>
    /// </summary>
    /// <author>Erez Mazor</author>
    /// <see cref="ObjectsNamespaceParser"/>
    [
        NamespaceParser(
            Namespace = "http://www.springframework.net/nvelocity",
            SchemaLocationAssemblyHint = typeof(TemplateNamespaceParser),
            SchemaLocation = "/Spring.Template.Velocity.Config/spring-nvelocity-1.3.xsd")
    ]
    public class TemplateNamespaceParser : ObjectsNamespaceParser {
        private const string TemplateTypePrefix = "template: ";

        static TemplateNamespaceParser() {
            TypeRegistry.RegisterType(TemplateTypePrefix + TemplateDefinitionConstants.NVelocityElement, typeof(VelocityEngineFactoryObject));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateNamespaceParser"/> class.
        /// </summary>
        public TemplateNamespaceParser() {
        }

        /// <see cref="INamespaceParser"/>
        public override IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext) {
            string name = element.GetAttribute(ObjectDefinitionConstants.IdAttribute);
            IConfigurableObjectDefinition templateDefinition = ParseTemplateDefinition(element, parserContext);
            if (!StringUtils.HasText(name)) {
                name = ObjectDefinitionReaderUtils.GenerateObjectName(templateDefinition, parserContext.Registry);
            }
            parserContext.Registry.RegisterObjectDefinition(name, templateDefinition);
            return null;
        }

        /// <summary>
        /// Parse a template definition from the templating namespace
        /// </summary>
        /// <param name="element">the root element defining the templating object</param>
        /// <param name="parserContext">the parser context</param>
        /// <returns></returns>
        private IConfigurableObjectDefinition ParseTemplateDefinition(XmlElement element, ParserContext parserContext) {
            switch (element.LocalName) {
                case TemplateDefinitionConstants.NVelocityElement:
                    return ParseNVelocityEngine(element, parserContext);
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
        private IConfigurableObjectDefinition ParseNVelocityEngine(XmlElement element, ParserContext parserContext) {
            string typeName = GetTypeName(element);
            IConfigurableObjectDefinition configurableObjectDefinition = parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(
                typeName, null, parserContext.ReaderContext.Reader.Domain);

            string preferFileSystemAccess = GetAttributeValue(element, TemplateDefinitionConstants.AttributePreferFileSystemAccess);
            string overrideLogging = GetAttributeValue(element, TemplateDefinitionConstants.AttributeOverrideLogging);
            string configFile = GetAttributeValue(element, TemplateDefinitionConstants.AttributeConfigFile);

            MutablePropertyValues objectDefinitionProperties = new MutablePropertyValues();
            if (StringUtils.HasText(preferFileSystemAccess)) {
                objectDefinitionProperties.Add(TemplateDefinitionConstants.PropertyPreferFileSystemAccess, preferFileSystemAccess);
            }

            if (StringUtils.HasText(overrideLogging)) {
                objectDefinitionProperties.Add(TemplateDefinitionConstants.PropertyOverrideLogging, overrideLogging);
            }

            if (StringUtils.HasText(configFile)) {
                objectDefinitionProperties.Add(TemplateDefinitionConstants.PropertyConfigFile, configFile);
            }

            XmlNodeList childElements = element.ChildNodes;
            if (childElements.Count > 0) {
                ParseChildDefinitions(childElements, parserContext, objectDefinitionProperties);
            }
            configurableObjectDefinition.PropertyValues = objectDefinitionProperties;
            return configurableObjectDefinition;
        }

        /// <summary>
        /// Parses child element definitions for the NVelocity engine. Typically resource loaders and locally defined properties are parsed here
        /// </summary>
        /// <param name="childElements">the XmlNodeList representing the child configuration of the root NVelocity engine element</param>
        /// <param name="parserContext">the parser context</param>
        /// <param name="objectDefinitionProperties">the MutablePropertyValues used to configure this object</param>
        private void ParseChildDefinitions(XmlNodeList childElements, ParserContext parserContext, MutablePropertyValues objectDefinitionProperties) {
            IDictionary<string, object> properties = new Dictionary<string, object>();

            foreach (XmlElement element in childElements) {
                switch (element.LocalName) {
                    case TemplateDefinitionConstants.ElementResourceLoader:
                        ParseResourceLoader(element, objectDefinitionProperties, properties);
                        break;
                    case TemplateDefinitionConstants.ElementNVelocityProperties:
                        ParseNVelocityProperties(element, parserContext, properties);
                        break;
                }
            }

            if (properties.Count > 0) {
                objectDefinitionProperties.Add(TemplateDefinitionConstants.PropertyVelocityProperties, properties);
            }
        }

        /// <summary>
        /// Configures the NVelocity resource loader definitions from the xml definition
        /// </summary>
        /// <param name="element">the root resource loader element</param>
        /// <param name="objectDefinitionProperties">the MutablePropertyValues used to configure this object</param>
        /// <param name="properties">the properties used to initialize the velocity engine</param>
        private void ParseResourceLoader(XmlElement element, MutablePropertyValues objectDefinitionProperties, IDictionary<string, object> properties) {
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
                    AppendResourceLoaderPaths(loaderElements, objectDefinitionProperties);
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
            IList<string> paths = new List<string>(elements.Count);
            foreach (XmlElement element in elements)
            {
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
            IList<string> assemblies = new List<string>(elements.Count);
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
        /// <param name="objectDefinitionProperties">the MutablePropertyValues to set the property for the engine factory</param>
        private void AppendResourceLoaderPaths(XmlNodeList elements, MutablePropertyValues objectDefinitionProperties) {
            IList<string> paths = new List<string>();
            foreach (XmlElement element in elements) {
                string path = GetAttributeValue(element, TemplateDefinitionConstants.AttributeUri);
                paths.Add(path);
            }
            objectDefinitionProperties.Add(TemplateDefinitionConstants.PropertyResourceLoaderPaths, paths);
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
            IDictionary parsedProperties = ParseDictionaryElement(element,
                TemplateDefinitionConstants.ElementNVelocityProperties, parserContext);
            foreach (DictionaryEntry entry in parsedProperties) {
                properties.Add(Convert.ToString(entry.Key), entry.Value);
            }
        }

        /// <summary>
        /// Gets the name of the object type for the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The name of the object type.</returns>
        private string GetTypeName(XmlElement element) {
            string typeName = element.GetAttribute(ObjectDefinitionConstants.TypeAttribute);
            if (StringUtils.IsNullOrEmpty(typeName)) {
                return TemplateTypePrefix + element.LocalName;
            }
            return typeName;
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

        /// <summary>
        /// This method is overriden from ObjectsNamespaceParser since when invoked on
        /// sub-elements from the objets namespace (e.g., objects:objectMap for nvelocity
        /// property map) the <code>element.SelectNodes</code> fails because it is in
        /// the nvelocity custom namespace and not the object's namespace (http://www.springframwork.net)
        /// to amend this the object's namespace is added to the provided XmlNamespaceManager
        /// </summary>
        ///<param name="element"> The element to be searched in. </param>
        /// <param name="childElementName"> The name of the child nodes to look for.
        /// </param>
        /// <returns> The child <see cref="System.Xml.XmlNode"/>s of the supplied
        /// <paramref name="element"/> with the supplied <paramref name="childElementName"/>.
        /// </returns>
        /// <see cref="ObjectsNamespaceParser"/>
        [Obsolete("not used anymore - ObjectsNamespaceParser will be dropped with 2.x, use ObjectDefinitionParserHelper instead")]
        protected override XmlNodeList SelectNodes(XmlElement element, string childElementName) {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(new NameTable());
            nsManager.AddNamespace(GetNamespacePrefix(element), element.NamespaceURI);
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
