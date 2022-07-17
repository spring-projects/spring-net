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
using FakeItEasy;

namespace Spring.Messaging.Nms.Connections
{
    public class TestSession : ISession
    {
        private int closeCount;
        private int createdCount;


        public TestSession()
        {
            createdCount++;
        }

        public int CloseCount
        {
            get { return closeCount; }
        }


        public int CreatedCount
        {
            get { return createdCount; }
        }

        public IMessageProducer CreateProducer()
        {
            return new TestMessageProducer();
        }

        public Task<IMessageProducer> CreateProducerAsync()
        {
            return Task.FromResult(CreateProducer());
        }

        public IMessageProducer CreateProducer(IDestination destination)
        {
            return new TestMessageProducer();
        }

        public Task<IMessageProducer> CreateProducerAsync(IDestination destination)
        {
            return Task.FromResult(CreateProducer());
        }

        public IMessageProducer CreateProducer(IDestination destination, TimeSpan requestTimeout)
        {
            return new TestMessageProducer();
        }

        public IMessageConsumer CreateConsumer(IDestination destination)
        {
            return new TestMessageConsumer();
        }

        public Task<IMessageConsumer> CreateConsumerAsync(IDestination destination)
        {
            return Task.FromResult(CreateConsumer(destination));
        }

        public IMessageConsumer CreateConsumer(IDestination destination, TimeSpan requestTimeout)
        {
            return new TestMessageConsumer();
        }

        public IMessageConsumer CreateConsumer(IDestination destination, string selector)
        {
            return new TestMessageConsumer();
        }

        public Task<IMessageConsumer> CreateConsumerAsync(IDestination destination, string selector)
        {
            return Task.FromResult(CreateConsumer(destination, selector));
        }

        public IMessageConsumer CreateConsumer(IDestination destination, string selector, TimeSpan requestTimeout)
        {
            return new TestMessageConsumer();
        }

        public IMessageConsumer CreateConsumer(IDestination destination, string selector, bool noLocal)
        {
            return new TestMessageConsumer();
        }

        public Task<IMessageConsumer> CreateConsumerAsync(IDestination destination, string selector, bool noLocal)
        {
            return Task.FromResult(CreateConsumer(destination, selector, noLocal));
        }

        public IMessageConsumer CreateDurableConsumer(ITopic destination, string name)
        {
            return new TestMessageConsumer();
        }

        public Task<IMessageConsumer> CreateDurableConsumerAsync(ITopic destination, string name)
        {
            return Task.FromResult(CreateConsumer(destination));
        }

        public IMessageConsumer CreateDurableConsumer(ITopic destination, string name, string selector)
        {
            throw new NotImplementedException();
        }

        public Task<IMessageConsumer> CreateDurableConsumerAsync(ITopic destination, string name, string selector)
        {
            throw new NotImplementedException();
        }

        public IMessageConsumer CreateConsumer(
            IDestination destination, string selector, bool noLocal,
            TimeSpan requestTimeout)
        {
            return new TestMessageConsumer();
        }

        public IMessageConsumer CreateDurableConsumer(ITopic destination, string name, string selector, bool noLocal)
        {
            return new TestMessageConsumer();
        }

        public Task<IMessageConsumer> CreateDurableConsumerAsync(ITopic destination, string name, string selector, bool noLocal)
        {
            throw new NotImplementedException();
        }

        public IMessageConsumer CreateSharedConsumer(ITopic destination, string name)
        {
            throw new NotImplementedException();
        }

        public Task<IMessageConsumer> CreateSharedConsumerAsync(ITopic destination, string name)
        {
            throw new NotImplementedException();
        }

        public IMessageConsumer CreateSharedConsumer(ITopic destination, string name, string selector)
        {
            throw new NotImplementedException();
        }

        public Task<IMessageConsumer> CreateSharedConsumerAsync(ITopic destination, string name, string selector)
        {
            throw new NotImplementedException();
        }

        public IMessageConsumer CreateSharedDurableConsumer(ITopic destination, string name)
        {
            throw new NotImplementedException();
        }

