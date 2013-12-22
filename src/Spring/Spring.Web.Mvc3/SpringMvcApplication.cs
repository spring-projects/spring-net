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


using System;
using System.Web;
using System.Web.Mvc;

using Spring.Context.Support;

namespace Spring.Web.Mvc
{
    /// <summary>
    /// Spring.NET-specific HttpApplication for ASP.NET MVC integration.
    /// </summary>
    public abstract class SpringMvcApplication : HttpApplication
    {
        /// <summary>
        /// Executes custom initialization code after all event handler modules have been added.
        /// </summary>
        public override void Init()
        {
            base.Init();

            //the Spring HTTP Module won't have built the context for us until now so we have to delay until the init
            ConfigureApplicationContext();
        }

        /// <summary>
        /// Handles the BeginRequest event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void Application_BeginRequest(object sender, EventArgs e)
        {
            var resolver = BuildDependencyResolver();
            RegisterDependencyResolver(resolver);
        }


        /// <summary>
        /// Builds the dependency resolver.
        /// </summary>
        /// <returns>The <see cref="IDependencyResolver"/> instance.</returns>
        /// You must override this method in a derived class to control the manner in which the
        /// <see cref="IDependencyResolver"/> is created.
        protected virtual IDependencyResolver BuildDependencyResolver()
        {
            return new SpringMvcDependencyResolver(ContextRegistry.GetContext());
        }

        /// <summary>
        /// Configures the <see cref="Spring.Context.IApplicationContext"/> instance.
        /// </summary>
        /// <remarks>
        /// You must override this method in a derived class to control the manner in which the
        /// <see cref="Spring.Context.IApplicationContext"/> is configured.
        /// </remarks>        
        protected virtual void ConfigureApplicationContext()
        {

        }

        /// <summary>
        /// Registers the DependencyResolver implementation with the MVC runtime.
        /// <remarks>
        /// You must override this method in a derived class to control the manner in which the
        /// <see cref="SpringMvcDependencyResolver"/> is registered.
        /// </remarks>
        /// </summary>
        public virtual void RegisterDependencyResolver(IDependencyResolver resolver)
        {
            ThreadSafeDependencyResolverRegistrar.Register(resolver);
        }

        /// <summary>
        /// Thread-safe class that ensures that the <see cref="IDependencyResolver"/> is registered only once.
        /// </summary>
        protected static class ThreadSafeDependencyResolverRegistrar
        {
            private static bool _isInitialized = false;
            private static readonly Object @lock = new Object();

            /// <summary>
            /// Registers the specified resolver.
            /// </summary>
            /// <param name="resolver">The resolver.</param>
            public static void Register(IDependencyResolver resolver)
            {
                if (_isInitialized)
                {
                    return;
                }

                lock (@lock)
                {
                    if (_isInitialized)
                    {
                        return;
                    }

                    DependencyResolver.SetResolver(resolver);

                    _isInitialized = true;
                }
            }
        }

    }
}
