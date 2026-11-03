/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.Messaging;
using NUnit.Framework;
using Spring.Testing.NUnit;

namespace Spring.Messaging.Support;

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
        Assert.That(queue, Is.Not.Null);
        Assert.That(queue.Path, Is.Empty);
        Assert.That(queue.DenySharedReceive, Is.EqualTo(false));
        Assert.That(queue.AccessMode, Is.EqualTo(QueueAccessMode.SendAndReceive));
        //EnableCache property not on queue.
    }

    [Test]
    public void CheckSimpleProperties()
    {
        MessageQueueFactoryObject mqFactoryObject = (MessageQueueFactoryObject) applicationContext["&testqueue"];
        Assert.That(mqFactoryObject.Path, Is.EqualTo(@".\Private$\testqueue"));
        Assert.That(mqFactoryObject.DenySharedReceive, Is.EqualTo(true));
        Assert.That(mqFactoryObject.AccessMode, Is.EqualTo(QueueAccessMode.Receive));
        Assert.That(mqFactoryObject.EnableCache, Is.EqualTo(true));
        MessageQueue mq = (MessageQueue) applicationContext["testqueue"];
        Assert.That(mq.Label, Is.EqualTo("MyLabel"));
    }

    [Test]
    public void CheckGetObjectReturnsNewInstance()
    {
        MessageQueueFactoryObject mqFactoryObject = new MessageQueueFactoryObject();
        MessageQueue queue = mqFactoryObject.GetObject() as MessageQueue;
        MessageQueue anotherQueue = mqFactoryObject.GetObject() as MessageQueue;
        Assert.That(queue == anotherQueue, Is.False, "Should be returning new instances");
        Assert.That(mqFactoryObject.IsSingleton,
            Is.False,
            "The MessageQueueFactoryObject class must be configured to return shared instances.");
    }

    [Test]
    public void ObjectTypePropertyYieldsTheCorrectType()
    {
        MessageQueueFactoryObject mqFactoryObject = new MessageQueueFactoryObject();
        Assert.That(mqFactoryObject.ObjectType, Is.EqualTo(typeof(MessageQueue)),
            "The MessageQueueFactoryObject class ain't giving back DefaultMessageQueue types (it must).");
    }

    protected override string[] ConfigLocations
    {
        get { return new string[] { "assembly://Spring.Messaging.Tests/Spring.Messaging/queue-context.xml" }; }
    }
}