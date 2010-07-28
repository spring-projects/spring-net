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
     * Implementation of {@link HttpMessageConverter} that can read and write byte arrays.
     *
     * <p>By default, this converter supports all media types (<code>&#42;&#47;&#42;</code>), and writes with a {@code
     * Content-Type} of {@code application/octet-stream}. This can be overridden by setting the {@link
     * #setSupportedMediaTypes(java.util.List) supportedMediaTypes} property.
     *
     * @author Arjen Poutsma
     * @since 3.0
     */
    public class ByteArrayHttpMessageConverter : AbstractHttpMessageConverter
    {
        /** Creates a new instance of the {@code ByteArrayHttpMessageConverter}. */
	    public ByteArrayHttpMessageConverter() :
            base(new MediaType("application", "octet-stream"), MediaType.ALL)
        {
	    }

        protected override bool Supports(Type type)
        {
            return type.Equals(typeof(byte[]));
        }

	    protected override T ReadInternal<T>(HttpWebResponse response)
        {
            // Get the response stream  
            using (BinaryReader reader = new BinaryReader(response.GetResponseStream()))
            {
                return reader.ReadBytes((int)response.ContentLength) as T;
            }
	    }

        protected override void WriteInternal(object content, HttpWebRequest request)
        {
            // Create a byte array of the data we want to send  
            byte[] byteData = content as byte[];

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
