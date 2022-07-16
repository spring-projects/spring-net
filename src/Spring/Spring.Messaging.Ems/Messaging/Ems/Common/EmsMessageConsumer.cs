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
    public class EmsMessageConsumer : IMessageConsumer
    {
        protected readonly MessageConsumer nativeMessageConsumer;

        public EmsMessageConsumer(MessageConsumer messageConsumer)
        {
            nativeMessageConsumer = messageConsumer;
            nativeMessageConsumer.MessageHandler += MessageHandler;
        }

        #region Implementation of IMessageConsumer

        public MessageConsumer NativeMessageConsumer
        {
            get { return this.nativeMessageConsumer; }
        }

        public event EMSMessageHandler MessageHandler;

        public IMessageListener MessageListener
        {
            get { return nativeMessageConsumer.MessageListener; }
            set { nativeMessageConsumer.MessageListener = value; }
        }

        public string MessageSelector
        {
            get { return nativeMessageConsumer.MessageSelector; }
        }

        public void Close()
        {
            nativeMessageConsumer.Close();
        }

        public Message Receive()
        {
            return nativeMessageConsumer.Receive();
        }

        public Message Receive(long timeout)
        {
            return nativeMessageConsumer.Receive(timeout);
        }

        public Message ReceiveNoWait()
        {
            return nativeMessageConsumer.ReceiveNoWait();
        }

        #endregion
    }
}
