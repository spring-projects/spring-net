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
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;

using NUnit.Framework;

namespace Spring.Http.Converters
{
    /// <summary>
    /// Unit tests for the FormHttpMessageConverter class.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    [TestFixture]
    public class FormHttpMessageConverterTests
    {
        private FormHttpMessageConverter converter;

	    [SetUp]
	    public void SetUp() 
        {
            converter = new FormHttpMessageConverter();
	    }

        [Test]
        public void CanRead() 
        {
            Assert.IsTrue(converter.CanRead(typeof(NameValueCollection), new MediaType("application", "x-www-form-urlencoded")));
            Assert.IsFalse(converter.CanRead(typeof(NameValueCollection), new MediaType("application", "xml")));
            Assert.IsFalse(converter.CanRead(typeof(string), new MediaType("application", "x-www-form-urlencoded")));
            Assert.IsFalse(converter.CanRead(typeof(IDictionary<string, object>), new MediaType("multipart","form-data")));
        }

        [Test]
        public void CanWrite() 
        {
            Assert.IsTrue(converter.CanWrite(typeof(NameValueCollection), new MediaType("application", "x-www-form-urlencoded")));
            Assert.IsFalse(converter.CanWrite(typeof(NameValueCollection), new MediaType("application", "xml")));
            Assert.IsFalse(converter.CanWrite(typeof(string), new MediaType("application", "x-www-form-urlencoded")));
            Assert.IsTrue(converter.CanWrite(typeof(IDictionary<string, object>), new MediaType("multipart", "form-data")));
            Assert.IsFalse(converter.CanWrite(typeof(IDictionary<string, object>), new MediaType("application", "xml")));
            Assert.IsFalse(converter.CanWrite(typeof(string), new MediaType("multipart", "form-data")));
        }

        [Test]
        public void ReadForm()
        {
            String body = "name+1=value+1&name+2=value+2%2B1&name+2=value+2%2B2&name+3";
            string charSet = "ISO-8859-1";
            Encoding charSetEncoding = Encoding.GetEncoding(charSet);
            MediaType mediaType = new MediaType("application", "x-www-form-urlencoded", charSet);

            MockHttpInputMessage message = new MockHttpInputMessage(body, charSetEncoding);
            message.Headers.ContentType = mediaType;

            NameValueCollection result = converter.Read<NameValueCollection>(message);
            Assert.AreEqual(3, result.Count, "Invalid result");
            Assert.AreEqual(1, result.GetValues(0).Length, "Invalid result");
            Assert.AreEqual("value 1", result.GetValues(0)[0], "Invalid result");
            Assert.AreEqual(2, result.GetValues("name 2").Length, "Invalid result");
            Assert.AreEqual("value 2+1", result.GetValues("name 2")[0], "Invalid result");
            Assert.AreEqual("value 2+2", result.GetValues("name 2")[1], "Invalid result");
            Assert.IsNull(result["name 3"], "Invalid result");
        }

        [Test]
        public void WriteForm()
        {
            string expectedBody = "name+1=value+1&name+2=value+2%2b1&name+2=value+2%2b2&name+3";
            NameValueCollection body = new NameValueCollection();
            body.Add("name 1", "value 1");
            body.Add("name 2", "value 2+1");
            body.Add("name 2", "value 2+2");
            body.Add("name 3", null);
            string charSet = "ISO-8859-1";
            Encoding charSetEncoding = Encoding.GetEncoding(charSet);

            MockHttpOutputMessage message = new MockHttpOutputMessage();

            converter.Write(body, MediaType.APPLICATION_FORM_URLENCODED, message);

            Assert.AreEqual(expectedBody, message.GetBodyAsString(charSetEncoding), "Invalid result");
            Assert.AreEqual(new MediaType("application", "x-www-form-urlencoded"), message.Headers.ContentType, "Invalid content-type");
            //Assert.AreEqual(charSetEncoding.GetBytes(expectedBody).Length, message.Headers.ContentLength, "Invalid content-length");
        }

        [Test]
        [Ignore] //TODO: relative path (needs IResource ?)
        public void WriteMultipart()
        {
		    IDictionary<string, object> parts = new Dictionary<string, object>();
            parts.Add("name 1", "value 1");
            parts.Add("name 2", "value 2+1");
            HttpEntity entity = new HttpEntity("<root><child/></root>");
            entity.Headers.ContentType = MediaType.TEXT_XML;
            parts.Add("xml", entity);
            parts.Add("logo", new FileInfo(@"C:\Users\Bruno\Pictures\Hero\downloadfile.jpeg"));

            MockHttpOutputMessage message = new MockHttpOutputMessage();

		    converter.Write(parts, MediaType.MULTIPART_FORM_DATA, message);

		    MediaType contentType = message.Headers.ContentType;
            Assert.IsNotNull(contentType, "Invalid content-type");
            Assert.AreEqual("multipart", contentType.Type, "Invalid content-type");
            Assert.AreEqual("form-data", contentType.Subtype, "Invalid content-type");
            string boundary = contentType.GetParameter("boundary");
            Assert.IsNotNull(boundary, "Invalid content-type");

            string result = message.GetBodyAsString(Encoding.UTF8);
            Assert.IsTrue(result.Contains("--" + boundary + "\r\nContent-Disposition: form-data; name=\"name 1\"\r\nContent-Type: text/plain;charset=ISO-8859-1\r\n\r\nvalue 1\r\n"), "Invalid content-disposition");
            Assert.IsTrue(result.Contains("--" + boundary + "\r\nContent-Disposition: form-data; name=\"name 2\"\r\nContent-Type: text/plain;charset=ISO-8859-1\r\n\r\nvalue 2+1\r\n"), "Invalid content-disposition");
            Assert.IsTrue(result.Contains("--" + boundary + "\r\nContent-Disposition: form-data; name=\"xml\"\r\nContent-Type: text/xml\r\n\r\n<root><child/></root>\r\n"), "Invalid content-disposition");
            Assert.IsTrue(result.Contains("--" + boundary + "\r\nContent-Disposition: form-data; name=\"logo\"; filename=\"C:\\Users\\Bruno\\Pictures\\Hero\\downloadfile.jpeg\"\r\nContent-Type: image/jpeg\r\n\r\n"), "Invalid content-disposition");
        }
    }
}
