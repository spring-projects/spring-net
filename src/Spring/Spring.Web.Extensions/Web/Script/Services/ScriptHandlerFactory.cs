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

using System.Reflection;
using System.Web;
using System.Web.Script.Services;

using Spring.Context;
using Spring.Util;
using Spring.Web.Services;
using Spring.Web.Support;

namespace Spring.Web.Script.Services
{
    /// <summary>
    /// An <see cref="System.Web.IHttpHandlerFactory"/> implementation that
    /// creates a handler object for either ASP.NET AJAX 1.0 or Spring web services.
    /// </summary>
    /// <author>Bruno Baia</author>
    /// <author>Thomas Broyer</author>
    public class ScriptHandlerFactory : AbstractHandlerFactory, IHttpHandlerFactory
    {
        private static readonly Type scriptHandlerFactoryType =
            typeof(ScriptServiceAttribute).Assembly.GetType("System.Web.Script.Services.ScriptHandlerFactory");

        private static readonly FieldInfo scriptHandlerFactory_webServiceHandlerFactory =
            scriptHandlerFactoryType.GetField("_webServiceHandlerFactory", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly Type webServiceDataType =
            typeof(ScriptServiceAttribute).Assembly.GetType("System.Web.Script.Services.WebServiceData");

        private static readonly ConstructorInfo webServiceData_ctor =
            webServiceDataType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Type), typeof(bool) }, null);

        private static readonly MethodInfo webServiceData_GetCacheKey =
            webServiceDataType.GetMethod("GetCacheKey", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(string) }, null);


        private readonly IHttpHandlerFactory scriptHandlerFactory =
            (IHttpHandlerFactory)Activator.CreateInstance(scriptHandlerFactoryType, true);

        /// <summary>
        /// Creates a new instance of the <see cref="ScriptHandlerFactory"/> class.
        /// </summary>
        public ScriptHandlerFactory()
            : base()
        {
            scriptHandlerFactory_webServiceHandlerFactory.SetValue(
                this.scriptHandlerFactory,
                new WebServiceHandlerFactory());
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="System.Web.IHttpHandler"/>
        /// implementation for handling web service requests
        /// for both Spring and ASP.NET AJAX 1.0 web services.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="requestType">The type of HTTP request (GET or POST).</param>
        /// <param name="url">The url of the web service.</param>
        /// <param name="pathTranslated">The physical application path for the web service.</param>
        /// <returns>The web service handler object.</returns>
        public override IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            string filename = VirtualPathUtility.ToAbsolute(context.Request.FilePath);
            string cacheKey = (string)webServiceData_GetCacheKey.Invoke(null, new object[] { filename });
            object webServiceData = context.Cache.Get(cacheKey);
            if (webServiceData == null)
            {
                IConfigurableApplicationContext appContext = base.GetCheckedApplicationContext(url);
                string appRelativeVirtualPath = WebUtils.GetAppRelativePath(url);
                NamedObjectDefinition nod = FindWebObjectDefinition(appRelativeVirtualPath, appContext.ObjectFactory);

                if (nod != null)
                {
                    Type serviceType = null;
                    if (appContext.IsTypeMatch(nod.Name, typeof(WebServiceExporter)))
                    {
                        WebServiceExporter wse = (WebServiceExporter)appContext.GetObject(nod.Name);
                        serviceType = wse.GetExportedType();
                    }
                    else
                    {
                        serviceType = appContext.GetType(nod.Name);
                    }

                    object[] attrs = serviceType.GetCustomAttributes(typeof(ScriptServiceAttribute), false);
                    if (attrs.Length > 0)
                    {
                        webServiceData = webServiceData_ctor.Invoke(new object[] { serviceType, false });
                        context.Cache.Insert(cacheKey, webServiceData);
                    }
                }
            }

            return this.scriptHandlerFactory.GetHandler(context, requestType, url, pathTranslated);
        }

        /// <summary>
        /// Enables a factory to reuse an existing handler instance.
        /// </summary>
        /// <param name="handler">The <see cref="System.Web.IHttpHandler" /> object to reuse.</param>
        public override void ReleaseHandler(IHttpHandler handler)
        {
            this.scriptHandlerFactory.ReleaseHandler(handler);
        }

        /// <summary>
        /// Create a handler instance for the given URL.
        /// </summary>
        /// <param name="appContext">the application context corresponding to the current request</param>
        /// <param name="context">The <see cref="HttpContext"/> instance for this request.</param>
        /// <param name="requestType">The HTTP data transfer method (GET, POST, ...)</param>
        /// <param name="rawUrl">The requested <see cref="HttpRequest.RawUrl"/>.</param>
        /// <param name="physicalPath">The physical path of the requested resource.</param>
        /// <returns>A handler instance for the current request.</returns>
        protected override IHttpHandler CreateHandlerInstance( IConfigurableApplicationContext appContext, HttpContext context, string requestType, string rawUrl, string physicalPath )
        {
            throw new NotSupportedException();
        }
    }
}
