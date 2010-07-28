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
     * Response extractor that uses the given {@linkplain HttpMessageConverter entity converters} to convert the response
     * into a type <code>T</code>.
     *
     * @author Arjen Poutsma
     * @see RestTemplate
     * @since 3.0
     */
    public class MessageConverterResponseExtractor<T> : IResponseExtractor<T> where T : class
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(MessageConverterResponseExtractor<T>));

        #endregion

        private IList<IHttpMessageConverter> messageConverters;

        /**
         * Creates a new instance of the {@code HttpMessageConverterExtractor} with the given response type and message
         * converters. The given converters must support the response type.
         */
        public MessageConverterResponseExtractor(IList<IHttpMessageConverter> messageConverters)
        {
            this.messageConverters = messageConverters;
        }

        public T ExtractData(HttpWebResponse response)
        {
            if (!StringUtils.HasText(response.Headers[HttpResponseHeader.ContentType]))
            {
                throw new RestClientException("Could not extract response: no Content-Type found");
            }
            MediaType contentType = MediaType.ParseMediaType(response.Headers[HttpResponseHeader.ContentType]);
            foreach(IHttpMessageConverter messageConverter in messageConverters) 
            {
                if (messageConverter.CanRead(typeof(T), contentType))
                {
                    #region Instrumentation

                    if (LOG.IsDebugEnabled) 
                    {
                        LOG.Debug(String.Format(
                            "Reading [{0}] as '{1}' using [{2}]", 
                            typeof(T).FullName, contentType, messageConverter));
                    }

                    #endregion

                    return messageConverter.Read<T>(response);
                }
            }
            throw new RestClientException(String.Format(
                "Could not extract response: no suitable HttpMessageConverter found for response type [{0}] and content type [{1}]", 
                typeof(T).FullName, contentType));
        }
    }
}
