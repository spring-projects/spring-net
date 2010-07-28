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

using Spring.Http;

namespace Spring.Http.Rest
{
    public interface IRestOperations
    {
        #region GET

        /**
	     * Retrieve a representation by doing a GET on the specified URL.
	     * The response (if any) is converted and returned.
	     * <p>URI Template variables are expanded using the given URI variables, if any.
	     * @param url the URL
	     * @param responseType the type of the return value
	     * @param uriVariables the variables to expand the template
	     * @return the converted object
	     */
        T GetForObject<T>(string url, params string[] uriVariables) where T : class; //throws RestClientException;

        /**
         * Retrieve a representation by doing a GET on the URI template.
         * The response (if any) is converted and returned.
         * <p>URI Template variables are expanded using the given map.
         * @param url the URL
         * @param responseType the type of the return value
         * @param uriVariables the map containing variables for the URI template
         * @return the converted object
         */
        T GetForObject<T>(string url, IDictionary<string, string> uriVariables) where T : class; //throws RestClientException;

        /**
         * Retrieve a representation by doing a GET on the URL .
         * The response (if any) is converted and returned.
         * @param url the URL
         * @param responseType the type of the return value
         * @return the converted object
         */
        T GetForObject<T>(Uri url) where T : class; //throws RestClientException;

        /**
         * Retrieve an entity by doing a GET on the specified URL.
         * The response is converted and stored in an {@link ResponseEntity}.
         * <p>URI Template variables are expanded using the given URI variables, if any.
         * @param url the URL
         * @param responseType the type of the return value
         * @param uriVariables the variables to expand the template
         * @return the entity
         * @since 3.0.2
         */
        HttpResponseMessage<T> GetForMessage<T>(string url, params string[] uriVariables) where T : class; //throws RestClientException;

        /**
         * Retrieve a representation by doing a GET on the URI template.
         * The response is converted and stored in an {@link ResponseEntity}.
         * <p>URI Template variables are expanded using the given map.
         * @param url the URL
         * @param responseType the type of the return value
         * @param uriVariables the map containing variables for the URI template
         * @return the converted object
         * @since 3.0.2
         */
        HttpResponseMessage<T> GetForMessage<T>(string url, IDictionary<string, string> uriVariables) where T : class; //throws RestClientException;

        /**
         * Retrieve a representation by doing a GET on the URL .
         * The response is converted and stored in an {@link ResponseEntity}.
         * @param url the URL
         * @param responseType the type of the return value
         * @return the converted object
         * @since 3.0.2
         */
        HttpResponseMessage<T> GetForMessage<T>(Uri url) where T : class; //throws RestClientException;

        #endregion

        #region HEAD

        /**
	     * Retrieve all headers of the resource specified by the URI template.
	     * <p>URI Template variables are expanded using the given URI variables, if any.
	     * @param url the URL
	     * @param uriVariables the variables to expand the template
	     * @return all HTTP headers of that resource
	     */
        WebHeaderCollection HeadForHeaders(string url, params string[] uriVariables); //throws RestClientException;

        /**
         * Retrieve all headers of the resource specified by the URI template.
         * <p>URI Template variables are expanded using the given map.
         * @param url the URL
         * @param uriVariables the map containing variables for the URI template
         * @return all HTTP headers of that resource
         */
        WebHeaderCollection HeadForHeaders(string url, IDictionary<string, string> uriVariables); //throws RestClientException;

        /**
         * Retrieve all headers of the resource specified by the URL.
         * @param url the URL
         * @return all HTTP headers of that resource
         */
        WebHeaderCollection HeadForHeaders(Uri url); //throws RestClientException;

        #endregion

        #region POST

