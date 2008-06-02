#region License

/*
 * Copyright 2002-2004 the original author or authors.
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
using System.Collections.Specialized;
using System.Reflection;
using System.Security.Permissions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using Common.Logging;
using Spring.Collections;
using Spring.Context;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Util;
using Spring.Web.Process;

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
        private readonly ILog Log = LogManager.GetLogger(typeof(PageHandlerFactory));

        private readonly IDictionary pageHandlerWrappers = CollectionsUtil.CreateCaseInsensitiveHashtable();

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

            bool isDebug = Log.IsDebugEnabled;

            if (isDebug) Log.Debug(string.Format("GetHandler():resolving url '{0}'", url));

            IHttpHandler pageHandlerWrapper;

            lock (pageHandlerWrappers.SyncRoot)
            {
                pageHandlerWrapper = (PageHandlerWrapper)pageHandlerWrappers[url];
            }

            if (pageHandlerWrapper != null)
            {
                if (isDebug)
                {
                    Log.Debug(string.Format("GetHandler():resolved url '{0}' from reusable handler cache", url));
                }
            }
            else
            {
                IConfigurableApplicationContext appContext =
                    WebApplicationContext.GetContext(url) as IConfigurableApplicationContext;

                if (appContext == null)
                {
                    throw new InvalidOperationException(
                        "Implementations of IApplicationContext must also implement IConfigurableApplicationContext");
                }

                string appRelativeVirtualPath = WebUtils.GetAppRelativePath(url);
                NamedObjectDefinition namedPageDefinition = FindWebObjectDefinition(appRelativeVirtualPath, appContext.ObjectFactory);

                if (namedPageDefinition != null)
                {
                    Type pageType = namedPageDefinition.ObjectDefinition.ObjectType;
                    if (typeof(IRequiresSessionState).IsAssignableFrom(pageType))
                    {
                        pageHandlerWrapper = new SessionAwarePageHandlerWrapper(appContext, namedPageDefinition.Name, url, null);
                    }
                    else
                    {
                        pageHandlerWrapper = new PageHandlerWrapper(appContext, namedPageDefinition.Name, url, null);
                    }
                }
                else
                {
                    Type pageType = WebObjectUtils.GetCompiledPageType(url);
                    if (typeof(IRequiresSessionState).IsAssignableFrom(pageType))
                    {
                        pageHandlerWrapper = new SessionAwarePageHandlerWrapper(appContext, appRelativeVirtualPath, url, physicalPath);
                    }
                    else
                    {
                        pageHandlerWrapper = new PageHandlerWrapper(appContext, appRelativeVirtualPath, url, physicalPath);
                    }
                }

                if (pageHandlerWrapper.IsReusable)
                {
                    lock (pageHandlerWrappers.SyncRoot)
                    {
                        pageHandlerWrappers[url] = pageHandlerWrapper;
                    }
                }
            }

            return pageHandlerWrapper;
        }
    }

    /// <summary>
    /// Wrapper for handlers that do not require <see cref="HttpSessionState"/>.
    /// </summary>
    /// <remarks>
    /// NOTE: This class has to extend System.Web.UI.Page instead of simply 
    /// implementing IHttpHandler in order for Server.Transfer to work properly.
    /// This in turn requires explicit IHttpHandler implementation in order to
    /// override non-virtual methods from the base Page class.
    /// </remarks>
    internal class PageHandlerWrapper : Page, IHttpHandler
    {
#if NET_2_0
        private static readonly FieldInfo fiHttpContext_CurrentHandler =
            typeof(HttpContext).GetField("_currentHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo miPage_SetPreviousPage =
            typeof(System.Web.UI.Page).GetMethod("SetPreviousPage", BindingFlags.NonPublic | BindingFlags.Instance);
#endif

        private readonly IApplicationContext appContext;
        private readonly string pageId;
        private readonly string url;
        private readonly string path;

        // cache handler if IsReusable == true
        // since we don't use sync, make it volatile
        private volatile IHttpHandler cachedHandler;

        // holds shared state for handlerType
        private Type handlerType;
        private IDictionary handlerState;

        private readonly object syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="PageHandlerWrapper"/> class.
        /// </summary>
        /// <param name="appContext">Application context instance to retrieve page from.</param>
        /// <param name="pageName">Name of the page object to execute.</param>
        /// <param name="url">Requested page URL.</param>
        /// <param name="path">Translated server path for the page.</param>
        public PageHandlerWrapper(IApplicationContext appContext, string pageName, string url, string path)
        {
            this.appContext = appContext;
            this.pageId = pageName;
            this.url = url;
            this.path = path;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageHandlerWrapper"/> class.
        /// </summary>
        /// <param name="appContext">Application context instance to retrieve page from.</param>
        /// <param name="pageName">Name of the page object to execute.</param>
        public PageHandlerWrapper(IApplicationContext appContext, string pageName)
            : this(appContext, pageName, null, null)
        {
        }

        #region Properties

        /// <summary>
        /// Use for sync access to this PageHandler instance.
        /// </summary>
        public object SyncRoot
        {
            get { return syncRoot; }
        }

        /// <summary>
        /// Gets <see cref="IDictionary"/> that contains handler state. 
        /// </summary>
        /// <remarks>
        /// This <see cref="IDictionary"/> will be assigned to the <c>SharedState</c>
        /// property of <see cref="IHttpHandler"/> instances that implement
        /// <see cref="ISharedStateAware"/> interface.
        /// </remarks>
        public IDictionary HandlerState
        {
            get { return handlerState; }
        }

        #endregion

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            IHttpHandler handler = cachedHandler;

            if (handler == null)
            {
                if (path != null)
                {
                    handler = CreatePageInstance();
                }
                else
                {
                    handler = GetOrCreateProcessHandler(context);
                }

                // note, that we don't care about sync here. The last call wins (it's the most current handler instance anyway)
                if (handler.IsReusable) cachedHandler = handler;
            }

            // replace handler proxy on context with "real" handler
            if (this == context.Handler)
            {
                context.Handler = handler;
            }

#if NET_2_0
            // this may happen under load, if GetHandler() 
            // and ProcessRequest() are executed under different threads
            // fix this...
            if (this == context.CurrentHandler)
            {
                fiHttpContext_CurrentHandler.SetValue(context, handler);
            }

            if (handler is System.Web.UI.Page)
            {
                System.Web.UI.Page page = (Page) handler;

                // During Server.Transfer/Execute() the PreviousPage property gets set
                if (this.PreviousPage != null)
                {
                    miPage_SetPreviousPage.Invoke(page, new object[] { this.PreviousPage });
                }
            }
#endif

            ApplySharedState(handler);
            ApplyDependencyInjection(handler);

            handler.ProcessRequest(context);
        }

        /// <summary>
        /// Returns true because this wrapper handler can be reused.
        /// Actual page is instantiated at the beginning of the ProcessRequest method.
        /// </summary>
        bool IHttpHandler.IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Creates a page instance corresponding to this handler's url.
        /// </summary>
        private IHttpHandler CreatePageInstance()
        {
            IHttpHandler handler;
            handler = WebObjectUtils.CreatePageInstance(url);
            if (handler is IApplicationContextAware)
            {
                ((IApplicationContextAware)handler).ApplicationContext = appContext;
            }
            return handler;
        }

        /// <summary>
        /// Gets or - if not found - creates a process handler instance.
        /// </summary>
        private IHttpHandler GetOrCreateProcessHandler(HttpContext context)
        {
            IHttpHandler handler = null;
            string processId = context.Request[AbstractProcessHandler.ProcessIdParamName];
            if (processId != null)
            {
                handler = (IHttpHandler)ProcessManager.GetProcess(processId);
            }

            if (handler == null)
            {
                handler = (IHttpHandler)this.appContext.GetObject(this.pageId);
                if (handler is IProcess)
                {
                    ((IProcess)handler).Start(url);
                }
            }
            return handler;
        }

        /// <summary>
        /// Apply dependency injection stuff on the handler.
        /// </summary>
        /// <param name="handler"></param>
        private void ApplyDependencyInjection(IHttpHandler handler)
        {
            if (handler is Control)
            {
                ControlInterceptor.EnsureControlIntercepted(appContext, (Control)handler);
            }
            else
            {
                if (handler is ISupportsWebDependencyInjection)
                {
                    ((ISupportsWebDependencyInjection)handler).DefaultApplicationContext = appContext;
                }
            }
        }

        /// <summary>
        /// Applies <see cref="HandlerState"/> to the given handler if applicable.
        /// </summary>
        private void ApplySharedState(IHttpHandler handler)
        {
            if (handler is ISharedStateAware)
            {
                CheckIfPageWasRecompiled(handler);
                ((ISharedStateAware)handler).SharedState = this.handlerState;
            }
        }

        /// <summary>
        /// Checks, if page has been recompiled. Creates/discards handlerState if necessary.
        /// </summary>
        /// <param name="handler"></param>
        private void CheckIfPageWasRecompiled(IHttpHandler handler)
        {
            if (handlerType != handler.GetType())
            {
                lock (SyncRoot)
                {
                    if (handlerType != handler.GetType())
                    {
                        // discard old handlerState and cache new pagetype
                        handlerState = new SynchronizedHashtable();
                        handlerType = handler.GetType();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Wrapper for handlers that require <see cref="HttpSessionState"/>.
    /// </summary>
    /// <remarks>
    /// Delays page object instantiation until ProcessRequest is called
    /// in order to be able to access session state.
    /// </remarks>
    internal class SessionAwarePageHandlerWrapper : PageHandlerWrapper, IRequiresSessionState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionAwarePageHandlerWrapper"/> class.
        /// </summary>
        /// <param name="appContext">Application context instance to retrieve page from.</param>
        /// <param name="pageName">Name of the page object to execute.</param>
        /// <param name="url">Requested page URL.</param>
        /// <param name="path">Translated server path for the page.</param>
        public SessionAwarePageHandlerWrapper(IApplicationContext appContext, string pageName, string url, string path)
            : base(appContext, pageName, url, path)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionAwarePageHandlerWrapper"/> class.
        /// </summary>
        /// <param name="appContext">Application context instance to retrieve page from.</param>
        /// <param name="pageName">Name of the page object to execute.</param>
        public SessionAwarePageHandlerWrapper(IApplicationContext appContext, string pageName)
            : base(appContext, pageName)
        {
        }
    }
}