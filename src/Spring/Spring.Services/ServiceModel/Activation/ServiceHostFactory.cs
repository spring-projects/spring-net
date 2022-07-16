#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.ServiceModel;

using Spring.Util;
using Spring.Context.Support;
using Spring.Context;

namespace Spring.ServiceModel.Activation
{
    /// <summary>
    /// Factory that provides instances of <see cref="Spring.ServiceModel.SpringServiceHost" />
    /// to host objects created by Spring's IoC container.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class ServiceHostFactory : System.ServiceModel.Activation.ServiceHostFactory
    {
        /// <summary>
        /// Creates a <see cref="Spring.ServiceModel.SpringServiceHost"/> for
        /// a specified Spring-managed object with a specific base address.
        /// </summary>
        /// <param name="reference">
        /// A reference to a Spring-managed object or to a service type.
        /// </param>
        /// <param name="baseAddresses">
        /// The <see cref="System.Array"/> of type <see cref="System.Uri"/> that contains
        /// the base addresses for the service hosted.
        /// </param>
        /// <returns>
        /// A <see cref="Spring.ServiceModel.SpringServiceHost"/> for the Spring-managed object.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// If the Service attribute in the ServiceHost directive was not provided.
        /// </exception>
        public override ServiceHostBase CreateServiceHost(string reference, Uri[] baseAddresses)
        {
            if (StringUtils.IsNullOrEmpty(reference))
            {
                return base.CreateServiceHost(reference, baseAddresses);
            }

            IApplicationContext applicationContext = ContextRegistry.GetContext();
            if (applicationContext.ContainsObject(reference))
            {
                return new SpringServiceHost(reference, applicationContext, baseAddresses);
            }

            return base.CreateServiceHost(reference, baseAddresses);
        }
    }
}
