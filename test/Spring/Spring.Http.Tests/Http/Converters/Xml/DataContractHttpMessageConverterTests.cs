#if NET_3_0
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

using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;

using NUnit.Framework;

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

	    [SetUp]
	    public void SetUp() 
        {
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

            MockHttpInputMessage message = new MockHttpInputMessage(body, Encoding.UTF8);

            DataContractClass result = converter.Read<DataContractClass>(message);
            Assert.IsNotNull(result, "Invalid result");
            Assert.AreEqual("1", result.ID, "Invalid result");
            Assert.AreEqual("Bruno Baïa", result.Name, "Invalid result");
        }

        [Test]
        public void Write()
        {
            string expectedBody = "<DataContractHttpMessageConverterTests.DataContractClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.datacontract.org/2004/07/Spring.Http.Converters.Xml\"><ID>1</ID><Name>Bruno Baïa</Name></DataContractHttpMessageConverterTests.DataContractClass>";
            DataContractClass body = new DataContractClass("1", "Bruno Baïa");

            MockHttpOutputMessage message = new MockHttpOutputMessage();

            converter.Write(body, null, message);

            Assert.AreEqual(expectedBody, message.GetBodyAsString(Encoding.UTF8), "Invalid result");
            Assert.AreEqual(new MediaType("application", "xml"), message.Headers.ContentType, "Invalid content-type");
            //Assert.IsTrue(message.Headers.ContentLength > -1, "Invalid content-length");
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
