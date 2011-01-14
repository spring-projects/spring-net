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
using System.IO;
using System.Net;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;

using NUnit.Framework;

namespace Spring.Http.Client
{
    /// <summary>
    /// Integration tests for IClientHttpRequestFactory implementations.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>    
    [TestFixture]
    public abstract class AbstractClientHttpRequestFactoryIntegrationTests
    {
        private IClientHttpRequestFactory requestFactory;

        private const string BASE_URL = "http://localhost:1337";
        private WebServiceHost webServiceHost;

        [SetUp]
        public void SetUp()
        {
            requestFactory = this.CreateRequestFactory();

            webServiceHost = new WebServiceHost(typeof(TestService), new Uri(BASE_URL));
            this.ConfigureWebServiceHost(webServiceHost);
            webServiceHost.Open();
        }

        [TearDown]
        public void TearDown()
        {
            webServiceHost.Close();
        }

        protected abstract IClientHttpRequestFactory CreateRequestFactory();

        protected virtual void ConfigureWebServiceHost(WebServiceHost webServiceHost)
        {
        }

        protected virtual IClientHttpRequest CreateRequest(string path, HttpMethod method)
        {
            Uri uri = new Uri(BASE_URL + path);
            IClientHttpRequest request = requestFactory.CreateRequest(uri, method);
            Assert.AreEqual(method, request.Method, "Invalid HTTP method");
            Assert.AreEqual(uri, request.Uri, "Invalid HTTP URI");

            return request;
        }

        #region Sync

        [Test]
        public void Status()
        {
            IClientHttpRequest request = this.CreateRequest("/status/notfound", HttpMethod.GET);

            using (IClientHttpResponse response = request.Execute())
            {
                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "Invalid status code");
                Assert.AreEqual("Status NotFound", response.StatusDescription, "Invalid status description");
            }
        }

