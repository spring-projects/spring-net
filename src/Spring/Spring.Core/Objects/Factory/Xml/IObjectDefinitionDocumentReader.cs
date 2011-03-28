#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// SPI for parsing an XML document that contains Spring object definitions.
    /// Used by <see cref="XmlObjectDefinitionReader"/> for actually parsing a DOM
    /// document.
    /// </summary>
    /// <remarks>Instantiated per document to parse:  Implementations can hold state in
    /// instance variables during the execution of the <code>RegisterObjectDefinitions</code>
    /// method, for example global settings that are defined for all object definitions
    /// in the document.
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Rob Harrop</author>
    /// <author>Mark Pollack (.NET)</author>
    /// 
    public interface IObjectDefinitionDocumentReader
    {
        /// <summary>
        /// Read object definitions from the given DOM element, and register
        /// them with the given object registry.
        /// </summary>
        /// <param name="doc">The DOM element containing object definitions, usually the
        /// root (document) element.</param>
        /// <param name="readerContext">The current context of the reader.  Includes 
        /// the resource being parsed</param>
        /// <returns>
        /// The number of object definitions that were loaded.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of parsing errors.
        /// </exception>
        void RegisterObjectDefinitions(XmlDocument doc, XmlReaderContext readerContext);
    }
}