        public Task<IMessageConsumer> CreateSharedDurableConsumerAsync(ITopic destination, string name)
        {
            throw new NotImplementedException();
        }

        public IMessageConsumer CreateSharedDurableConsumer(ITopic destination, string name, string selector)
        {
            throw new NotImplementedException();
        }

        public Task<IMessageConsumer> CreateSharedDurableConsumerAsync(ITopic destination, string name, string selector)
        {
            throw new NotImplementedException();
        }

        public IMessageConsumer CreateDurableConsumer(
            ITopic destination, string name, string selector, bool noLocal,
            TimeSpan requestTimeout)
        {
            return new TestMessageConsumer();
        }

        public void DeleteDurableConsumer(string name)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(string name)
        {
            throw new NotImplementedException();
        }

        public Task UnsubscribeAsync(string name)
        {
            throw new NotImplementedException();
        }

        public IQueueBrowser CreateBrowser(IQueue queue)
        {
            throw new NotImplementedException();
        }

        public Task<IQueueBrowser> CreateBrowserAsync(IQueue queue)
        {
            throw new NotImplementedException();
        }

        public IQueueBrowser CreateBrowser(IQueue queue, string selector)
        {
            throw new NotImplementedException();
        }

        public Task<IQueueBrowser> CreateBrowserAsync(IQueue queue, string selector)
        {
            throw new NotImplementedException();
        }

        public void DeleteDurableConsumer(string name, TimeSpan requestTimeout)
        {
            throw new NotImplementedException();
        }

        public Task<IQueue> GetQueueAsync(string name)
        {
            throw new NotImplementedException();
        }
        
        public IQueue GetQueue(string name)
        {
            return A.Fake<IQueue>();
        }

        public ITopic GetTopic(string name)
        {
            throw new NotImplementedException();
        }

        public Task<ITopic> GetTopicAsync(string name)
        {
            throw new NotImplementedException();
        }

        public ITemporaryQueue CreateTemporaryQueue()
        {
            throw new NotImplementedException();
        }

        public Task<ITemporaryQueue> CreateTemporaryQueueAsync()
        {
            throw new NotImplementedException();
        }

        public ITemporaryTopic CreateTemporaryTopic()
        {
            throw new NotImplementedException();
        }

        public Task<ITemporaryTopic> CreateTemporaryTopicAsync()
        {
            throw new NotImplementedException();
        }

        public void DeleteDestination(IDestination destination)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDestinationAsync(IDestination destination)
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

        public void Close()
        {
            closeCount++;
        }

        public Task CloseAsync()
        {
            throw new NotImplementedException();
        }

        public void Recover()
        {
        }

        public Task RecoverAsync()
        {
            throw new NotImplementedException();
        }

        public void Acknowledge()
        {
            throw new NotImplementedException();
        }

        public Task AcknowledgeAsync()
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
        }

        public Task CommitAsync()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
        }

        public Task RollbackAsync()
        {
            return Task.CompletedTask;
        }

        public ConsumerTransformerDelegate ConsumerTransformer
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ProducerTransformerDelegate ProducerTransformer
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public TimeSpan RequestTimeout
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool Transacted
        {
            get { return true; }
        }

        public AcknowledgementMode AcknowledgementMode
        {
            get { throw new NotImplementedException(); }
        }

        #region Transaction State Events

        public void TransactionStarted()
        {
            if (TransactionStartedListener != null)
            {
                TransactionStartedListener(this);
            }
        }

        public void TransactionCommitted()
        {
            if (TransactionCommittedListener != null)
            {
                TransactionCommittedListener(this);
            }
        }

        public void TransactionRolledBack()
        {
            if (TransactionRolledBackListener != null)
            {
                TransactionRolledBackListener(this);
            }
        }

        public event SessionTxEventDelegate TransactionStartedListener;

        public event SessionTxEventDelegate TransactionCommittedListener;

        public event SessionTxEventDelegate TransactionRolledBackListener;

        #endregion

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}