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

using System.Globalization;
using System.Reflection;
using System.Security;
using System.Web;
using System.Web.Caching;
using System.Web.Routing;
using System.Web.SessionState;
using System.Web.UI;

using Common.Logging;
using Spring.Core.IO;
using Spring.Core.TypeConversion;
using Spring.Core.TypeResolution;
using Spring.Expressions;
using Spring.Reflection.Dynamic;
using Spring.Threading;
using Spring.Util;
using Spring.Web.Support;

namespace Spring.Context.Support
{
    /// <summary>
    /// Provides various support for proper handling requests.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class WebSupportModule : IHttpModule
    {
        /// <summary>
        /// Identifies the Objectdefinition used for the current IHttpHandler instance in TLS
        /// </summary>
        private static readonly string CURRENTHANDLER_OBJECTDEFINITION = "__spring.web" + new Guid().ToString();

        /// <summary>
        /// Holds the handler configuration information.
        /// </summary>
        private class HandlerConfigurationMetaData
        {
            public readonly IConfigurableApplicationContext ApplicationContext;
            public readonly string ObjectDefinitionName;
            public readonly bool IsContainerManaged;

            public HandlerConfigurationMetaData(IConfigurableApplicationContext applicationContext, string objectDefinitionName, bool isContainerManaged)
            {
                ApplicationContext = applicationContext;
                ObjectDefinitionName = objectDefinitionName;
                IsContainerManaged = isContainerManaged;
            }
        }

        private static readonly ILog s_log;

        private static bool s_isInitialized = false;

        // Required for Session End event handling
        private static int CACHEKEYPREFIXLENGTH = 0;
        private static CacheItemRemovedCallback s_originalCallback;

        // required to enable accessing HttpContext.Request during IHttpModule.Init() in integrated mode
        private static readonly FieldInfo fiHideRequestResponse;
        private static readonly SafeField ContextHideRequestResponse;

        /// <summary>
        /// For webapplications always
        /// <ul>
        /// <li>convert IResources using the current context.</li>
        /// <li>use "web" as default resource protocol</li>
        /// <li>use <see cref="HybridContextStorage"/> as default threading storage</li>
        /// </ul>
        /// </summary>
        static WebSupportModule()
        {
            s_log = LogManager.GetLogger(typeof(WebSupportModule));

            // required to enable accessing HttpContext.Request during IHttpModule.Init() in integrated mode
            ContextHideRequestResponse = null;
            try
            {
                fiHideRequestResponse = typeof(HttpContext).GetField("HideRequestResponse", BindingFlags.Instance|BindingFlags.NonPublic);
//                fiHideRequestResponse.SetValue(HttpContext.Current, false);
                ContextHideRequestResponse = (fiHideRequestResponse!=null)?new SafeField(fiHideRequestResponse):null;
            }
            catch(SecurityException sec)
            {
                s_log.Warn(string.Format("failed reflecting field HttpContext.HideRequestResponse due to security restrictions {0}", sec));
            }

            // register additional resource handler
            ResourceHandlerRegistry.RegisterResourceHandler(WebUtils.DEFAULT_RESOURCE_PROTOCOL, typeof(WebResource));
            // replace default IResource converter
            TypeConverterRegistry.RegisterConverter(typeof(IResource),
                                                    new ResourceConverter(
                                                        new ConfigurableResourceLoader(WebUtils.DEFAULT_RESOURCE_PROTOCOL)));
            // default to hybrid thread storage implementation
            LogicalThreadContext.SetStorage(new HybridContextStorage());

            s_log.Debug("Set default resource protocol to 'web' and installed HttpContext-aware HybridContextStorage");
        }

        /// <summary>
        /// Registers this module for all events required by the Spring.Web framework
        /// </summary>
        public virtual void Init(HttpApplication app)
        {
            lock (typeof(WebSupportModule))
            {
                s_log.Debug("Initializing Application instance");
                if (!s_isInitialized)
                {
                    HttpModuleCollection modules = app.Modules;
                    foreach (string moduleKey in modules.AllKeys)
                    {
                        if (modules[moduleKey] is SessionStateModule)
                        {
                            HookSessionEvent((SessionStateModule)modules[moduleKey]);
                        }
                    }
                }
                s_isInitialized = true;

                // signal, that VirtualEnvironment is ready to accept
                // handler registrations for EndRequest and EndSession events
                VirtualEnvironment.SetInitialized();
            }

            app.PreRequestHandlerExecute += OnConfigureHandler;
            app.EndRequest += VirtualEnvironment.RaiseEndRequest;

            // TODO: this is only a workaround to get us up & running in IIS7/integrated mode
            // We must review all code for relative virtual paths - they must be resolved to application-relative paths
            // during parsing of the object definitions
            bool hideRequestResponse = false;
            if (ContextHideRequestResponse != null)
            {
                hideRequestResponse = (bool)ContextHideRequestResponse.GetValue(app.Context);
                ContextHideRequestResponse.SetValue(app.Context, false);
            }

            try
            {
                // ensure context is instantiated
                IConfigurableApplicationContext appContext = WebApplicationContext.GetRootContext() as IConfigurableApplicationContext;
                // configure this app + it's module instances
                if (appContext == null)
                {
                    throw new InvalidOperationException("Implementations of IApplicationContext must also implement IConfigurableApplicationContext");
                }

                HttpApplicationConfigurer.Configure(appContext, app);
            }
            finally
            {
                if (ContextHideRequestResponse!=null) ContextHideRequestResponse.SetValue(app.Context, hideRequestResponse);
            }
        }

        #region IHttpHandler configuration

        /// <summary>
        /// Configures the current IHttpHandler as specified by <see cref="Spring.Web.Support.PageHandlerFactory" />. If the
        /// <see cref="Spring.Web.Support.PageHandlerFactory" /> is not executed for the current request and an instance of
        /// <see cref="Page" /> is served revalidate if the instance should be configured.
        /// </summary>
        private void OnConfigureHandler(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            HandlerConfigurationMetaData hCfg = (HandlerConfigurationMetaData)LogicalThreadContext.GetData(CURRENTHANDLER_OBJECTDEFINITION);
            if (hCfg != null)
            {
                // app.Context.Handler = // TODO: check, if this makes sense (EE)
                ConfigureHandlerNow(app.Context.Handler, hCfg.ApplicationContext, hCfg.ObjectDefinitionName, hCfg.IsContainerManaged);
            }
            else
            {
                Page page = app.Context.Handler as Page;
                if (!IsPageWithRouteHandler(page))
                {
                    return;
                }

                // In case of Routing pages are not handled by the PageHandlerFactory therefore no HandlerConfigurationMetaData
                // is set.
                IConfigurableApplicationContext applicationContext = (IConfigurableApplicationContext)WebApplicationContext.Current;
                string normalizedVirtualPath = WebUtils.GetNormalizedVirtualPath(page.AppRelativeVirtualPath);

                ControlInterceptor.EnsureControlIntercepted(applicationContext, page);
                ConfigureHandlerNow(page, applicationContext, normalizedVirtualPath, true);
            }
        }

        /// <summary>
        /// Determines whether the specified page is processed by a <see cref="PageRouteHandler" />.
        /// </summary>
        /// <param name="page">the page.</param>
        /// <returns>whether the page has a page route assigned</returns>
        private static bool IsPageWithRouteHandler(Page page)
        {
            return page != null && page.RouteData != null && page.RouteData.RouteHandler != null;
        }

        /// <summary>
        /// Configures the specified handler instance using the object definition <paramref name="name"/>.
        /// </summary>
        /// <remarks>
        /// TODO
        /// </remarks>
        /// <param name="context"></param>
        /// <param name="handler"></param>
        /// <param name="applicationContext"></param>
        /// <param name="name"></param>
        /// <param name="isContainerManaged"></param>
        /// <returns></returns>
        public static IHttpHandler ConfigureHandler(HttpContext context, IHttpHandler handler, IConfigurableApplicationContext applicationContext, string name, bool isContainerManaged)
        {
            if (context.Handler != null)
            {
                s_log.Debug(string.Format("previous handler is present - configuring handler now using application context '{0}' and name '{1}'", applicationContext, name));
                // this is a Server.Execute() or Server.Transfer() request -> configure immediately
                return ConfigureHandlerNow(handler, applicationContext, name, isContainerManaged);
            }
            else
            {
                // remember the resolved object definition name for applying it during PreRequestHandlerExecute
                s_log.Debug(string.Format("no previous handler is present - defer handler configuration using application context '{0}' and name '{1}'", applicationContext, name));
                SetCurrentHandlerConfiguration(applicationContext, name, isContainerManaged);
                return handler;
            }
        }

        ///<summary>
        /// TODO
        ///</summary>
        ///<param name="applicationContext"></param>
        ///<param name="name"></param>
        ///<param name="isContainerManaged"></param>
        private static void SetCurrentHandlerConfiguration(IConfigurableApplicationContext applicationContext, string name, bool isContainerManaged)
        {
            LogicalThreadContext.SetData(CURRENTHANDLER_OBJECTDEFINITION, new HandlerConfigurationMetaData(applicationContext, name, isContainerManaged));
        }

        ///<summary>
        /// TODO
        ///</summary>
        ///<param name="handler"></param>
        ///<param name="applicationContext"></param>
        ///<param name="name"></param>
        ///<param name="isContainerManaged"></param>
        private static IHttpHandler ConfigureHandlerNow(IHttpHandler handler, IConfigurableApplicationContext applicationContext, string name, bool isContainerManaged)
        {
            if (isContainerManaged)
            {
                s_log.Debug(string.Format("configuring managed handler using application context '{0}' and name '{1}'", applicationContext, name));
                handler = (IHttpHandler)applicationContext.ObjectFactory.ConfigureObject(handler, name);
            }
            else
            {
                s_log.Debug(string.Format("configuring unmanaged handler using application context '{0}' and name '{1}'", applicationContext, name));
                // at a minimum we'll apply ObjectPostProcessors
                handler = (IHttpHandler)applicationContext.ObjectFactory.ApplyObjectPostProcessorsBeforeInitialization(handler, name);
                handler = (IHttpHandler)applicationContext.ObjectFactory.ApplyObjectPostProcessorsAfterInitialization(handler, name);
            }

            return handler;
        }

        #endregion

        /// <summary>
        /// Disposes this instance
        /// </summary>
        public virtual void Dispose()
        {
            // noop
        }

        #region Session Handling Stuff

        private static void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            s_log.Debug("end session " + key + " because of " + reason);

            try
            {
                HttpSessionState ss = CreateSessionState(key, value);

                VirtualEnvironment.RaiseEndSession(ss, reason);
            }
            catch (Exception ex)
            {
                string msg = "Failure during EndSession event handling";
                // are we on a current request?
                if (HttpContext.Current != null)
                {
                    s_log.Error(msg, ex);
                }
                else
                {
                    // this is an async session timeout - log as fatal since this is the thread's exit point!
                    s_log.Fatal(msg, ex);
                }
            }
            finally
            {
                if (s_originalCallback != null)
                {
                    s_originalCallback(key, value, reason);
                }
            }
        }

