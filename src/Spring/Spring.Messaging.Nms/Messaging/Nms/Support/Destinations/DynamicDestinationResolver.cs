#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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
using Apache.NMS;

namespace Spring.Messaging.Nms.Support.IDestinations
{
    /// <summary> Simple DestinationResolver implementation resolving destination names
    /// as dynamic destinations.</summary>
    ///
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class DynamicDestinationResolver : IDestinationResolver
    {
        /// <summary> Resolve the given destination name, either as located resource
        /// or as dynamic destination.
        /// </summary>
        /// <param name="session">the current NMS Session
        /// </param>
        /// <param name="destinationName">the name of the destination
        /// </param>
        /// <param name="pubSubDomain"><code>true</code> if the domain is pub-sub, <code>false</code> if P2P
        /// </param>
        /// <returns> the NMS destination (either a topic or a queue)
        /// </returns>
        /// <throws>NMSException if resolution failed </throws>
        public IDestination ResolveDestinationName(ISession session, string destinationName, bool pubSubDomain)
        {
            AssertUtils.ArgumentNotNull(session, "ISession must not be null");
            AssertUtils.ArgumentNotNull(destinationName, "IDestination name must not be null");
            if (pubSubDomain)
            {
                return ResolveTopic(session, destinationName);
            }
            else
            {
                return ResolveQueue(session, destinationName);
            }
        }


        /// <summary> Resolve the given destination name to a Topic.</summary>
        /// <param name="session">the current NMS Session
        /// </param>
        /// <param name="topicName">the name of the desired Topic.
        /// </param>
        /// <returns> the NMS Topic name
        /// </returns>
        /// <throws>NMSException if resolution failed </throws>
        protected internal virtual IDestination ResolveTopic(ISession session, System.String topicName)
        {
            return session.GetTopic(topicName);
        }

        /// <summary> Resolve the given destination name to a Queue.</summary>
        /// <param name="session">the current NMS Session
        /// </param>
        /// <param name="queueName">the name of the desired Queue.
        /// </param>
        /// <returns> the NMS Queue name
        /// </returns>
        /// <throws>NMSException if resolution failed </throws>
        protected internal virtual IDestination ResolveQueue(ISession session, string queueName)
        {
            return session.GetQueue(queueName);
        }
    }
}
