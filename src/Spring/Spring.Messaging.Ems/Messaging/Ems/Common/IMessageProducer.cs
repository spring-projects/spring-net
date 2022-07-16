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

namespace Spring.Messaging.Ems.Common
{
    public interface IMessageProducer
    {
        MessageProducer NativeMessageProducer { get; }

        void Close();
        void Send(Message message);
        void Send(TIBCO.EMS.Destination dest, Message message);
        void Send(Message message, int deliveryMode, int priority, long timeToLive);
        void Send(Message message, MessageDeliveryMode deliveryMode, int priority, long timeToLive);
        void Send(TIBCO.EMS.Destination dest, Message message, int deliveryMode, int priority, long timeToLive);
        void Send(TIBCO.EMS.Destination dest, Message message, MessageDeliveryMode deliveryMode, int priority, long timeToLive);
        string ToString();
        int DeliveryMode { get; set; }
        TIBCO.EMS.Destination Destination { get; }
        bool DisableMessageID { get; set; }
        bool DisableMessageTimestamp { get; set; }
        MessageDeliveryMode MsgDeliveryMode { get; set; }
        int Priority { get; set; }

        /// <summary>
        /// Gets or sets the the default length of time in milliseconds from its dispatch time
        /// that a produced message should be retained by the message system.
        /// </summary>
        /// <remarks>Time to live is set to zero by default.</remarks>
        /// <value>The message time to live in milliseconds; zero is unlimited</value>
        long TimeToLive { get; set; }
    }
}
