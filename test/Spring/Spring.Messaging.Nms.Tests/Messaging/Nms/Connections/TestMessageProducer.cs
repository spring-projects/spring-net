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

using System;
using System.Threading.Tasks;
using Apache.NMS;

namespace Spring.Messaging.Nms.Connections
{

    public class TestMessageProducer : IMessageProducer
    {
        private MsgDeliveryMode msgDeliveryMode;
        private TimeSpan timeToLive = new TimeSpan(0,0,0,60,0);
        private TimeSpan requestTimeout;

        public void Send(IMessage message)
        {
            throw new NotImplementedException();
        }

        public void Send(IMessage message, MsgDeliveryMode deliveryMode, MsgPriority priority, TimeSpan timeToLive)
        {
            throw new NotImplementedException();
        }

        public void Send(IDestination destination, IMessage message)
        {
            throw new NotImplementedException();
        }

        public void Send(IDestination destination, IMessage message, MsgDeliveryMode deliveryMode, MsgPriority priority, TimeSpan timeToLive)
        {
            throw new NotImplementedException();
        }

        public Task SendAsync(IMessage message)
        {
            throw new NotImplementedException();
        }

        public Task SendAsync(IMessage message, MsgDeliveryMode deliveryMode, MsgPriority priority, TimeSpan timeToLive)
        {
            throw new NotImplementedException();
        }

        public Task SendAsync(IDestination destination, IMessage message)
        {
            throw new NotImplementedException();
        }

        public Task SendAsync(IDestination destination, IMessage message, MsgDeliveryMode deliveryMode, MsgPriority priority, TimeSpan timeToLive)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public Task CloseAsync()
        {
            throw new NotImplementedException();
        }

        public IMessage CreateMessage()
        {
            throw new NotImplementedException();
        }

        public Task<IMessage> CreateMessageAsync()
        {
            throw new NotImplementedException();
        }

        public ITextMessage CreateTextMessage()
        {
            throw new NotImplementedException();
        }

        public Task<ITextMessage> CreateTextMessageAsync()
        {
            throw new NotImplementedException();
        }

        public ITextMessage CreateTextMessage(string text)
        {
            throw new NotImplementedException();
        }

        public Task<ITextMessage> CreateTextMessageAsync(string text)
        {
            throw new NotImplementedException();
        }

        public IMapMessage CreateMapMessage()
        {
            throw new NotImplementedException();
        }

        public Task<IMapMessage> CreateMapMessageAsync()
        {
            throw new NotImplementedException();
        }

        public IObjectMessage CreateObjectMessage(object body)
        {
            throw new NotImplementedException();
        }

        public Task<IObjectMessage> CreateObjectMessageAsync(object body)
        {
            throw new NotImplementedException();
        }

        public IBytesMessage CreateBytesMessage()
        {
            throw new NotImplementedException();
        }

        public Task<IBytesMessage> CreateBytesMessageAsync()
        {
            throw new NotImplementedException();
        }

        public IBytesMessage CreateBytesMessage(byte[] body)
        {
            throw new NotImplementedException();
        }

        public Task<IBytesMessage> CreateBytesMessageAsync(byte[] body)
        {
            throw new NotImplementedException();
        }

        public IStreamMessage CreateStreamMessage()
        {
            throw new NotImplementedException();
        }

        public Task<IStreamMessage> CreateStreamMessageAsync()
        {
            throw new NotImplementedException();
        }

        public ProducerTransformerDelegate ProducerTransformer
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public MsgDeliveryMode DeliveryMode
        {
            get { return msgDeliveryMode; }
            set { msgDeliveryMode = value; }
        }

        public TimeSpan TimeToLive
        {
            get { return timeToLive; }
            set { timeToLive = value; }
        }

        public TimeSpan RequestTimeout
        {
            get { return requestTimeout; }
            set { requestTimeout = value; }
        }

        public MsgPriority Priority
        {
            get { return MsgPriority.Normal; }
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

        public TimeSpan DeliveryDelay { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}