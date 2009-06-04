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

using NUnit.Framework;
using Rhino.Mocks;
using Spring.Messaging.Ems.Common;
using TIBCO.EMS;

#endregion

namespace Spring.Messaging.Ems.Connections
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
            IConnection connection = (IConnection)mocks.CreateMock(typeof(IConnection));

            connection.Start();
            LastCall.On(connection).Repeat.Twice();
            connection.Stop();
            LastCall.On(connection).Repeat.Once();
            connection.Close();
            LastCall.On(connection).Repeat.Once();

            mocks.ReplayAll();
            
            SingleConnectionFactory scf = new SingleConnectionFactory(connection);
            IConnection con1 = scf.CreateConnection();
            con1.Start();
            con1.Stop(); // should be ignored
            con1.Close(); // should be ignored
            IConnection con2 = scf.CreateConnection();
            con2.Start();
            con2.Stop(); // should be ignored
            con2.Close(); // should be ignored.
            scf.Dispose();

            mocks.VerifyAll();
        }

        [Test]
        public void UsingConnectionFactory()
        {
            IConnectionFactory connectionFactory = (IConnectionFactory)mocks.CreateMock(typeof(IConnectionFactory));
            IConnection connection = (IConnection)mocks.CreateMock(typeof(IConnection));

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
            IConnectionFactory connectionFactory = (IConnectionFactory)mocks.CreateMock(typeof(IConnectionFactory));            
            IConnection connection = (IConnection)mocks.CreateMock(typeof(IConnection));

            Expect.Call(connectionFactory.CreateConnection()).Return(connection).Repeat.Once();
            connectionFactory.ClientID = "MyId";
            LastCall.On(connectionFactory).Repeat.Once();
            connection.ClientID = "MyId";
            LastCall.On(connection).Repeat.Once();
            connection.Start();
            LastCall.On(connection).Repeat.Twice();
            connection.Stop();
            LastCall.On(connection).Repeat.Once();
            connection.Close();
            LastCall.On(connection).Repeat.Once();

            mocks.ReplayAll();

            SingleConnectionFactory scf = new SingleConnectionFactory(connectionFactory);
            scf.ClientID = "MyId";
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
            IConnectionFactory connectionFactory = (IConnectionFactory)mocks.CreateMock(typeof(IConnectionFactory));
            IConnection connection = (IConnection)mocks.CreateMock(typeof(IConnection));


            IExceptionListener listener = new ChainedExceptionListener();
            Expect.Call(connectionFactory.CreateConnection()).Return(connection).Repeat.Once();
            connection.ExceptionListener = listener;
            LastCall.On(connection).IgnoreArguments();
            Expect.Call(connection.ExceptionListener).Return(listener).Repeat.Once();

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

            Assert.AreEqual(listener, connection.ExceptionListener);

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
            IConnectionFactory connectionFactory = (IConnectionFactory)mocks.CreateMock(typeof(IConnectionFactory));
            TestConnection con = new TestConnection();

            Expect.Call(connectionFactory.CreateConnection()).Return(con).Repeat.Twice();

            mocks.ReplayAll();

            SingleConnectionFactory scf = new SingleConnectionFactory(connectionFactory);
            scf.ReconnectOnException = true;
            IConnection con1 = scf.CreateConnection();

            con1.Start();
            con.FireExcpetionEvent(new EMSException(""));
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
            IConnectionFactory connectionFactory = (IConnectionFactory)mocks.CreateMock(typeof(IConnectionFactory));
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
            con.FireExcpetionEvent(new EMSException(""));
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
            IConnectionFactory connectionFactory = (IConnectionFactory)mocks.CreateMock(typeof(IConnectionFactory));
            IConnection connection = (IConnection)mocks.CreateMock(typeof(IConnection));
            ISession txSession = (ISession)mocks.CreateMock(typeof(ISession));
            ISession nonTxSession = (ISession)mocks.CreateMock(typeof(ISession));
            Expect.Call(connectionFactory.CreateConnection()).Return(connection).Repeat.Once();

            Expect.Call(connection.CreateSession(true, SessionMode.SessionTransacted)).Return(txSession).Repeat.Once();
            Expect.Call(txSession.Transacted).Return(true).Repeat.Twice();
            txSession.Rollback();
            LastCall.Repeat.Once();
            txSession.Commit();
            LastCall.Repeat.Once();            
            txSession.Close();
            LastCall.Repeat.Once();

            Expect.Call(connection.CreateSession(false, SessionMode.ClientAcknowledge)).Return(nonTxSession).Repeat.Once();           
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
            ISession session1 = con1.CreateSession(true, Session.SESSION_TRANSACTED);
            bool b = session1.Transacted;
            session1.Close();  // should be ignored
            session1 = con1.CreateSession(false, Session.CLIENT_ACKNOWLEDGE);
            session1.Close();  // should be ignored
            con1.Start();
            con1.Close(); // should be ignored
            IConnection con2 = scf.CreateConnection();
            ISession session2 = con2.CreateSession(false, Session.CLIENT_ACKNOWLEDGE);
            session2.Close(); // should be ignored
            session2 = con2.CreateSession(true, Session.SESSION_TRANSACTED);
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