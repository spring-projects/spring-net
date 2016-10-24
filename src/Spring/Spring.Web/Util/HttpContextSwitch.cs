#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

using System;
using System.Web;
using Common.Logging;

#endregion

namespace Spring.Util
{
    /// <summary>
    /// Performs a <see cref="HttpContext.RewritePath(string)"/>. Original path will be restored on <see cref="Dispose"/>
    /// </summary>
    /// <remarks>
    /// Rewrites the current HttpContext's filepath to &lt;directory&gt;/currentcontext.dummy.<br/>
    /// This affects resolving resources by calls to <see cref="Spring.Util.ConfigurationUtils.GetSection"/> and <see cref="HttpRequest.MapPath(string)"/><br/>
    /// Original path is restored during <see cref="HttpContextSwitch.Dispose"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// using( new HttpContextSwitch( "/path" ) )
    /// {
    ///     Response.Write( Request.FilePath ); // writes "/path/currentcontext.dummy" to response.
    /// }
    /// // Request.FilePath has been reset to original url here
    /// </code>
    /// </example>
    /// <author>Erich Eichinger</author>
    public class HttpContextSwitch : IDisposable
    {
        private readonly IDisposable rewriteContext;
        private static readonly ILog log = LogManager.GetLogger(typeof(HttpContextSwitch));

//        /// <summary>
//        /// Performs an immediate call to <see cref="HttpContext.RewritePath(string)"/>
//        /// </summary>
//        /// <param name="virtualDirectory">a directory path (without trailing filename!)</param>
//        public HttpContextSwitch( string virtualDirectory )
//        {
//            HttpContext currentContext = HttpContext.Current;
//            if (currentContext == null) return; // no webrequest
//
//            virtualDirectory = WebUtils.GetVirtualDirectory(virtualDirectory);
//            string currentFileDirectory = WebUtils.GetVirtualDirectory(currentContext.Request.FilePath);
//            // only switch path if necessary
//            if (string.Compare( virtualDirectory, currentFileDirectory, true ) != 0)
//            {
//                savedContext = currentContext;
//                originalUrl = savedContext.Request.Url.PathAndQuery;
//                string newPath = virtualDirectory + "currentcontext.dummy";
//#if NET_2_0
//                savedContext.RewritePath( newPath, false );
//#else
//                savedContext.RewritePath( newPath );
//#endif
//                if (log.IsDebugEnabled) log.Debug("rewriting path from " + originalUrl + " to " + newPath + " results in " + savedContext.Request.FilePath);
//            }
//        }

        /// <summary>
        /// Performs an immediate call to <see cref="HttpContext.RewritePath(string)"/>
        /// </summary>
        /// <param name="virtualDirectory">a directory path (without trailing filename!)</param>
        public HttpContextSwitch(string virtualDirectory)
        {
            rewriteContext = VirtualEnvironment.RewritePath(virtualDirectory, false);

//            string currentFileDirectory = WebUtils.GetVirtualDirectory(VirtualEnvironment.CurrentVirtualFilePath);
//            // only switch path if necessary
//            if (string.Compare(virtualDirectory, currentFileDirectory, true) != 0)
//            {
//                originalUrl = VirtualEnvironment.CurrentVirtualPathAndQuery;
//                string newPath = virtualDirectory + "currentcontext.dummy";
//                VirtualEnvironment.RewritePath(newPath, false);
//
//                #region Instrumentation
//
//                if (log.IsDebugEnabled)
//                {
//                    log.Debug("rewriting path from " + originalUrl + " to " + newPath + " results in " + VirtualEnvironment.CurrentVirtualFilePath);
//                }
//
//                #endregion
//            }
        }

        /// <summary>
        /// Restores original path if necessary
        /// </summary>
        public void Dispose()
        {
            rewriteContext.Dispose();
//            if (rewriteContext != null)
//            {
//                VirtualEnvironment.RewritePath(originalUrl, false);
//
//                #region Instrumentation
//
//                if (log.IsDebugEnabled)
//                {
//                    log.Debug("restoring original path from " + VirtualEnvironment.CurrentVirtualFilePath + " back to " + originalUrl);
//                }
//
//                #endregion
//            }
        }

//        /// <summary>
//        /// Restores original path if necessary
//        /// </summary>
//        public void Dispose()
//        {
//            if (originalUrl != null)
//            {
//                HttpContext context = HttpContext.Current;
//#if NET_2_0
//                context.RewritePath( originalUrl, false );
//#else
//                context.RewritePath( originalUrl );
//#endif
//
//                #region Instrumentation
//
//                if (log.IsDebugEnabled)
//                {
//                    log.Debug("restoring original path from " + context.Request.FilePath + " back to " + originalUrl);
//                }
//
//                #endregion
//            }
//        }
    }
}