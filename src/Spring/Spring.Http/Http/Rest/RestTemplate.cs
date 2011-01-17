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
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;

using Spring.Http.Client;
using Spring.Http.Converters;
using Spring.Http.Converters.Xml;
#if NET_3_5 && !SILVERLIGHT
using Spring.Http.Converters.Feed;
#endif
using Spring.Http.Rest.Support;
using UriTemplate = Spring.Util.UriTemplate; // UriTemplate exists in .NET Framework since 3.5
using AssertUtils = Spring.Util.AssertUtils;
using StringUtils = Spring.Util.StringUtils;

namespace Spring.Http.Rest
{
    /// <summary>
    /// The central class for client-side HTTP access. 
    /// It simplifies communication with HTTP servers, and enforces RESTful principles. 
    /// It handles HTTP connections, leaving application code to provide URLs (with possible template variables) and extract results.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The main entry points of this template are the methods named after the six main HTTP methods: 
    /// <table>
    ///     <tr><th>HTTP method</th><th>RestTemplate methods</th></tr>
    ///     <tr><td>DELETE</td><td><see cref="M:Delete"/></td></tr>
    ///     <tr><td>GET</td><td><see cref="M:GetForObject"/></td></tr>
    ///     <tr><td></td><td><see cref="M:GetForMessage"/></td></tr>
    ///     <tr><td>HEAD</td><td><see cref="M:HeadForHeaders"/></td></tr>
    ///     <tr><td>OPTIONS</td><td><see cref="M:OptionsForAllow"/></td></tr>
    ///     <tr><td>POST</td><td><see cref="M:PostForLocation"/></td></tr>
    ///     <tr><td></td><td><see cref="M:PostForObject"/></td></tr>
    ///     <tr><td></td><td><see cref="M:PostForMessage"/></td></tr>
    ///     <tr><td>PUT</td><td><see cref="M:Put"/></td></tr>
    ///     <tr><td>Any</td><td><see cref="M:Exchange"/></td></tr>
    ///     <tr><td></td><td><see cref="M:Execute"/></td></tr>
    /// </table>
    /// </para>
    /// <para>
    /// For each of these HTTP methods, there are three corresponding Java methods in the <see cref="RestTemplate"/>. 
    /// Two variant take a string URI as first argument and are capable of substituting any URI templates in 
    /// that URL using either a string variable arguments array, or a string dictionary. 
    /// The string varargs variant expands the given template variables in order, so that 
    /// <code>
    /// string result = restTemplate.GetForObject&lt;string>("http://example.com/hotels/{hotel}/bookings/{booking}", "42", "21");
    /// </code>
    /// will perform a GET on 'http://example.com/hotels/42/bookings/21'. The map variant expands the template based on
    /// variable name, and is therefore more useful when using many variables, or when a single variable is used multiple
    /// times. For example:
    /// <code>
    /// IDictionary&lt;String, String&gt; vars = new Dictionary&lt;String, String&gt;();
    /// vars.Add("hotel", "42");
    /// string result = restTemplate.GetForObject&lt;string>("http://example.com/hotels/{hotel}/rooms/{hotel}", vars);
    /// </code>
    /// will perform a GET on 'http://example.com/hotels/42/rooms/42'. Alternatively, there are URI variant 
    /// methods, which do not allow for URI templates, but allow you to reuse a single, expanded URI multiple times.
    /// </para>
    /// <para>
    /// Furthermore, the string-argument methods assume that the URL String is unencoded. This means that
    /// <code>
    /// restTemplate.GetForObject&lt;string>("http://example.com/hotel list");
    /// </code>
    /// will perform a GET on 'http://example.com/hotel%20list'.
    /// </para>
    /// <para>
    /// Objects passed to and returned from these methods are converted to and from HTTP messages by 
    /// <see cref="IHttpMessageConverter"/> instances. Converters for the main mime types are registered by default, 
    /// but you can also write your own converter and register it via the <see cref="P:MessageConverters"/> property.
    /// </para>
    /// <para>
    /// This template uses a <see cref="WebClientHttpRequestFactory"/> and a <see cref="DefaultResponseErrorHandler"/> 
    /// as default strategies for creating HTTP connections or handling HTTP errors, respectively. 
    /// These defaults can be overridden through the <see cref="P:RequestFactory"/> and <see cref="P:ErrorHandler"/> properties.
    /// </para>
    /// </remarks>
    /// <see cref="IClientHttpRequestFactory"/>
    /// <see cref="IResponseErrorHandler"/>
    /// <see cref="IHttpMessageConverter"/>
    /// <see cref="IRequestCallback"/>
    /// <see cref="IResponseExtractor{T}"/>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public class RestTemplate : 
#if !SILVERLIGHT
        IRestOperations, 
#endif
        IRestAsyncOperations
    {
        #region Logging
#if !SILVERLIGHT
        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(RestTemplate));
#endif
        #endregion

        #region Fields / Properties

        private Uri _baseAddress;
        private bool _throwExceptionOnError;
        private IList<IHttpMessageConverter> _messageConverters;
        private IClientHttpRequestFactory _requestFactory;
        private IResponseErrorHandler _errorHandler;

        private IResponseExtractor<HttpHeaders> headersExtractor;


        /// <summary>
        /// Gets or sets the base URL for the request.
        /// </summary>
        public Uri BaseAddress
        {
            get
            {
                return this._baseAddress;
            }
            set
            {
                AssertUtils.ArgumentNotNull(value, "BaseAddress");
                if (!value.IsAbsoluteUri)
                {
                    throw new ArgumentException(String.Format("'{0}' is not an absolute URI", value), "BaseAddress");
                }
                this._baseAddress = value;
            }
        }

        /// <summary>
        /// Indicates if an exception is thrown when a HTTP client or server error happens (HTTP status code 3xx or 4xx). 
        /// By default, the value is <see langword="true"/> to be consistent with .NET behavior in <see cref="M:HttpWebRequest.GetResponse()"/>.
        /// </summary>
        public bool ThrowExceptionOnError
        {
            get { return _throwExceptionOnError; }
            set { _throwExceptionOnError = value; }
        }

