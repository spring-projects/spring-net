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

using System.Collections;
using Apache.NMS;
using Apache.NMS.Util;
using Spring.Messaging.Nms.Support;

namespace Spring.Messaging.Nms.Connections
{
    public class NmsProducer : INMSProducer
    {
        private readonly IMessageProducer producer;
        private readonly ISession session;
        private String correlationId;
        private String type;
        private IDestination replyTo;
        private readonly IPrimitiveMap messageProperties = new PrimitiveMap();

        public NmsProducer(ISession session)
        {
            this.session = session;
            this.producer = session.CreateProducer();
        }

        public void Dispose()
        {
            producer.Dispose();
        }

        public INMSProducer Send(IDestination destination, IMessage message)
        {
            return SendAsync(destination, message).GetAsyncResult();
        }

        public INMSProducer Send(IDestination destination, string body)
        {
            return SendAsync(destination, body).GetAsyncResult();
        }

        public INMSProducer Send(IDestination destination, IPrimitiveMap body)
        {
            return SendAsync(destination, body).GetAsyncResult();
        }

        public INMSProducer Send(IDestination destination, byte[] body)
        {
            return SendAsync(destination, body).GetAsyncResult();
        }

        public INMSProducer Send(IDestination destination, object body)
        {
            return SendAsync(destination, body).GetAsyncResult();
        }

        public async Task<INMSProducer> SendAsync(IDestination destination, IMessage message)
        {
            CopyMap(messageProperties, message.Properties);
            await producer.SendAsync(destination, message).Awaiter();
            return this;
        }

        public async Task<INMSProducer> SendAsync(IDestination destination, string body)
        {
            var message = await CreateTextMessageAsync(body).Awaiter();
            return await SendAsync(destination, message).Awaiter();
        }

        public async Task<INMSProducer> SendAsync(IDestination destination, IPrimitiveMap body)
        {
            var message = await CreateMapMessageAsync().Awaiter();
            CopyMap(body, message.Body);

            return await SendAsync(destination, message).Awaiter();
        }

        public async Task<INMSProducer> SendAsync(IDestination destination, byte[] body)
        {
            var message = await CreateBytesMessageAsync(body).Awaiter();
            return await SendAsync(destination, message).Awaiter();
        }

        public async Task<INMSProducer> SendAsync(IDestination destination, object body)
        {
            var message = await CreateObjectMessageAsync(body).Awaiter();
            return await SendAsync(destination, message).Awaiter();
        }

        public INMSProducer ClearProperties()
        {
            messageProperties.Clear();
            return this;
        }

        public INMSProducer SetDeliveryDelay(TimeSpan deliveryDelay)
        {
            DeliveryDelay = deliveryDelay;
            return this;
        }

        public INMSProducer SetTimeToLive(TimeSpan timeToLive)
        {
            TimeToLive = timeToLive;
            return this;
        }

        public INMSProducer SetDeliveryMode(MsgDeliveryMode deliveryMode)
        {
            DeliveryMode = deliveryMode;
            return this;
        }

        public INMSProducer SetDisableMessageID(bool value)
        {
            DisableMessageID = value;
            return this;
        }

        public INMSProducer SetDisableMessageTimestamp(bool value)
        {
            DisableMessageTimestamp = value;
            return this;
        }

        public INMSProducer SetNMSCorrelationID(string correlationID)
        {
            NMSCorrelationID = correlationID;
            return this;
        }

        public INMSProducer SetNMSReplyTo(IDestination replyTo)
        {
            NMSReplyTo = replyTo;
            return this;
        }

        public INMSProducer SetNMSType(string type)
        {
            NMSType = type;
            return this;
        }

        public INMSProducer SetPriority(MsgPriority priority)
        {
            Priority = priority;
            return this;
        }

        public INMSProducer SetProperty(string name, bool value)
        {
            messageProperties.SetBool(name, value);
            return this;
        }

        public INMSProducer SetProperty(string name, byte value)
        {
            messageProperties.SetByte(name, value);
            return this;
        }

        public INMSProducer SetProperty(string name, double value)
        {
            messageProperties.SetDouble(name, value);
            return this;
        }

        public INMSProducer SetProperty(string name, float value)
        {
            messageProperties.SetFloat(name, value);
            return this;
        }

        public INMSProducer SetProperty(string name, int value)
        {
            messageProperties.SetInt(name, value);
            return this;
        }

