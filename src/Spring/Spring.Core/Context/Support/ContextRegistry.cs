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

using Common.Logging;

using Spring.Context.Events;
using Spring.Util;

namespace Spring.Context.Support
{
    /// <summary>
    /// Provides access to a central registry of
    /// <see cref="Spring.Context.IApplicationContext"/>s.
    /// </summary>
    /// <remarks>
    /// <p>
    /// A singleton implementation to access one or more application contexts.  Application
    /// context instances are cached.
    /// </p>
    /// <p>Note that the use of this class or similar is unnecessary except (sometimes) for
    /// a small amount of glue code.  Excessive usage will lead to code that is more tightly
    /// coupled, and harder to modify or test.  Consider refactoring your code to use standard
    /// Dependency Injection techniques or implement the interface IApplicationContextAware to
    /// obtain a reference to an application context.</p>
    /// </remarks>
    /// <author>Mark Pollack</author>
    /// <author>Aleksandar Seovic</author>
    /// <seealso cref="Spring.Context.IApplicationContext"/>
    public sealed class ContextRegistry
    {
        /// <summary>
        /// The shared <see cref="Common.Logging.ILog"/> instance for this class (and derived classes).
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(ContextRegistry));

        private static readonly object syncRoot = new Object();
        private static readonly ContextRegistry instance = new ContextRegistry();
        private static string rootContextName = null;

        private IDictionary<string, IApplicationContext> contextMap = new Dictionary<string, IApplicationContext>(StringComparer.OrdinalIgnoreCase);

        #region Constructor (s) / Destructor

        // CLOVER:OFF

        /// <summary>
        /// Creates a new instance of the ContextRegistry class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Explicit static constructor to tell C# compiler
        /// not to mark type as beforefieldinit.
        /// </p>
        /// </remarks>
        static ContextRegistry()
        { }

        // CLOVER:ON

        #endregion

        /// <summary>
        /// This event is fired, if ContextRegistry.Clear() is called.<br/>
        /// Clients may register to get informed
        /// </summary>
        /// <remarks>
        /// This event is fired while still holding a lock on the Registry.<br/>
        /// 'sender' parameter is sent as typeof(ContextRegistry), EventArgs are not used
        /// </remarks>
        public static event EventHandler Cleared;

        /// <summary>
        /// Gets an object that should be used to synchronize access to ContextRegistry
        /// from the calling code.
        /// </summary>
        public static object SyncRoot
        {
            get { return syncRoot; }
        }

        private static void ConstructNestedDefaultContextName(IApplicationContext context)
        {
            IApplicationContext parent = context.ParentContext;

            Dictionary<int, IApplicationContext> contexts = new Dictionary<int, IApplicationContext>();

            int contextIndex = 0;

            contexts.Add(contextIndex, context);

            while (parent != null)
            {
                contextIndex++;
                contexts.Add(contextIndex, parent);
                parent = parent.ParentContext;
            }

            string prefix = string.Empty;

            for (int i = contextIndex; i > 0; i--)
            {
                IApplicationContext contextToUpdate = contexts[i];

                if (prefix != string.Empty)
                    prefix = string.Format("{0}/{1}", prefix, contextToUpdate.Name);
                else
                    prefix = contextToUpdate.Name;

            }

            context.Name = string.Format("{0}/{1}", prefix, context.Name);
        }

        private static void EnsureHierarchicalNameIfDefault(IApplicationContext context)
        {
            //if there is no parent context there is no change needed
            if (context.ParentContext == null)
                return;

            if (context.Name == AbstractApplicationContext.DefaultRootContextName)
                ConstructNestedDefaultContextName(context);

        }


