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

namespace Spring.ServiceModel.Dispatcher
{
    public class SpringServiceBehavior : IServiceBehavior
    {
        public SpringInstanceProvider InstanceProvider { get; set; }

        public string ContextName { get; set; }

        public SpringServiceBehavior()
        {
            IApplicationContext applicationContext;
            if (StringUtils.IsNullOrEmpty(ContextName))
            {
                applicationContext = ContextRegistry.GetContext();
            }
            else
            {
                applicationContext = ContextRegistry.GetContext(ContextName);
            }

            InstanceProvider = new SpringInstanceProvider(applicationContext);
        }

        public SpringServiceBehavior(IApplicationContext applicationContext)
        {
            InstanceProvider = new SpringInstanceProvider(applicationContext);
        }

        #region IServiceBehavior Members

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, 
            Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

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

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        #endregion
    }
}
#endif