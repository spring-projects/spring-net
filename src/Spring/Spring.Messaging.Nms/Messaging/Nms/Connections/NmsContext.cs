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

using Apache.NMS;
using Spring.Messaging.Nms.Support;

namespace Spring.Messaging.Nms.Connections
{
    internal class NmsContext: INMSContext
    {
        private readonly IConnection connection;
        private ISession session;
        private readonly SemaphoreSlimLock lockRoot = new SemaphoreSlimLock();
        private readonly AcknowledgementMode acknowledgementMode;
        private bool autoStart = true;
        
        public NmsContext(IConnection connection, AcknowledgementMode acknowledgementMode)
        {
            this.connection = connection;
            this.acknowledgementMode = acknowledgementMode;
        }
        
        public void Dispose()
        {
            connection.Dispose();
        }

        public void Start()
        {
            StartAsync().GetAsyncResult();
        }

        public Task StartAsync()
        {
            return connection.StartAsync();
        }

        public bool IsStarted => connection.IsStarted;

        public void Stop()
        {
            StopAsync().GetAsyncResult();
        }

        public Task StopAsync()
        {
            return connection.StopAsync();
        }

        public INMSContext CreateContext(AcknowledgementMode ackMode)
        {
            return new NmsContext(connection, ackMode);
        }

        public INMSProducer CreateProducer()
        {
            return CreateProducerAsync().GetAsyncResult();
        }

        public async Task<INMSProducer> CreateProducerAsync()
        {
            return new NmsProducer(await GetSessionAsync().Awaiter());
        }

        public INMSConsumer CreateConsumer(IDestination destination)
        {
            return PrepareConsumer(new NmsConsumer(session.CreateConsumer(destination)));
        }

        public INMSConsumer CreateConsumer(IDestination destination, string selector)
        {
            return PrepareConsumer(new NmsConsumer(session.CreateConsumer(destination, selector)));
        }

        public INMSConsumer CreateConsumer(IDestination destination, string selector, bool noLocal)
        {
            return PrepareConsumer(new NmsConsumer(session.CreateConsumer(destination, selector, noLocal)));
        }

        public INMSConsumer CreateDurableConsumer(ITopic destination, string subscriptionName)
        {
            return PrepareConsumer(new NmsConsumer(session.CreateDurableConsumer(destination, subscriptionName)));
        }

        public INMSConsumer CreateDurableConsumer(ITopic destination, string subscriptionName, string selector)
        {
            return PrepareConsumer(new NmsConsumer(session.CreateDurableConsumer(destination, subscriptionName, selector)));
        }

        public INMSConsumer CreateDurableConsumer(ITopic destination, string subscriptionName, string selector, bool noLocal)
        {
            return PrepareConsumer(new NmsConsumer(session.CreateDurableConsumer(destination, subscriptionName, selector, noLocal)));
        }

        public INMSConsumer CreateSharedConsumer(ITopic destination, string subscriptionName)
        {
            return PrepareConsumer(new NmsConsumer(session.CreateSharedConsumer(destination, subscriptionName)));
        }

        public INMSConsumer CreateSharedConsumer(ITopic destination, string subscriptionName, string selector)
        {
            return PrepareConsumer(new NmsConsumer(session.CreateSharedConsumer(destination, subscriptionName, selector)));
        }

        public INMSConsumer CreateSharedDurableConsumer(ITopic destination, string subscriptionName)
        {
            return PrepareConsumer(new NmsConsumer(session.CreateSharedDurableConsumer(destination, subscriptionName)));
        }

        public INMSConsumer CreateSharedDurableConsumer(ITopic destination, string subscriptionName, string selector)
        {
            return PrepareConsumer(new NmsConsumer(session.CreateSharedDurableConsumer(destination, subscriptionName, selector)));
        }

        public async Task<INMSConsumer> CreateConsumerAsync(IDestination destination)
        {
            return await PrepareConsumerAsync(new NmsConsumer(await session.CreateConsumerAsync(destination).Awaiter())).Awaiter();
        }

        public async Task<INMSConsumer> CreateConsumerAsync(IDestination destination, string selector)
        {
            return await PrepareConsumerAsync(new NmsConsumer(await session.CreateConsumerAsync(destination, selector).Awaiter())).Awaiter();
        }

        public async Task<INMSConsumer> CreateConsumerAsync(IDestination destination, string selector, bool noLocal)
        {
            return await PrepareConsumerAsync(new NmsConsumer(await session.CreateConsumerAsync(destination, selector, noLocal).Awaiter())).Awaiter();
        }

        public async Task<INMSConsumer> CreateDurableConsumerAsync(ITopic destination, string subscriptionName)
        {
            return await PrepareConsumerAsync(new NmsConsumer(await session.CreateDurableConsumerAsync(destination, subscriptionName).Awaiter())).Awaiter();
        }

