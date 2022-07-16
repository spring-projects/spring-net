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
using Spring.Objects.Factory.Config;
using Spring.Util;

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Support class for implementing custom namespace parsers.
    /// </summary>
    /// <remarks>Parsing of individual elements is done via a ObjectDefintionParser.
    /// Provides the RegisterObjectDefinitionParser for registering a ObjectDefintionParser
    /// to handle a specific element.</remarks>
    /// <author>Rob Harrop</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public abstract class NamespaceParserSupport : INamespaceParser
    {

        private readonly IDictionary<string, IObjectDefinitionParser> objectParsers = new Dictionary<string, IObjectDefinitionParser>();

        #region IXmlObjectDefinitionParser Members

        /// <summary>
        /// Invoked by <see cref="NamespaceParserRegistry"/> after construction but before any
        /// elements have been parsed.
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Parses an element under the root node, typically
        /// an object definition or import statement.
        /// </summary>
        /// <param name="element">
        /// The element to be parsed.
        /// </param>
        /// <param name="parserContext">
        /// The parser context.
        /// </param>
        /// <returns>
        /// The number of object defintions created from this element.
        /// </returns>
        public virtual IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
        {
            return FindParserForElement(element, parserContext).ParseElement(element, parserContext);
        }


        /// <summary>
        /// Parse the specified XmlNode and decorate the supplied ObjectDefinitionHolder,
        /// returning the decorated definition.
        /// </summary>
        /// <remarks>The XmlNode may either be an XmlAttribute or an XmlElement, depending on
        /// whether a custom attribute or element is being parsed.
        /// <para>Implementations may choose to return a completely new definition,
        /// which will replace the original definition in the resulting IApplicationContext/IObjectFactory.
        /// </para>
        /// <para>The supplied ParserContext can be used to register any additional objects needed to support
        /// the main definition.</para>
        /// </remarks>
        /// <param name="node">The source element or attribute that is to be parsed.</param>
        /// <param name="definition">The current object definition.</param>
        /// <param name="parserContext">The object encapsulating the current state of the parsing
        /// process.</param>
        /// <returns>The decorated definition (to be registered in the IApplicationContext/IObjectFactory),
        /// or simply the original object definition if no decoration is required.  A null value is strickly
        /// speaking invalid, but will leniently treated like the case where the original object definition
        /// gets returned.</returns>
        public ObjectDefinitionHolder Decorate(XmlNode node, ObjectDefinitionHolder definition,
                                               ParserContext parserContext)
        {
            return null;
        }

        private IObjectDefinitionParser FindParserForElement(XmlElement element, ParserContext parserContext)
        {
            IObjectDefinitionParser parser;
            if (!objectParsers.TryGetValue(element.LocalName, out parser))
            {
                parserContext.ReaderContext.ReportException(element, "unknown object name", "Cannot locate IObjectDefinitionParser for element ["
                    + element.LocalName + "]");
            }
            return parser;

        }

        /// <summary>
        /// Register the specified <see cref="IObjectDefinitionParser"/> for the given <paramref name="elementName"/>
        /// </summary>
        protected virtual void RegisterObjectDefinitionParser(string elementName, IObjectDefinitionParser parser)
        {
            AssertUtils.ArgumentNotNull(elementName, "elementName");
            AssertUtils.ArgumentNotNull(parser, "parser");
            objectParsers[elementName] = parser;
        }

        #endregion
    }
}
