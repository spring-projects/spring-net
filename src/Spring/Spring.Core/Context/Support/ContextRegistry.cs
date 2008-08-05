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
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using Common.Logging;

using Spring.Util;

#endregion

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

        private IDictionary contextMap = CollectionsUtil.CreateCaseInsensitiveHashtable();

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
        {}

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
            lock (syncRoot)
            {
                if (instance.contextMap.Contains(context.Name))
                {
                    IApplicationContext ctx = (IApplicationContext)instance.contextMap[context.Name];
                    throw new ApplicationContextException(
                        string.Format("Existing context '{0}' already registered under name '{1}'.",
                                      ctx, context.Name));
                }
                instance.contextMap[context.Name] = context;

                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(String.Format(
                        "Registering context '{0}' under name '{1}'.", context, context.Name));
                }

                #endregion

                if (rootContextName == null)
                {
                    rootContextName = context.Name;
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
                    IApplicationContext ctx = (IApplicationContext)instance.contextMap[name];

                    #region Instrumentation

                    if (log.IsDebugEnabled)
                    {
                        if (ctx == null)
                        {
                            log.Debug(String.Format(
                                          "No context registered under name '{0}'.", name));
                        }
                        else
                        {
                            log.Debug(String.Format(
                                          "Returning context '{0}' registered under name '{1}'.", ctx, name));
                        }
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
                foreach (IApplicationContext ctx in instance.contextMap.Values)
                {
                    ctx.Dispose();
                }
                instance.contextMap.Clear();
                rootContextName = null;
                ConfigurationUtils.RefreshSection(AbstractApplicationContext.ContextSectionName);
                DynamicCodeManager.Clear();
                if (Cleared != null)
                {
                    Cleared(typeof(ContextRegistry), EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Allows to check, if a context is already registered
        /// </summary>
        /// <param name="name">The context name.</param>
        /// <returns>true, if the context is already registered. false otherwise</returns>
        public static bool IsContextRegistered( string name )
        {
            lock (instance)
            {
                return (instance.contextMap[name] != null);
            }
        }

        private static bool rootContextCurrentlyInCreation;

        private static void InitializeContextIfNeeded()
        {
            if (rootContextName == null)
            {
                if (rootContextCurrentlyInCreation)
                {
                    throw new InvalidOperationException("root context is currently in creation. You must not call ContextRegistry.GetContext() from e.g. constructors of your singleton objects");    
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
}

