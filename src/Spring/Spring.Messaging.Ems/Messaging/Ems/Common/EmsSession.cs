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
    public class EmsSession : ISession
    {
        private Session nativeSession;

        public EmsSession(Session session)
        {
            this.nativeSession = session;
        }

        #region Implementation of ISession

        public Session NativeSession
        {
            get { return this.nativeSession; }
        }

        public void Close()
        {
            nativeSession.Close();
        }

        public void Commit()
        {
            nativeSession.Commit();
        }

        public QueueBrowser CreateBrowser(Queue queue)
        {
            return nativeSession.CreateBrowser(queue);
        }

        public QueueBrowser CreateBrowser(Queue queue, string messageSelector)
        {
            return nativeSession.CreateBrowser(queue, messageSelector);
        }

        public BytesMessage CreateBytesMessage()
        {
            return nativeSession.CreateBytesMessage();
        }

        public IMessageConsumer CreateConsumer(Destination dest)
        {
            return new EmsMessageConsumer(nativeSession.CreateConsumer(dest));

        }

        public IMessageConsumer CreateConsumer(Destination dest, string messageSelector)
        {
            return new EmsMessageConsumer(nativeSession.CreateConsumer(dest, messageSelector));
        }

        public IMessageConsumer CreateConsumer(Destination dest, string messageSelector, bool noLocal)
        {
            return new EmsMessageConsumer(nativeSession.CreateConsumer(dest, messageSelector, noLocal));
        }

        public ITopicSubscriber CreateDurableSubscriber(Topic topic, string name)
        {
            return new EmsTopicSubscriber(nativeSession.CreateDurableSubscriber(topic, name));
        }

        public ITopicSubscriber CreateDurableSubscriber(Topic topic, string name, string messageSelector, bool noLocal)
        {
            return new EmsTopicSubscriber(nativeSession.CreateDurableSubscriber(topic, name, messageSelector, noLocal));
        }

        public MapMessage CreateMapMessage()
        {
            return nativeSession.CreateMapMessage();
        }

        public Message CreateMessage()
        {
            return nativeSession.CreateMessage();
        }

        public ObjectMessage CreateObjectMessage()
        {
            return nativeSession.CreateObjectMessage();
        }

        public ObjectMessage CreateObjectMessage(object obj)
        {
            return nativeSession.CreateObjectMessage(obj);
        }

        public IMessageProducer CreateProducer(Destination dest)
        {
            return new EmsMessageProducer(nativeSession.CreateProducer(dest));
        }

        public Queue CreateQueue(string queueName)
        {
            return nativeSession.CreateQueue(queueName);
        }

        public StreamMessage CreateStreamMessage()
        {
            return nativeSession.CreateStreamMessage();
        }

        public TemporaryQueue CreateTemporaryQueue()
        {
            return nativeSession.CreateTemporaryQueue();
        }

        public TemporaryTopic CreateTemporaryTopic()
        {
            return nativeSession.CreateTemporaryTopic();
        }

        public TextMessage CreateTextMessage()
        {
            return nativeSession.CreateTextMessage();
        }

        public TextMessage CreateTextMessage(string text)
        {
            return nativeSession.CreateTextMessage(text);
        }

        public Topic CreateTopic(string topicName)
        {
            return nativeSession.CreateTopic(topicName);
        }

        public void Recover()
        {
            nativeSession.Recover();
        }

        public void Rollback()
        {
            nativeSession.Rollback();
        }

        public void Run()
        {
            nativeSession.Run();
        }

        public void Unsubscribe(string name)
        {
            nativeSession.Unsubscribe(name);
        }

        public int AcknowledgeMode
        {
            get { return nativeSession.AcknowledgeMode; }
        }

        // TODO
        public Connection Connection
        {
            get { return nativeSession.Connection; }
        }

        public bool IsClosed
        {
            get { return nativeSession.IsClosed; }
        }

        public bool IsTransacted
        {
            get { return nativeSession.IsTransacted; }
        }

        public long SessID
        {
            get { return nativeSession.SessID; }
        }

        public SessionMode SessionAcknowledgeMode
        {
            get { return nativeSession.SessionAcknowledgeMode; }
        }

        public bool Transacted
        {
            get { return nativeSession.Transacted; }
        }

        #endregion
    }
}
