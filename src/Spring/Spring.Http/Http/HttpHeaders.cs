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
using System.Globalization;
using System.Collections.Generic;

using Spring.Util;
#if SILVERLIGHT
using Spring.Collections.Specialized;
#else
using System.Collections.Specialized;
#endif

namespace Spring.Http
{
    /// <summary>
    /// Represents HTTP request and response headers, mapping string header names to list of string values.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>    
    public class HttpHeaders : NameValueCollection
    {
        private const string ACCEPT = "Accept";
        private const string ACCEPT_CHARSET = "Accept-Charset";
        private const string ALLOW = "Allow";
        private const string CACHE_CONTROL = "Cache-Control";
        private const string CONTENT_LENGTH = "Content-Length";
        private const string CONTENT_TYPE = "Content-Type";
        private const string DATE = "Date";
        private const string ETAG = "ETag";
        private const string EXPIRES = "Expires";
        private const string IF_MODIFIED_SINCE = "If-Modified-Since";
        private const string IF_NONE_MATCH = "If-None-Match";
        private const string LAST_MODIFIED = "Last-Modified";
        private const string LOCATION = "Location";
        private const string PRAGMA = "Pragma";

        private static readonly DateTimeFormatInfo DateTimeFormatInfo = new DateTimeFormatInfo();

        #region Constructor(s)

        /// <summary>
        /// Creates a new, empty instance of the <see cref="HttpHeaders"/> object. 
        /// </summary>
        public HttpHeaders() :
            base(8, StringComparer.OrdinalIgnoreCase)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the array of acceptable <see cref="MediaType">media types</see>, 
        /// as specified by the 'Accept' header.
        /// </summary>
        /// <remarks>
        /// Returns an empty array when the acceptable media types are unspecified.
        /// </remarks>
        public MediaType[] Accept
        {
            get
            {
                string[] values = this.GetMultiValues(ACCEPT);
                if (values == null || values.Length == 0)
                {
                    return new MediaType[0];
                }
                else
                {
                    MediaType[] result = new MediaType[values.Length];
                    for (int i = 0; i < values.Length; i++)
                    {
                        result[i] = MediaType.Parse(values[i]);
                    }
                    return result;
                }
            }
            set
            {
                foreach (MediaType mediaType in value)
                {
                    this.Add(ACCEPT, mediaType.ToString());
                }
            }
        }

        //**
        // * Set the list of acceptable {@linkplain Charset charsets}, as specified by the {@code Accept-Charset} header.
        // * @param acceptableCharsets the acceptable charsets
        // */
        //public void setAcceptCharset(List<Charset> acceptableCharsets) {
        //    StringBuilder builder = new StringBuilder();
        //    for (Iterator<Charset> iterator = acceptableCharsets.iterator(); iterator.hasNext();) {
        //        Charset charset = iterator.next();
        //        builder.append(charset.name().toLowerCase(Locale.ENGLISH));
        //        if (iterator.hasNext()) {
        //            builder.append(", ");
        //        }
        //    }
        //    set(ACCEPT_CHARSET, builder.toString());
        //}

        //**
        // * Return the list of acceptable {@linkplain Charset charsets}, as specified by the {@code Accept-Charset}
        // * header.
        // * @return the acceptable charsets
        // */
        //public List<Charset> getAcceptCharset() {
        //    List<Charset> result = new ArrayList<Charset>();
        //    String value = getFirst(ACCEPT_CHARSET);
        //    if (value != null) {
        //        String[] tokens = value.split(",\\s*");
        //        for (String token : tokens) {
        //            int paramIdx = token.indexOf(';');
        //            if (paramIdx == -1) {
        //                result.add(Charset.forName(token));
        //            }
        //            else {
        //                result.add(Charset.forName(token.substring(0, paramIdx)));
        //            }
        //        }
        //    }
        //    return result;
        //}

