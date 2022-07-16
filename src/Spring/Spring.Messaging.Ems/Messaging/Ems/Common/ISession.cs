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

using System.ComponentModel;

namespace Spring.Messaging.Ems.Common
{
    public interface ISession
    {
        Session NativeSession { get; }

        void Close();

        void Commit();

        QueueBrowser CreateBrowser(Queue queue);

        QueueBrowser CreateBrowser(Queue queue, string messageSelector);

        IMessageConsumer CreateConsumer(Destination dest);

        IMessageConsumer CreateConsumer(Destination dest, string messageSelector);

        IMessageConsumer CreateConsumer(Destination dest, string messageSelector, bool noLocal);

        ITopicSubscriber CreateDurableSubscriber(Topic topic, string name);

        ITopicSubscriber CreateDurableSubscriber(Topic topic, string name, string messageSelector, bool noLocal);

        IMessageProducer CreateProducer(Destination dest);

        Queue CreateQueue(string queueName);

        Topic CreateTopic(string topicName);

        TemporaryQueue CreateTemporaryQueue();

        TemporaryTopic CreateTemporaryTopic();

        Message CreateMessage();

        TextMessage CreateTextMessage();

        TextMessage CreateTextMessage(string text);

        MapMessage CreateMapMessage();

        BytesMessage CreateBytesMessage();

        ObjectMessage CreateObjectMessage();

        ObjectMessage CreateObjectMessage(object obj);

        StreamMessage CreateStreamMessage();

        void Recover();

        void Rollback();

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Ordinary JMS clients should not use this method.")]
        void Run();

        void Unsubscribe(string name);

        int AcknowledgeMode { get; }
        TIBCO.EMS.Connection Connection { get; }
        bool IsClosed { get; }
        bool IsTransacted { get; }

        long SessID { get; }
        SessionMode SessionAcknowledgeMode { get; }
        bool Transacted { get; }
    }
}
