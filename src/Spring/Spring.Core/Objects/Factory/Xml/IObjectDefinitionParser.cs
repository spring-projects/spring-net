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

namespace Spring.Objects.Factory.Xml;

/// <summary>
/// Interface used to handle custom, top-level tags.
/// </summary>
/// <remarks>Implementations are free to turn the metadata in the custom tag into as
/// many <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/> as required.
/// </remarks>
/// <author>Rob Harrop</author>
/// <author>Mark Pollack (.NET)</author>
public interface IObjectDefinitionParser
{
    /// <summary>
    /// Parse the specified XmlElement and register the resulting
    /// ObjectDefinitions with the <see cref="ParserContext.Registry"/> IObjectDefinitionRegistry
    /// embedded in the supplied <see cref="ParserContext"/>
    /// </summary>
    /// <remarks>
    /// <p>
    /// This method is never invoked if the parser is namespace aware
    /// and was called to process the root node.
    /// </p>
    /// </remarks>
    /// <param name="element">
    /// The element to be parsed.
    /// </param>
    /// <param name="parserContext">
    /// The object encapsulating the current state of the parsing process.
    /// Provides access to a IObjectDefinitionRegistry
    /// </param>
    /// <returns>
    /// The primary object definition.
    /// </returns>
    IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext);
}
