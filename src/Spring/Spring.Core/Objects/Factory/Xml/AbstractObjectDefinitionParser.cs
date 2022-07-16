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
using Spring.Objects.Factory.Support;
using Spring.Util;

namespace Spring.Objects.Factory.Xml
{


    /// <summary>
    /// Abstract <see cref="IObjectDefinitionParser"/> implementation providing
    /// a number of convenience methods and a
    /// <see cref="AbstractObjectDefinitionParser.ParseInternal"/> template method
    /// that subclasses must override to provide the actual parsing logic.
    /// </summary>
    /// <remarks>
    /// Use this <see cref="IObjectDefinitionParser"/> implementation when you want
    /// to parse some arbitrarily complex XML into one or more
    /// <see cref="IObjectDefinition"/> ObjectDefinitions. If you just want to parse some
    /// XML into a single <code>IObjectDefinition</code>, you may wish to consider
    /// the simpler convenience extensions of this class, namely
    /// <see cref="AbstractSingleObjectDefinitionParser"/> and
    /// <see cref="AbstractSimpleObjectDefinitionParser"/>
    /// </remarks>
    /// <author>Rob Harrop</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans</author>
    /// <author>Mark Pollack (.NET)</author>
    public abstract class AbstractObjectDefinitionParser : IObjectDefinitionParser
    {
        /// <summary>
        /// Constant for the ID attribute
        /// </summary>
        public static readonly string ID_ATTRIBUTE = "id";

        #region Properties

        /// <summary>
        /// Gets a value indicating whether an ID should be generated instead of read
        /// from the passed in XmlElement.
        /// </summary>
        /// <remarks>Note that this flag is about always generating an ID; the parser
        /// won't even check for an "id" attribute in this case.
        /// </remarks>
        /// <value><c>true</c> if should generate id; otherwise, <c>false</c>.</value>
        protected virtual bool ShouldGenerateId
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether an ID should be generated instead if the
        /// passed in XmlElement does not specify an "id" attribute explicitly.
        /// </summary>
        /// <remarks>Disabled by default; subclasses can override this to enable ID generation
        /// as fallback: The parser will first check for an "id" attribute in this case,
        /// only falling back to a generated ID if no value was specified.</remarks>
        /// <value>
        /// 	<c>true</c> if should generate id if no value was specified; otherwise, <c>false</c>.
        /// </value>
        protected virtual bool ShouldGenerateIdAsFallback
        {
            get { return false; }
        }

        #endregion

        #region IObjectDefinitionParser Members


        /// <summary>
        /// Parse the specified XmlElement and register the resulting
        /// ObjectDefinitions with the <see cref="ParserContext.Registry"/> IObjectDefinitionRegistry
        /// embedded in the supplied <see cref="ParserContext"/>
        /// </summary>
        /// <param name="element">The element to be parsed.</param>
        /// <param name="parserContext">The object encapsulating the current state of the parsing process.
        /// Provides access to a IObjectDefinitionRegistry</param>
        /// <returns>The primary object definition.</returns>
        /// <remarks>
        /// 	<p>
        /// This method is never invoked if the parser is namespace aware
        /// and was called to process the root node.
        /// </p>
        /// </remarks>
        public virtual IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
        {
            AbstractObjectDefinition definition = ParseInternal(element, parserContext);

            if (!parserContext.IsNested)
            {
                string id = null;
                try
                {
                    id = ResolveId(element, definition, parserContext);
                    if (!StringUtils.HasText(id))
                    {
                        parserContext.ReaderContext.ReportException(element, "null",
                                "Id is required for element '" + element.LocalName + "' when used as a top-level tag", null);
                    }
                    ObjectDefinitionHolder holder = new ObjectDefinitionHolder(definition, id);
                    RegisterObjectDefinition(holder, parserContext.Registry);
                }
                catch (ObjectDefinitionStoreException ex)
                {
                    parserContext.ReaderContext.ReportException(element, id, ex.Message);
                    return null;
                }
            }
            return definition;


        }

