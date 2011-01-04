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
using System.Net;
using System.Collections.Generic;

namespace Spring.Http.Rest
{
    /// <summary>
    /// Interface specifying a basic set of RESTful operations. 
    /// </summary>
    /// <remarks>
    /// Not often used directly, but a useful option to enhance testability, 
    /// as it can easily be mocked or stubbed.
    /// </remarks>
    /// <see cref="RestTemplate"/>
    /// <author>Arjen Poutsma</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Bruno Baia (.NET)</author>
    public interface IRestAsyncOperations
    {
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
        void GetForObjectAsync<T>(string url, object[] uriVariables, Action<MethodCompletedEventArgs<T>> getCompleted) where T : class;

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
        void GetForObjectAsync<T>(string url, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<T>> getCompleted) where T : class;

        /// <summary>
        /// Retrieve a representation by doing a GET on the specified URL. 
        /// The response (if any) is converted and returned.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <returns>The converted object</returns>
        void GetForObjectAsync<T>(Uri url, Action<MethodCompletedEventArgs<T>> getCompleted) where T : class;

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
        void GetForMessageAsync<T>(string url, object[] uriVariables, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> getCompleted) where T : class;

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
        void GetForMessageAsync<T>(string url, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> getCompleted) where T : class;

        /// <summary>
        /// Retrieve an entity by doing a GET on the specified URL. 
        /// The response is converted and stored in an <see cref="HttpResponseMessage{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <returns>The HTTP response message.</returns>
        void GetForMessageAsync<T>(Uri url, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> getCompleted) where T : class;

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
        void HeadForHeadersAsync(string url, object[] uriVariables, Action<MethodCompletedEventArgs<HttpHeaders>> headCompleted);

        /// <summary>
        /// Retrieve all headers of the resource specified by the URI template.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>All HTTP headers of that resource</returns>
        void HeadForHeadersAsync(string url, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<HttpHeaders>> headCompleted);

        /// <summary>
        /// Retrieve all headers of the resource specified by the URI template.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>All HTTP headers of that resource</returns>
        void HeadForHeadersAsync(Uri url, Action<MethodCompletedEventArgs<HttpHeaders>> headCompleted);

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
        /// <para>
        /// The request parameter can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The value for the Location header.</returns>
        void PostForLocationAsync(string url, object request, object[] uriVariables, Action<Uri> postCompleted);

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
        /// The request parameter can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The value for the Location header.</returns>
        void PostForLocationAsync(string url, object request, IDictionary<string, object> uriVariables, Action<Uri> postCompleted);

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the value of the 'Location' header. 
        /// This header typically indicates where the new resource is stored.
        /// </summary>
        /// <remarks>
        /// The request parameter can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <returns>The value for the Location header.</returns>
        void PostForLocationAsync(Uri url, object request, Action<Uri> postCompleted);

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the representation found in the response. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The converted object.</returns>
        void PostForObjectAsync<T>(string url, object request, object[] uriVariables, Action<MethodCompletedEventArgs<T>> postCompleted) where T : class;

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the representation found in the response. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The converted object.</returns>
        void PostForObjectAsync<T>(string url, object request, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<T>> postCompleted) where T : class;

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the representation found in the response. 
        /// </summary>
        /// <remarks>
        /// The request parameter can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <returns>The converted object.</returns>
        void PostForObjectAsync<T>(Uri url, object request, Action<MethodCompletedEventArgs<T>> postCompleted) where T : class;

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The HTTP response message.</returns>
        void PostForMessageAsync<T>(string url, object request, object[] uriVariables, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> postCompleted) where T : class;

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The HTTP response message.</returns>
        void PostForMessageAsync<T>(string url, object request, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> postCompleted) where T : class;

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response as <see cref="HttpResponseMessage{T}"/>. 
        /// </summary>
        /// <remarks>
        /// The request parameter can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </remarks>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <returns>The HTTP response message.</returns>
        void PostForMessageAsync<T>(Uri url, object request, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> postCompleted) where T : class;

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        /// <returns>The HTTP response message with no entity.</returns>
        void PostForMessageAsync(string url, object request, object[] uriVariables, Action<MethodCompletedEventArgs<HttpResponseMessage>> postCompleted);

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The HTTP response message with no entity.</returns>
        void PostForMessageAsync(string url, object request, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<HttpResponseMessage>> postCompleted);

        /// <summary>
        /// Create a new resource by POSTing the given object to the URI template, 
        /// and returns the response with no entity as <see cref="HttpResponseMessage"/>. 
        /// </summary>
        /// <remarks>
        /// The request parameter can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be POSTed, may be null.</param>
        /// <returns>The HTTP response message with no entity.</returns>
        void PostForMessageAsync(Uri url, object request, Action<MethodCompletedEventArgs<HttpResponseMessage>> postCompleted);

