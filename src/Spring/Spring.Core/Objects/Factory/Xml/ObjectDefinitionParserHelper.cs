#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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

using System;
using System.Collections;
using System.Globalization;
using System.Xml;
using Common.Logging;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Util;

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Stateful class used to parse XML object definitions.
    /// </summary>
    /// <remarks>Not all parsing code has been refactored into this class. See 
    /// BeanDefinitionParserDelegate in Java for how this class should evolve.</remarks>
    /// <author>Rob Harrop</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Rod Johnson</author>
    /// <author>Mark Pollack (.NET)</author>
    public class ObjectDefinitionParserHelper
    {

        #region Fields
        /// <summary>
        /// The shared <see cref="Common.Logging.ILog"/> instance for this class (and derived classes).
        /// </summary>
        protected static readonly ILog log =
            LogManager.GetLogger(typeof(ObjectDefinitionParserHelper));

        private DocumentDefaultsDefinition defaults;

        private XmlReaderContext readerContext;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDefinitionParserHelper"/> class.
        /// </summary>
        /// <param name="readerContext">The reader context.</param>
        public ObjectDefinitionParserHelper(XmlReaderContext readerContext)
        {
            AssertUtils.ArgumentNotNull(readerContext, "readerContext");
            this.readerContext = readerContext;
        }

        /// <summary>
        /// Gets the defaults definition object, or <code>null</code> if the 
        /// default have not yet been initialized.
        /// </summary>
        /// <value>The defaults.</value>
        public DocumentDefaultsDefinition Defaults
        {
            get { return defaults; }
        }


        /// <summary>
        /// Gets the reader context.
        /// </summary>
        /// <value>The reader context.</value>
        public XmlReaderContext ReaderContext
        {
            get { return readerContext; }
        }

        /// <summary>
        /// Initialize the default lazy-init, dependency check, and autowire settings.
        /// </summary>
        /// <param name="root">The root element</param>
        public void InitDefaults(XmlElement root)
        {
            DocumentDefaultsDefinition ddd = new DocumentDefaultsDefinition();
            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug("Loading object definitions...");
            }

            #endregion

            ddd.LazyInit = GetAttributeValue(root, ObjectDefinitionConstants.DefaultLazyInitAttribute);

            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug(
                    string.Format(
                        "Default lazy init '{0}'.",
                        ddd.LazyInit));
            }

            #endregion

            ddd.DependencyCheck = GetAttributeValue(root, ObjectDefinitionConstants.DefaultDependencyCheckAttribute);

            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug(
                    string.Format(
                        "Default dependency check '{0}'.",
                        ddd.DependencyCheck));
            }

            #endregion

            ddd.Autowire = GetAttributeValue(root, ObjectDefinitionConstants.DefaultAutowireAttribute);

            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug(
                    string.Format(
                        "Default autowire '{0}'.",
                        ddd.Autowire));
            }

            #endregion

            defaults = ddd;
        }


        /// <summary>
        /// Determines whether the Spring object namespace is equal to the the specified namespace URI.
        /// </summary>
        /// <param name="namespaceUri">The namespace URI.</param>
        /// <returns>
        /// 	<c>true</c> if is the default Spring namespace; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDefaultNamespace(string namespaceUri)
        {
            return
                (!StringUtils.HasLength(namespaceUri) || ObjectsNamespaceParser.Namespace.Equals(namespaceUri));
        }


        /// <summary>
        /// Decorates the object definition if required.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="holder">The holder.</param>
        /// <returns></returns>
        public ObjectDefinitionHolder DecorateObjectDefinitionIfRequired(XmlElement element, ObjectDefinitionHolder holder)
        {

            //TODO decoration processing.
            return holder;
        }

        /// <summary>
        /// Parse a standard object definition into a
        /// <see cref="Spring.Objects.Factory.Config.ObjectDefinitionHolder"/>,
        /// including object name and aliases.
        /// </summary>
        /// <param name="element">The element containing the object definition.</param>
        /// <returns>
        /// The parsed object definition wrapped within an
        /// <see cref="Spring.Objects.Factory.Config.ObjectDefinitionHolder"/>
        /// instance.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Object elements specify their canonical name via the "id" attribute
        /// and their aliases as a delimited "name" attribute.
        /// </para>
        /// <para>
        /// If no "id" is specified, uses the first name in the "name" attribute
        /// as the canonical name, registering all others as aliases.
        /// </para>
        /// </remarks>
        public ObjectDefinitionHolder ParseObjectDefinitionElement(XmlElement element)
        {
            return ParseObjectDefinitionElement(element, null);
        }

        /// <summary>
        /// Parse a standard object definition into a
        /// <see cref="Spring.Objects.Factory.Config.ObjectDefinitionHolder"/>,
        /// including object name and aliases.
        /// </summary>
        /// <param name="element">The element containing the object definition.</param>
        /// <param name="containingDefinition">The containing object definition if <paramref name="element"/> is a nested element.</param>
        /// <returns>
        /// The parsed object definition wrapped within an
        /// <see cref="Spring.Objects.Factory.Config.ObjectDefinitionHolder"/>
        /// instance.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Object elements specify their canonical name via the "id" attribute
        /// and their aliases as a delimited "name" attribute.
        /// </para>
        /// <para>
        /// If no "id" is specified, uses the first name in the "name" attribute
        /// as the canonical name, registering all others as aliases.
        /// </para>
        /// </remarks>
        public ObjectDefinitionHolder ParseObjectDefinitionElement(XmlElement element, IObjectDefinition containingDefinition)
        {
            // TODO: move code from ObjectsNamespaceParser into this class to eliminate ONP
            ObjectsNamespaceParser parser = (ObjectsNamespaceParser) NamespaceParserRegistry.GetParser(ObjectsNamespaceParser.Namespace);

            string id = GetAttributeValue(element, ObjectDefinitionConstants.IdAttribute);
            string nameAttr = GetAttributeValue(element, ObjectDefinitionConstants.NameAttribute);
            ArrayList aliases = new ArrayList();
            if (StringUtils.HasText(nameAttr))
            {
                aliases.AddRange(GetObjectNames(nameAttr));
            }

            // if we ain't got an id, check if object is page definition or assign any existing (first) alias...
            string objectName = id;
            if (StringUtils.IsNullOrEmpty(objectName))
            {
                // TODO (EE): pass parserContext to CalculateId as well (resolving relative Urls in WebApps is parserContext-dependent) (EE)
                objectName = parser.CalculateId(element, aliases);
            }


            ParserContext parserContext = new ParserContext(this, containingDefinition);
            IConfigurableObjectDefinition definition = parser.ParseObjectDefinitionElement(element, objectName, parserContext);
            if (definition != null)
            {
                if (StringUtils.IsNullOrEmpty(objectName))
                {
                    if (containingDefinition != null)
                    {
                        objectName =
                            ObjectDefinitionReaderUtils.GenerateObjectName(definition, parserContext.Registry, true);
                    }
                    else
                    {
                        objectName = ObjectDefinitionReaderUtils.GenerateObjectName(definition, parserContext.Registry);
                    }

                    #region Instrumentation

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(string.Format(
                                      "Neither XML '{0}' nor '{1}' specified - using object " +
                                      "class name [{2}] as the id.",
                                      id, ObjectDefinitionConstants.IdAttribute, ObjectDefinitionConstants.NameAttribute));
                    }

                    #endregion
                }
                string[] aliasesArray = (string[])aliases.ToArray(typeof(string));
                return new ObjectDefinitionHolder(definition, objectName, aliasesArray);
            }
            return null;
        }

        /// <summary>
        /// Parses an element in a custom namespace.
        /// </summary>
        /// <param name="ele"></param>
        /// <returns>the parsed object definition or null if not supported by the corresponding parser.</returns>
        public IObjectDefinition ParseCustomElement(XmlElement ele)
        {
            return ParseCustomElement(ele, null);
        }

        /// <summary>
        /// Parses an element in a custom namespace.
        /// </summary>
        /// <param name="ele"></param>
        /// <param name="containingDefinition">if a nested element, the containing object definition</param>
        /// <returns>the parsed object definition or null if not supported by the corresponding parser.</returns>
        public IObjectDefinition ParseCustomElement(XmlElement ele, IObjectDefinition containingDefinition)
        {
            String namespaceUri = ele.NamespaceURI;
            INamespaceParser handler = NamespaceParserRegistry.GetParser(namespaceUri);
            if (handler == null)
            {
                Error("Unable to locate Spring NamespaceHandler for XML schema namespace [" + namespaceUri + "]", ele);
                return null;
            }
            return handler.ParseElement(ele, new ParserContext(this, containingDefinition));
        }

        /// <summary>
        /// Given a string containing delimited object names, returns
        /// a string array split on the object name delimeter.
        /// </summary>
        /// <param name="value">
        /// The string containing delimited object names.
        /// </param>
        /// <returns>
        /// A string array split on the object name delimeter.
        /// </returns>
        /// <seealso cref="ObjectDefinitionConstants.ObjectNameDelimiters"/>
        private string[] GetObjectNames(string value)
        {
            return StringUtils.Split(
                value, ObjectDefinitionConstants.ObjectNameDelimiters, true, true);
        }

        /// <summary>
        /// Determines whether the string represents a 'true' boolean value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if is 'true' string value; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTrueStringValue(string value)
        {
            return ObjectDefinitionConstants.TrueValue.Equals(value.ToLower(CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Convenience method to create a builder for a root object definition.
        /// </summary>
        /// <param name="objectTypeName">Name of the object type.</param>
        /// <returns>A builder for a root object definition.</returns>
        public ObjectDefinitionBuilder CreateRootObjectDefinitionBuilder(string objectTypeName)
        {
            return ObjectDefinitionBuilder.RootObjectDefinition(this.readerContext.ObjectDefinitionFactory, objectTypeName);
        }

        /// <summary>
        /// Convenience method to create a builder for a root object definition.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>a builder for a root object definition</returns>
        public ObjectDefinitionBuilder CreateRootObjectDefinitionBuilder(Type objectType)
        {
            return ObjectDefinitionBuilder.RootObjectDefinition(this.readerContext.ObjectDefinitionFactory, objectType);
        }

        /// <summary>
        /// Returns the value of the element's attribute or <c>null</c>, if the attribute is not specified.
        /// </summary>
        /// <remarks>
        /// This is a helper for bypassing the behavior of <see cref="XmlElement.GetAttribute(string)"/> 
        /// to return <see cref="string.Empty"/> if the attribute does not exist.
        /// </remarks>
        public string GetAttributeValue(XmlElement element, string attributeName)
        {
            if (element.HasAttribute(attributeName))
            {
                return element.GetAttribute(attributeName);
            }
            return null;
        }

        /// <summary>
        /// Returns the value of the element's attribute or <paramref name="defaultValue"/>, 
        /// if the attribute is not specified.
        /// </summary>
        /// <remarks>
        /// This is a helper for bypassing the behavior of <see cref="XmlElement.GetAttribute(string)"/> 
        /// to return <see cref="string.Empty"/> if the attribute does not exist.
        /// </remarks>
        public string GetAttributeValue(XmlElement element, string attributeName, string defaultValue)
        {
            if (element.HasAttribute(attributeName))
            {
                return element.GetAttribute(attributeName);
            }
            return defaultValue;
        }

        /// <summary>
        /// Report a parser error.
        /// </summary>
        protected virtual void Error(string message, XmlElement element)
        {
            this.ReaderContext.ReportFatalException(element, message);
        }
    }
}
