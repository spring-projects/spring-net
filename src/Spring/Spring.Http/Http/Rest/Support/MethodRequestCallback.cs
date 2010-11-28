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

namespace Spring.Http.Rest.Support
{
    /// <summary>
    /// Request callback implementation that sets the HTTP method.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class MethodRequestCallback : IRequestCallback
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(MethodRequestCallback));

        #endregion

        /// <summary>
        /// The HTTP method.
        /// </summary>
        protected HttpMethod method;

        /// <summary>
        /// Creates a new instance of <see cref="MethodRequestCallback"/>.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        public MethodRequestCallback(HttpMethod method)
        {
            this.method = method;
        }

        /// <summary>
        /// Gets called by <see cref="RestTemplate"/> with an opened <see cref="HttpWebRequest"/> to write data. 
        /// Does not need to care about closing the request or about handling errors: 
        /// this will all be handled by the <see cref="RestTemplate"/> class.
        /// </summary>
        /// <param name="request">The active HTTP request.</param>
        public virtual void DoWithRequest(HttpWebRequest request)
        {
            #region Instrumentation

            if (LOG.IsDebugEnabled)
            {
                LOG.Debug(String.Format("Setting request Method to '{0}'", this.method));
            }

            #endregion

            request.Method = this.method.ToString();
        }
    }
}
