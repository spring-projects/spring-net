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
using System.IO;
using System.Net;
using System.Collections.Generic;

using Spring.Util;
using Spring.Http;
using Spring.Http.Converters;
using Spring.Http.Converters.Xml;
using Spring.Http.Converters.Json;
using Spring.Http.Converters.Feed;
using Spring.Http.Rest.Support;
using UriTemplate = Spring.Util.UriTemplate; // UriTemplate in .NET Framework since 3.5

namespace Spring.Http.Rest
{
    /**
     * <strong>The central class for client-side HTTP access.</strong> It simplifies communication with HTTP servers, and
     * enforces RESTful principles. It handles HTTP connections, leaving application code to provide URLs (with possible
     * template variables) and extract results.
     *
     * <p>The main entry points of this template are the methods named after the six main HTTP methods:
     * <table>
     * <tr><th>HTTP method</th><th>RestTemplate methods</th></tr>
     * <tr><td>DELETE</td><td>{@link #delete}</td></tr>
     * <tr><td>GET</td><td>{@link #getForObject}</td></tr>
     * <tr><td></td><td>{@link #getForEntity}</td></tr>
     * <tr><td>HEAD</td><td>{@link #headForHeaders}</td></tr>
     * <tr><td>OPTIONS</td><td>{@link #optionsForAllow}</td></tr>
     * <tr><td>POST</td><td>{@link #postForLocation}</td></tr>
     * <tr><td></td><td>{@link #postForObject}</td></tr>
     * <tr><td>PUT</td><td>{@link #put}</td></tr>
     * <tr><td>any</td><td>{@link #exchange}</td></tr>
     * <tr><td></td><td>{@link #execute}</td></tr> </table>
     *
     * <p>For each of these HTTP methods, there are three corresponding Java methods in the {@code RestTemplate}. Two
     * variant take a {@code String} URI as first argument (eg. {@link #getForObject(String, Class, Object[])}, {@link
     * #getForObject(String, Class, Map)}), and are capable of substituting any {@linkplain UriTemplate URI templates} in
     * that URL using either a {@code String} variable arguments array, or a {@code Map<String, String>}. The string varargs
     * variant expands the given template variables in order, so that
     * <pre>
     * String result = restTemplate.getForObject("http://example.com/hotels/{hotel}/bookings/{booking}", String.class,"42",
     * "21");
     * </pre>
     * will perform a GET on {@code http://example.com/hotels/42/bookings/21}. The map variant expands the template based on
     * variable name, and is therefore more useful when using many variables, or when a single variable is used multiple
     * times. For example:
     * <pre>
     * Map&lt;String, String&gt; vars = Collections.singletonMap("hotel", "42");
     * String result = restTemplate.getForObject("http://example.com/hotels/{hotel}/rooms/{hotel}", String.class, vars);
     * </pre>
     * will perform a GET on {@code http://example.com/hotels/42/rooms/42}. Alternatively, there are {@link URI} variant
     * methods ({@link #getForObject(URI, Class)}), which do not allow for URI templates, but allow you to reuse a single,
     * expanded URI multiple times.
     *
     * <p>Furthermore, the {@code String}-argument methods assume that the URL String is unencoded. This means that
     * <pre>
     * restTemplate.getForObject("http://example.com/hotel list");
     * </pre>
     * will perform a GET on {@code http://example.com/hotel%20list}. As a result, any URL passed that is already encoded
     * will be encoded twice (i.e. {@code http://example.com/hotel%20list} will become {@code
     * http://example.com/hotel%2520list}). If this behavior is undesirable, use the {@code URI}-argument methods, which
     * will not perform any URL encoding.
     *
     * <p>Objects passed to and returned from these methods are converted to and from HTTP messages by {@link
     * HttpMessageConverter} instances. Converters for the main mime types are registered by default, but you can also write
     * your own converter and register it via the {@link #setMessageConverters messageConverters} bean property.
     *
     * <p>This template uses a {@link org.springframework.http.client.SimpleClientHttpRequestFactory} and a {@link
     * DefaultResponseErrorHandler} as default strategies for creating HTTP connections or handling HTTP errors,
     * respectively. These defaults can be overridden through the {@link #setRequestFactory(ClientHttpRequestFactory)
     * requestFactory} and {@link #setErrorHandler(ResponseErrorHandler) errorHandler} bean properties.
     *
     * @author Arjen Poutsma
     * @see HttpMessageConverter
     * @see RequestCallback
     * @see ResponseExtractor
     * @see ResponseErrorHandler
     * @since 3.0
     */
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

        public bool ThrowExceptionOnError
        {
            get { return _throwExceptionOnError; }
            set { _throwExceptionOnError = value; }
        }

        public IList<IHttpMessageConverter> MessageConverters
        {
            get { return this._messageConverters; }
            set { this._messageConverters = value; }
        }

