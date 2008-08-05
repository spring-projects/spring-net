#if NET_3_5
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

namespace Spring.ServiceModel.Dispatcher
{
    public class SpringInstanceProvider : IInstanceProvider
    {
        public IApplicationContext ApplicationContext { get; set; }
        
        public Type ServiceType { get; set; }

        public SpringInstanceProvider(IApplicationContext applicationContext)
        {
            ApplicationContext = applicationContext;
        }

        public SpringInstanceProvider(IApplicationContext applicationContext, Type type)
        {
            ApplicationContext = applicationContext;
            ServiceType = type;
        } 

        #region IInstanceProvider Members

        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            object result = null;

            string[] objectNames = ApplicationContext.GetObjectNamesForType(ServiceType);
            if (objectNames.Length != 1)
            {
                // TODO : Exception type and message ?
                throw new InvalidOperationException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    "There must exist exactly one object definition of type '{0}' in the Spring container.",
                    ServiceType.FullName)
                    );
            }
            return ApplicationContext.GetObject(objectNames[0]);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance) 
        { 
        }

        #endregion
    }
}
#endif