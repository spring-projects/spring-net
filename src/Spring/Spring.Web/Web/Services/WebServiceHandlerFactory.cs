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

using System.Reflection;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.Services;

using Spring.Context;
using Spring.Context.Support;
using Spring.Util;
using Spring.Web.Support;

#if MONO_2_0
using System.Web.Compilation;
using System.CodeDom.Compiler;
using System.Collections;
#endif

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
		#if !MONO_2_0
        private static readonly MethodInfo CoreGetHandler =
            typeof(System.Web.Services.Protocols.WebServiceHandlerFactory).GetMethod("CoreGetHandler", BindingFlags.NonPublic | BindingFlags.Instance, null,
                                                                                     new Type[] {typeof(Type), typeof(HttpContext), typeof(HttpRequest), typeof(HttpResponse)}, null);
		#endif

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
#if !MONO
                if (appContext.IsTypeMatch(nod.Name, typeof(WebServiceExporter)))
                {
                    WebServiceExporter wse = (WebServiceExporter)appContext.GetObject(nod.Name);
                    serviceType = wse.GetExportedType();
                }
                else
                {
                    serviceType = appContext.GetType(nod.Name);
                    // check if the type defines a Web Service
                    object[] wsAttribute = serviceType.GetCustomAttributes(typeof(WebServiceAttribute), true);
                    if (wsAttribute.Length == 0)
                    {
                        serviceType = null;
                    }
                }
#else
                serviceType = appContext.GetType(nod.Name);

                // check if the type defines a Web Service
                object[] wsAttribute = serviceType.GetCustomAttributes(typeof(WebServiceAttribute), true);
                if (wsAttribute.Length == 0)
                {
                    serviceType = null;
                }
#endif
            }

            if (serviceType == null)
            {
                serviceType = WebServiceParser.GetCompiledType(url, context);
            }


#if !MONO_2_0
            return (IHttpHandler) CoreGetHandler.Invoke(this, new object[] {serviceType, context, context.Request, context.Response});
#else

			// find if the BuildManager already contains a cached value of the service type
			var buildCacheField = typeof(BuildManager).GetField("buildCache", BindingFlags.Static | BindingFlags.NonPublic);
			var buildCache = (IDictionary)buildCacheField.GetValue(null);

			if(!buildCache.Contains(appRelativeVirtualPath))
			{
				// create new fake BuildManagerCacheItem wich represent the target type
				var buildManagerCacheItemType = Type.GetType("System.Web.Compilation.BuildManagerCacheItem, System.Web");
				var cacheItemCtor = buildManagerCacheItemType.GetConstructor(new Type[]{typeof(Assembly), typeof(BuildProvider), typeof(CompilerResults)});
				var buildProvider = new FakeBuildProvider(serviceType);
				var cacheItem = cacheItemCtor.Invoke(new object[]{serviceType.Assembly, buildProvider, null });

				// store it in the BuildManager
				buildCache [appRelativeVirtualPath] = cacheItem;
			}

			// now that the target type is in the cache, let the default process continue
			return base.GetHandler(context, requestType, url, path);
#endif
        }
    }

#if MONO_2_0
	public class FakeBuildProvider : BuildProvider
	{
		// Define an internal member for the compiler type.
	    protected CompilerType _compilerType = null;
		private Type _type;

	    public FakeBuildProvider(Type type)
	    {
			_type = type;
	        _compilerType = GetDefaultCompilerTypeForLanguage("C#");
	    }

	    // Return the internal CompilerType member
	    // defined in this implementation.
	    public override CompilerType CodeCompilerType
	    {
	        get { return _compilerType; }
	    }

	    // Define the build provider implementation of the GenerateCode method.
	    public override void GenerateCode(AssemblyBuilder assemBuilder)
	    {

	    }

	    public override System.Type GetGeneratedType(CompilerResults results)
	    {
	        return _type;
	    }

		public override string GetCustomString (System.CodeDom.Compiler.CompilerResults results)
		{
			return "No source code";
		}

		public override BuildProviderResultFlags GetResultFlags (System.CodeDom.Compiler.CompilerResults results)
		{
			return BuildProviderResultFlags.Default;
		}
	}
#endif
}
