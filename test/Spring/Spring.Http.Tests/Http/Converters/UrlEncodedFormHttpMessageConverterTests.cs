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
using System.Collections.Specialized;

using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Http.Converters
{
    /// <summary>
    /// Unit tests for the UrlEncodedFormHttpMessageConverter class.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    [TestFixture]
    public class UrlEncodedFormHttpMessageConverterTests
    {
        private UrlEncodedFormHttpMessageConverter converter;
        private MockRepository mocks;

	    [SetUp]
	    public void SetUp() 
        {
            mocks = new MockRepository();
            converter = new UrlEncodedFormHttpMessageConverter();
	    }

        [Test]
        public void CanRead() 
        {
            Assert.IsTrue(converter.CanRead(typeof(NameValueCollection), new MediaType("application", "x-www-form-urlencoded")));
            Assert.IsFalse(converter.CanRead(typeof(NameValueCollection), new MediaType("application", "xml")));
            Assert.IsFalse(converter.CanRead(typeof(string), new MediaType("application", "x-www-form-urlencoded")));
        }

        [Test]
        public void CanWrite() 
        {
            Assert.IsTrue(converter.CanWrite(typeof(NameValueCollection), new MediaType("application", "x-www-form-urlencoded")));
            Assert.IsFalse(converter.CanWrite(typeof(NameValueCollection), new MediaType("application", "xml")));
            Assert.IsFalse(converter.CanWrite(typeof(string), new MediaType("application", "x-www-form-urlencoded")));
        }

        [Test]
        public void Read()
        {
            String body = "name+1=value+1&name+2=value+2%2B1&name+2=value+2%2B2&name+3";
            string charSet = "ISO-8859-1";
            Encoding charSetEncoding = Encoding.GetEncoding(charSet);
            MediaType mediaType = new MediaType("application", "x-www-form-urlencoded", charSet);

            HttpWebResponse webResponse = mocks.CreateMock<HttpWebResponse>();
            Expect.Call<Stream>(webResponse.GetResponseStream()).Return(new MemoryStream(Encoding.UTF8.GetBytes(body)));
            Expect.Call<string>(webResponse.ContentType).Return(mediaType.ToString());

            mocks.ReplayAll();

            NameValueCollection result = converter.Read<NameValueCollection>(webResponse);
            Assert.AreEqual(3, result.Count, "Invalid result");
            Assert.AreEqual(1, result.GetValues(0).Length, "Invalid result");
            Assert.AreEqual("value 1", result.GetValues(0)[0], "Invalid result");
            Assert.AreEqual(2, result.GetValues("name 2").Length, "Invalid result");
            Assert.AreEqual("value 2+1", result.GetValues("name 2")[0], "Invalid result");
            Assert.AreEqual("value 2+2", result.GetValues("name 2")[1], "Invalid result");
            Assert.IsNull(result["name 3"], "Invalid result");

            mocks.VerifyAll();
        }

        [Test]
        public void Write()
        {
            MemoryStream requestStream = new MemoryStream();

            string expectedBody = "name+1=value+1&name+2=value+2%2b1&name+2=value+2%2b2&name+3";
            NameValueCollection body = new NameValueCollection();
            body.Add("name 1", "value 1");
            body.Add("name 2", "value 2+1");
            body.Add("name 2", "value 2+2");
            body.Add("name 3", null);
            string charSet = "ISO-8859-1";
            Encoding charSetEncoding = Encoding.GetEncoding(charSet);
            MediaType mediaType = new MediaType("application", "x-www-form-urlencoded", charSet);

            HttpWebRequest webRequest = mocks.CreateMock<HttpWebRequest>();
            Expect.Call(webRequest.ContentType = mediaType.ToString()).PropertyBehavior();
            Expect.Call(webRequest.ContentLength = 1337).PropertyBehavior();
            Expect.Call<Stream>(webRequest.GetRequestStream()).Return(requestStream);

            mocks.ReplayAll();

            converter.Write(body, null, webRequest);

            byte[] result = requestStream.ToArray();
            Assert.AreEqual(expectedBody, charSetEncoding.GetString(result), "Invalid result");

            mocks.VerifyAll();
        }
    }
}
