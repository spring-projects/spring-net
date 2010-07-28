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

namespace Spring.Http.Converters
{
    /**
     * Implementation of {@link HttpMessageConverter} that can read and write strings.
     *
     * <p>By default, this converter supports all media types (<code>&#42;&#47;&#42;</code>), and writes with a {@code
     * Content-Type} of {@code text/plain}. This can be overridden by setting the {@link
     * #setSupportedMediaTypes(java.util.List) supportedMediaTypes} property.
     *
     * @author Arjen Poutsma
     * @since 3.0
     */
    public class StringHttpMessageConverter : AbstractHttpMessageConverter
    {
        public static readonly Encoding DEFAULT_CHARSET = Encoding.GetEncoding("ISO-8859-1");

        public StringHttpMessageConverter() :
            base(new MediaType("text", "plain", "ISO-8859-1"), MediaType.ALL)
        {
        }

        protected override bool Supports(Type type)
        {
            return type.Equals(typeof(string));
        }

        protected override T ReadInternal<T>(HttpWebResponse response)
        {
            // Get the response encoding
            Encoding encoding;
            if (String.IsNullOrEmpty(response.CharacterSet))
            {
                encoding = DEFAULT_CHARSET;
            }
            else
            {
                encoding = Encoding.GetEncoding(response.CharacterSet);
            }

            // Get the response stream  
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
            {
                return reader.ReadToEnd() as T;
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

            // Create a byte array of the data we want to send  
            byte[] byteData = encoding.GetBytes(content as string);

            // Set the content length in the request headers  
            request.ContentLength = byteData.Length;

            // Write data  
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }
        }
    }
}
