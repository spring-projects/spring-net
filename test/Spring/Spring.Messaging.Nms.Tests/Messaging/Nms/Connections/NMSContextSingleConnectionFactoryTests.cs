#region License
// /*
//  * Copyright 2022 the original author or authors.
//  *
//  * Licensed under the Apache License, Version 2.0 (the "License");
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  *      http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */
#endregion

#region Imports

using Apache.NMS;

using FakeItEasy;

using NUnit.Framework;

#endregion

namespace Spring.Messaging.Nms.Connections
{
    /// <summary>
    /// Adapted NMSContext based version of SingleConnectionFactoryTest
    /// </summary>
    /// <see cref="SingleConnectionFactoryTests"/>
    [TestFixture]
    public class NMSContextSingleConnectionFactoryTests
    {
        [Test]
        public void UsingConnection()
        {
            IConnection connection = A.Fake<IConnection>();

            SingleConnectionFactory scf = new SingleConnectionFactory(connection);
            INMSContext con1 = scf.CreateContext();
            con1.Start();
            con1.PurgeTempDestinations();
            con1.Stop(); // should be ignored
            con1.Close(); // should be ignored
            INMSContext con2 = scf.CreateContext();
            con2.Start();
            con1.PurgeTempDestinations();
            con2.Stop(); // should be ignored
            con2.Close(); // should be ignored.
            scf.Dispose();

            A.CallTo(() => connection.StartAsync()).MustHaveHappenedTwiceExactly();
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
            INMSContext con1 = scf.CreateContext();
            con1.Start();
            con1.Close(); // should be ignored
            INMSContext con2 = scf.CreateContext();
            con2.Start();
            con2.Close(); //should be ignored
            scf.Dispose(); //should trigger actual close

            A.CallTo(() => connection.StartAsync()).MustHaveHappenedTwiceExactly();
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
            INMSContext con1 = scf.CreateContext();
            con1.Start();
            con1.Close(); // should be ignored
            INMSContext con2 = scf.CreateContext();
            con2.Start();
            con2.Close(); // should be ignored
            scf.Dispose(); // should trigger actual close

            A.CallToSet(() => connection.ClientId).WhenArgumentsMatch(x => x.Get<string>(0) == "MyId").MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.StartAsync()).MustHaveHappenedTwiceExactly();
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
            INMSContext con1 = scf.CreateContext();

            con1.Start();
            con.FireExcpetionEvent(new NMSException(""));
            INMSContext con2 = scf.CreateContext();
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
            INMSContext con1 = scf.CreateContext();
            //Assert.AreSame(listener, );
            con1.Start();
            con.FireExcpetionEvent(new NMSException(""));
            INMSContext con2 = scf.CreateContext();
            con2.Start();
            scf.Dispose();

            Assert.AreEqual(2, con.StartCount);
            Assert.AreEqual(2, con.CloseCount);
            Assert.AreEqual(1, listener.Count);
        }

    }
}
