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
    public class EmsMessageProducer : IMessageProducer
    {
        private MessageProducer nativeMessageProducer;

        public EmsMessageProducer(MessageProducer messageProducer)
        {
            this.nativeMessageProducer = messageProducer;
        }

        #region Implementation of IMessageProducer

        public MessageProducer NativeMessageProducer
        {
            get { return this.nativeMessageProducer; }
        }

        public void Close()
        {
            nativeMessageProducer.Close();
        }

        public void Send(Message message)
        {
            nativeMessageProducer.Send(message);
        }

        public void Send(Destination dest, Message message)
        {
            nativeMessageProducer.Send(dest, message);
        }

        public void Send(Message message, int deliveryMode, int priority, long timeToLive)
        {
            nativeMessageProducer.Send(message, deliveryMode, priority, timeToLive);
        }

        public void Send(Message message, MessageDeliveryMode deliveryMode, int priority, long timeToLive)
        {
            nativeMessageProducer.Send(message, deliveryMode, priority, timeToLive);
        }

        public void Send(Destination dest, Message message, int deliveryMode, int priority, long timeToLive)
        {
            nativeMessageProducer.Send(dest, message, deliveryMode, priority, timeToLive);
        }

        public void Send(Destination dest, Message message, MessageDeliveryMode deliveryMode, int priority, long timeToLive)
        {
            nativeMessageProducer.Send(dest, message, deliveryMode, priority, timeToLive);
        }

        public int DeliveryMode
        {
            get { return nativeMessageProducer.DeliveryMode; }
            set { nativeMessageProducer.DeliveryMode = value; }
        }

        public Destination Destination
        {
            get { return nativeMessageProducer.Destination; }
        }

        public bool DisableMessageID
        {
            get { return nativeMessageProducer.DisableMessageID; }
            set { nativeMessageProducer.DisableMessageID = value; }
        }

        public bool DisableMessageTimestamp
        {
            get { return nativeMessageProducer.DisableMessageTimestamp; }
            set { nativeMessageProducer.DisableMessageTimestamp = value; }
        }

        public MessageDeliveryMode MsgDeliveryMode
        {
            get { return nativeMessageProducer.MsgDeliveryMode; }
            set { nativeMessageProducer.MsgDeliveryMode = value; }
        }

        public int Priority
        {
            get { return nativeMessageProducer.Priority; }
            set { nativeMessageProducer.Priority = value; }
        }

        public long TimeToLive
        {
            get { return nativeMessageProducer.TimeToLive; }
            set { nativeMessageProducer.TimeToLive = value; }
        }

        #endregion
    }
}
