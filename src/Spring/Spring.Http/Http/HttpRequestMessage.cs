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
    // http://www.w3.org/Protocols/rfc2616/rfc2616-sec5.html#sec5
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

        public HttpMethod Method
        {
            get { return this.method; }
            set { this.method = value; }
        }

        /**
         * Returns the headers of this message.
         */
        public WebHeaderCollection Headers
        {
            get { return this.headers; }
        }

        /**
         * Returns the body of this message.
         */
        public object Body
        {
            get { return this.body; }
        }

        /**
         * Create a new {@code HttpRequestMessage} with no body and no headers.
         */
        public HttpRequestMessage(HttpMethod method) :
            this(null, new WebHeaderCollection(), method)
        {
        }

        /**
         * Create a new {@code HttpRequestMessage} with the given headers and no body.
         * @param headers the message headers
         */
        public HttpRequestMessage(WebHeaderCollection headers) :
            this(null, headers, HttpMethod.GET)
        {
        }

        /**
         * Create a new {@code HttpRequestMessage} with the given headers and no body.
         * @param headers the message headers
         */
        public HttpRequestMessage(WebHeaderCollection headers, HttpMethod method) :
            this(null, headers, method)
        {
        }

        /**
         * Create a new {@code HttpRequestMessage} with the given body and no headers.
         * @param body the message body
         */
        public HttpRequestMessage(object body) :
            this(body, new WebHeaderCollection(), HttpMethod.GET)
        {
        }

        /**
         * Create a new {@code HttpRequestMessage} with the given body and no headers.
         * @param body the message body
         */
        public HttpRequestMessage(object body, HttpMethod method) :
            this(body, new WebHeaderCollection(), method)
        {
        }

        /**
         * Create a new {@code HttpRequestMessage} with the given body and headers.
         * @param body the messagae body
         * @param headers the message headers
         */
        public HttpRequestMessage(object body, WebHeaderCollection headers) :
            this(body, headers, HttpMethod.GET)
        {
        }

        /**
         * Create a new {@code HttpRequestMessage} with the given body and headers.
         * @param body the messagae body
         * @param headers the message headers
         */
        public HttpRequestMessage(object body, WebHeaderCollection headers, HttpMethod method)
        {
            this.method = method;
            this.body = body;
            this.headers = headers;
        }
    }
}
