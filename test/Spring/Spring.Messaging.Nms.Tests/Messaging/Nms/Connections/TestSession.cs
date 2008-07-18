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
using Apache.NMS;

namespace Spring.Messaging.Nms.Connection
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

        public IMessageConsumer CreateConsumer(IDestination destination)
        {
            throw new NotImplementedException();
        }

        public IMessageConsumer CreateConsumer(IDestination destination, string selector)
        {
            throw new NotImplementedException();
        }

        public IMessageConsumer CreateConsumer(IDestination destination, string selector, bool noLocal)
        {
            throw new NotImplementedException();
        }

        public IMessageConsumer CreateDurableConsumer(ITopic destination, string name, string selector, bool noLocal)
        {
            throw new NotImplementedException();
        }

        public IQueue GetQueue(string name)
        {
            throw new NotImplementedException();
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

        public void Close()
        {
            closeCount++;
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }

        public bool Transacted
        {
            get { throw new NotImplementedException(); }
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