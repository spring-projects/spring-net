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

using Spring.Util;

namespace Spring.Http.Converters
{
    /**
     * Abstract base class for most {@link HttpMessageConverter} implementations.
     *
     * <p>This base class adds support for setting supported {@code MediaTypes}, through the
     * {@link #setSupportedMediaTypes(List) supportedMediaTypes} bean property. It also adds
     * support for {@code Content-Type} and {@code Content-Length} when writing to output messages.
     *
     * @author Arjen Poutsma
     * @author Juergen Hoeller
     * @since 3.0
     */
    public abstract class AbstractHttpMessageConverter : IHttpMessageConverter
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(AbstractHttpMessageConverter));

        #endregion

        private IList<MediaType> _supportedMediaTypes = new List<MediaType>();

        /**
         * Set the list of {@link MediaType} objects supported by this converter.
         */
        public IList<MediaType> SupportedMediaTypes
        {
            get { return _supportedMediaTypes; }
            set { _supportedMediaTypes = value; }
        }

        #region Constructor(s)

        /**
         * Construct an {@code AbstractHttpMessageConverter} with no supported media types.
         * @see #setSupportedMediaTypes
         */
        protected AbstractHttpMessageConverter()
        {
        }

        /**
         * Construct an {@code AbstractHttpMessageConverter} with multiple supported media type.
         * @param supportedMediaTypes the supported media types
         */
        protected AbstractHttpMessageConverter(params MediaType[] supportedMediaTypes)
        {
            this._supportedMediaTypes = new List<MediaType>(supportedMediaTypes);
        }

        #endregion

        #region IHttpMessageConverter Membres

        /**
	     * {@inheritDoc}
	     * <p>This implementation checks if the given class is {@linkplain #supports(Class) supported},
	     * and if the {@linkplain #getSupportedMediaTypes() supported media types}
	     * {@linkplain MediaType#includes(MediaType) include} the given media type.
	     */
	    public bool CanRead(Type type, MediaType mediaType) 
        {
            return Supports(type) && CanRead(mediaType);
	    }

        /**
	     * {@inheritDoc}
	     * <p>This implementation checks if the given class is {@linkplain #supports(Class) supported},
	     * and if the {@linkplain #getSupportedMediaTypes() supported media types}
	     * {@linkplain MediaType#includes(MediaType) include} the given media type.
	     */
	    public bool CanWrite(Type type, MediaType mediaType) 
        {
		    return Supports(type) && CanWrite(mediaType);
		}

        /**
         * {@inheritDoc}
         * <p>This implementation simple delegates to {@link #readInternal(Class, HttpInputMessage)}.
         * Future implementations might add some default behavior, however.
         */
        public T Read<T>(HttpWebResponse response) where T : class
        {
            return ReadInternal<T>(response);
        }

        /**
         * {@inheritDoc}
         * <p>This implementation delegates to {@link #getDefaultContentType(Object)} if a content
         * type was not provided, calls {@link #getContentLength}, and sets the corresponding headers
         * on the output message. It then calls {@link #writeInternal}.
         */
        public void Write(object content, MediaType mediaType, HttpWebRequest request)
        {
            if (!StringUtils.HasText(request.Headers[HttpRequestHeader.ContentType]))
            {
                if (mediaType == null || mediaType.IsWildcardType || mediaType.IsWildcardSubtype)
                {
                    mediaType = GetDefaultContentType(content.GetType());
                }
                if (mediaType != null)
                {
                    request.ContentType = mediaType.ToString();
                }
            }
            WriteInternal(content, request);
        }

        #endregion

    	/**
	     * Returns true if any of the {@linkplain #setSupportedMediaTypes(List) supported media types}
	     * include the given media type.
	     * @param mediaType the media type to read, can be {@code null} if not specified. Typically the value of a
	     *                  {@code Content-Type} header.
	     * @return true if the supported media types include the media type, or if the media type is {@code null}
	     */
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

        /**
         * Returns true if the given media type includes any of the
         * {@linkplain #setSupportedMediaTypes(List) supported media types}.
         * @param mediaType the media type to write, can be {@code null} if not specified. Typically the value of an
         * 		  			{@code Accept} header.
         * @return true if the supported media types are compatible with the media type, or if the media type is {@code null}
         */
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

        /**
	     * Returns the default content type for the given type. Called when {@link #write}
	     * is invoked without a specified content type parameter.
	     * <p>By default, this returns the first element of the
	     * {@link #setSupportedMediaTypes(List) supportedMediaTypes} property, if any.
	     * Can be overridden in subclasses.
	     * @param t the type to return the content type for
	     * @return the content type, or <code>null</code> if not known
	     */
        protected virtual MediaType GetDefaultContentType(Type type)
        {
            return (this._supportedMediaTypes.Count > 0 ? this._supportedMediaTypes[0] : null);
        }

        /**
         * Indicates whether the given class is supported by this converter.
         * @param clazz the class to test for support
         * @return <code>true</code> if supported; <code>false</code> otherwise
         */
        protected abstract bool Supports(Type type);

        /**
         * Abstract template method that reads the actualy object. Invoked from {@link #read}.
         * @param clazz the type of object to return
         * @param inputMessage the HTTP input message to read from
         * @return the converted object
         * @throws IOException in case of I/O errors
         * @throws HttpMessageNotReadableException in case of conversion errors
         */
        protected abstract T ReadInternal<T>(HttpWebResponse response) where T : class;

        /**
         * Abstract template method that writes the actual body. Invoked from {@link #write}.
         * @param t the object to write to the output message
         * @param outputMessage the message to write to
         * @throws IOException in case of I/O errors
         * @throws HttpMessageNotWritableException in case of conversion errors
         */
        protected abstract void WriteInternal(object content, HttpWebRequest request);
    }
}
