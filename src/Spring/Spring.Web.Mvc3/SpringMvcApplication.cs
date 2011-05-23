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
            RegisterResolver();
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
        public virtual void RegisterResolver()
        {
            DependencyResolver.SetResolver(new SpringMvcDependencyResolver(ContextRegistry.GetContext()));
        }

    }
}
