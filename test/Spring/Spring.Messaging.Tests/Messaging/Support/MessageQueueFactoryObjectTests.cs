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

using System.Messaging;
using NUnit.Framework;

using Spring.Testing.NUnit;

#endregion

namespace Spring.Messaging.Support
{
    /// <summary>
    /// This class contains tests for the MessageQueueFactory
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class MessageQueueFactoryObjectTests : AbstractDependencyInjectionSpringContextTests
    {
        [Test]
        public void CheckDefaultConstructorValues()
        {
            MessageQueueFactoryObject mqFactoryObject = new MessageQueueFactoryObject();
            MessageQueue queue = mqFactoryObject.GetObject() as MessageQueue;
            Assert.IsNotNull(queue);
            Assert.AreEqual(string.Empty, queue.Path);
            Assert.AreEqual(false, queue.DenySharedReceive);
            Assert.AreEqual(QueueAccessMode.SendAndReceive, queue.AccessMode);
            //EnableCache property not on queue.
        }

        [Test]
        public void CheckSimpleProperties()
        {
            MessageQueueFactoryObject mqFactoryObject = (MessageQueueFactoryObject) applicationContext["&testqueue"];
            Assert.AreEqual(@".\Private$\testqueue", mqFactoryObject.Path);
            Assert.AreEqual(true, mqFactoryObject.DenySharedReceive);
            Assert.AreEqual(QueueAccessMode.Receive, mqFactoryObject.AccessMode);
            Assert.AreEqual(true, mqFactoryObject.EnableCache);
            MessageQueue mq = (MessageQueue) applicationContext["testqueue"];
            Assert.AreEqual("MyLabel", mq.Label);
        }


        [Test]
        public void CheckGetObjectReturnsNewInstance()
        {
            MessageQueueFactoryObject mqFactoryObject = new MessageQueueFactoryObject();
            MessageQueue queue = mqFactoryObject.GetObject() as MessageQueue;
            MessageQueue anotherQueue = mqFactoryObject.GetObject() as MessageQueue;
            Assert.IsFalse(queue == anotherQueue, "Should be returning new instances");
            Assert.IsFalse(mqFactoryObject.IsSingleton,
                           "The MessageQueueFactoryObject class must be configured to return shared instances.");
        }

        [Test]
        public void ObjectTypePropertyYieldsTheCorrectType()
        {
            MessageQueueFactoryObject mqFactoryObject = new MessageQueueFactoryObject();
            Assert.AreEqual(typeof (MessageQueue), mqFactoryObject.ObjectType,
                            "The MessageQueueFactoryObject class ain't giving back DefaultMessageQueue types (it must).");
        }

        protected override string[] ConfigLocations
        {
            get { return new string[] {"assembly://Spring.Messaging.Tests/Spring.Messaging/queue-context.xml"}; }
        }
    }
}