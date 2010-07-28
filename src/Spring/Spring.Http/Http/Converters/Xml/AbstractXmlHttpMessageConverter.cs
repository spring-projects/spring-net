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

using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Text;

namespace Spring.Http.Converters.Xml
{
    public abstract class AbstractXmlHttpMessageConverter : AbstractHttpMessageConverter
    {
        public static readonly Encoding DEFAULT_CHARSET = Encoding.UTF8;

        private XmlReaderSettings _xmlReaderSettings;

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

        /**
         * Construct an {@code AbstractHttpMessageConverter} with multiple supported media type.
         * @param supportedMediaTypes the supported media types
         */
        protected AbstractXmlHttpMessageConverter(params MediaType[] supportedMediaTypes) :
            base(supportedMediaTypes)
        {
        }

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

        protected override void WriteInternal(object content, HttpWebRequest request)
        {
            // Get the request encoding
            MediaType mediaType = MediaType.ParseMediaType(request.Headers[HttpRequestHeader.ContentType]);
            Encoding encoding;
            if (mediaType == null || String.IsNullOrEmpty(mediaType.CharSet))
            {
                encoding = DEFAULT_CHARSET;
            }
            else
            {
                encoding = Encoding.GetEncoding(mediaType.CharSet);
            }

            using (Stream postStream = request.GetRequestStream())
            {
                using (XmlTextWriter xmlWriter = new XmlTextWriter(postStream, encoding))
                {
                    WriteXml(xmlWriter, content, request);
                    xmlWriter.Flush();
                }

                // TODO : Don't work
                // Set the content length in the request headers  
                request.ContentLength = postStream.Length;
            }
        }

        protected virtual XmlReaderSettings GetDefaultXmlReaderSettings()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Auto;
            settings.CloseInput = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            return settings;
        }

        protected abstract T ReadXml<T>(XmlReader xmlReader, HttpWebResponse response) where T : class;

        protected abstract void WriteXml(XmlWriter xmlWriter, object content, HttpWebRequest request);
    }
}