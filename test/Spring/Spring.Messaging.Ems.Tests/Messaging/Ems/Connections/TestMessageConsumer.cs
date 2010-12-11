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
using Spring.Messaging.Ems.Common;
using TIBCO.EMS;

namespace Spring.Messaging.Ems.Connections
{
    public class TestMessageConsumer : IMessageConsumer
    {

        private IMessageListener messageListener;

        public Message Receive()
        {
            throw new NotImplementedException();
        }

        public Message Receive(long timeout)
        {
            throw new NotImplementedException();
        }


        public Message ReceiveNoWait()
        {
            throw new NotImplementedException();
        }

        public MessageConsumer NativeMessageConsumer
        {
            get { throw new NotImplementedException(); }
        }

        public event EMSMessageHandler MessageHandler;

        public IMessageListener MessageListener
        {
            get { return messageListener; }
            set { messageListener = value; }
        }

        public string MessageSelector
        {
            get { throw new NotImplementedException(); }
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

    }
}