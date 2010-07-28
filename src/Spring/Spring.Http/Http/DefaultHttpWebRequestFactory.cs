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
    /**
     * Factory for {@link ClientHttpRequest} objects.
     * Requests are created by the {@link #createRequest(URI, HttpMethod)} method.
     *
     * @author Arjen Poutsma
     * @since 3.0
     */
    public class DefaultHttpWebRequestFactory : IHttpWebRequestFactory
    {
        // TODO : Add other properties

        private X509CertificateCollection _clientCertificates;
        private ICredentials _credentials;
        private IWebProxy _proxy;
        private int? _timeout;

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

        public ICredentials Credentials
        {
            get { return _credentials; }
            set { _credentials = value; }
        }

        public IWebProxy Proxy
        {
            get { return _proxy; }
            set { _proxy = value; }
        }

        public int? Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        #region IHttpWebRequestFactory Membres

        /**
         * Create a new {@link ClientHttpRequest} for the specified URI and HTTP method.
         * <p>The returned request can be written to, and then executed by calling
         * {@link ClientHttpRequest#execute()}.
         * @param uri the URI to create a request for
         * @param httpMethod the HTTP method to execute
         * @return the created request
         * @throws IOException in case of I/O errors
         */
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
