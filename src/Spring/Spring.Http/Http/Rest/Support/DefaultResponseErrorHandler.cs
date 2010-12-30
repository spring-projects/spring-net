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

using Spring.Http.Client;

namespace Spring.Http.Rest.Support
{
    /// <summary>
    /// Default implementation of the <see cref="IResponseErrorHandler"/> interface.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This error handler checks for the status code on the <see cref="IClientHttpResponse"/> : 
    /// any client code error (4xx) or server code error (5xx) is considered to be an error.
    /// </para>
    /// <para>
    /// This behavior can be changed by overriding the <see cref="M:HasError(HttpStatusCode)"/> method.
    /// </para>
    /// </remarks>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public class DefaultResponseErrorHandler : IResponseErrorHandler
    {
        #region IResponseErrorHandler Members

        /// <summary>
        /// Indicates whether the given response has any errors.
        /// </summary>
        /// <remarks>
        /// This implementation delegates to <see cref="M:HasError(HttpStatusCode)"/> 
        /// with the response status code.
        /// </remarks>
        /// <param name="response">The response to inspect.</param>
        /// <returns>
        /// <see langword="true"/> if the response has an error; otherwise <see langword="false"/>.
        /// </returns>
        public virtual bool HasError(IClientHttpResponse response)
        {
            return this.HasError(response.StatusCode);
        }

        /// <summary>
        /// Handles the error in the given response. 
        /// This method is only called when <see cref="M:HasError"/> has returned <see langword="true"/>.
        /// </summary>
        /// <param name="response">The response with the error</param>
        public virtual void HandleError(IClientHttpResponse response)
        {
            int type = (int)response.StatusCode / 100;
            switch (type)
            {
                case 4 :
                    throw new HttpClientErrorException(response.StatusCode, response.StatusDescription);
                case 5:
                    throw new HttpServerErrorException(response.StatusCode, response.StatusDescription);
                default :
                    throw new HttpStatusCodeException(response.StatusCode, response.StatusDescription);
            }
        }

        #endregion

        /// <summary>
        /// Checks if the given status code is a client code error (4xx) or a server code error (5xx).
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <returns>
        /// <see langword="true"/> if the response has an error; otherwise <see langword="false"/>.
        /// </returns>
        protected virtual bool HasError(HttpStatusCode statusCode)
        {
            int type = (int)statusCode / 100;
            return type == 4 || type == 5;
        }
    }
}
