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
using System.Xml;

using NUnit.Framework;
using Rhino.Mocks;

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
        private MockRepository mocks;

	    [SetUp]
	    public void SetUp() 
        {
            mocks = new MockRepository();
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
            string body = "<?xml version='1.0' encoding='UTF-8' ?><TestElement testAttribute='value' />";

            HttpWebResponse webResponse = mocks.CreateMock<HttpWebResponse>();
            Expect.Call<Stream>(webResponse.GetResponseStream()).Return(new MemoryStream(Encoding.UTF8.GetBytes(body)));

            mocks.ReplayAll();

            XmlDocument result = converter.Read<XmlDocument>(webResponse);
            Assert.IsNotNull(result, "Invalid result");
            XmlNode xmlNodeResult = result.SelectSingleNode("//TestElement");
            Assert.IsNotNull(xmlNodeResult, "Invalid result");
            Assert.AreEqual("TestElement", xmlNodeResult.LocalName, "Invalid result");
            Assert.IsNotNull(xmlNodeResult.Attributes["testAttribute"], "Invalid result");
            Assert.AreEqual("value", xmlNodeResult.Attributes["testAttribute"].Value, "Invalid result");

            mocks.VerifyAll();
        }

        [Test]
        public void Write()
        {
            MemoryStream requestStream = new MemoryStream();

            XmlDocument body = new XmlDocument();
            body.LoadXml("<?xml version='1.0' encoding='UTF-8' ?><TestElement testAttribute='value' />");

            HttpWebRequest webRequest = mocks.CreateMock<HttpWebRequest>();
            Expect.Call(webRequest.ContentType = "application/xml").PropertyBehavior();
            Expect.Call(webRequest.ContentLength = 1337).PropertyBehavior();
            Expect.Call<Stream>(webRequest.GetRequestStream()).Return(requestStream);

            mocks.ReplayAll();

            converter.Write(body, null, webRequest);            

            byte[] result = requestStream.ToArray();
            Assert.AreEqual(body.OuterXml, Encoding.UTF8.GetString(result), "Invalid result");

            mocks.VerifyAll();
        }
    }
}
