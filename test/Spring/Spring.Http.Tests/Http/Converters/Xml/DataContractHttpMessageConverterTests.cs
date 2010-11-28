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

using System.IO;
using System.Net;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;

using NUnit.Framework;
using Rhino.Mocks;

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
            Assert.IsTrue(converter.CanRead(typeof(DataContractClass), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanRead(typeof(DataContractClass), new MediaType("text", "xml")));
            Assert.IsTrue(converter.CanRead(typeof(DataContractClass), new MediaType("application", "soap+xml"))); // application/*+xml
            Assert.IsFalse(converter.CanRead(typeof(DataContractClass), new MediaType("text", "plain")));
            Assert.IsFalse(converter.CanRead(typeof(NonDataContractClass), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanRead(typeof(CollectionDataContractClass), new MediaType("application", "xml")));
        }

        [Test]
        public void CanWrite() 
        {
            Assert.IsTrue(converter.CanWrite(typeof(DataContractClass), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanWrite(typeof(DataContractClass), new MediaType("text", "xml")));
            Assert.IsTrue(converter.CanWrite(typeof(DataContractClass), new MediaType("application", "soap+xml"))); // application/*+xml
            Assert.IsFalse(converter.CanWrite(typeof(DataContractClass), new MediaType("text", "plain")));
            Assert.IsFalse(converter.CanWrite(typeof(NonDataContractClass), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanWrite(typeof(CollectionDataContractClass), new MediaType("application", "xml")));
        }

        [Test]
        public void Read()
        {
            string body = @"<?xml version='1.0' encoding='UTF-8' ?>
                <DataContractHttpMessageConverterTests.DataContractClass xmlns='http://schemas.datacontract.org/2004/07/Spring.Http.Converters.Xml' xmlns:i='http://www.w3.org/2001/XMLSchema-instance'>
                    <ID>1</ID><Name>Bruno Baïa</Name>
                </DataContractHttpMessageConverterTests.DataContractClass>";

            HttpWebResponse webResponse = mocks.CreateMock<HttpWebResponse>();
            Expect.Call<Stream>(webResponse.GetResponseStream()).Return(new MemoryStream(Encoding.UTF8.GetBytes(body)));

            mocks.ReplayAll();

            DataContractClass result = converter.Read<DataContractClass>(webResponse);
            Assert.IsNotNull(result, "Invalid result");
            Assert.AreEqual("1", result.ID, "Invalid result");
            Assert.AreEqual("Bruno Baïa", result.Name, "Invalid result");

            mocks.VerifyAll();
        }

        [Test]
        public void Write()
        {
            MemoryStream requestStream = new MemoryStream();

            string expectedBody = "<DataContractHttpMessageConverterTests.DataContractClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.datacontract.org/2004/07/Spring.Http.Converters.Xml\"><ID>1</ID><Name>Bruno Baïa</Name></DataContractHttpMessageConverterTests.DataContractClass>";
            DataContractClass body = new DataContractClass("1", "Bruno Baïa");

            HttpWebRequest webRequest = mocks.CreateMock<HttpWebRequest>();
            Expect.Call(webRequest.ContentType = "application/xml").PropertyBehavior();
            Expect.Call(webRequest.ContentLength = 1337).PropertyBehavior();
            Expect.Call<Stream>(webRequest.GetRequestStream()).Return(requestStream);

            mocks.ReplayAll();

            converter.Write(body, null, webRequest);

            byte[] result = requestStream.ToArray();
            Assert.AreEqual(expectedBody, Encoding.UTF8.GetString(result), "Invalid result");

            mocks.VerifyAll();
        }

        #region Test classes
        
        [DataContract]
        public class DataContractClass
        {
            [DataMember]
            public string ID { get; set; }

            [DataMember]
            public string Name { get; set; }

            public DataContractClass(string id, string name)
            {
                this.ID = id;
                this.Name = name;
            }
        }

        [CollectionDataContract]
        public class CollectionDataContractClass : List<string>
        {
            public CollectionDataContractClass()
                : base()
            {
            }
        }

        public class NonDataContractClass
        {
        }

        #endregion
    }
}
#endif
