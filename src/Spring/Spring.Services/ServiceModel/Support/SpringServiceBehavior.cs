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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

using Spring.Context;
using Spring.Context.Support;
using Spring.Util;

#endregion

namespace Spring.ServiceModel.Support
{
    /// <summary>
    /// An implementation of IServiceBehavior that applies the <see cref="SpringInstanceProvider"/> to all endpoints.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class SpringServiceBehavior : IServiceBehavior
    {
        private SpringInstanceProvider springInstanceProvider;

        private string contextName;

        /// <summary>
        /// Gets or sets the spring instance provider.
        /// </summary>
        /// <value>The spring instance provider.</value>
        public SpringInstanceProvider InstanceProvider
        {
            get { return springInstanceProvider; }
        }


        /// <summary>
        /// Gets the name of the spring context used when creating this behavior.
        /// </summary>
        /// <value>The name of the context.</value>
        public string ContextName
        {
            get { return contextName; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringServiceBehavior"/> class and initializes the
        /// SpringInstanceProvider
        /// </summary>
        public SpringServiceBehavior() : this((string)null)
        {
            springInstanceProvider = new SpringInstanceProvider(ContextRegistry.GetContext());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringServiceBehavior"/> class with a given Spring
        /// context name.
        /// </summary>
        /// <param name="contextName">Name of the context.</param>
        public SpringServiceBehavior(string contextName)
        {
            IApplicationContext applicationContext;
            if (StringUtils.IsNullOrEmpty(contextName))
            {
                applicationContext = ContextRegistry.GetContext();
            }
            else
            {
                applicationContext = ContextRegistry.GetContext(contextName);
            }
            this.contextName = contextName;
            springInstanceProvider = new SpringInstanceProvider(applicationContext);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringServiceBehavior"/> class.
        /// </summary>
        /// <param name="applicationContext">The application context.</param>
        public SpringServiceBehavior(IApplicationContext applicationContext)
        {
            springInstanceProvider = new SpringInstanceProvider(applicationContext);
        }

        #region IServiceBehavior Members

        /// <summary>
        /// Provides the ability to pass custom data to binding elements to support the contract implementation.
        /// No-op implementation
        /// </summary>
        /// <param name="serviceDescription">The service description of the service.</param>
        /// <param name="serviceHostBase">The host of the service.</param>
        /// <param name="endpoints">The service endpoints.</param>
        /// <param name="bindingParameters">Custom objects to which binding elements have access.</param>
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, 
            Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Add the SpringInstanceProvider to all endpoint dispatcher.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The host that is currently being built.</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcherBase cdb in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher cd = cdb as ChannelDispatcher;
                if (cd != null)
                {
                    foreach (EndpointDispatcher ed in cd.Endpoints)
                    {
                        InstanceProvider.ServiceType = serviceDescription.ServiceType;
                        ed.DispatchRuntime.InstanceProvider = InstanceProvider;
                    }
                }
            }
        }

        /// <summary>
        /// Provides the ability to inspect the service host and the service description to confirm that the service can run successfully.        
        /// </summary>
        /// <remarks>
        /// No-op implementation
        /// </remarks>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The service host that is currently being constructed.</param>
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        #endregion
    }
}
#endif