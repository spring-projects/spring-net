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

using Spring.Util;
using Spring.Http;
using Spring.Http.Converters;

namespace Spring.Http.Rest.Support
{
    /**
     * Request callback implementation that writes the given object to the request stream.
     */
    public class HttpMessageRequestCallback : AcceptHeaderRequestCallback
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(HttpMessageRequestCallback));

        #endregion

        private HttpRequestMessage requestMessage;

        public HttpMessageRequestCallback(HttpMethod method, object requestBody, IList<IHttpMessageConverter> messageConverters) :
            this(method, requestBody, null, messageConverters)
        {
        }

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

        public HttpMessageRequestCallback(HttpRequestMessage requestMessage, IList<IHttpMessageConverter> messageConverters) :
            this(requestMessage, null, messageConverters)
        {
        }

        public HttpMessageRequestCallback(HttpRequestMessage requestMessage, Type responseType, IList<IHttpMessageConverter> messageConverters) :
            base(requestMessage.Method, responseType, messageConverters)
        {
            this.requestMessage = requestMessage;
        }

        public override void DoWithRequest(HttpWebRequest request)
        {
            base.DoWithRequest(request);

            // headers
            if (requestMessage.Headers.Count > 0)
            {
                request.Headers.Add(requestMessage.Headers);
            }

            // body
            if (requestMessage.Body != null)
            {
                object requestBody = requestMessage.Body;
                MediaType requestContentType = null;
                if (StringUtils.HasText(requestMessage.Headers[HttpRequestHeader.ContentType]))
                {
                    requestContentType = MediaType.ParseMediaType(requestMessage.Headers[HttpRequestHeader.ContentType]);
                }
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
