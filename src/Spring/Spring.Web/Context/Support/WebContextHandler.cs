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

using System.Web.Configuration;
using System.Xml;

using Common.Logging;

using Spring.Util;

#endregion

namespace Spring.Context.Support
{
    /// <summary>
    /// Creates an <see cref="Spring.Context.IApplicationContext"/> instance
    /// using context definitions supplied in a custom configuration and
    /// configures the <see cref="ContextRegistry"/> with that instance.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="ContextHandler">ContextHandler</see> with
    /// web specific behaviour. It uses <see cref="WebApplicationContext">WebApplicationContext</see>
    /// as default Context-Type.
    /// </remarks>
    /// <author>Erich Eichinger</author>
    public class WebContextHandler : ContextHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WebContextHandler));

        /// <summary>
        /// Sets default context type to <see cref="WebApplicationContext"/>
        /// </summary>
        protected override Type DefaultApplicationContextType
        {
            get { return typeof(WebApplicationContext); }
        }

        /// <summary>
        /// Sets default case-sensitivity to 'false' for web-applications
        /// </summary>
        protected override bool DefaultCaseSensitivity
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the context name from the given <see cref="HttpConfigurationContext"/>
        /// </summary>
        protected override string GetContextName(object configContext, XmlElement contextElement)
        {
            string contextName = GetVirtualPath(configContext);
            // NET 2.0 returns "/" for root path
            if (contextName == "/")
            {
                contextName = String.Empty;
            }
            return contextName;
        }

        /// <summary>
        /// Throws a configuration exception, if child contexts are specified.
        /// </summary>
        /// <remarks>
        /// Nesting contexts in webapplications is done by explicitly declaring
        /// spring context sections for each directory.
        /// </remarks>
        protected override void CreateChildContexts(IApplicationContext parentContext, object configContext, IList<XmlNode> childContexts)
        {
            // disable child contexts in webapps
            if (childContexts.Count > 0)
            {
                throw ConfigurationUtils.CreateConfigurationException(
                    String.Format("Nested Child Contexts are not allowed in Web Applications. Use Web.config hierarchy instead."), childContexts[0]);
            }
        }

        /// <summary>
        /// Handles web specific details of context instantiation.
        /// </summary>
        protected override IApplicationContext InstantiateContext(IApplicationContext parent, object configContext, string contextName, Type contextType, bool caseSensitive, IList<string> resources)
        {
            // ASP.NET may scavenge it's configuration section cache if memory usage is too high.
            // Thus a handler may be called more than once for the same context.
            // Return registered context in this case.
            if (ContextRegistry.IsContextRegistered(contextName))
            {
                IApplicationContext ctx = ContextRegistry.GetContext(contextName);
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(
                        string.Format("web context '{0}' already registered - returning existing instance {1}",
                                      contextName, ctx));
                }
                return ctx;
            }

            // for rewriting path during context instantiation
            string vpath = GetVirtualPath(configContext);
            if (!vpath.EndsWith("/")) vpath = vpath + "/";
            using (new HttpContextSwitch(vpath))
            {
                return base.InstantiateContext(parent, configContext, contextName, contextType, caseSensitive, resources);
            }
        }

        private String GetVirtualPath(Object configContext)
        {
            HttpConfigurationContext httpConfigurationContext = (HttpConfigurationContext)configContext;
            if (httpConfigurationContext != null)
            {
                return httpConfigurationContext.VirtualPath;
            }
            return String.Empty;
        }
    }
}
