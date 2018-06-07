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

using System.Threading;
using NUnit.Framework;
using Spring.Messaging.Core;
using Spring.Testing.NUnit;

#endregion

namespace Spring.Messaging.Listener
{
    /// <summary>
    /// This class contains tests for 
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id:$</version>
    [TestFixture]
    public class NonTransactionalMessageListenerContainerTests : AbstractDependencyInjectionSpringContextTests
    {

        private int waitInMillis = 20000;
        private NonTransactionalMessageListenerContainer container;
        private SimpleHandler listener;
        private SimpleExceptionHandler exceptionHandler;

        [SetUp]
        public override void SetUp()
        {
            MessageQueueUtils.RecreateMessageQueue(@".\Private$\testqueue", false);
            MessageQueueUtils.RecreateMessageQueue(@".\Private$\testresponsequeue", false);
                   
            base.SetUp();

            //Reset the state so that running all tests together will succeed.
            exceptionHandler.MessageCount = 0;
            listener.MessageCount = 0;
        }

        public SimpleExceptionHandler ExceptionHandler
        {
            set { exceptionHandler = value; }
        }

        public NonTransactionalMessageListenerContainer Container
        {
            get { return container; }
            set { container = value; }
        }

        public SimpleHandler Listener
        {
            get { return listener; }
            set { listener = value; }
        }

        [Test]
        [Ignore("Appveyor problems")]
        public void SendAndAsyncReceiveWithExceptionHandling()
        {
            MessageQueueTemplate q = applicationContext["testQueueTemplate"] as MessageQueueTemplate;
            Assert.IsNotNull(q);
            q.ConvertAndSend("Goodbye World 1");
            Assert.AreEqual(0, listener.MessageCount);
            container.Start();
            Thread.Sleep(waitInMillis);
            Assert.AreEqual(0, listener.MessageCount);
            Assert.AreEqual(1, exceptionHandler.MessageCount);
            container.Stop();
            container.Shutdown();
            Thread.Sleep(2500);
        }

        [Test]
        [Ignore("Appveyor problems")]
        public void SendAndAsyncReceive()
        {
            
            //MessageQueueTemplate q = applicationContext["testQueueTemplate"] as MessageQueueTemplate;
            
            MessageQueueTemplate q = applicationContext["testRemoteTemplate"] as MessageQueueTemplate;
            Assert.IsNotNull(q);
            
            q.ConvertAndSend("Hello World 1");
            q.ConvertAndSend("Hello World 2");
            q.ConvertAndSend("Hello World 3");
            q.ConvertAndSend("Hello World 4");
            q.ConvertAndSend("Hello World 5");

            //Reset the state so that running all tests together will succeed.
            exceptionHandler.MessageCount = 0;

            Assert.AreEqual(0, listener.MessageCount);

            container.Start();

            Thread.Sleep(waitInMillis);
            Assert.AreEqual(5, listener.MessageCount);
            Assert.AreEqual(0, exceptionHandler.MessageCount);

            container.Stop();
            container.Shutdown();
            Thread.Sleep(2500);                       
            
        }

        protected override string[] ConfigLocations
        {
            get { return new string[] { "assembly://Spring.Messaging.Tests/Spring.Messaging.Listener/NonTransactionalMessageListenerContainerTests.xml" }; }
        }
    }
}