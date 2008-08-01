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

namespace Spring.Messaging.Nms.Core.Connections
{

    public class TestMessageProducer : IMessageProducer
    {
        public void Send(IMessage message)
        {
            throw new NotImplementedException();
        }

        public void Send(IMessage message, bool persistent, byte priority, TimeSpan timeToLive)
        {
            throw new NotImplementedException();
        }

        public void Send(IDestination destination, IMessage message)
        {
            throw new NotImplementedException();
        }

        public void Send(IDestination destination, IMessage message, bool persistent, byte priority, TimeSpan timeToLive)
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

        public bool Persistent
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public TimeSpan TimeToLive
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public byte Priority
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}