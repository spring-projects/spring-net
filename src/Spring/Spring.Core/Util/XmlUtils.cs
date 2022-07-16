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

using System.Xml;
using System.Xml.Schema;

#endregion

namespace Spring.Util
{
    /// <summary>
    /// XML utility methods.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class XmlUtils
    {
        /// <summary>
        /// Gets an appropriate <see cref="System.Xml.XmlReader"/> implementation
        /// for the supplied <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="stream">The XML <see cref="System.IO.Stream"/> that is going to be read.</param>
        /// <param name="schemas">XML schemas that should be used for validation.</param>
        /// <param name="eventHandler">Validation event handler.</param>
        /// <returns>
        /// A validating <see cref="System.Xml.XmlReader"/> implementation.
        /// </returns>
        public static XmlReader CreateValidatingReader(Stream stream, XmlSchemaSet schemas, ValidationEventHandler eventHandler)
        {
            return CreateValidatingReader(stream, new XmlUrlResolver(), schemas, eventHandler);
        }

        /// <summary>
        /// Gets an appropriate <see cref="System.Xml.XmlReader"/> implementation
        /// for the supplied <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="stream">The XML <see cref="System.IO.Stream"/> that is going to be read.</param>
        /// <param name="xmlResolver"><see cref="XmlResolver"/> to be used for resolving external references</param>
        /// <param name="schemas">XML schemas that should be used for validation.</param>
        /// <param name="eventHandler">Validation event handler.</param>
        /// <returns>
        /// A validating <see cref="System.Xml.XmlReader"/> implementation.
        /// </returns>
        public static XmlReader CreateValidatingReader(Stream stream, XmlResolver xmlResolver, XmlSchemaSet schemas, ValidationEventHandler eventHandler)
        {
            lock (typeof(XmlUtils))
            {
                if (!schemas.IsCompiled)
                {
                    schemas.Compile();
                }

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas.XmlResolver = xmlResolver;
                settings.Schemas.Add(schemas);
                settings.ValidationType = ValidationType.Schema;
                if (eventHandler != null)
                {
                    settings.ValidationEventHandler += eventHandler;
                }

                return XmlReader.Create(stream, settings);
            }
        }

        /// <summary>
        /// Gets an appropriate <see cref="System.Xml.XmlReader"/> implementation
        /// for the supplied <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="stream">The XML <see cref="System.IO.Stream"/> that is going to be read.</param>
        /// <returns>
        /// A non-validating <see cref="System.Xml.XmlReader"/> implementation.
        /// </returns>
        public static XmlReader CreateReader(Stream stream)
        {
            return XmlReader.Create(stream);
        }
    }
}
