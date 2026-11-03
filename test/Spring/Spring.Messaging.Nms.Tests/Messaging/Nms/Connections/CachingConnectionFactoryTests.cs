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

using Apache.NMS;
using FakeItEasy;
using NUnit.Framework;

namespace Spring.Messaging.Nms.Connections;

/// <summary>
/// This class contains tests for CachingConnectionFactory
/// </summary>
/// <author>Mark Pollack</author>
[TestFixture]
public class CachingConnectionFactoryTests
{
    [Test]
    public void CachedSession()
    {
        IConnectionFactory connectionFactory = CreateConnectionFactory();

        CachingConnectionFactory cachingConnectionFactory = new CachingConnectionFactory();
        cachingConnectionFactory.TargetConnectionFactory = connectionFactory;

        IConnection con1 = cachingConnectionFactory.CreateConnection();

        ISession session1 = con1.CreateSession(AcknowledgementMode.Transactional);
        TestSession testSession = GetTestSession(session1);
        Assert.That(testSession.CreatedCount, Is.EqualTo(1));
        Assert.That(testSession.CloseCount, Is.EqualTo(0));

        session1.Close(); // won't close, will put in session cache.
        Assert.That(testSession.CloseCount, Is.EqualTo(0));

        ISession session2 = con1.CreateSession(AcknowledgementMode.Transactional);

        TestSession testSession2 = GetTestSession(session2);

        Assert.That(testSession2, Is.SameAs(testSession));

        Assert.That(testSession.CreatedCount, Is.EqualTo(1));
        Assert.That(testSession.CloseCount, Is.EqualTo(0));

        //don't explicitly call close on
    }

    private static TestSession GetTestSession(ISession session1)
    {
        CachedSession cachedSession = session1 as CachedSession;
        Assert.That(cachedSession, Is.Not.Null);
        TestSession testSession = cachedSession.TargetSession as TestSession;
        Assert.That(testSession, Is.Not.Null);
        return testSession;
    }

    [Test]
    public void CachedSessionTwoRequests()
    {
        IConnectionFactory connectionFactory = CreateConnectionFactory();

        CachingConnectionFactory cachingConnectionFactory = new CachingConnectionFactory();
        cachingConnectionFactory.TargetConnectionFactory = connectionFactory;
        IConnection con1 = cachingConnectionFactory.CreateConnection();

        ISession session1 = con1.CreateSession(AcknowledgementMode.Transactional);
        TestSession testSession1 = GetTestSession(session1);
        Assert.That(testSession1.CreatedCount, Is.EqualTo(1));
        Assert.That(testSession1.CloseCount, Is.EqualTo(0));

        //will create a new one, not in the cache.
        ISession session2 = con1.CreateSession(AcknowledgementMode.Transactional);
        TestSession testSession2 = GetTestSession(session2);
        Assert.That(testSession2.CreatedCount, Is.EqualTo(1));
        Assert.That(testSession2.CloseCount, Is.EqualTo(0));

        Assert.That(testSession2, Is.Not.SameAs(testSession1));

        Assert.That(session2, Is.Not.SameAs(session1));

        session1.Close(); // will be put in the cache

        ISession session3 = con1.CreateSession(AcknowledgementMode.Transactional);
        TestSession testSession3 = GetTestSession(session3);
        Assert.That(testSession3, Is.SameAs(testSession1));
        Assert.That(session3, Is.SameAs(session1));
        Assert.That(testSession1.CreatedCount, Is.EqualTo(1));
        Assert.That(testSession1.CloseCount, Is.EqualTo(0));
    }

    /// <summary>
    /// Tests that the same underlying instance of the message producer is returned after
    /// creating a session, creating the producer (A), closing the session, and creating another
    /// producer (B).  Assert that (A)=(B).
    /// </summary>
    [Test]
    public void CachedMessageProducer()
    {
        IConnectionFactory connectionFactory = CreateConnectionFactory();

        CachingConnectionFactory cachingConnectionFactory = new CachingConnectionFactory();
        cachingConnectionFactory.TargetConnectionFactory = connectionFactory;
        IConnection con1 = cachingConnectionFactory.CreateConnection();

        ISession sessionA = con1.CreateSession(AcknowledgementMode.Transactional);
        IMessageProducer producerA = sessionA.CreateProducer();
        TestMessageProducer tmpA = GetTestMessageProducer(producerA);

        sessionA.Close();

        ISession sessionB = con1.CreateSession(AcknowledgementMode.Transactional);
        IMessageProducer producerB = sessionB.CreateProducer();
        TestMessageProducer tmpB = GetTestMessageProducer(producerB);

        Assert.That(tmpB, Is.SameAs(tmpA));
    }

