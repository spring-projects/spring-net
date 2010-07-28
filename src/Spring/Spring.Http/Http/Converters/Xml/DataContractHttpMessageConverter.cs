#if NET_3_0
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
using System.Xml;
using System.Text;
using System.Runtime.Serialization;

using Spring.Util;

namespace Spring.Http.Converters.Xml
{
    // TODO : Derive from AbstractXmlHttpMessageConverter ?
    // TODO : Support for known types, etc...
    public class DataContractHttpMessageConverter : AbstractHttpMessageConverter
    {
        public static readonly Encoding DEFAULT_CHARSET = Encoding.UTF8;

        public DataContractHttpMessageConverter() :
            base(new MediaType("application", "xml"), new MediaType("text", "xml"), new MediaType("application", "*+xml"))
        {
        }

        protected override bool Supports(Type type)
        {
            return true;
            //return (
            //    AttributeUtils.FindAttribute(type, typeof(DataContractAttribute)) != null ||
            //    AttributeUtils.FindAttribute(type, typeof(SerializableAttribute)) != null ||
            //    typeof(ISerializable).IsAssignableFrom(type));
        }

        protected override T ReadInternal<T>(HttpWebResponse response)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
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

            DataContractSerializer serializer = new DataContractSerializer(content.GetType());
            
            // Write data  
            using (Stream postStream = request.GetRequestStream())
            {
                using (XmlTextWriter xmlWriter = new XmlTextWriter(postStream, encoding))
                {
                    serializer.WriteObject(xmlWriter, content);
                    xmlWriter.Flush();
                }
                
                // Set the content length in the request headers  
                request.ContentLength = postStream.Length;
            }
        }
    }
}
#endif