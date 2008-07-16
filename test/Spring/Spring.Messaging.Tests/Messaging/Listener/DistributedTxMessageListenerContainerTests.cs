#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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

using System.Threading;
using NUnit.Framework;
using Spring.Messaging.Core;
using Spring.Testing.NUnit;

#endregion

namespace Spring.Messaging.Listener
{
    /// <summary>
    /// This class contains tests for DistributedTxMessageListenerContainer
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class DistributedTxMessageListenerContainerTests : AbstractDependencyInjectionSpringContextTests
    {
        private int waitInMillis = 20000;
        private DistributedTxMessageListenerContainer distributedTxMessageListenerContainer;
        private SimpleHandler listener;


        public DistributedTxMessageListenerContainer DistributedTxMessageListenerContainer
        {
            set { distributedTxMessageListenerContainer = value; }
        }


        public SimpleHandler Listener
        {
            set { listener = value; }
        }

        [Test]
        public void SendAndAsyncReceiveWithExceptionHandling()
        {
            MessageQueueTemplate q = applicationContext["queueTemplate"] as MessageQueueTemplate;
            Assert.IsNotNull(q);

            MessageQueueTemplate retryQ = applicationContext["retryQueueTemplate"] as MessageQueueTemplate;
            Assert.IsNotNull(retryQ);

            q.ConvertAndSend("Goodbye World 1");

            Assert.AreEqual(0, listener.MessageCount);
            distributedTxMessageListenerContainer.Start();

            Thread.Sleep(waitInMillis);


            distributedTxMessageListenerContainer.Stop();
            distributedTxMessageListenerContainer.Shutdown();
            Thread.Sleep(2500);

            object msg = retryQ.ReceiveAndConvert();
            Assert.IsNotNull(msg);
            string textMsg = msg as string;
            Assert.IsNotNull(textMsg);
            Assert.AreEqual("Goodbye World 1", textMsg);
        }


        [Test]
        public void SendAndAsyncReceive()
        {


            MessageQueueTemplate q = applicationContext["queueTemplate"] as MessageQueueTemplate;
            Assert.IsNotNull(q);

            q.ConvertAndSend("Hello World 1");
            q.ConvertAndSend("Hello World 2");
            q.ConvertAndSend("Hello World 3");
            q.ConvertAndSend("Hello World 4");
            q.ConvertAndSend("Hello World 5");

            Assert.AreEqual(0, listener.MessageCount);

            distributedTxMessageListenerContainer.Start();

            Thread.Sleep(waitInMillis);
            Assert.AreEqual(5, listener.MessageCount);

            distributedTxMessageListenerContainer.Stop();
            distributedTxMessageListenerContainer.Shutdown();
            Thread.Sleep(2500);

        }

        protected override string[] ConfigLocations
        {
            get { return new string[] { "assembly://Spring.Messaging.Tests/Spring.Messaging.Listener/DistributedTxMessageListenerContainerTests.xml" }; }
        }
    }
}