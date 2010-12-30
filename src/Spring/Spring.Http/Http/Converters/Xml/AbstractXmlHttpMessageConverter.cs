#region License

/*
 * Copyright 2002-2011 the original author or authors.
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

using System.IO;
using System.Xml;
using System.Net;
using System.Text;

using Spring.Util;

namespace Spring.Http.Converters.Xml
{
    /// <summary>
    /// Base class for <see cref="IHttpMessageConverter"/> that convert from/to XML.
    /// </summary>
    /// <remarks>
    /// By default, subclasses of this converter support 'text/xml', 'application/xml', and 'application/*-xml' media types. 
    /// This can be overridden by setting the <see cref="P:SupportedMediaTypes"/> property.
    /// </remarks>
    /// <author>Bruno Baia</author>
    public abstract class AbstractXmlHttpMessageConverter : AbstractHttpMessageConverter
    {
        /// <summary>
        /// Default encoding for XML.
        /// </summary>
        public static readonly Encoding DEFAULT_CHARSET = new UTF8Encoding(false); // Remove byte Order Mask (BOM)

        /// <summary>
        /// Creates a new instance of the <see cref="AbstractHttpMessageConverter"/> 
        /// with multiple supported media type.
        /// </summary>
        /// <param name="supportedMediaTypes">The supported media types.</param>
        protected AbstractXmlHttpMessageConverter(params MediaType[] supportedMediaTypes) :
            base(supportedMediaTypes)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AbstractHttpMessageConverter"/> that sets 
        /// the <see cref="P:SupportedMediaTypes"/> to 'text/xml' and 'application/xml', and 'application/*-xml'.
        /// </summary>
        protected AbstractXmlHttpMessageConverter() :
            base(new MediaType("application", "xml"), new MediaType("text", "xml"), new MediaType("application", "*+xml"))
        {
        }

        /// <summary>
        /// Abstract template method that reads the actualy object. Invoked from <see cref="M:Read"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="message">The HTTP message to read from.</param>
        /// <returns>The converted object.</returns>
        /// <exception cref="HttpMessageNotReadableException">In case of conversion errors</exception>
        protected override T ReadInternal<T>(IHttpInputMessage message)
        {
            XmlReaderSettings settings = this.GetXmlReaderSettings();

            // Read from the message stream  
            using (XmlReader xmlReader = XmlReader.Create(message.Body, settings))
            {
                return ReadXml<T>(xmlReader);
            }
        }

        /// <summary>
        /// Abstract template method that writes the actual body. Invoked from <see cref="M:Write"/>.
        /// </summary>
        /// <param name="content">The object to write to the HTTP message.</param>
        /// <param name="message">The HTTP message to write to.</param>
        /// <exception cref="HttpMessageNotWritableException">In case of conversion errors</exception>
        protected override void WriteInternal(object content, IHttpOutputMessage message)
        {
            // Get the message encoding
            Encoding encoding;
            MediaType mediaType = message.Headers.ContentType;
            if (mediaType == null || !StringUtils.HasText(mediaType.CharSet))
            {
                encoding = DEFAULT_CHARSET;
            }
            else
            {
                encoding = Encoding.GetEncoding(mediaType.CharSet);
            }

            XmlWriterSettings settings = this.GetXmlWriterSettings();
            settings.Encoding = encoding;

            // Write to the message stream
            message.Body = delegate(Stream stream)
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stream, settings))
                {
                    WriteXml(xmlWriter, content);
                }
            };
        }

        /// <summary>
        /// Abstract template method that reads the actualy object using a <see cref="XmlReader"/>. Invoked from <see cref="M:ReadInternal"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="xmlReader">The XmlReader to use.</param>
        /// <returns>The converted object.</returns>
        protected abstract T ReadXml<T>(XmlReader xmlReader) where T : class;

        /// <summary>
        /// Abstract template method that writes the actual body using a <see cref="XmlWriter"/>. Invoked from <see cref="M:WriteInternal"/>.
        /// </summary>
        /// <param name="xmlWriter">The XmlWriter to use.</param>
        /// <param name="content">The object to write to the HTTP message.</param>
        protected abstract void WriteXml(XmlWriter xmlWriter, object content);

        /// <summary>
        /// Returns the <see cref="XmlReaderSettings">XmlReader settings</see> 
        /// used by this converter to read from the HTTP message.
        /// </summary>
        /// <returns>The XmlReader settings.</returns>
        protected virtual XmlReaderSettings GetXmlReaderSettings()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Auto;
            settings.CloseInput = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            return settings;
        }

        /// <summary>
        /// Returns the <see cref="XmlWriterSettings">XmlWriter settings</see> 
        /// used by this converter to write to the HTTP message.
        /// </summary>
        /// <returns>The XmlWriter settings.</returns>
        protected virtual XmlWriterSettings GetXmlWriterSettings()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.CloseOutput = false;
            settings.NewLineHandling = NewLineHandling.Entitize;
            settings.OmitXmlDeclaration = true;
            settings.CheckCharacters = false;
            return settings;
        }
    }
}