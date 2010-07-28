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
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace Spring.Http.Converters.Xml
{
    // TODO : Support XElement.Load options
    public class XElementHttpMessageConverter : AbstractXmlHttpMessageConverter
    {
        public XElementHttpMessageConverter() :
            base(new MediaType("application", "xml"), new MediaType("text", "xml"), new MediaType("application", "*+xml"))
        {
        }

        protected override bool Supports(Type type)
        {
            return type.Equals(typeof(XElement));
        }

        protected override T ReadXml<T>(XmlReader xmlReader, HttpWebResponse response)
        {
            return XElement.Load(xmlReader) as T;
        }

        protected override void WriteXml(XmlWriter xmlWriter, object content, HttpWebRequest request)
        {
            XElement xElement = content as XElement;
            xElement.WriteTo(xmlWriter);
        }
    }
}
#endif