#region License

/*
 * Copyright 2002-2011 the original author or authors.
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
using System.Text;
using System.Xml;

using NUnit.Framework;

namespace Spring.Http.Converters.Xml
{
    /// <summary>
    /// Unit tests for the XmlDocumentHttpMessageConverter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class XmlDocumentHttpMessageConverterTests
    {
        private XmlDocumentHttpMessageConverter converter;

	    [SetUp]
	    public void SetUp() 
        {
            converter = new XmlDocumentHttpMessageConverter();
	    }

        [Test]
        public void CanRead() 
        {
            Assert.IsTrue(converter.CanRead(typeof(XmlDocument), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanRead(typeof(XmlDocument), new MediaType("text", "xml")));
            Assert.IsTrue(converter.CanRead(typeof(XmlDocument), new MediaType("application", "soap+xml"))); // application/*+xml
            Assert.IsFalse(converter.CanRead(typeof(XmlDocument), new MediaType("text", "plain")));
            Assert.IsFalse(converter.CanRead(typeof(String), new MediaType("application", "xml")));
        }

        [Test]
        public void CanWrite() 
        {
            Assert.IsTrue(converter.CanWrite(typeof(XmlDocument), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanWrite(typeof(XmlDocument), new MediaType("text", "xml")));
            Assert.IsTrue(converter.CanWrite(typeof(XmlDocument), new MediaType("application", "soap+xml"))); // application/*+xml
            Assert.IsFalse(converter.CanWrite(typeof(XmlDocument), new MediaType("text", "plain")));
            Assert.IsFalse(converter.CanWrite(typeof(String), new MediaType("application", "xml")));
        }

        [Test]
        public void Read()
        {
            string body = "<TestElement testAttribute='value' />";

            MockHttpInputMessage message = new MockHttpInputMessage(body, Encoding.UTF8);

            XmlDocument result = converter.Read<XmlDocument>(message);
            Assert.IsNotNull(result, "Invalid result");
            XmlNode xmlNodeResult = result.SelectSingleNode("//TestElement");
            Assert.IsNotNull(xmlNodeResult, "Invalid result");
            Assert.AreEqual("TestElement", xmlNodeResult.LocalName, "Invalid result");
            Assert.IsNotNull(xmlNodeResult.Attributes["testAttribute"], "Invalid result");
            Assert.AreEqual("value", xmlNodeResult.Attributes["testAttribute"].Value, "Invalid result");
        }

        [Test]
        public void Write()
        {
            XmlDocument body = new XmlDocument();
            body.LoadXml("<TestElement testAttribute='value' />");

            MockHttpOutputMessage message = new MockHttpOutputMessage();

            converter.Write(body, null, message);

            Assert.AreEqual(body.OuterXml, message.GetBodyAsString(Encoding.UTF8), "Invalid result");
            Assert.AreEqual(new MediaType("application", "xml"), message.Headers.ContentType, "Invalid content-type");
            //Assert.IsTrue(message.Headers.ContentLength > -1, "Invalid content-length");
        }
    }
}