        /**
	     * Create a new resource by POSTing the given object to the URI template, and returns the value of the
	     * <code>Location</code> header. This header typically indicates where the new resource is stored.
	     * <p>URI Template variables are expanded using the given URI variables, if any.
	     * <p>The {@code request} parameter can be a {@link HttpEntity} in order to
	     * add additional HTTP headers to the request.
	     * @param url the URL
	     * @param request the Object to be POSTed, may be <code>null</code>
	     * @param uriVariables the variables to expand the template
	     * @return the value for the <code>Location</code> header
	     * @see HttpEntity
	     */
        Uri PostForLocation(string url, object request, params string[] uriVariables); //throws RestClientException;

        /**
         * Create a new resource by POSTing the given object to the URI template, and returns the value of the
         * <code>Location</code> header. This header typically indicates where the new resource is stored.
         * <p>URI Template variables are expanded using the given map.
         * <p>The {@code request} parameter can be a {@link HttpEntity} in order to
         * add additional HTTP headers to the request.
         * @param url the URL
         * @param request the Object to be POSTed, may be <code>null</code>
         * @param uriVariables the variables to expand the template
         * @return the value for the <code>Location</code> header
         * @see HttpEntity
         */
        Uri PostForLocation(string url, object request, IDictionary<string, string> uriVariables); //throws RestClientException;

        /**
         * Create a new resource by POSTing the given object to the URL, and returns the value of the
         * <code>Location</code> header. This header typically indicates where the new resource is stored.
         * <p>The {@code request} parameter can be a {@link HttpEntity} in order to
         * add additional HTTP headers to the request.
         * @param url the URL
         * @param request the Object to be POSTed, may be <code>null</code>
         * @return the value for the <code>Location</code> header
         * @see HttpEntity
         */
        Uri PostForLocation(Uri url, object request); //throws RestClientException;

        /**
         * Create a new resource by POSTing the given object to the URI template,
         * and returns the representation found in the response.
         * <p>URI Template variables are expanded using the given URI variables, if any.
         * <p>The {@code request} parameter can be a {@link HttpEntity} in order to
         * add additional HTTP headers to the request.
         * @param url the URL
         * @param request the Object to be POSTed, may be <code>null</code>
         * @param responseType the type of the return value
         * @param uriVariables the variables to expand the template
         * @return the converted object
         * @see HttpEntity
         */
        T PostForObject<T>(string url, object request, params string[] uriVariables) where T : class; //throws RestClientException;

        /**
         * Create a new resource by POSTing the given object to the URI template,
         * and returns the representation found in the response.
         * <p>URI Template variables are expanded using the given map.
         * <p>The {@code request} parameter can be a {@link HttpEntity} in order to
         * add additional HTTP headers to the request.
         * @param url the URL
         * @param request the Object to be POSTed, may be <code>null</code>
         * @param responseType the type of the return value
         * @param uriVariables the variables to expand the template
         * @return the converted object
         * @see HttpEntity
         */
        T PostForObject<T>(string url, object request, IDictionary<string, string> uriVariables) where T : class; //throws RestClientException;

        /**
         * Create a new resource by POSTing the given object to the URL,
         * and returns the representation found in the response.
         * <p>The {@code request} parameter can be a {@link HttpEntity} in order to
         * add additional HTTP headers to the request.
         * @param url the URL
         * @param request the Object to be POSTed, may be <code>null</code>
         * @param responseType the type of the return value
         * @return the converted object
         * @see HttpEntity
         */
        T PostForObject<T>(Uri url, object request) where T : class; //throws RestClientException;

        /**
         * Create a new resource by POSTing the given object to the URI template,
         * and returns the response as {@link ResponseEntity}.
         * <p>URI Template variables are expanded using the given URI variables, if any.
         * <p>The {@code request} parameter can be a {@link HttpEntity} in order to
         * add additional HTTP headers to the request.
         * @param url the URL
         * @param request the Object to be POSTed, may be <code>null</code>
         * @param uriVariables the variables to expand the template
         * @return the converted object
         * @see HttpEntity
         * @since 3.0.2
         */
        HttpResponseMessage<T> PostForMessage<T>(string url, object request, params string[] uriVariables) where T : class; //throws RestClientException;

