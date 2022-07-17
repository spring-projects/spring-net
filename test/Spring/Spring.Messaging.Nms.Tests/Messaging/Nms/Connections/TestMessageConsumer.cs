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

namespace Spring.Messaging.Nms.Connections
{
    public class TestMessageConsumer : IMessageConsumer
    {
        public string MessageSelector { get; }
        public event MessageListener Listener;

        private void InvokeListener(IMessage message)
        {
            MessageListener listener = Listener;
            if (listener != null) listener(message);
        }

        public IMessage Receive()
        {
            throw new NotImplementedException();
        }

        public Task<IMessage> ReceiveAsync()
        {
            throw new NotImplementedException();
        }

        public IMessage Receive(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public Task<IMessage> ReceiveAsync(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public IMessage ReceiveNoWait()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public Task CloseAsync()
        {
            throw new NotImplementedException();
        }

        public ConsumerTransformerDelegate ConsumerTransformer
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