        /// <summary>
        /// Registers an instance of an
        /// <see cref="Spring.Context.IApplicationContext"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is usually called via a
        /// <see cref="Spring.Context.Support.ContextHandler"/> inside a .NET
        /// application configuration file.
        /// </p>
        /// </remarks>
        /// <param name="context">The application context to be registered.</param>
        /// <exception cref="Spring.Context.ApplicationContextException">
        /// If a context has previously been registered using the same name
        /// </exception>
        public static void RegisterContext(IApplicationContext context)
        {

            EnsureHierarchicalNameIfDefault(context);

            lock (syncRoot)
            {
                IApplicationContext ctx;
                if (instance.contextMap.TryGetValue(context.Name, out ctx))
                {
                    throw new ApplicationContextException(string.Format("Existing context '{0}' already registered under name '{1}'.", ctx, context.Name));
                }

                instance.contextMap[context.Name] = context;
                context.ContextEvent += OnContextEvent;

                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(String.Format("Registering context '{0}' under name '{1}'.", context, context.Name));
                }

                #endregion

                if (rootContextName == null)
                {
                    rootContextName = context.Name;
                }
            }
        }

        /// <summary>
        /// Handles events raised by an application context.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnContextEvent(object sender, ApplicationEventArgs e)
        {
            ContextEventArgs cea = e as ContextEventArgs;
            if (cea != null
                && cea.Event == ContextEventArgs.ContextEvent.Closed
                && sender is IApplicationContext)
            {
                // we know the context is registered!
                UnregisterContext((IApplicationContext)sender);
            }
        }

        /// <summary>
        /// Removes the context from the registry
        /// </summary>
        /// <remarks>
        /// Has no effect if the context wasn't registered
        /// </remarks>
        /// <param name="context">The context to remove from the registry</param>
        private static void UnregisterContext(IApplicationContext context)
        {
            AssertUtils.ArgumentNotNull(context, "context");
            lock (syncRoot)
            {
                if (IsContextRegistered(context.Name))
                {
                    instance.contextMap.Remove(context.Name);
                    if (rootContextName == context.Name)
                    {
                        rootContextName = null;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the root application context.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The first call to GetContext will create the context
        /// as specified in the .NET application configuration file
        /// under the location spring/context.
        /// </p>
        /// </remarks>
        /// <returns>The root application context.</returns>
        public static IApplicationContext GetContext()
        {
            lock (syncRoot)
            {
                InitializeContextIfNeeded();
                if (rootContextName == null)
                {
                    throw new ApplicationContextException(
                        "No context registered. Use the 'RegisterContext' method or the 'spring/context' section from your configuration file.");
                }
                return GetContext(rootContextName);
            }
        }

        /// <summary>
        /// Returns context based on specified name.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The first call to GetContext will create the context
        /// as specified in the .NET application configuration file
        /// under the location spring/context.
        /// </p>
        /// </remarks>
        /// <param name="name">The context name.</param>
        /// <returns>The specified context, or null, if context with that name doesn't exists.</returns>
        /// <exception cref="System.ArgumentException">
        /// If the context name is null or empty
        /// </exception>
        public static IApplicationContext GetContext(string name)
        {
            if (StringUtils.IsNullOrEmpty(name))
            {
                throw new ArgumentException(
                    "The context name passed to the GetContext method cannot be null or empty.");
            }
            else
            {
                lock (syncRoot)
                {
                    InitializeContextIfNeeded();
                    IApplicationContext ctx;
                    if (!instance.contextMap.TryGetValue(name, out ctx))
                    {
                        throw new ApplicationContextException(String.Format(
                            "No context registered under name '{0}'. Use the 'RegisterContext' method or the 'spring/context' section from your configuration file.",
                            name));
                    }

                    #region Instrumentation

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(String.Format(
                            "Returning context '{0}' registered under name '{1}'.", ctx, name));
                    }

                    #endregion

                    return ctx;
                }
            }
        }

        /// <summary>
        /// Removes all registered
        /// <see cref="Spring.Context.IApplicationContext"/>s from this
        /// registry.
        /// </summary>
        /// <remarks>
        /// Raises the <see cref="ContextRegistry.Cleared"/> event while still holding a lock on <see cref="ContextRegistry.SyncRoot"/>
        /// </remarks>
        public static void Clear()
        {
            lock (syncRoot)
            {
                ICollection<IApplicationContext> contexts = new List<IApplicationContext>(instance.contextMap.Values);
                foreach (IApplicationContext ctx in contexts)
                {
                    ctx.Dispose();
                }

                // contexts will be removed from contextMap during OnContextEvent handler
                // but someone might choose to override AbstractApplicationContext.Dispose() without
                // calling base.Dispose() ...
                if (log.IsWarnEnabled)
                {
                    if (instance.contextMap.Count > 0)
                    {
                        log.Warn(
                            String.Format(
                                "Not all contexts were removed from registry during cleanup - did you forget to call base.Dispose() when overriding AbstractApplicationContext.Dispose()?"));
                    }
                }

                instance.contextMap.Clear();
                ConfigurationUtils.ClearCache();
                rootContextName = null;
                // mark section dirty - force re-read from disk next time
                ConfigurationUtils.RefreshSection(AbstractApplicationContext.ContextSectionName);
                DynamicCodeManager.Clear();
                Cleared?.Invoke(typeof(ContextRegistry), EventArgs.Empty);
            }
        }

        /// <summary>
        /// Allows to check, if a context is already registered
        /// </summary>
        /// <param name="name">The context name.</param>
        /// <returns>true, if the context is already registered. false otherwise</returns>
        public static bool IsContextRegistered(string name)
        {
            lock (instance)
            {
                IApplicationContext temp;
                instance.contextMap.TryGetValue(name, out temp);
                return temp != null;
            }
        }

        private static bool rootContextCurrentlyInCreation;

        private static void InitializeContextIfNeeded()
        {
            if (rootContextName != null)
            {
                return;
            }

            DoInitializeRootContext();
        }

        private static void DoInitializeRootContext()
        {
            if (rootContextCurrentlyInCreation)
            {
                throw new InvalidOperationException(
                    "root context is currently in creation. You must not call ContextRegistry.GetContext() from e.g. constructors of your singleton objects");
            }

            rootContextCurrentlyInCreation = true;
            try
            {
                ConfigurationUtils.GetSection(AbstractApplicationContext.ContextSectionName);
            }
            finally
            {
                rootContextCurrentlyInCreation = false;
            }
        }
    }
}

