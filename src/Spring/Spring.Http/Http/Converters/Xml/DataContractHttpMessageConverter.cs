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

namespace Spring.Http.Converters.Xml
{
    // TODO : Support for known types, etc...

    /// <summary>
    /// Implementation of <see cref="IHttpMessageConverter"/> that can read and write XML 
    /// using <see cref="DataContractSerializer"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By default, this converter supports 'text/xml', 'application/xml', and 'application/*-xml' media types. 
    /// This can be overridden by setting the <see cref="P:SupportedMediaTypes"/> property.
    /// </para>
    /// <para>
    /// This converter can read classes annotated with <see cref="DataContractAttribute"/> and <see cref="CollectionDataContractAttribute"/>, and write classes 
    /// annotated with with {@link XmlRootElement}, or subclasses thereof.
    /// </para>
    /// </remarks>
    /// <author>Bruno Baia</author>
    public class DataContractHttpMessageConverter : AbstractXmlHttpMessageConverter
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DataContractHttpMessageConverter"/> 
        /// with 'text/xml', 'application/xml', and 'application/*-xml' media types.
        /// </summary>
        public DataContractHttpMessageConverter() :
            base()
        {
        }

        /// <summary>
        /// Indicates whether the given class is supported by this converter.
        /// </summary>
        /// <param name="type">The type to test for support.</param>
        /// <returns><see langword="true"/> if supported; otherwise <see langword="false"/></returns>
        protected override bool Supports(Type type)
        {
            return (
                Attribute.GetCustomAttributes(type, typeof(DataContractAttribute), true).Length > 0 ||
                Attribute.GetCustomAttributes(type, typeof(CollectionDataContractAttribute), true).Length > 0
                );
        }

        /// <summary>
        /// Abstract template method that reads the actualy object using a <see cref="XmlReader"/>. Invoked from <see cref="M:ReadInternal"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="xmlReader">The XmlReader to use.</param>
        /// <param name="response">The HTTP response to read from.</param>
        /// <returns>The converted object.</returns>
        protected override T ReadXml<T>(XmlReader xmlReader, HttpWebResponse response)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            return serializer.ReadObject(xmlReader) as T;
        }

        /// <summary>
        /// Abstract template method that writes the actual body using a <see cref="XmlWriter"/>. Invoked from <see cref="M:WriteInternal"/>.
        /// </summary>
        /// <param name="xmlWriter">The XmlWriter to use.</param>
        /// <param name="content">The object to write to the HTTP request.</param>
        /// <param name="request">The HTTP request to write to.</param>
        protected override void WriteXml(XmlWriter xmlWriter, object content, HttpWebRequest request)
        {
            DataContractSerializer serializer = new DataContractSerializer(content.GetType());
            serializer.WriteObject(xmlWriter, content);
        }
    }
}
#endif