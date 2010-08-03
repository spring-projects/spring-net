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
using System.Security.Cryptography.X509Certificates;

namespace Spring.Http
{
    /// <summary>
    /// Factory for <see cref="HttpWebRequest"/> objects. Requests are created by the <see cref="M:CreateRequest"/> method.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public class DefaultHttpWebRequestFactory : IHttpWebRequestFactory
    {
        // TODO : Add other properties

        private X509CertificateCollection _clientCertificates;
        private ICredentials _credentials;
        private IWebProxy _proxy;
        private int? _timeout;

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

        /// <summary>
        /// Gets or sets authentication information for the request.
        /// </summary>
        public ICredentials Credentials
        {
            get { return _credentials; }
            set { _credentials = value; }
        }

        /// <summary>
        /// Gets or sets proxy information for the request.
        /// </summary>
        /// <remarks>
        ///  The default value is set by calling the <see cref="P:System.Net.GlobalProxySelection.Select"/> property.
        /// </remarks>
        public IWebProxy Proxy
        {
            get { return _proxy; }
            set { _proxy = value; }
        }

        /// <summary>
        /// Gets or sets the time-out value in milliseconds for the <see cref="M:System.Net.HttpWebRequest.GetResponse()"/> 
        /// and <see cref="M:System.Net.HttpWebRequest.GetRequestStream()"/> methods.
        /// </summary>
        /// <remarks>
        /// The default is 100,000 milliseconds (100 seconds).
        /// </remarks>
        public int? Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        #region IHttpWebRequestFactory Membres

        /// <summary>
        /// Create a new <see cref="HttpWebRequest"/> for the specified URI.
        /// </summary>
        /// <param name="uri">The URI to create a request for.</param>
        /// <returns>The created request</returns>
        public HttpWebRequest CreateRequest(Uri uri)
        {
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;

            if (this._clientCertificates != null)
            {
                foreach (X509Certificate2 certificate in this._clientCertificates)
                {
                    request.ClientCertificates.Add(certificate);
                }
            }

            if (this._credentials != null)
            {
                request.Credentials = this._credentials;
            }

            if (this._proxy != null)
            {
                request.Proxy = this._proxy;
            }

            if (this._timeout != null)
            {
                request.Timeout = this._timeout.Value;
            }

            return request;
        }

        #endregion
    }
}