        private static void HookSessionEvent(SessionStateModule sessionStateModule)
        {
            // Hook only into InProcState - all others ignore SessionEnd anyway
            object store = ExpressionEvaluator.GetValue(sessionStateModule, "_store");
            if ((store != null) && store.GetType().Name == "InProcSessionStateStore")
            {
                s_log.Debug("attaching to InProcSessionStateStore");
                s_originalCallback = (CacheItemRemovedCallback)ExpressionEvaluator.GetValue(store, "_callback");
                ExpressionEvaluator.SetValue(store, "_callback", new CacheItemRemovedCallback(OnCacheItemRemoved));

                CACHEKEYPREFIXLENGTH = (int)ExpressionEvaluator.GetValue(store, "CACHEKEYPREFIXLENGTH");
            }
        }

        private static HttpSessionState CreateSessionState(string key, object state)
        {
            string id = key.Substring(CACHEKEYPREFIXLENGTH);
            ISessionStateItemCollection sessionItems =
                (ISessionStateItemCollection)ExpressionEvaluator.GetValue(state, "_sessionItems");
            HttpStaticObjectsCollection staticObjects =
                (HttpStaticObjectsCollection)ExpressionEvaluator.GetValue(state, "_staticObjects");
            int timeout = (int)ExpressionEvaluator.GetValue(state, "_timeout");
            TypeRegistry.RegisterType("SessionStateModule", typeof(SessionStateModule));
            HttpCookieMode cookieMode =
                (HttpCookieMode)ExpressionEvaluator.GetValue(null, "SessionStateModule.s_configCookieless");
            SessionStateMode stateMode =
                (SessionStateMode)ExpressionEvaluator.GetValue(null, "SessionStateModule.s_configMode");
            HttpSessionStateContainer container = new HttpSessionStateContainer(
                id
                , sessionItems
                , staticObjects
                , timeout
                , false
                , cookieMode
                , stateMode
                , true
                );

            return (HttpSessionState)Activator.CreateInstance(
                                          typeof(HttpSessionState)
                                          , BindingFlags.Instance | BindingFlags.NonPublic
                                          , null
                                          , new object[] { container }
                                          , CultureInfo.InvariantCulture
                                          );
        }
        #endregion Session Handling Stuff
    }
}
