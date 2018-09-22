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

using System.Text;

using Apache.NMS;

using FakeItEasy;

using NUnit.Framework;

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
        private static string TEXT = "I fancy a good cuppa right now";

        [Test]
        public void MessageContentsHandlerForTextMessage()
        {
            int numIterations = 10;
            ITextMessage message = A.Fake<ITextMessage>();
            A.CallTo(() => message.Text).Returns(TEXT).NumberOfTimes(numIterations);
            MessageContentsHandler handler = new MessageContentsHandler();

            MessageListenerAdapter adapter = new MessageListenerAdapter(handler);
            for (int i = 0; i < numIterations; i++)
            {
                adapter.OnMessage(message);
            }

            Assert.AreEqual(numIterations, handler.HandledStringCount);
        }

        [Test]
        public void MessageContentsHandlerForBytesMessage()
        {
            int numIterations = 10;
            IBytesMessage message = A.Fake<IBytesMessage>();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] content = encoding.GetBytes("test");
            A.CallTo(() => message.Content).Returns(content).NumberOfTimes(numIterations);
            MessageContentsHandler handler = new MessageContentsHandler();

            MessageListenerAdapter adapter = new MessageListenerAdapter(handler);
            for (int i = 0; i < numIterations; i++)
            {
                adapter.OnMessage(message);
            }

            Assert.AreEqual(numIterations, handler.HandledByteArrayCount);
        }

        [Test]
        public void MessageContentsHandlerOverloadCalls()
        {
            int numIterations = 10;
            IBytesMessage bytesMessage = A.Fake<IBytesMessage>();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] content = encoding.GetBytes("test");
            A.CallTo(() => bytesMessage.Content).Returns(content).NumberOfTimes(numIterations / 2);

            ITextMessage textMessage = A.Fake<ITextMessage>();
            A.CallTo(() => textMessage.Text).Returns(TEXT).NumberOfTimes(numIterations / 2);

            MessageContentsHandler handler = new MessageContentsHandler();

            MessageListenerAdapter adapter = new MessageListenerAdapter(handler);
            for (int i = 0; i < numIterations / 2; i++)
            {
                adapter.OnMessage(textMessage);
                adapter.OnMessage(bytesMessage);
            }

            Assert.AreEqual(numIterations / 2, handler.HandledByteArrayCount);
            Assert.AreEqual(numIterations / 2, handler.HandledStringCount);
        }
    }
}