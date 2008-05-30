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
using System.ServiceModel;
using System.ServiceModel.Activation;

using Spring.Util;

#endregion

namespace Spring.ServiceModel.Activation
{
    /// <summary>
    /// Factory that provides instances of <see cref="Spring.ServiceModel.ServiceHost" /> 
    /// to host objects created by Spring's IoC container.
    /// </summary>
    /// <author>Bruno Baia</author>
    /// <version>$Id: ServiceHostFactory.cs,v 1.2 2007/05/18 21:04:37 bbaia Exp $</version>
    public class ServiceHostFactory : ServiceHostFactoryBase
    {
        /// <summary>
        /// Creates a <see cref="Spring.ServiceModel.ServiceHost"/> for 
        /// a specified Spring-managed object with a specific base address.
        /// </summary>
        /// <param name="reference">
        /// A reference to a Spring-managed object 'contextName:objectName'.
        /// </param>
        /// <param name="baseAddresses">
        /// The <see cref="System.Array"/> of type <see cref="System.Uri"/> that contains 
        /// the base addresses for the service hosted.
        /// </param>
        /// <returns>
        /// A <see cref="Spring.ServiceModel.ServiceHost"/> for the Spring-managed object.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// If the Service attribute in the ServiceHost directive was not provided.
        /// </exception>
        public override ServiceHostBase CreateServiceHost(string reference, Uri[] baseAddresses)
        {
            if (StringUtils.IsNullOrEmpty(reference))
            {
                throw new ArgumentException("The service name was not provided in the ServiceHost directive.", "Service");
            }

            string[] refSplitted = reference.Split(':');
            if (refSplitted.Length == 2)
            {
                return new ServiceHost(refSplitted[0], refSplitted[1], baseAddresses);
            }
            else
            {
                return new ServiceHost(reference, baseAddresses);
            }
        }
    }
}
#endif