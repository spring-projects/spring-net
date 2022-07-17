#region License
// /*
//  * Copyright 2022 the original author or authors.
//  *
//  * Licensed under the Apache License, Version 2.0 (the "License");
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  *      http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */
#endregion

using Apache.NMS;
using Spring.Messaging.Nms.Support;

namespace Spring.Messaging.Nms.Connections
{
    public class NmsConsumer : INMSConsumer
    {
        private readonly IMessageConsumer consumer;

        public NmsConsumer(IMessageConsumer consumer)
        {
            this.consumer = consumer;
        }
        
       public void Dispose()
        {
            consumer.Dispose();
        }

        public IMessage Receive()
        {
            return consumer.Receive();
        }

        public Task<IMessage> ReceiveAsync()
        {
            return consumer.ReceiveAsync();
        }

        public IMessage Receive(TimeSpan timeout)
        {
            return consumer.Receive(timeout);
        }

        public Task<IMessage> ReceiveAsync(TimeSpan timeout)
        {
            return consumer.ReceiveAsync(timeout);
        }

        public IMessage ReceiveNoWait()
        {
            return consumer.ReceiveNoWait();
        }

        public T ReceiveBody<T>()
        {
            return ReceiveBodyInternal<T>(() => Task.FromResult(consumer.Receive())).GetAsyncResult();
        }

        public Task<T> ReceiveBodyAsync<T>()
        {
            return ReceiveBodyInternal<T>(() => consumer.ReceiveAsync());
        }

        public T ReceiveBody<T>(TimeSpan timeout)
        {
            return ReceiveBodyInternal<T>(() => Task.FromResult(consumer.Receive(timeout))).GetAsyncResult();
        }

        public Task<T> ReceiveBodyAsync<T>(TimeSpan timeout)
        {
            return ReceiveBodyInternal<T>(() => consumer.ReceiveAsync(timeout));
        }

        public T ReceiveBodyNoWait<T>()
        {
            return ReceiveBodyInternal<T>( () => Task.FromResult( consumer.ReceiveNoWait())).GetAsyncResult();
        }

        private async Task<T> ReceiveBodyInternal<T>(Func<Task<IMessage>> receiveMessageFunc)
        {
            var message = await receiveMessageFunc().Awaiter();
            if (message != null)
            {
                return message.Body<T>();
            }
            else
            {
                return default(T);
            }
        }
        
        public void Close()
        {
            consumer.Close();
        }

        public Task CloseAsync()
        {
            return consumer.CloseAsync();
        }

        public string MessageSelector => consumer.MessageSelector;

        public ConsumerTransformerDelegate ConsumerTransformer
        {
            get => consumer.ConsumerTransformer; 
            set => consumer.ConsumerTransformer = value; 
        }

        public event MessageListener Listener
        {
            add => consumer.Listener += value;
            remove => consumer.Listener -= value;
        }
    }
}