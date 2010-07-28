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
using System.Collections.Generic;

using Spring.Util;
using Spring.Http;
using Spring.Http.Converters;

namespace Spring.Http.Rest.Support
{
    /**
     * Response extractor for {@link HttpEntity}.
     */
    public class HttpMessageResponseExtractor<T> : IResponseExtractor<HttpResponseMessage<T>> where T : class
    {
        private MessageConverterResponseExtractor<T> httpMessageConverterExtractor;

        public HttpMessageResponseExtractor(IList<IHttpMessageConverter> messageConverters)
        {
            httpMessageConverterExtractor = new MessageConverterResponseExtractor<T>(messageConverters);
        }

        public HttpResponseMessage<T> ExtractData(HttpWebResponse response)
        {
            if (StringUtils.HasText(response.Headers[HttpResponseHeader.ContentType]))
            {
                T body = httpMessageConverterExtractor.ExtractData(response);
                return new HttpResponseMessage<T>(body, response.Headers, response.StatusCode, response.StatusDescription);
            }
            else
            {
                return new HttpResponseMessage<T>(response.Headers, response.StatusCode, response.StatusDescription);
            }
        }
    }
}
