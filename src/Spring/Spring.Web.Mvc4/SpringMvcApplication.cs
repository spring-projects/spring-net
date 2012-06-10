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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Spring.Objects.Factory;
using Spring.Core.IO;
using Spring.Objects.Factory.Xml;
using System.Web.Routing;
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

            var webApiResolver = BuildWebApiDependencyResolver();
            RegisterDependencyResolver(webApiResolver);
        }


        /// <summary>
        /// Builds the dependency resolver.
        /// </summary>
        /// <returns>The <see cref="System.Web.Mvc.IDependencyResolver"/> instance.</returns>
        /// You must override this method in a derived class to control the manner in which the
        /// <see cref="IDependencyResolver"/> is created.
        protected virtual System.Web.Mvc.IDependencyResolver BuildDependencyResolver()
        {
            return new SpringMvcDependencyResolver(ContextRegistry.GetContext());
        }

        /// <summary>
        /// Builds the dependency resolver.
        /// </summary>
        /// <returns>The <see cref="System.Web.Http.Dependencies.IDependencyResolver"/> instance.</returns>
        /// You must override this method in a derived class to control the manner in which the
        /// <see cref="System.Web.Http.Dependencies.IDependencyResolver"/> is created.
        protected virtual System.Web.Http.Dependencies.IDependencyResolver BuildWebApiDependencyResolver()
        {
            return new SpringWebApiDependencyResolver(ContextRegistry.GetContext());
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
        /// <see cref="System.Web.Mvc.IDependencyResolver"/> is registered.
        /// </remarks>
        /// </summary>
        public virtual void RegisterDependencyResolver(System.Web.Mvc.IDependencyResolver resolver)
        {
            ThreadSafeDependencyResolverRegistrar.Register(resolver);
        }

        /// <summary>
        /// Registers the DependencyResolver implementation with the MVC runtime.
        /// <remarks>
        /// You must override this method in a derived class to control the manner in which the
        /// <see cref="System.Web.Http.Dependencies.IDependencyResolver"/> is registered.
        /// </remarks>
        /// </summary>
        public virtual void RegisterDependencyResolver(System.Web.Http.Dependencies.IDependencyResolver resolver)
        {
            ThreadSafeDependencyResolverRegistrar.Register(resolver);
        }

        /// <summary>
        /// Thread-safe class that ensures that the <see cref="IDependencyResolver"/> is registered only once.
        /// </summary>
        protected static class ThreadSafeDependencyResolverRegistrar
        {
            private static bool _isMvcResolverRegistered = false;
            private static bool _isWebApiResolverRegistered = false;
            private static readonly Object Lock = new Object();

            /// <summary>
            /// Registers the specified <see cref="System.Web.Mvc.IDependencyResolver"/>.
            /// </summary>
            /// <param name="resolver">The resolver.</param>
            public static void Register(IDependencyResolver resolver)
            {
                if (_isMvcResolverRegistered)
                {
                    return;
                }

                lock (Lock)
                {
                    if (_isMvcResolverRegistered)
                    {
                        return;
                    }

                    DependencyResolver.SetResolver(resolver);

                    _isMvcResolverRegistered = true;
                }
            }

            /// <summary>
            /// Registers the specified <see cref="System.Web.Http.Dependencies.IDependencyResolver"/>.
            /// </summary>
            /// <param name="resolver">The resolver.</param>
            public static void Register(System.Web.Http.Dependencies.IDependencyResolver resolver)
            {
                if (_isWebApiResolverRegistered)
                {
                    return;
                }

                lock (Lock)
                {
                    if (_isWebApiResolverRegistered)
                    {
                        return;
                    }

                    System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver = resolver;

                    _isWebApiResolverRegistered = true;
                }
            }
        }

    }
}
