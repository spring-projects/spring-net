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
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;

using NUnit.Framework;

namespace Spring.Http.Rest
{
    /// <summary>
    /// Integration tests for the RestTemplate class.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    [TestFixture]
    public class RestTemplateIntegrationTests
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(RestTemplateIntegrationTests));

        #endregion

        private WebServiceHost webServiceHost;
        private string uri = "http://localhost:1337";
        private RestTemplate template;
        private MediaType contentType;

        [SetUp]
        public void SetUp()
        {
            template = new RestTemplate(uri);
            contentType = new MediaType("text", "plain");

            webServiceHost = new WebServiceHost(typeof(TestService), new Uri(uri));
            webServiceHost.Open();
        }

        [TearDown]
        public void TearDownClass()
        {
            webServiceHost.Close();
        }

        [Test]
        public void GetString()
        {
            string result = template.GetForObject<string>("users");
            Assert.AreEqual("2", result, "Invalid content");
        }

        [Test]
        public void GetStringVarArgsTemplateVariables()
        {
            string result = template.GetForObject<string>("user/{id}", "1");
            Assert.AreEqual("Bruno Baïa", result, "Invalid content");
        }

        [Test]
        public void GetStringDictionaryTemplateVariables()
        {
            IDictionary<string, string> uriVariables = new Dictionary<string, string>(1);
            uriVariables.Add("id", "2");
            string result = template.GetForObject<string>("user/{id}", uriVariables);
            Assert.AreEqual("Marie Baia", result, "Invalid content");
        }

        [Test]
        [ExpectedException(typeof(RestClientException),
            ExpectedMessage = "The server returned 'User with id '5' not found' with the status code 404 - NotFound.")]
        public void GetStringError()
        {
            string result = template.GetForObject<string>("user/{id}", "5");
        }

        [Test]
        public void GetStringForMessage()
        {
            HttpResponseMessage<string> result = template.GetForMessage<string>("user/{id}", "1");
            Assert.AreEqual("Bruno Baïa", result.Body, "Invalid content");
            Assert.AreEqual(contentType, MediaType.ParseMediaType(result.Headers[HttpResponseHeader.ContentType]), "Invalid content-type");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Invalid status code");
            Assert.AreEqual("OK", result.StatusDescription, "Invalid status description");
        }

        [Test]
        [ExpectedException(typeof(RestClientException),
            ExpectedMessage = "Could not extract response: no Content-Type found")]
        public void GetStringNoResponse()
        {
            string result = template.GetForObject<string>("/nothing");
        }

        [Test]
        public void HeadForHeaders()
        {
            WebHeaderCollection result = template.HeadForHeaders("head");
            Assert.AreEqual(new MediaType("text", "plain"), MediaType.ParseMediaType(result[HttpResponseHeader.ContentType]), "Invalid content-type");
        }

        [Test]
        public void PostStringForLocation()
        {
            Uri result = template.PostForLocation("user", "Lisa Baia");
            Assert.AreEqual(new Uri(new Uri(uri), "/user/3"), result, "Invalid location");
        }

        [Test]
        public void PostStringForMessage()
        {
            HttpResponseMessage<string> result = template.PostForMessage<string>("user", "Lisa Baia");
            Assert.AreEqual(new Uri(new Uri(uri), "/user/3"), new Uri(result.Headers[HttpResponseHeader.Location]), "Invalid location");
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode, "Invalid status code");
            Assert.AreEqual("User id '3' created with 'Lisa Baia'", result.StatusDescription, "Invalid status description");
            Assert.AreEqual("3", result.Body, "Invalid content");
        }

        [Test]
        public void PostStringForObject()
        {
            string result = template.PostForObject<string>("user", "Lisa Baia");
            Assert.AreEqual("3", result, "Invalid content");
        }

        [Test]
        [ExpectedException(typeof(RestClientException),
            ExpectedMessage = "The server returned 'Content cannot be null or empty' with the status code 400 - BadRequest.")]
        public void PostStringForObjectWithError()
        {
            string result = template.PostForObject<string>("user", "");
        }

        [Test]
        public void Put()
        {
            string result = template.GetForObject<string>("user/1");
            Assert.AreEqual("Bruno Baïa", result, "Invalid content");

            template.Put("user/1", "Bruno Baia");

            result = template.GetForObject<string>("user/1");
            Assert.AreEqual("Bruno Baia", result, "Invalid content");
        }

        [Test]
        [ExpectedException(typeof(RestClientException),
            ExpectedMessage = "The server returned 'User id '4' does not exist' with the status code 400 - BadRequest.")]
        public void PutWithError()
        {
            template.Put("user/4", "Dinora Baia");
        }

        [Test]
        public void Delete()
        {
            string result = template.GetForObject<string>("users");
            Assert.AreEqual("2", result, "Invalid content");

            template.Delete("user/2");

            result = template.GetForObject<string>("users");
            Assert.AreEqual("1", result, "Invalid content");
        }

        [Test]
        [ExpectedException(typeof(RestClientException),
            ExpectedMessage = "The server returned 'User id '10' does not exist' with the status code 400 - BadRequest.")]
        public void DeleteWithError()
        {
            template.Delete("user/10");
        }

        [Test]
        public void OptionsForAllow()
        {
            IList<HttpMethod> result = template.OptionsForAllow("allow");
            Assert.AreEqual(3, result.Count, "Invalid response");
            Assert.IsTrue(result.Contains(HttpMethod.GET), "Invalid response");
            Assert.IsTrue(result.Contains(HttpMethod.HEAD), "Invalid response");
            Assert.IsTrue(result.Contains(HttpMethod.PUT), "Invalid response");
        }

        [Test]
        public void ExchangePost()
        {
            HttpResponseMessage<string> result = template.Exchange<string>(
                "user", new HttpRequestMessage("Maryse Baia", HttpMethod.POST));

            Assert.AreEqual("3", result.Body, "Invalid content");
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode, "Invalid status code");
            Assert.AreEqual("User id '3' created with 'Maryse Baia'", result.StatusDescription, "Invalid status description");
        }

        [Test]
        public void ExchangePut()
        {
            HttpResponseMessage result = template.Exchange(
                "user/1", new HttpRequestMessage("Bruno Baia", HttpMethod.PUT));

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Invalid status code");
            Assert.AreEqual("User id '1' updated with 'Bruno Baia'", result.StatusDescription, "Invalid status description");
        }

        [Test]
        [ExpectedException(ExpectedMessage = "The server returned 'Not Found' with the status code 404 - NotFound.")]
        public void ClientError()
        {
            template.Execute<object>("clienterror", null, null);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "The server returned 'Internal Server Error' with the status code 500 - InternalServerError.")]
        public void ServerError()
        {
            template.Execute<object>("servererror", null, null);
        }

        #region REST test service

        [ServiceContract]
        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
        public class TestService
        {
            private IDictionary<string, string> users;

            public TestService()
            {
                users = new Dictionary<string, string>();
                users.Add("1", "Bruno Baïa");
                users.Add("2", "Marie Baia");
            }

            [WebGet(UriTemplate = "clienterror")]
            public void ClientError()
            {
                WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound();
            }

            [WebGet(UriTemplate = "servererror")]
            public void ServerError()
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
            }

            [WebInvoke(UriTemplate = "allow", Method = "OPTIONS")]
            public void Allow()
            {
                WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.Allow] = "GET, HEAD, PUT";
            }

            [WebInvoke(UriTemplate = "head", Method = "HEAD")]
            public void Head()
            {
                WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.ContentType] = "text/plain";
            }

            [WebGet(UriTemplate = "user/{id}")]
            public Message GetUser(string id)
            {
                WebOperationContext context = WebOperationContext.Current;

                if (!users.ContainsKey(id))
                {
                    context.OutgoingResponse.SetStatusAsNotFound(String.Format("User with id '{0}' not found", id));
                    return context.CreateTextResponse(null);
                }

                return context.CreateTextResponse(users[id]);
            }

            [WebGet(UriTemplate = "users")]
            public Message GetUsersCount()
            {
                WebOperationContext context = WebOperationContext.Current;

                return context.CreateTextResponse(users.Count.ToString());
            }

            [WebGet(UriTemplate = "nothing")]
            public void GetNothing()
            {
                WebOperationContext.Current.OutgoingResponse.SuppressEntityBody = true;
            }

            [WebInvoke(UriTemplate = "user", Method = "POST")]
            public Message Post(Stream stream)
            {
                WebOperationContext context = WebOperationContext.Current;

                UriTemplateMatch match = context.IncomingRequest.UriTemplateMatch;
                UriTemplate template = new UriTemplate("/user/{id}");

                MediaType mediaType = MediaType.ParseMediaType(context.IncomingRequest.ContentType);

                string id = (users.Count + 1).ToString(); // generate new ID
                string name;
                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(mediaType.CharSet)))
                {
                    name = reader.ReadToEnd();
                }

                if (String.IsNullOrEmpty(name))
                {
                    context.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                    context.OutgoingResponse.StatusDescription = "Content cannot be null or empty";
                    return WebOperationContext.Current.CreateTextResponse("");
                }

                users.Add(id, name);

                Uri uri = template.BindByPosition(match.BaseUri, id);
                context.OutgoingResponse.SetStatusAsCreated(uri);
                context.OutgoingResponse.StatusDescription = String.Format("User id '{0}' created with '{1}'", id, name);

                return WebOperationContext.Current.CreateTextResponse(id);
            }

            [WebInvoke(UriTemplate = "user/{id}", Method = "PUT")]
            public void Update(string id, Stream stream)
            {
                WebOperationContext context = WebOperationContext.Current;

                if (!users.ContainsKey(id))
                {
                    context.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                    context.OutgoingResponse.StatusDescription = String.Format("User id '{0}' does not exist", id);
                    return;
                }

                MediaType mediaType = MediaType.ParseMediaType(context.IncomingRequest.ContentType);

                string name;
                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(mediaType.CharSet)))
                {
                    name = reader.ReadToEnd();
                }
                users[id] = name;

                context.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                context.OutgoingResponse.StatusDescription = String.Format("User id '{0}' updated with '{1}'", id, name);
            }

            [WebInvoke(UriTemplate = "user/{id}", Method = "DELETE")]
            public void Delete(string id)
            {
                WebOperationContext context = WebOperationContext.Current;

                if (!users.ContainsKey(id))
                {
                    context.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                    context.OutgoingResponse.StatusDescription = String.Format("User id '{0}' does not exist", id);
                    return;
                }

                users.Remove(id);

                context.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                context.OutgoingResponse.StatusDescription = String.Format("User id '{0}' have been removed", id);
            }
        }

        #endregion
    }
}
#endif