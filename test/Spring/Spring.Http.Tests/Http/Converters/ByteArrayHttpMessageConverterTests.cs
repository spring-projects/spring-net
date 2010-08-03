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

using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Http.Converters
{
    /// <summary>
    /// Unit tests for the ByteArrayHttpMessageConverter class.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    [TestFixture]
    public class ByteArrayHttpMessageConverterTests
    {
        private ByteArrayHttpMessageConverter converter;
        private MockRepository mocks;

	    [SetUp]
	    public void SetUp() 
        {
            mocks = new MockRepository();
		    converter = new ByteArrayHttpMessageConverter();
	    }

        [Test]
        public void CanRead() 
        {
            Assert.IsTrue(converter.CanRead(typeof(byte[]), new MediaType("application", "octet-stream")));
            Assert.IsTrue(converter.CanRead(typeof(byte[]), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanWrite(typeof(byte[]), MediaType.ALL));
            Assert.IsFalse(converter.CanRead(typeof(string), new MediaType("application", "octet-stream")));
        }

        [Test]
        public void CanWrite() 
        {
            Assert.IsTrue(converter.CanRead(typeof(byte[]), new MediaType("application", "octet-stream")));
            Assert.IsTrue(converter.CanRead(typeof(byte[]), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanWrite(typeof(byte[]), MediaType.ALL));
            Assert.IsFalse(converter.CanRead(typeof(string), new MediaType("application", "octet-stream")));
        }

        [Test]
        public void Read() 
        {
            byte[] body = new byte[] { 0x1, 0x2 };

            HttpWebResponse webResponse = mocks.CreateMock<HttpWebResponse>();
            Expect.Call<Stream>(webResponse.GetResponseStream()).Return(new MemoryStream(body));
            Expect.Call<long>(webResponse.ContentLength).Return(2);

            mocks.ReplayAll();
            
            byte[] result = converter.Read<byte[]>(webResponse);
            Assert.AreEqual(body.Length, result.Length, "Invalid result");
            Assert.AreEqual(body[0], result[0], "Invalid result");
            Assert.AreEqual(body[1], result[1], "Invalid result");

            mocks.VerifyAll();
        }

        [Test]
        public void Write()
        {
            MemoryStream requestStream = new MemoryStream();

            byte[] body = new byte[] { 0x1, 0x2 };

            HttpWebRequest webRequest = mocks.CreateMock<HttpWebRequest>();
            Expect.Call(webRequest.ContentType = "application/octet-stream").PropertyBehavior();
            Expect.Call(webRequest.ContentLength = 2).PropertyBehavior();
            Expect.Call<Stream>(webRequest.GetRequestStream()).Return(requestStream);

            mocks.ReplayAll();

            converter.Write(body, null, webRequest);

            byte[] result = requestStream.ToArray();
            Assert.AreEqual(body, result, "Invalid result");

            mocks.VerifyAll();
        }
    }
}
