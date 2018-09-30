#region License
/*
 * Copyright © 2002-2011 the original author or authors.
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
using System.Web.UI;
using Common.Logging;
using Spring.Context;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// Support Class providing a method to ensure a control has been intercepted
    /// </summary>
    /// <author>Erich Eichinger</author>
    internal sealed class ControlInterceptor
    {
        private class NoOpInterceptionStrategy : IInterceptionStrategy
        {
            private static readonly ILog Log = LogManager.GetLogger( typeof( NoOpInterceptionStrategy ) );

            public bool Intercept( IApplicationContext defaultApplicationContext, ControlAccessor ctlAccessor,
                                  ControlCollectionAccessor ctlColAccessor )
            {
                return true;
            }
        }

        /// <summary>
        /// Holds all available interception strategies
        /// </summary>
        private static readonly IInterceptionStrategy[] s_availableInterceptionStrategies = new IInterceptionStrategy[]
            {
                new InterceptControlCollectionStrategy()
                , new InterceptControlCollectionOwnerStrategy()
            };

        /// <summary>
        /// The last resort interception strategy...
        /// </summary>
        private static readonly IInterceptionStrategy s_noopInterceptionStrategy = new NoOpInterceptionStrategy();

        /// <summary>
        /// Holds a control.GetType()->IInterceptionStrategy table.
        /// </summary>
        private static readonly Hashtable s_cachedInterceptionStrategies = new Hashtable();

        /// <summary>
        /// Holds well-known Type/Strategy mappings.
        /// </summary>
        private static readonly Hashtable s_knownInterceptionStrategies = new Hashtable();

        static ControlInterceptor()
        {
            s_knownInterceptionStrategies[typeof( ControlCollection )] = s_availableInterceptionStrategies[1];
        }

        private ControlInterceptor()
        {
        }

        /// <summary>
        /// Ensures, a control has been intercepted to support Web-DI. If not, the control will be intercepted.
        /// </summary>
        public static void EnsureControlIntercepted( IApplicationContext defaultApplicationContext, Control control )
        {
            if (control is LiteralControl)
            {
                return; // nothing more to do
            }

            // check control itself
            if (IsDependencyInjectionAware( defaultApplicationContext, control ))
            {
                return; // nothing more to do
            }

            // check control's ControlCollection
            EnsureControlCollectionIntercepted( defaultApplicationContext, control );
        }

        private static void EnsureControlCollectionIntercepted( IApplicationContext defaultApplicationContext, Control control )
        {
            // check the collection
            ControlAccessor ctlAccessor = new ControlAccessor( control );
            ControlCollection childControls = ctlAccessor.Controls;
            if (IsDependencyInjectionAware( defaultApplicationContext, childControls ))
            {
                return; // nothing more to do				
            }

            // check, if the collection's owner has already been intercepted
            ControlCollectionAccessor ctlColAccessor = new ControlCollectionAccessor( childControls );
            if (IsDependencyInjectionAware( defaultApplicationContext, ctlColAccessor.Owner ))
            {
                return; // nothing more to do				
            }

            // lookup strategy in cache
            IInterceptionStrategy strategy = null;
            lock (s_cachedInterceptionStrategies)
            {
                strategy = (IInterceptionStrategy)s_cachedInterceptionStrategies[control.GetType()];
            }

            if (strategy != null)
            {
                strategy.Intercept( defaultApplicationContext, ctlAccessor, ctlColAccessor );
            }
            else
            {
                // nothing in cache - try well-known strategies for owner resp. child collection type
                strategy = (IInterceptionStrategy)s_knownInterceptionStrategies[control.GetType()];
                if (strategy == null)
                {
                    strategy = (IInterceptionStrategy)s_knownInterceptionStrategies[childControls.GetType()];
                }

                // try intercept using well-known strategy
                if (strategy != null)
                {
                    bool bOk = strategy.Intercept( defaultApplicationContext, ctlAccessor, ctlColAccessor );
                    if (!bOk)
                    {
                        strategy = null;
                    }
                }

                // not well-known or didn't work out
                if (strategy == null)
                {
                    // probe for a strategy
                    bool bOk = false;
                    for (int i = 0; i < s_availableInterceptionStrategies.Length; i++)
                    {
                        strategy = s_availableInterceptionStrategies[i];
                        bOk = strategy.Intercept( defaultApplicationContext, ctlAccessor, ctlColAccessor );
                        if (bOk)
                            break;
                    }
                    if (!bOk)
                    {
                        LogManager.GetLogger( typeof( ControlInterceptor ) ).Warn( string.Format( "dependency injection not supported for control type {0}", ctlAccessor.GetTarget().GetType() ) );
                        strategy = s_noopInterceptionStrategy;
                    }
                }

                lock (s_cachedInterceptionStrategies)
                {
                    s_cachedInterceptionStrategies[control.GetType()] = strategy;
                }
            }
        }

        private static bool IsDependencyInjectionAware( IApplicationContext defaultApplicationContext, object o )
        {
            ISupportsWebDependencyInjection diAware = o as ISupportsWebDependencyInjection;
            if (diAware != null)
            {
                // If the ControlCollection is alread DI-aware ensure appContext is set
                if (diAware.DefaultApplicationContext == null)
                {
                    diAware.DefaultApplicationContext = defaultApplicationContext;
                }
                return true; // nothing more to do				
            }
            return false;
        }
    }
}
