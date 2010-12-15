#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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

using Apache.NMS;

namespace Spring.Messaging.Nms.Support.Destinations
{
    /// <summary> Strategy interface for resolving NMS destinations.
    /// </summary>
    /// <remarks>
    /// <para>Used by MessageTemplate for resolving
    /// destination names from simple Strings to actual
    /// IDestination implementation instances.
    /// </para>
    /// 
    /// <para>The default DestinationResolver implementation used by
    /// MessageTemplate instances is the
    /// DynamicDestinationResolver class. Consider using the
    /// JndiDestinationResolver for more advanced scenarios.
    /// </para>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface IDestinationResolver
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
        IDestination ResolveDestinationName(ISession session, string destinationName, bool pubSubDomain);
	
    }
}
