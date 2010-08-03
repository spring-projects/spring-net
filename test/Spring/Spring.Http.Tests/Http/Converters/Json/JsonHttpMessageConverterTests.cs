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

using System.IO;
using System.Net;
using System.Text;

using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Http.Converters.Json
{
    /// <summary>
    /// Unit tests for the JsonHttpMessageConverter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class JsonHttpMessageConverterTests
    {
        private JsonHttpMessageConverter converter;
        private MockRepository mocks;

	    [SetUp]
	    public void SetUp() 
        {
            mocks = new MockRepository();
            converter = new JsonHttpMessageConverter();
	    }

        [Test]
        public void CanRead() 
        {
            Assert.IsTrue(converter.CanRead(typeof(CustomClass), new MediaType("application", "json")));
            Assert.IsFalse(converter.CanRead(typeof(CustomClass), new MediaType("text", "xml")));
        }

        [Test]
        public void CanWrite() 
        {
            Assert.IsTrue(converter.CanRead(typeof(CustomClass), new MediaType("application", "json")));
            Assert.IsFalse(converter.CanRead(typeof(CustomClass), new MediaType("text", "xml")));
        }

        [Test]
        public void Read()
        {
            string body = "{\"ID\":\"1\",\"Name\":\"Bruno Baïa\"}";

            HttpWebResponse webResponse = mocks.CreateMock<HttpWebResponse>();
            Expect.Call<Stream>(webResponse.GetResponseStream()).Return(new MemoryStream(Encoding.UTF8.GetBytes(body)));

            mocks.ReplayAll();

            CustomClass result = converter.Read<CustomClass>(webResponse);
            Assert.IsNotNull(result, "Invalid result");
            Assert.AreEqual("1", result.ID, "Invalid result");
            Assert.AreEqual("Bruno Baïa", result.Name, "Invalid result");

            mocks.VerifyAll();
        }

        [Test]
        public void Write()
        {
            MemoryStream requestStream = new MemoryStream();

            string expectedBody = "{\"ID\":\"1\",\"Name\":\"Bruno Baïa\"}";
            CustomClass body = new CustomClass("1", "Bruno Baïa");

            HttpWebRequest webRequest = mocks.CreateMock<HttpWebRequest>();
            Expect.Call(webRequest.ContentType = "application/json").PropertyBehavior();
            Expect.Call(webRequest.ContentLength = 1337).PropertyBehavior();
            Expect.Call<Stream>(webRequest.GetRequestStream()).Return(requestStream);

            mocks.ReplayAll();

            converter.Write(body, null, webRequest);

            byte[] result = requestStream.ToArray();
            Assert.AreEqual(expectedBody, Encoding.UTF8.GetString(result), "Invalid result");

            mocks.VerifyAll();
        }

        #region Test classes
        
        public class CustomClass
        {
            public string ID { get; set; }

            public string Name { get; set; }

            public CustomClass()
            {
            }

            public CustomClass(string id, string name)
            {
                this.ID = id;
                this.Name = name;
            }
        }

        #endregion
    }
}
#endif