        public async Task<INMSConsumer> CreateDurableConsumerAsync(ITopic destination, string subscriptionName, string selector)
        {
            return await PrepareConsumerAsync(new NmsConsumer(await session.CreateDurableConsumerAsync(destination, subscriptionName, selector).Awaiter())).Awaiter();
        }

        public async Task<INMSConsumer> CreateDurableConsumerAsync(ITopic destination, string subscriptionName, string selector, bool noLocal)
        {
            return await PrepareConsumerAsync(new NmsConsumer(await session.CreateDurableConsumerAsync(destination, subscriptionName, selector, noLocal).Awaiter())).Awaiter();
        }

        public async Task<INMSConsumer> CreateSharedConsumerAsync(ITopic destination, string subscriptionName)
        {
            return await PrepareConsumerAsync(new NmsConsumer(await session.CreateSharedConsumerAsync(destination, subscriptionName).Awaiter())).Awaiter();
        }

        public async Task<INMSConsumer> CreateSharedConsumerAsync(ITopic destination, string subscriptionName, string selector)
        {
            return await PrepareConsumerAsync(new NmsConsumer(await session.CreateSharedConsumerAsync(destination, subscriptionName, selector).Awaiter())).Awaiter();
        }

        public async Task<INMSConsumer> CreateSharedDurableConsumerAsync(ITopic destination, string subscriptionName)
        {
            return await PrepareConsumerAsync(new NmsConsumer(await session.CreateSharedDurableConsumerAsync(destination, subscriptionName).Awaiter())).Awaiter();
        }

        public async Task<INMSConsumer> CreateSharedDurableConsumerAsync(ITopic destination, string subscriptionName, string selector)
        {
            return await PrepareConsumerAsync(new NmsConsumer(await session.CreateSharedDurableConsumerAsync(destination, subscriptionName, selector).Awaiter())).Awaiter();
        }

        public void Unsubscribe(string name)
        {
            UnsubscribeAsync(name).GetAsyncResult();
        }

        public Task UnsubscribeAsync(string name)
        {
            return GetSession().UnsubscribeAsync(name);
        }

        public IQueueBrowser CreateBrowser(IQueue queue)
        {
            return CreateBrowserAsync(queue).GetAsyncResult();
        }

        public Task<IQueueBrowser> CreateBrowserAsync(IQueue queue)
        {
            return GetSession().CreateBrowserAsync(queue);
        }

        public IQueueBrowser CreateBrowser(IQueue queue, string selector)
        {
            return CreateBrowserAsync(queue, selector).GetAsyncResult();
        }

        public Task<IQueueBrowser> CreateBrowserAsync(IQueue queue, string selector)
        {
            return GetSession().CreateBrowserAsync(queue, selector);
        }

        public IQueue GetQueue(string name)
        {
            return GetQueueAsync(name).GetAsyncResult();
        }

        public Task<IQueue> GetQueueAsync(string name)
        {
            return GetSession().GetQueueAsync(name);
        }

        public ITopic GetTopic(string name)
        {
            return GetTopicAsync(name).GetAsyncResult();
        }

        public Task<ITopic> GetTopicAsync(string name)
        {
            return GetSession().GetTopicAsync(name);
        }

        public ITemporaryQueue CreateTemporaryQueue()
        {
            return CreateTemporaryQueueAsync().GetAsyncResult();
        }

        public Task<ITemporaryQueue> CreateTemporaryQueueAsync()
        {
            return GetSession().CreateTemporaryQueueAsync();
        }

        public ITemporaryTopic CreateTemporaryTopic()
        {
            return CreateTemporaryTopicAsync().GetAsyncResult();
        }

        public Task<ITemporaryTopic> CreateTemporaryTopicAsync()
        {
            return GetSession().CreateTemporaryTopicAsync();
        }

        public IMessage CreateMessage()
        {
            return CreateMessageAsync().GetAsyncResult();
        }

        public Task<IMessage> CreateMessageAsync()
        {
            return GetSession().CreateMessageAsync();
        }

        public ITextMessage CreateTextMessage()
        {
            return CreateTextMessageAsync().GetAsyncResult();
        }

        public Task<ITextMessage> CreateTextMessageAsync()
        {
            return GetSession().CreateTextMessageAsync();
        }

        public ITextMessage CreateTextMessage(string text)
        {
            return CreateTextMessageAsync(text).GetAsyncResult();
        }

        public Task<ITextMessage> CreateTextMessageAsync(string text)
        {
            return GetSession().CreateTextMessageAsync(text);
        }

        public IMapMessage CreateMapMessage()
        {
            return CreateMapMessageAsync().GetAsyncResult();
        }

        public Task<IMapMessage> CreateMapMessageAsync()
        {
            return GetSession().CreateMapMessageAsync();
        }

