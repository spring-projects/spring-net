#if NET_3_0
#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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

using Spring.Util;
using Spring.Context;
using Spring.Context.Support;
using Spring.ServiceModel.Support;
using Spring.ServiceModel.Dispatcher;

#endregion

namespace Spring.ServiceModel
{
    /// <summary>
    /// Provides a host for Spring-managed services.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class SpringServiceHost : System.ServiceModel.ServiceHost
    {
        private bool useSpringServiceBehavior = false;

        public IApplicationContext ApplicationContext { get; set; }

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.ServiceModel.SpringServiceHost"/> class.
        /// </summary>
        /// <param name="serviceName">The name of the service within Spring's IoC container.</param>
        /// <param name="baseAddresses">The base addresses for the hosted service.</param>
        public SpringServiceHost(string serviceName, params Uri[] baseAddresses)
            : this(serviceName, GetApplicationContext(null), baseAddresses)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.ServiceModel.SpringServiceHost"/> class.
        /// </summary>
        /// <param name="serviceName">The name of the service within Spring's IoC container.</param>
        /// <param name="contextName">The name of the Spring context to use.</param>
        /// <param name="baseAddresses">The base addresses for the hosted service.</param>
        public SpringServiceHost(string serviceName, string contextName, params Uri[] baseAddresses)
            : this(serviceName, GetApplicationContext(contextName), baseAddresses)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.ServiceModel.SpringServiceHost"/> class.
        /// </summary>
        /// <param name="serviceName">The name of the service within Spring's IoC container.</param>
        /// <param name="applicationContext">The <see cref="IApplicationContext"/> to use.</param>
        /// <param name="baseAddresses">The base addresses for the hosted service.</param>
        public SpringServiceHost(string serviceName, IApplicationContext applicationContext, params Uri[] baseAddresses)
            : base(CreateServiceType(serviceName, applicationContext), baseAddresses)
        {
            ApplicationContext = applicationContext;
        }

        public SpringServiceHost(Type serviceType, params Uri[] baseAddresses)
            : this(serviceType, GetApplicationContext(null), baseAddresses)
        {
        }

        public SpringServiceHost(Type serviceType, IApplicationContext applicationContext, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            useSpringServiceBehavior = true;
            ApplicationContext = applicationContext;
        }

        private static IApplicationContext GetApplicationContext(string contextName)
        {
            if (StringUtils.IsNullOrEmpty(contextName))
            {
                return ContextRegistry.GetContext();
            }
            else
            {
                return ContextRegistry.GetContext(contextName);
            }
        }

        private static Type CreateServiceType(string serviceName, IApplicationContext applicationContext)
        {
            if (StringUtils.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentException("The service name cannot be null or an empty string.", "serviceName");
            }

            Type serviceType = applicationContext.GetType(serviceName);

            return new ServiceProxyTypeBuilder(serviceName, applicationContext, serviceType).BuildProxyType();
        }

        #endregion

        protected override void OnOpening()
        {
            if (this.useSpringServiceBehavior && 
                this.Description.Behaviors.Find<SpringServiceBehavior>() == null)
            {
                this.Description.Behaviors.Add(new SpringServiceBehavior(ApplicationContext));
            }
            base.OnOpening();
        }
    }
}
#endif