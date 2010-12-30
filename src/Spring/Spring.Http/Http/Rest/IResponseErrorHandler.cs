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

using Spring.Http.Client;

namespace Spring.Http.Rest
{
    /// <summary>
    /// Strategy interface used by the <see cref="RestTemplate"/> to determine 
    /// whether a particular response has an error or not.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public interface IResponseErrorHandler
    {
        /// <summary>
        /// Indicates whether the given response has any errors.
        /// </summary>
        /// <remarks>
        /// Implementations will typically inspect the status code of the response.
        /// </remarks>
        /// <param name="response">The response to inspect.</param>
        /// <returns>
        /// <see langword="true"/> if the response has an error; otherwise <see langword="false"/>.
        /// </returns>
        bool HasError(IClientHttpResponse response);

        /// <summary>
        /// Handles the error in the given response. 
        /// This method is only called when <see cref="M:HasError"/> has returned <see langword="true"/>.
        /// </summary>
        /// <param name="response">The response with the error</param>
        void HandleError(IClientHttpResponse response);
    }
}
