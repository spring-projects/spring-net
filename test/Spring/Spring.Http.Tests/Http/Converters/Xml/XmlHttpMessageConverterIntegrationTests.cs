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
using System.Xml.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Runtime.Serialization;

using Spring.Http.Rest;

using NUnit.Framework;

namespace Spring.Http.Converters.Xml
{
    /// <summary>
    /// Integration tests for the Xml based IHttpMessageConverter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class XmlHttpMessageConverterIntegrationTests
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(XmlHttpMessageConverterIntegrationTests));

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
        public void TearDown()
        {
            webServiceHost.Close();
        }

        [Test]
        public void DataContractGetForObject()
        {
            template.MessageConverters.Add(new DataContractHttpMessageConverter());

            User result = template.GetForObject<User>("user/dc/{id}", "1");
            Assert.IsNotNull(result, "Invalid content");
            Assert.AreEqual("1", result.ID, "Invalid content");
            Assert.AreEqual("Bruno Baïa", result.Name, "Invalid content");
        }

        [Test]
        public void DataContractPostForMessage()
        {
            template.MessageConverters.Add(new DataContractHttpMessageConverter());

            User user = new User() { Name = "Lisa Baia" };

            HttpResponseMessage result = template.PostForMessage("user/dc", user);
            Assert.IsNull(result.Body, "Invalid content");
            Assert.AreEqual(new Uri(new Uri(uri), "/user/dc/3"), result.Headers.Location, "Invalid location");
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode, "Invalid status code");
            Assert.AreEqual("User id '3' created with 'Lisa Baia'", result.StatusDescription, "Invalid status description");
        }

        [Test]
        public void XElementGetForObject()
        {
            template.MessageConverters.Add(new XElementHttpMessageConverter());

            XElement result = template.GetForObject<XElement>("user/xml/{id}", "1");
            Assert.IsNotNull(result, "Invalid content");
            Assert.AreEqual("1", result.Element("ID").Value, "Invalid content");
            Assert.AreEqual("Bruno Baïa", result.Element("Name").Value, "Invalid content");
        }

        [Test]
        public void XElementPostForMessage()
        {
            template.MessageConverters.Add(new XElementHttpMessageConverter());

            XElement user = new XElement("User", 
                new XElement("Name", "Lisa Baia"));

            HttpResponseMessage result = template.PostForMessage("user/xml", user);
            Assert.IsNull(result.Body, "Invalid content");
            Assert.AreEqual(new Uri(new Uri(uri), "/user/xml/3"), result.Headers.Location, "Invalid location");
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

            [WebGet(UriTemplate = "user/dc/{id}")]
            public User GetUserDataContract(string id)
            {
                WebOperationContext context = WebOperationContext.Current;

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

            [WebInvoke(UriTemplate = "user/dc", Method = "POST")]
            public void CreateDataContract(User user)
            {
                WebOperationContext context = WebOperationContext.Current;

                UriTemplateMatch match = context.IncomingRequest.UriTemplateMatch;
                UriTemplate template = new UriTemplate("/user/dc/{id}");

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

            [WebGet(UriTemplate = "user/xml/{id}")]
            public XElement GetUserXElement(string id)
            {
                WebOperationContext context = WebOperationContext.Current;

                foreach (User user in this.users)
                {
                    if (user.ID.Equals(id, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new XElement("User",
                            new XElement("ID", user.ID),
                            new XElement("Name", user.Name));
                    }
                }

                context.OutgoingResponse.SetStatusAsNotFound(String.Format("User with id '{0}' not found", id));
                return null;
            }

            [WebInvoke(UriTemplate = "user/xml", Method = "POST")]
            public void CreateXElement(XElement user)
            {
                WebOperationContext context = WebOperationContext.Current;

                UriTemplateMatch match = context.IncomingRequest.UriTemplateMatch;
                UriTemplate template = new UriTemplate("/user/xml/{id}");

                MediaType mediaType = MediaType.Parse(context.IncomingRequest.ContentType);

                if (user.Element("ID") != null && !String.IsNullOrEmpty(user.Element("ID").Value))
                {
                    context.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                    context.OutgoingResponse.StatusDescription = String.Format("User id '{0}' already exists", user.Element("ID"));
                    return;
                }

                User newUser = new User();
                newUser.ID = (users.Count + 1).ToString(); // generate new ID
                newUser.Name = user.Element("Name").Value;

                users.Add(newUser);

                Uri uri = template.BindByPosition(match.BaseUri, newUser.ID);
                context.OutgoingResponse.SetStatusAsCreated(uri);
                context.OutgoingResponse.StatusDescription = String.Format("User id '{0}' created with '{1}'", newUser.ID, newUser.Name);
            }
        }

        #endregion
    }
}
#endif