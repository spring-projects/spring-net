#if NET_4_0
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

using System;
using System.Net;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Runtime.Serialization;

using Spring.Http.Rest;

using NUnit.Framework;

namespace Spring.Http.Converters.Json
{
    /// <summary>
    /// Integration tests for the JsonHttpMessageConverter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class JsonHttpMessageConverterIntegrationTests
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(JsonHttpMessageConverterIntegrationTests));

        #endregion

        private WebServiceHost webServiceHost;
        private string uri = "http://localhost:1337";
        private RestTemplate template;
        private MediaType contentType;

        [SetUp]
        public void SetUp()
        {
            template = new RestTemplate(uri);
            template.MessageConverters = new List<IHttpMessageConverter>();
            //template.MessageConverters.Add(new StringHttpMessageConverter()); // for debugging purpose

            contentType = new MediaType("application", "json");

            webServiceHost = new WebServiceHost(typeof(TestService), new Uri(uri));
            webServiceHost.Open();
        }

        [TearDown]
        public void TearDownClass()
        {
            webServiceHost.Close();
        }

        [Test]
        public void GetForObject()
        {
            template.MessageConverters.Add(new JsonHttpMessageConverter());

            User result = template.GetForObject<User>("user/{id}", "1");
            Assert.IsNotNull(result, "Invalid content");
            Assert.AreEqual("1", result.ID, "Invalid content");
            Assert.AreEqual("Bruno Baïa", result.Name, "Invalid content");
        }

        [Test]
        public void PostForMessage()
        {
            template.MessageConverters.Add(new JsonHttpMessageConverter());

            User user = new User() { Name = "Lisa Baia" };

            HttpResponseMessage result = template.PostForMessage("user", user);
            Assert.IsNull(result.Body, "Invalid content");
            Assert.AreEqual(new Uri(new Uri(uri), "/user/3"), result.Headers.Location, "Invalid location");
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode, "Invalid status code");
            Assert.AreEqual("User id '3' created with 'Lisa Baia'", result.StatusDescription, "Invalid status description");
        }

        #region REST test service

        [DataContract]
        public class User
        {
            [DataMember]
            public string ID { get; set; }

            [DataMember]
            public string Name { get; set; }
        }

        [ServiceContract]
        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
        public class TestService
        {
            private IList<User> users;

            public TestService()
            {
                users = new List<User>();
                users.Add(new User() { ID = "1", Name = "Bruno Baïa" });
                users.Add(new User() { ID = "2", Name = "Marie Baia" });
            }

            [WebGet(UriTemplate = "user/{id}")]
            public User GetUser(string id)
            {
                WebOperationContext context = WebOperationContext.Current;
                context.OutgoingResponse.Format = WebMessageFormat.Json;

                foreach (User user in this.users)
                {
                    if (user.ID.Equals(id, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return user;
                    }
                }

                context.OutgoingResponse.SetStatusAsNotFound(String.Format("User with id '{0}' not found", id));
                return null;
            }

            [WebInvoke(UriTemplate = "user", Method = "POST")]
            public void Create(User user)
            {
                WebOperationContext context = WebOperationContext.Current;
                context.OutgoingResponse.Format = WebMessageFormat.Json;

                UriTemplateMatch match = context.IncomingRequest.UriTemplateMatch;
                UriTemplate template = new UriTemplate("/user/{id}");

                MediaType mediaType = MediaType.Parse(context.IncomingRequest.ContentType);

                if (!String.IsNullOrEmpty(user.ID))
                {
                    context.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                    context.OutgoingResponse.StatusDescription = String.Format("User id '{0}' already exists", user.ID);
                    return;
                }

                user.ID = (users.Count + 1).ToString(); // generate new ID

                users.Add(user);

                Uri uri = template.BindByPosition(match.BaseUri, user.ID);
                context.OutgoingResponse.SetStatusAsCreated(uri);
                context.OutgoingResponse.StatusDescription = String.Format("User id '{0}' created with '{1}'", user.ID, user.Name);
            }
        }

        #endregion
    }
}
#endif