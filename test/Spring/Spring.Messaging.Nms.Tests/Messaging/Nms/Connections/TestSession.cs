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
using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;

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

        public IMessageProducer CreateProducer(IDestination destination)
        {
            return new TestMessageProducer();
        }

        public IMessageProducer CreateProducer(IDestination destination, TimeSpan requestTimeout)
        {
            throw new NotImplementedException();
        }

        public IMessageConsumer CreateConsumer(IDestination destination)
        {
            return new TestMessageConsumer();
        }

        public IMessageConsumer CreateConsumer(IDestination destination, TimeSpan requestTimeout)
        {
            return new TestMessageConsumer();
        }

        public IMessageConsumer CreateConsumer(IDestination destination, string selector)
        {
            return new TestMessageConsumer();
            //IConnectionFactory connectionFactory = (IConnectionFactory)mocks.CreateMock(typeof(IConnectionFactory));
            //IMessageConsumer msgConsumer = (IMessageConsumer) mocks.CreateMock(typeof (IMessageConsumer));
            //return msgConsumer;
        }

        public IMessageConsumer CreateConsumer(IDestination destination, string selector, TimeSpan requestTimeout)
        {
            return new TestMessageConsumer();
        }

        public IMessageConsumer CreateConsumer(IDestination destination, string selector, bool noLocal)
        {
            return new TestMessageConsumer();
        }

        public IMessageConsumer CreateConsumer(IDestination destination, string selector, bool noLocal,
                                               TimeSpan requestTimeout)
        {
            return new TestMessageConsumer();
        }

        public IMessageConsumer CreateDurableConsumer(ITopic destination, string name, string selector, bool noLocal)
        {
            return new TestMessageConsumer();
        }

        public IMessageConsumer CreateDurableConsumer(ITopic destination, string name, string selector, bool noLocal,
                                                      TimeSpan requestTimeout)
        {
            return new TestMessageConsumer();
        }

        public void DeleteDurableConsumer(string name)
        {
            throw new NotImplementedException();
        }

        public IQueueBrowser CreateBrowser(IQueue queue)
        {
            throw new NotImplementedException();
        }

        public IQueueBrowser CreateBrowser(IQueue queue, string selector)
        {
            throw new NotImplementedException();
        }

        public void DeleteDurableConsumer(string name, TimeSpan requestTimeout)
        {
            throw new NotImplementedException();
        }

        public IQueue GetQueue(string name)
        {
            return new ActiveMQQueue(name);
        }

        public ITopic GetTopic(string name)
        {
            throw new NotImplementedException();
        }

        public ITemporaryQueue CreateTemporaryQueue()
        {
            throw new NotImplementedException();
        }

        public ITemporaryTopic CreateTemporaryTopic()
        {
            throw new NotImplementedException();
        }

        #region ISession Members

        public void DeleteDestination(IDestination destination)
        {
            throw new NotImplementedException();
        }

        #endregion

        public IMessage CreateMessage()
        {
            throw new NotImplementedException();
        }

        public ITextMessage CreateTextMessage()
        {
            throw new NotImplementedException();
        }

        public ITextMessage CreateTextMessage(string text)
        {
            throw new NotImplementedException();
        }

        public IMapMessage CreateMapMessage()
        {
            throw new NotImplementedException();
        }

        public IObjectMessage CreateObjectMessage(object body)
        {
            throw new NotImplementedException();
        }

        public IBytesMessage CreateBytesMessage()
        {
            throw new NotImplementedException();
        }

        public IBytesMessage CreateBytesMessage(byte[] body)
        {
            throw new NotImplementedException();
        }

        public IStreamMessage CreateStreamMessage()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            closeCount++;
        }

        public void Commit()
        {

        }

        public void Rollback()
        {

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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}