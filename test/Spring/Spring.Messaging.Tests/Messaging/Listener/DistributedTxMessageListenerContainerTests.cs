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
/// This class contains tests for DistributedTxMessageListenerContainer
/// </summary>
/// <author>Mark Pollack</author>
[TestFixture]
public class DistributedTxMessageListenerContainerTests : AbstractDependencyInjectionSpringContextTests
{
    private int waitInMillis = 20000;
    private DistributedTxMessageListenerContainer distributedTxMessageListenerContainer;
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
        Assert.That(q, Is.Not.Null);

        MessageQueueTemplate retryQ = applicationContext["retryQueueTemplate"] as MessageQueueTemplate;
        Assert.That(retryQ, Is.Not.Null);

        q.ConvertAndSend("Goodbye World 1");

        Assert.That(listener.MessageCount, Is.EqualTo(0), "PRECONDITION FAILURE: Unable to send the message!");

        distributedTxMessageListenerContainer.Start();

        Thread.Sleep(waitInMillis);

        distributedTxMessageListenerContainer.Stop();
        distributedTxMessageListenerContainer.Shutdown();
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
        const int MESSAGE_COUNT = 5;

        //must match the retry count in the object registration for test to pass!
        //const int EXCEPTION_QUEUE_RETRY_COUNT = 2;

        int expectedMessageCount = MESSAGE_COUNT; // +(MESSAGE_COUNT * EXCEPTION_QUEUE_RETRY_COUNT);

        MessageQueueTemplate q = applicationContext["queueTemplate"] as MessageQueueTemplate;
        Assert.That(q, Is.Not.Null);

        for (int i = 0; i < MESSAGE_COUNT; i++)
        {
            q.ConvertAndSend(String.Format("Hello World {0}", (i + 1)));
        }

        Assert.That(listener.MessageCount, Is.EqualTo(0));

        distributedTxMessageListenerContainer.Start();

        Thread.Sleep(10000);
        /*
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();

        while (listener.MessageCount < expectedMessageCount)
        {
            if (timer.ElapsedMilliseconds > 10000)
            {
                break;
            }
                //Assert.Fail("Did not receive expected number of messages within the permitted time limit.");
            Thread.Sleep(1000);
        }

        timer.Stop();

        System.Diagnostics.Debug.WriteLine("elapsed time = " + timer.ElapsedMilliseconds);
        */

        Assert.That(listener.MessageCount, Is.EqualTo(expectedMessageCount));

        distributedTxMessageListenerContainer.Stop();
        distributedTxMessageListenerContainer.Shutdown();
        Thread.Sleep(2500);

        MessageQueueTemplate responseQ = applicationContext["responseQueueTemplate"] as MessageQueueTemplate;
        responseQ.ReceiveTimeout = new System.TimeSpan(0, 0, 2);

        Assert.That(responseQ, Is.Not.Null);
        for (int i = 0; i < MESSAGE_COUNT; i++)
        {
            Assert.That(responseQ.ReceiveAndConvert(), Is.Not.Null);
        }
    }

    protected override string[] ConfigLocations
    {
        get { return new string[] { "assembly://Spring.Messaging.Tests/Spring.Messaging.Listener/DistributedTxMessageListenerContainerTests.xml" }; }
    }
}
