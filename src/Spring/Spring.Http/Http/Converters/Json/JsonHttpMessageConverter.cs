#if NET_3_5
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
using System.Xml;
using System.IO;
using System.Net;
using System.Text;
using System.Runtime.Serialization.Json;

using Spring.Util;

namespace Spring.Http.Converters.Json
{
    // TODO : Support for known types, etc...

    /// <summary>
    /// Implementation of <see cref="IHttpMessageConverter"/> that can read and write JSON.
    /// </summary>
    /// <remarks>
    /// By default, this converter supports 'application/json' media type. 
    /// This can be overridden by setting the <see cref="P:SupportedMediaTypes"/> property.
    /// </remarks>
    /// <author>Bruno Baia</author>
    public class JsonHttpMessageConverter : AbstractHttpMessageConverter
    {
        /// <summary>
        /// Default encoding for JSON.
        /// </summary>
        public static readonly Encoding DEFAULT_CHARSET = Encoding.UTF8;

        /// <summary>
        /// Creates a new instance of the <see cref="JsonHttpMessageConverter"/> 
        /// with the media type 'application/json'. 
        /// </summary>
        public JsonHttpMessageConverter() :
            base(new MediaType("application", "json"))
        {
        }

        /// <summary>
        /// Indicates whether the given class is supported by this converter.
        /// </summary>
        /// <param name="type">The type to test for support.</param>
        /// <returns><see langword="true"/> if supported; otherwise <see langword="false"/></returns>
        protected override bool Supports(Type type)
        {
            return true;
        }

        /// <summary>
        /// Abstract template method that reads the actualy object. Invoked from <see cref="M:Read"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="response">The HTTP response to read from.</param>
        /// <returns>The converted object.</returns>
        protected override T ReadInternal<T>(HttpWebResponse response)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            using (Stream stream = response.GetResponseStream())
            {
                return (T)serializer.ReadObject(stream) as T;
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

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(content.GetType());
            
            // Write to the request
            using (IgnoreCloseMemoryStream requestStream = new IgnoreCloseMemoryStream())
            {
                using (XmlDictionaryWriter jsonWriter = JsonReaderWriterFactory.CreateJsonWriter(requestStream, encoding, false))
                {
                    serializer.WriteObject(jsonWriter, content);
                }

                // Set the content length in the request headers  
                request.ContentLength = requestStream.Length;

                using (Stream postStream = request.GetRequestStream())
                {
                    requestStream.CopyToAndClose(postStream);
                }
            }
        }
    }
}
#endif