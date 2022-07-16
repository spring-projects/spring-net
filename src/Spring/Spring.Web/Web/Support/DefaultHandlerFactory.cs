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
using Spring.Context;
using Spring.Context.Support;
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
#if !MONO
    /// <summary>
    /// SimpleHandlerFactory is used to wrap any arbitrary <see cref="IHttpHandlerFactory"/> to make it "Spring-aware".
    /// </summary>
    /// <remarks>
    /// By default, an instance of <see cref="System.Web.UI.SimpleHandlerFactory"/> is used as underlying factory.
    /// </remarks>
    /// <author>Erich Eichinger</author>
#endif
    public class DefaultHandlerFactory : AbstractHandlerFactory
    {
        private readonly IHttpHandlerFactory _innerFactory;
#if !MONO
        /// <summary>
        /// Creates a new instance, using a <see cref="System.Web.UI.SimpleHandlerFactory"/> as underlying factory.
        /// </summary>
#else
        /// <summary>
        /// Creates a new instance of the DefaultHandlerFactory
        /// </summary>
#endif
        public DefaultHandlerFactory()
            : this(SimpleHandlerFactory)
        { }

        /// <summary>
        /// Create a new instance, using an instance of <paramref name="innerFactoryType"/> as underlying factory.
        /// </summary>
        /// <param name="innerFactoryType">a type that implements <see cref="IHttpHandlerFactory"/></param>
        public DefaultHandlerFactory(Type innerFactoryType)
            : this((IHttpHandlerFactory)Activator.CreateInstance(innerFactoryType, true))
        {
        }

        /// <summary>
        /// Create a new instance, using <paramref name="innerFactory"/> as underlying factory.
        /// </summary>
        /// <param name="innerFactory">the factory to be wrapped.</param>
        public DefaultHandlerFactory(IHttpHandlerFactory innerFactory)
        {
            AssertUtils.ArgumentNotNull(innerFactory, "innerFactory");
            _innerFactory = innerFactory;
        }

        /// <summary>
        /// Create a handler instance for the given URL.
        /// </summary>
        /// <param name="appContext">the application context corresponding to the current request</param>
        /// <param name="context">The <see cref="HttpContext"/> instance for this request.</param>
        /// <param name="requestType">The HTTP data transfer method (GET, POST, ...)</param>
        /// <param name="rawUrl">The requested <see cref="HttpRequest.RawUrl"/>.</param>
        /// <param name="physicalPath">The physical path of the requested resource.</param>
        /// <returns>A handler instance for processing the current request.</returns>
        protected override IHttpHandler CreateHandlerInstance(IConfigurableApplicationContext appContext, HttpContext context, string requestType, string rawUrl, string physicalPath)
        {
            IHttpHandler handler = _innerFactory.GetHandler(context, requestType, rawUrl, physicalPath);

            // find a matching object definition
            string appRelativeVirtualPath = WebUtils.GetAppRelativePath(rawUrl);
            NamedObjectDefinition nod = FindWebObjectDefinition(appRelativeVirtualPath, appContext.ObjectFactory);
            string objectDefinitionName = (nod != null) ? nod.Name : rawUrl;

            handler = WebSupportModule.ConfigureHandler(context, handler, appContext, objectDefinitionName, (nod != null));
            return handler;
        }

        /// <summary>
        /// Enables a factory to release an existing
        /// <see cref="System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <param name="handler">
        /// The <see cref="System.Web.IHttpHandler"/> object to release.
        /// </param>
        public override void ReleaseHandler(IHttpHandler handler)
        {
            _innerFactory.ReleaseHandler(handler);
        }
    }
}
