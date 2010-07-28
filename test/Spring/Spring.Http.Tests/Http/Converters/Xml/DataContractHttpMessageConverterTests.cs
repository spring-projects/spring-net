#if NET_3_0
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
using System.Xml;
using System.Runtime.Serialization;

namespace Spring.Http.Converters.Xml
{
    /// <summary>
    /// Unit tests for the DataContractHttpMessageConverter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class DataContractHttpMessageConverterTests
    {
        private DataContractHttpMessageConverter converter;
        private MockRepository mocks;

	    [SetUp]
	    public void SetUp() 
        {
            mocks = new MockRepository();
            converter = new DataContractHttpMessageConverter();
	    }

        [Test]
        public void CanRead() 
        {
            Assert.IsTrue(converter.CanRead(typeof(CustomClass), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanRead(typeof(CustomClass), new MediaType("text", "xml")));
            Assert.IsTrue(converter.CanRead(typeof(CustomClass), new MediaType("application", "soap+xml"))); // application/*+xml
        }

        [Test]
        public void CanWrite() 
        {
            Assert.IsTrue(converter.CanWrite(typeof(CustomClass), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanWrite(typeof(CustomClass), new MediaType("text", "xml")));
            Assert.IsTrue(converter.CanRead(typeof(CustomClass), new MediaType("application", "soap+xml"))); // application/*+xml
        }

        //[Test]
        //public void Read()
        //{
        //    string body = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?><TestElement testAttribute=\"value\" />";

        //    HttpWebResponse webResponse = mocks.CreateMock<HttpWebResponse>();
        //    Expect.Call<Stream>(webResponse.GetResponseStream()).Return(new MemoryStream(Encoding.UTF8.GetBytes(body))).Repeat.Once();

        //    mocks.ReplayAll();

        //    XmlDocument result = converter.Read<XmlDocument>(webResponse);
        //    Assert.IsNotNull(result, "Invalid result");

        //    mocks.VerifyAll();
        //}

        //[Test]
        //public void Write()
        //{
        //    XmlDocument body = new XmlDocument();
        //    body.CreateElement("TestElement");

        //    HttpWebRequest webRequest = WebRequest.Create("http://localhost") as HttpWebRequest;
        //    webRequest.Method = "POST";

        //    converter.Write(body, null, webRequest);

        //    Assert.AreEqual(new MediaType("application", "xml"), MediaType.ParseMediaType(webRequest.ContentType), "Invalid content-type");

        //    using (Stream postStream = webRequest.GetRequestStream())
        //    {
        //        using (StreamReader reader = new StreamReader(postStream))
        //        {
        //            string result = reader.ReadToEnd();
        //            Assert.AreEqual(result.Length, webRequest.ContentLength, "Invalid content-length");
        //        }
                
        //    }
        //}

        #region Test classes

        [DataContract]
        public class CustomClass
        {
            [DataMember]
            public string ID { get; set; }
        }

        #endregion
    }
}
#endif
