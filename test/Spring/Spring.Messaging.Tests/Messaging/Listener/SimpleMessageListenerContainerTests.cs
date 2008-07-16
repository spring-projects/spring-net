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

using System;
using System.Messaging;
using System.Threading;
using NUnit.Framework;
using Spring.Messaging.Core;
using Spring.Testing.NUnit;

#endregion

namespace Spring.Messaging.Listener
{
    /// <summary>
    /// This class contains tests for SimpleMessageListenerContainer
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id:$</version>
    [TestFixture]
    public class SimpleMessageListenerContainerTests : AbstractDependencyInjectionSpringContextTests
    {


        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = "Property 'DefaultMessageQueue' is required")]
        public void EnsureMessageQueuePropertyIsSet()
        {
            SimpleMessageListenerContainer container = new SimpleMessageListenerContainer();
            container.AfterPropertiesSet();
            container.Start();
        }


        [Test]
        public void SendAndAsyncReceive()
        {

            SimpleMessageListenerContainer container =
                applicationContext["simpleMessageListenerContainer"] as SimpleMessageListenerContainer;
            SimpleMessageListener listener = applicationContext["simpleMessageListener"] as SimpleMessageListener;
            Assert.IsNotNull(container);
            Assert.IsNotNull(listener);
            
            MessageQueueTemplate q = applicationContext["queue"] as MessageQueueTemplate;
            Assert.IsNotNull(q);
            q.ConvertAndSend("Hello World 1");
           
            int waitInMillis = 2000;
            Thread.Sleep(waitInMillis);
            Assert.AreEqual(0, listener.MessageCount);

            container.Start();
            //pick up the message that is already in the queue
            Thread.Sleep(waitInMillis);
            Assert.AreEqual(1, listener.MessageCount);

            container.Stop();
            q.ConvertAndSend("Hello World 2");

            //what happens to this message, we stopped, so no new event is fired.
            Thread.Sleep(waitInMillis);
            Assert.AreEqual(1, listener.MessageCount);

            container.Start();
            Thread.Sleep(waitInMillis);
            //did we get hello world 2?
            Assert.AreEqual(2, listener.MessageCount);

            /*
            q.ConvertAndSend("Hello World");

            Thread.Sleep(waitInMillis);
            Assert.AreEqual(2, listener.MessageCount);
            */
            container.Stop();
            container.Shutdown();
            Thread.Sleep(waitInMillis);

        }


        protected override string[] ConfigLocations
        {
            get { return new string[] { "assembly://Spring.Messaging.Tests/Spring.Messaging.Listener/SimpleMessageListenerContainerTests.xml" }; }
        }
    }
}