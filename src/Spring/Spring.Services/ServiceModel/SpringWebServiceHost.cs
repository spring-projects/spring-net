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

using Spring.Util;
using Spring.Context;
using Spring.Context.Support;
using Spring.ServiceModel.Support;
using Spring.Objects.Factory;
using System.ServiceModel.Web;

namespace Spring.ServiceModel
{
    /// <summary>
    /// Provides a host for Spring-managed services.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class SpringWebServiceHost : WebServiceHost
    {
        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.ServiceModel.SpringWebServiceHost"/> class.
        /// </summary>
        /// <param name="serviceName">The name of the service within Spring's IoC container.</param>
        /// <param name="baseAddresses">The base addresses for the hosted service.</param>
        public SpringWebServiceHost(string serviceName, params Uri[] baseAddresses)
            : this(serviceName, GetApplicationContext(null), baseAddresses)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.ServiceModel.SpringWebServiceHost"/> class.
        /// </summary>
        /// <param name="serviceName">The name of the service within Spring's IoC container.</param>
        /// <param name="contextName">The name of the Spring context to use.</param>
        /// <param name="baseAddresses">The base addresses for the hosted service.</param>
        public SpringWebServiceHost(string serviceName, string contextName, params Uri[] baseAddresses)
            : this(serviceName, GetApplicationContext(contextName), baseAddresses)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.ServiceModel.SpringWebServiceHost"/> class.
        /// </summary>
        /// <param name="serviceName">The name of the service within Spring's IoC container.</param>
        /// <param name="objectFactory">The <see cref="IObjectFactory"/> to use.</param>
        /// <param name="baseAddresses">The base addresses for the hosted service.</param>
        public SpringWebServiceHost(string serviceName, IObjectFactory objectFactory, params Uri[] baseAddresses)
            : this(serviceName, objectFactory, true, baseAddresses)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.ServiceModel.SpringWebServiceHost"/> class.
        /// </summary>
        /// <param name="serviceName">The name of the service within Spring's IoC container.</param>
        /// <param name="objectFactory">The <see cref="IObjectFactory"/> to use.</param>
        /// <param name="useServiceProxyTypeCache">Whether to cache the generated service proxy type.</param>
        /// <param name="baseAddresses">The base addresses for the hosted service.</param>
        public SpringWebServiceHost(string serviceName, IObjectFactory objectFactory, bool useServiceProxyTypeCache, params Uri[] baseAddresses)
            : base(CreateServiceType(serviceName, objectFactory, useServiceProxyTypeCache), baseAddresses)
        {
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

        private static Type CreateServiceType(string serviceName, IObjectFactory objectFactory, bool useServiceProxyTypeCache)
        {
            if (StringUtils.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentException("The service name cannot be null or an empty string.", "serviceName");
            }

            if (objectFactory.IsTypeMatch(serviceName, typeof(Type)))
            {
                return objectFactory.GetObject(serviceName) as Type;
            }

            return new ServiceProxyTypeBuilder(serviceName, objectFactory, useServiceProxyTypeCache)
                .BuildProxyType();
        }

        #endregion
    }
}
