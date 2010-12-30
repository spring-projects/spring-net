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

namespace Spring.Http.Client
{
    /// <summary>
    /// Represents a client-side HTTP response.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Obtained via an 'execution' of the <see cref="IClientHttpRequest"/>.
    /// </para>
    /// <para>
    /// A client HTTP response must be <see cref="M:Close">closed</see>, 
    /// typically in a <code>finally</code> or via an <code>using</code> block.
    /// </para>
    /// </remarks>
    /// <seealso cref="IClientHttpRequest"/>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public interface IClientHttpResponse : IHttpInputMessage, IDisposable
    {
        /// <summary>
        /// Gets the HTTP status code of the response.
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the HTTP status description of the response.
        /// </summary>
        string StatusDescription { get; }

        /// <summary>
        /// Closes this response, freeing any resources created.
        /// </summary>
        void Close();
    }
}
