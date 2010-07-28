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
    public class JsonHttpMessageConverter : AbstractHttpMessageConverter
    {
        public static readonly Encoding DEFAULT_CHARSET = Encoding.UTF8;

        public JsonHttpMessageConverter() :
            base(new MediaType("application", "json"))
        {
        }

        protected override bool Supports(Type type)
        {
            return true;
        }

        protected override T ReadInternal<T>(HttpWebResponse response)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            using (Stream stream = response.GetResponseStream())
            {
                return (T)serializer.ReadObject(stream) as T;
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

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(content.GetType());
            
            // Write data  
            using (Stream postStream = request.GetRequestStream())
            {
                using (XmlDictionaryWriter jsonWriter = JsonReaderWriterFactory.CreateJsonWriter(postStream, encoding, false))
                {
                    serializer.WriteObject(jsonWriter, content);
                    jsonWriter.Flush();
                }
                postStream.Flush();
                
                // Set the content length in the request headers  
                request.ContentLength = postStream.Length;
            }
        }
    }
}
#endif