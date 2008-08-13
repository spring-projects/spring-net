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
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;

using Spring.Context;
using System.Globalization;

#endregion

namespace Spring.ServiceModel.Support
{
    /// <summary>
    /// An IInstanceProvider implementation that delegates to the Spring container to instantiate a given
    /// service type.  This allows for the service intstance to be configured via dependency injection and
    /// have AOP advice applied
    /// </summary>
    /// <author>Bruno Baia</author>
    public class SpringInstanceProvider : IInstanceProvider
    {
        private IApplicationContext applicationContext;

        private Type serviceType;


        /// <summary>
        /// Gets or sets the application context.
        /// </summary>
        /// <value>The application context.</value>
        public IApplicationContext ApplicationContext
        {
            get { return applicationContext; }
            set { applicationContext = value; }

        }


        /// <summary>
        /// Gets or sets the type of the service.
        /// </summary>
        /// <value>The type of the service.</value>
        public Type ServiceType
        {
            get { return serviceType; }
            set { serviceType = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringInstanceProvider"/> class.
        /// </summary>
        /// <param name="applicationContext">The application context.</param>
        public SpringInstanceProvider(IApplicationContext applicationContext)
        {
            ApplicationContext = applicationContext;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringInstanceProvider"/> class.
        /// </summary>
        /// <param name="applicationContext">The application context.</param>
        /// <param name="serviceType">The type.</param>
        public SpringInstanceProvider(IApplicationContext applicationContext, Type serviceType)
        {
            ApplicationContext = applicationContext;
            ServiceType = serviceType;
        } 

        #region IInstanceProvider Members

        /// <summary>
        /// Returns a service object given the specified <see cref="T:System.ServiceModel.InstanceContext"/> object.
        /// </summary>
        /// <param name="instanceContext">The current <see cref="T:System.ServiceModel.InstanceContext"/> object.</param>
        /// <returns>A user-defined service object.</returns>
        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        /// <summary>
        /// Returns a service object given the specified <see cref="T:System.ServiceModel.InstanceContext"/> object.
        /// Delegates to the Spring container to instantiate the given object type.
        /// </summary>
        /// <param name="instanceContext">The current <see cref="T:System.ServiceModel.InstanceContext"/> object.</param>
        /// <param name="message">The message that triggered the creation of a service object.</param>
        /// <returns>The service object.</returns>
        public object GetInstance(InstanceContext instanceContext, Message message)
        {          
            string[] objectNames = ApplicationContext.GetObjectNamesForType(ServiceType);
            if (objectNames.Length != 1)
            {
                throw new InvalidOperationException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    "There must exist exactly one object definition of type '{0}' in the Spring container.",
                    ServiceType.FullName)
                    );
            }
            return ApplicationContext.GetObject(objectNames[0]);
        }

        /// <summary>
        /// Called when an <see cref="T:System.ServiceModel.InstanceContext"/> object recycles a service object.
        /// A no-op implementation
        /// </summary>
        /// <param name="instanceContext">The service's instance context.</param>
        /// <param name="instance">The service object to be recycled.</param>
        public void ReleaseInstance(InstanceContext instanceContext, object instance) 
        { 
        }

        #endregion
    }
}
#endif