        public IObjectMessage CreateObjectMessage(object body)
        {
            return CreateObjectMessageAsync(body).GetAsyncResult();
        }

        public Task<IObjectMessage> CreateObjectMessageAsync(object body)
        {
            return GetSession().CreateObjectMessageAsync(body);
        }

        public IBytesMessage CreateBytesMessage()
        {
            return CreateBytesMessageAsync().GetAsyncResult();
        }

        public Task<IBytesMessage> CreateBytesMessageAsync()
        {
            return GetSession().CreateBytesMessageAsync();
        }

        public IBytesMessage CreateBytesMessage(byte[] body)
        {
            return CreateBytesMessageAsync(body).GetAsyncResult();
        }

        public Task<IBytesMessage> CreateBytesMessageAsync(byte[] body)
        {
            return GetSession().CreateBytesMessageAsync(body);
        }

        public IStreamMessage CreateStreamMessage()
        {
            return CreateStreamMessageAsync().GetAsyncResult();
        }

        public Task<IStreamMessage> CreateStreamMessageAsync()
        {
            return GetSession().CreateStreamMessageAsync();
        }

        public void Close()
        {
            session?.Close();
        }

        public async Task CloseAsync()
        {
            if (session != null)
            {
                await session.CloseAsync().Awaiter();
            }
        }

        public void Recover()
        {
            RecoverAsync().GetAsyncResult();
        }

        public Task RecoverAsync()
        {
            return GetSession().RecoverAsync();
        }

        public void Acknowledge()
        {
            AcknowledgeAsync().GetAsyncResult();
        }

        public Task AcknowledgeAsync()
        {
            return GetSession().AcknowledgeAsync();
        }

        public void Commit()
        {
            CommitAsync().GetAsyncResult();
        }

        public Task CommitAsync()
        {
            return GetSession().CommitAsync();
        }

        public void Rollback()
        {
            RollbackAsync().GetAsyncResult();
        }

        public Task RollbackAsync()
        {
            return GetSession().RollbackAsync();
        }

        public void PurgeTempDestinations()
        {
            connection.PurgeTempDestinations();
        }

        public ConsumerTransformerDelegate ConsumerTransformer
        {
            get => GetSession().ConsumerTransformer;
            set => GetSession().ConsumerTransformer = value;
        }

        public ProducerTransformerDelegate ProducerTransformer
        {
            get => GetSession().ProducerTransformer;
            set => GetSession().ProducerTransformer = value;
        }

        public TimeSpan RequestTimeout
        {
            get => GetSession().RequestTimeout;
            set => GetSession().RequestTimeout = value;
        }

        public bool Transacted => GetSession().Transacted;

        public AcknowledgementMode AcknowledgementMode => acknowledgementMode;

        public string ClientId
        {
            get => connection.ClientId;
            set => connection.ClientId = value;
        }

        public bool AutoStart
        {
            get => autoStart;
            set => autoStart = value;
        }

        public event SessionTxEventDelegate TransactionStartedListener
        {
            add => GetSession().TransactionStartedListener += value;
            remove => GetSession().TransactionStartedListener -= value;
        }

        public event SessionTxEventDelegate TransactionCommittedListener
        {
            add => GetSession().TransactionCommittedListener += value;
            remove => GetSession().TransactionCommittedListener -= value;
        }

        public event SessionTxEventDelegate TransactionRolledBackListener
        {
            add => GetSession().TransactionRolledBackListener += value;
            remove => GetSession().TransactionRolledBackListener -= value;
        }

        public event ExceptionListener ExceptionListener
        {
            add => connection.ExceptionListener += value;
            remove => connection.ExceptionListener -= value;
        }

        public event ConnectionInterruptedListener ConnectionInterruptedListener
        {
            add => connection.ConnectionInterruptedListener += value;
            remove => connection.ConnectionInterruptedListener -= value;
        }

        public event ConnectionResumedListener ConnectionResumedListener
        {
            add => connection.ConnectionResumedListener += value;
            remove => connection.ConnectionResumedListener -= value;
        }

        private INMSConsumer PrepareConsumer(INMSConsumer consumer)
        {
            return PrepareConsumerAsync(consumer).GetAsyncResult();
        }
        
        private async Task<INMSConsumer> PrepareConsumerAsync(INMSConsumer consumer)
        {
            if (autoStart) {
                await connection.StartAsync().Awaiter();
            }
            return consumer;
        }

        private ISession GetSession()
        {
            return GetSessionAsync().GetAsyncResult();
        }
        
        private async Task<ISession> GetSessionAsync()
        {
            if (session == null)
            {
                using (await lockRoot.LockAsync().Awaiter())
                {
                    if (session == null)
                    {
                        session = await connection.CreateSessionAsync(acknowledgementMode).Awaiter();
                    }
                }
            }

            return session;
        }
    }
}