        /// <summary>
        /// Gets or sets the message converters. 
        /// These converters are used to convert from and to HTTP request and response messages.
        /// </summary>
        public IList<IHttpMessageConverter> MessageConverters
        {
            get { return this._messageConverters; }
            set { this._messageConverters = value; }
        }

        /// <summary>
        /// Gets or sets the request factory that this class uses for obtaining for <see cref="IClientHttpRequest"/> objects.
        /// </summary>
        /// <remarks>
        /// Default value is <see cref="WebClientHttpRequestFactory"/>.
        /// </remarks>
        public IClientHttpRequestFactory RequestFactory
        {
            get { return this._requestFactory; }
            set { this._requestFactory = value; }
        }

        /// <summary>
        /// Gets or sets the error handler.
        /// </summary>
        /// <remarks>
        /// Default value is <see cref="DefaultResponseErrorHandler"/>.
        /// </remarks>
        public IResponseErrorHandler ErrorHandler
        {
            get { return this._errorHandler; }
            set { this._errorHandler = value; }
        }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Creates a new instance of <see cref="RestTemplate"/>.
        /// </summary>
        /// <param name="baseAddress">The base address to use.</param>
        public RestTemplate(Uri baseAddress) :
            this()
        {
            this.BaseAddress = baseAddress;
        }

        /// <summary>
        /// Creates a new instance of <see cref="RestTemplate"/>.
        /// </summary>
        /// <param name="baseAddress">The base address to use.</param>
        public RestTemplate(string baseAddress) :
            this() 
        {
            this.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
        }

        /// <summary>
        /// Creates a new instance of <see cref="RestTemplate"/>.
        /// </summary>
        public RestTemplate()
        {
            this._throwExceptionOnError = true;
            this.headersExtractor = new HeadersResponseExtractor();

            this._requestFactory = new WebClientHttpRequestFactory();
            this._errorHandler = new DefaultResponseErrorHandler();

            this._messageConverters = new List<IHttpMessageConverter>();

            this._messageConverters.Add(new ByteArrayHttpMessageConverter());
            this._messageConverters.Add(new StringHttpMessageConverter());
            this._messageConverters.Add(new FormHttpMessageConverter());
#if !SILVERLIGHT
            this._messageConverters.Add(new XmlDocumentHttpMessageConverter());
#endif
#if NET_3_0 || SILVERLIGHT
            this._messageConverters.Add(new DataContractHttpMessageConverter());
#else // NET_2_0 only
            this._messageConverters.Add(new XmlSerializableHttpMessageConverter());
#endif
#if NET_3_5 || WINDOWS_PHONE
            this._messageConverters.Add(new XElementHttpMessageConverter());
#if NET_3_5
            this._messageConverters.Add(new Rss20FeedHttpMessageConverter());
            this._messageConverters.Add(new Atom10FeedHttpMessageConverter());
#endif
#endif
            //this._messageConverters.Add(new JsonHttpMessageConverter());            
        }

        #endregion

        #region IRestOperations Membres

#if !SILVERLIGHT
        #region GET

