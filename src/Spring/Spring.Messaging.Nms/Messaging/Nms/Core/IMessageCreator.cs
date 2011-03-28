#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

namespace Spring.Messaging.Nms.Core
{
    /// <summary> Creates a NMS message given a Session</summary>
    /// <remarks>
    /// <p>The Session typically is provided by an instance
    /// of the MessageTemplate class.</p>
    /// </remarks>
    /// <author>Mark Pollack</author>
    public interface IMessageCreator
    {
        /// <summary> Create a Message to be sent.</summary>
        /// <param name="session">the NMS Session to be used to create the
        /// <code>IMessage</code> (never <code>null</code>)
        /// </param>
        /// <returns> the <code>Message</code> to be sent
        /// </returns>
        /// <throws>NMSException if thrown by NMS API methods </throws>
        IMessage CreateMessage(ISession session);
    }
}
