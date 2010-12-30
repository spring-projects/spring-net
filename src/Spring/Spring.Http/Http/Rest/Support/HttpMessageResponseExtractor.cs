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

using Spring.Http.Client;

namespace Spring.Http.Rest.Support
{
    /// <summary>
    /// Response extractor that extracts the HTTP response message with no body.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class HttpMessageResponseExtractor : IResponseExtractor<HttpResponseMessage>
    {
        /// <summary>
        /// Gets called by <see cref="RestTemplate"/> with an opened <see cref="IClientHttpResponse"/> to extract data. 
        /// Does not need to care about closing the request or about handling errors: 
        /// this will all be handled by the <see cref="RestTemplate"/> class.
        /// </summary>
        /// <param name="response">The active HTTP request.</param>
        public HttpResponseMessage ExtractData(IClientHttpResponse response)
        {
            return new HttpResponseMessage(response.Headers, response.StatusCode, response.StatusDescription);
        }
    }
}
