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
            Assert.IsTrue(converter.CanRead(typeof(byte[]), MediaType.ALL));
            Assert.IsFalse(converter.CanRead(typeof(string), new MediaType("application", "octet-stream")));
        }

        [Test]
        public void CanWrite() 
        {
            Assert.IsTrue(converter.CanWrite(typeof(byte[]), new MediaType("application", "octet-stream")));
            Assert.IsTrue(converter.CanWrite(typeof(byte[]), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanWrite(typeof(byte[]), MediaType.ALL));
            Assert.IsFalse(converter.CanWrite(typeof(string), new MediaType("application", "octet-stream")));
        }

        [Test]
        public void Read() 
        {
            byte[] body = new byte[] { 0x1, 0x2 };

            IHttpInputMessage message = mocks.CreateMock<IHttpInputMessage>();
            Expect.Call<Stream>(message.Body).Return(new MemoryStream(body));
            HttpHeaders headers = new HttpHeaders();
            headers.ContentLength = body.Length;
            Expect.Call<HttpHeaders>(message.Headers).Return(headers).Repeat.Any();

            mocks.ReplayAll();
            
            byte[] result = converter.Read<byte[]>(message);
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

            IHttpOutputMessage message = mocks.CreateMock<IHttpOutputMessage>();
            Expect.Call(message.Body).PropertyBehavior();
            HttpHeaders headers = new HttpHeaders();
            Expect.Call<HttpHeaders>(message.Headers).Return(headers).Repeat.Any();

            mocks.ReplayAll();

            converter.Write(body, null, message);

            message.Body(requestStream);
            byte[] result = requestStream.ToArray();
            Assert.AreEqual(body, result, "Invalid result");
            Assert.AreEqual(new MediaType("application", "octet-stream"), message.Headers.ContentType, "Invalid content-type");
            //Assert.AreEqual(2, message.Headers.ContentLength, "Invalid content-length");

            mocks.VerifyAll();
        }
    }
}
