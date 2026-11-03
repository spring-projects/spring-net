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

using NUnit.Framework;
using Spring.Messaging.Core;
using Spring.Testing.NUnit;

namespace Spring.Messaging.Listener;

/// <summary>
/// This class contains integration tests for the TransactionalMessageListenerContainer
/// </summary>
/// <author>Mark Pollack</author>
[TestFixture]
public class TransactionalMessageListenerContainerTests : AbstractDependencyInjectionSpringContextTests
{
    private int waitInMillis = 20000;
    private TransactionalMessageListenerContainer transactionalMessageListenerContainer;
    private SimpleHandler listener;

    [SetUp]
    public override void SetUp()
    {
        MessageQueueUtils.RecreateMessageQueue(@".\Private$\testtxqueue", true);
        MessageQueueUtils.RecreateMessageQueue(@".\Private$\testtxretryqueue", true);
        MessageQueueUtils.RecreateMessageQueue(@".\Private$\testtxresponsequeue", true);
        if (listener != null)
            listener.MessageCount = 0; //reset the property between tests b/c the object lifecycle is singleton!
        base.SetUp();
    }

    public TransactionalMessageListenerContainer TransactionalMessageListenerContainer
    {
        set { transactionalMessageListenerContainer = value; }
    }

    public SimpleHandler SimpleHandler
    {
        set { listener = value; }
    }

    [Test]
    public void EnsureMessageQueuePropertyIsSet()
    {
        TransactionalMessageListenerContainer container = new TransactionalMessageListenerContainer();

        try
        {
            container.AfterPropertiesSet();
            Assert.Fail("Expected ArgumentException not thrown.");
        }
        catch (ArgumentException ex)
        {
            Assert.That(ex.Message, Is.EqualTo("Property 'MessageQueueObjectName' is required"));
        }
    }

    [Test]
    public void EnsureuseContainerManagedMessageQueueTransactionIsSetCorrectly()
    {
        TransactionalMessageListenerContainer container = applicationContext["transactionalMessageListenerContainer"] as TransactionalMessageListenerContainer;
        Assert.That(container, Is.Not.Null);
        Assert.That(container.UseContainerManagedMessageQueueTransaction, Is.EqualTo(true));
    }

    [Test]
    public void SendAndAsyncReceiveWithExceptionHandling()
    {
        listener.MessageCount = 0;

        MessageQueueTemplate q = applicationContext["queueTemplate"] as MessageQueueTemplate;
        Assert.That(q, Is.Not.Null);

        MessageQueueTemplate retryQ = applicationContext["retryQueueTemplate"] as MessageQueueTemplate;
        Assert.That(retryQ, Is.Not.Null);

        q.ConvertAndSend("Goodbye World 1");

        Assert.That(listener.MessageCount, Is.EqualTo(0));
        transactionalMessageListenerContainer.Start();

        Thread.Sleep(waitInMillis);

        transactionalMessageListenerContainer.Stop();
        transactionalMessageListenerContainer.Shutdown();
        Thread.Sleep(2500);

        object msg = retryQ.ReceiveAndConvert();
        Assert.That(msg, Is.Not.Null);
        string textMsg = msg as string;
        Assert.That(textMsg, Is.Not.Null);
        Assert.That(textMsg, Is.EqualTo("Goodbye World 1"));
    }

    [Test]
    public void SendAndAsyncReceive()
    {
        listener.MessageCount = 0;

        MessageQueueTemplate q = applicationContext["queueTemplate"] as MessageQueueTemplate;
        Assert.That(q, Is.Not.Null);

        q.ConvertAndSend("Hello World 1");
        q.ConvertAndSend("Hello World 2");
        q.ConvertAndSend("Hello World 3");
        q.ConvertAndSend("Hello World 4");
        q.ConvertAndSend("Hello World 5");

        Assert.That(listener.MessageCount, Is.EqualTo(0));

        transactionalMessageListenerContainer.Start();

        Thread.Sleep(waitInMillis * 2);
        Assert.That(listener.MessageCount, Is.EqualTo(5));

        transactionalMessageListenerContainer.Stop();
        transactionalMessageListenerContainer.Shutdown();
        Thread.Sleep(2500);
    }

    protected override string[] ConfigLocations
    {
        get { return new string[] { "assembly://Spring.Messaging.Tests/Spring.Messaging.Listener/TransactionalMessageListenerContainerTests.xml" }; }
    }
}