    [Test]
    public void CachedMessageProducerTwoRequests()
    {
        IConnectionFactory connectionFactory = CreateConnectionFactory();

        CachingConnectionFactory cachingConnectionFactory = new CachingConnectionFactory();
        cachingConnectionFactory.TargetConnectionFactory = connectionFactory;
        IConnection con1 = cachingConnectionFactory.CreateConnection();

        ISession sessionA = con1.CreateSession(AcknowledgementMode.Transactional);
        IMessageProducer producerA = sessionA.CreateProducer();
        TestMessageProducer tmpA = GetTestMessageProducer(producerA);

        ISession sessionB = con1.CreateSession(AcknowledgementMode.Transactional);
        IMessageProducer producerB = sessionB.CreateProducer();
        TestMessageProducer tmpB = GetTestMessageProducer(producerB);

        Assert.That(tmpB, Is.Not.SameAs(tmpA));

        sessionA.Close();

        ISession sessionC = con1.CreateSession(AcknowledgementMode.Transactional);
        IMessageProducer producerC = sessionC.CreateProducer();
        TestMessageProducer tmpC = GetTestMessageProducer(producerC);

        Assert.That(tmpC, Is.SameAs(tmpA));
    }

#if NETFRAMEWORK
    /// <summary>
    /// Tests that the same underlying instance of the message consumer is returned after
    /// creating a session, creating the consumer (A), closing the session, and creating another
    /// consumer (B).  Assert that (A)=(B).
    /// </summary>
    [Test]
    public void CachedMessageConsumer()
    {
        IConnectionFactory connectionFactory = CreateConnectionFactory();

        CachingConnectionFactory cachingConnectionFactory = new CachingConnectionFactory();
        cachingConnectionFactory.TargetConnectionFactory = connectionFactory;
        IConnection con1 = cachingConnectionFactory.CreateConnection();

        ISession sessionA = con1.CreateSession(AcknowledgementMode.Transactional);
        IDestination destination = new Apache.NMS.ActiveMQ.Commands.ActiveMQQueue("test.dest");
        IMessageConsumer consumerA = sessionA.CreateConsumer(destination);
        TestMessageConsumer tmpA = GetTestMessageConsumer(consumerA);

        sessionA.Close();

        ISession sessionB = con1.CreateSession(AcknowledgementMode.Transactional);
        IMessageConsumer consumerB = sessionB.CreateConsumer(destination);
        TestMessageConsumer tmpB = GetTestMessageConsumer(consumerB);

        Assert.That(tmpB, Is.SameAs(tmpA));
    }
#endif

    private IConnectionFactory CreateConnectionFactory()
    {
        IConnectionFactory connectionFactory = A.Fake<IConnectionFactory>();
        IConnection connection = new TestConnection();

        A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection).Once();
        return connectionFactory;
    }

    private static TestMessageConsumer GetTestMessageConsumer(IMessageConsumer consumer)
    {
        CachedMessageConsumer cmp1 = consumer as CachedMessageConsumer;
        Assert.That(cmp1, Is.Not.Null);
        TestMessageConsumer tmp1 = cmp1.Target as TestMessageConsumer;
        Assert.That(tmp1, Is.Not.Null);
        return tmp1;
    }

    private static TestMessageProducer GetTestMessageProducer(IMessageProducer producer1)
    {
        CachedMessageProducer cmp1 = producer1 as CachedMessageProducer;
        Assert.That(cmp1, Is.Not.Null);
        TestMessageProducer tmp1 = cmp1.Target as TestMessageProducer;
        Assert.That(tmp1, Is.Not.Null);
        return tmp1;
    }
}