        /// <summary>
        /// Retrieve a representation by doing a GET on the specified URL. 
        /// The response (if any) is converted and returned.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The converted object</returns>
        public T GetForObject<T>(string url, params object[] uriVariables) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, HttpMethod.GET, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Retrieve a representation by doing a GET on the specified URL. 
        /// The response (if any) is converted and returned.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The converted object</returns>
        public T GetForObject<T>(string url, IDictionary<string, object> uriVariables) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, HttpMethod.GET, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Retrieve a representation by doing a GET on the specified URL. 
        /// The response (if any) is converted and returned.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <returns>The converted object</returns>
        public T GetForObject<T>(Uri url) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, HttpMethod.GET, requestCallback, responseExtractor);
        }

        /// <summary>
        /// Retrieve an entity by doing a GET on the specified URL. 
        /// The response is converted and stored in an <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The HTTP response message.</returns>
        public HttpResponseMessage<T> GetForMessage<T>(string url, params object[] uriVariables) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, HttpMethod.GET, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Retrieve an entity by doing a GET on the specified URL. 
        /// The response is converted and stored in an <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The HTTP response message.</returns>
        public HttpResponseMessage<T> GetForMessage<T>(string url, IDictionary<string, object> uriVariables) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, HttpMethod.GET, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Retrieve an entity by doing a GET on the specified URL. 
        /// The response is converted and stored in an <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <returns>The HTTP response message.</returns>
        public HttpResponseMessage<T> GetForMessage<T>(Uri url) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, HttpMethod.GET, requestCallback, responseExtractor);
        }

        #endregion

        #region HEAD

        /// <summary>
        /// Retrieve all headers of the resource specified by the URI template.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>All HTTP headers of that resource</returns>
        public HttpHeaders HeadForHeaders(string url, params object[] uriVariables)
        {
            return this.Execute<HttpHeaders>(url, HttpMethod.HEAD, null, this.headersExtractor, uriVariables);
        }

        /// <summary>
        /// Retrieve all headers of the resource specified by the URI template.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>All HTTP headers of that resource</returns>
        public HttpHeaders HeadForHeaders(string url, IDictionary<string, object> uriVariables)
        {
            return this.Execute<HttpHeaders>(url, HttpMethod.HEAD, null, this.headersExtractor, uriVariables);
        }

        /// <summary>
        /// Retrieve all headers of the resource specified by the URI template.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>All HTTP headers of that resource</returns>
        public HttpHeaders HeadForHeaders(Uri url)
        {
            return this.Execute<HttpHeaders>(url, HttpMethod.HEAD, null, this.headersExtractor);
        }

        #endregion

        #region POST

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the value of the 'Location' header. 
        /// This header typically indicates where the new resource is stored.
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The value for the Location header.</returns>
        public Uri PostForLocation(string url, object request, params object[] uriVariables)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            HttpHeaders headers = this.Execute<HttpHeaders>(
                url, HttpMethod.POST, requestCallback, this.headersExtractor, uriVariables);
            return headers.Location;
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the value of the 'Location' header. 
        /// This header typically indicates where the new resource is stored.
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The value for the Location header.</returns>
        public Uri PostForLocation(string url, object request, IDictionary<string, object> uriVariables)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            HttpHeaders headers = this.Execute<HttpHeaders>(
                url, HttpMethod.POST, requestCallback, this.headersExtractor, uriVariables);
            return headers.Location;
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the value of the 'Location' header. 
        /// This header typically indicates where the new resource is stored.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <returns>The value for the Location header.</returns>
        public Uri PostForLocation(Uri url, object request)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            HttpHeaders headers = this.Execute<HttpHeaders>(
                url, HttpMethod.POST, requestCallback, this.headersExtractor);
            return headers.Location;
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the representation found in the response. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The converted object.</returns>
        public T PostForObject<T>(string url, object request, params object[] uriVariables) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, HttpMethod.POST, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the representation found in the response. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The converted object.</returns>
        public T PostForObject<T>(string url, object request, IDictionary<string, object> uriVariables) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, HttpMethod.POST, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the representation found in the response. 
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <returns>The converted object.</returns>
        public T PostForObject<T>(Uri url, object request) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, HttpMethod.POST, requestCallback, responseExtractor);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The HTTP response message.</returns>
        public HttpResponseMessage<T> PostForMessage<T>(string url, object request, params object[] uriVariables) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, HttpMethod.POST, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The HTTP response message.</returns>
        public HttpResponseMessage<T> PostForMessage<T>(string url, object request, IDictionary<string, object> uriVariables) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, HttpMethod.POST, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>. 
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <returns>The HTTP response message.</returns>
        public HttpResponseMessage<T> PostForMessage<T>(Uri url, object request) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, HttpMethod.POST, requestCallback, responseExtractor);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The HTTP response message with no entity.</returns>
        public HttpResponseMessage PostForMessage(string url, object request, params object[] uriVariables)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            return this.Execute<HttpResponseMessage>(url, HttpMethod.POST, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The HTTP response message with no entity.</returns>
        public HttpResponseMessage PostForMessage(string url, object request, IDictionary<string, object> uriVariables)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            return this.Execute<HttpResponseMessage>(url, HttpMethod.POST, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>. 
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <returns>The HTTP response message with no entity.</returns>
        public HttpResponseMessage PostForMessage(Uri url, object request)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            return this.Execute<HttpResponseMessage>(url, HttpMethod.POST, requestCallback, responseExtractor);
        }

        #endregion

        #region PUT

        /// <summary>
        /// Create or update a resource by PUTting the given object to the URI.
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void Put(string url, object request, params object[] uriVariables)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            this.Execute<object>(url, HttpMethod.PUT, requestCallback, null, uriVariables);
        }

        /// <summary>
        /// Create or update a resource by PUTting the given object to the URI.
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        public void Put(string url, object request, IDictionary<string, object> uriVariables)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            this.Execute<object>(url, HttpMethod.PUT, requestCallback, null, uriVariables);
        }

        /// <summary>
        /// Create or update a resource by PUTting the given object to the URI.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        public void Put(Uri url, object request)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            this.Execute<object>(url, HttpMethod.PUT, requestCallback, null);
        }

        #endregion

        #region DELETE

        /// <summary>
        /// Delete the resources at the specified URI.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void Delete(string url, params object[] uriVariables)
        {
            this.Execute<object>(url, HttpMethod.DELETE, null, null, uriVariables);
        }

        /// <summary>
        /// Delete the resources at the specified URI.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        public void Delete(string url, IDictionary<string, object> uriVariables)
        {
            this.Execute<object>(url, HttpMethod.DELETE, null, null, uriVariables);
        }

        /// <summary>
        /// Delete the resources at the specified URI.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void Delete(Uri url)
        {
            this.Execute<object>(url, HttpMethod.DELETE, null, null);
        }

        #endregion

        #region OPTIONS

        /// <summary>
        /// Return the value of the Allow header for the given URI.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The value of the allow header.</returns>
        public IList<HttpMethod> OptionsForAllow(string url, params object[] uriVariables)
        {
            HttpHeaders headers = this.Execute<HttpHeaders>(
                url, HttpMethod.OPTIONS, null, this.headersExtractor, uriVariables);
            return new List<HttpMethod>(headers.Allow);
        }

        /// <summary>
        /// Return the value of the Allow header for the given URI.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The value of the allow header.</returns>
        public IList<HttpMethod> OptionsForAllow(string url, IDictionary<string, object> uriVariables)
        {
            HttpHeaders headers = this.Execute<HttpHeaders>(
                url, HttpMethod.OPTIONS, null, this.headersExtractor, uriVariables);
            return new List<HttpMethod>(headers.Allow);
        }

        /// <summary>
        /// Return the value of the Allow header for the given URI.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The value of the allow header.</returns>
        public IList<HttpMethod> OptionsForAllow(Uri url)
        {
            HttpHeaders headers = this.Execute<HttpHeaders>(
                url, HttpMethod.OPTIONS, null, this.headersExtractor);
            return new List<HttpMethod>(headers.Allow);
        }

        #endregion


        #region Exchange

        /// <summary>
        /// Execute the HTTP method to the given URI template, writing the given request message to the request, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestEntity">
        /// The HTTP entity (headers and/or body) to write to the request, may be <see langword="null"/>.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The HTTP response message.</returns>
        public HttpResponseMessage<T> Exchange<T>(string url, HttpMethod method, HttpEntity requestEntity, params object[] uriVariables) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(requestEntity, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, method, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Execute the HTTP method to the given URI template, writing the given request message to the request, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestEntity">
        /// The HTTP entity (headers and/or body) to write to the request, may be <see langword="null"/>.
        /// </param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The HTTP response message.</returns>
        public HttpResponseMessage<T> Exchange<T>(string url, HttpMethod method, HttpEntity requestEntity, IDictionary<string, object> uriVariables) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(requestEntity, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, method, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Execute the HTTP method to the given URI template, writing the given request message to the request, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestEntity">
        /// The HTTP entity (headers and/or body) to write to the request, may be <see langword="null"/>.
        /// </param>
        /// <returns>The HTTP response message.</returns>
        public HttpResponseMessage<T> Exchange<T>(Uri url, HttpMethod method, HttpEntity requestEntity) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(requestEntity, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, method, requestCallback, responseExtractor);
        }

        /// <summary>
        /// Execute the HTTP method to the given URI template, writing the given request message to the request, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestEntity">
        /// The HTTP entity (headers and/or body) to write to the request, may be <see langword="null"/>.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The HTTP response message with no entity.</returns>
        public HttpResponseMessage Exchange(string url, HttpMethod method, HttpEntity requestEntity, params object[] uriVariables)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(requestEntity, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            return this.Execute<HttpResponseMessage>(url, method, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Execute the HTTP method to the given URI template, writing the given request message to the request, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestEntity">
        /// The HTTP entity (headers and/or body) to write to the request, may be <see langword="null"/>.
        /// </param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The HTTP response message with no entity.</returns>
        public HttpResponseMessage Exchange(string url, HttpMethod method, HttpEntity requestEntity, IDictionary<string, object> uriVariables)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(requestEntity, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            return this.Execute<HttpResponseMessage>(url, method, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Execute the HTTP method to the given URI template, writing the given request message to the request, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestEntity">
        /// The HTTP entity (headers and/or body) to write to the request, may be <see langword="null"/>.
        /// </param>
        /// <returns>The HTTP response message with no entity.</returns>
        public HttpResponseMessage Exchange(Uri url, HttpMethod method, HttpEntity requestEntity)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(requestEntity, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            return this.Execute<HttpResponseMessage>(url, method, requestCallback, responseExtractor);
        }

        #endregion

        #region General execution

        /// <summary>
        /// Execute the HTTP request to the given URI template, preparing the request with the 
        /// <see cref="IRequestCallback"/>, and reading the response with an <see cref="IResponseExtractor{T}"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestCallback">Object that prepares the request.</param>
        /// <param name="responseExtractor">Object that extracts the return value from the response.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>An arbitrary object, as returned by the <see cref="IResponseExtractor{T}"/>.</returns>        
        public T Execute<T>(string url, HttpMethod method, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor, params object[] uriVariables) where T : class
        {
            return this.DoExecute<T>(this.BuildUri(this._baseAddress, url, uriVariables), method, requestCallback, responseExtractor);
        }

        /// <summary>
        /// Execute the HTTP request to the given URI template, preparing the request with the 
        /// <see cref="IRequestCallback"/>, and reading the response with an <see cref="IResponseExtractor{T}"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestCallback">Object that prepares the request.</param>
        /// <param name="responseExtractor">Object that extracts the return value from the response.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>An arbitrary object, as returned by the <see cref="IResponseExtractor{T}"/>.</returns>   
        public T Execute<T>(string url, HttpMethod method, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor, IDictionary<string, object> uriVariables) where T : class
        {
            return this.DoExecute<T>(this.BuildUri(this._baseAddress, url, uriVariables), method, requestCallback, responseExtractor);
        }

        /// <summary>
        /// Execute the HTTP request to the given URI template, preparing the request with the 
        /// <see cref="IRequestCallback"/>, and reading the response with an <see cref="IResponseExtractor{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestCallback">Object that prepares the request.</param>
        /// <param name="responseExtractor">Object that extracts the return value from the response.</param>
        /// <returns>An arbitrary object, as returned by the <see cref="IResponseExtractor{T}"/>.</returns>   
        public T Execute<T>(Uri url, HttpMethod method, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor) where T : class
        {
            return this.DoExecute<T>(this.BuildUri(this._baseAddress, url), method, requestCallback, responseExtractor);
        }

        #endregion
#endif

        #endregion

        #region IRestAsyncOperations Membres

        #region GET

        /// <summary>
        /// Asynchronously retrieve a representation by doing a GET on the specified URL. 
        /// The response (if any) is converted.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="getCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous GET method completes.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void GetForObjectAsync<T>(string url, Action<MethodCompletedEventArgs<T>> getCompleted, params object[] uriVariables) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            this.ExecuteAsync<T>(url, HttpMethod.GET, requestCallback, responseExtractor, getCompleted, uriVariables);
        }

        /// <summary>
        /// Asynchronously retrieve a representation by doing a GET on the specified URL. 
        /// The response (if any) is converted.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <param name="getCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous GET method completes.
        /// </param>
        public void GetForObjectAsync<T>(string url, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<T>> getCompleted) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            this.ExecuteAsync<T>(url, HttpMethod.GET, requestCallback, responseExtractor, uriVariables, getCompleted);
        }

        /// <summary>
        /// Asynchronously retrieve a representation by doing a GET on the specified URL. 
        /// The response (if any) is converted.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="getCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous GET method completes.
        /// </param>
        public void GetForObjectAsync<T>(Uri url, Action<MethodCompletedEventArgs<T>> getCompleted) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            this.ExecuteAsync<T>(url, HttpMethod.GET, requestCallback, responseExtractor, getCompleted);
        }

        /// <summary>
        /// Asynchronously retrieve an entity by doing a GET on the specified URL. 
        /// The response is converted and stored in an <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="getCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous GET method completes.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void GetForMessageAsync<T>(string url, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> getCompleted, params object[] uriVariables) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            this.ExecuteAsync<HttpResponseMessage<T>>(url, HttpMethod.GET, requestCallback, responseExtractor, getCompleted, uriVariables);
        }

        /// <summary>
        /// Asynchronously retrieve an entity by doing a GET on the specified URL. 
        /// The response is converted and stored in an <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <param name="getCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous GET method completes.
        /// </param>
        public void GetForMessageAsync<T>(string url, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> getCompleted) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            this.ExecuteAsync<HttpResponseMessage<T>>(url, HttpMethod.GET, requestCallback, responseExtractor, uriVariables, getCompleted);
        }

        /// <summary>
        /// Asynchronously retrieve an entity by doing a GET on the specified URL. 
        /// The response is converted and stored in an <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="getCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous GET method completes.
        /// </param>
        public void GetForMessageAsync<T>(Uri url, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> getCompleted) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            this.ExecuteAsync<HttpResponseMessage<T>>(url, HttpMethod.GET, requestCallback, responseExtractor, getCompleted);
        }

        #endregion

        #region HEAD

        /// <summary>
        /// Asynchronously retrieve all headers of the resource specified by the URI template.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="headCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous HEAD method completes.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void HeadForHeadersAsync(string url, Action<MethodCompletedEventArgs<HttpHeaders>> headCompleted, params object[] uriVariables)
        {
            this.ExecuteAsync<HttpHeaders>(url, HttpMethod.HEAD, null, this.headersExtractor, headCompleted, uriVariables);
        }

        /// <summary>
        /// Asynchronously retrieve all headers of the resource specified by the URI template.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <param name="headCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous HEAD method completes.
        /// </param>
        public void HeadForHeadersAsync(string url, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<HttpHeaders>> headCompleted)
        {
            this.ExecuteAsync<HttpHeaders>(url, HttpMethod.HEAD, null, this.headersExtractor, uriVariables, headCompleted);
        }

        /// <summary>
        /// Asynchronously retrieve all headers of the resource specified by the URI template.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="headCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous HEAD method completes.
        /// </param>
        public void HeadForHeadersAsync(Uri url, Action<MethodCompletedEventArgs<HttpHeaders>> headCompleted)
        {
            this.ExecuteAsync<HttpHeaders>(url, HttpMethod.HEAD, null, this.headersExtractor, headCompleted);
        }

        #endregion

        #region POST

        /// <summary>
        /// Asynchronously create a new resource by POSTing the given object to the URI template, 
        /// and returns the value of the 'Location' header. 
        /// This header typically indicates where the new resource is stored.
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="postCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous POST method completes.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void PostForLocationAsync(string url, object request, Action<Uri> postCompleted, params object[] uriVariables)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            this.ExecuteAsync<HttpHeaders>(url, HttpMethod.POST, requestCallback, this.headersExtractor, 
                delegate (MethodCompletedEventArgs<HttpHeaders> args) 
                {
                    postCompleted(args.Response.Location);
                }, 
                uriVariables);
        }

        /// <summary>
        /// Asynchronously create a new resource by POSTing the given object to the URI template, 
        /// and returns the value of the 'Location' header. 
        /// This header typically indicates where the new resource is stored.
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <param name="postCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous POST method completes.
        /// </param>
        public void PostForLocationAsync(string url, object request, IDictionary<string, object> uriVariables, Action<Uri> postCompleted)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            this.ExecuteAsync<HttpHeaders>(url, HttpMethod.POST, requestCallback, this.headersExtractor, uriVariables,
                delegate(MethodCompletedEventArgs<HttpHeaders> args) 
                {
                    postCompleted(args.Response.Location);
                });
        }

        /// <summary>
        /// Asynchronously create a new resource by POSTing the given object to the URI template, 
        /// and returns the value of the 'Location' header. 
        /// This header typically indicates where the new resource is stored.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="postCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous POST method completes.
        /// </param>
        public void PostForLocationAsync(Uri url, object request, Action<Uri> postCompleted)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            this.ExecuteAsync<HttpHeaders>(url, HttpMethod.POST, requestCallback, this.headersExtractor,
                delegate(MethodCompletedEventArgs<HttpHeaders> args) 
                {
                    postCompleted(args.Response.Location);
                });
        }

        /// <summary>
        /// Asynchronously create a new resource by POSTing the given object to the URI template, 
        /// and returns the representation found in the response. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="postCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous POST method completes.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void PostForObjectAsync<T>(string url, object request, Action<MethodCompletedEventArgs<T>> postCompleted, params object[] uriVariables) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            this.ExecuteAsync<T>(url, HttpMethod.POST, requestCallback, responseExtractor, postCompleted, uriVariables);
        }

        /// <summary>
        /// Asynchronously create a new resource by POSTing the given object to the URI template, 
        /// and returns the representation found in the response. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <param name="postCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous POST method completes.
        /// </param>
        public void PostForObjectAsync<T>(string url, object request, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<T>> postCompleted) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            this.ExecuteAsync<T>(url, HttpMethod.POST, requestCallback, responseExtractor, uriVariables, postCompleted);
        }

        /// <summary>
        /// Asynchronously create a new resource by POSTing the given object to the URI template, 
        /// and returns the representation found in the response. 
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="postCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous POST method completes.
        /// </param>
        public void PostForObjectAsync<T>(Uri url, object request, Action<MethodCompletedEventArgs<T>> postCompleted) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            this.ExecuteAsync<T>(url, HttpMethod.POST, requestCallback, responseExtractor, postCompleted);
        }

        /// <summary>
        /// Asynchronously create a new resource by POSTing the given object to the URI template, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="postCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous POST method completes.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void PostForMessageAsync<T>(string url, object request, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> postCompleted, params object[] uriVariables) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            this.ExecuteAsync<HttpResponseMessage<T>>(url, HttpMethod.POST, requestCallback, responseExtractor, postCompleted, uriVariables);
        }

        /// <summary>
        /// Asynchronously create a new resource by POSTing the given object to the URI template, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <param name="postCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous POST method completes.
        /// </param>
        public void PostForMessageAsync<T>(string url, object request, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> postCompleted) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            this.ExecuteAsync<HttpResponseMessage<T>>(url, HttpMethod.POST, requestCallback, responseExtractor, uriVariables, postCompleted);
        }

        /// <summary>
        /// Asynchronously create a new resource by POSTing the given object to the URI template, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>. 
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="postCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous POST method completes.
        /// </param>
        public void PostForMessageAsync<T>(Uri url, object request, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> postCompleted) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            this.ExecuteAsync<HttpResponseMessage<T>>(url, HttpMethod.POST, requestCallback, responseExtractor, postCompleted);
        }

        /// <summary>
        /// Asynchronously create a new resource by POSTing the given object to the URI template, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="postCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous POST method completes.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void PostForMessageAsync(string url, object request, Action<MethodCompletedEventArgs<HttpResponseMessage>> postCompleted, params object[] uriVariables)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            this.ExecuteAsync<HttpResponseMessage>(url, HttpMethod.POST, requestCallback, responseExtractor, postCompleted, uriVariables);
        }

        /// <summary>
        /// Asynchronously create a new resource by POSTing the given object to the URI template, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <param name="postCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous POST method completes.
        /// </param>
        public void PostForMessageAsync(string url, object request, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<HttpResponseMessage>> postCompleted)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            this.ExecuteAsync<HttpResponseMessage>(url, HttpMethod.POST, requestCallback, responseExtractor, uriVariables, postCompleted);
        }

        /// <summary>
        /// Asynchronously create a new resource by POSTing the given object to the URI template, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>. 
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="postCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous POST method completes.
        /// </param>
        public void PostForMessageAsync(Uri url, object request, Action<MethodCompletedEventArgs<HttpResponseMessage>> postCompleted)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            this.ExecuteAsync<HttpResponseMessage>(url, HttpMethod.POST, requestCallback, responseExtractor, postCompleted);
        }

        #endregion

        #region PUT

        /// <summary>
        /// Asynchronously create or update a resource by PUTting the given object to the URI.
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="putCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous PUT method completes.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void PutAsync(string url, object request, Action<MethodCompletedEventArgs<object>> putCompleted, params object[] uriVariables)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            this.ExecuteAsync<object>(url, HttpMethod.PUT, requestCallback, null, putCompleted, uriVariables);
        }

        /// <summary>
        /// Asynchronously create or update a resource by PUTting the given object to the URI.
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <param name="putCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous PUT method completes.
        /// </param>
        public void PutAsync(string url, object request, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<object>> putCompleted)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            this.ExecuteAsync<object>(url, HttpMethod.PUT, requestCallback, null, uriVariables, putCompleted);
        }

        /// <summary>
        /// Asynchronously create or update a resource by PUTting the given object to the URI.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="request">
        /// The object to be POSTed, may be a <see cref="HttpEntity"/> in order to add additional HTTP headers.
        /// </param>
        /// <param name="putCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous PUT method completes.
        /// </param>
        public void PutAsync(Uri url, object request, Action<MethodCompletedEventArgs<object>> putCompleted)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(request, this._messageConverters);
            this.ExecuteAsync<object>(url, HttpMethod.PUT, requestCallback, null, putCompleted);
        }

        #endregion

        #region DELETE

        /// <summary>
        /// Asynchronously delete the resources at the specified URI.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="deleteCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous PUT method completes.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void DeleteAsync(string url, Action<MethodCompletedEventArgs<object>> deleteCompleted, params object[] uriVariables)
        {
            this.ExecuteAsync<object>(url, HttpMethod.DELETE, null, null, deleteCompleted, uriVariables);
        }

        /// <summary>
        /// Asynchronously delete the resources at the specified URI.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <param name="deleteCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous PUT method completes.
        /// </param>
        public void DeleteAsync(string url, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<object>> deleteCompleted)
        {
            this.ExecuteAsync<object>(url, HttpMethod.DELETE, null, null, uriVariables, deleteCompleted);
        }

        /// <summary>
        /// Asynchronously delete the resources at the specified URI.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="deleteCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous PUT method completes.
        /// </param>
        public void DeleteAsync(Uri url, Action<MethodCompletedEventArgs<object>> deleteCompleted)
        {
            this.ExecuteAsync<object>(url, HttpMethod.DELETE, null, null, deleteCompleted);
        }

        #endregion

        #region OPTIONS

        /// <summary>
        /// Asynchronously return the value of the Allow header for the given URI.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="optionsCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous OPTIONS method completes.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void OptionsForAllowAsync(string url, Action<IList<HttpMethod>> optionsCompleted, params object[] uriVariables)
        {
            this.ExecuteAsync<HttpHeaders>(url, HttpMethod.OPTIONS, null, this.headersExtractor, 
                delegate(MethodCompletedEventArgs<HttpHeaders> args) 
                {
                    optionsCompleted(args.Response.Allow);
                },
                uriVariables);
        }

        /// <summary>
        /// Asynchronously return the value of the Allow header for the given URI.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <param name="optionsCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous OPTIONS method completes.
        /// </param>
        public void OptionsForAllowAsync(string url, IDictionary<string, object> uriVariables, Action<IList<HttpMethod>> optionsCompleted)
        {
            this.ExecuteAsync<HttpHeaders>(url, HttpMethod.OPTIONS, null, this.headersExtractor, uriVariables,
                delegate(MethodCompletedEventArgs<HttpHeaders> args) 
                {
                    optionsCompleted(args.Response.Allow);
                });
        }

        /// <summary>
        /// Asynchronously return the value of the Allow header for the given URI.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="optionsCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous OPTIONS method completes.
        /// </param>
        public void OptionsForAllowAsync(Uri url, Action<IList<HttpMethod>> optionsCompleted)
        {
            this.ExecuteAsync<HttpHeaders>(url, HttpMethod.OPTIONS, null, this.headersExtractor,
                delegate(MethodCompletedEventArgs<HttpHeaders> args) 
                {
                    optionsCompleted(args.Response.Allow);
                });
        }

        #endregion


        #region Exchange

        /// <summary>
        /// Asynchronously execute the HTTP method to the given URI template, writing the given request message to the request, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestEntity">
        /// The HTTP entity (headers and/or body) to write to the request, may be <see langword="null"/>.
        /// </param>
        /// <param name="methodCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous method completes.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void ExchangeAsync<T>(string url, HttpMethod method, HttpEntity requestEntity, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> methodCompleted, params object[] uriVariables) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(requestEntity, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            this.ExecuteAsync<HttpResponseMessage<T>>(url, method, requestCallback, responseExtractor, methodCompleted, uriVariables);
        }

        /// <summary>
        /// Asynchronously execute the HTTP method to the given URI template, writing the given request message to the request, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestEntity">
        /// The HTTP entity (headers and/or body) to write to the request, may be <see langword="null"/>.
        /// </param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <param name="methodCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous method completes.
        /// </param>
        public void ExchangeAsync<T>(string url, HttpMethod method, HttpEntity requestEntity, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> methodCompleted) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(requestEntity, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            this.ExecuteAsync<HttpResponseMessage<T>>(url, method, requestCallback, responseExtractor, uriVariables, methodCompleted);
        }

        /// <summary>
        /// Asynchronously execute the HTTP method to the given URI template, writing the given request message to the request, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestEntity">
        /// The HTTP entity (headers and/or body) to write to the request, may be <see langword="null"/>.
        /// </param>
        /// <param name="methodCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous method completes.
        /// </param>
        public void ExchangeAsync<T>(Uri url, HttpMethod method, HttpEntity requestEntity, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> methodCompleted) where T : class
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(requestEntity, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            this.ExecuteAsync<HttpResponseMessage<T>>(url, method, requestCallback, responseExtractor, methodCompleted);
        }

        /// <summary>
        /// Asynchronously execute the HTTP method to the given URI template, writing the given request message to the request, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestEntity">
        /// The HTTP entity (headers and/or body) to write to the request, may be <see langword="null"/>.
        /// </param>
        /// <param name="methodCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous method completes.
        /// </param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void ExchangeAsync(string url, HttpMethod method, HttpEntity requestEntity, Action<MethodCompletedEventArgs<HttpResponseMessage>> methodCompleted, params object[] uriVariables)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(requestEntity, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            this.ExecuteAsync<HttpResponseMessage>(url, method, requestCallback, responseExtractor, methodCompleted, uriVariables);
        }

        /// <summary>
        /// Asynchronously execute the HTTP method to the given URI template, writing the given request message to the request, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestEntity">
        /// The HTTP entity (headers and/or body) to write to the request, may be <see langword="null"/>.
        /// </param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <param name="methodCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous method completes.
        /// </param>
        public void ExchangeAsync(string url, HttpMethod method, HttpEntity requestEntity, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<HttpResponseMessage>> methodCompleted)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(requestEntity, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            this.ExecuteAsync<HttpResponseMessage>(url, method, requestCallback, responseExtractor, uriVariables, methodCompleted);
        }

        /// <summary>
        /// Asynchronously execute the HTTP method to the given URI template, writing the given request message to the request, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestEntity">
        /// The HTTP entity (headers and/or body) to write to the request, may be <see langword="null"/>.
        /// </param>
        /// <param name="methodCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous method completes.
        /// </param>
        public void ExchangeAsync(Uri url, HttpMethod method, HttpEntity requestEntity, Action<MethodCompletedEventArgs<HttpResponseMessage>> methodCompleted)
        {
            HttpEntityRequestCallback requestCallback = new HttpEntityRequestCallback(requestEntity, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            this.ExecuteAsync<HttpResponseMessage>(url, method, requestCallback, responseExtractor, methodCompleted);
        }

        #endregion

        #region General execution

        /// <summary>
        /// Asynchronously execute the HTTP method to the given URI template, preparing the request with the 
        /// <see cref="IRequestCallback"/>, and reading the response with an <see cref="IResponseExtractor{T}"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestCallback">Object that prepares the request.</param>
        /// <param name="responseExtractor">Object that extracts the return value from the response.</param>
        /// <param name="methodCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous method completes.
        /// </param>   
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void ExecuteAsync<T>(string url, HttpMethod method, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor, Action<MethodCompletedEventArgs<T>> methodCompleted, params object[] uriVariables) where T : class
        {
            this.DoExecuteAsync<T>(BuildUri(this._baseAddress, url, uriVariables), method, 
                requestCallback, responseExtractor, methodCompleted);
        }

        /// <summary>
        /// Asynchronously execute the HTTP method to the given URI template, preparing the request with the 
        /// <see cref="IRequestCallback"/>, and reading the response with an <see cref="IResponseExtractor{T}"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestCallback">Object that prepares the request.</param>
        /// <param name="responseExtractor">Object that extracts the return value from the response.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <param name="methodCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous method completes.
        /// </param>  
        public void ExecuteAsync<T>(string url, HttpMethod method, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<T>> methodCompleted) where T : class
        {
            this.DoExecuteAsync<T>(BuildUri(this._baseAddress, url, uriVariables), method, 
                requestCallback, responseExtractor, methodCompleted);
        }

        /// <summary>
        /// Asynchronously execute the HTTP method to the given URI template, preparing the request with the 
        /// <see cref="IRequestCallback"/>, and reading the response with an <see cref="IResponseExtractor{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestCallback">Object that prepares the request.</param>
        /// <param name="responseExtractor">Object that extracts the return value from the response.</param>
        /// <param name="methodCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous method completes.
        /// </param>  
        public void ExecuteAsync<T>(Uri url, HttpMethod method, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor, Action<MethodCompletedEventArgs<T>> methodCompleted) where T : class
        {
            this.DoExecuteAsync<T>(BuildUri(this._baseAddress, url), method, 
                requestCallback, responseExtractor, methodCompleted);
        }

        #endregion

        #endregion

        #region DoExecute

#if !SILVERLIGHT
        /// <summary>
        /// Execute the HTTP request to the given URI, preparing the request with the 
        /// <see cref="IRequestCallback"/>, and reading the response with an <see cref="IResponseExtractor{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="uri">The fully-expanded URI to connect to.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestCallback">Object that prepares the request.</param>
        /// <param name="responseExtractor">Object that extracts the return value from the response.</param>
        /// <returns>An arbitrary object, as returned by the <see cref="IResponseExtractor{T}"/>.</returns>  
        protected virtual T DoExecute<T>(Uri uri, HttpMethod method, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor) where T : class
        {
            IClientHttpRequest request = this._requestFactory.CreateRequest(uri, method);

            if (requestCallback != null)
            {
                requestCallback.DoWithRequest(request);
            }

            using (IClientHttpResponse response = request.Execute())
            {
                if (response != null)
                {
                    if (this._errorHandler.HasError(response))
                    {
                        HandleResponseError(uri, method, response, this._errorHandler);
                    }
                    else
                    {
                        LogResponseStatus(uri, method, response);
                    }

                    if (responseExtractor != null)
                    {
                        return responseExtractor.ExtractData(response);
                    }
                }
            }

            return null;
        }
#endif

        #endregion

        #region DoExecuteAsync

        /// <summary>
        /// Asynchronously execute the HTTP request to the given URI, preparing the request with the 
        /// <see cref="IRequestCallback"/>, and reading the response with an <see cref="IResponseExtractor{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="uri">The fully-expanded URI to connect to.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestCallback">Object that prepares the request.</param>
        /// <param name="responseExtractor">Object that extracts the return value from the response.</param>
        /// <param name="methodCompleted">
        /// The <code>Action&lt;T&gt;</code> to perform when the asynchronous method completes.
        /// </param>  
        protected virtual void DoExecuteAsync<T>(Uri uri, HttpMethod method,
            IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor,
            Action<MethodCompletedEventArgs<T>> methodCompleted) where T : class
        {
            IClientHttpRequest request = this._requestFactory.CreateRequest(uri, method);

            ExecuteState<T> state = new ExecuteState<T>(uri, method, responseExtractor, this._errorHandler, methodCompleted);

            if (requestCallback != null)
            {
                requestCallback.DoWithRequest(request);
            }

            if (methodCompleted == null)
            {
                request.ExecuteAsync(null, null);
            }
            else
            {
                request.ExecuteAsync(state, ResponseReceivedCallback<T>);
            }
        }

        private static void ResponseReceivedCallback<T>(ExecuteCompletedEventArgs responseReceived) where T : class
        {
            ExecuteState<T> state = (ExecuteState<T>)responseReceived.UserState;
            if (responseReceived.Error == null)
            {
                using (IClientHttpResponse response = responseReceived.Response)
                {
                    if (response == null)
                    {
                        state.MethodCompleted(new MethodCompletedEventArgs<T>(null, null, false, null));
                    }
                    else
                    {
                        T value = null;
                        Exception exception = null;
                        try
                        {
                            if (state.ResponseErrorHandler.HasError(response))
                            {
                                HandleResponseError(state.Uri, state.Method, response, state.ResponseErrorHandler);
                            }
                            else
                            {
                                LogResponseStatus(state.Uri, state.Method, response);
                            }

                            if (state.ResponseExtractor != null)
                            {
                                value = state.ResponseExtractor.ExtractData(response);
                            }
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                        }
                        finally
                        {
                            state.MethodCompleted(new MethodCompletedEventArgs<T>(value, exception, false, null));
                        }
                    }
                }
            }
            else
            {
                state.MethodCompleted(new MethodCompletedEventArgs<T>(null, responseReceived.Error, false, null));
            }
        }

        private sealed class ExecuteState<T> where T : class
        {
            public Uri Uri;
            public HttpMethod Method;
            public IResponseExtractor<T> ResponseExtractor;
            public IResponseErrorHandler ResponseErrorHandler;
            public Action<MethodCompletedEventArgs<T>> MethodCompleted;

            public ExecuteState(Uri uri, HttpMethod method, 
                IResponseExtractor<T> responseExtractor, 
                IResponseErrorHandler responseErrorHandler, 
                Action<MethodCompletedEventArgs<T>> methodCompleted)
            {
                this.Uri = uri;
                this.Method = method;
                this.ResponseExtractor = responseExtractor;
                this.ResponseErrorHandler = responseErrorHandler;
                this.MethodCompleted = methodCompleted;
            }
        }

        #endregion

        #region BuildUri

        // TODO : Add IRequestUriBuilder interface to support .NET 3.5 UriTemplate class ?
        // Problems with silverlight implementation :
        // - UriTemplate.BindByPosition is not supported in Silverlight
        // - UriTemplate class is not included in the Silverlight core dlls

        /// <summary>
        /// Builds an uri using the given parameters.
        /// </summary>
        /// <param name="baseAddress">The base address to use, may be <see langword="null"/>.</param>
        /// <param name="url">An absolute or relative URI template to expand.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The absolute build URI.</returns>
        /// <exception cref="ArgumentException">If an absolute URI can't be build.</exception>
        protected virtual Uri BuildUri(Uri baseAddress, string url, params object[] uriVariables)
        {
            UriTemplate uriTemplate = new UriTemplate(url);
            Uri uri = uriTemplate.Expand(uriVariables);
            return BuildUri(baseAddress, uri);
        }

        /// <summary>
        /// Builds an uri using the given parameters.
        /// </summary>
        /// <param name="baseAddress">The base address to use, may be <see langword="null"/>.</param>
        /// <param name="url">An absolute or relative URI template to expand.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The absolute build URI.</returns>
        /// <exception cref="ArgumentException">If an absolute URI can't be build.</exception>
        protected virtual Uri BuildUri(Uri baseAddress, string url, IDictionary<string, object> uriVariables)
        {
            UriTemplate uriTemplate = new UriTemplate(url);
            Uri uri = uriTemplate.Expand(uriVariables);
            return BuildUri(baseAddress, uri);
        }

        /// <summary>
        /// Builds an uri using the given parameters.
        /// </summary>
        /// <param name="baseAddress">The base address to use, may be <see langword="null"/>.</param>
        /// <param name="uri">An absolute or relative URI.</param>
        /// <returns>The absolute build URI.</returns>
        /// <exception cref="ArgumentException">If an absolute URI can't be build.</exception>
        protected virtual Uri BuildUri(Uri baseAddress, Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                if (baseAddress != null)
                {
                    return new Uri(baseAddress, uri);
                }
                else
                {
                    throw new ArgumentException(String.Format("'{0}' is not an absolute URI", uri), "uri");
                }
            }
            return uri;
        }

        #endregion

        private static void LogResponseStatus(Uri uri, HttpMethod method, IClientHttpResponse response) 
        {
            #region Instrumentation
#if !SILVERLIGHT
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug(String.Format(
                    "{0} request for '{1}' resulted in {2:d} - {2} ({3})",
                    method, uri, response.StatusCode, response.StatusDescription));
            }
#endif
            #endregion
        }

        private static void HandleResponseError(Uri uri, HttpMethod method, IClientHttpResponse response, 
            IResponseErrorHandler errorHandler) 
        {
            #region Instrumentation
#if !SILVERLIGHT
            if (LOG.IsWarnEnabled)
            {
                LOG.Warn(String.Format(
                    "{0} request for '{1}' resulted in {2:d} - {2} ({3}); invoking error handler",
                    method, uri, response.StatusCode, response.StatusDescription));
            }
#endif
            #endregion

            errorHandler.HandleError(response);
        }
    }
}
