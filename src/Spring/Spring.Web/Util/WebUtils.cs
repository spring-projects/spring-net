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

using System.Web;
using System.Web.UI;

#endregion

namespace Spring.Util
{
    /// <summary>
    /// Miscellaneous web utility methods.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public sealed class WebUtils
    {
        #region Constructor (s) / Destructor

        // CLOVER:OFF

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.Util.WebUtils"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is a utility class, and as such exposes no public constructors.
        /// </p>
        /// </remarks>
        private WebUtils()
        {}

        // CLOVER:ON

        #endregion

        /// <summary>
        /// Default protocol used for resolving resources in web applications
        /// </summary>
        internal static readonly string DEFAULT_RESOURCE_PROTOCOL = "web";

        /// <summary>
        /// Extracts the bare ASPX page name without any extension from the
        /// supplied <paramref name="url"/>.
        /// </summary>
        /// <example>
        /// <p>
        /// Examples of what would be returned from this method given a url would be:
        /// </p>
        /// <p>
        /// <list type="bullet">
        /// <item><description>'Login.aspx' => 'Login'</description></item>
        /// <item><description>'~/Login.aspx' => 'Login'</description></item>
        /// <item><description>'~/B2B/SignUp.aspx' => 'SignUp'</description></item>
        /// <item><description>'B2B/Foo/FooServices.aspx' => 'FooServices'</description></item>
        /// </list>
        /// </p>
        /// </example>
        /// <param name="url">The full URL to the ASPX page.</param>
        /// <returns>
        /// The bare ASPX page name without any extension.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="url"/> is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        public static string GetPageName(string url)
        {
            AssertUtils.ArgumentHasText(url, "url");
            int lastSlash = url.LastIndexOf('/');
            int lastDot = url.LastIndexOf('.');
            if (lastDot < lastSlash)
            {
                lastDot = -1;
            }
            if (lastDot < 0)
            {
                int length = url.Length - lastSlash - 1;
                return url.Substring(lastSlash + 1, length);
            }

            return url.Substring(lastSlash + 1, lastDot - lastSlash - 1);
        }

        /// <summary>
        /// Returns only the directory portion of a virtual path
        /// </summary>
        /// <remarks>
        /// The returned path is guaranteed to always have a leading and a trailing slash.<br/>
        /// If a path does not end with a file-extension it is assumed to be a directory
        /// </remarks>
        public static string GetVirtualDirectory(string virtualPath)
        {
            AssertUtils.ArgumentNotNull(virtualPath, "virtualPath");

            if (virtualPath.Length == 0)
            {
                return "/";
            }
            
            if (virtualPath[0] != '/')
            {
                virtualPath = "/" + virtualPath;
            }

            if (virtualPath[virtualPath.Length - 1] != '/')
            {
                int iDot = virtualPath.LastIndexOf('.');
                int iSlash = virtualPath.LastIndexOf('/');
                if (iDot < iSlash) iDot = -1;
                if (iDot > -1)
                {
                    virtualPath = virtualPath.Substring(0, iSlash + 1);
                }
                else
                {
                    virtualPath = virtualPath + "/";
                }
            }
            return virtualPath;
        }

        /// <summary>
        /// Returns absolute path that can be referenced within plain HTML.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If relative path starts with '/' (forward slash), no concatenation will occur
        /// and it will be assumed that the relative path specified is indeed the absolute path
        /// and will be returned verbatim.</p>
        /// <p>
        /// Otherwise, relative path will be appended to the application path, while making sure that 
        /// path separators are not duplicated between the paths.</p>
        /// </remarks>
        /// <param name="applicationPath">Application path.</param>
        /// <param name="relativePath">Relative path to combine with the application path.</param>
        /// <returns>Absolute path.</returns>
        public static string CreateAbsolutePath(string applicationPath, string relativePath)
        {
            if (StringUtils.HasLength(relativePath))
            {
                if (relativePath.ToLower().StartsWith("http://") || relativePath.ToLower().StartsWith("https://"))
                {
                    return relativePath;
                }

                if (relativePath.StartsWith("/"))
                {
                    return relativePath;
                }

                if (relativePath.StartsWith("~/"))
                {
                    relativePath = relativePath.Substring(2);
                }
            }

            applicationPath = (applicationPath == null) ? "" : applicationPath.TrimEnd('/');

            return string.Format("{0}/{1}", applicationPath, relativePath);
        }

        /// <summary>
        /// Combines a rooted base path with a relative path.
        /// </summary>
        /// <param name="rootPath">Must be a path starting with '/'</param>
        /// <param name="relativePath">the path to be combined. May start with basepath Placeholder '~'</param>
        /// <returns>the combined path</returns>
        /// <remarks>
        /// If relativePath starts with '~', rootPath is ignored and '~' resolves to the current AppDomain's application virtual path<br/>
        /// If relativePath start with '/', rootPath is ignored and relativePath is returned as-is.
        /// </remarks>
        public static string CombineVirtualPaths(string rootPath, string relativePath)
        {
            AssertUtils.ArgumentHasText(rootPath, "rootPath");
            if (rootPath[0] != '/')
            {
                throw new ArgumentException("RootPath must start with '/'", "rootPath");
            }

            string combinedPath = relativePath;

            if (combinedPath.StartsWith("~/"))
            {
                combinedPath = VirtualEnvironment.ApplicationVirtualPath.TrimEnd('/') + relativePath.Substring(1);
            }

            if (!combinedPath.StartsWith("/"))
            {
                combinedPath = GetVirtualDirectory(rootPath).TrimEnd('/') + '/' + relativePath;
            }

            // TODO: reduce contained directory upwalks here

            return combinedPath;
        }

        /// <summary>
        /// Gets the application-relative virtual path portion of the given absolute URL.
        /// </summary>
        /// <param name="url">the absolute url</param>
        /// <returns>the url relative to the current application's virtual path</returns>
        public static string GetAppRelativePath(string url)
        {
            string appPath = VirtualEnvironment.ApplicationVirtualPath;
            return GetRelativePath(appPath, url);
        }

        /// <summary>
        /// Gets a normalized application-relative virtual path of the given virtual path. 
        /// </summary>
        /// <example>
        /// <p>
        /// Examples of what would be returned from this method given a virtual path would be:
        /// </p>
        /// <p>
        /// <list type="bullet">
        /// <item><description>'Login.aspx' => 'Login.aspx'</description></item>
        /// <item><description>'~/Login.aspx' => '/Login.aspx'</description></item>
        /// <item><description>'~/B2B/SignUp.aspx' => '/B2B/SignUp.aspx'</description></item>
        /// <item><description>'B2B/Foo/FooServices.aspx' => 'B2B/Foo/FooServices.aspx'</description></item>
        /// </list>
        /// </p>
        /// </example>
        /// <param name="virtualPath">the virtual path.</param>
        /// <returns>the normalized virtual path</returns>
        public static string GetNormalizedVirtualPath(string virtualPath)
        {
            if(String.IsNullOrEmpty(virtualPath))
            {
                return virtualPath;
            }
            return virtualPath.StartsWith("~/") ? virtualPath.Substring(1) : virtualPath;
        }

        /// <summary>
        /// Gets the virtual path portion of the given absolute URL 
        /// relative to the given base path.
        /// </summary>
        /// <remarks>
        /// Base path comparison is done case insensitive.
        /// </remarks>
        /// <param name="basePath">the absolute base path</param>
        /// <param name="url">the absolute url</param>
        /// <returns>the url relative to the given basePath</returns>
        public static string GetRelativePath(string basePath, string url)
        {
            // strip application path from url
            string appPath = basePath.TrimEnd('/');
            string appRelativeVirtualPath = url;
            if (appRelativeVirtualPath.ToLower().StartsWith(appPath.ToLower()))
            {
                appRelativeVirtualPath = appRelativeVirtualPath.Substring(appPath.Length);
            }
            return appRelativeVirtualPath;            
        }

        ///<summary>
        /// Returns the 'logical' parent of the specified control. Technically when dealing with masterpages and control hierarchy,
        /// the order goes controls-&gt;masterpage-&gt;page. But one often wants the more logical order controls-&gt;page-&gt;masterpage.
        ///</summary>
        ///<param name="control">the control, who's parent is to be determined.</param>
        ///<returns>the logical parent or <c>null</c> if the top of the hierarchy is reached.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="control"/> is <c>null</c></exception>
        public static Control GetLogicalParent(Control control)
        {
            AssertUtils.ArgumentNotNull(control, "control");

            // to determine "correct" order of bubbling control->page->masterpage, 
            // the trick below is necessary because technically the hierarchy goes 
            // control->masterpage->page
            if (control is Page)
            {
                control = ((Page)control).Master;
            }
            else if (IsMaster(control))
            {
                control = null;
            }
            else if (IsMaster(control.Parent))
            {
                control = control.Page;  
            }
            else
            {
                control = control.Parent;
            }
            return control;
        }

        private static bool IsMaster(Control control)
        {
            return (control is MasterPage);
        }

        /// <summary>
        /// Encode <paramref name="value"/> for use in URLs.
        /// </summary>
        /// <param name="value">the text to be encoded.</param>
        /// <returns>the url-encoded <paramref name="value"/></returns>
        /// <remarks>
        /// This method may be used outside of a current request. If executed within a 
        /// request, <see cref="HttpServerUtility.UrlEncode(string)"/> is used.
        /// <see cref="HttpUtility.UrlEncode(string)"/> will be used otherwise.
        /// </remarks>
        public static string UrlEncode( string value )
        {
            HttpContext ctx = HttpContext.Current;
            return (ctx == null) ? HttpUtility.UrlEncode( value ) : ctx.Server.UrlEncode( value );
        }
    }
}