        /**
         * Create a new resource by POSTing the given object to the URI template,
         * and returns the response as {@link HttpEntity}.
         * <p>URI Template variables are expanded using the given map.
         * <p>The {@code request} parameter can be a {@link HttpEntity} in order to
         * add additional HTTP headers to the request.
         * @param url the URL
         * @param request the Object to be POSTed, may be <code>null</code>
         * @param uriVariables the variables to expand the template
         * @return the converted object
         * @see HttpEntity
         * @since 3.0.2
         */
        HttpResponseMessage<T> PostForMessage<T>(string url, object request, IDictionary<string, string> uriVariables) where T : class; //throws RestClientException;

        /**
         * Create a new resource by POSTing the given object to the URL,
         * and returns the response as {@link ResponseEntity}.
         * <p>The {@code request} parameter can be a {@link HttpEntity} in order to
         * add additional HTTP headers to the request.
         * @param url the URL
         * @param request the Object to be POSTed, may be <code>null</code>
         * @return the converted object
         * @see HttpEntity
         * @since 3.0.2
         */
        HttpResponseMessage<T> PostForMessage<T>(Uri url, object request) where T : class; //throws RestClientException;

        #endregion

        #region PUT

        /**
	     * Create or update a resource by PUTting the given object to the URI.
	     * <p>URI Template variables are expanded using the given URI variables, if any.
	     * <p>The {@code request} parameter can be a {@link HttpEntity} in order to
	     * add additional HTTP headers to the request.
	     * @param url the URL
	     * @param request the Object to be PUT, may be <code>null</code>
	     * @param uriVariables the variables to expand the template
	     * @see HttpEntity
	     */
        void Put(string url, object request, params string[] uriVariables); //throws RestClientException;

        /**
         * Creates a new resource by PUTting the given object to URI template.
         * <p>URI Template variables are expanded using the given map.
         * <p>The {@code request} parameter can be a {@link HttpEntity} in order to
         * add additional HTTP headers to the request.
         * @param url the URL
         * @param request the Object to be PUT, may be <code>null</code>
         * @param uriVariables the variables to expand the template
         * @see HttpEntity
         */
        void Put(string url, object request, IDictionary<string, string> uriVariables); //throws RestClientException;

        /**
         * Creates a new resource by PUTting the given object to URL.
         * <p>The {@code request} parameter can be a {@link HttpEntity} in order to
         * add additional HTTP headers to the request.
         * @param url the URL
         * @param request the Object to be PUT, may be <code>null</code>
         * @see HttpEntity
         */
        void Put(Uri url, object request); //throws RestClientException;

        #endregion

        #region DELETE

        /**
	     * Delete the resources at the specified URI.
	     * <p>URI Template variables are expanded using the given URI variables, if any.
	     * @param url the URL
	     * @param uriVariables the variables to expand in the template
	     */
        void Delete(string url, params string[] uriVariables); //throws RestClientException;

        /**
         * Delete the resources at the specified URI.
         * <p>URI Template variables are expanded using the given map.
         *
         * @param url the URL
         * @param uriVariables the variables to expand the template
         */
        void Delete(string url, IDictionary<string, string> uriVariables); //throws RestClientException;

        /**
         * Delete the resources at the specified URL.
         * @param url the URL
         */
        void Delete(Uri url); //throws RestClientException;

        #endregion

        #region OPTIONS

        /**
	     * Return the value of the Allow header for the given URI.
	     * <p>URI Template variables are expanded using the given URI variables, if any.
	     * @param url the URL
	     * @param uriVariables the variables to expand in the template
	     * @return the value of the allow header
	     */
        IList<HttpMethod> OptionsForAllow(string url, params string[] uriVariables); //throws RestClientException;

        /**
         * Return the value of the Allow header for the given URI.
         * <p>URI Template variables are expanded using the given map.
         * @param url the URL
         * @param uriVariables the variables to expand in the template
         * @return the value of the allow header
         */
        IList<HttpMethod> OptionsForAllow(string url, IDictionary<string, string> uriVariables); //throws RestClientException;

