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

namespace Spring.Http.Client
{
    /// <summary>
    /// Represents a client-side HTTP request.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Created via an implementation of the <see cref="IClientHttpRequestFactory"/>.
    /// </para>
    /// <para>
    /// A client HTTP request can be executed, 
    /// getting an <see cref="IClientHttpResponse"/> which can be read from.
    /// </para>
    /// </remarks>
    /// <seealso cref="IClientHttpRequestFactory"/>
    /// <seealso cref="IClientHttpResponse"/>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public interface IClientHttpRequest : IHttpOutputMessage
    {
        /// <summary>
        /// Gets the HTTP method of the request.
        /// </summary>
        HttpMethod Method { get; }

        /// <summary>
        /// Gets the URI of the request.
        /// </summary>
        Uri Uri { get; }

#if !SILVERLIGHT
        /// <summary>
        /// Execute this request, resulting in a <see cref="IClientHttpResponse" /> that can be read.
        /// </summary>
        /// <returns>The response result of the execution</returns>
	    IClientHttpResponse Execute();
#endif

        /// <summary>
        /// Execute this request asynchronously.
        /// </summary>
        /// <param name="state">
        /// An optional user-defined object that is passed to the method invoked 
        /// when the asynchronous operation completes.
        /// </param>
        /// <param name="executeCompleted">
        /// The <see cref="Action{ExecuteCompletedEventArgs}"/> to perform when the asynchronous execution completes.
        /// </param>
        void ExecuteAsync(object state, Action<ExecuteCompletedEventArgs> executeCompleted);

        /// <summary>
        /// Cancels a pending asynchronous operation.
        /// </summary>
        void CancelAsync();
    }
}
