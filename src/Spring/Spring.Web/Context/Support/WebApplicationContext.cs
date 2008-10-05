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
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Common.Logging;
using Spring.Collections;
using Spring.Objects;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Objects.Support;
using Spring.Util;

#endregion

namespace Spring.Context.Support
{
    /// <summary>
    /// Web application context, taking the context definition files
    /// from the file system or from URLs. 
    ///
    /// Treats resource paths as web resources, when using
    /// IApplicationContext.GetResource. Resource paths are considered relative
    /// to the virtual directory.
    ///
    /// Note: In case of multiple config locations, later object definitions will
    /// override ones defined in earlier loaded files. This can be leveraged to
    /// deliberately override certain object definitions via an extra XML file.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class WebApplicationContext : AbstractXmlApplicationContext
    {
        /// <summary>
        /// The <see cref="Common.Logging.ILog"/> instance for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(WebApplicationContext));

        // holds construction info for debugging output
        private DateTime _constructionTimeStamp;
        private string _constructionUrl;

        private readonly string[] _configurationLocations;

        /// <summary>
        /// Create a new WebApplicationContext, loading the definitions
        /// from the given XML resource.
        /// </summary>
        /// <param name="configurationLocations">Names of configuration resources.</param>
        public WebApplicationContext(params string[] configurationLocations)
            : this(null, false, null, configurationLocations)
        {
        }

        /// <summary>
        /// Create a new WebApplicationContext, loading the definitions
        /// from the given XML resource.
        /// </summary>
        /// <param name="name">The application context name.</param>
        /// <param name="caseSensitive">Flag specifying whether to make this context case sensitive or not.</param>
        /// <param name="configurationLocations">Names of configuration resources.</param>
        public WebApplicationContext(string name, bool caseSensitive, params string[] configurationLocations)
            : this(name, caseSensitive, null, configurationLocations)
        {
        }

        /// <summary>
        /// Create a new WebApplicationContext with the given parent,
        /// loading the definitions from the given XML resources.
        /// </summary>
        /// <param name="name">The application context name.</param>
        /// <param name="caseSensitive">Flag specifying whether to make this context case sensitive or not.</param>
        /// <param name="parentContext">The parent context.</param>
        /// <param name="configurationLocations">Names of configuration resources.</param>
        public WebApplicationContext(string name, bool caseSensitive, IApplicationContext parentContext,
                                     params string[] configurationLocations) : base(name, caseSensitive, parentContext)
        {
            _configurationLocations = configurationLocations;
            DefaultResourceProtocol = WebUtils.DEFAULT_RESOURCE_PROTOCOL;
            Refresh();

            // remember creation info for debug output
            this._constructionTimeStamp = DateTime.Now;
            if (HttpContext.Current != null)
            {
                this._constructionUrl = HttpContext.Current.Request.RawUrl;
            }
            if (log.IsDebugEnabled)
            {
                log.Debug("created instance " + this.ToString());
            }
        }

        /// <summary>
        /// returns detailed instance information for debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //return base.ToString() + " - created on " + _constructionTimeStamp + "\n\ncreated by:" + _constructionUrl + "\n" + _constructionStackTrace.ToString();
            return string.Format("[{0}]:{1}({2})", this.Name, this.GetType().Name, base.GetHashCode().ToString());
        }


        /// <summary>
        /// Since the HttpRuntime discards it's configurationsection cache, we maintain our own context cache.
        /// Despite it really speeds up context-lookup, since we don't have to go through the whole HttpConfigurationSystem
        /// </summary>
        private static readonly Hashtable s_webContextCache = new CaseInsensitiveHashtable();

