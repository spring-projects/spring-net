#region License

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

#endregion

using System.Xml;
using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Xml
{

	/// <summary>
	/// Strategy interface for parsing XML object definitions. Equivalent to Spring/Java's <c>NamespaceHandler</c> interface.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Used by <see cref="DefaultObjectDefinitionDocumentReader"/>
	/// for actually parsing a DOM document or
	/// <see cref="System.Xml.XmlElement"/> fragment.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	/// <author>Sandu Turcan (.NET)</author>
	public interface INamespaceParser
	{

        /// <summary>
        /// Invoked by <see cref="NamespaceParserRegistry"/> after construction but before any
        /// elements have been parsed.
        /// </summary>
        void Init();


        /// <summary>
        /// Parse the specified element and register any resulting
        /// IObjectDefinitions with the IObjectDefinitionRegistry that is
        /// embedded in the supplied ParserContext.
        /// </summary>
        /// <remarks>
        /// Implementations should return the primary IObjectDefinition
        /// that results from the parse phase if they wish to used nested
        /// inside (for example) a <code>&lt;property&gt;</code> tag.
        /// <para>Implementations may return null if they will not
        /// be used in a nested scenario.
        /// </para>
        /// </remarks>
        /// <param name="element">The element to be parsed into one or more IObjectDefinitions</param>
        /// <param name="parserContext">The object encapsulating the current state of the parsing
        /// process.</param>
        /// <returns>
        /// The primary IObjectDefinition (can be null as explained above)
        /// </returns>
        IObjectDefinition ParseElement(XmlElement element,  ParserContext parserContext);


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
	    ObjectDefinitionHolder Decorate(XmlNode node, ObjectDefinitionHolder definition, ParserContext parserContext);
	}
}
