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

namespace Spring.Http.Rest.Support
{
    /// <summary>
    /// Request callback implementation that writes the given object to the request stream.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public class HttpEntityRequestCallback : AcceptHeaderRequestCallback
    {
        #region Logging
#if !SILVERLIGHT
        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(HttpEntityRequestCallback));
#endif
        #endregion

        private HttpEntity requestEntity;

        /// <summary>
        /// Creates a new instance of <see cref="HttpEntityRequestCallback"/>.
        /// </summary>
        /// <param name="requestBody">
        /// The object to write to the request. 
        /// Can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </param>
        /// <param name="messageConverters">The list of <see cref="IHttpMessageConverter"/> to use.</param>
        public HttpEntityRequestCallback(object requestBody, IList<IHttpMessageConverter> messageConverters) :
            this(requestBody, null, messageConverters)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpEntityRequestCallback"/>.
        /// </summary>
        /// <param name="requestBody">The object to write to the request.</param>
        /// <param name="responseType">The expected response body type.</param>
        /// <param name="messageConverters">The list of <see cref="IHttpMessageConverter"/> to use.</param>
        public HttpEntityRequestCallback(object requestBody, Type responseType, IList<IHttpMessageConverter> messageConverters) :
            base(responseType, messageConverters)
        {
            if (requestBody is HttpEntity) 
            {
                this.requestEntity = (HttpEntity)requestBody;
			}
			else 
            {
                this.requestEntity = new HttpEntity(requestBody);
			}
        }

        /// <summary>
        /// Gets called by <see cref="RestTemplate"/> with an <see cref="IClientHttpRequest"/> to write data. 
        /// </summary>
        /// <remarks>
        /// Does not need to care about closing the request or about handling errors: 
        /// this will all be handled by the <see cref="RestTemplate"/> class.
        /// </remarks>
        /// <param name="request">The active HTTP request.</param>
        public override void DoWithRequest(IClientHttpRequest request)
        {
            base.DoWithRequest(request);

            // headers
            foreach (string header in requestEntity.Headers)
            {
                request.Headers[header] = requestEntity.Headers[header];
            }

            // body
            if (requestEntity.HasBody)
            {
                object requestBody = requestEntity.Body;
                MediaType requestContentType = requestEntity.Headers.ContentType;
                foreach (IHttpMessageConverter messageConverter in base.messageConverters)
                {
                    if (messageConverter.CanWrite(requestBody.GetType(), requestContentType))
                    {
                        #region Instrumentation
#if !SILVERLIGHT
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
#endif
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
            else
            {
                if (request.Headers.ContentLength == -1)
                {
                    request.Headers.ContentLength = 0;
                }
            }
        }
    }
}
