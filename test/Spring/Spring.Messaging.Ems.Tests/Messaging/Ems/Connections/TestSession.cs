#region License

/*
 * Copyright 2002-2008 the original author or authors.
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
using Rhino.Mocks;
using Spring.Messaging.Ems.Common;
using TIBCO.EMS;

namespace Spring.Messaging.Ems.Connections
{
    public class TestSession : ISession
    {
        private MockRepository mocks = new MockRepository();
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

        public IMessageProducer CreateProducer(Destination dest)
        {
            return new TestMessageProducer();
        }


        public IMessageConsumer CreateConsumer(Destination destination)
        {
            return new TestMessageConsumer();
        }

        public IMessageConsumer CreateConsumer(Destination destination, string selector)
        {
            return new TestMessageConsumer();
        }

        public IMessageConsumer CreateConsumer(Destination dest, string messageSelector, bool noLocal)
        {
            return new TestMessageConsumer();
        }

        public ITopicSubscriber CreateDurableSubscriber(Topic topic, string name)
        {
            return new TestTopicSubscriber();
        }

        public ITopicSubscriber CreateDurableSubscriber(Topic topic, string name, string messageSelector, bool noLocal)
        {
            return new TestTopicSubscriber();
        }

        public Queue CreateQueue(string queueName)
        {
            return new Queue(queueName);
        }

        public Topic CreateTopic(string topicName)
        {
            throw new NotImplementedException();
        }

        TemporaryQueue ISession.CreateTemporaryQueue()
        {
            throw new NotImplementedException();
        }

        TemporaryTopic ISession.CreateTemporaryTopic()
        {
            throw new NotImplementedException();
        }



        public Message CreateMessage()
        {
            throw new NotImplementedException();
        }

        public TextMessage CreateTextMessage()
        {
            throw new NotImplementedException();
        }

        public TextMessage CreateTextMessage(string text)
        {
            throw new NotImplementedException();
        }

        public MapMessage CreateMapMessage()
        {
            throw new NotImplementedException();
        }

        public ObjectMessage CreateObjectMessage(object body)
        {
            throw new NotImplementedException();
        }

        public BytesMessage CreateBytesMessage()
        {
            throw new NotImplementedException();
        }

        public ObjectMessage CreateObjectMessage()
        {
            throw new NotImplementedException();
        }

        public StreamMessage CreateStreamMessage()
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

        public bool Transacted
        {
            get { return true; }
        }

        public int AcknowledgeMode
        {
            get { throw new NotImplementedException(); }
        }

        public Session NativeSession
        {
            get { throw new NotImplementedException(); }
        }

        public QueueBrowser CreateBrowser(Queue queue)
        {
            throw new NotImplementedException();
        }

        public QueueBrowser CreateBrowser(Queue queue, string messageSelector)
        {
            throw new NotImplementedException();
        }

        public void Recover()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(string name)
        {
            throw new NotImplementedException();
        }

        public Connection Connection
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsTransacted
        {
            get { throw new NotImplementedException(); }
        }

        public IMessageListener MessageListener
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public long SessID
        {
            get { throw new NotImplementedException(); }
        }

        public SessionMode SessionAcknowledgeMode
        {
            get { throw new NotImplementedException(); }
        }
    }
}