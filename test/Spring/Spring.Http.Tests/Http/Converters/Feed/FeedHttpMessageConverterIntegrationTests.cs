#if NET_4_0
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
using System.Net;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Syndication;

using Spring.Http.Rest;

using NUnit.Framework;

namespace Spring.Http.Converters.Feed
{
    /// <summary>
    /// Integration tests for the SyndicationFeed based IHttpMessageConverter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class FeedHttpMessageConverterIntegrationTests
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(FeedHttpMessageConverterIntegrationTests));

        #endregion

        private WebServiceHost webServiceHost;
        private string uri = "http://localhost:1337";
        private RestTemplate template;

        [SetUp]
        public void SetUp()
        {
            template = new RestTemplate(uri);
            template.MessageConverters = new List<IHttpMessageConverter>();
            //template.MessageConverters.Add(new StringHttpMessageConverter()); // for debugging purpose

            webServiceHost = new WebServiceHost(typeof(TestService), new Uri(uri));
            webServiceHost.Open();
        }

        [TearDown]
        public void TearDownClass()
        {
            webServiceHost.Close();
        }

        [Test]
        public void Rss20GetForObject()
        {
            template.MessageConverters.Add(new Rss20FeedHttpMessageConverter());

            SyndicationFeed result = template.GetForObject<SyndicationFeed>("feed");
            Assert.IsNotNull(result, "Invalid content");
            Assert.AreEqual("Test Feed", result.Title.Text, "Invalid content");
            Assert.AreEqual("This is a test feed", result.Description.Text, "Invalid content");
        }

        [Test]
        public void Rss20PostForMessage()
        {
            template.MessageConverters.Add(new Rss20FeedHttpMessageConverter());

            SyndicationItem item = new SyndicationItem("Bruno's item", "Bruno's content", null);

            HttpResponseMessage result = template.PostForMessage("feed/entry", item);
            Assert.IsNull(result.Body, "Invalid content");
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode, "Invalid status code");
            Assert.AreEqual("Syndication item added with title 'Bruno's item'", result.StatusDescription, "Invalid status description");
        }

        [Test]
        public void Atom10GetForObject()
        {
            template.MessageConverters.Add(new Atom10FeedHttpMessageConverter());

            SyndicationFeed result = template.GetForObject<SyndicationFeed>("feed/?format=atom");
            Assert.IsNotNull(result, "Invalid content");
            Assert.AreEqual("Test Feed", result.Title.Text, "Invalid content");
            Assert.AreEqual("This is a test feed", result.Description.Text, "Invalid content");
        }

        [Test]
        public void Atom10PostForMessage()
        {
            template.MessageConverters.Add(new Atom10FeedHttpMessageConverter());

            SyndicationItem item = new SyndicationItem("Bruno's item", "Bruno's content", null);

            HttpResponseMessage result = template.PostForMessage("feed/entry", item);
            Assert.IsNull(result.Body, "Invalid content");
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode, "Invalid status code");
            Assert.AreEqual("Syndication item added with title 'Bruno's item'", result.StatusDescription, "Invalid status description");
        }

        #region REST test service

        [ServiceContract]
        [ServiceKnownType(typeof(Atom10FeedFormatter))]
        [ServiceKnownType(typeof(Rss20FeedFormatter))]
        [ServiceKnownType(typeof(Atom10ItemFormatter))]
        [ServiceKnownType(typeof(Rss20ItemFormatter))]
        public class TestService
        {
            [WebGet(UriTemplate = "feed/", BodyStyle = WebMessageBodyStyle.Bare)]
            public SyndicationFeedFormatter CreateFeed()
            {
                // Create a new Syndication Feed.
                SyndicationFeed feed = new SyndicationFeed("Test Feed", "This is a test feed", null);
                List<SyndicationItem> items = new List<SyndicationItem>();

                // Create a new Syndication Item.
                SyndicationItem item = new SyndicationItem("An item", "Item content", null);
                items.Add(item);
                feed.Items = items;

                // Return ATOM or RSS based on uri
                // rss -> http://localhost:1337/feed/
                // atom -> http://localhost:1337/feed/?format=atom
                string query = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["format"];
                SyndicationFeedFormatter formatter = null;
                if (query == "atom")
                {
                    formatter = new Atom10FeedFormatter(feed);
                }
                else
                {
                    formatter = new Rss20FeedFormatter(feed);
                }

                return formatter;
            }

            [WebInvoke(UriTemplate = "feed/entry")]
            public void AddEntry(SyndicationItemFormatter item)
            {
                WebOperationContext context = WebOperationContext.Current;

                // Add entry
                // ..

                context.OutgoingResponse.StatusCode = HttpStatusCode.Created;
                context.OutgoingResponse.StatusDescription = String.Format("Syndication item added with title '{0}'", item.Item.Title.Text);
            }
        }

        #endregion
    }
}
#endif