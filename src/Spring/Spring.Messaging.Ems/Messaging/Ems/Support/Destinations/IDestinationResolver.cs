#region License

/*
 * Copyright � 2002-2010 the original author or authors.
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

using Spring.Messaging.Ems.Common;

namespace Spring.Messaging.Ems.Support.Destinations
{
    /// <summary> Strategy interface for resolving EMS destinations.
    /// </summary>
    /// <remarks>
    /// <para>Used by EmsTemplate for resolving
    /// destination names from simple Strings to actual
    /// Destination implementation instances.
    /// </para>
    ///
    /// <para>The default DestinationResolver implementation used by
    /// EmsTemplate instances is the
    /// DynamicDestinationResolver class. Consider using the
    /// JndDestinationResolver for more advanced scenarios.
    /// </para>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface IDestinationResolver
    {
        /// <summary> Resolve the given destination name, either as located resource
        /// or as dynamic destination.
        /// </summary>
        /// <param name="session">the current EMS Session
        /// </param>
        /// <param name="destinationName">the name of the destination
        /// </param>
        /// <param name="pubSubDomain"><code>true</code> if the domain is pub-sub, <code>false</code> if P2P
        /// </param>
        /// <returns> the EMS destination (either a topic or a queue)
        /// </returns>
        /// <throws>EMSException if resolution failed </throws>
        Destination ResolveDestinationName(ISession session, string destinationName, bool pubSubDomain);

    }
}
