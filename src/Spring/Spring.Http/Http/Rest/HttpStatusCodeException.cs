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
using System.Security.Permissions;

namespace Spring.Http.Rest
{
    /// <summary>
    /// Base class for exceptions based on a <see cref="HttpStatusCode"/>.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class HttpStatusCodeException : RestClientException
    {
        private HttpStatusCode statusCode;
        private string statusDescription;

        /// <summary>
        /// Gets the HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return this.statusCode; }
        }

        /// <summary>
        /// Gets the HTTP status description.
        /// </summary>
        public string StatusDescription
        {
            get { return this.statusDescription; }
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpStatusCodeException"/> 
        /// based on a <see cref="HttpStatusCode"/>.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        public HttpStatusCodeException(HttpStatusCode statusCode)
            : base(String.Format("The server returned '{0}' with the status code {0:d}.", statusCode))
        {
            this.statusCode = statusCode;
            this.statusDescription = statusCode.ToString();
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpStatusCodeException"/> 
        /// based on a <see cref="HttpStatusCode"/> and a status description.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="statusDescription">The HTTP status description.</param>
        public HttpStatusCodeException(HttpStatusCode statusCode, string statusDescription)
            : base(String.Format("The server returned '{0}' with the status code {1:d} - {1}.", statusDescription, statusCode))
        {
            this.statusCode = statusCode;
            this.statusDescription = statusDescription;
        }

#if !SILVERLIGHT
        /// <summary>
        /// Creates a new instance of the <see cref="RestClientException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected HttpStatusCodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
            {
                this.statusCode = (HttpStatusCode)info.GetInt32("StatusCode");
                this.statusDescription = info.GetString("StatusDescription");
            }
        }

        /// <summary>
        /// Populates the <see cref="System.Runtime.Serialization.SerializationInfo"/> with 
        /// information about the exception.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds 
        /// the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual 
        /// information about the source or destination.
        /// </param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(
            SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            if (info != null)
            {
                info.AddValue("StatusCode", (int)this.statusCode);
                info.AddValue("StatusDescription", this.statusDescription);
            }
        }
#endif
    }
}
