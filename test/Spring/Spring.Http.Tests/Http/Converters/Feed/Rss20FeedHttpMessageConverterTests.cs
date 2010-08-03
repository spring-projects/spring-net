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

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Globalization;
using System.ServiceModel.Syndication;

using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Http.Converters.Feed
{
    /// <summary>
    /// Unit tests for the Rss20FeedHttpMessageConverter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class Rss20FeedHttpMessageConverterTests
    {
        private Rss20FeedHttpMessageConverter converter;
        private MockRepository mocks;

	    [SetUp]
	    public void SetUp() 
        {
            mocks = new MockRepository();
            converter = new Rss20FeedHttpMessageConverter();
	    }

        [Test]
        public void CanRead() 
        {
            Assert.IsTrue(converter.CanRead(typeof(SyndicationFeed), new MediaType("application", "rss+xml")));
            Assert.IsTrue(converter.CanRead(typeof(SyndicationItem), new MediaType("application", "rss+xml")));
            Assert.IsTrue(converter.CanRead(typeof(SyndicationFeed), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanRead(typeof(SyndicationFeed), new MediaType("text", "xml")));
            Assert.IsFalse(converter.CanRead(typeof(string), new MediaType("application", "rss+xml")));
            Assert.IsFalse(converter.CanRead(typeof(SyndicationFeed), new MediaType("text", "plain")));
        }

        [Test]
        public void CanWrite() 
        {
            Assert.IsTrue(converter.CanRead(typeof(SyndicationFeed), new MediaType("application", "rss+xml")));
            Assert.IsTrue(converter.CanRead(typeof(SyndicationItem), new MediaType("application", "rss+xml")));
            Assert.IsTrue(converter.CanRead(typeof(SyndicationFeed), new MediaType("application", "xml")));
            Assert.IsTrue(converter.CanRead(typeof(SyndicationFeed), new MediaType("text", "xml")));
            Assert.IsFalse(converter.CanRead(typeof(string), new MediaType("application", "rss+xml")));
            Assert.IsFalse(converter.CanRead(typeof(SyndicationFeed), new MediaType("text", "plain")));
        }

        [Test]
        public void Read()
        {
            DateTime now = DateTime.Now;

            string body = String.Format("﻿<rss xmlns:a10=\"http://www.w3.org/2005/Atom\" version=\"2.0\"><channel><title>Test Feed</title><link>http://www.springframework.net/Feed</link><description>This is a test feed</description><copyright>Copyright 2010</copyright><managingEditor>bruno.baia@springframework.net</managingEditor><lastBuildDate>{0}</lastBuildDate><a10:id>Atom10FeedHttpMessageConverterTests.Write</a10:id></channel></rss>",
                now.ToString("ddd, dd MMM yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture).Remove(29, 1));

            HttpWebResponse webResponse = mocks.CreateMock<HttpWebResponse>();
            Expect.Call<Stream>(webResponse.GetResponseStream()).Return(new MemoryStream(Encoding.UTF8.GetBytes(body)));

            mocks.ReplayAll();

            SyndicationFeed result = converter.Read<SyndicationFeed>(webResponse);
            Assert.IsNotNull(result, "Invalid result");
            Assert.AreEqual("Atom10FeedHttpMessageConverterTests.Write", result.Id, "Invalid result");
            Assert.AreEqual("Test Feed", result.Title.Text, "Invalid result");
            Assert.AreEqual("This is a test feed", result.Description.Text, "Invalid result");
            Assert.IsTrue(result.Links.Count == 1, "Invalid result");
            Assert.AreEqual(new Uri("http://www.springframework.net/Feed"), result.Links[0].Uri, "Invalid result");
            Assert.AreEqual("Copyright 2010", result.Copyright.Text, "Invalid result");
            Assert.IsTrue(result.Authors.Count == 1, "Invalid result");
            Assert.AreEqual("bruno.baia@springframework.net", result.Authors[0].Email, "Invalid result");

            mocks.VerifyAll();
        }

        [Test]
        public void Write()
        {
            MemoryStream requestStream = new MemoryStream();

            DateTime now = DateTime.Now;

            string expectedBody = String.Format("﻿<rss xmlns:a10=\"http://www.w3.org/2005/Atom\" version=\"2.0\"><channel><title>Test Feed</title><link>http://www.springframework.net/Feed</link><description>This is a test feed</description><copyright>Copyright 2010</copyright><managingEditor>bruno.baia@springframework.net</managingEditor><lastBuildDate>{0}</lastBuildDate><a10:id>Atom10FeedHttpMessageConverterTests.Write</a10:id></channel></rss>", 
                now.ToString("ddd, dd MMM yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture).Remove(29, 1));
            
            SyndicationFeed body = new SyndicationFeed("Test Feed", "This is a test feed", new Uri("http://www.springframework.net/Feed"), "Atom10FeedHttpMessageConverterTests.Write", now);
            SyndicationPerson sp = new SyndicationPerson("bruno.baia@springframework.net", "Bruno Baïa", "http://www.springframework.net/bbaia");
            body.Authors.Add(sp);
            body.Copyright = new TextSyndicationContent("Copyright 2010");

            HttpWebRequest webRequest = mocks.CreateMock<HttpWebRequest>();
            Expect.Call(webRequest.ContentType = "application/rss+xml").PropertyBehavior();
            Expect.Call(webRequest.ContentLength = 1337).PropertyBehavior();
            Expect.Call<Stream>(webRequest.GetRequestStream()).Return(requestStream);

            mocks.ReplayAll();

            converter.Write(body, null, webRequest);

            byte[] result = requestStream.ToArray();
            Assert.AreEqual(expectedBody, Encoding.UTF8.GetString(result), "Invalid result");

            mocks.VerifyAll();
        }
    }
}
#endif