        #endregion

        #region Methods

        /// <summary>
        /// Resolves the ID for the supplied <see cref="IObjectDefinition"/>.
        /// </summary>
        /// <remarks>
        /// When using <see cref="ShouldGenerateId"/> generation, a name is generated automatically.
        /// Otherwise, the ID is extracted from the "id" attribute, potentially with a
        /// <see cref="ShouldGenerateIdAsFallback"/> fallback to a generated id.
        /// </remarks>
        /// <param name="element">The element that the object definition has been built from.</param>
        /// <param name="definition">The object definition to be registered.</param>
        /// <param name="parserContext">The the object encapsulating the current state of the parsing process;
        /// provides access to a <see cref="IObjectDefinitionRegistry"/> </param>
        /// <returns>the resolved id</returns>
        /// <exception cref="ObjectDefinitionStoreException">
        /// if no unique name could be generated for the given object definition
        /// </exception>
        protected virtual string ResolveId(XmlElement element, AbstractObjectDefinition definition, ParserContext parserContext)
	    {

		    if (ShouldGenerateId) {
			    return parserContext.ReaderContext.GenerateObjectName(definition);
		    }
		    else {
                string id = GetAttributeValue(element, ID_ATTRIBUTE);
			    if (!StringUtils.HasText(id) && ShouldGenerateIdAsFallback) {
				    id = parserContext.ReaderContext.GenerateObjectName(definition);
			    }
			    return id;
		    }
	    }

        /// <summary>
        /// Registers the supplied <see cref="ObjectDefinitionHolder"/> with the supplied
        /// <see cref="IObjectDefinitionRegistry"/>.
        /// </summary>
        /// <remarks>Subclasses can override this method to control whether or not the supplied
        /// <see cref="ObjectDefinitionHolder"/> is actually even registered, or to
        /// register even more objects.
        /// <para>
        /// The default implementation registers the supplied <see cref="ObjectDefinitionHolder"/>
        /// with the supplied <see cref="ObjectDefinitionHolder"/> only if the <code>IsNested</code>
        /// parameter is <code>false</code>, because one typically does not want inner objects
        /// to be registered as top level objects.
        /// </para>
        /// </remarks>
        ///
        /// <param name="definition">The object definition to be registered.</param>
        /// <param name="registry">The registry that the bean is to be registered with.</param>
        protected virtual void RegisterObjectDefinition(ObjectDefinitionHolder definition, IObjectDefinitionRegistry registry)
        {
            ObjectDefinitionReaderUtils.RegisterObjectDefinition(definition, registry);
        }

        /// <summary>
        /// Returns the value of the element's attribute or <c>null</c>, if the attribute is not specified.
        /// </summary>
        /// <remarks>
        /// This is a helper for bypassing the behavior of <see cref="XmlElement.GetAttribute(string)"/>
        /// to return <see cref="string.Empty"/> if the attribute does not exist.
        /// </remarks>
        protected static string GetAttributeValue(XmlElement element, string attributeName)
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
        protected static string GetAttributeValue(XmlElement element, string attributeName, string defaultValue)
        {
            if (element.HasAttribute(attributeName))
            {
                return element.GetAttribute(attributeName);
            }
            return defaultValue;
        }

        #endregion


        #region Abstract Methods

        /// <summary>
        /// Central template method to actually parse the supplied XmlElement
        /// into one or more IObjectDefinitions.
        /// </summary>
        /// <param name="element">The element that is to be parsed into one or more <see cref="IObjectDefinition"/>s</param>
        /// <param name="parserContext">The the object encapsulating the current state of the parsing process;
        /// provides access to a <see cref="IObjectDefinitionRegistry"/> </param>
        /// <returns>The primary IObjectDefinition resulting from the parsing of the supplied XmlElement</returns>
        protected abstract AbstractObjectDefinition ParseInternal(XmlElement element, ParserContext parserContext);

        #endregion
    }
}
