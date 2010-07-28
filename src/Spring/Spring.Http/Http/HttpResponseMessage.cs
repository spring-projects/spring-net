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
    // http://www.w3.org/Protocols/rfc2616/rfc2616-sec6.html#sec6
    public class HttpResponseMessage<T> where T : class
    {
        private WebHeaderCollection headers;
        private T body;
        private HttpStatusCode statusCode;
        private string statusDescription;

        /**
         * Returns the headers of this entity.
         */
        public WebHeaderCollection Headers
        {
            get { return this.headers; }
        }

        /**
         * Returns the body of this entity.
         */
        public T Body
        {
            get { return this.body; }
        }

        /**
         * Return the HTTP status code of the response.
         * @return the HTTP status as an HttpStatus enum value
         */
        public HttpStatusCode StatusCode
        {
            get { return statusCode; }
        }

        public string StatusDescription
        {
            get { return statusDescription; }
        }

        /**
         * Create a new {@code ResponseEntity} with the given status code, and no body, no headers.
         * @param body the entity body
         * @param statusCode the status code
         */
        public HttpResponseMessage(HttpStatusCode statusCode, string statusDescription) :
            this(null, null, statusCode, statusDescription)
        {
        }

        /**
         * Create a new {@code ResponseEntity} with the given body and status code, and no headers.
         * @param body the entity body
         * @param statusCode the status code
         */
        public HttpResponseMessage(T body, HttpStatusCode statusCode, string statusDescription) :
            this(body, null, statusCode, statusDescription)
        {
        }

        /**
         * Create a new {@code HttpEntity} with the given headers and status code, and no body.
         * @param headers the entity headers
         * @param statusCode the status code
         */
        public HttpResponseMessage(WebHeaderCollection headers, HttpStatusCode statusCode, string statusDescription) :
            this(null, headers, statusCode, statusDescription)
        {
        }

        /**
         * Create a new {@code HttpEntity} with the given body, headers, and status code.
         * @param body the entity body
         * @param headers the entity headers
         * @param statusCode the status code
         */
        public HttpResponseMessage(T body, WebHeaderCollection headers, HttpStatusCode statusCode, string statusDescription)
        {
            this.statusCode = statusCode;
            this.statusDescription = statusDescription;
            this.body = body;
            this.headers = headers;
        }
    }
}
