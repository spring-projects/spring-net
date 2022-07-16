#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using System.Collections;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using Spring.Collections;
using Spring.Context.Attributes;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Concrete implementation of
    /// <see cref="Spring.Objects.Factory.IListableObjectFactory"/> that knows
    /// how to handle <see cref="IWebObjectDefinition"/>s.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This class should only be used within the context of an ASP.NET web application.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class WebObjectFactory : DefaultListableObjectFactory
    {
        private static bool s_eventHandlersRegistered = false;
        private readonly static string OBJECTTABLEKEY = "spring.objects";

        private delegate IDictionary ObjectDictionaryCreationHandler();

        /// <summary>
        /// Holds the virtual path this factory has been created from.
        /// </summary>
        private readonly string contextPath;
        /// <summary>
        /// Holds the handler reference for creating an object dictionary
        /// matching this factory's case-sensitivity
        /// </summary>
        private readonly ObjectDictionaryCreationHandler createObjectDictionary;

        #region Constructor (s) / Destructor

        static WebObjectFactory()
        {
            //EnsureEventHandlersRegistered();
        }

        /// <summary>
        /// Registers events handlers with <see cref="WebSupportModule"/> to ensure
        /// proper disposal of 'request'- and 'session'-scoped objects
        /// </summary>
        private void EnsureEventHandlersRegistered()
        {
            if (!s_eventHandlersRegistered)
            {
                lock(typeof(WebObjectFactory))
                {
                    if (s_eventHandlersRegistered) return;

                    if (log.IsDebugEnabled) log.Debug("hooking up event handlers");
                    VirtualEnvironment.EndRequest += OnEndRequest;
                    VirtualEnvironment.EndSession += OnEndSession;

                    s_eventHandlersRegistered = true;
                }
            }
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.WebObjectFactory"/> class.
        /// </summary>
        /// <param name="contextPath">The virtual path resources will be relative resolved to.</param>
        /// <param name="caseSensitive">Flag specifying whether to make this object factory case sensitive or not.</param>
        public WebObjectFactory(string contextPath, bool caseSensitive)
            : this(contextPath, caseSensitive, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.WebObjectFactory"/> class.
        /// </summary>
        /// <param name="contextPath">The virtual path resources will be relative resolved to.</param>
        /// <param name="caseSensitive">Flag specifying whether to make this object factory case sensitive or not.</param>
        /// <param name="parentFactory">
        /// The parent object factory.
        /// </param>
        public WebObjectFactory(string contextPath, bool caseSensitive, IObjectFactory parentFactory)
            : base(caseSensitive, parentFactory)
        {
            this.contextPath = contextPath;
            this.createObjectDictionary = (caseSensitive)
                                            ? new ObjectDictionaryCreationHandler(CreateCaseSensitiveDictionary)
                                            : new ObjectDictionaryCreationHandler(CreateCaseInsensitiveDictionary);

            InstantiationStrategy = new WebInstantiationStrategy();
        }

        #endregion

        /// <summary>
        /// Returns the virtual path this object factory is associated with.
        /// </summary>
        public string ContextPath
        {
            get { return contextPath; }
        }

        #region Convenience accessors for Http* objects

        /// <summary>
        /// Convinience accessor for HttpContext
        /// </summary>
        private HttpContext Context
        {
            get { return HttpContext.Current; }
        }

        /// <summary>
        /// Get the table of 'request'-scoped objects.
        /// </summary>
        protected virtual IDictionary Request
        {
            get
            {
                IDictionary objecttable = null;
                if (this.Context != null)
                {
                    objecttable = this.Context.Items[OBJECTTABLEKEY] as IDictionary;
                    if (objecttable == null)
                    {
                        this.Context.Items[OBJECTTABLEKEY] = CreateObjectDictionary();
                    }
                }
                return objecttable;
            }
        }

        /// <summary>
        /// Get the table of 'session'-scoped objects. Returns null, if Session is disabled.
        /// </summary>
        protected virtual IDictionary Session
        {
            get
            {
                IDictionary objecttable = null;
                if ((Context != null) && (Context.Session != null))
                {
                    objecttable = Context.Session[OBJECTTABLEKEY] as IDictionary;
                    if (objecttable == null)
                    {
                        Context.Session[OBJECTTABLEKEY] = CreateObjectDictionary();
                    }
                }
                return objecttable;
            }
        }

        #endregion

        /// <summary>
        /// Creates a dictionary matching this factory's case sensitivity behaviour.
        /// </summary>
        protected IDictionary CreateObjectDictionary()
        {
            return createObjectDictionary();
        }

        /// <summary>
        /// Creates the root object definition.
        /// </summary>
        /// <param name="templateDefinition">The template definition.</param>
        /// <returns>Root object definition.</returns>
        protected override RootObjectDefinition CreateRootObjectDefinition(IObjectDefinition templateDefinition)
        {
            return new RootWebObjectDefinition(templateDefinition);
        }

        /// <summary>
        /// Tries to find cached object for the specified name.
        /// </summary>
        /// <remarks>
        /// This implementation tries to find object first in the Request scope,
        /// then in the Session scope and finally in the Application scope.
        /// </remarks>
        /// <param name="objectName">Object name to look for.</param>
        /// <returns>Cached object if found, null otherwise.</returns>
        public override object GetSingleton(string objectName)
        {
            object instance = null;

            instance = GetScopedSingleton(objectName, this.Request);
            if (instance != null) return instance;

            instance = GetScopedSingleton(objectName, this.Session);
            if (instance != null) return instance;

            instance = base.GetSingleton(objectName);
            return instance;
        }

        /// <summary>
        /// Looks up an <paramref name="objectName"/> in the specified cache dictionary.
        /// </summary>
        /// <param name="objectName">the name to lookup.</param>
        /// <param name="scopedSingletonCache">the cache dictionary to search</param>
        /// <returns>the found instance. null otherwise</returns>
        protected object GetScopedSingleton(string objectName, IDictionary scopedSingletonCache)
        {
            if (scopedSingletonCache == null) return null;

            lock (scopedSingletonCache.SyncRoot)
            {
                object instance = scopedSingletonCache[objectName];
                if (instance == TemporarySingletonPlaceHolder)
                {
                    throw new ObjectCurrentlyInCreationException(objectName);
                }

                return instance;
            }
        }

        /// <summary>
        /// Creates a singleton instance for the specified object name and definition.
        /// </summary>
        /// <param name="objectName">
        /// The object name (will be used as the key in the singleton cache key).
        /// </param>
        /// <param name="objectDefinition">The object definition.</param>
        /// <param name="arguments">
        /// The arguments to use if creating a prototype using explicit arguments to
        /// a static factory method. It is invalid to use a non-null arguments value
        /// in any other case.
        /// </param>
        /// <returns>The created object instance.</returns>
        protected override object CreateAndCacheSingletonInstance(
            string objectName, RootObjectDefinition objectDefinition, object[] arguments)
        {
            if (IsWebScopedSingleton(objectDefinition))
            {
                ObjectScope scope = GetObjectScope(objectDefinition);

                if (scope == ObjectScope.Request)
                {
                    IDictionary requestCache = this.Request;
                    if (requestCache == null)
                    {
                        throw new ObjectCreationException(string.Format("'request' scoped web singleton object '{0}' requires a valid Request.", objectName));
                    }

                    object instance = CreateAndCacheScopedSingletonInstance(objectName, objectDefinition, arguments, requestCache);
                    return instance;
                }
                else if (scope == ObjectScope.Session)
                {
                    IDictionary sessionCache = this.Session;
                    if (sessionCache == null)
                    {
                        throw new ObjectCreationException(string.Format("'session' scoped web singleton object '{0}' requires a valid Session.", objectName));
                    }
                    object instance = CreateAndCacheScopedSingletonInstance(objectName, objectDefinition, arguments, sessionCache);
                    return instance;
                }

                throw new ObjectDefinitionException("Web singleton objects must be either request, session or application scoped.");
            }

            return base.CreateAndCacheSingletonInstance(objectName, objectDefinition, arguments);
        }

        /// <summary>
        /// Creates a singleton instance for the specified object name and definition
        /// and caches the instance in the specified dictionary
        /// </summary>
        /// <param name="objectName">
        /// The object name (will be used as the key in the singleton cache key).
        /// </param>
        /// <param name="objectDefinition">The object definition.</param>
        /// <param name="arguments">
        /// The arguments to use if creating a prototype using explicit arguments to
        /// a static factory method. It is invalid to use a non-null arguments value
        /// in any other case.
        /// </param>
        /// <param name="scopedSingletonCache">the dictionary to be used for caching singleton instances</param>
        /// <returns>The created object instance.</returns>
        /// <remarks>
        /// If the object is successfully created, <paramref name="scopedSingletonCache"/>
        /// contains the cached instance with the key <paramref name="objectName"/>.
        /// </remarks>
        protected virtual object CreateAndCacheScopedSingletonInstance(string objectName, RootObjectDefinition objectDefinition, object[] arguments, IDictionary scopedSingletonCache)
        {
            object instance;
            lock (scopedSingletonCache.SyncRoot)
            {
                EnsureEventHandlersRegistered();

                instance = scopedSingletonCache[objectName];
                if (instance == TemporarySingletonPlaceHolder)
                {
                    throw new ObjectCurrentlyInCreationException(objectName);
                }
                else if (instance == null)
                {
                    scopedSingletonCache.Add(objectName, TemporarySingletonPlaceHolder);
                    try
                    {
                        instance = InstantiateObject(objectName, objectDefinition, arguments, true, false);
                        AssertUtils.ArgumentNotNull(instance, "instance");
                        scopedSingletonCache[objectName] = instance;
                    }
                    catch
                    {
                        scopedSingletonCache.Remove(objectName);
                        throw;
                    }
                }
            }
            return instance;
        }

        /// <summary>
        /// We need this override so that Web Scoped Singletons are not registered in general
        /// DisposalObjectRegister
        /// </summary>
        /// <param name="name"></param>
        /// <param name="instance"></param>
        /// <param name="objectDefinition"></param>
        protected override void RegisterDisposableObjectIfNecessary(string name, object instance,
                                                                    RootObjectDefinition objectDefinition)
        {
            if (!IsWebScopedSingleton(objectDefinition))
            {
                base.RegisterDisposableObjectIfNecessary(name, instance, objectDefinition);
            }
        }

        /// <summary>
        /// Add the created, but yet unpopulated singleton to the singleton cache
        /// to be able to resolve circular references
        /// </summary>
        /// <param name="objectName">the name of the object to add to the cache.</param>
        /// <param name="objectDefinition">the definition used to create and populated the object.</param>
        /// <param name="rawSingletonInstance">the raw object instance.</param>
        /// <remarks>
        /// Derived classes may override this method to select the right cache based on the object definition.
        /// </remarks>
        protected override void AddEagerlyCachedSingleton(string objectName, IObjectDefinition objectDefinition, object rawSingletonInstance)
        {
            if (IsWebScopedSingleton(objectDefinition))
            {
                ObjectScope scope = GetObjectScope(objectDefinition);
                if (scope == ObjectScope.Request)
                {
                    this.Request[objectName] = rawSingletonInstance;
                }
                else if (scope == ObjectScope.Session)
                {
                    this.Session[objectName] = rawSingletonInstance;
                }
                else
                {
                    throw new ObjectDefinitionException("Web singleton objects must be either request, session or application scoped.");
                }
            }
            else
            {
                base.AddEagerlyCachedSingleton(objectName, objectDefinition, rawSingletonInstance);
            }
        }

        /// <summary>
        /// Remove the specified singleton from the singleton cache that has
        /// been added before by a call to <see cref="AddEagerlyCachedSingleton"/>
        /// </summary>
        /// <param name="objectName">the name of the object to remove from the cache.</param>
        /// <param name="objectDefinition">the definition used to create and populated the object.</param>
        /// <remarks>
        /// Derived classes may override this method to select the right cache based on the object definition.
        /// </remarks>
        protected override void RemoveEagerlyCachedSingleton(string objectName, IObjectDefinition objectDefinition)
        {
            if (IsWebScopedSingleton(objectDefinition))
            {
                ObjectScope scope = GetObjectScope(objectDefinition);
                if (scope == ObjectScope.Request)
                {
                    this.Request.Remove(objectName);
                }
                else if (scope == ObjectScope.Session)
                {
                    this.Session.Remove(objectName);
                }
                else
                {
                    throw new ObjectDefinitionException("Web singleton objects must be either 'request' or 'session' scoped.");
                }
            }
            else
            {
                base.RemoveEagerlyCachedSingleton(objectName, objectDefinition);
            }
        }

        private bool IsWebScopedSingleton(IObjectDefinition objectDefinition)
        {
            if (objectDefinition.IsSingleton &&
                (objectDefinition is IWebObjectDefinition || objectDefinition is ScannedGenericObjectDefinition))
            {
                ObjectScope scope = GetObjectScope(objectDefinition);
                return (scope == ObjectScope.Request) || (scope == ObjectScope.Session);
            }
            return false;
        }

        private ObjectScope GetObjectScope(IObjectDefinition objectDefinition)
        {
            if (objectDefinition is IWebObjectDefinition)
            {
                return ((IWebObjectDefinition) objectDefinition).Scope;
            }

            ObjectScope scope = (ObjectScope) Enum.Parse(typeof (ObjectScope), objectDefinition.Scope, true);

            return scope;
        }

        /// <summary>
        /// Configures object instance by injecting dependencies, satisfying Spring lifecycle
        /// interfaces and applying object post-processors.
        /// </summary>
        /// <param name="name">
        /// The name of the object definition expressing the dependencies that are to
        /// be injected into the supplied <parameref name="target"/> instance.
        /// </param>
        /// <param name="definition">
        /// An object definition that should be used to configure object.
        /// </param>
        /// <param name="wrapper">
        /// A wrapped object instance that is to be so configured.
        /// </param>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.ConfigureObject(object, string)"/>
        protected override object ConfigureObject(string name, RootObjectDefinition definition, IObjectWrapper wrapper)
        {
            // always configure object relative to contextPath
            using (new HttpContextSwitch(contextPath))
            {
                return base.ConfigureObject(name, definition, wrapper);
            }
        }

        /// <summary>
        /// Disposes all 'request'-scoped objects at the end of each Request
        /// </summary>
        private void OnEndRequest(HttpContext context)
        {
            IDictionary items = context.Items[OBJECTTABLEKEY] as IDictionary;

            if (items != null)
            {
                log.Debug("disposing 'request'-scoped item cache");
                ArrayList keys = new ArrayList(items.Keys);
                for (int i = 0; i < keys.Count; i++)
                {
                    IDisposable d = items[keys[i]] as IDisposable;
                    if (d != null)
                    {
                        d.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Disposes all 'session'-scoped objects at the end of a session
        /// </summary>
        private void OnEndSession(HttpSessionState session, CacheItemRemovedReason reason)
        {
            IDictionary items = null;
            try
            {
                items = session[OBJECTTABLEKEY] as IDictionary;
            }
            catch
            {
                // ignore exceptions while accessing session
            }
            if (items != null)
            {
                log.Debug("disposing 'session'-scoped item cache");
                object key = null;
                try
                {
                    ArrayList keys = new ArrayList(items.Keys);
                    for (int i = 0; i < keys.Count; i++)
                    {
                        key = keys[i];
                        IDisposable d = items[key] as IDisposable;
                        if (d != null)
                        {
                            d.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Fatal(string.Format("error during disposing session item with key '{0}'", key), ex);
                }
            }
        }

        /// <summary>
        /// Creates a case insensitive hashtable instance
        /// </summary>
        private static IDictionary CreateCaseInsensitiveDictionary()
        {
            return new CaseInsensitiveHashtable(); //CollectionsUtil.CreateCaseInsensitiveHashtable();
        }

        /// <summary>
        /// Creates a case sensitive hashtable instance
        /// </summary>
        private static IDictionary CreateCaseSensitiveDictionary()
        {
            return new Hashtable();
        }
    }
}
