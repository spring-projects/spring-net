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

using System;
using System.Net;
using System.Collections.Generic;

using Spring.Http;
using Spring.Http.Converters;

namespace Spring.Http.Rest.Support
{
    /// <summary>
    /// Request callback implementation that writes the given object to the request stream.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public class HttpMessageRequestCallback : AcceptHeaderRequestCallback
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(HttpMessageRequestCallback));

        #endregion

        private HttpRequestMessage requestMessage;

        /// <summary>
        /// Creates a new instance of <see cref="HttpMessageRequestCallback"/>.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestBody">The object to write to the request.</param>
        /// <param name="messageConverters">The list of <see cref="IHttpMessageConverter"/> to use.</param>
        public HttpMessageRequestCallback(HttpMethod method, object requestBody, IList<IHttpMessageConverter> messageConverters) :
            this(method, requestBody, null, messageConverters)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpMessageRequestCallback"/>.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestBody">The object to write to the request.</param>
        /// <param name="responseType">The expected response body type.</param>
        /// <param name="messageConverters">The list of <see cref="IHttpMessageConverter"/> to use.</param>
        public HttpMessageRequestCallback(HttpMethod method, object requestBody, Type responseType, IList<IHttpMessageConverter> messageConverters) :
            base(method, responseType, messageConverters)
        {
            if (requestBody is HttpRequestMessage) 
            {
                this.requestMessage = (HttpRequestMessage)requestBody;
                this.requestMessage.Method = method;
			}
			else 
            {
                this.requestMessage = new HttpRequestMessage(requestBody, method);
			}
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpMessageRequestCallback"/>.
        /// </summary>
        /// <param name="requestMessage">The HTTP request message.</param>
        /// <param name="messageConverters">The list of <see cref="IHttpMessageConverter"/> to use.</param>
        public HttpMessageRequestCallback(HttpRequestMessage requestMessage, IList<IHttpMessageConverter> messageConverters) :
            this(requestMessage, null, messageConverters)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpMessageRequestCallback"/>.
        /// </summary>
        /// <param name="requestMessage">The HTTP request message.</param>
        /// <param name="responseType">The expected response body type.</param>
        /// <param name="messageConverters">The list of <see cref="IHttpMessageConverter"/> to use.</param>
        public HttpMessageRequestCallback(HttpRequestMessage requestMessage, Type responseType, IList<IHttpMessageConverter> messageConverters) :
            base(requestMessage.Method, responseType, messageConverters)
        {
            this.requestMessage = requestMessage;
        }

        /// <summary>
        /// Gets called by <see cref="RestTemplate"/> with an opened <see cref="HttpWebRequest"/> to write data.  
        /// Does not need to care about closing the request or about handling errors: 
        /// this will all be handled by the <see cref="RestTemplate"/> class.
        /// </summary>
        /// <param name="request">The active HTTP request.</param>
        public override void DoWithRequest(HttpWebRequest request)
        {
            base.DoWithRequest(request);

            // headers
            if (requestMessage.Headers.Count > 0)
            {
                foreach(string headerName in requestMessage.Headers)
                {
                    // TODO : Check other special cases or create a HttpHeaders class
                    // Special cases
                    if (headerName == "Content-Type")
                    {
                        request.ContentType = requestMessage.Headers[HttpRequestHeader.ContentType];
                    }
                    else
                    {
                        request.Headers.Add(headerName, requestMessage.Headers[headerName]);
                    }
                }
            }

            // body
            if (requestMessage.Body != null)
            {
                object requestBody = requestMessage.Body;
                MediaType requestContentType = MediaType.ParseMediaType(requestMessage.Headers[HttpRequestHeader.ContentType]);
                foreach (IHttpMessageConverter messageConverter in base.messageConverters)
                {
                    if (messageConverter.CanWrite(requestBody.GetType(), requestContentType))
                    {
                        #region Instrumentation

                        if (LOG.IsDebugEnabled)
                        {
                            if (requestContentType != null)
                            {
                                LOG.Debug(String.Format(
                                    "Writing [{0}] as '{1}' using [{2}]",
                                    requestBody, requestContentType, messageConverter));
                            }
                            else
                            {
                                LOG.Debug(String.Format(
                                    "Writing [{0}] using [{1}]",
                                    requestBody, messageConverter));
                            }
                        }

                        #endregion

                        messageConverter.Write(requestBody, requestContentType, request);
                        return;
                    }
                }
                string message = String.Format(
                    "Could not write request: no suitable IHttpMessageConverter found for request type [{0}]", 
                    requestBody.GetType().FullName);
                if (requestContentType != null)
                {
                    message = String.Format("{0} and content type [{1}]", message, requestContentType);
                }
                throw new RestClientException(message);
            }
        }
    }
}
