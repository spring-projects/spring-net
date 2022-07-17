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

using Apache.NMS;

using FakeItEasy;

using NUnit.Framework;

using Spring.Messaging.Nms.Core;

#endregion

namespace Spring.Messaging.Nms.Connections
{
    /// <summary>
    /// This class contains tests for the SingleConnectionFactory
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class SingleConnectionFactoryTests
    {
        [Test]
        public void UsingConnection()
        {
            IConnection connection = A.Fake<IConnection>();

            SingleConnectionFactory scf = new SingleConnectionFactory(connection);
            IConnection con1 = scf.CreateConnection();
            con1.Start();
            con1.PurgeTempDestinations();
            con1.Stop(); // should be ignored
            con1.Close(); // should be ignored
            IConnection con2 = scf.CreateConnection();
            con2.Start();
            con1.PurgeTempDestinations();
            con2.Stop(); // should be ignored
            con2.Close(); // should be ignored.
            scf.Dispose();

            A.CallTo(() => connection.Start()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => connection.PurgeTempDestinations()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => connection.Stop()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Close()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void UsingConnectionFactory()
        {
            IConnectionFactory connectionFactory = A.Fake<IConnectionFactory>();
            IConnection connection = A.Fake<IConnection>();

            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection).Once();

            SingleConnectionFactory scf = new SingleConnectionFactory(connectionFactory);
            IConnection con1 = scf.CreateConnection();
            con1.Start();
            con1.Close(); // should be ignored
            IConnection con2 = scf.CreateConnection();
            con2.Start();
            con2.Close(); //should be ignored
            scf.Dispose(); //should trigger actual close

            A.CallTo(() => connection.Start()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => connection.Stop()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Close()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void UsingConnectionFactoryAndClientId()
        {
            IConnectionFactory connectionFactory = A.Fake<IConnectionFactory>();
            IConnection connection = A.Fake<IConnection>();

            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection).Once();

            SingleConnectionFactory scf = new SingleConnectionFactory(connectionFactory);
            scf.ClientId = "MyId";
            IConnection con1 = scf.CreateConnection();
            con1.Start();
            con1.Close(); // should be ignored
            IConnection con2 = scf.CreateConnection();
            con2.Start();
            con2.Close(); // should be ignored
            scf.Dispose(); // should trigger actual close

            A.CallToSet(() => connection.ClientId).WhenArgumentsMatch(x => x.Get<string>(0) == "MyId").MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Start()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => connection.Stop()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Close()).MustHaveHappenedOnceExactly();
        }


        [Test]
        public void UsingConnectionFactoryAndExceptionListener()
        {
            IConnectionFactory connectionFactory = A.Fake<IConnectionFactory>();
            IConnection connection = A.Fake<IConnection>();


            IExceptionListener listener = new ChainedExceptionListener();
            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection).Once();

            SingleConnectionFactory scf = new SingleConnectionFactory(connectionFactory);
            scf.ExceptionListener = listener;
            IConnection con1 = scf.CreateConnection();

            //can't look at invocation list on event ...grrr.

            con1.Start();
            con1.Stop(); // should be ignored
            con1.Close(); // should be ignored
            IConnection con2 = scf.CreateConnection();
            con2.Start();
            con2.Stop();
            con2.Close();
            scf.Dispose();

            // TODO
            //connection.ExceptionListener += listener.OnException;
             // LastCall.On(connection).IgnoreArguments();


            A.CallTo(() => connection.Start()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => connection.Stop()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Close()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void UsingConnectionFactoryAndReconnectOnException()
        {
            IConnectionFactory connectionFactory = A.Fake<IConnectionFactory>();
            TestConnection con = new TestConnection();

            A.CallTo(() => connectionFactory.CreateConnection()).Returns(con).Twice();

            SingleConnectionFactory scf = new SingleConnectionFactory(connectionFactory);
            scf.ReconnectOnException = true;
            IConnection con1 = scf.CreateConnection();

            con1.Start();
            con.FireExcpetionEvent(new NMSException(""));
            IConnection con2 = scf.CreateConnection();
            con2.Start();
            scf.Dispose();

            Assert.AreEqual(2, con.StartCount);
            Assert.AreEqual(2, con.CloseCount);
        }

        [Test]
        public void UsingConnectionFactoryAndExceptionListenerAndReconnectOnException()
        {
            IConnectionFactory connectionFactory = A.Fake<IConnectionFactory>();
            TestConnection con = new TestConnection();
            TestExceptionListener listener = new TestExceptionListener();

            A.CallTo(() => connectionFactory.CreateConnection()).Returns(con).Twice();

            SingleConnectionFactory scf = new SingleConnectionFactory(connectionFactory);
            scf.ExceptionListener = listener;
            scf.ReconnectOnException = true;
            IConnection con1 = scf.CreateConnection();
            //Assert.AreSame(listener, );
            con1.Start();
            con.FireExcpetionEvent(new NMSException(""));
            IConnection con2 = scf.CreateConnection();
            con2.Start();
            scf.Dispose();

            Assert.AreEqual(2, con.StartCount);
            Assert.AreEqual(2, con.CloseCount);
            Assert.AreEqual(1, listener.Count);
        }

        [Test]
        public void CachingConnectionFactory()
        {
            IConnectionFactory connectionFactory = A.Fake<IConnectionFactory>();
            IConnection connection = A.Fake<IConnection>();
            ISession txSession = A.Fake<ISession>();
            ISession nonTxSession = A.Fake<ISession>();
            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection).Once();
            A.CallTo(() => connectionFactory.CreateConnectionAsync()).Returns(connection).Once();

            A.CallTo(() => connection.CreateSession(AcknowledgementMode.Transactional)).Returns(txSession).Once();
            A.CallTo(() => connection.CreateSessionAsync(AcknowledgementMode.Transactional)).Returns(txSession).Once();
            
            A.CallTo(() => txSession.Transacted).Returns(true).Twice();

            A.CallTo(() => connection.CreateSession(AcknowledgementMode.ClientAcknowledge)).Returns(nonTxSession).Once();
            A.CallTo(() => connection.CreateSessionAsync(AcknowledgementMode.ClientAcknowledge)).Returns(nonTxSession).Once();

            CachingConnectionFactory scf = new CachingConnectionFactory(connectionFactory);
            scf.ReconnectOnException = false;

            IConnection con1 = scf.CreateConnection();
            ISession session1 = con1.CreateSession(AcknowledgementMode.Transactional);
            bool b = session1.Transacted;
            session1.Close(); // should be ignored
            session1 = con1.CreateSession(AcknowledgementMode.ClientAcknowledge);
            session1.Close(); // should be ignored
            con1.Start();
            con1.Close(); // should be ignored
            IConnection con2 = scf.CreateConnection();
            ISession session2 = con2.CreateSession(AcknowledgementMode.ClientAcknowledge);
            session2.Close(); // should be ignored
            session2 = con2.CreateSession(AcknowledgementMode.Transactional);
            session2.Commit();
            session2.Close(); // should be ignored
            con2.Start();
            con2.Close();
            scf.Dispose();

            A.CallTo(() => txSession.RollbackAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => txSession.Commit()).MustHaveHappenedOnceExactly();
            A.CallTo(() => txSession.CloseAsync()).MustHaveHappenedOnceExactly();

            A.CallTo(() => nonTxSession.CloseAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Start()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => connection.Stop()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Close()).MustHaveHappenedOnceExactly();
        }
    }
}