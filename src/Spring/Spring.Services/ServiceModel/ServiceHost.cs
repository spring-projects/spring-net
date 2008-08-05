#if NET_3_0
#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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

#endregion

namespace Spring.ServiceModel
{
    /// <summary>
    /// Provides a host for Spring-managed services.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class ServiceHost : System.ServiceModel.ServiceHost
    {
        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.ServiceModel.ServiceHost"/> class.
        /// </summary>
        /// <param name="serviceName">The name of the service within Spring's IoC container.</param>
        /// <param name="baseAddresses">The base addresses for the hosted service.</param>
        public ServiceHost(string serviceName, params Uri[] baseAddresses)
            : this(null, serviceName, baseAddresses)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.ServiceModel.ServiceHost"/> class.
        /// </summary>
        /// <param name="contextName">The name of the context to use.</param>
        /// <param name="serviceName">The name of the service within Spring's IoC container.</param>
        /// <param name="baseAddresses">The base addresses for the hosted service.</param>
        public ServiceHost(string contextName, string serviceName, params Uri[] baseAddresses)
            : base(CreateServiceType(contextName, serviceName), baseAddresses)
        {
        }

        private static Type CreateServiceType(string contextName, string serviceName)
        {
            if (StringUtils.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentException("The service name cannot be null or an empty string.", "serviceName");
            }

            IApplicationContext ctx;
            if (StringUtils.IsNullOrEmpty(contextName))
            {
                ctx = ContextRegistry.GetContext();
                if (ctx == null)
                {
                    throw new ArgumentException("No context registered.");
                }
            }
            else
            {
                ctx = ContextRegistry.GetContext(contextName);
                if (ctx == null)
                {
                    throw new ArgumentException(string.Format("Context '{0}' is not registered.", contextName));
                }
            }
            Type serviceType = ctx.GetType(serviceName);

            return new ServiceProxyTypeBuilder(contextName, serviceName, serviceType).BuildProxyType();
        }

        #endregion
    }
}
#endif