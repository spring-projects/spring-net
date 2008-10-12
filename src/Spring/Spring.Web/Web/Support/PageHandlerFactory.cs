#region License

/*
 * Copyright 2002-2008 the original author or authors.
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
using System.Collections;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using Spring.Context;
using Spring.Context.Support;
using Spring.Objects;
using Spring.Objects.Factory.Support;
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// Implementation of <see cref="System.Web.IHttpHandlerFactory"/> that retrieves
    /// configured <see cref="IHttpHandler"/> instances from Spring web application context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This handler factory uses the page name from the URL, without the extension,
    /// to find the handler object in the Spring context. This means that the target object 
    /// definition doesn't need to resolve to an .aspx page -- it can be any valid 
    /// object that implements <see cref="IHttpHandler"/> interface.
    /// </para>
    /// <para>
    /// If the specified page is not found in the Spring application context, this
    /// handler factory falls back to the standard ASP.NET behavior and tries 
    /// to find physical page with a given name.
    /// </para>
    /// <para>
    /// In either case, handlers that implement <see cref="IApplicationContextAware"/>
    /// and <see cref="ISharedStateAware"/> will be provided with the references
    /// to appropriate Spring.NET application context (based on the request URL) 
    /// and a <see cref="IDictionary"/> that should be used to store the information
    /// that needs to be shared by all instances of the handler.
    /// </para>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class PageHandlerFactory : AbstractHandlerFactory
    {
        /// <summary>
        /// Retrieves instance of the configured page from Spring web application context, 
        /// or if page is not defined in Spring config file tries to find it using standard
        /// ASP.Net mechanism.
        /// </summary>
        /// <param name="context">Current HttpContext</param>
        /// <param name="requestType">Type of HTTP request (GET, POST, etc.)</param>
        /// <param name="url">Requested page URL</param>
        /// <param name="physicalPath">Translated server path for the page</param>
        /// <returns>Instance of the IHttpHandler object that should be used to process request.</returns>
        public override IHttpHandler GetHandler(HttpContext context, string requestType, string url, string physicalPath)
        {
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();

            return base.GetHandler(context, requestType, url, physicalPath);
        }

        /// <summary>
        /// Create a handler instance for the given URL.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> instance for this request.</param>
        /// <param name="requestType">The HTTP data transfer method (GET, POST, ...)</param>
        /// <param name="url">The requested <see cref="HttpRequest.RawUrl"/>.</param>
        /// <param name="physicalPath">The physical path of the requested resource.</param>
        /// <returns>A handler instance for the current request.</returns>
        protected override IHttpHandler CreateHandlerInstance(HttpContext context, string requestType, string url, string physicalPath)
        {
            IHttpHandler handler;
            IConfigurableApplicationContext appContext = GetCheckedApplicationContext(url);

            if (appContext == null)
            {
                throw new InvalidOperationException("PageHandlerFactory requires an IConfigurableApplicationContext");
            }

            string appRelativeVirtualPath = WebUtils.GetAppRelativePath(url);
            NamedObjectDefinition namedPageDefinition = FindWebObjectDefinition(appRelativeVirtualPath, appContext.ObjectFactory);

            if (namedPageDefinition != null)
            {
                // is this a nested call (HttpServerUtility.Transfer() or HttpServerUtility.Execute())?
                if (context.Handler != null)
                {
                    // all deps can/must be resolved now
                    handler = (IHttpHandler)appContext.GetObject(namedPageDefinition.Name, typeof(IHttpHandler), null);
                }
                else
                {
                    // execution pipeline "entry-point" - create page instance only 
                    // and defer configuration to PreRequestHandlerExecute step
                    handler = (IHttpHandler)appContext.CreateObject(namedPageDefinition.Name, typeof(IHttpHandler), null);
                }
                WebSupportModule.SetCurrentHandlerConfiguration(appContext, namedPageDefinition.Name, true);
            }
            else
            {
                handler = WebObjectUtils.CreateHandler(context, url);

                // is this a nested call (HttpServerUtility.Transfer() or HttpServerUtility.Execute())?
                if (context.Handler != null)
                {
                    // apply ObjectPostProcessors now
                    handler = WebSupportModule.ConfigureHandler(handler, appContext, url, false);
                }
                else
                {
                    // execution pipeline "entry-point" - create page instance only 
                    // and defer configuration to PreRequestHandlerExecute step
                    WebSupportModule.SetCurrentHandlerConfiguration(appContext, url, false);
                }
            }

            ApplyDependencyInjectionInfrastructure(handler, appContext);

            return handler;
        }

        /// <summary>
        /// Apply dependency injection stuff on the handler.
        /// </summary>
        /// <param name="handler">the handler to be intercepted</param>
        /// <param name="applicationContext">the context responsible for configuring this handler</param>
        private static void ApplyDependencyInjectionInfrastructure(IHttpHandler handler, IApplicationContext applicationContext)
        {
            if (handler is Control)
            {
                ControlInterceptor.EnsureControlIntercepted(applicationContext, (Control)handler);
            }
            else
            {
                if (handler is ISupportsWebDependencyInjection)
                {
                    ((ISupportsWebDependencyInjection)handler).DefaultApplicationContext = applicationContext;
                }
            }
        }
    }
}
