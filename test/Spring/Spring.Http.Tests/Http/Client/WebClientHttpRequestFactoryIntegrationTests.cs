#if NET_3_5
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
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Security;
using System.IdentityModel.Tokens;
using System.IdentityModel.Selectors;

using NUnit.Framework;

namespace Spring.Http.Client
{
    /// <summary>
    /// Unit tests for the WebClientHttpRequestFactory class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class WebClientHttpRequestFactoryIntegrationTests : AbstractClientHttpRequestFactoryIntegrationTests
    {
        private WebClientHttpRequestFactory webRequestFactory;

        protected override IClientHttpRequestFactory CreateRequestFactory()
        {
            webRequestFactory = new WebClientHttpRequestFactory();
            return webRequestFactory;
        }

        protected override void ConfigureWebServiceHost(WebServiceHost webServiceHost)
        {
            WebHttpBinding httpBinding1 = new WebHttpBinding();
            httpBinding1.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
            httpBinding1.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            webServiceHost.AddServiceEndpoint(typeof(TestService), httpBinding1, "/basic");

            WebHttpBinding httpBinding2 = new WebHttpBinding();
            httpBinding2.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
            httpBinding2.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
            webServiceHost.AddServiceEndpoint(typeof(TestService), httpBinding2, "/ntlm");

            webServiceHost.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
            webServiceHost.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new CustomUserNamePasswordValidator();
        }

        [Test]
        public void Timeout()
        {
            this.webRequestFactory.Timeout = 1000;
            IClientHttpRequest request = this.CreateRequest("/sleep/2", HttpMethod.GET);

            try
            {
                request.Execute();
                Assert.Fail("Execute should failed !");
            }
            catch (Exception ex)
            {
                WebException webEx = ex as WebException;
                Assert.IsNotNull(webEx, "Invalid response exception");
                Assert.AreEqual(WebExceptionStatus.Timeout, webEx.Status, "Invalid response exception status");
            }
        }

        [Test]
        public void BasicAuthorizationKO()
        {
            IClientHttpRequest request = this.CreateRequest("/basic/echo", HttpMethod.PUT);
            string authInfo = "bruno:password";
            authInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;
            request.Headers.ContentLength = 0;

            using (IClientHttpResponse response = request.Execute())
            {
                Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, "Invalid status code");
            }
        }

        [Test]
        public void BasicAuthorizationOK()
        {
            IClientHttpRequest request = this.CreateRequest("/basic/echo", HttpMethod.PUT);
            string authInfo = "login:password";
            authInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;
            request.Headers.ContentLength = 0;

            using (IClientHttpResponse response = request.Execute())
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Invalid status code");
            }
        }

        [Test]
        public void NtlmAuthorizationKO()
        {
            IClientHttpRequest request = this.CreateRequest("/ntlm/echo", HttpMethod.PUT);
            request.Headers.ContentLength = 0;

            using (IClientHttpResponse response = request.Execute())
            {
                Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Invalid status code");
            }
        }

        [Test]
        public void NtlmAuthorizationOK()
        {
            this.webRequestFactory.UseDefaultCredentials = true;
            IClientHttpRequest request = this.CreateRequest("/ntlm/echo", HttpMethod.PUT);
            request.Headers.ContentLength = 0;

            using (IClientHttpResponse response = request.Execute())
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Invalid status code");
            }
        }

        [Test]
        public void SpecialHeaders() // related to HttpWebRequest implementation
        {
            IClientHttpRequest request = this.CreateRequest("/echo", HttpMethod.PUT);

            request.Headers.Accept = new MediaType[] { MediaType.ALL };
            request.Headers.ContentType = MediaType.TEXT_PLAIN;
            request.Headers["Connection"] = "close";
            request.Headers.ContentLength = 0;
#if NET_4_0
            request.Headers.Date = DateTime.Now;
#endif
            request.Headers["Expect"] = "bla";
            request.Headers.IfModifiedSince = DateTime.Now;
            request.Headers["Referer"] = "http://www.springframework.net/";
            //request.Headers["Transfer-Encoding"] = "Identity";
            request.Headers["User-Agent"] = "Unit tests";

            using (IClientHttpResponse response = request.Execute())
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Invalid status code");
            }
        }


        #region Test classes

        public class CustomUserNamePasswordValidator : UserNamePasswordValidator
        {
            public override void Validate(string userName, string password)
            {
                if (userName == "login" && password == "password")
                {
                    return;
                }
                throw new SecurityTokenException("Unknown username or incorrect password");
            }
        }

        #endregion
    }
}
#endif