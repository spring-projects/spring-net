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
        }

        [Test]
        public void CanWrite() 
        {
            Assert.IsTrue(converter.CanWrite(typeof(string), new MediaType("text", "plain")));
            Assert.IsTrue(converter.CanWrite(typeof(string), MediaType.ALL));
        }

        [Test]
	    public void Read() 
        {
            string body = "Hello Bruno Baïa";

            HttpWebResponse webResponse = mocks.CreateMock<HttpWebResponse>();
            Expect.Call<Stream>(webResponse.GetResponseStream()).Return(new MemoryStream(Encoding.UTF8.GetBytes(body))).Repeat.Once();
            Expect.Call<string>(webResponse.CharacterSet).Return("utf-8").Repeat.Twice();

            mocks.ReplayAll();
            
            string result = converter.Read<string>(webResponse);
            Assert.AreEqual(body, result, "Invalid result");

            mocks.VerifyAll();
	    }

        [Test]
        public void WriteDefaultCharset()
        {
            string body = "H\u00e9llo W\u00f6rld";

            string charSet = "ISO-8859-1";
            Encoding charSetEncoding = Encoding.GetEncoding(charSet);

            HttpWebRequest webRequest = WebRequest.Create("http://localhost") as HttpWebRequest;
            webRequest.Method = "POST";

            converter.Write(body, null, webRequest);

            using (Stream postStream = webRequest.GetRequestStream())
            {
                //Assert.AreEqual(body.Length, postStream.Length, "Invalid result");
            }

            Assert.AreEqual(new MediaType("text", "plain", charSet), MediaType.ParseMediaType(webRequest.ContentType), "Invalid content-type");
            Assert.AreEqual(charSetEncoding.GetBytes(body).Length, webRequest.ContentLength, "Invalid content-length");
            //Assert.IsFalse(String.IsNullOrEmpty(webRequest.Headers[HttpRequestHeader.AcceptCharset]), "Invalid accept-charset");
        }

        [Test]
        public void WriteUTF8()
        {
            string body = "H\u00e9llo W\u00f6rld";

            string charSet = "UTF-8";
            Encoding charSetEncoding = Encoding.GetEncoding(charSet);
            MediaType mediaType = new MediaType("text", "plain", charSet);

            HttpWebRequest webRequest = WebRequest.Create("http://localhost") as HttpWebRequest;
            webRequest.Method = "POST";

            converter.Write(body, mediaType, webRequest);

            using (Stream postStream = webRequest.GetRequestStream())
            {
                //Assert.AreEqual(body.Length, postStream.Length, "Invalid result");
            }

            Assert.AreEqual(mediaType, MediaType.ParseMediaType(webRequest.ContentType), "Invalid content-type");
            Assert.AreEqual(charSetEncoding.GetBytes(body).Length, webRequest.ContentLength, "Invalid content-length");
            //Assert.IsFalse(String.IsNullOrEmpty(webRequest.Headers[HttpRequestHeader.AcceptCharset]), "Invalid accept-charset");
        }
    }
}