        /// <summary>
        /// Gets or sets the array of allowed <see cref="HttpMethod">HTTP methods</see>, 
        /// as specified by the 'Allow' header.
        /// </summary>
        /// <remarks>
        /// Returns an empty array when the allowed methods are unspecified.
        /// </remarks>
        public HttpMethod[] Allow
        {
            get
            {
                string[] values = this.GetMultiValues(ALLOW);
                if (values == null || values.Length == 0)
                {
                    return new HttpMethod[0];
                }
                else
                {
                    HttpMethod[] result = new HttpMethod[values.Length];
                    for (int i = 0; i < values.Length; i++)
                    {
                        result[i] = (HttpMethod)Enum.Parse(typeof(HttpMethod), values[i], true);
                    }
                    return result;
                }
            }
            set
            {
                foreach (HttpMethod method in value)
                {
                    this.Add(ALLOW, method.ToString());
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the 'Cache-Control' header.
        /// </summary>
        public string CacheControl
        {
            get 
            { 
                return this.Get(CACHE_CONTROL); 
            }
            set
            {
                this.Set(CACHE_CONTROL, value);
            }
        }

        /// <summary>
        /// Gets or sets the length of the body in bytes, 
        /// as specified by the 'Content-Length' header.
        /// </summary>
        /// <remarks>
        /// Returns -1 when the content-length is unknown.
        /// </remarks>
        public long ContentLength
        {
            get
            {
                string value = this.GetSingleValue(CONTENT_LENGTH);
                return (value != null ? long.Parse(value) : -1);
            }
            set
            {
                this.Set(CONTENT_LENGTH, value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="MediaType">media type</see> of the body, 
        /// as specified by the 'Content-Type' header.
        /// </summary>
        /// <remarks>
        /// Returns <see langword="null"/> when the content type is unknown.
        /// </remarks>
        public MediaType ContentType
        {
            get
            {
                string value = this.GetSingleValue(CONTENT_TYPE);
                return (value != null ? MediaType.Parse(value) : null);
            }
            set
            {
                if (value.IsWildcardType)
                {
                    throw new ArgumentException("'Content-Type' header cannot contain wildcard type '*'", "Content-Type");
                }
                if (value.IsWildcardSubtype)
                {
                    throw new ArgumentException("'Content-Type' header cannot contain wildcard subtype '*'", "Content-Type");
                }
                this.Set(CONTENT_TYPE, value.ToString());
            }
        }

        //**
        // * Returns the date and time at which the message was created, as specified by the {@code Date} header.
        // * <p>The date is returned as the number of milliseconds since January 1, 1970 GMT. Returns -1 when the date is unknown.
        // * @return the creation date/time
        // * @throws IllegalArgumentException if the value can't be converted to a date
        // */
        //**
        // * Sets the date and time at which the message was created, as specified by the {@code Date} header.
        // * <p>The date should be specified as the number of milliseconds since January 1, 1970 GMT.
        // * @param date the date
        // */
        /// <summary>
        /// Gets or sets the date and time at which the message was created, 
        /// as specified by the 'Date' header.
        /// </summary>
        /// <remarks>
        /// Returns <see langword="null"/> when the date is unknown.
        /// </remarks>
        public DateTime? Date
        {
            get
            {
                return this.GetSingleDate(DATE);
            }
            set
            {
                this.SetDate(DATE, value);
            }
        }

        /// <summary>
        /// Gets or sets the entity tag of the body, as specified by the 'ETag' header.
        /// </summary>
        public string ETag
        {
            get
            {
                return this.Unquote(this.Get(ETAG));
            }
            set
            {
                this.Set(ETAG, this.Quote(value));
            }
        }

        /// <summary>
        /// Gets or sets the date and time at which the message is no longer valid, 
        /// as specified by the 'Expires' header.
        /// </summary>
        public string Expires
        {
            get 
            {
                return this.Get(EXPIRES);
            }
            set
            {
                this.Set(EXPIRES, value);
            }
        }

        /// <summary>
        /// Gets or sets the date and time as specified by the 'If-Modified-Since' header.
        /// </summary>
        /// <remarks>
        /// Returns <see langword="null"/> when the date is unknown.
        /// </remarks>
        public DateTime? IfModifiedSince
        {
            get
            {
                return this.GetSingleDate(IF_MODIFIED_SINCE);
            }
            set
            {
                this.SetDate(IF_MODIFIED_SINCE, value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the 'If-None-Match' header.
        /// </summary>
        public string[] IfNoneMatch
        {
            get
            {
                string[] values = this.GetMultiValues(IF_NONE_MATCH);
                if (values == null || values.Length == 0)
                {
                    return new string[0];
                }
                else
                {
                    string[] result = new string[values.Length];
                    for (int i = 0; i < values.Length; i++)
                    {
                        result[i] = Unquote(values[i]);
                    }
                    return result;
                }
            }
            set
            {
                foreach (string str in value)
                {
                    this.Add(IF_NONE_MATCH, this.Quote(str));
                }
            }
        }

        /// <summary>
        /// Gets or sets the time the resource was last changed, 
        /// as specified by the 'Last-Modified' header.
        /// </summary>
        /// <remarks>
        /// Returns <see langword="null"/> when the date is unknown.
        /// </remarks>
        public DateTime? LastModified
        {
            get
            {
                return this.GetSingleDate(LAST_MODIFIED);
            }
            set
            {
                this.SetDate(LAST_MODIFIED, value);
            }
        }

        /// <summary>
        /// Gets or sets the (new) location of a resource, 
        /// as specified by the 'Location' header.
        /// </summary>
        public Uri Location
        {
            get
            {
                string value = this.GetSingleValue(LOCATION);
                return (value != null ? new Uri(value, UriKind.RelativeOrAbsolute) : null);
            }
            set
            {
                this.Set(LOCATION, value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the value of the 'Pragma' header.
        /// </summary>
        public string Pragma
        {
            get
            {
                return this.Get(PRAGMA);
            }
            set
            {
                this.Set(PRAGMA, value);
            }
        }

        #endregion

        #region Private methods

        private string Quote(string s)
        {
            if (s == null)
            {
                return null;
            }
            if (!s.StartsWith("\"") && !s.EndsWith("\""))
            {
                s = "\"" + s + "\"";
            }
            return s;
        }

        private string Unquote(string s)
        {
            if (s == null)
            {
                return null;
            }
            if (s.StartsWith("\"") && s.EndsWith("\""))
            {
                s = s.Substring(1, s.Length - 2);
            }
            return s;
        }

        private DateTime? GetSingleDate(string headerName)
        {
            string headerValue = GetSingleValue(headerName);
            if (headerValue != null)
            {
                return DateTime.Parse(headerValue, DateTimeFormatInfo).ToUniversalTime();
            }
            else
            {
                return null;
            }
        }

        private void SetDate(string headerName, DateTime? date)
        {
            if (date.HasValue)
            {
                this.Set(headerName, date.Value.ToUniversalTime().ToString("R", DateTimeFormatInfo));
            }
            else
            {
                this.Remove(headerName);
            }
        }

        #endregion

        /// <summary>
        /// Return the header value for the given header name, if any.
        /// </summary>
        /// <param name="headerName">The header name</param>
        /// <returns>The first header value; or <see langword="null"/></returns>
        /// <exception cref="NotSupportedException">
        /// If multiple values are stored for the given header name.
        /// </exception>
        public string GetSingleValue(string headerName)
        {
            string[] headerValues = this.GetValues(headerName);
            if (headerValues == null || headerValues.Length == 0)
            {
                return null;
            }
            if (headerValues.Length == 1)
            {
                return headerValues[0];
            }
            throw new NotSupportedException(String.Format(
                "Multiple values not supported for header '{0}'", headerName));
        }

        /// <summary>
        /// Return an array of header values for the given header name, if any.
        /// </summary>
        /// <param name="headerName">The header name</param>
        /// <returns>The array of header values; or <see langword="null"/></returns>
        public string[] GetMultiValues(string headerName)
        {
            string headerValue = this.Get(headerName);
            if (headerValue == null)
            {
                return null;
            }
            else
            {
                return headerValue.Split(',');
            }
        }
    }
}