        #endregion

        #region PUT

        /// <summary>
        /// Create or update a resource by PUTting the given object to the URI.
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given URI variables, if any.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be PUT, may be null.</param>
        /// <param name="uriVariables">The variables to expand the template.</param>
        void PutAsync(string url, object request, object[] uriVariables, Action<MethodCompletedEventArgs<object>> putCompleted);

        /// <summary>
        /// Create or update a resource by PUTting the given object to the URI.
        /// </summary>
        /// <remarks>
        /// <para>
        /// URI Template variables are expanded using the given dictionary.
        /// </para>
        /// <para>
        /// The request parameter can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </para>
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be PUT, may be null.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        void PutAsync(string url, object request, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<object>> putCompleted);

        /// <summary>
        /// Create or update a resource by PUTting the given object to the URI.
        /// </summary>
        /// <remarks>
        /// The request parameter can be a <see cref="HttpEntity"/> in order to add additional HTTP headers to the request.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="request">The Object to be PUT, may be null.</param>
        void PutAsync(Uri url, object request, Action<MethodCompletedEventArgs<object>> putCompleted);

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
        void DeleteAsync(string url, object[] uriVariables, Action<MethodCompletedEventArgs<object>> deleteCompleted);

        /// <summary>
        /// Delete the resources at the specified URI.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        void DeleteAsync(string url, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<object>> deleteCompleted);

        /// <summary>
        /// Delete the resources at the specified URI.
        /// </summary>
        /// <param name="url">The URL.</param>
        void DeleteAsync(Uri url, Action<MethodCompletedEventArgs<object>> deleteCompleted);

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
        void OptionsForAllowAsync(string url, object[] uriVariables, Action<IList<HttpMethod>> optionsCompleted);

        /// <summary>
        /// Return the value of the Allow header for the given URI.
        /// </summary>
        /// <remarks>
        /// URI Template variables are expanded using the given dictionary.
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <param name="uriVariables">The dictionary containing variables for the URI template.</param>
        /// <returns>The value of the allow header.</returns>
        void OptionsForAllowAsync(string url, IDictionary<string, object> uriVariables, Action<IList<HttpMethod>> optionsCompleted);

        /// <summary>
        /// Return the value of the Allow header for the given URI.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The value of the allow header.</returns>
        void OptionsForAllowAsync(Uri url, Action<IList<HttpMethod>> optionsCompleted);

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
        void ExchangeAsync<T>(string url, HttpMethod method, HttpEntity requestEntity, object[] uriVariables, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> methodCompleted) where T : class;

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
        void ExchangeAsync<T>(string url, HttpMethod method, HttpEntity requestEntity, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> methodCompleted) where T : class;

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
        void ExchangeAsync<T>(Uri url, HttpMethod method, HttpEntity requestEntity, Action<MethodCompletedEventArgs<HttpResponseMessage<T>>> methodCompleted) where T : class;

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
        void ExchangeAsync(string url, HttpMethod method, HttpEntity requestEntity, object[] uriVariables, Action<MethodCompletedEventArgs<HttpResponseMessage>> methodCompleted);

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
        void ExchangeAsync(string url, HttpMethod method, HttpEntity requestEntity, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<HttpResponseMessage>> methodCompleted);

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
        void ExchangeAsync(Uri url, HttpMethod method, HttpEntity requestEntity, Action<MethodCompletedEventArgs<HttpResponseMessage>> methodCompleted);

        #endregion

        #region General execution

        /// <summary>
        /// Execute the HTTP method to the given URI template, preparing the request with the 
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
        void ExecuteAsync<T>(string url, HttpMethod method, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor, object[] uriVariables, Action<MethodCompletedEventArgs<T>> methodCompleted) where T : class;

        /// <summary>
        /// Execute the HTTP method to the given URI template, preparing the request with the 
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
        void ExecuteAsync<T>(string url, HttpMethod method, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor, IDictionary<string, object> uriVariables, Action<MethodCompletedEventArgs<T>> methodCompleted) where T : class;

        /// <summary>
        /// Execute the HTTP method to the given URI template, preparing the request with the 
        /// <see cref="IRequestCallback"/>, and reading the response with an <see cref="IResponseExtractor{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the response value.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="requestCallback">Object that prepares the request.</param>
        /// <param name="responseExtractor">Object that extracts the return value from the response.</param>
        /// <returns>An arbitrary object, as returned by the <see cref="IResponseExtractor{T}"/>.</returns>   
        void ExecuteAsync<T>(Uri url, HttpMethod method, IRequestCallback requestCallback, IResponseExtractor<T> responseExtractor, Action<MethodCompletedEventArgs<T>> methodCompleted) where T : class;

        #endregion

        // TODO : void CancelAsync();
    }
}