        /**
         * Return the value of the Allow header for the given URL.
         * @param url the URL
         * @return the value of the allow header
         */
        IList<HttpMethod> OptionsForAllow(Uri url); //throws RestClientException;

        #endregion


        #region Exchange

        /**
         * Execute the HTTP method to the given URI template, writing the given request entity to the request, and
         * returns the response as {@link ResponseEntity}.
         * <p>URI Template variables are expanded using the given URI variables, if any.
         * @param url the URL
         * @param method the HTTP method (GET, POST, etc)
         * @param requestEntity the entity (headers and/or body) to write to the request, may be {@code null}
         * @param responseType the type of the return value
         * @param uriVariables the variables to expand in the template
         * @return the response as entity
         * @since 3.0.2
         */
        HttpResponseMessage<T> Exchange<T>(string url, HttpRequestMessage requestMessage, params string[] uriVariables) where T : class; //throws RestClientException;

        /**
         * Execute the HTTP method to the given URI template, writing the given request entity to the request, and
         * returns the response as {@link ResponseEntity}.
         * <p>URI Template variables are expanded using the given URI variables, if any.
         * @param url the URL
         * @param method the HTTP method (GET, POST, etc)
         * @param requestEntity the entity (headers and/or body) to write to the request, may be {@code null}
         * @param responseType the type of the return value
         * @param uriVariables the variables to expand in the template
         * @return the response as entity
         * @since 3.0.2
         */
        HttpResponseMessage<T> Exchange<T>(string url, HttpRequestMessage requestMessage, IDictionary<string, string> uriVariables) where T : class; //throws RestClientException;

        /**
         * Execute the HTTP method to the given URI template, writing the given request entity to the request, and
         * returns the response as {@link ResponseEntity}.
         * @param url the URL
         * @param method the HTTP method (GET, POST, etc)
         * @param requestEntity the entity (headers and/or body) to write to the request, may be {@code null}
         * @param responseType the type of the return value
         * @return the response as entity
         * @since 3.0.2
         */
        HttpResponseMessage<T> Exchange<T>(Uri url, HttpRequestMessage requestMessage) where T : class; //throws RestClientException;

        #endregion

        #region General execution

        /**
         * Execute the HTTP method to the given URI template, preparing the request with the
         * {@link RequestCallback}, and reading the response with a {@link ResponseExtractor}.
         * <p>URI Template variables are expanded using the given URI variables, if any.
         * @param url the URL
         * @param method the HTTP method (GET, POST, etc)
         * @param requestCallback object that prepares the request
         * @param responseExtractor object that extracts the return value from the response
         * @param uriVariables the variables to expand in the template
         * @return an arbitrary object, as returned by the {@link ResponseExtractor}
         */
        T Execute<T>(string url, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor, params string[] uriVariables) where T : class; //throws RestClientException;

        /**
         * Execute the HTTP method to the given URI template, preparing the request with the
         * {@link RequestCallback}, and reading the response with a {@link ResponseExtractor}.
         * <p>URI Template variables are expanded using the given URI variables map.
         * @param url the URL
         * @param method the HTTP method (GET, POST, etc)
         * @param requestCallback object that prepares the request
         * @param responseExtractor object that extracts the return value from the response
         * @param uriVariables the variables to expand in the template
         * @return an arbitrary object, as returned by the {@link ResponseExtractor}
         */
        T Execute<T>(string url, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor, IDictionary<string, string> uriVariables) where T : class; //throws RestClientException;

        /**
         * Execute the HTTP method to the given URL, preparing the request with the
         * {@link RequestCallback}, and reading the response with a {@link ResponseExtractor}.
         * @param url the URL
         * @param method the HTTP method (GET, POST, etc)
         * @param requestCallback object that prepares the request
         * @param responseExtractor object that extracts the return value from the response
         * @return an arbitrary object, as returned by the {@link ResponseExtractor}
         */
        T Execute<T>(Uri url, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor) where T : class; //throws RestClientException;

        #endregion
    }
}
