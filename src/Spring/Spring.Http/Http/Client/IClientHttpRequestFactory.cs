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

namespace Spring.Http.Client
{
    /// <summary>
    /// Factory for <see cref="IClientHttpRequest"/> objects. 
    /// Requests are created by the <see cref="M:CreateRequest"/> method.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public interface IClientHttpRequestFactory
    {
        /// <summary>
        /// Create a new <see cref="IClientHttpRequest"/> for the specified URI and HTTP method.
        /// </summary>
        /// <param name="uri">The URI to create a request for.</param>
        /// <param name="method">The HTTP method to execute.</param>
        /// <returns>The created request</returns>
        IClientHttpRequest CreateRequest(Uri uri, HttpMethod method);
    }
}
