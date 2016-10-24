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

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Used by <see cref="DefaultObjectDefinitionDocumentReader"/> to locate
    /// <see cref="INamespaceParser"/> implementations for a particular namespace URI.
    /// </summary>
    /// <remarks>TODO (EE): clarify naming of INamespaceParser (SPR/NET) vs. INamespaceHandler (SPR/Java), thus internal for now</remarks>
    /// <author>Erich Eichinger</author>
    /// <seealso cref="XmlObjectDefinitionReader.NamespaceParserResolver"/>
    /// <seealso cref="XmlObjectDefinitionReader.CreateDefaultNamespaceParserResolver"/>
    /// <seealso cref="XmlReaderContext.NamespaceParserResolver"/>
    internal interface INamespaceParserResolver
    {
        /// <summary>
        /// Lookup a <see cref="INamespaceParser"/> for the given namespace URI.
        /// </summary>
        /// <param name="namespaceUri">the namespace URI</param>
        /// <returns>the located namespace handler or <c>null</c></returns>
        INamespaceParser Resolve(string namespaceUri);
    }
}