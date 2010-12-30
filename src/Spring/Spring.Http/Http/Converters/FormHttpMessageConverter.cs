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
using System.Text;
using System.Collections.Generic;
#if SILVERLIGHT
using Spring.Collections.Specialized;
#else
using System.Collections.Specialized;
#endif
using Spring.Util;

namespace Spring.Http.Converters
{
    /// <summary>
    /// Implementation of <see cref="IHttpMessageConverter"/> that can handle form data, 
    /// including multipart form data (i.e. file uploads).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This converter supports the 'application/x-www-form-urlencoded' and 'multipart/form-data' media 
    /// types, and read the 'application/x-www-form-urlencoded' media type (but not 'multipart/form-data').
    /// </para>
    /// <para>
    /// In other words, this converter can read and write 'normal' HTML forms (as <see cref="NameValueCollection"/>), 
    /// and it can write multipart form (as <see cref="IDictionary{String,Object}"/>). 
    /// When writing multipart, this converter uses other <see cref="IHttpMessageConverter"/> to write the respective MIME parts. 
    /// By default, basic converters are registered (supporting <see cref="String"/> and <see cref="FileInfo"/>, for instance); 
    /// these can be overridden by setting <see cref="P:PartConverters"/> property.
    /// </para>
    /// <para>
    /// For example, the following snippet shows how to submit an HTML form:
    /// <code>
    /// RestTemplate template = new RestTemplate(); // FormHttpMessageConverter is configured by default
    /// NameValueCollection form = new NameValueCollection();
    /// form.Add("field 1", "value 1");
    /// form.Add("field 2", "value 2");
    /// form.Add("field 2", "value 3");
    /// template.PostForLocation("http://example.com/myForm", form);
    /// </code>
    /// </para>
    /// <para>
    /// The following snippet shows how to do a file upload:
    /// <code>
    /// RestTemplate template = new RestTemplate();
    /// IDictionary&lt;string, object> parts = new Dictionary&lt;string, object>();
    /// parts.Add("field 1", "value 1");
    /// parts.Add("file", new FileInfo(@"C:\myFile.jpg"));
    /// template.PostForLocation("http://example.com/myFileUpload", parts);
    /// </code>
    /// </para>
    /// </remarks>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public class FormHttpMessageConverter : IHttpMessageConverter
    {
        private static char[] BOUNDARY_CHARS =
			new char[]{'-', '_', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'a', 'b', 'c', 'd', 'e', 'f', 'g',
					'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A',
					'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
				 	'V', 'W', 'X', 'Y', 'Z'};

        private Random random;
        private Encoding _charset;
        private IList<MediaType> _supportedMediaTypes;
        private IList<IHttpMessageConverter> _partConverters;

        /// <summary>
        /// Gets or sets the message body converters to use. 
        /// These converters are used to convert objects to MIME parts.
        /// </summary>
        public IList<IHttpMessageConverter> PartConverters
        {
            get { return _partConverters; }
            set { _partConverters = value; }
        }

        /// <summary>
        /// Gets or sets the encoding used for writing form data.
        /// </summary>
        public Encoding Charset
        {
            get { return this._charset; }
            set { _charset = value; }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FormHttpMessageConverter"/>.
        /// </summary>
        public FormHttpMessageConverter()
        {
            this.random = new Random();
#if SILVERLIGHT
            this._charset = new UTF8Encoding(false); // Remove byte Order Mask (BOM)
#else
            this._charset = Encoding.GetEncoding("ISO-8859-1");
#endif
            this._supportedMediaTypes = new List<MediaType>(2);
            this._supportedMediaTypes.Add(MediaType.APPLICATION_FORM_URLENCODED);
            this._supportedMediaTypes.Add(MediaType.MULTIPART_FORM_DATA);

            this._partConverters = new List<IHttpMessageConverter>(3);
            this._partConverters.Add(new ByteArrayHttpMessageConverter());
            this._partConverters.Add(new StringHttpMessageConverter());
            this._partConverters.Add(new FileInfoHttpMessageConverter());
            //this._partConverters.Add(new ResourceHttpMessageConverter());
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
            if (!typeof(NameValueCollection).IsAssignableFrom(type)) 
            {
			    return false;
            }
		    if (mediaType != null) 
            {
			    return MediaType.APPLICATION_FORM_URLENCODED.Includes(mediaType);
		    }
		    return true;
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
            if (!typeof(NameValueCollection).IsAssignableFrom(type) && 
                !typeof(IDictionary<string, object>).IsAssignableFrom(type))
            {
                return false;
            }
            if (mediaType != null)
            {
                return MediaType.APPLICATION_FORM_URLENCODED.IsCompatibleWith(mediaType) || 
                    MediaType.MULTIPART_FORM_DATA.IsCompatibleWith(mediaType);
            }
            return true;
        }

        /// <summary>
        /// Gets the list of <see cref="MediaType"/> objects supported by this converter.
        /// </summary>
        public IList<MediaType> SupportedMediaTypes
        {
            get { return _supportedMediaTypes; }
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
            // Get the message encoding
            Encoding encoding;
            MediaType mediaType = message.Headers.ContentType;
            if (mediaType == null || !StringUtils.HasText(mediaType.CharSet))
            {
                encoding = this._charset;
            }
            else
            {
                encoding = Encoding.GetEncoding(mediaType.CharSet);
            }

            // Read from the message stream  
            string body;
            using (StreamReader reader = new StreamReader(message.Body, encoding))
            {
                body = reader.ReadToEnd();
            }

            string[] pairs = body.Split('&');
            NameValueCollection result = new NameValueCollection(pairs.Length);
            foreach (string pair in pairs)
            {
                int idx = pair.IndexOf('=');
                if (idx == -1)
                {
                    result.Add(UrlDecode(pair, this._charset), null);
                }
                else
                {
                    string name = UrlDecode(pair.Substring(0, idx), this._charset);
                    string value = UrlDecode(pair.Substring(idx + 1), this._charset);
                    result.Add(name, value);
                }
            }
            return result as T;
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
    		if (content is NameValueCollection) 
            {
	            this.WriteForm((NameValueCollection) content, message);
            }
            else if (content is IDictionary<string, object>)
            {
	            this.WriteMultipart((IDictionary<string, object>) content, message);
            }
        }

        #endregion

        #region Write Form

        private void WriteForm(NameValueCollection form, IHttpOutputMessage message)
        {
            message.Headers.ContentType = MediaType.APPLICATION_FORM_URLENCODED;

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < form.AllKeys.Length; i++)
            {
                string name = form.AllKeys[i];
                string[] values = form.GetValues(name);
                if (values == null)
                {
                    builder.Append(UrlEncode(name, this._charset));
                }
                else
                {
                    for (int j = 0; j < values.Length; j++)
                    {
                        string value = values[j];
                        builder.Append(UrlEncode(name, this._charset));
                        builder.Append('=');
                        builder.Append(UrlEncode(value, this._charset));
                        if (j != (values.Length - 1))
                        {
                            builder.Append('&');
                        }
                    }
                }
                if (i != (form.AllKeys.Length - 1))
                {
                    builder.Append('&');
                }
            }

            // Create a byte array of the data we want to send  
            byte[] byteData = this._charset.GetBytes(builder.ToString());

//#if !SILVERLIGHT
//            // Set the content length in the message headers  
//            message.Headers.ContentLength = byteData.Length;
//#endif

            // Write to the message stream
            message.Body = delegate(Stream stream)
            {
                stream.Write(byteData, 0, byteData.Length);
            };
        }

        private static string UrlDecode(string url, Encoding charset)
        {
#if WINDOWS_PHONE
            return System.Net.HttpUtility.UrlDecode(url);
#elif SILVERLIGHT
            return System.Windows.Browser.HttpUtility.UrlDecode(url);
#else
            return System.Web.HttpUtility.UrlDecode(url, charset);
#endif
        }

        private static string UrlEncode(string url, Encoding charset)
        {
#if WINDOWS_PHONE
            return System.Net.HttpUtility.UrlEncode(url);
#elif SILVERLIGHT
            return System.Windows.Browser.HttpUtility.UrlEncode(url);
#else
            return System.Web.HttpUtility.UrlEncode(url, charset);
#endif
        }

        #endregion

        #region Write Multipart

        private void WriteMultipart(IDictionary<string, object> parts, IHttpOutputMessage message)
        {
		    string boundary = this.GenerateMultipartBoundary();

		    IDictionary<string, string> parameters = new Dictionary<string, string>(1);
            parameters.Add("boundary", boundary);
		    MediaType contentType = new MediaType(MediaType.MULTIPART_FORM_DATA, parameters);
		    message.Headers.ContentType = contentType;

            message.Body = delegate(Stream stream)
            {
                using (StreamWriter streamWriter = new StreamWriter(stream))
                {
                    streamWriter.NewLine = "\r\n";
                    this.WriteParts(boundary, parts, streamWriter);
                    this.WriteEnd(boundary, streamWriter);
                }
            };
	    }

        /// <summary>
        /// Generates a multipart boundary.
        /// </summary>
        /// <remarks>
        /// Default implementation returns a random boundary. Can be overridden in subclasses.
        /// </remarks>
        /// <returns>A multipart boundary</returns>
        protected virtual string GenerateMultipartBoundary()
        {
            char[] boundary = new char[random.Next(11) + 30];
            for (int i = 0; i < boundary.Length; i++)
            {
                boundary[i] = BOUNDARY_CHARS[random.Next(BOUNDARY_CHARS.Length)];
            }
            return new String(boundary);
        }

        /// <summary>
        /// Return the filename of the given multipart part 
        /// to be used for the 'Content-Disposition' header.
        /// </summary>
        /// <remarks>
        /// Default implementation returns <see cref="P:FileInfo.FullName"/> if the part is a <see cref="FileInfo"/>, 
        /// and <see langword="null"/> in other cases. Can be overridden in subclasses.
        /// </remarks>
        /// <param name="part">The part to determine the file name for</param>
        /// <returns>The filename, or <see langword="null"/> if not known</returns>
        protected virtual string GetMultipartFilename(object part)
        {
            if (part is FileInfo)
            {
                return ((FileInfo)part).FullName;
            }
            return null;
        }

        private void WriteParts(string boundary, IDictionary<string, object> parts, StreamWriter streamWriter) 
        {
		    foreach(KeyValuePair<string, object> entry in parts)
            {
			    this.WriteBoundary(boundary, streamWriter);
				HttpEntity entity = this.GetEntity(entry.Value);
				this.WritePart(entry.Key, entity, streamWriter);
                streamWriter.WriteLine();
		    }
	    }

        private void WriteBoundary(string boundary, StreamWriter streamWriter) 
        {
            streamWriter.Write("--");
            streamWriter.Write(boundary);
            streamWriter.WriteLine();
	    }

	    private void WritePart(String name, HttpEntity partEntity, StreamWriter streamWriter) 
        {
		    object partBody = partEntity.Body;
		    Type partType = partBody.GetType();
		    HttpHeaders partHeaders = partEntity.Headers;
		    MediaType partContentType = partHeaders.ContentType;
            foreach (IHttpMessageConverter messageConverter in this._partConverters)
            {
                if (messageConverter.CanWrite(partType, partContentType))
                {
                    IHttpOutputMessage multipartMessage = new MultipartHttpOutputMessage(streamWriter);
                    multipartMessage.Headers["Content-Disposition"] = this.GetContentDispositionFormData(name, this.GetMultipartFilename(partBody));
                    foreach (string header in partHeaders)
                    {
                        multipartMessage.Headers[header] = partHeaders[header];
                    }
                    messageConverter.Write(partBody, partContentType, multipartMessage);
                    return;
                }
            }
		    throw new HttpMessageNotWritableException(String.Format(
				    "Could not write request: no suitable HttpMessageConverter found for part type [{0}]", partType));
	    }

        private void WriteEnd(string boundary, StreamWriter streamWriter)
        {
            streamWriter.Write("--");
            streamWriter.Write(boundary);
            streamWriter.Write("--");
            streamWriter.WriteLine();
        }

        private HttpEntity GetEntity(object part)
        {
            if (part is HttpEntity)
            {
                return (HttpEntity)part;
            }
            return new HttpEntity(part);
        }

        /// <summary>
        /// Return the value of the 'Content-Disposition' header for 'form-data'.
        /// </summary>
        /// <param name="name">The field name</param>
        /// <param name="filename">The filename, may be <see langwrod="null"/></param>
        /// <returns>The value of the 'Content-Disposition' header</returns>
        private string GetContentDispositionFormData(string name, string filename)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("form-data; name=\"{0}\"", name);
            if (filename != null)
            {
                builder.AppendFormat("; filename=\"{0}\"", filename);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Implementation of <see cref="IHttpOutputMessage"/> used for writing multipart data.
        /// </summary>
	    private sealed class MultipartHttpOutputMessage : IHttpOutputMessage 
        {
            private HttpHeaders headers;

            private StreamWriter bodyWriter;

            public MultipartHttpOutputMessage(StreamWriter bodyWriter)
            {
                this.headers = new HttpHeaders();
                this.bodyWriter = bodyWriter;
            }

            #region IHttpMessage Membres

            public HttpHeaders Headers
            {
                get { return this.headers; } 
            }

            public Action<Stream> Body
            {
                get { throw new InvalidOperationException(); }
                set { this.WritePartBody(value); }
            }

            #endregion

            private void WritePartBody(Action<Stream> body)
            {
                foreach (string header in this.headers)
                {
                    bodyWriter.Write(header);
                    bodyWriter.Write(": ");
                    bodyWriter.Write(this.headers[header]);
                    bodyWriter.WriteLine();
                }
                bodyWriter.WriteLine();
                bodyWriter.Flush();
                Stream stream = bodyWriter.BaseStream;
                stream.Flush();
                body(stream);
                stream.Flush();
            }
        }

        #endregion
    }
}