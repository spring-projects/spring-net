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
using System.Collections.Generic;

using Spring.Util;

namespace Spring.Http.Converters
{
    /// <summary>
    /// Implementation of <see cref="IHttpMessageConverter"/> that can write files.
    /// </summary>
    /// <remarks>
    /// A mapping between file extension and mime types is used to determine the Content-Type of written files. 
    /// If no Content-Type is available, 'application/octet-stream' is used.
    /// </remarks>
    /// <author>Bruno Baia</author>
    public class FileInfoHttpMessageConverter : IHttpMessageConverter
    {
        // Pre-defined mapping between file extension and mime types
        private static IDictionary<string, string> defaultMimeMapping;

        private IList<MediaType> _supportedMediaTypes;
        private IDictionary<string, string> _mimeMapping;

        /// <summary>
        /// Gets or sets the mapping between file extension and mime types.
        /// </summary>
        public IDictionary<string, string> MimeMapping
        {
            get 
            {
                if (this._mimeMapping == null)
                {
                    this._mimeMapping = new Dictionary<string, string>(defaultMimeMapping);
                }
                return _mimeMapping; 
            }
            set { _mimeMapping = value; }
        }

        static FileInfoHttpMessageConverter()
        {
            defaultMimeMapping = new Dictionary<string, string>(9, StringComparer.OrdinalIgnoreCase);
            defaultMimeMapping.Add(".bmp", "image/bmp");
            defaultMimeMapping.Add(".gif", "image/gif");
            defaultMimeMapping.Add(".jpg", "image/jpeg");
            defaultMimeMapping.Add(".jpeg", "image/jpeg");
            defaultMimeMapping.Add(".pdf", "application/pdf");
            defaultMimeMapping.Add(".png", "image/png");
            defaultMimeMapping.Add(".tif", "image/tiff");
            defaultMimeMapping.Add(".txt", "text/plain");
            defaultMimeMapping.Add(".zip", "application/x-zip-compressed");
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileInfoHttpMessageConverter"/> 
        /// with 'application/octet-stream', and '*/*' media types.
        /// </summary>
        public FileInfoHttpMessageConverter()
        {
            this._supportedMediaTypes = new List<MediaType>();
            this._supportedMediaTypes.Add(MediaType.APPLICATION_OCTET_STREAM);
            this._supportedMediaTypes.Add(MediaType.ALL);
        }

        #region IHttpMessageConverter Membres

        /// <summary>
        /// Indicates whether the given class can be read by this converter.
        /// </summary>
        /// <param name="type">The class to test for readability</param>
        /// <param name="mediaType">
        /// The media type to read, can be null if not specified. Typically the value of a 'Content-Type' header.
        /// </param>
        /// <returns><see langword="true"/> if readable; otherwise <see langword="false"/></returns>
        public bool CanRead(Type type, MediaType mediaType)
        {
            return false;
        }

        /// <summary>
        /// Indicates whether the given class can be written by this converter.
        /// </summary>
        /// <param name="type">The class to test for writability</param>
        /// <param name="mediaType">
        /// The media type to write, can be null if not specified. Typically the value of an 'Accept' header.
        /// </param>
        /// <returns><see langword="true"/> if writable; otherwise <see langword="false"/></returns>
        public bool CanWrite(Type type, MediaType mediaType)
        {
            return type.Equals(typeof(FileInfo));
        }

        /// <summary>
        /// Gets the list of <see cref="MediaType"/> objects supported by this converter.
        /// </summary>  
        public IList<MediaType> SupportedMediaTypes
        {
            get { return this._supportedMediaTypes; }
        }

        /// <summary>
        /// Read an object of the given type form the given HTTP message, and returns it.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to return. This type must have previously been passed to the 
        /// <see cref="M:CanRead"/> method of this interface, which must have returned <see langword="true"/>.
        /// </typeparam>
        /// <param name="message">The HTTP message to read from.</param>
        /// <returns>The converted object.</returns>
        /// <exception cref="HttpMessageNotReadableException">In case of conversion errors</exception>
        public T Read<T>(IHttpInputMessage message) where T : class
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Write an given object to the given HTTP message.
        /// </summary>
        /// <param name="content">
        /// The object to write to the HTTP message. The type of this object must have previously been 
        /// passed to the <see cref="M:CanWrite"/> method of this interface, which must have returned <see langword="true"/>.
        /// </param>
        /// <param name="contentType">
        /// The content type to use when writing. May be null to indicate that the default content type of the converter must be used. 
        /// If not null, this media type must have previously been passed to the <see cref="M:CanWrite"/> method of this interface, 
        /// which must have returned <see langword="true"/>.
        /// </param>
        /// <param name="message">The HTTP message to write to.</param>
        /// <exception cref="HttpMessageNotWritableException">In case of conversion errors</exception>
        public void Write(object content, MediaType contentType, IHttpOutputMessage message)
        {
            // Get the content type
            HttpHeaders headers = message.Headers;
            if (headers.ContentType == null)
            {
                if (contentType == null || contentType.IsWildcardType || contentType.IsWildcardSubtype)
                {
                    contentType = GetContentType(content as FileInfo);
                }
                if (contentType != null)
                {
                    headers.ContentType = contentType;
                }
            }

            // Write to the message stream
            message.Body = delegate(Stream stream)
            {
                using (FileStream fs = ((FileInfo)content).OpenRead())
                {
                    IoUtils.CopyStream(fs, stream);
                }
            };
        }

        #endregion

        private MediaType GetContentType(FileInfo file)
        {
            IDictionary<string, string> mimeMapping = 
                (this._mimeMapping == null) ? defaultMimeMapping : this._mimeMapping;

            string mimeType;
            if (mimeMapping.TryGetValue(file.Extension, out mimeType))
            {
                return MediaType.Parse(mimeType);
            }
            else
            {
                return MediaType.APPLICATION_OCTET_STREAM;
            }
        }
    }
}
