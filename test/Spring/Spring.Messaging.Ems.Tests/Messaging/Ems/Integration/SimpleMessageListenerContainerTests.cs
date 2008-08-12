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
using System.Threading;
using NUnit.Framework;
using Spring.Messaging.Ems.Core;
using Spring.Messaging.Ems.Listener;
using Spring.Testing.NUnit;

#endregion

namespace Spring.Messaging.Ems.Integration
{
    /// <summary>
    /// This class contains integration tests for the SimpleMessageListenerContainer
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id:$</version>
    [TestFixture]
    public class SimpleMessageListenerContainerTests : AbstractDependencyInjectionSpringContextTests
    {

        [Test]
        [Explicit]
        public void SendAndAsyncReceive()        
        {
            SimpleMessageListenerContainer container =
                (SimpleMessageListenerContainer) applicationContext["SimpleMessageListenerContainer"];
            SimpleMessageListener listener = applicationContext["SimpleMessageListener"] as SimpleMessageListener;           
            Assert.IsNotNull(container);
            Assert.IsNotNull(listener);
            

            EmsTemplate emsTemplate = (EmsTemplate) applicationContext["MessageTemplate"] as EmsTemplate;
            Assert.IsNotNull(emsTemplate);

            Assert.AreEqual(0, listener.MessageCount);
            emsTemplate.ConvertAndSend("Hello World 1");

            int waitInMillis = 2000;
            Thread.Sleep(waitInMillis);
            Assert.AreEqual(1,listener.MessageCount);

            container.Stop();
            Console.WriteLine("container stopped.");
            emsTemplate.ConvertAndSend("Hello World 2");
            Thread.Sleep(waitInMillis);
            Assert.AreEqual(1, listener.MessageCount);

            container.Start();
            Console.WriteLine("container started.");
            Thread.Sleep(waitInMillis);
            Assert.AreEqual(2, listener.MessageCount);
            
            container.Shutdown();

            Thread.Sleep(waitInMillis);


        }


        protected override string[] ConfigLocations
        {
            get { return new string[] { "assembly://Spring.Messaging.Ems.Tests/Spring.Messaging.Ems.Integration/SimpleMessageListenerContainerTests.xml" }; }
        }

        
    }
}