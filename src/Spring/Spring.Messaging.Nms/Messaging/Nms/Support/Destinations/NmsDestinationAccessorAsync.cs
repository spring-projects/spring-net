#region License
// /*
//  * Copyright 2022 the original author or authors.
//  *
//  * Licensed under the Apache License, Version 2.0 (the "License");
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  *      http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */
#endregion

using Apache.NMS;
using Spring.Util;

namespace Spring.Messaging.Nms.Support.Destinations
{
    /// <summary>
    /// Async version of NmsDestinationAccessor
    /// </summary>
    /// <see cref="NmsDestinationAccessor"/>
    public class NmsDestinationAccessorAsync : NmsAccessorAsync
    {
        #region Fields
        
        private IDestinationResolver destinationResolver = new DynamicDestinationResolver();

        private bool pubSubDomain = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the destination resolver that is to be used to resolve
        /// IDestination references for this accessor.
        /// </summary>
        /// <remarks>The default resolver is a DynamicDestinationResolver. Specify a
        /// JndiDestinationResolver for resolving destination names as JNDI locations.
        /// </remarks>
        /// <value>The destination resolver.</value>
        virtual public IDestinationResolver DestinationResolver
        {
            get
            {
                return destinationResolver;
            }

            set
            {
                AssertUtils.ArgumentNotNull(value, "DestinationResolver must not be null");
                this.destinationResolver = value;
            }

        }


        /// <summary>
        /// Gets or sets a value indicating whether Publish/Subscribe
        /// domain (Topics) is used. Otherwise, the Point-to-Point domain
        /// (Queues) is used.
        /// 
        /// </summary>
        /// <remarks>this
        /// setting tells what type of destination to create if dynamic destinations are enabled.</remarks>
        /// <value><c>true</c> if Publish/Subscribe domain; otherwise, <c>false</c>
        /// for the Point-to-Point domain.</value>
        public virtual bool PubSubDomain
        {
            get
            {
                return pubSubDomain;
            }

            set
            {
                this.pubSubDomain = value;
            }

        }
		
        #endregion

        /// <summary>
        /// Resolves the given destination name to a NMS destination.
        /// </summary>
        /// <param name="session">The current session.</param>
        /// <param name="destinationName">Name of the destination.</param>
        /// <returns>The located IDestination</returns>
        /// <exception cref="NMSException">If resolution failed.</exception>
        public virtual Task<IDestination> ResolveDestinationName(ISession session, System.String destinationName)
        {
            return Task.FromResult(DestinationResolver.ResolveDestinationName(session, destinationName, PubSubDomain));
        }
    }
}