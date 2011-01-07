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

using NUnit.Framework;

namespace Spring.Http.Converters
{
    /// <summary>
    /// Unit tests for the FileInfoHttpMessageConverter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class FileInfoHttpMessageConverterTests
    {
        private FileInfoHttpMessageConverter converter;

	    [SetUp]
	    public void SetUp() 
        {
            converter = new FileInfoHttpMessageConverter();
	    }

        [Test]
        public void CanRead() 
        {
            Assert.IsFalse(converter.CanRead(typeof(FileInfo), MediaType.ALL));
        }

        [Test]
        public void CanWrite() 
        {
            Assert.IsTrue(converter.CanWrite(typeof(FileInfo), new MediaType("application", "octet-stream")));
            Assert.IsTrue(converter.CanWrite(typeof(FileInfo), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanWrite(typeof(FileInfo), MediaType.ALL));
            Assert.IsFalse(converter.CanWrite(typeof(string), new MediaType("application", "octet-stream")));
        }

        //[Test]
        //public void Write()
        //{
        //    FileInfo body = new FileInfo(@"C:\File.txt");

        //    MockHttpOutputMessage message = new MockHttpOutputMessage();

        //    converter.Write(body, null, message);

        //    Assert.AreEqual(body, message.GetBodyAsBytes(), "Invalid result");
        //    Assert.AreEqual(new MediaType("text", "plain"), message.Headers.ContentType, "Invalid content-type");
        //}

        [Test]
        public void WriteWithUnknownExtension()
        {
            FileInfo body = new FileInfo(@"C:\Dummy.unknown");

            MockHttpOutputMessage message = new MockHttpOutputMessage();

            converter.Write(body, null, message);

            //Assert.AreEqual(body, message.GetBodyAsBytes(), "Invalid result");
            Assert.AreEqual(new MediaType("application", "octet-stream"), message.Headers.ContentType, "Invalid content-type");
        }

        [Test]
        public void WriteWithKnownExtension()
        {
            FileInfo body = new FileInfo(@"C:\Dummy.txt");

            MockHttpOutputMessage message = new MockHttpOutputMessage();

            converter.Write(body, null, message);

            Assert.AreEqual(new MediaType("text", "plain"), message.Headers.ContentType, "Invalid content-type");
        }


        [Test]
        public void WriteWithCustomExtension()
        {
            FileInfo body = new FileInfo(@"C:\Dummy.myext");

            MockHttpOutputMessage message = new MockHttpOutputMessage();

            converter.MimeMapping.Add(".myext", "spring/custom");
            converter.Write(body, null, message);

            Assert.AreEqual(new MediaType("spring", "custom"), message.Headers.ContentType, "Invalid content-type");
        }
    }
}
