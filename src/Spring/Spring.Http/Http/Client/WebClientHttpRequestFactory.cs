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
using System.Security.Cryptography.X509Certificates;

namespace Spring.Http.Client
{
    /// <summary>
    /// <see cref="IClientHttpRequestFactory"/> implementation that uses 
    /// .NET <see cref="HttpWebRequest"/>'s class to create requests.
    /// </summary>
    /// <see cref="WebClientHttpRequest"/>
    /// <author>Bruno Baia</author>
    public class WebClientHttpRequestFactory : IClientHttpRequestFactory
    {
        /// <summary>
        /// The .NET <see cref="HttpWebRequest"/> used by this factory 
        /// or <see langword="null"/> if not created.
        /// </summary>
        private HttpWebRequest httpWebRequest;

        #region Properties

#if !SILVERLIGHT_3
        private bool? _useDefaultCredentials;
        /// <summary>
        /// Gets or sets a boolean value that controls whether default credentials are sent with this request.
        /// </summary>
        public bool? UseDefaultCredentials
        {
            get { return this._useDefaultCredentials; }
            set { this._useDefaultCredentials = value; }
        }
#endif

        private ICredentials _credentials;
        /// <summary>
        /// Gets or sets authentication information for the request.
        /// </summary>
        public ICredentials Credentials
        {
            get { return this._credentials; }
            set { this._credentials = value; }
        }

#if !SILVERLIGHT
        private X509CertificateCollection _clientCertificates;
        /// <summary>
        /// Gets or sets the collection of security certificates that are associated with this request.
        /// </summary>
        public X509CertificateCollection ClientCertificates
        {
            get
            {
                if (this._clientCertificates == null)
                {
                    this._clientCertificates = new X509CertificateCollection();
                }
                return this._clientCertificates;
            }
        }

        private IWebProxy _proxy;
        /// <summary>
        /// Gets or sets proxy information for the request.
        /// </summary>
        /// <remarks>
        ///  The default value is set by calling the <see cref="P:System.Net.GlobalProxySelection.Select"/> property.
        /// </remarks>
        public IWebProxy Proxy
        {
            get { return this._proxy; }
            set { this._proxy = value; }
        }

        private int? _timeout;
        /// <summary>
        /// Gets or sets the time-out value in milliseconds for synchrone request only.
        /// </summary>
        /// <remarks>
        /// The default is 100,000 milliseconds (100 seconds).
        /// </remarks>
        public int? Timeout
        {
            get { return this._timeout; }
            set { this._timeout = value; }
        }
#endif

#if SILVERLIGHT && !WINDOWS_PHONE
        private WebRequestCreatorType _webRequestCreator;
        /// <summary>
        /// Gets or sets a value that indicates how HTTP requests and responses will be handled. 
        /// </summary>
        /// <remarks>
        /// By default, this factory will use the default Silverlight behavior for HTTP methods GET and POST, 
        /// and force the client HTTP stack for other HTTP methods.
        /// </remarks>
        public WebRequestCreatorType WebRequestCreator
        {
            get { return this._webRequestCreator; }
            set { this._webRequestCreator = value; }
        }
#endif

        #endregion

#if SILVERLIGHT && !WINDOWS_PHONE
        /// <summary>
        /// Creates a new instance of <see cref="WebClientHttpRequestFactory"/>.
        /// </summary>
        public WebClientHttpRequestFactory()
        {
            this._webRequestCreator = WebRequestCreatorType.Unknown;
        }
#endif

        #region IClientHttpRequestFactory Membres

        /// <summary>
        /// Create a new <see cref="IClientHttpRequest"/> for the specified URI and HTTP method.
        /// </summary>
        /// <param name="uri">The URI to create a request for.</param>
        /// <param name="method">The HTTP method to execute.</param>
        /// <returns>The created request</returns>
        public virtual IClientHttpRequest CreateRequest(Uri uri, HttpMethod method)
        {
#if SILVERLIGHT && !WINDOWS_PHONE
            switch (this._webRequestCreator)
            {
                case WebRequestCreatorType.ClientHttp:
                    this.httpWebRequest = (HttpWebRequest)System.Net.Browser.WebRequestCreator.ClientHttp.Create(uri);
                    break;
                case WebRequestCreatorType.BrowserHttp:
                    this.httpWebRequest = (HttpWebRequest)System.Net.Browser.WebRequestCreator.BrowserHttp.Create(uri);
                    break;
                case WebRequestCreatorType.Unknown:
                    if (method == HttpMethod.GET || method == HttpMethod.POST)
                    {
                        this.httpWebRequest = WebRequest.Create(uri) as HttpWebRequest;
                    }
                    else
                    {
                        // Force Client HTTP stack
                        this.httpWebRequest = (HttpWebRequest)System.Net.Browser.WebRequestCreator.ClientHttp.Create(uri);
                    }
                    break;
            }
#else
            this.httpWebRequest = WebRequest.Create(uri) as HttpWebRequest;
#endif

            this.httpWebRequest.Method = method.ToString();

#if !SILVERLIGHT_3
            if (this._useDefaultCredentials.HasValue)
            {
                this.httpWebRequest.UseDefaultCredentials = this._useDefaultCredentials.Value;
            }
#endif
            if (this._credentials != null)
            {
                this.httpWebRequest.Credentials = this._credentials;
            }
#if !SILVERLIGHT
            if (this._clientCertificates != null)
            {
                foreach (X509Certificate2 certificate in this._clientCertificates)
                {
                    this.httpWebRequest.ClientCertificates.Add(certificate);
                }
            }
            if (this._proxy != null)
            {
                this.httpWebRequest.Proxy = this._proxy;
            }
            if (this._timeout != null)
            {
                this.httpWebRequest.Timeout = this._timeout.Value;
            }
#endif

            return new WebClientHttpRequest(this.httpWebRequest);
        }

        #endregion
    }

#if SILVERLIGHT && !WINDOWS_PHONE
    /// <summary>
    /// Defines identifiers for supported Silverlight HTTP handling stacks.  
    /// </summary>
    public enum WebRequestCreatorType
    {
        /// <summary>
        /// Specifies an unknown HTTP handling stack.
        /// </summary>
        Unknown,
        /// <summary>
        /// Specifies browser HTTP handling stack.
        /// </summary>
        BrowserHttp,
        /// <summary>
        /// Specifies client HTTP handling stack.
        /// </summary>
        ClientHttp
    }
#endif
}
