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
using System.Net;
using System.Text;

using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Http.Converters
{
    /// <summary>
    /// Unit tests for the StringHttpMessageConverter class.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    [TestFixture]
    public class StringHttpMessageConverterTests
    {
        private StringHttpMessageConverter converter;
        private MockRepository mocks;

	    [SetUp]
	    public void SetUp() 
        {
            mocks = new MockRepository();
            converter = new StringHttpMessageConverter();
	    }

        [Test]
        public void CanRead() 
        {
            Assert.IsTrue(converter.CanRead(typeof(string), new MediaType("text", "plain")));
            Assert.IsTrue(converter.CanRead(typeof(string), MediaType.ALL));
            Assert.IsTrue(converter.CanRead(typeof(string), new MediaType("application", "xml")));
            Assert.IsFalse(converter.CanRead(typeof(int[]), new MediaType("text", "plain")));
        }

        [Test]
        public void CanWrite() 
        {
            Assert.IsTrue(converter.CanWrite(typeof(string), new MediaType("text", "plain")));
            Assert.IsTrue(converter.CanWrite(typeof(string), MediaType.ALL));
            Assert.IsTrue(converter.CanWrite(typeof(string), new MediaType("application", "xml")));
            Assert.IsFalse(converter.CanWrite(typeof(int[]), new MediaType("text", "plain")));
        }

        [Test]
	    public void Read() 
        {
            string body = "Hello Bruno Baïa";
            string charSet = "utf-8";
            Encoding charSetEncoding = Encoding.GetEncoding(charSet);
            MediaType mediaType = new MediaType("text", "plain", charSet);

            IHttpInputMessage message = mocks.CreateMock<IHttpInputMessage>();
            Expect.Call<Stream>(message.Body).Return(new MemoryStream(Encoding.UTF8.GetBytes(body)));
            HttpHeaders headers = new HttpHeaders();
            headers.ContentType = mediaType;
            Expect.Call<HttpHeaders>(message.Headers).Return(headers).Repeat.Any();

            mocks.ReplayAll();
            
            string result = converter.Read<string>(message);
            Assert.AreEqual(body, result, "Invalid result");

            mocks.VerifyAll();
	    }

        [Test]
        public void WriteDefaultCharset()
        {
            MemoryStream requestStream = new MemoryStream();

            string body = "H\u00e9llo W\u00f6rld";
            string charSet = "ISO-8859-1";
            Encoding charSetEncoding = Encoding.GetEncoding(charSet);
            MediaType mediaType = new MediaType("text", "plain", charSet);

            IHttpOutputMessage message = mocks.CreateMock<IHttpOutputMessage>();
            Expect.Call(message.Body).PropertyBehavior();
            HttpHeaders headers = new HttpHeaders();
            Expect.Call<HttpHeaders>(message.Headers).Return(headers).Repeat.Any();

            mocks.ReplayAll();

            converter.Write(body, null, message);

            message.Body(requestStream);
            byte[] result = requestStream.ToArray();
            Assert.AreEqual(body, charSetEncoding.GetString(result), "Invalid result");
            Assert.AreEqual(mediaType, message.Headers.ContentType, "Invalid content-type");
            //Assert.AreEqual(charSetEncoding.GetBytes(body).Length, message.Headers.ContentLength, "Invalid content-length");

            mocks.VerifyAll();
        }

        [Test]
        public void WriteUTF8()
        {
            MemoryStream requestStream = new MemoryStream();

            string body = "H\u00e9llo W\u00f6rld";
            string charSet = "UTF-8";
            Encoding charSetEncoding = Encoding.GetEncoding(charSet);
            MediaType mediaType = new MediaType("text", "plain", charSet);

            IHttpOutputMessage message = mocks.CreateMock<IHttpOutputMessage>();
            Expect.Call(message.Body).PropertyBehavior();
            HttpHeaders headers = new HttpHeaders();
            Expect.Call<HttpHeaders>(message.Headers).Return(headers).Repeat.Any();

            mocks.ReplayAll();

            converter.Write(body, mediaType, message);

            message.Body(requestStream);
            byte[] result = requestStream.ToArray();
            Assert.AreEqual(body, charSetEncoding.GetString(result), "Invalid result");
            Assert.AreEqual(mediaType, message.Headers.ContentType, "Invalid content-type");
            //Assert.AreEqual(charSetEncoding.GetBytes(body).Length, message.Headers.ContentLength, "Invalid content-length");

            mocks.VerifyAll();
        }
    }
}
