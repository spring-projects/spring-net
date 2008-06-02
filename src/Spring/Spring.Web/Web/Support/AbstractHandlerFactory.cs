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

using System.IO;
using System.Web;
using Common.Logging;
using Spring.Objects.Factory.Config;
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// Provides base functionality for Spring.NET context-aware
    /// <see cref="System.Web.IHttpHandlerFactory"/> implementations.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Provides derived classes with a default implementation of
    /// <see cref="IHttpHandlerFactory.ReleaseHandler(IHttpHandler)"/> method.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public abstract class AbstractHandlerFactory : IHttpHandlerFactory
    {
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Web.Support.AbstractHandlerFactory"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an abstract class and as such provides no public constructors.
        /// </p>
        /// </remarks>
        protected AbstractHandlerFactory()
        {}

        /// <summary>
        /// Returns an appropriate <see cref="System.Web.IHttpHandler"/> implementation.
        /// </summary>
        /// <param name="context">
        /// An instance of the <see cref="System.Web.HttpContext"/> class that
        /// provides references to intrinsic server objects.
        /// </param>
        /// <param name="requestType">
        /// The HTTP method of the request.
        /// </param>
        /// <param name="url">The request URL.</param>
        /// <param name="pathTranslated">
        /// The physical path of the requested resource.
        /// </param>
        /// <returns>
        /// A new <see cref="System.Web.IHttpHandler"/> object that processes
        /// the request.
        /// </returns>
        public abstract IHttpHandler GetHandler(
            HttpContext context, string requestType, string url, string pathTranslated);

        /// <summary>
        /// Enables a factory to release an existing
        /// <see cref="System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <param name="handler">
        /// The <see cref="System.Web.IHttpHandler"/> object to release.
        /// </param>
        public virtual void ReleaseHandler(IHttpHandler handler)
        {}

        /// <summary>
        /// DO NOT USE - this is subject to change!
        /// </summary>
        /// <param name="appRelativeVirtualPath"></param>
        /// <param name="objectFactory"></param>
        /// <returns>
        /// This method requires registrars to follow the convention of registering web object definitions using their
        /// application relative urls (~/mypath/mypage.aspx). 
        /// </returns>
        /// <remarks>
        /// Resolve an object definition by url.
        /// </remarks>
        protected internal static NamedObjectDefinition FindWebObjectDefinition(string appRelativeVirtualPath, IConfigurableListableObjectFactory objectFactory)
        {
            ILog Log = LogManager.GetLogger(typeof(AbstractHandlerFactory));
            bool isDebug = Log.IsDebugEnabled;

            // lookup definition using app-relative url
            if (isDebug) Log.Debug(string.Format("GetHandler():looking up definition for app-relative url '{0}'", appRelativeVirtualPath));
            string objectDefinitionName = appRelativeVirtualPath;
            IObjectDefinition pageDefinition = objectFactory.GetObjectDefinition(appRelativeVirtualPath, true);

            if (pageDefinition == null)
            {
                // try using pagename+extension and pagename only
                string pageExtension = Path.GetExtension(appRelativeVirtualPath);
                string pageName = WebUtils.GetPageName(appRelativeVirtualPath);
                // only looks in the specified object factory -- it will *not* search parent contexts
                pageDefinition = objectFactory.GetObjectDefinition(pageName + pageExtension, false);
                if (pageDefinition == null)
                {
                    pageDefinition = objectFactory.GetObjectDefinition(pageName, false);
                    if (pageDefinition != null) objectDefinitionName = pageName;
                }
                else
                {
                    objectDefinitionName = pageName + pageExtension;
                }

                if (pageDefinition != null)
                {
                    if (isDebug)
                        Log.Debug(string.Format("GetHandler():found definition for page-name '{0}'", objectDefinitionName));
                }
                else
                {
                    if (isDebug)
                        Log.Debug(string.Format("GetHandler():no definition found for page-name '{0}'", pageName));
                }
            }
            else
            {
                if (isDebug) Log.Debug(string.Format("GetHandler():found definition for page-url '{0}'", appRelativeVirtualPath));
            }

            return (pageDefinition == null) ? (NamedObjectDefinition)null : new NamedObjectDefinition(objectDefinitionName, pageDefinition);
        }

        /// <summary>
        /// DO NOT USE - this is subject to change!
        /// </summary>
        protected internal class NamedObjectDefinition
        {
            private readonly string _name;
            private readonly IObjectDefinition _objectDefinition;

            /// <summary>
            /// DO NOT USE
            /// </summary>
            public NamedObjectDefinition(string name, IObjectDefinition objectDefinition)
            {
                _name = name;
                _objectDefinition = objectDefinition;
            }

            /// <summary>
            /// DO NOT USE
            /// </summary>
            public string Name
            {
                get { return _name; }
            }

            /// <summary>
            /// DO NOT USE
            /// </summary>
            public IObjectDefinition ObjectDefinition
            {
                get { return _objectDefinition; }
            }
        }
    }

}