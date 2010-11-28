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
using System.Web;
using System.Text;
using System.Collections.Specialized;

using Spring.Util;

namespace Spring.Http.Converters
{
    /// <summary>
    /// Implementation of <see cref="IHttpMessageConverter"/> that can handle form data, 
    /// including multipart form data (i.e. file uploads).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This converter supports the 'application/x-www-form-urlencoded' media type.
    /// </para>
    /// <para>
    /// For example, the following snippet shows how to submit an HTML form:
    /// <code>
    /// RestTemplate template = new RestTemplate(); // UrlEncodedFormHttpMessageConverter is configured by default
    /// NameValueCollection form = new NameValueCollection();
    /// form.Add("field 1", "value 1");
    /// form.Add("field 2", "value 2");
    /// form.Add("field 2", "value 3");
    /// template.PostForLocation("http://example.com/myForm", form);
    /// </code>
    /// </para>
    /// </remarks>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public class UrlEncodedFormHttpMessageConverter : AbstractHttpMessageConverter
    {
        private Encoding charset = Encoding.GetEncoding("ISO-8859-1");

        /// <summary>
        /// Sets the encoding used for writing form data.
        /// </summary>
        public Encoding Charset
        {
            set { charset = value; }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="UrlEncodedFormHttpMessageConverter"/> 
        /// with 'application/x-www-form-urlencoded' media type.
        /// </summary>
        public UrlEncodedFormHttpMessageConverter() :
            base(MediaType.APPLICATION_FORM_URLENCODED)
        {
        }

        /// <summary>
        /// Indicates whether the given class is supported by this converter.
        /// </summary>
        /// <param name="type">The type to test for support.</param>
        /// <returns><see langword="true"/> if supported; otherwise <see langword="false"/></returns>
        protected override bool Supports(Type type)
        {
            return type.Equals(typeof(NameValueCollection));
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
                encoding = this.charset;
            }
            else
            {
                encoding = Encoding.GetEncoding(mediaType.CharSet);
            }

            // Get the response stream  
            string body;
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
            {
                body = reader.ReadToEnd();
            }

            string[] pairs = body.Split('&');
            NameValueCollection result = new NameValueCollection(pairs.Length);
            foreach (string pair in pairs)
            {
                int idx = pair.IndexOf('=');
                if (idx == -1)
                {
                    result.Add(HttpUtility.UrlDecode(pair, this.charset), null);
                }
                else
                {
                    string name = HttpUtility.UrlDecode(pair.Substring(0, idx), this.charset);
                    string value = HttpUtility.UrlDecode(pair.Substring(idx + 1), this.charset);
                    result.Add(name, value);
                }
            }
            return result as T;
        }

        /// <summary>
        /// Abstract template method that writes the actual body. Invoked from <see cref="M:Write"/>.
        /// </summary>
        /// <param name="content">The object to write to the HTTP request.</param>
        /// <param name="request">The HTTP request to write to.</param>
        protected override void WriteInternal(object content, HttpWebRequest request)
        {
            StringBuilder builder = new StringBuilder();
            NameValueCollection form = content as NameValueCollection;
            for(int i=0; i < form.AllKeys.Length; i++)
            {
                string name = form.GetKey(i);
                string[] values = form.GetValues(name);
                if (values == null)
                {
                    builder.Append(HttpUtility.UrlEncode(name, this.charset));
                }
                else
                {
                    for (int j = 0; j < values.Length; j++)
                    {
                        string value = values[j];
                        builder.Append(HttpUtility.UrlEncode(name, this.charset));
                        builder.Append('=');
                        builder.Append(HttpUtility.UrlEncode(value, this.charset));
                        if (j != (values.Length - 1))
                        {
                            builder.Append('&');
                        }
                    }
                }
                if (i != (form.AllKeys.Length - 1))
                {
                    builder.Append('&');
                }
            }

            // Create a byte array of the data we want to send  
            byte[] byteData = this.charset.GetBytes(builder.ToString());

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
