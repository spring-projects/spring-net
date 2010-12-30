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
using System.Collections.Generic;

namespace Spring.Http.Converters
{
    /// <summary>
    /// Base class for most <see cref="IHttpMessageConverter"/> implementations.
    /// </summary>
    /// <remarks>
    /// This base class adds support for setting supported <see cref="MediaType"/>s, through the
    /// <see cref="P:SupportedMediaTypes"/> property. 
    /// It also adds support for 'Content-Type' when writing to the HTTP message.
    /// </remarks>
    /// <author>Arjen Poutsma</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Bruno Baia (.NET)</author>
    public abstract class AbstractHttpMessageConverter : IHttpMessageConverter
    {
        #region Logging
#if !SILVERLIGHT
        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(AbstractHttpMessageConverter));
#endif
        #endregion

        private IList<MediaType> _supportedMediaTypes = new List<MediaType>();

        #region Constructor(s)

        /// <summary>
        /// Creates a new instance of the <see cref="AbstractHttpMessageConverter"/> 
        /// with no supported media types.
        /// </summary>
        protected AbstractHttpMessageConverter()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AbstractHttpMessageConverter"/> 
        /// with multiple supported media type.
        /// </summary>
        /// <param name="supportedMediaTypes">The supported media types.</param>
        protected AbstractHttpMessageConverter(params MediaType[] supportedMediaTypes)
        {
            this._supportedMediaTypes = new List<MediaType>(supportedMediaTypes);
        }

        #endregion

        #region IHttpMessageConverter Membres

        /// <summary>
        /// Indicates whether the given class can be read by this converter.
        /// </summary>
        /// <remarks>
        /// This implementation checks if the given class is <see cref="M:Supports(Type)">supported</see>, 
        /// and if the <see cref="P:SupportedMediaTypes">supported media types</see> <see cref="M:MediaType.Includes(MediaType)">include</see> 
        /// the given media type.
        /// </remarks>
        /// <param name="type">The class to test for readability</param>
        /// <param name="mediaType">
        /// The media type to read, can be null if not specified. Typically the value of a 'Content-Type' header.
        /// </param>
        /// <returns><see langword="true"/> if readable; otherwise <see langword="false"/></returns>
        public bool CanRead(Type type, MediaType mediaType) 
        {
            return Supports(type) && CanRead(mediaType);
	    }

        /// <summary>
        /// Indicates whether the given class can be written by this converter.
        /// </summary>
        /// <remarks>
        /// This implementation checks if the given class is <see cref="M:Supports(Type)">supported</see>, 
        /// and if the <see cref="P:SupportedMediaTypes">supported media types</see> <see cref="M:MediaType.Includes(MediaType)">include</see> 
        /// the given media type.
        /// </remarks>
        /// <param name="type">The class to test for writability</param>
        /// <param name="mediaType">
        /// The media type to write, can be null if not specified. Typically the value of an 'Accept' header.
        /// </param>
        /// <returns><see langword="true"/> if writable; otherwise <see langword="false"/></returns>
	    public bool CanWrite(Type type, MediaType mediaType) 
        {
		    return Supports(type) && CanWrite(mediaType);
		}

        /// <summary>
        /// Gets or sets the list of <see cref="MediaType"/> objects supported by this converter.
        /// </summary>
        public IList<MediaType> SupportedMediaTypes
        {
            get { return _supportedMediaTypes; }
            set { _supportedMediaTypes = value; }
        }

        /// <summary>
        /// Read an object of the given type form the given HTTP message, and returns it.
        /// </summary>
        /// <remarks>
        /// This implementation simple delegates to <see cre="ReadInternal"/> method. 
        /// Future implementations might add some default behavior, however.
        /// </remarks>
        /// <typeparam name="T">
        /// The type of object to return. This type must have previously been passed to the 
        /// <see cref="M:CanRead"/> method of this interface, which must have returned <see langword="true"/>.
        /// </typeparam>
        /// <param name="message">The HTTP message to read from.</param>
        /// <returns>The converted object.</returns>
        /// <exception cref="HttpMessageNotReadableException">In case of conversion errors</exception>
        public T Read<T>(IHttpInputMessage message) where T : class
        {
            return ReadInternal<T>(message);
        }

