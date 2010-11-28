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
using System.Net;
using System.Text;

using Spring.Util;

namespace Spring.Http.Converters
{
    /// <summary>
    /// Implementation of <see cref="IHttpMessageConverter"/> that can read and write strings.
    /// </summary>
    /// <remarks>
    /// By default, this converter supports all media types '*/*', and writes with a 'Content-Type' 
    /// of 'text/plain'. 
    /// This can be overridden by setting the <see cref="P:SupportedMediaTypes"/> property.
    /// </remarks>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public class StringHttpMessageConverter : AbstractHttpMessageConverter
    {
        /// <summary>
        /// Default encoding for strings.
        /// </summary>
        public static readonly Encoding DEFAULT_CHARSET = Encoding.GetEncoding("ISO-8859-1");

        /// <summary>
        /// Creates a new instance of the <see cref="ByteArrayHttpMessageConverter"/> 
        /// with 'text/plain; charset=ISO-8859-1', and '*/*' media types.
        /// </summary>
        public StringHttpMessageConverter() :
            base(new MediaType("text", "plain", "ISO-8859-1"), MediaType.ALL)
        {
        }

        /// <summary>
        /// Indicates whether the given class is supported by this converter.
        /// </summary>
        /// <param name="type">The type to test for support.</param>
        /// <returns><see langword="true"/> if supported; otherwise <see langword="false"/></returns>
        protected override bool Supports(Type type)
        {
            return type.Equals(typeof(string));
        }

        /// <summary>
        /// Abstract template method that reads the actualy object. Invoked from <see cref="M:Read"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="response">The HTTP response to read from.</param>
        /// <returns>The converted object.</returns>
        protected override T ReadInternal<T>(HttpWebResponse response)
        {
            // Get the response encoding
            Encoding encoding;
            MediaType mediaType = MediaType.ParseMediaType(response.ContentType);
            if (mediaType == null || !StringUtils.HasText(mediaType.CharSet))
            {
                encoding = DEFAULT_CHARSET;
            }
            else
            {
                encoding = Encoding.GetEncoding(mediaType.CharSet);
            }

            // Get the response stream  
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
            {
                return reader.ReadToEnd() as T;
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

            // Create a byte array of the data we want to send  
            byte[] byteData = encoding.GetBytes(content as string);

            // Set the content length in the request headers  
            request.ContentLength = byteData.Length;

            // Write to the request
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }
        }
    }
}
