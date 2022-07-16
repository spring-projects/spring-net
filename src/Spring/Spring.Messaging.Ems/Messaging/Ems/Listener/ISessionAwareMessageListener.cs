#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

namespace Spring.Messaging.Ems.Listener
{
    /// <summary>
    /// Variant of the standard EMS MessageListener interface,
    /// offering not only the received Message but also the underlying
    /// Session object. The latter can be used to send reply messages,
    /// without the need to access an external Connection/Session,
    /// i.e. without the need to access the underlying ConnectionFactory.
    /// </summary>
    /// <remarks>
    /// Supported by Spring's <see cref="SimpleMessageListenerContainer"/>
    /// as direct alternative to the standard MessageListener interface.
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface ISessionAwareMessageListener
    {
        /// <summary> Callback for processing a received EMS message.
        /// Implementors are supposed to process the given Message,
        /// typically sending reply messages through the given Session.
        /// </summary>
        /// <param name="message">the received EMS message
        /// </param>
        /// <param name="session">the underlying EMS Session
        /// </param>
        /// <throws>  EMSException if thrown by EMS methods </throws>
        void OnMessage(Message message, ISession session);
    }
}