        /// <summary>
        /// Write an given object to the given HTTP message.
        /// </summary>
        /// <remarks>
        /// This implementation delegates to <see cref="M:GetDefaultContentType"/> method if a content 
        /// type was not provided, and calls <see cref="M:WriteInternal"/>.
        /// </remarks>
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
            HttpHeaders headers = message.Headers;
            if (headers.ContentType == null)
            {
                if (contentType == null || contentType.IsWildcardType || contentType.IsWildcardSubtype)
                {
                    contentType = GetDefaultContentType(content.GetType());
                }
                if (contentType != null)
                {
                    headers.ContentType = contentType;
                }
            }
            WriteInternal(content, message);
        }

        #endregion

        /// <summary>
        /// Returns true if any of the <see cref="P:SupportedMediaTypes">supported media types</see> include the given media type.
        /// </summary>
        /// <param name="mediaType">
        /// The media type to read, can be null if not specified. Typically the value of a 'Content-Type' header.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the supported media types include the media type, or if the media type is null.
        /// </returns>        
        protected bool CanRead(MediaType mediaType) 
        {
		    if (mediaType == null) 
            {
			    return true;
		    }
		    foreach(MediaType supportedMediaType in this._supportedMediaTypes) 
            {
			    if (supportedMediaType.Includes(mediaType)) 
                {
				    return true;
			    }
		    }
		    return false;
	    }

        /// <summary>
        /// Returns true if the given media type includes any of the <see cref="P:SupportedMediaTypes">supported media types</see>.
        /// </summary>
        /// <param name="mediaType">
        /// The media type to write, can be {@code null} if not specified. Typically the value of an 'Accept' header.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the supported media types are compatible with the media type, or if the media type is null.
        /// </returns>
        protected bool CanWrite(MediaType mediaType) 
        {
            if (mediaType == null || mediaType.Equals(MediaType.ALL)) 
            {
		        return true;
	        }
	        foreach(MediaType supportedMediaType in this._supportedMediaTypes)
            {
		        if (supportedMediaType.IsCompatibleWith(mediaType)) 
                {
			        return true;
		        }
	        }
	        return false;
        }

        /// <summary>
        /// Returns the default content type for the given type. 
        /// Called when <see cref="M:Write"/> is invoked without a specified content type parameter.
        /// </summary>
        /// <remarks>
        /// By default, this returns the first element of the <see cref="P:SupportedMediaTypes"/> property, if any.
        /// </remarks>
        /// <param name="type">The type to return the content type for.</param>
        /// <returns>The <see cref="MediaType">content type</see>, or null if not known.</returns>
        protected virtual MediaType GetDefaultContentType(Type type)
        {
            return (this._supportedMediaTypes.Count > 0 ? this._supportedMediaTypes[0] : null);
        }

        /// <summary>
        /// Indicates whether the given class is supported by this converter.
        /// </summary>
        /// <param name="type">The type to test for support.</param>
        /// <returns><see langword="true"/> if supported; otherwise <see langword="false"/></returns>
        protected abstract bool Supports(Type type);

        /// <summary>
        /// Abstract template method that reads the actualy object. Invoked from <see cref="M:Read"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="message">The HTTP message to read from.</param>
        /// <returns>The converted object.</returns>
        /// <exception cref="HttpMessageNotReadableException">In case of conversion errors</exception>
        protected abstract T ReadInternal<T>(IHttpInputMessage message) where T : class;

        /// <summary>
        /// Abstract template method that writes the actual body. Invoked from <see cref="M:Write"/>.
        /// </summary>
        /// <param name="content">The object to write to the HTTP message.</param>
        /// <param name="message">The HTTP message to write to.</param>
        /// <exception cref="HttpMessageNotWritableException">In case of conversion errors</exception>
        protected abstract void WriteInternal(object content, IHttpOutputMessage message);
    }
}
