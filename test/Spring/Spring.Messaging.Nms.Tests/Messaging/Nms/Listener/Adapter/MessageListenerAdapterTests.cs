#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

#region Imports

using System;
using System.Text;
using Apache.NMS;
using NUnit.Framework;
using Rhino.Mocks;

#endregion

namespace Spring.Messaging.Nms.Listener.Adapter
{
    /// <summary>
    /// This class contains tests for MessageListenerAdapter
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id:$</version>
    [TestFixture]
    public class MessageListenerAdapterTests
    {
        private MockRepository mocks;

        private static string TEXT = "I fancy a good cuppa right now";

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [Test]
        public void MessageContentsHandlerForTextMessage()
        {
            int numIterations = 10;
            ITextMessage message = mocks.StrictMock<ITextMessage>();
            Expect.Call(message.Text).Return(TEXT).Repeat.Times(numIterations);
            MessageContentsHandler handler = new MessageContentsHandler();
            mocks.ReplayAll();

            MessageListenerAdapter adapter = new MessageListenerAdapter(handler);
            for (int i = 0; i < numIterations; i++)
            {
                adapter.OnMessage(message);
            }
            Assert.AreEqual(numIterations, handler.HandledStringCount);

            mocks.VerifyAll();            
        }

        [Test]
        public void MessageContentsHandlerForBytesMessage()
        {
            int numIterations = 10;
            IBytesMessage message = mocks.StrictMock<IBytesMessage>();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] content = encoding.GetBytes("test");
            Expect.Call(message.Content).Return(content).Repeat.Times(numIterations);
            MessageContentsHandler handler = new MessageContentsHandler();            
            mocks.ReplayAll();

            MessageListenerAdapter adapter = new MessageListenerAdapter(handler);
            for (int i = 0; i < numIterations; i++)
            {
                adapter.OnMessage(message);
            }
            Assert.AreEqual(numIterations, handler.HandledByteArrayCount);

            mocks.VerifyAll();
        }

        [Test]
        public void MessageContentsHandlerOverloadCalls()
        {
            int numIterations = 10;
            IBytesMessage bytesMessage = mocks.StrictMock<IBytesMessage>();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] content = encoding.GetBytes("test");
            Expect.Call(bytesMessage.Content).Return(content).Repeat.Times(numIterations / 2);

            ITextMessage textMessage = mocks.StrictMock<ITextMessage>();
            Expect.Call(textMessage.Text).Return(TEXT).Repeat.Times(numIterations/2);

            MessageContentsHandler handler = new MessageContentsHandler();
            mocks.ReplayAll();

            MessageListenerAdapter adapter = new MessageListenerAdapter(handler);
            for (int i = 0; i < numIterations/2; i++)
            {
                adapter.OnMessage(textMessage);
                adapter.OnMessage(bytesMessage);
            }
            Assert.AreEqual(numIterations / 2, handler.HandledByteArrayCount);
            Assert.AreEqual(numIterations / 2, handler.HandledStringCount);

            mocks.VerifyAll();
        }
    }

    [Serializable]
    public class SerializableObject
    {
        
    }
}