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
    /**
     * Strategy interface that specifies a converter that can convert from and to HTTP requests and responses.
     *
     * @author Arjen Poutsma
     * @author Juergen Hoeller
     * @since 3.0
     */
    public interface IHttpMessageConverter
    {
        /**
         * Indicates whether the given class can be read by this converter.
         * @param clazz the class to test for readability
         * @param mediaType the media type to read, can be {@code null} if not specified. Typically the value of a
         *                  {@code Content-Type} header.
         * @return {@code true} if readable; {@code false} otherwise
         */
        bool CanRead(Type type, MediaType mediaType);

        /**
         * Indicates whether the given class can be written by this converter.
         * @param clazz the class to test for writability
         * @param mediaType the media type to write, can be {@code null} if not specified. Typically the value of an
         * 		  			{@code Accept} header.
         * @return {@code true} if writable; {@code false} otherwise
         */
        bool CanWrite(Type type, MediaType mediaType);

        /**
         * Return the list of {@link MediaType} objects supported by this converter.
         * @return the list of supported media types
         */
        IList<MediaType> SupportedMediaTypes { get; }

        /**
         * Read an object of the given type form the given input message, and returns it.
         * @param clazz the type of object to return. This type must have previously been passed to the
         * {@link #canRead canRead} method of this interface, which must have returned {@code true}.
         * @param inputMessage the HTTP input message to read from
         * @return the converted object
         * @throws IOException in case of I/O errors
         * @throws HttpMessageNotReadableException in case of conversion errors
         */
        T Read<T>(HttpWebResponse response) where T : class;

        /**
         * Write an given object to the given output message.
         * @param t the object to write to the output message. The type of this object must have previously been
         * passed to the {@link #canWrite canWrite} method of this interface, which must have returned {@code true}.
         * @param contentType the content type to use when writing. May be {@code null} to indicate that the
         * default content type of the converter must be used. If not {@code null}, this media type must have
         * previously been passed to the {@link #canWrite canWrite} method of this interface, which must have
         * returned {@code true}.
         * @param outputMessage the message to write to
         * @throws IOException in case of I/O errors
         * @throws HttpMessageNotWritableException in case of conversion errors
         */
        void Write(object content, MediaType mediaType, HttpWebRequest request);
    }
}
