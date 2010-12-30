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
using System.Collections.Generic;

using Spring.Http.Client;
using Spring.Http.Converters;

using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Http.Rest
{
    /// <summary>
    /// Unit tests for the RestTemplate class.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    [TestFixture]
    public class RestTemplateTests
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(RestTemplateTests));

        #endregion

        private MockRepository mocks;
        private RestTemplate template;
	    private IClientHttpRequestFactory requestFactory;
        private IClientHttpRequest request;
        private IClientHttpResponse response;
        private IResponseErrorHandler errorHandler;
	    private IHttpMessageConverter converter;

	    [SetUp]
	    public void SetUp() 
        {
            mocks = new MockRepository();
            requestFactory = mocks.CreateMock<IClientHttpRequestFactory>();
            request = mocks.CreateMock<IClientHttpRequest>();
            response = mocks.CreateMock<IClientHttpResponse>();
            errorHandler = mocks.CreateMock<IResponseErrorHandler>();
		    converter = mocks.CreateMock<IHttpMessageConverter>();
            
            IList<IHttpMessageConverter> messageConverters = new List<IHttpMessageConverter>(1);
            messageConverters.Add(converter);

            template = new RestTemplate();
            template.RequestFactory = requestFactory;
            template.MessageConverters = messageConverters;
            template.ErrorHandler = errorHandler;
	    }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

	    [Test]
	    public void VarArgsTemplateVariables() 
        {
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com/hotels/42/bookings/21"), HttpMethod.GET))
                    .Return(request);
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);

		    mocks.ReplayAll();

		    template.Execute<object>("http://example.com/hotels/{hotel}/bookings/{booking}", HttpMethod.GET, null, null, "42", "21");
	    }

        [Test]
        public void DictionaryTemplateVariables()
        {
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com/hotels/42/bookings/21"), HttpMethod.GET))
                .Return(request);
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);

		    mocks.ReplayAll();

            IDictionary<string, string> variables = new Dictionary<string, string>();
            variables.Add("booking", "41");
            variables.Add("hotel", "42");
            template.Execute<object>("http://example.com/hotels/{hotel}/bookings/{booking}", HttpMethod.GET, null, null, "42", "21");
        }

        [Test]
        public void BaseAddressTemplate()
        {
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com/hotels/42/bookings/21"), HttpMethod.GET))
                    .Return(request);
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);

            mocks.ReplayAll();

            template.BaseAddress = new Uri("http://example.com");
            template.Execute<object>("hotels/{hotel}/bookings/{booking}", HttpMethod.GET, null, null, "42", "21");
        }

        //[Test]
        //public void errorHandling() {
        //    Expect.Call(requestFactory.createRequest(new URI("http://example.com"), HttpMethod.GET)).andReturn(request);
        //    Expect.Call(request.execute()).andReturn(response);
        //    Expect.Call(errorHandler.hasError(response)).andReturn(true);
        //    Expect.Call(response.getStatusCode()).andReturn(HttpStatus.INTERNAL_SERVER_ERROR);
        //    Expect.Call(response.getStatusText()).andReturn("Internal Server Error");
        //    errorHandler.handleError(response);
        //    expectLastCall().andThrow(new HttpServerErrorException(HttpStatus.INTERNAL_SERVER_ERROR));
        //    response.close();

        //    mocks.ReplayAll();

        //    try {
        //        template.execute("http://example.com", HttpMethod.GET, null, null);
        //        fail("HttpServerErrorException expected");
        //    }
        //    catch (HttpServerErrorException ex) {
        //        // expected
        //    }
        //    mocks.ReplayAll();
        //}

        [Test]
        public void GetForObject() 
        {
            Expect.Call<bool>(converter.CanRead(typeof(string), null)).Return(true);
            MediaType textPlain = new MediaType("text", "plain");
            IList<MediaType> mediaTypes = new List<MediaType>(1);
            mediaTypes.Add(textPlain);
            Expect.Call<IList<MediaType>>(converter.SupportedMediaTypes).Return(mediaTypes);
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.GET)).Return(request);
            HttpHeaders requestHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(request.Headers).Return(requestHeaders).Repeat.Any();
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            responseHeaders.ContentType = textPlain;
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders).Repeat.Any();
            Expect.Call<bool>(converter.CanRead(typeof(string), textPlain)).Return(true);
            String expected = "Hello World";
            Expect.Call<string>(converter.Read<string>(response)).Return(expected);

            mocks.ReplayAll();

            string result = template.GetForObject<string>("http://example.com");
            Assert.AreEqual(expected, result, "Invalid GET result");
            Assert.AreEqual(textPlain.ToString(), requestHeaders.GetSingleValue("Accept"), "Invalid Accept header");
        }

        [Test]
        [ExpectedException(typeof(RestClientException),
            ExpectedMessage = "Could not extract response: no suitable HttpMessageConverter found for response type [System.String] and content type [bar/baz]")]
        public void GetUnsupportedMediaType() 
        {
            Expect.Call<bool>(converter.CanRead(typeof(string), null)).Return(true);
            MediaType textPlain = new MediaType("foo", "bar");
            IList<MediaType> mediaTypes = new List<MediaType>(1);
            mediaTypes.Add(textPlain);
            Expect.Call<IList<MediaType>>(converter.SupportedMediaTypes).Return(mediaTypes);
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com/resource"), HttpMethod.GET)).Return(request);
            HttpHeaders requestHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(request.Headers).Return(requestHeaders).Repeat.Any();
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            MediaType contentType = new MediaType("bar", "baz");
            responseHeaders.ContentType = contentType;
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders).Repeat.Any();
            Expect.Call<bool>(converter.CanRead(typeof(string), contentType)).Return(false);

            mocks.ReplayAll();

            template.GetForObject<string>("http://example.com/{p}", "resource");
        }

        [Test]
        public void GetForMessage() 
        {
            Expect.Call<bool>(converter.CanRead(typeof(string), null)).Return(true);
            MediaType textPlain = new MediaType("text", "plain");
            IList<MediaType> mediaTypes = new List<MediaType>(1);
            mediaTypes.Add(textPlain);
            Expect.Call<IList<MediaType>>(converter.SupportedMediaTypes).Return(mediaTypes);
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.GET)).Return(request);
            HttpHeaders requestHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(request.Headers).Return(requestHeaders).Repeat.Any();
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            responseHeaders.ContentType = textPlain;
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders).Repeat.Any();
            Expect.Call<bool>(converter.CanRead(typeof(string), textPlain)).Return(true);
            String expected = "Hello World";
            Expect.Call<string>(converter.Read<string>(response)).Return(expected);
            Expect.Call<HttpStatusCode>(response.StatusCode).Return(HttpStatusCode.OK);
            Expect.Call<string>(response.StatusDescription).Return("OK");

            mocks.ReplayAll();

            HttpResponseMessage<String> result = template.GetForMessage<string>("http://example.com");
            Assert.AreEqual(expected, result.Body, "Invalid GET result");
            Assert.AreEqual(textPlain, result.Headers.ContentType, "Invalid Content-Type");
            Assert.AreEqual(textPlain.ToString(), requestHeaders.GetSingleValue("Accept"), "Invalid Accept header");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Invalid status code");
            Assert.AreEqual("OK", result.StatusDescription, "Invalid status description");

            mocks.ReplayAll();
        }

        [Test]
        public void HeadForHeaders()
        {
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.HEAD)).Return(request);
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders).Repeat.Any();

            mocks.ReplayAll();

            HttpHeaders result = template.HeadForHeaders("http://example.com");

            Assert.AreSame(responseHeaders, result, "Invalid headers returned");
        }

        [Test]
        public void PostForLocation() 
        {
            string helloWorld = "Hello World";
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.POST)).Return(request);
            Expect.Call<bool>(converter.CanWrite(typeof(string), null)).Return(true);
            converter.Write(helloWorld, null, request);
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            Uri expected = new Uri("http://example.com/hotels");
            responseHeaders.Location = expected;
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders).Repeat.Any();

            mocks.ReplayAll();

            Uri result = template.PostForLocation("http://example.com", helloWorld);
            Assert.AreEqual(expected, result, "Invalid POST result");
        }

        [Test]
        public void PostForLocationMessageContentType() 
        {
            string helloWorld = "Hello World";
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.POST)).Return(request);
            MediaType contentType = new MediaType("text", "plain");
            Expect.Call<bool>(converter.CanWrite(typeof(string), contentType)).Return(true);
            HttpHeaders requestHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(request.Headers).Return(requestHeaders).Repeat.Any();
            converter.Write(helloWorld, contentType, request);
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            Uri expected = new Uri("http://example.com/hotels");
            responseHeaders.Location = expected;
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders).Repeat.Any();

            mocks.ReplayAll();

            HttpHeaders entityHeaders = new HttpHeaders();
            entityHeaders.ContentType = contentType;
            HttpEntity entity = new HttpEntity(helloWorld, entityHeaders);

            Uri result = template.PostForLocation("http://example.com", entity);
            Assert.AreEqual(expected, result, "Invalid POST result");
        }

        [Test]
        public void PostForLocationMessageCustomHeader() 
        {
            string helloWorld = "Hello World";
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.POST)).Return(request);
            Expect.Call<bool>(converter.CanWrite(typeof(string), null)).Return(true);
            HttpHeaders requestHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(request.Headers).Return(requestHeaders).Repeat.Any();
            converter.Write(helloWorld, null, request);
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            Uri expected = new Uri("http://example.com/hotels");
            responseHeaders.Location = expected;
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders).Repeat.Any();

            mocks.ReplayAll();

            HttpHeaders entityHeaders = new HttpHeaders();
            entityHeaders.Add("MyHeader", "MyValue");
            HttpEntity entity = new HttpEntity(helloWorld, entityHeaders);

            Uri result = template.PostForLocation("http://example.com", entity);
            Assert.AreEqual(expected, result, "Invalid POST result");
            Assert.AreEqual("MyValue", requestHeaders.Get("MyHeader"), "No custom header set");
        }

        [Test]
        public void PostForLocationNoLocation() 
        {
            string helloWorld = "Hello World";
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.POST)).Return(request);
            Expect.Call<bool>(converter.CanWrite(typeof(string), null)).Return(true);
            converter.Write(helloWorld, null, request);
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders).Repeat.Any();

            mocks.ReplayAll();

            Uri result = template.PostForLocation("http://example.com", helloWorld);
            Assert.IsNull(result, "Invalid POST result");

            mocks.ReplayAll();
        }

        [Test]
        public void PostForLocationNull()
        {
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.POST)).Return(request);
            HttpHeaders requestHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(request.Headers).Return(requestHeaders).Repeat.Any();
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders).Repeat.Any();

            mocks.ReplayAll();

            template.PostForLocation("http://example.com", null);

            Assert.AreEqual(0, requestHeaders.ContentLength, "Invalid content length");
        }

        [Test]
        public void PostForObject() 
        {
            Expect.Call<bool>(converter.CanRead(typeof(Version), null)).Return(true);
            MediaType textPlain = new MediaType("text", "plain");
            IList<MediaType> mediaTypes = new List<MediaType>(1);
            mediaTypes.Add(textPlain);
            Expect.Call<IList<MediaType>>(converter.SupportedMediaTypes).Return(mediaTypes);
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.POST)).Return(request);
            HttpHeaders requestHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(request.Headers).Return(requestHeaders).Repeat.Any();
            string helloWorld = "Hello World";
            Expect.Call<bool>(converter.CanWrite(typeof(string), null)).Return(true);
            converter.Write(helloWorld, null, request);
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            responseHeaders.ContentType = textPlain;
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders).Repeat.Any();
            Version expected = new Version(1, 0);
            Expect.Call<bool>(converter.CanRead(typeof(Version), textPlain)).Return(true);
            Expect.Call<Version>(converter.Read<Version>(response)).Return(expected);

            mocks.ReplayAll();

            Version result = template.PostForObject<Version>("http://example.com", helloWorld);
            Assert.AreEqual(expected, result, "Invalid POST result");
            Assert.AreEqual(textPlain.ToString(), requestHeaders.GetSingleValue("Accept"), "Invalid Accept header");
        }

        [Test]
        public void PostForMessage() 
        {
            Expect.Call<bool>(converter.CanRead(typeof(Version), null)).Return(true);
            MediaType textPlain = new MediaType("text", "plain");
            IList<MediaType> mediaTypes = new List<MediaType>(1);
            mediaTypes.Add(textPlain);
            Expect.Call<IList<MediaType>>(converter.SupportedMediaTypes).Return(mediaTypes);
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.POST)).Return(request);
            HttpHeaders requestHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(request.Headers).Return(requestHeaders).Repeat.Any();
            string helloWorld = "Hello World";
            Expect.Call<bool>(converter.CanWrite(typeof(string), null)).Return(true);
            converter.Write(helloWorld, null, request);
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            responseHeaders.ContentType = textPlain;
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders).Repeat.Any();
            Version expected = new Version(1, 0);
            Expect.Call<bool>(converter.CanRead(typeof(Version), textPlain)).Return(true);
            Expect.Call<Version>(converter.Read<Version>(response)).Return(expected);
            Expect.Call<HttpStatusCode>(response.StatusCode).Return(HttpStatusCode.OK);
            Expect.Call<string>(response.StatusDescription).Return("OK");

            mocks.ReplayAll();

            HttpResponseMessage<Version> result = template.PostForMessage<Version>("http://example.com", helloWorld);
            Assert.AreEqual(expected, result.Body, "Invalid POST result");
            Assert.AreEqual(textPlain, result.Headers.ContentType, "Invalid Content-Type");
            Assert.AreEqual(textPlain.ToString(), requestHeaders.GetSingleValue("Accept"), "Invalid Accept header");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Invalid status code");
            Assert.AreEqual("OK", result.StatusDescription, "Invalid status description");
        }

        [Test]
        public void PostForMessageNoBody()
        {
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.POST)).Return(request);
            HttpHeaders requestHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(request.Headers).Return(requestHeaders).Repeat.Any();
            string helloWorld = "Hello World";
            Expect.Call<bool>(converter.CanWrite(typeof(string), null)).Return(true);
            converter.Write(helloWorld, null, request);
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders);
            Expect.Call<HttpStatusCode>(response.StatusCode).Return(HttpStatusCode.Created);
            Expect.Call<string>(response.StatusDescription).Return("CREATED");

            mocks.ReplayAll();

            HttpResponseMessage result = template.PostForMessage("http://example.com", helloWorld);
            Assert.IsNull(result.Body, "Invalid POST result");
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode, "Invalid status code");
            Assert.AreEqual("CREATED", result.StatusDescription, "Invalid status description");
        }

        [Test]
        public void PostForObjectNull() 
        {
            Expect.Call<bool>(converter.CanRead(typeof(Version), null)).Return(true);
            MediaType textPlain = new MediaType("text", "plain");
            IList<MediaType> mediaTypes = new List<MediaType>(1);
            mediaTypes.Add(textPlain);
            Expect.Call<IList<MediaType>>(converter.SupportedMediaTypes).Return(mediaTypes);
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.POST)).Return(request);
            HttpHeaders requestHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(request.Headers).Return(requestHeaders).Repeat.Any();
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            responseHeaders.ContentType = textPlain;
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders).Repeat.Any();
            Expect.Call<bool>(converter.CanRead(typeof(Version), textPlain)).Return(true);
            Expect.Call<Version>(converter.Read<Version>(response)).Return(null);

            mocks.ReplayAll();

            Version result = template.PostForObject<Version>("http://example.com", null);
            Assert.IsNull(result, "Invalid POST result");
            Assert.AreEqual(textPlain.ToString(), requestHeaders.GetSingleValue("Accept"), "Invalid Accept header");

            Assert.AreEqual(0, requestHeaders.ContentLength, "Invalid content length");
        }
    	
        [Test]
        public void PostForEntityNull() 
        {
            Expect.Call<bool>(converter.CanRead(typeof(Version), null)).Return(true);
            MediaType textPlain = new MediaType("text", "plain");
            IList<MediaType> mediaTypes = new List<MediaType>(1);
            mediaTypes.Add(textPlain);
            Expect.Call<IList<MediaType>>(converter.SupportedMediaTypes).Return(mediaTypes);
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.POST)).Return(request);
            HttpHeaders requestHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(request.Headers).Return(requestHeaders).Repeat.Any();
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            responseHeaders.ContentType = textPlain;
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders).Repeat.Any();
            Expect.Call<bool>(converter.CanRead(typeof(Version), textPlain)).Return(true);
            Expect.Call<Version>(converter.Read<Version>(response)).Return(null);
            Expect.Call<HttpStatusCode>(response.StatusCode).Return(HttpStatusCode.OK);
            Expect.Call<string>(response.StatusDescription).Return("OK");

            mocks.ReplayAll();

            HttpResponseMessage<Version> result = template.PostForMessage<Version>("http://example.com", null);
            Assert.IsNull(result.Body, "Invalid POST result");
            Assert.AreEqual(textPlain, result.Headers.ContentType, "Invalid Content-Type");
            Assert.AreEqual(textPlain.ToString(), requestHeaders.GetSingleValue("Accept"), "Invalid Accept header");

            Assert.AreEqual(0, requestHeaders.ContentLength, "Invalid content length");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Invalid status code");
            Assert.AreEqual("OK", result.StatusDescription, "Invalid status description");
        }

        [Test]
        public void Put() 
        {
            Expect.Call<bool>(converter.CanWrite(typeof(string), null)).Return(true);
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.PUT)).Return(request);
            string helloWorld = "Hello World";
            converter.Write(helloWorld, null, request);
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);

            mocks.ReplayAll();

            template.Put("http://example.com", helloWorld);
        }

        [Test]
        public void PutNull()
        {
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.PUT)).Return(request);
            HttpHeaders requestHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(request.Headers).Return(requestHeaders).Repeat.Any();
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);

            mocks.ReplayAll();

            template.Put("http://example.com", null);

            Assert.AreEqual(0, requestHeaders.ContentLength, "Invalid content length");
        }

        [Test]
        public void Delete()
        {
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.DELETE)).Return(request);
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);

            mocks.ReplayAll();

            template.Delete("http://example.com");
        }

        [Test]
        public void OptionsForAllow()
        {
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.OPTIONS)).Return(request);
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            responseHeaders.Add("Allow", "GET");
            responseHeaders.Add("Allow", "POST");
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders).Repeat.Any();

            mocks.ReplayAll();

            IList<HttpMethod> result = template.OptionsForAllow("http://example.com");
            Assert.AreEqual(2, result.Count, "Invalid OPTIONS result");
            Assert.IsTrue(result.Contains(HttpMethod.GET), "Invalid OPTIONS result");
            Assert.IsTrue(result.Contains(HttpMethod.POST), "Invalid OPTIONS result");
        }

        //[Test]
        //public void ioException() {
        //    Expect.Call(converter.canRead(String.class, null)).andReturn(true);
        //    MediaType mediaType = new MediaType("foo", "bar");
        //    Expect.Call(converter.getSupportedMediaTypes()).andReturn(Collections.singletonList(mediaType));
        //    Expect.Call(requestFactory.createRequest(new URI("http://example.com/resource"), HttpMethod.GET)).andReturn(request);
        //    Expect.Call(request.getHeaders()).andReturn(new HttpHeaders());
        //    Expect.Call(request.execute()).andThrow(new IOException());

        //    mocks.ReplayAll();

        //    try {
        //        template.getForObject("http://example.com/resource", String.class);
        //        fail("RestClientException expected");
        //    }
        //    catch (ResourceAccessException ex) {
        //        // expected
        //    }

        //    mocks.ReplayAll();
        //}

        [Test]
        public void Exchange() 
        {
            Expect.Call<bool>(converter.CanRead(typeof(Version), null)).Return(true);
            MediaType textPlain = new MediaType("text", "plain");
            IList<MediaType> mediaTypes = new List<MediaType>(1);
            mediaTypes.Add(textPlain);
            Expect.Call<IList<MediaType>>(converter.SupportedMediaTypes).Return(mediaTypes);
            Expect.Call<IClientHttpRequest>(requestFactory.CreateRequest(new Uri("http://example.com"), HttpMethod.POST)).Return(request);
            HttpHeaders requestHeaders = new HttpHeaders();
            Expect.Call<HttpHeaders>(request.Headers).Return(requestHeaders).Repeat.Any();
            string helloWorld = "Hello World";
            Expect.Call<bool>(converter.CanWrite(typeof(string), null)).Return(true);
            converter.Write(helloWorld, null, request);
            ExpectGetResponse();
            Expect.Call<bool>(errorHandler.HasError(response)).Return(false);
            HttpHeaders responseHeaders = new HttpHeaders();
            responseHeaders.ContentType = textPlain;
            Expect.Call<HttpHeaders>(response.Headers).Return(responseHeaders).Repeat.Any();
            Version expected = new Version(1, 0);
            Expect.Call<bool>(converter.CanRead(typeof(Version), textPlain)).Return(true);
            Expect.Call<Version>(converter.Read<Version>(response)).Return(expected);
            Expect.Call<HttpStatusCode>(response.StatusCode).Return(HttpStatusCode.OK);
            Expect.Call<string>(response.StatusDescription).Return("OK");

            mocks.ReplayAll();

            HttpHeaders requestMessageHeaders = new HttpHeaders();
            requestMessageHeaders.Add("MyHeader", "MyValue");
            HttpEntity requestEntity = new HttpEntity(helloWorld, requestMessageHeaders);
            HttpResponseMessage<Version> result = template.Exchange<Version>("http://example.com", HttpMethod.POST, requestEntity);
            Assert.AreEqual(expected, result.Body, "Invalid POST result");
            Assert.AreEqual(textPlain, result.Headers.ContentType, "Invalid Content-Type");
            Assert.AreEqual(textPlain.ToString(), requestHeaders.GetSingleValue("Accept"), "Invalid Accept header");
            Assert.AreEqual("MyValue", requestHeaders.Get("MyHeader"), "No custom header set");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Invalid status code");
            Assert.AreEqual("OK", result.StatusDescription, "Invalid status description");
        }

        #region Utility methods

        private void ExpectGetResponse()
        {
            Expect.Call<IClientHttpResponse>(request.Execute()).Return(response);
            #region Instrumentation
            if (LOG.IsDebugEnabled)
            {
                Expect.Call<HttpStatusCode>(response.StatusCode).Return(HttpStatusCode.OK);
                Expect.Call<string>(response.StatusDescription).Return("OK");
            }
            #endregion
            Expect.Call(response.Dispose);
        }

        #endregion
    }
}
