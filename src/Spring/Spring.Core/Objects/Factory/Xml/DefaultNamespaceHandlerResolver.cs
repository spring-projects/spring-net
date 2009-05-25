#region License

/*
 * Copyright 2002-2009 the original author or authors.
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
    /// Default implementation of the <see cref="INamespaceParserResolver"/> interface.
    /// Resolves namespace URIs to implementation types based on mappings.
    /// </summary>
    /// <author>Erich Eichinger</author>
    /// <seealso cref="INamespaceParser"/>
    /// <seealso cref="DefaultObjectDefinitionDocumentReader"/>
    internal class DefaultNamespaceHandlerResolver : INamespaceParserResolver
    {
        /// <summary>
        /// Resolve the namespace URI and return the corresponding <see cref="INamespaceParser"/>
        /// implementation.
        /// </summary>
        /// <param name="namespaceUri">the namespace URI to get the matching parser for.</param>
        /// <returns>the matching parser or <c>null</c></returns>
        public INamespaceParser Resolve(string namespaceUri)
        {
            return NamespaceParserRegistry.GetParser(namespaceUri);
        }
    }
}