        public IHttpWebRequestFactory RequestFactory
        {
            get { return this._requestFactory; }
            set { this._requestFactory = value; }
        }

        #endregion

        #region Constructor(s)

        public RestTemplate(Uri baseAddress) :
            this()
        {
            this.BaseAddress = baseAddress;
        }

        public RestTemplate(string baseAddress) :
            this() 
        {
            this.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
        }

        /** Create a new instance of the {@link RestTemplate} using default settings. */
        public RestTemplate()
        {
            this._throwExceptionOnError = true;
            this.headersExtractor = new HeadersResponseExtractor();
            this._requestFactory = new DefaultHttpWebRequestFactory();

            this._messageConverters = new List<IHttpMessageConverter>();
#if NET_3_5
            //this._messageConverters.Add(new JsonHttpMessageConverter());
            this._messageConverters.Add(new Atom10FeedHttpMessageConverter());
            this._messageConverters.Add(new Rss20FeedHttpMessageConverter());
            this._messageConverters.Add(new XElementHttpMessageConverter());
#endif
            this._messageConverters.Add(new XmlDocumentHttpMessageConverter());
            //this._messageConverters.Add(new XmlSerializableHttpMessageConverter());
            this._messageConverters.Add(new StringHttpMessageConverter());
            this._messageConverters.Add(new ByteArrayHttpMessageConverter());
        }

        #endregion

        #region IRestOperations Membres

