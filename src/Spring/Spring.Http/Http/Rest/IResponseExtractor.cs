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

namespace Spring.Http.Rest
{
    /// <summary>
    /// Callback interface for code that operates on a <see cref="IClientHttpResponse"/>. 
    /// Allows to manipulate the response headers, and extract the response body.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Generic callback interface used by <see cref="RestTemplate"/>'s retrieval methods. 
    /// Implementations of this interface perform the actual work of extracting data 
    /// from a <see cref="IClientHttpResponse"/>, but don't need to worry about exception 
    /// handling or closing resources.
    /// </para>
    /// <para>
    /// Used internally by the <see cref="RestTemplate"/>, but also useful for application code.
    /// </para>
    /// </remarks>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public interface IResponseExtractor<T> where T : class
    {
        /// <summary>
        /// Gets called by <see cref="RestTemplate"/> with an opened <see cref="IClientHttpResponse"/> to extract data. 
        /// Does not need to care about closing the request or about handling errors: 
        /// this will all be handled by the <see cref="RestTemplate"/> class.
        /// </summary>
        /// <param name="response">The active HTTP request.</param>
        T ExtractData(IClientHttpResponse response);
    }
}
