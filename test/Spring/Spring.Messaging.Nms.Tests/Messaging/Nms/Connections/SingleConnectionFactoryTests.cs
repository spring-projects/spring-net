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
using NUnit.Framework;
using Rhino.Mocks;

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
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }


        [Test]
        public void UsingConnection()
        {
            IConnection connection = mocks.StrictMock<IConnection>();

            connection.Start();
            LastCall.On(connection).Repeat.Twice();
            connection.PurgeTempDestinations();
            LastCall.On( connection ).Repeat.Twice();
            connection.Stop();
            LastCall.On(connection).Repeat.Once();
            connection.Close();
            LastCall.On(connection).Repeat.Once();

            mocks.ReplayAll();
            
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

            mocks.VerifyAll();
        }

        [Test]
        public void UsingConnectionFactory()
        {
            IConnectionFactory connectionFactory = (IConnectionFactory)mocks.StrictMock<IConnectionFactory>();
            IConnection connection = mocks.StrictMock<IConnection>();

            Expect.Call(connectionFactory.CreateConnection()).Return(connection).Repeat.Once();
            connection.Start();
            LastCall.On(connection).Repeat.Twice();
            connection.Stop();
            LastCall.On(connection).Repeat.Once();
            connection.Close();
            LastCall.On(connection).Repeat.Once();


            mocks.ReplayAll();

            SingleConnectionFactory scf = new SingleConnectionFactory(connectionFactory);
            IConnection con1 = scf.CreateConnection();
            con1.Start();
            con1.Close(); // should be ignored
            IConnection con2 = scf.CreateConnection();
            con2.Start();
            con2.Close();   //should be ignored
            scf.Dispose();  //should trigger actual close

            mocks.VerifyAll();

        }

        [Test]
        public void UsingConnectionFactoryAndClientId()
        {
            IConnectionFactory connectionFactory = mocks.StrictMock<IConnectionFactory>();
            IConnection connection = mocks.StrictMock<IConnection>();

            Expect.Call(connectionFactory.CreateConnection()).Return(connection).Repeat.Once();
            connection.ClientId = "MyId";
            LastCall.On(connection).Repeat.Once();
            connection.Start();
            LastCall.On(connection).Repeat.Twice();
            connection.Stop();
            LastCall.On(connection).Repeat.Once();
            connection.Close();
            LastCall.On(connection).Repeat.Once();

            mocks.ReplayAll();

            SingleConnectionFactory scf = new SingleConnectionFactory(connectionFactory);
            scf.ClientId = "MyId";
            IConnection con1 = scf.CreateConnection();
            con1.Start();
            con1.Close(); // should be ignored
            IConnection con2 = scf.CreateConnection();
            con2.Start();
            con2.Close();   // should be ignored
            scf.Dispose();  // should trigger actual close

            mocks.VerifyAll();


        }


        [Test]
        public void UsingConnectionFactoryAndExceptionListener()
        {
            IConnectionFactory connectionFactory = mocks.StrictMock<IConnectionFactory>();
            IConnection connection = mocks.StrictMock<IConnection>();


            IExceptionListener listener = new ChainedExceptionListener();
            Expect.Call(connectionFactory.CreateConnection()).Return(connection).Repeat.Once();
            connection.ExceptionListener += listener.OnException;
            LastCall.On(connection).IgnoreArguments();

            connection.Start();
            LastCall.On(connection).Repeat.Twice();
            connection.Stop();
            LastCall.On(connection).Repeat.Once();
            connection.Close();
            LastCall.On(connection).Repeat.Once();

            mocks.ReplayAll();
            
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
                       
            mocks.VerifyAll();
        }

        [Test]
        public void UsingConnectionFactoryAndReconnectOnException()
        {
            IConnectionFactory connectionFactory = mocks.StrictMock<IConnectionFactory>();
            TestConnection con = new TestConnection();

            Expect.Call(connectionFactory.CreateConnection()).Return(con).Repeat.Twice();

            mocks.ReplayAll();

            SingleConnectionFactory scf = new SingleConnectionFactory(connectionFactory);
            scf.ReconnectOnException = true;
            IConnection con1 = scf.CreateConnection();

            con1.Start();
            con.FireExcpetionEvent(new NMSException(""));
            IConnection con2 = scf.CreateConnection();
            con2.Start();
            scf.Dispose();

            mocks.VerifyAll();

            Assert.AreEqual(2, con.StartCount);
            Assert.AreEqual(2, con.CloseCount);
        }

        [Test]
        public void UsingConnectionFactoryAndExceptionListenerAndReconnectOnException()
        {
            IConnectionFactory connectionFactory = mocks.StrictMock<IConnectionFactory>();
            TestConnection con = new TestConnection();
            TestExceptionListener listener = new TestExceptionListener();

            Expect.Call(connectionFactory.CreateConnection()).Return(con).Repeat.Twice();

            mocks.ReplayAll();
            
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

            mocks.VerifyAll();

            Assert.AreEqual(2, con.StartCount);
            Assert.AreEqual(2, con.CloseCount);
            Assert.AreEqual(1, listener.Count);
        }

        [Test]
        public void CachingConnectionFactory()
        {
            IConnectionFactory connectionFactory = mocks.StrictMock<IConnectionFactory>();
            IConnection connection = mocks.StrictMock<IConnection>();
            ISession txSession = mocks.StrictMock<ISession>();
            ISession nonTxSession = mocks.StrictMock<ISession>();
            Expect.Call(connectionFactory.CreateConnection()).Return(connection).Repeat.Once();

            Expect.Call(connection.CreateSession(AcknowledgementMode.Transactional)).Return(txSession).Repeat.Once();
            Expect.Call(txSession.Transacted).Return(true).Repeat.Twice();
            txSession.Rollback();
            LastCall.Repeat.Once();
            txSession.Commit();
            LastCall.Repeat.Once();            
            txSession.Close();
            LastCall.Repeat.Once();

            Expect.Call(connection.CreateSession(AcknowledgementMode.ClientAcknowledge)).Return(nonTxSession).Repeat.Once();           
            nonTxSession.Close();
            LastCall.Repeat.Once();
            connection.Start();
            LastCall.Repeat.Twice();
            connection.Stop();
            LastCall.Repeat.Once();
            connection.Close();
            LastCall.Repeat.Once();

            mocks.ReplayAll();

            CachingConnectionFactory scf = new CachingConnectionFactory(connectionFactory);
            scf.ReconnectOnException = false;

            IConnection con1 = scf.CreateConnection();
            ISession session1 = con1.CreateSession(AcknowledgementMode.Transactional);
            bool b = session1.Transacted;
            session1.Close();  // should be ignored
            session1 = con1.CreateSession(AcknowledgementMode.ClientAcknowledge);
            session1.Close();  // should be ignored
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

            mocks.Verify(connectionFactory);
            mocks.Verify(connection);
            mocks.Verify(txSession);
            mocks.Verify(nonTxSession);


        }
    }
}