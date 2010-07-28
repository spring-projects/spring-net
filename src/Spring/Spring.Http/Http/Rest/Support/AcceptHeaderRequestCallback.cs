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
using Spring.Http.Converters;

namespace Spring.Http.Rest.Support
{
    /**
     * Request callback implementation that prepares the request's accept headers.
     */
    public class AcceptHeaderRequestCallback : MethodRequestCallback
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(AcceptHeaderRequestCallback));

        #endregion

        protected Type responseType;
        protected IList<IHttpMessageConverter> messageConverters;

        public AcceptHeaderRequestCallback(HttpMethod method, Type responseType, IList<IHttpMessageConverter> messageConverters) :
            base(method)
        {
            this.responseType = responseType;
            this.messageConverters = messageConverters;
        }

        public override void DoWithRequest(HttpWebRequest request)
        {
            base.DoWithRequest(request);

            if (responseType != null)
            {
                List<MediaType> allSupportedMediaTypes = new List<MediaType>();
                foreach (IHttpMessageConverter messageConverter in this.messageConverters)
                {
                    if (messageConverter.CanRead(responseType, null))
                    {
                        foreach (MediaType supportedMediaType in messageConverter.SupportedMediaTypes)
                        {
                            if (!String.IsNullOrEmpty(supportedMediaType.CharSet))
                            {
                                allSupportedMediaTypes.Add(new MediaType(
                                    supportedMediaType.Type, supportedMediaType.Subtype));
                            }
                            else
                            {
                                allSupportedMediaTypes.Add(supportedMediaType);
                            }
                        }
                    }
                }
                if (allSupportedMediaTypes.Count > 0)
                {
                    MediaType.SortBySpecificity(allSupportedMediaTypes);

                    #region Instrumentation

                    if (LOG.IsDebugEnabled)
                    {
                        LOG.Debug(String.Format(
                            "Setting request Accept header to '{0}'",
                            MediaType.ToString(allSupportedMediaTypes)));
                    }

                    #endregion

                    request.Accept = MediaType.ToString(allSupportedMediaTypes);
                }
            }
        }
    }
}
