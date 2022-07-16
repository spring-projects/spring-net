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

using System.Collections;
using Common.Logging;
using Spring.Context;
using Spring.Context.Support;
using Spring.Util;

namespace Spring.Testing.NUnit
{
    /// <summary>
    /// Superclass for NUnit test cases using a Spring context.
    /// </summary>
    /// <remarks>
    /// <p>Maintains a cache of contexts by key. This has significant performance
    /// benefit if initializing the context would take time. While initializing a
    /// Spring context itself is very quick, some objects in a context, such as
    /// a LocalSessionFactoryObject for working with NHibernate, may take time to
    /// initialize. Hence it often makes sense to do that initializing once.</p>
    /// <p>Normally you won't extend this class directly but rather extend one
    /// of its subclasses.</p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    public abstract class AbstractSpringContextTests
    {
        /// <summary>
        /// Map of context keys returned by subclasses of this class, to
        /// Spring contexts.
        /// </summary>
	    private static readonly IDictionary contextKeyToContextMap;

        /// <summary>
        /// Static ctor to avoid "beforeFieldInit" problem.
        /// </summary>
        static AbstractSpringContextTests()
        {
            contextKeyToContextMap = new Hashtable();
        }

        /// <summary>
        /// Disposes any cached context instance and removes it from cache.
        /// </summary>
        public static void ClearContextCache()
        {
            foreach(IApplicationContext ctx in contextKeyToContextMap.Values)
            {
                ctx.Dispose();
            }
            contextKeyToContextMap.Clear();
        }

        /// <summary>
        /// Indicates, whether context instances should be automatically registered with the global <see cref="ContextRegistry"/>.
        /// </summary>
        private bool registerContextWithContextRegistry = true;

        /// <summary>
        /// Logger available to subclasses.
        /// </summary>
        protected readonly ILog logger;

        /// <summary>
        /// Default constructor for AbstractSpringContextTests.
        /// </summary>
	    protected AbstractSpringContextTests()
        {
            logger = LogManager.GetLogger(GetType());
        }

        /// <summary>
        /// Controls, whether application context instances will
        /// be registered/unregistered with the global <see cref="ContextRegistry"/>.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool RegisterContextWithContextRegistry
        {
            get { return registerContextWithContextRegistry; }
            set { registerContextWithContextRegistry = value; }
        }

        /// <summary>
        /// Set custom locations dirty. This will cause them to be reloaded
        /// from the cache before the next test case is executed.
        /// </summary>
        /// <remarks>
        /// Call this method only if you change the state of a singleton
        /// object, potentially affecting future tests.
        /// </remarks>
        /// <param name="locations">Locations </param>
	    protected void SetDirty(string[] locations)
        {
            SetDirty((object)locations);
	    }

        /// <summary>
        /// Set context with <paramref name="contextKey"/> dirty. This will cause
        /// it to be reloaded from the cache before the next test case is executed.
        /// </summary>
        /// <remarks>
        /// Call this method only if you change the state of a singleton
        /// object, potentially affecting future tests.
        /// </remarks>
        /// <param name="contextKey">Locations </param>
	    protected void SetDirty(object contextKey)
        {
		    String keyString = ContextKeyString(contextKey);
		    IConfigurableApplicationContext ctx =
				    (IConfigurableApplicationContext) contextKeyToContextMap[keyString];
	        contextKeyToContextMap.Remove(keyString);

            if (ctx != null)
            {
			    ctx.Dispose();
		    }
	    }

        /// <summary>
        /// Returns <c>true</c> if context for the specified
        /// <paramref name="contextKey"/> is cached.
        /// </summary>
        /// <param name="contextKey">Context key to check.</param>
        /// <returns>
        /// <c>true</c> if context for the specified
        /// <paramref name="contextKey"/> is cached,
        /// <c>false</c> otherwise.
        /// </returns>
	    protected bool HasCachedContext(object contextKey)
        {
            string keyString = ContextKeyString(contextKey);
            return contextKeyToContextMap.Contains(keyString);
	    }

        /// <summary>
        /// Converts context key to string.
        /// </summary>
        /// <remarks>
        /// Subclasses can override this to return a string representation of
        /// their contextKey for use in logging.
        /// </remarks>
        /// <param name="contextKey">Context key to convert.</param>
        /// <returns>
        /// String representation of the specified <paramref name="contextKey"/>.  Null if
        /// contextKey is null.
        /// </returns>
	    protected virtual string ContextKeyString(object contextKey)
        {
            if (contextKey == null)
            {
                return null;
            }
		    if (contextKey is string[])
            {
                return StringUtils.CollectionToCommaDelimitedString((string[])contextKey);
		    }
		    else
            {
			    return contextKey.ToString();
		    }
	    }

        /// <summary>
        /// Caches application context.
        /// </summary>
        /// <param name="key">Key to use.</param>
        /// <param name="context">Context to cache.</param>
	    public void AddContext(object key, IConfigurableApplicationContext context)
        {
            AssertUtils.ArgumentNotNull(context, "context", "ApplicationContext must not be null");
            string keyString = ContextKeyString(key);
            contextKeyToContextMap.Add(keyString, context);

            if (RegisterContextWithContextRegistry
                && !ContextRegistry.IsContextRegistered(context.Name))
            {
                ContextRegistry.RegisterContext(context);
            }
	    }

        /// <summary>
        /// Returns cached context if present, or loads it if not.
        /// </summary>
        /// <param name="key">Context key.</param>
        /// <returns>Spring application context associated with the specified key.</returns>
	    protected IConfigurableApplicationContext GetContext(object key)
        {
		    string keyString = ContextKeyString(key);
		    IConfigurableApplicationContext ctx =
				    (IConfigurableApplicationContext) contextKeyToContextMap[keyString];
		    if (ctx == null)
            {
			    if (key is string[])
                {
				    ctx = LoadContextLocations((string[]) key);
			    }
			    else
                {
				    ctx = LoadContext(key);
			    }
			    AddContext(key, ctx);
		    }
		    return ctx;
	    }




	    /// <summary>
	    /// Loads application context from the specified resource locations.
	    /// </summary>
	    /// <param name="locations">Resources to load object definitions from.</param>
	    protected virtual IConfigurableApplicationContext LoadContextLocations(string[] locations)
        {
		    if (logger.IsInfoEnabled)
            {
                logger.Info("Loading config for: " + StringUtils.CollectionToCommaDelimitedString(locations));
		    }
		    return new XmlApplicationContext(locations);
	    }

        /// <summary>
        /// Loads application context based on user-defined key.
        /// </summary>
        /// <remarks>
        /// Unless overriden by the user, this method will alway throw
        /// a <see cref="NotSupportedException"/>.
        /// </remarks>
        /// <param name="key">User-defined key.</param>
        protected virtual IConfigurableApplicationContext LoadContext(object key)
        {
		    throw new NotSupportedException("Subclasses may override this");
	    }

    }
}
