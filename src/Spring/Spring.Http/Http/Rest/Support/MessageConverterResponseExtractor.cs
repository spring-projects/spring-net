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
using System.Collections.Generic;

using Spring.Util;
using Spring.Http.Client;
using Spring.Http.Converters;

namespace Spring.Http.Rest.Support
{
    /// <summary>
    /// Response extractor that uses the given HTTP message converters to convert the response into a type.
    /// </summary>
    /// <typeparam name="T">The response body type.</typeparam>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public class MessageConverterResponseExtractor<T> : IResponseExtractor<T> where T : class
    {
        #region Logging
#if !SILVERLIGHT
        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(MessageConverterResponseExtractor<T>));
#endif
        #endregion

        private IList<IHttpMessageConverter> messageConverters;

        /// <summary>
        /// Creates a new instance of the <see cref="MessageConverterResponseExtractor{T}"/> class.
        /// </summary>
        /// <param name="messageConverters">The list of <see cref="IHttpMessageConverter"/> to use.</param>
        public MessageConverterResponseExtractor(IList<IHttpMessageConverter> messageConverters)
        {
            this.messageConverters = messageConverters;
        }

        /// <summary>
        /// Gets called by <see cref="RestTemplate"/> with an opened <see cref="IClientHttpResponse"/> to extract data. 
        /// Does not need to care about closing the request or about handling errors: 
        /// this will all be handled by the <see cref="RestTemplate"/> class.
        /// </summary>
        /// <param name="response">The active HTTP request.</param>
        public T ExtractData(IClientHttpResponse response)
        {
            MediaType mediaType = response.Headers.ContentType;
            if (mediaType == null)
            {
                throw new RestClientException("Could not extract response: no Content-Type found");
            }
            foreach(IHttpMessageConverter messageConverter in messageConverters) 
            {
                if (messageConverter.CanRead(typeof(T), mediaType))
                {
                    #region Instrumentation
#if !SILVERLIGHT
                    if (LOG.IsDebugEnabled) 
                    {
                        LOG.Debug(String.Format(
                            "Reading [{0}] as '{1}' using [{2}]", 
                            typeof(T).FullName, mediaType, messageConverter));
                    }
#endif
                    #endregion

                    return messageConverter.Read<T>(response);
                }
            }
            throw new RestClientException(String.Format(
                "Could not extract response: no suitable HttpMessageConverter found for response type [{0}] and content type [{1}]", 
                typeof(T).FullName, mediaType));
        }
    }
}