        [Test]
        public void Echo()
        {
            IClientHttpRequest request = this.CreateRequest("/echo", HttpMethod.PUT);
            request.Headers.ContentType = new MediaType("text", "plain", "utf-8");
            String headerName = "MyHeader";
            String headerValue1 = "value1";
            request.Headers.Add(headerName, headerValue1);
            String headerValue2 = "value2";
            request.Headers.Add(headerName, headerValue2);

            byte[] body = Encoding.UTF8.GetBytes("Hello World");
            request.Body = delegate(Stream stream)
            {
                stream.Write(body, 0, body.Length);
            };

            using (IClientHttpResponse response = request.Execute())
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Invalid status code");
                Assert.AreEqual("value1,value2", response.Headers[headerName], "Header values not found");
                using (BinaryReader reader = new BinaryReader(response.Body))
                {
                    byte[] result = reader.ReadBytes((int)response.Headers.ContentLength);
                    Assert.AreEqual(body, result, "Invalid body");
                }
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException),
            ExpectedMessage = "Client HTTP request already executed or is currently executing.")]
        public void MultipleExecute()
        {
            IClientHttpRequest request = this.CreateRequest("/status/ok", HttpMethod.GET);

            request.Execute();
            request.Execute();
        }

        [Test]
	    public void HttpMethods() 
        {
		    AssertHttpMethod("get", HttpMethod.GET);
		    AssertHttpMethod("head", HttpMethod.HEAD);
            AssertHttpMethod("post", HttpMethod.POST);
            AssertHttpMethod("put", HttpMethod.PUT);
		    AssertHttpMethod("options", HttpMethod.OPTIONS);
		    AssertHttpMethod("delete", HttpMethod.DELETE);
        }

        private void AssertHttpMethod(String path, HttpMethod method)
        {
            IClientHttpRequest request = this.CreateRequest("/methods/" + path, method);
            request.Headers.ContentLength = 0; // TODO : post/put null

            using (IClientHttpResponse response = request.Execute())
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Invalid status code");
                Assert.AreEqual(path, response.StatusDescription, "Invalid status description");
            }
        }

        #endregion

        #region Async

        [Test]
        public void StatusAsync()
        {
            ManualResetEvent manualEvent = new ManualResetEvent(false);
            Exception exception = null;

            IClientHttpRequest request = this.CreateRequest("/status/notfound", HttpMethod.GET);

            request.ExecuteAsync(null, delegate(ExecuteCompletedEventArgs args)
            {
                try
                {
                    Assert.IsNull(args.Error, "Invalid response");
                    Assert.IsFalse(args.Cancelled, "Invalid response");
                    Assert.AreEqual(HttpStatusCode.NotFound, args.Response.StatusCode, "Invalid status code");
                    Assert.AreEqual("Status NotFound", args.Response.StatusDescription, "Invalid status description");
                }
                catch(Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    manualEvent.Set();
                }
            });

            manualEvent.WaitOne();
            if (exception != null)
            {
                throw exception;
            }
        }

        [Test]
        public void EchoAsync()
        {
            ManualResetEvent manualEvent = new ManualResetEvent(false);
            Exception exception = null;

            IClientHttpRequest request = this.CreateRequest("/echo", HttpMethod.PUT);
            request.Headers.ContentType = new MediaType("text", "plain", "utf-8");
            String headerName = "MyHeader";
            String headerValue1 = "value1";
            request.Headers.Add(headerName, headerValue1);
            String headerValue2 = "value2";
            request.Headers.Add(headerName, headerValue2);

            byte[] body = Encoding.UTF8.GetBytes("Hello World");
            request.Body = delegate(Stream stream)
            {
                stream.Write(body, 0, body.Length);
            };

            request.ExecuteAsync(null, delegate(ExecuteCompletedEventArgs args)
            {
                try
                {
                    Assert.IsNull(args.Error, "Invalid response");
                    Assert.IsFalse(args.Cancelled, "Invalid response");
                    Assert.AreEqual(HttpStatusCode.OK, args.Response.StatusCode, "Invalid status code");
                    Assert.AreEqual("value1,value2", args.Response.Headers[headerName], "Header values not found");
                    using (BinaryReader reader = new BinaryReader(args.Response.Body))
                    {
                        byte[] result = reader.ReadBytes((int)args.Response.Headers.ContentLength);
                        Assert.AreEqual(body, result, "Invalid body");
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    manualEvent.Set();
                }
            });

            manualEvent.WaitOne();
            if (exception != null)
            {
                throw exception;
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException),
            ExpectedMessage = "Client HTTP request already executed or is currently executing.")]
        public void MultipleExecuteAsync()
        {
            IClientHttpRequest request = this.CreateRequest("/status/ok", HttpMethod.GET);

            request.ExecuteAsync(null, null);
            request.ExecuteAsync(null, null);
        }

        [Test]
        public void HttpMethodsAsync()
        {
            AssertHttpMethodAsync("get", HttpMethod.GET);
            AssertHttpMethodAsync("head", HttpMethod.HEAD);
            AssertHttpMethodAsync("post", HttpMethod.POST);
            AssertHttpMethodAsync("put", HttpMethod.PUT);
            AssertHttpMethodAsync("options", HttpMethod.OPTIONS);
            AssertHttpMethodAsync("delete", HttpMethod.DELETE);
        }

        private void AssertHttpMethodAsync(String path, HttpMethod method)
        {
            ManualResetEvent manualEvent = new ManualResetEvent(false);
            Exception exception = null;

            IClientHttpRequest request = this.CreateRequest("/methods/" + path, method);
            request.Headers.ContentLength = 0; // TODO : post/put null

            request.ExecuteAsync(null, delegate(ExecuteCompletedEventArgs args)
            {
                try
                {
                    Assert.IsNull(args.Error, "Invalid response");
                    Assert.IsFalse(args.Cancelled, "Invalid response");
                    Assert.AreEqual(HttpStatusCode.OK, args.Response.StatusCode, "Invalid status code");
                    Assert.AreEqual(path, args.Response.StatusDescription, "Invalid status description");
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    manualEvent.Set();
                }
            });

            manualEvent.WaitOne();
            if (exception != null)
            {
                throw exception;
            }
        }

        [Test]
        public void CancelAsync()
        {
            ManualResetEvent manualEvent = new ManualResetEvent(false);
            Exception exception = null;

            IClientHttpRequest request = this.CreateRequest("/sleep/2", HttpMethod.GET);

            request.ExecuteAsync(null, delegate(ExecuteCompletedEventArgs args)
            {
                try
                {
                    Assert.IsTrue(args.Cancelled, "Invalid response");

                    WebException webEx = args.Error as WebException;
                    Assert.IsNotNull(webEx, "Invalid response exception");
                    Assert.AreEqual(WebExceptionStatus.RequestCanceled, webEx.Status, "Invalid response exception status");
                    
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    manualEvent.Set();
                }
            });

            request.CancelAsync();

            manualEvent.WaitOne();
            if (exception != null)
            {
                throw exception;
            }
        }

        #endregion


        #region Test service

        [ServiceContract]
        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
        public class TestService
        {
            [OperationContract]
            [WebInvoke(UriTemplate = "echo", Method = "PUT")]
            public Stream Echo(Stream message)
            {
                WebOperationContext context = WebOperationContext.Current;
                foreach (string headerName in context.IncomingRequest.Headers)
                {
                    context.OutgoingResponse.Headers[headerName] = context.IncomingRequest.Headers[headerName];
                }
                context.OutgoingResponse.StatusCode = HttpStatusCode.OK;

                return message;
            }

            [OperationContract]
            [WebGet(UriTemplate = "status/ok")]
            public void StatusOk()
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = "Status OK";
            }

            [OperationContract]
            [WebGet(UriTemplate = "status/notfound")]
            public void StatusNotFound()
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = "Status NotFound";
            }

            [OperationContract]
            [WebGet(UriTemplate = "methods/get")]
            public void MethodsGet()
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = "get";
            }

            [OperationContract]
            [WebInvoke(UriTemplate = "methods/delete", Method = "DELETE")]
            public void MethodsDelete()
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = "delete";
            }

            [OperationContract]
            [WebInvoke(UriTemplate = "methods/head", Method = "HEAD")]
            public void MethodsHead()
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = "head";
            }

            [OperationContract]
            [WebInvoke(UriTemplate = "methods/options", Method = "OPTIONS")]
            public void MethodsOptions()
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = "options";
            }

            [OperationContract]
            [WebInvoke(UriTemplate = "methods/post", Method = "POST")]
            public void MethodsPost()
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = "post";
            }

            [OperationContract]
            [WebInvoke(UriTemplate = "methods/put", Method = "PUT")]
            public void MethodsPut()
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = "put";
            }

            [OperationContract]
            [WebGet(UriTemplate = "sleep/{seconds}")]
            public void Sleep(string seconds)
            {
                Thread.Sleep(TimeSpan.FromSeconds(Int32.Parse(seconds)));

                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = "Status OK";
            }
        }

        #endregion
    }
}
#endif