#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using Spring.Messaging.Ems.Common;
using TIBCO.EMS;

namespace Spring.Messaging.Ems.Connections
{

    public class TestMessageProducer : IMessageProducer
    {
        private long timeToLive = 60;
        private int deliveryMode = Message.DEFAULT_DELIVERY_MODE;
        private MessageDeliveryMode messageDeliveryMode = MessageDeliveryMode.NonPersistent;

        public void Send(Message message)
        {
            throw new NotImplementedException();
        }

        public void Send(Destination destination, Message message)
        {
            throw new NotImplementedException();
        }

        public void Send(Message message, int deliveryMode, int priority, long timeToLive)
        {
            throw new NotImplementedException();
        }

        public void Send(Message message, MessageDeliveryMode deliveryMode, int priority, long timeToLive)
        {
            throw new NotImplementedException();
        }

        public void Send(Destination dest, Message message, int deliveryMode, int priority, long timeToLive)
        {
            throw new NotImplementedException();
        }

        public void Send(Destination dest, Message message, MessageDeliveryMode deliveryMode, int priority, long timeToLive)
        {
            throw new NotImplementedException();
        }


        public void Close()
        {
            throw new NotImplementedException();
        }




        public long TimeToLive
        {
            get { return timeToLive; }
            set { timeToLive = value; }
        }


        public int Priority
        {
            get { return 1; }
            set {  }
        }

        public bool DisableMessageID
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool DisableMessageTimestamp
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public MessageProducer NativeMessageProducer
        {
            get { throw new NotImplementedException(); }
        }

        public int DeliveryMode
        {
            get { return deliveryMode; }
            set { deliveryMode = value; }
        }

        public Destination Destination
        {
            get { throw new NotImplementedException(); }
        }

        public MessageDeliveryMode MsgDeliveryMode
        {
            get { return messageDeliveryMode; }
            set { messageDeliveryMode = value; }
        }
    }
}