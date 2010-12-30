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
using System.Runtime.Serialization;

namespace Spring.Http.Rest
{
    /// <summary>
    /// Exception thrown when an HTTP 4xx is received.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class HttpClientErrorException : HttpStatusCodeException
    {
        /// <summary>
        /// Creates a new instance of <see cref="HttpClientErrorException"/> 
        /// based on a <see cref="HttpStatusCode"/>.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        public HttpClientErrorException(HttpStatusCode statusCode)
            : base (statusCode)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpClientErrorException"/> 
        /// based on a <see cref="HttpStatusCode"/> and a status description.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="statusDescription">The HTTP status description.</param>
        public HttpClientErrorException(HttpStatusCode statusCode, string statusDescription)
            : base (statusCode, statusDescription)
        {
        }

#if !SILVERLIGHT
        /// <summary>
        /// Creates a new instance of the <see cref="HttpClientErrorException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected HttpClientErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
