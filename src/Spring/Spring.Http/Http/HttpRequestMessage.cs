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

using System.Net;

namespace Spring.Http
{
    /// <summary>
    /// Represents a HTTP request message, as defined in the HTTP specification. 
    /// <a href="http://tools.ietf.org/html/rfc2616#section-5">HTTP 1.1, section 5</a>
    /// </summary>
    /// <author>Bruno Baia</author>
    public class HttpRequestMessage
    {
        //private string requestUri;
        //private string httpVersion;
        private HttpMethod method;
        private WebHeaderCollection headers;
        private object body;

        //public string RequestUri
        //{
        //    get { return this.requestUri; }
        //    set { this.requestUri = value; }
        //}

        //public string HttpVersion
        //{
        //    get { return httpVersion; }
        //    set { httpVersion = value; }
        //}

        /// <summary>
        /// Gets the HTTP method.
        /// </summary>
        public HttpMethod Method
        {
            get { return this.method; }
            set { this.method = value; }
        }

        /// <summary>
        /// Gets the request headers.
        /// </summary>
        public WebHeaderCollection Headers
        {
            get { return this.headers; }
        }

        /// <summary>
        /// Gets the response body.
        /// </summary>
        public object Body
        {
            get { return this.body; }
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpRequestMessage"/> with the given Http method.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        public HttpRequestMessage(HttpMethod method) :
            this(null, new WebHeaderCollection(), method)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpRequestMessage"/> with the given headers.
        /// </summary>
        /// <param name="headers">The request headers.</param>
        public HttpRequestMessage(WebHeaderCollection headers) :
            this(null, headers, HttpMethod.GET)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpRequestMessage"/> with the given headers and HTTP method.
        /// </summary>
        /// <param name="headers">The request headers.</param>
        /// <param name="method">The HTTP method.</param>
        public HttpRequestMessage(WebHeaderCollection headers, HttpMethod method) :
            this(null, headers, method)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpRequestMessage"/> with the given body.
        /// </summary>
        /// <param name="body">The response body.</param>
        public HttpRequestMessage(object body) :
            this(body, new WebHeaderCollection(), HttpMethod.GET)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpRequestMessage"/> with the given body and HTTP method.
        /// </summary>
        /// <param name="body">The response body.</param>
        /// <param name="method">The HTTP method.</param>
        public HttpRequestMessage(object body, HttpMethod method) :
            this(body, new WebHeaderCollection(), method)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpRequestMessage"/> with the given body and headers.
        /// </summary>
        /// <param name="body">The response body.</param>
        /// <param name="headers">The response headers.</param>
        public HttpRequestMessage(object body, WebHeaderCollection headers) :
            this(body, headers, HttpMethod.GET)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpRequestMessage"/> with the given body, headers and HTTP method.
        /// </summary>
        /// <param name="body">The response body.</param>
        /// <param name="headers">The response headers.</param>
        /// <param name="method">The HTTP method.</param>
        public HttpRequestMessage(object body, WebHeaderCollection headers, HttpMethod method)
        {
            this.method = method;
            this.body = body;
            this.headers = headers;
        }
    }
}
