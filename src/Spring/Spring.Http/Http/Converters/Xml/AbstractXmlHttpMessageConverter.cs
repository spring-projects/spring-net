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
        public static readonly Encoding DEFAULT_CHARSET = new UTF8Encoding(false); // Remove byte Order Mask (BOM) when using XmlTextWriter

        private XmlReaderSettings _xmlReaderSettings;

        /// <summary>
        /// Gets or sets the <see cref="XmlReaderSettings">XmlReader settings</see> 
        /// used by this converter to read from the HTTP response.
        /// </summary>
        public XmlReaderSettings XmlReaderSettings
        {
            get 
            {
                if (_xmlReaderSettings == null)
                {
                    _xmlReaderSettings = this.GetDefaultXmlReaderSettings();
                }
                return _xmlReaderSettings; 
            }
            set { _xmlReaderSettings = value; }
        }

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
        /// <param name="response">The HTTP response to read from.</param>
        /// <returns>The converted object.</returns>
        protected override T ReadInternal<T>(HttpWebResponse response)
        {
            using (Stream stream = response.GetResponseStream())
            {
                using (XmlReader xmlReader = XmlReader.Create(stream, this.XmlReaderSettings))
                {
                    return ReadXml<T>(xmlReader, response);
                }                
            }
        }

        /// <summary>
        /// Abstract template method that writes the actual body. Invoked from <see cref="M:Write"/>.
        /// </summary>
        /// <param name="content">The object to write to the HTTP request.</param>
        /// <param name="request">The HTTP request to write to.</param>
        protected override void WriteInternal(object content, HttpWebRequest request)
        {
            // Get the request encoding
            Encoding encoding;
            MediaType mediaType = MediaType.ParseMediaType(request.ContentType);
            if (mediaType == null || !StringUtils.HasText(mediaType.CharSet))
            {
                encoding = DEFAULT_CHARSET;
            }
            else
            {
                encoding = Encoding.GetEncoding(mediaType.CharSet);
            }

            // Write to the request
            using (IgnoreCloseMemoryStream requestStream = new IgnoreCloseMemoryStream())
            {
                using (XmlTextWriter xmlWriter = new XmlTextWriter(requestStream, encoding))
                {
                    WriteXml(xmlWriter, content, request);
                }

                // Set the content length in the request headers  
                request.ContentLength = requestStream.Length;

                using (Stream postStream = request.GetRequestStream())
                {
                    requestStream.CopyToAndClose(postStream);
                }
            }
        }

        /// <summary>
        /// Abstract template method that reads the actualy object using a <see cref="XmlReader"/>. Invoked from <see cref="M:ReadInternal"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="xmlReader">The XmlReader to use.</param>
        /// <param name="response">The HTTP response to read from.</param>
        /// <returns>The converted object.</returns>
        protected abstract T ReadXml<T>(XmlReader xmlReader, HttpWebResponse response) where T : class;

        /// <summary>
        /// Abstract template method that writes the actual body using a <see cref="XmlWriter"/>. Invoked from <see cref="M:WriteInternal"/>.
        /// </summary>
        /// <param name="xmlWriter">The XmlWriter to use.</param>
        /// <param name="content">The object to write to the HTTP request.</param>
        /// <param name="request">The HTTP request to write to.</param>
        protected abstract void WriteXml(XmlWriter xmlWriter, object content, HttpWebRequest request);

        /// <summary>
        /// Returns the default <see cref="XmlReaderSettings">XmlReader settings</see> 
        /// used by this converter to read from the HTTP response.
        /// </summary>
        /// <returns>The XmlReader settings.</returns>
        protected virtual XmlReaderSettings GetDefaultXmlReaderSettings()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Auto;
            settings.CloseInput = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            return settings;
        }
    }
}