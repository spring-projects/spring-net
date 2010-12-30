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

using Spring.Util;

namespace Spring.Http.Client
{
    /// <summary>
    /// <see cref="IClientHttpResponse"/> implementation that uses 
    /// .NET <see cref="HttpWebResponse"/>'s class to read responses.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class WebClientHttpResponse : IClientHttpResponse
    {
        private HttpHeaders headers;
        private HttpWebResponse httpWebResponse;

        /// <summary>
        /// Gets the <see cref="HttpWebResponse"/> instance used.
        /// </summary>
        public HttpWebResponse HttpWebResponse
        {
            get { return this.httpWebResponse; }
        }

        /// <summary>
        /// Creates a new instance of <see cref="WebClientHttpResponse"/> 
        /// with the given <see cref="HttpWebResponse"/> instance.
        /// </summary>
        /// <param name="response">The <see cref="HttpWebResponse"/> instance to use.</param>
        public WebClientHttpResponse(HttpWebResponse response)
        {
            AssertUtils.ArgumentNotNull(response, "HttpWebResponse");

            this.httpWebResponse = response;
            this.headers = new HttpHeaders();

#if NET_2_0 || WINDOWS_PHONE
            foreach (string header in this.httpWebResponse.Headers)
            {
                this.headers[header] = this.httpWebResponse.Headers[header];
            }
#endif
#if SILVERLIGHT_3
            try
            {
                foreach (string header in this.httpWebResponse.Headers)
                {
                    this.headers[header] = this.httpWebResponse.Headers[header];
                }
            }
            catch(NotImplementedException)
            {
                this.headers.ContentLength = this.httpWebResponse.ContentLength;
                this.headers["Content-Type"] = this.httpWebResponse.ContentType;
            }
#elif SILVERLIGHT
            if (this.httpWebResponse.SupportsHeaders)
            {
                foreach (string header in this.httpWebResponse.Headers)
                {
                    this.headers[header] = this.httpWebResponse.Headers[header];
                }
            }
            else
            {
                this.headers.ContentLength = this.httpWebResponse.ContentLength;
                this.headers["Content-Type"] = this.httpWebResponse.ContentType;
            }
#endif
        }

        #region IClientHttpResponse Membres

        /// <summary>
        /// Gets the message headers.
        /// </summary>
        public HttpHeaders Headers
        {
            get { return this.headers; }
        }

        /// <summary>
        /// Gets the body of the message as a stream.
        /// </summary>
        public Stream Body
        {
            get
            {
                return this.httpWebResponse.GetResponseStream();
            }
        }

        /// <summary>
        /// Gets the HTTP status code of the response.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get
            {
                return this.httpWebResponse.StatusCode;
            }
        }

        /// <summary>
        /// Gets the HTTP status description of the response.
        /// </summary>
        public string StatusDescription
        {
            get
            {
                return this.httpWebResponse.StatusDescription;
            }
        }

        /// <summary>
        /// Closes this response, freeing any resources created.
        /// </summary>
        public void Close()
        {
            this.httpWebResponse.Close();
        }

        void IDisposable.Dispose()
        {
            ((IDisposable)this.httpWebResponse).Dispose();
        }

        #endregion
    }
}
