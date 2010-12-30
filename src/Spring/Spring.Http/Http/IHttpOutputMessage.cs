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

namespace Spring.Http
{
    /// <summary>
    /// Represents an HTTP message, consisting of <see cref="P:Headers">headers</see> 
    /// and a writable <see cref="P:Body">body</see>.
    /// </summary>
    /// <remarks>
    /// Typically implemented by an HTTP request on the client-side, or a response on the server-side.
    /// </remarks>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public interface IHttpOutputMessage
    {
        /// <summary>
        /// Gets the message headers.
        /// </summary>
        HttpHeaders Headers { get; }

        /// <summary>
        /// Sets the delegate that writes the body message as a stream.
        /// </summary>
        Action<Stream> Body { get; set; }
    }
}
