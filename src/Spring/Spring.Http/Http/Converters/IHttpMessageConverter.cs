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

namespace Spring.Http.Converters
{
    // TODO: HttpMessageNotReadableException & HttpMessageNotWritableException exceptions ?

    /// <summary>
    /// Strategy interface that specifies a converter that can convert from and to HTTP requests and responses.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Bruno Baia (.NET)</author>
    public interface IHttpMessageConverter
    {
        /// <summary>
        /// Indicates whether the given class can be read by this converter.
        /// </summary>
        /// <param name="type">The class to test for readability</param>
        /// <param name="mediaType">
        /// The media type to read, can be null if not specified. Typically the value of a 'Content-Type' header.
        /// </param>
        /// <returns><see langword="true"/> if readable; otherwise <see langword="false"/></returns>
        bool CanRead(Type type, MediaType mediaType);

        /// <summary>
        /// Indicates whether the given class can be written by this converter.
        /// </summary>
        /// <param name="type">The class to test for writability</param>
        /// <param name="mediaType">
        /// The media type to write, can be null if not specified. Typically the value of an 'Accept' header.
        /// </param>
        /// <returns><see langword="true"/> if writable; otherwise <see langword="false"/></returns>
        bool CanWrite(Type type, MediaType mediaType);
 
        /// <summary>
        /// Gets the list of <see cref="MediaType"/> objects supported by this converter.
        /// </summary>        
        IList<MediaType> SupportedMediaTypes { get; }
        
        /// <summary>
        /// Read an object of the given type form the given HTTP response, and returns it.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to return. This type must have previously been passed to the 
        /// <see cref="M:CanRead"/> method of this interface, which must have returned <see langword="true"/>.
        /// </typeparam>
        /// <param name="response">The HTTP response to read from.</param>
        /// <returns>The converted object.</returns>
        T Read<T>(HttpWebResponse response) where T : class;

        /// <summary>
        /// Write an given object to the given HTTP request.
        /// </summary>
        /// <param name="content">
        /// The object to write to the HTTP request. The type of this object must have previously been 
        /// passed to the <see cref="M:CanWrite"/> method of this interface, which must have returned <see langword="true"/>.
        /// </param>
        /// <param name="mediaType">
        /// The content type to use when writing. May be null to indicate that the default content type of the converter must be used. 
        /// If not null, this media type must have previously been passed to the <see cref="M:CanWrite"/> method of this interface, 
        /// which must have returned <see langword="true"/>.
        /// </param>
        /// <param name="request">The HTTP request to write to.</param>
        void Write(object content, MediaType mediaType, HttpWebRequest request);
    }
}
