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

using Spring.Http.Converters;
using Spring.Http.Converters.Xml;
#if NET_3_5
using Spring.Http.Converters.Feed;
#endif
using Spring.Http.Rest.Support;
using UriTemplate = Spring.Util.UriTemplate; // UriTemplate in .NET Framework since 3.5
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
    ///     <tr><td>PUT</td><td><see cref="M:Put"/></td></tr>
    ///     <tr><td>any</td><td><see cref="M:Exchange"/></td></tr>
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
    /// This template uses a <see cref="T:DefaultHttpWebRequestFactory"/> as default strategy for creating HTTP connections. 
    /// </para>
    /// </remarks>
    /// <see cref="IHttpMessageConverter"/>
    /// <see cref="IRequestCallback"/>
    /// <see cref="IResponseExtractor{T}"/>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public class RestTemplate : IRestOperations
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(RestTemplate));

        #endregion

        #region Fields / Properties

        private Uri _baseAddress;
        private bool _throwExceptionOnError;
        private IList<IHttpMessageConverter> _messageConverters;
        private IHttpWebRequestFactory _requestFactory;

        private IResponseExtractor<WebHeaderCollection> headersExtractor;


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
        /// Gets or sets the request factory that this class uses for obtaining for <see cref="HttpWebRequest"/> objects.
        /// </summary>
        public IHttpWebRequestFactory RequestFactory
        {
            get { return this._requestFactory; }
            set { this._requestFactory = value; }
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
            this._requestFactory = new DefaultHttpWebRequestFactory();

            this._messageConverters = new List<IHttpMessageConverter>();

            this._messageConverters.Add(new ByteArrayHttpMessageConverter());
            this._messageConverters.Add(new StringHttpMessageConverter());
            this._messageConverters.Add(new UrlEncodedFormHttpMessageConverter());
            this._messageConverters.Add(new XmlDocumentHttpMessageConverter());
#if NET_3_0
            this._messageConverters.Add(new DataContractHttpMessageConverter());
#else // NET_2_0 only
            this._messageConverters.Add(new XmlSerializableHttpMessageConverter());
#endif
#if NET_3_5
            this._messageConverters.Add(new XElementHttpMessageConverter());
            this._messageConverters.Add(new Rss20FeedHttpMessageConverter());
            this._messageConverters.Add(new Atom10FeedHttpMessageConverter());
            //this._messageConverters.Add(new JsonHttpMessageConverter());            
#endif
        }

        #endregion

        #region IRestOperations Membres

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
        public T GetForObject<T>(string url, params string[] uriVariables) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(HttpMethod.GET, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, requestCallback, responseExtractor, uriVariables);
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
        public T GetForObject<T>(string url, IDictionary<string, string> uriVariables) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(HttpMethod.GET, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, requestCallback, responseExtractor, uriVariables);
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
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(HttpMethod.GET, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, requestCallback, responseExtractor);
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
        public HttpResponseMessage<T> GetForMessage<T>(string url, params string[] uriVariables) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(HttpMethod.GET, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor, uriVariables);
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
        public HttpResponseMessage<T> GetForMessage<T>(string url, IDictionary<string, string> uriVariables) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(HttpMethod.GET, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor, uriVariables);
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
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(HttpMethod.GET, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor);
        }

        /// <summary>
        /// Retrieve all headers of the resource specified by the URI template.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>All HTTP headers of that resource</returns>
        public WebHeaderCollection HeadForHeaders(string url, params string[] uriVariables)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.HEAD);
            return this.Execute<WebHeaderCollection>(url, requestCallback, this.headersExtractor, uriVariables);
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
        public WebHeaderCollection HeadForHeaders(string url, IDictionary<string, string> uriVariables)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.HEAD);
            return this.Execute<WebHeaderCollection>(url, requestCallback, this.headersExtractor, uriVariables);
        }

        /// <summary>
        /// Retrieve all headers of the resource specified by the URI template.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>All HTTP headers of that resource</returns>
        public WebHeaderCollection HeadForHeaders(Uri url)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.HEAD);
            return this.Execute<WebHeaderCollection>(url, requestCallback, this.headersExtractor);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the value of the 'Location' header. 
        /// This header typically indicates where the new resource is stored.
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpRequestMessage"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The value for the Location header.</returns>
        public Uri PostForLocation(string url, object request, params string[] uriVariables)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, this._messageConverters);
            WebHeaderCollection headers = this.Execute<WebHeaderCollection>(
                url, requestCallback, this.headersExtractor, uriVariables);
            string location = headers[HttpResponseHeader.Location];
            return StringUtils.HasText(location) ? new Uri(location) : null;
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
        /// <para>
        /// The request parameter can be a <see cref="HttpRequestMessage"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The value for the Location header.</returns>
        public Uri PostForLocation(string url, object request, IDictionary<string, string> uriVariables)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, this._messageConverters);
            WebHeaderCollection headers = this.Execute<WebHeaderCollection>(
                url, requestCallback, this.headersExtractor, uriVariables);
            string location = headers[HttpResponseHeader.Location];
            return StringUtils.HasText(location) ? new Uri(location) : null;
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the value of the 'Location' header. 
        /// This header typically indicates where the new resource is stored.
        /// </summary>
        /// <remarks>
        /// The request parameter can be a <see cref="HttpRequestMessage"/> in order to add additional HTTP headers to the request.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <returns>The value for the Location header.</returns>
        public Uri PostForLocation(Uri url, object request)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, this._messageConverters);
            WebHeaderCollection headers = this.Execute<WebHeaderCollection>(
                url, requestCallback, this.headersExtractor);
            string location = headers[HttpResponseHeader.Location];
            return StringUtils.HasText(location) ? new Uri(location) : null;
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the representation found in the response. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpRequestMessage"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The converted object.</returns>
        public T PostForObject<T>(string url, object request, params string[] uriVariables) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the representation found in the response. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpRequestMessage"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The converted object.</returns>
        public T PostForObject<T>(string url, object request, IDictionary<string, string> uriVariables) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the representation found in the response. 
        /// </summary>
        /// <remarks>
        /// The request parameter can be a <see cref="HttpRequestMessage"/> in order to add additional HTTP headers to the request.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <returns>The converted object.</returns>
        public T PostForObject<T>(Uri url, object request) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, requestCallback, responseExtractor);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpRequestMessage"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The HTTP response message.</returns>
        public HttpResponseMessage<T> PostForMessage<T>(string url, object request, params string[] uriVariables) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpRequestMessage"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The HTTP response message.</returns>
        public HttpResponseMessage<T> PostForMessage<T>(string url, object request, IDictionary<string, string> uriVariables) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>. 
        /// </summary>
        /// <remarks>
        /// The request parameter can be a <see cref="HttpRequestMessage"/> in order to add additional HTTP headers to the request.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <returns>The HTTP response message.</returns>
        public HttpResponseMessage<T> PostForMessage<T>(Uri url, object request) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpRequestMessage"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The HTTP response message with no entity.</returns>
        public HttpResponseMessage PostForMessage(string url, object request, params string[] uriVariables)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            return this.Execute<HttpResponseMessage>(url, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpRequestMessage"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The HTTP response message with no entity.</returns>
        public HttpResponseMessage PostForMessage(string url, object request, IDictionary<string, string> uriVariables)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            return this.Execute<HttpResponseMessage>(url, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>. 
        /// </summary>
        /// <remarks>
        /// The request parameter can be a <see cref="HttpRequestMessage"/> in order to add additional HTTP headers to the request.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <returns>The HTTP response message with no entity.</returns>
        public HttpResponseMessage PostForMessage(Uri url, object request)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            return this.Execute<HttpResponseMessage>(url, requestCallback, responseExtractor);
        }

        /// <summary>
        /// Create or update a resource by PUTting the given object to the URI.
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpRequestMessage"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be PUT, may be null.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void Put(string url, object request, params string[] uriVariables)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.PUT, request, this._messageConverters);
            this.Execute<object>(url, requestCallback, null, uriVariables);
        }

        /// <summary>
        /// Create or update a resource by PUTting the given object to the URI.
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpRequestMessage"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be PUT, may be null.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        public void Put(string url, object request, IDictionary<string, string> uriVariables)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.PUT, request, this._messageConverters);
            this.Execute<object>(url, requestCallback, null, uriVariables);
        }

        /// <summary>
        /// Create or update a resource by PUTting the given object to the URI.
        /// </summary>
        /// <remarks>
        /// The request parameter can be a <see cref="HttpRequestMessage"/> in order to add additional HTTP headers to the request.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be PUT, may be null.</param>
        public void Put(Uri url, object request)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.PUT, request, this._messageConverters);
            this.Execute<object>(url, requestCallback, null);
        }

        /// <summary>
        /// Delete the resources at the specified URI.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        public void Delete(string url, params string[] uriVariables)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.DELETE);
            this.Execute<object>(url, requestCallback, null, uriVariables);
        }

        /// <summary>
        /// Delete the resources at the specified URI.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        public void Delete(string url, IDictionary<string, string> uriVariables)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.DELETE);
            this.Execute<object>(url, requestCallback, null, uriVariables);
        }

        /// <summary>
        /// Delete the resources at the specified URI.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void Delete(Uri url)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.DELETE);
            this.Execute<object>(url, requestCallback, null);
        }

        /// <summary>
        /// Return the value of the Allow header for the given URI.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The value of the allow header.</returns>
        public IList<HttpMethod> OptionsForAllow(string url, params string[] uriVariables)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.OPTIONS);
            WebHeaderCollection headers = this.Execute<WebHeaderCollection>(
                url, requestCallback, this.headersExtractor, uriVariables);
            string allow = headers[HttpResponseHeader.Allow];

            return ParseAllowHeader(allow);
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
        public IList<HttpMethod> OptionsForAllow(string url, IDictionary<string, string> uriVariables)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.OPTIONS);
            WebHeaderCollection headers = this.Execute<WebHeaderCollection>(url, requestCallback, this.headersExtractor, uriVariables);
            string allow = headers[HttpResponseHeader.Allow];

            return ParseAllowHeader(allow);
        }

        /// <summary>
        /// Return the value of the Allow header for the given URI.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The value of the allow header.</returns>
        public IList<HttpMethod> OptionsForAllow(Uri url)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.OPTIONS);
            WebHeaderCollection headers = this.Execute<WebHeaderCollection>(url, requestCallback, this.headersExtractor);
            string allow = headers[HttpResponseHeader.Allow];

            return ParseAllowHeader(allow);
        }

        /// <summary>
        /// Execute the HTTP request to the given URI template, writing the given request message to the request, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="requestMessage">The HTTP request message to write to the request.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The HTTP response message.</returns>
        public HttpResponseMessage<T> Exchange<T>(string url, HttpRequestMessage requestMessage, params string[] uriVariables) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(requestMessage, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Execute the HTTP request to the given URI template, writing the given request message to the request, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="requestMessage">The HTTP request message to write to the request.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The HTTP response message.</returns>
        public HttpResponseMessage<T> Exchange<T>(string url, HttpRequestMessage requestMessage, IDictionary<string, string> uriVariables) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(requestMessage, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Execute the HTTP request to the given URI template, writing the given request message to the request, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="requestMessage">The HTTP request message to write to the request.</param>
        /// <returns>The HTTP response message.</returns>
        public HttpResponseMessage<T> Exchange<T>(Uri url, HttpRequestMessage requestMessage) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(requestMessage, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor);
        }

        /// <summary>
        /// Execute the HTTP request to the given URI template, writing the given request message to the request, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="requestMessage">The HTTP request message to write to the request.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The HTTP response message with no entity.</returns>
        public HttpResponseMessage Exchange(string url, HttpRequestMessage requestMessage, params string[] uriVariables)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(requestMessage, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            return this.Execute<HttpResponseMessage>(url, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Execute the HTTP request to the given URI template, writing the given request message to the request, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="requestMessage">The HTTP request message to write to the request.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The HTTP response message with no entity.</returns>
        public HttpResponseMessage Exchange(string url, HttpRequestMessage requestMessage, IDictionary<string, string> uriVariables)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(requestMessage, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            return this.Execute<HttpResponseMessage>(url, requestCallback, responseExtractor, uriVariables);
        }

        /// <summary>
        /// Execute the HTTP request to the given URI template, writing the given request message to the request, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="requestMessage">The HTTP request message to write to the request.</param>
        /// <returns>The HTTP response message with no entity.</returns>
        public HttpResponseMessage Exchange(Uri url, HttpRequestMessage requestMessage)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(requestMessage, this._messageConverters);
            HttpMessageResponseExtractor responseExtractor = new HttpMessageResponseExtractor();
            return this.Execute<HttpResponseMessage>(url, requestCallback, responseExtractor);
        }

        /// <summary>
        /// Execute the HTTP request to the given URI template, preparing the request with the 
        /// <see cref="IRequestCallback"/>, and reading the response with an <see cref="IResponseExtractor{T}"/>.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="requestCallback">Object that prepares the request.</param>
        /// <param name="responseExtractor">Object that extracts the return value from the response.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>An arbitrary object, as returned by the <see cref="IResponseExtractor{T}"/>.</returns>        
        public T Execute<T>(string url, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor, params string[] uriVariables) where T : class
        {
            UriTemplate uriTemplate = new UriTemplate(url);
            Uri uri = uriTemplate.Expand(uriVariables);
            return this.DoExecute<T>(uri, requestCallback, responseExtractor);
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
        /// <param name="requestCallback">Object that prepares the request.</param>
        /// <param name="responseExtractor">Object that extracts the return value from the response.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>An arbitrary object, as returned by the <see cref="IResponseExtractor{T}"/>.</returns>   
        public T Execute<T>(string url, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor, IDictionary<string, string> uriVariables) where T : class
        {
            UriTemplate uriTemplate = new UriTemplate(url);
            Uri uri = uriTemplate.Expand(uriVariables);
            return this.DoExecute<T>(uri, requestCallback, responseExtractor);
        }

        /// <summary>
        /// Execute the HTTP request to the given URI template, preparing the request with the 
        /// <see cref="IRequestCallback"/>, and reading the response with an <see cref="IResponseExtractor{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="requestCallback">Object that prepares the request.</param>
        /// <param name="responseExtractor">Object that extracts the return value from the response.</param>
        /// <returns>An arbitrary object, as returned by the <see cref="IResponseExtractor{T}"/>.</returns>   
        public T Execute<T>(Uri url, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor) where T : class
        {
            return this.DoExecute<T>(url, requestCallback, responseExtractor);
        }

        #endregion

        /// <summary>
        /// Execute the HTTP request to the given URI, preparing the request with the 
        /// <see cref="IRequestCallback"/>, and reading the response with an <see cref="IResponseExtractor{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="uri">The fully-expanded URI to connect to.</param>
        /// <param name="requestCallback">Object that prepares the request.</param>
        /// <param name="responseExtractor">Object that extracts the return value from the response.</param>
        /// <returns>An arbitrary object, as returned by the <see cref="IResponseExtractor{T}"/>.</returns>  
        protected virtual T DoExecute<T>(Uri uri, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor) where T : class
        {
            HttpWebRequest request;
            HttpWebResponse response = null;

            Uri finalUri = uri;
            if (!uri.IsAbsoluteUri)
            {
                if (this._baseAddress != null)
                {
                    finalUri = new Uri(this._baseAddress, uri);
                }
                else
                {
                    throw new ArgumentException(String.Format("'{0}' is not an absolute URI", uri), "uri");
                }
            }

            // Create and initialize the web request  
            request = this._requestFactory.CreateRequest(finalUri);

            if (requestCallback != null)
            {
                requestCallback.DoWithRequest(request);
            }

            try
            {
                // Get response  
                response = request.GetResponse() as HttpWebResponse;

                if (request.HaveResponse == true && response != null)
                {
                    #region Instrumentation

                    if (LOG.IsDebugEnabled)
                    {
                        LOG.Debug(String.Format(
                            "Request for '{0}' resulted in {1:d} - {1} ({2})", 
                            finalUri, response.StatusCode, response.StatusDescription));
                    }

                    #endregion

                    if (responseExtractor != null)
                    {
                        return responseExtractor.ExtractData(response);
                    }
                }
            }
            catch (WebException ex)
            {
                // This exception will be raised if the server didn't return 200 - OK  
                // Try to retrieve more information about the network error  
                if (ex.Response != null)
                {
                    using (HttpWebResponse errorResponse = (HttpWebResponse)ex.Response)
                    {
                        if (this._throwExceptionOnError)
                        {
                            throw new RestClientException(String.Format(
                                "The server returned '{0}' with the status code {1:d} - {1}.",
                                errorResponse.StatusDescription, errorResponse.StatusCode),
                                ex);
                        }
                        else
                        {
                            if (responseExtractor != null)
                            {
                                return responseExtractor.ExtractData(errorResponse);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (response != null) 
                { 
                    response.Close(); 
                }
            }

            return null;
	    }

        private static IList<HttpMethod> ParseAllowHeader(string allow)
        {
            IList<HttpMethod> methods = new List<HttpMethod>();

            if (StringUtils.HasText(allow))
            {
                string[] methodsArray = allow.Split(',');

                foreach (string method in methodsArray)
                {
                    methods.Add((HttpMethod)Enum.Parse(typeof(HttpMethod), method.Trim(), true));
                }
            }

            return methods;
        }
    }
}