        static WebApplicationContext()
        {
            // register for ContextRegistry.Cleared event - we need to discard our cache in this case
            ContextRegistry.Cleared += new EventHandler(OnContextRegistryCleared);

#if NET_2_0 && !MONO_2_0
            if (HttpRuntime.AppDomainAppVirtualPath != null) // check if we're within an ASP.NET AppDomain!
            {
                // ensure HttpRuntime has been fully initialized!
                // this is a problem,.if ASP.NET Web Administration Application is used. This app does not fully set up the AppDomain...
                HttpRuntime runtime =
                    (HttpRuntime)
                    typeof(HttpRuntime).GetField("_theRuntime", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

                bool beforeFirstRequest = false;
                lock (runtime)
                {
                    beforeFirstRequest =
                        (bool)
                        typeof(HttpRuntime).GetField("_beforeFirstRequest", BindingFlags.Instance | BindingFlags.NonPublic).
                            GetValue(runtime);
                }
                log.Debug("BeforeFirstRequest:" + beforeFirstRequest);
                if (beforeFirstRequest)
                {
                    try
                    {
                        string firstRequestPath = HttpRuntime.AppDomainAppVirtualPath.TrimEnd('/') + "/dummy.context";
                        log.Info("Forcing first request " + firstRequestPath);
                        HttpRuntime.ProcessRequest(
                            new SimpleWorkerRequest(firstRequestPath, string.Empty, new StringWriter()));
                        log.Info("Successfully processed first request!");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Failed processing first request", ex);
                        throw;
                    }
                }
            }
#endif
        }

        /// <summary>
        /// EventHandler for ContextRegistry.Cleared event. Discards webContextCache.
        /// </summary>
        private static void OnContextRegistryCleared(object sender, EventArgs ev)
        {
            lock (s_webContextCache)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("received ContextRegistry.Cleared event - clearing webContextCache");
                }
                s_webContextCache.Clear();
            }
        }

        /// <summary>
        /// Returns the root context of this web application
        /// </summary>
        public static IApplicationContext GetRootContext()
        {
            return GetContextInternal( ("" + HttpRuntime.AppDomainAppVirtualPath).TrimEnd('/') + "/dummy.context");
        }

        /// <summary>
        /// Returns the web application context for the given (absolute!) virtual path
        /// </summary>
        public static IApplicationContext GetContext(string virtualPath)
        {
            return GetContextInternal(virtualPath);
        }

        /// <summary>
        /// Returns the web application context for the current request's filepath
        /// </summary>
        public static IApplicationContext Current
        {
            get
            {
                string requestUrl = HttpContext.Current.Request.FilePath;
                return GetContextInternal(requestUrl);
            }
        }

        private static IApplicationContext GetContextInternal(string virtualPath)
        {
            string virtualDirectory = WebUtils.GetVirtualDirectory(virtualPath);
            string contextName = virtualDirectory;
            if ( 0 == string.Compare( contextName , ("" + HttpRuntime.AppDomainAppVirtualPath).TrimEnd('/') + "/", true) )
            {
                contextName = DefaultRootContextName;
            }

            lock (s_webContextCache)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("looking up web context '{0}' in WebContextCache", contextName));
                }
                // first lookup in our own cache
                IApplicationContext context = (IApplicationContext) s_webContextCache[contextName];
                if (context != null)
                {
                    // found - nothing to do anymore
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                            string.Format("returning WebContextCache hit '{0}' for vpath '{1}' ", context, contextName));
                    }
                    return context;
                }

                // lookup ContextRegistry
                lock (ContextRegistry.SyncRoot)
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(string.Format("looking up web context '{0}' in ContextRegistry", contextName));
                    }

                    if (ContextRegistry.IsContextRegistered(contextName))
                    {
                        context = ContextRegistry.GetContext(contextName);
                    }

                    if (context == null)
                    {
                        // finally ask HttpConfigurationSystem for the requested context
                        try
                        {
                            if (log.IsDebugEnabled)
                            {
                                log.Debug(
                                    string.Format(
                                        "web context for vpath '{0}' not found. Force creation using filepath '{1}'",
                                        contextName, virtualPath));
                            }

                            // assure context is resolved to the given virtualDirectory
                            using (new HttpContextSwitch(virtualDirectory))
                            {
                                context = (IApplicationContext)ConfigurationUtils.GetSection(ContextSectionName);
                            }

                            if (context != null)
                            {
                                if (log.IsDebugEnabled)
                                    log.Debug(string.Format("got context '{0}' for vpath '{1}'", context, contextName));
                            }
                            else
                            {
                                if (log.IsDebugEnabled)
                                    log.Debug(string.Format("no context defined for vpath '{0}'", contextName));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (log.IsErrorEnabled)
                            {
                                log.Error(string.Format("failed creating context '{0}'", contextName), ex);
                            }
#if NET_1_1
                            if (ConfigurationUtils.IsConfigurationException(ex))
                            {
                                if (ex.InnerException != null)
                                {
                                    throw ex.InnerException;
                                }
                            }
#endif
                            throw;
                        }
                    }
                }

                // add it to the cache
                // Note: use 'contextName' not 'context.Name' here - the same context may be used for different paths!
                s_webContextCache.Add(contextName, context);
                if (log.IsDebugEnabled)
                {
                    log.Debug(
                        string.Format("added context '{0}' to WebContextCache for vpath '{1}'", context, contextName));
                }

                if (context != null)
                {
                    // register this context and all ParentContexts by their name - parent contexts may be additionally created by the HttpRuntime
                    IApplicationContext parentContext = context;
                    while (parentContext != null)
                    {
                        if (!s_webContextCache.ContainsKey(parentContext.Name))
                        {
                            s_webContextCache.Add(parentContext.Name, parentContext);
                            if (log.IsDebugEnabled)
                            {
                                log.Debug(
                                    string.Format("added parent context '{0}' to WebContextCache for vpath '{1}'",
                                                  parentContext, parentContext.Name));
                            }
                        }
                        parentContext = parentContext.ParentContext;
                    }
                }
                return context;
            } // lock(s_webContextCache)
        }

        /// <summary>
        /// Initializes object definition reader.
        /// </summary>
        /// <param name="objectDefinitionReader">Reader to initialize.</param>
        protected override void InitObjectDefinitionReader(XmlObjectDefinitionReader objectDefinitionReader)
        {
            NamespaceParserRegistry.RegisterParser(typeof(WebObjectsNamespaceParser));
        }

        /// <summary>
        /// Return an array of resource locations, referring to the XML object
        /// definition files that this context should be built with.
        /// </summary>
        /// <returns>an array of resource locations, or null if none</returns>
        protected override string[] ConfigurationLocations
        {
            get { return _configurationLocations; }
        }

        /// <summary>
        /// Creates web object factory for this context using parent context's factory as a parent.
        /// </summary>
        /// <returns>Web object factory to use.</returns>
        protected override DefaultListableObjectFactory CreateObjectFactory()
        {
            string contextPath = this.Name;
            if (contextPath == DefaultRootContextName)
            {
                contextPath = "/";
            }
            else
            {
                contextPath = contextPath + "/";
            }
            return new WebObjectFactory(contextPath, this.CaseSensitive, GetInternalParentObjectFactory());
        }
    }
}
