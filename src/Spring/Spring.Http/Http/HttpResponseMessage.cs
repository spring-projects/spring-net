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

using System.Net;

namespace Spring.Http
{
    /// <summary>
    /// Represents a HTTP response message with no body.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class HttpResponseMessage : HttpResponseMessage<object>
    {
        /// <summary>
        /// Creates a new instance of <see cref="HttpResponseMessage"/> with the given status code and status description.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="statusDescription">The HTTP status description.</param>
        public HttpResponseMessage(HttpStatusCode statusCode, string statusDescription) :
            base(null, null, statusCode, statusDescription)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpResponseMessage"/> with the given headers, status code and status description.
        /// </summary>
        /// <param name="headers">The response headers.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="statusDescription">The HTTP status description.</param>
        public HttpResponseMessage(HttpHeaders headers, HttpStatusCode statusCode, string statusDescription) :
            base(null, headers, statusCode, statusDescription)
        {
        }
    }
}
