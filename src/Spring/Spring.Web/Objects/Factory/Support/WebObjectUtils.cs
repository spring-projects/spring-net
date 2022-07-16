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

using Common.Logging;
using Spring.Util;
using IHttpHandler = System.Web.IHttpHandler;
using HttpException = System.Web.HttpException;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Miscellaneous utility methods to support web functionality within Spring.Objects
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public sealed class WebObjectUtils
    {
        private static ILog s_log = LogManager.GetLogger( typeof( WebObjectUtils ) );

        // CLOVER:OFF

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.Util.WebUtils"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is a utility class, and as such exposes no public constructors.
        /// </p>
        /// </remarks>
        private WebObjectUtils()
        { }

        // CLOVER:ON


        /// <summary>
        /// Creates an instance of the ASPX page
        /// referred to by the supplied <paramref name="pageUrl"/>.
        /// </summary>
        /// <param name="pageUrl">
        /// The URL of the ASPX page.
        /// </param>
        /// <returns>Page instance.</returns>
        /// <exception cref="Spring.Objects.Factory.ObjectCreationException">
        /// If this method is not called in the scope of an active web session
        /// (i.e. the implementation this method depends on this code executing
        /// in the environs of a running web server such as IIS); or if the
        /// page could not be instantiated (for whatever reason, such as the
        /// ASPX <paramref name="pageUrl"/> not actually existing).
        /// </exception>
        public static IHttpHandler CreatePageInstance( string pageUrl )
        {
            if (s_log.IsDebugEnabled)
            {
                s_log.Debug( "creating page instance '" + pageUrl + "'" );
            }

            IHttpHandler page;
            try
            {
                page = CreateHandler( pageUrl );
            }
            catch (HttpException httpEx)
            {
                string msg = String.Format( "Unable to instantiate page [{0}]: {1}", pageUrl, httpEx.Message );
                if (httpEx.GetHttpCode() == 404)
                {
                    throw new FileNotFoundException( msg );
                }
                s_log.Error( msg, httpEx );
                throw new ObjectCreationException( msg, httpEx );
            }
            catch (Exception ex)
            {
                // in case of FileNotFound recreate the exception for clarity
                FileNotFoundException fnfe = ex as FileNotFoundException;
                if (fnfe != null)
                {
                    string fmsg = String.Format( "Unable to instantiate page [{0}]: The file '{1}' does not exist.", pageUrl, fnfe.Message );
                    throw new FileNotFoundException( fmsg );
                }

                string msg = String.Format( "Unable to instantiate page [{0}]", pageUrl );
                s_log.Error( msg, ex );
                throw new ObjectCreationException( msg, ex );
            }
            return page;
        }

        /// <summary>
        /// Creates the raw handler instance without any exception handling
        /// </summary>
        /// <param name="pageUrl"></param>
        /// <returns></returns>
        internal static IHttpHandler CreateHandler( string pageUrl )
        {
            return VirtualEnvironment.CreateInstanceFromVirtualPath(pageUrl, typeof(IHttpHandler)) as IHttpHandler;
        }

        /// <summary>
        /// Returns the <see cref="System.Type"/> of the ASPX page
        /// referred to by the supplied <paramref name="pageUrl"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// As indicated by the exception that can be thrown by this method,
        /// the ASPX page referred to by the supplied <paramref name="pageUrl"/>
        /// does have to be instantiated in order to determine its
        /// see cref="System.Type"/>
        /// </p>
        /// </remarks>
        /// <param name="pageUrl">
        /// The filename of the ASPX page.
        /// </param>
        /// <returns>
        /// The <see cref="System.Type"/> of the ASPX page
        /// referred to by the supplied <paramref name="pageUrl"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="pageUrl"/> is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectCreationException">
        /// If this method is not called in the scope of an active web session
        /// (i.e. the implementation this method depends on this code executing
        /// in the environs of a running web server such as IIS); or if the
        /// page could not be instantiated (for whatever reason, such as the
        /// ASPX <paramref name="pageUrl"/> not actually existing).
        /// </exception>
        public static Type GetPageType( string pageUrl )
        {
            AssertUtils.ArgumentHasText( pageUrl, "pageUrl" );

//            HttpContext ctx = HttpContext.Current;
//            if (ctx == null)
//            {
//                throw new ObjectCreationException( "Unable to get page type. HttpContext is not defined." );
//            }

            try
            {
                Type pageType = GetCompiledPageType( pageUrl );
                return pageType;
            }
            catch (Exception ex)
            {
                string msg = String.Format( "Unable to get page type for url [{0}]", pageUrl );
                s_log.Error( msg, ex );
                throw new ObjectCreationException( msg, ex );
            }
        }

        /// <summary>
        /// Calls the underlying ASP.NET infrastructure to obtain the compiled page type
        /// relative to the current <see cref="System.Web.HttpRequest.CurrentExecutionFilePath"/>.
        /// </summary>
        /// <param name="pageUrl">
        /// The filename of the ASPX page relative to the current <see cref="System.Web.HttpRequest.CurrentExecutionFilePath"/>
        /// </param>
        /// <returns>
        /// The <see cref="System.Type"/> of the ASPX page
        /// referred to by the supplied <paramref name="pageUrl"/>.
        /// </returns>
        public static Type GetCompiledPageType( string pageUrl )
        {
            if (s_log.IsDebugEnabled)
            {
                s_log.Debug( "getting page type for " + pageUrl );
            }

            string rootedVPath = WebUtils.CombineVirtualPaths( VirtualEnvironment.CurrentExecutionFilePath, pageUrl );
            if (s_log.IsDebugEnabled)
            {
                s_log.Debug( "page vpath is " + rootedVPath );
            }

            Type pageType = VirtualEnvironment.GetCompiledType(rootedVPath);
            if (s_log.IsDebugEnabled)
            {
                s_log.Debug( string.Format( "got page type '{0}' for vpath '{1}'", pageType.FullName, rootedVPath ) );
            }
            return pageType;
        }


        /// <summary>
        /// Gets the controls type from a given filename
        /// </summary>
        public static Type GetControlType( string controlName )
        {
            AssertUtils.ArgumentHasText( controlName, "controlName" );
            if (s_log.IsDebugEnabled)
            {
                s_log.Debug( "getting control type for " + controlName );
            }

//            HttpContext ctx = HttpContext.Current;
//            if (ctx == null)
//            {
//                throw new ObjectCreationException( "Unable to get control type. HttpContext is not defined." );
//            }

            string rootedVPath = WebUtils.CombineVirtualPaths( VirtualEnvironment.CurrentExecutionFilePath, controlName );

            if (s_log.IsDebugEnabled)
            {
                s_log.Debug( "control vpath is " + rootedVPath );
            }

            Type controlType;
            try
            {
                controlType = VirtualEnvironment.GetCompiledType(rootedVPath);
            }
            catch (HttpException httpEx)
            {
                // for better error-handling suppress 404 HttpExceptions here
                if (httpEx.GetHttpCode() == 404)
                {
                    throw new FileNotFoundException( string.Format( "Control '{0}' does not exist", rootedVPath ) );
                }
                throw;
            }

            if (s_log.IsDebugEnabled)
            {
                s_log.Debug( string.Format( "got control type '{0}' for vpath '{1}'", controlType.FullName, rootedVPath ) );
            }
            return controlType;
        }
    }
}
