#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using System.Reflection;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.Services;

using Spring.Context;
using Spring.Context.Support;
using Spring.Util;
using Spring.Web.Support;

#endregion

namespace Spring.Web.Services
{
    /// <summary>
    /// An <see cref="System.Web.IHttpHandlerFactory"/> implementation that
    /// retrieves configured <c>WebService</c> objects from the Spring.NET web
    /// application context.
    /// </summary>
    /// <remarks>
    /// This handler factory uses web service name from the URL, without the extension,
    /// to find web service object in the Spring context.
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public class WebServiceHandlerFactory : System.Web.Services.Protocols.WebServiceHandlerFactory, IHttpHandlerFactory
    {
        private static readonly MethodInfo CoreGetHandler =
            typeof(System.Web.Services.Protocols.WebServiceHandlerFactory).GetMethod("CoreGetHandler", BindingFlags.NonPublic | BindingFlags.Instance, null,
                                                                                     new Type[] {typeof(Type), typeof(HttpContext), typeof(HttpRequest), typeof(HttpResponse)}, null);

        /// <summary>
        /// Retrieves instance of the page from Spring web application context.
        /// </summary>
        /// <param name="context">current HttpContext</param>
        /// <param name="requestType">type of HTTP request (GET, POST, etc.)</param>
        /// <param name="url">requested page URL</param>
        /// <param name="path">translated server path for the page</param>
        /// <returns>instance of the configured page object</returns>
        IHttpHandler IHttpHandlerFactory.GetHandler(HttpContext context, string requestType, string url, string path)
        {
            new AspNetHostingPermission(AspNetHostingPermissionLevel.Minimal).Demand();

            IConfigurableApplicationContext appContext =
                WebApplicationContext.GetContext(url) as IConfigurableApplicationContext;
            
            if (appContext == null)
            {
                throw new InvalidOperationException(
                    "Implementations of IApplicationContext must also implement IConfigurableApplicationContext");
            }

            string appRelativeVirtualPath = WebUtils.GetAppRelativePath(url);

            AbstractHandlerFactory.NamedObjectDefinition nod =
                AbstractHandlerFactory.FindWebObjectDefinition(appRelativeVirtualPath, appContext.ObjectFactory);

            Type serviceType = null;
            if (nod != null)
            {
                serviceType = appContext.GetType(nod.Name);

                // check if the type defines a Web Service
                object[] wsAttribute = serviceType.GetCustomAttributes(typeof(WebServiceAttribute), true);
                if (wsAttribute.Length == 0)
                {
                    serviceType = null;
                }                
            }

            if (serviceType == null)
            {
#if NET_2_0
                serviceType = WebServiceParser.GetCompiledType(url, context);
#else
                serviceType = WebServiceParser.GetCompiledType(path, context);
#endif
            }

            return (IHttpHandler) CoreGetHandler.Invoke(this, new object[] {serviceType, context, context.Request, context.Response});
        }
    }
}