        public INMSProducer SetProperty(string name, long value)
        {
            messageProperties.SetLong(name, value);
            return this;
        }

        public INMSProducer SetProperty(string name, short value)
        {
            messageProperties.SetShort(name, value);
            return this;
        }

        public INMSProducer SetProperty(string name, char value)
        {
            messageProperties.SetChar(name, value);
            return this;
        }

        public INMSProducer SetProperty(string name, string value)
        {
            messageProperties.SetString(name, value);
            return this;
        }

        public INMSProducer SetProperty(string name, byte[] value)
        {
            messageProperties.SetBytes(name, value);
            return this;
        }

        public INMSProducer SetProperty(string name, IList value)
        {
            messageProperties.SetList(name, value);
            return this;
        }

        public INMSProducer SetProperty(string name, IDictionary value)
        {
            messageProperties.SetDictionary(name, value);
            return this;
        }

        public IMessage CreateMessage()
        {
            return session.CreateMessage();
        }

        public Task<IMessage> CreateMessageAsync()
        {
            return session.CreateMessageAsync();
        }

        public ITextMessage CreateTextMessage()
        {
            return session.CreateTextMessage();
        }

        public Task<ITextMessage> CreateTextMessageAsync()
        {
            return session.CreateTextMessageAsync();
        }

        public ITextMessage CreateTextMessage(string text)
        {
            return session.CreateTextMessage(text);
        }

        public Task<ITextMessage> CreateTextMessageAsync(string text)
        {
            return session.CreateTextMessageAsync(text);
        }

        public IMapMessage CreateMapMessage()
        {
            return session.CreateMapMessage();
        }

        public Task<IMapMessage> CreateMapMessageAsync()
        {
            return session.CreateMapMessageAsync();
        }

        public IObjectMessage CreateObjectMessage(object body)
        {
            return session.CreateObjectMessage(body);
        }

        public Task<IObjectMessage> CreateObjectMessageAsync(object body)
        {
            return session.CreateObjectMessageAsync(body);
        }

        public IBytesMessage CreateBytesMessage()
        {
            return session.CreateBytesMessage();
        }

        public Task<IBytesMessage> CreateBytesMessageAsync()
        {
            return session.CreateBytesMessageAsync();
        }

        public IBytesMessage CreateBytesMessage(byte[] body)
        {
            return session.CreateBytesMessage(body);
        }

        public Task<IBytesMessage> CreateBytesMessageAsync(byte[] body)
        {
            return session.CreateBytesMessageAsync(body);
        }

        public IStreamMessage CreateStreamMessage()
        {
            return session.CreateStreamMessage();
        }

        public Task<IStreamMessage> CreateStreamMessageAsync()
        {
            return session.CreateStreamMessageAsync();
        }

        public void Close()
        {
            producer.Close();
        }

        public Task CloseAsync()
        {
            return producer.CloseAsync();
        }

        public IPrimitiveMap Properties => messageProperties;
        public string NMSCorrelationID
        {
            get => correlationId;
            set => correlationId = value;
        }
        public IDestination NMSReplyTo
        {
            get => replyTo;
            set => replyTo = value;
        }
        public string NMSType
        {
            get => type;
            set => type = value;
        }


        public ProducerTransformerDelegate ProducerTransformer {
            get => producer.ProducerTransformer;
            set => producer.ProducerTransformer = value;
        }
        public MsgDeliveryMode DeliveryMode { get => producer.DeliveryMode; set => producer.DeliveryMode = value; }
        public TimeSpan DeliveryDelay { get => producer.DeliveryDelay;
            set => producer.DeliveryDelay = value;
        }
        public TimeSpan TimeToLive
        {
            get => producer.TimeToLive;
            set => producer.TimeToLive = value;
        }

        public TimeSpan RequestTimeout
        {
            get => producer.RequestTimeout;
            set => producer.RequestTimeout = value;
        }
        public MsgPriority Priority
        {
            get => producer.Priority;
            set => producer.Priority = value;
        }
        public bool DisableMessageID
        {
            get => producer.DisableMessageID;
            set => producer.DisableMessageID = value;
        }
        public bool DisableMessageTimestamp
        {
            get => producer.DisableMessageTimestamp;
            set => producer.DisableMessageTimestamp = value;
        }

        private void CopyMap(IPrimitiveMap source, IPrimitiveMap target)
        {
            foreach (object key in source.Keys)
            {
                string name = key.ToString();
                target[name] = source[name];
            }
        }

    }
}
