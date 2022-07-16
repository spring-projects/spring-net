#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

#region Imports

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// The various result modes.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <seealso cref="Spring.Web.Support.Result"/>
    [Serializable]
    public enum ResultMode
    {
        /// <summary>
        /// A server-side transfer.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Issues a server-side transfer using the
        /// <see cref="System.Web.HttpServerUtility.Transfer(string)"/> method.
        /// </p>
        /// </remarks>
        /// <seealso cref="System.Web.HttpServerUtility.Transfer(string)"/>
        Transfer = 0,

        /// <summary>
        /// A redirect.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Issues a redirect (to the user-agent - typically a browser) using
        /// the <see cref="System.Web.HttpResponse.Redirect(string)"/> method.
        /// </p>
        /// </remarks>
        /// <seealso cref="System.Web.HttpResponse.Redirect(string)"/>
        Redirect = 1,

        /// <summary>
        /// A server-side transfer.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Issues a server-side transfer using the
        /// <see cref="System.Web.HttpServerUtility.Transfer(string,bool)"/> method with parameter 'preserveForm=false'.
        /// </p>
        /// </remarks>
        /// <seealso cref="System.Web.HttpServerUtility.Transfer(string,bool)"/>
        TransferNoPreserve = 2,

    /// <summary>
    /// A redirect.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Issues a redirect (to the user-agent - typically a browser) using
    /// the <see cref="System.Web.HttpResponse.Redirect(string, bool)"/> method.
    /// </p>
    /// </remarks>
    /// <seealso cref="System.Web.HttpResponse.Redirect(string, bool)"/>
        RedirectNoAbort = 3
    }
}
