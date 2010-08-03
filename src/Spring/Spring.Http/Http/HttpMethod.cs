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

namespace Spring.Http
{
    /// <summary>
    /// Enumeration of HTTP request methods as defined in the HTTP specification. 
    /// <a href="http://tools.ietf.org/html/rfc2616#section-5.1.1">HTTP 1.1, section 6</a>
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia</author>
    public enum HttpMethod
    {
        /// <summary>
        /// The OPTIONS method.
        /// </summary>
        OPTIONS,

        /// <summary>
        /// The GET method.
        /// </summary>
        GET,

        /// <summary>
        /// The HEAD method.
        /// </summary>
        HEAD,

        /// <summary>
        /// The POST method.
        /// </summary>
        POST,

        /// <summary>
        /// The PUT method.
        /// </summary>
        PUT,

        /// <summary>
        /// The DELETE method.
        /// </summary>
        DELETE,

        /// <summary>
        /// The TRACE method.
        /// </summary>
        TRACE,

        /// <summary>
        /// The CONNECT method.
        /// </summary>
        CONNECT
    }
}