        public T GetForObject<T>(string url, params string[] uriVariables) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(HttpMethod.GET, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, requestCallback, responseExtractor, uriVariables);
        }

        public T GetForObject<T>(string url, IDictionary<string, string> uriVariables) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(HttpMethod.GET, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, requestCallback, responseExtractor, uriVariables);
        }

        public T GetForObject<T>(Uri url) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(HttpMethod.GET, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, requestCallback, responseExtractor);
        }

        public HttpResponseMessage<T> GetForMessage<T>(string url, params string[] uriVariables) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(HttpMethod.GET, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor, uriVariables);
        }

        public HttpResponseMessage<T> GetForMessage<T>(string url, IDictionary<string, string> uriVariables) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(HttpMethod.GET, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor, uriVariables);
        }

        public HttpResponseMessage<T> GetForMessage<T>(Uri url) where T : class
        {
            AcceptHeaderRequestCallback requestCallback = new AcceptHeaderRequestCallback(HttpMethod.GET, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor);
        }

        public WebHeaderCollection HeadForHeaders(string url, params string[] uriVariables)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.HEAD);
            return this.Execute<WebHeaderCollection>(url, requestCallback, this.headersExtractor, uriVariables);
        }

        public WebHeaderCollection HeadForHeaders(string url, IDictionary<string, string> uriVariables)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.HEAD);
            return this.Execute<WebHeaderCollection>(url, requestCallback, this.headersExtractor, uriVariables);
        }

        public WebHeaderCollection HeadForHeaders(Uri url)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.HEAD);
            return this.Execute<WebHeaderCollection>(url, requestCallback, this.headersExtractor);
        }

        public Uri PostForLocation(string url, object request, params string[] uriVariables)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, this._messageConverters);
            WebHeaderCollection headers = this.Execute<WebHeaderCollection>(
                url, requestCallback, this.headersExtractor, uriVariables);
            string location = headers[HttpResponseHeader.Location];
            return StringUtils.HasText(location) ? new Uri(location) : null;
        }

        public Uri PostForLocation(string url, object request, IDictionary<string, string> uriVariables)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, this._messageConverters);
            WebHeaderCollection headers = this.Execute<WebHeaderCollection>(
                url, requestCallback, this.headersExtractor, uriVariables);
            string location = headers[HttpResponseHeader.Location];
            return StringUtils.HasText(location) ? new Uri(location) : null;
        }

        public Uri PostForLocation(Uri url, object request)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, this._messageConverters);
            WebHeaderCollection headers = this.Execute<WebHeaderCollection>(
                url, requestCallback, this.headersExtractor);
            string location = headers[HttpResponseHeader.Location];
            return StringUtils.HasText(location) ? new Uri(location) : null;
        }

        public T PostForObject<T>(string url, object request, params string[] uriVariables) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, requestCallback, responseExtractor, uriVariables);
        }

        public T PostForObject<T>(string url, object request, IDictionary<string, string> uriVariables) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, requestCallback, responseExtractor, uriVariables);
        }

        public T PostForObject<T>(Uri url, object request) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, typeof(T), this._messageConverters);
            MessageConverterResponseExtractor<T> responseExtractor = new MessageConverterResponseExtractor<T>(this._messageConverters);
            return this.Execute<T>(url, requestCallback, responseExtractor);
        }

        public HttpResponseMessage<T> PostForMessage<T>(string url, object request, params string[] uriVariables) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor, uriVariables);
        }

        public HttpResponseMessage<T> PostForMessage<T>(string url, object request, IDictionary<string, string> uriVariables) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor, uriVariables);
        }

        public HttpResponseMessage<T> PostForMessage<T>(Uri url, object request) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.POST, request, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor);
        }

        public void Put(string url, object request, params string[] uriVariables)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.PUT, request, this._messageConverters);
            this.Execute<object>(url, requestCallback, null, uriVariables);
        }

        public void Put(string url, object request, IDictionary<string, string> uriVariables)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.PUT, request, this._messageConverters);
            this.Execute<object>(url, requestCallback, null, uriVariables);
        }

        public void Put(Uri url, object request)
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(
                HttpMethod.PUT, request, this._messageConverters);
            this.Execute<object>(url, requestCallback, null);
        }

        public void Delete(string url, params string[] uriVariables)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.DELETE);
            this.Execute<object>(url, requestCallback, null, uriVariables);
        }

        public void Delete(string url, IDictionary<string, string> uriVariables)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.DELETE);
            this.Execute<object>(url, requestCallback, null, uriVariables);
        }

        public void Delete(Uri url)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.DELETE);
            this.Execute<object>(url, requestCallback, null);
        }

        public IList<HttpMethod> OptionsForAllow(string url, params string[] uriVariables)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.OPTIONS);
            WebHeaderCollection headers = this.Execute<WebHeaderCollection>(
                url, requestCallback, this.headersExtractor, uriVariables);
            string allow = headers[HttpResponseHeader.Allow];

            return ParseAllowHeader(allow);
        }

        public IList<HttpMethod> OptionsForAllow(string url, IDictionary<string, string> uriVariables)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.OPTIONS);
            WebHeaderCollection headers = this.Execute<WebHeaderCollection>(url, requestCallback, this.headersExtractor, uriVariables);
            string allow = headers[HttpResponseHeader.Allow];

            return ParseAllowHeader(allow);
        }

        public IList<HttpMethod> OptionsForAllow(Uri url)
        {
            MethodRequestCallback requestCallback = new MethodRequestCallback(HttpMethod.OPTIONS);
            WebHeaderCollection headers = this.Execute<WebHeaderCollection>(url, requestCallback, this.headersExtractor);
            string allow = headers[HttpResponseHeader.Allow];

            return ParseAllowHeader(allow);
        }

        public HttpResponseMessage<T> Exchange<T>(string url, HttpRequestMessage requestMessage, params string[] uriVariables) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(requestMessage, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor, uriVariables);
        }

        public HttpResponseMessage<T> Exchange<T>(string url, HttpRequestMessage requestMessage, IDictionary<string, string> uriVariables) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(requestMessage, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor, uriVariables);
        }

        public HttpResponseMessage<T> Exchange<T>(Uri url, HttpRequestMessage requestMessage) where T : class
        {
            HttpMessageRequestCallback requestCallback = new HttpMessageRequestCallback(requestMessage, typeof(T), this._messageConverters);
            HttpMessageResponseExtractor<T> responseExtractor = new HttpMessageResponseExtractor<T>(this._messageConverters);
            return this.Execute<HttpResponseMessage<T>>(url, requestCallback, responseExtractor);
        }

        public T Execute<T>(string url, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor, params string[] uriVariables) where T : class
        {
            UriTemplate uriTemplate = new UriTemplate(url);
            Uri uri = uriTemplate.Expand(uriVariables);
            return this.DoExecute<T>(uri, requestCallback, responseExtractor);
        }

        public T Execute<T>(string url, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor, IDictionary<string, string> uriVariables) where T : class
        {
            UriTemplate uriTemplate = new UriTemplate(url);
            Uri uri = uriTemplate.Expand(uriVariables);
            return this.DoExecute<T>(uri, requestCallback, responseExtractor);
        }

        public T Execute<T>(Uri url, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor) where T : class
        {
            return this.DoExecute<T>(url, requestCallback, responseExtractor);
        }

        #endregion

        /**
	     * Execute the given method on the provided URI. The {@link ClientHttpRequest} is processed using the {@link
	     * RequestCallback}; the response with the {@link ResponseExtractor}.
	     * @param url the fully-expanded URL to connect to
	     * @param method the HTTP method to execute (GET, POST, etc.)
	     * @param requestCallback object that prepares the request (can be <code>null</code>)
	     * @param responseExtractor object that extracts the return value from the response (can be <code>null</code>)
	     * @return an arbitrary object, as returned by the {@link ResponseExtractor}
	     */
        protected virtual T DoExecute<T>(Uri url, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor) where T : class
        {
            HttpWebRequest request;
            HttpWebResponse response = null;

            Uri finalUri = url;
            if (!url.IsAbsoluteUri)
            {
                if (this._baseAddress != null)
                {
                    finalUri = new Uri(this._baseAddress, url);
                }
                else
                {
                    throw new ArgumentException(String.Format("'{0}' is not an absolute URI", url), "url");
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
