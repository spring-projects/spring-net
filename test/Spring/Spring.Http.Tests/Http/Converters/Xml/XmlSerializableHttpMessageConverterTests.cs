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

using NUnit.Framework;

namespace Spring.Http.Converters.Xml
{
    /// <summary>
    /// Unit tests for the XmlSerializableHttpMessageConverter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class XmlSerializableHttpMessageConverterTests
    {
        private XmlSerializableHttpMessageConverter converter;

	    [SetUp]
	    public void SetUp() 
        {
            converter = new XmlSerializableHttpMessageConverter();
	    }

        [Test]
        public void CanRead() 
        {
            Assert.IsTrue(converter.CanRead(typeof(CustomClass), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanRead(typeof(CustomClass), new MediaType("text", "xml")));
            Assert.IsTrue(converter.CanRead(typeof(CustomClass), new MediaType("application", "soap+xml"))); // application/*+xml
            Assert.IsFalse(converter.CanRead(typeof(CustomClass), new MediaType("text", "plain")));
        }

        [Test]
        public void CanWrite() 
        {
            Assert.IsTrue(converter.CanWrite(typeof(CustomClass), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanWrite(typeof(CustomClass), new MediaType("text", "xml")));
            Assert.IsTrue(converter.CanWrite(typeof(CustomClass), new MediaType("application", "soap+xml"))); // application/*+xml
            Assert.IsFalse(converter.CanWrite(typeof(CustomClass), new MediaType("text", "plain")));
        }

        [Test]
        public void Read()
        {
            string body = @"<CustomClass xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
                    <ID>1</ID>
                    <Name>Bruno Baïa</Name>
                </CustomClass>";

            MockHttpInputMessage message = new MockHttpInputMessage(body, Encoding.UTF8);

            CustomClass result = converter.Read<CustomClass>(message);
            Assert.IsNotNull(result, "Invalid result");
            Assert.AreEqual("1", result.ID, "Invalid result");
            Assert.AreEqual("Bruno Baïa", result.Name, "Invalid result");
        }

        [Test]
        public void Write()
        {
            string expectedBody = "<CustomClass xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ID>1</ID><Name>Bruno Baïa</Name></CustomClass>";
            CustomClass body = new CustomClass("1", "Bruno Baïa");

            MockHttpOutputMessage message = new MockHttpOutputMessage();

            converter.Write(body, null, message);

            Assert.AreEqual(expectedBody, message.GetBodyAsString(Encoding.UTF8), "Invalid result");
            Assert.AreEqual(new MediaType("application", "xml"), message.Headers.ContentType, "Invalid content-type");
            //Assert.IsTrue(message.Headers.ContentLength > -1, "Invalid content-length");
        }

        #region Test classes
        
        public class CustomClass
        {
            private string _id;
            private string _name;

            public string ID
            {
                get { return _id; }
                set { _id = value; }
            }

            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }

            public CustomClass()
            {
            }

            public CustomClass(string id, string name)
            {
                this._id = id;
                this._name = name;
            }
        }

        #endregion
    }
}
