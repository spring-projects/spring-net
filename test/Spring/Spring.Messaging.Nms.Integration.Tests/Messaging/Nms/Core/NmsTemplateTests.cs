#region License

/*
 * Copyright 2004-2009 the original author or authors.
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

using System;

using Apache.NMS;
using NUnit.Framework;

using Spring.Testing.NUnit;

namespace Spring.Messaging.Nms.Core
{
    [TestFixture]
    public class NmsTemplateTests : AbstractDependencyInjectionSpringContextTests
    {
        protected IConnectionFactory nmsConnectionFactory;

        protected IConnectionFactory connectionFactory;

        protected NmsTemplate nmsTemplate;

        /// <summary>
        /// Default constructor for NmsTemplateTests.
        /// </summary>
        public NmsTemplateTests()
        {
            this.PopulateProtectedVariables = true;
        }

#if NETFRAMEWORK
        [Test]
        public void ConnectionThrowException()
        {
            var cf = new Apache.NMS.ActiveMQ.ConnectionFactory();
            cf.BrokerUri = new Uri("tcp://localaaahost:61616");
            Assert.Throws<NMSConnectionException>(() => cf.CreateConnection());
        }
#endif

        [Test]
        public void ConvertAndSend()
        {
            Assert.NotNull(connectionFactory);
            Assert.NotNull(nmsTemplate);

            string msgText = "Hello World";

            //Use with destination set at runtime
            nmsTemplate.ConvertAndSend("APP.TESTING", msgText);

            AssertRecievedHelloWorldMessage(msgText, nmsTemplate.ReceiveAndConvert("APP.TESTING"));

            //Now using default destination set via property
            nmsTemplate.DefaultDestinationName = "APP.TESTING";
            nmsTemplate.ConvertAndSend(msgText);
            AssertRecievedHelloWorldMessage(msgText, nmsTemplate.ReceiveAndConvert());
        }


        private void AssertRecievedHelloWorldMessage(string msgText, object message)
        {
            Assert.NotNull(message);
            string text = message as string;
            Assert.NotNull(text);
            Assert.AreEqual(msgText, text);
        }

        #region Overrides of AbstractDependencyInjectionSpringContextTests

        /// <summary>
        /// Subclasses must implement this property to return the locations of their
        /// config files. A plain path will be treated as a file system location.
        /// </summary>
        /// <value>An array of config locations</value>
        protected override string[] ConfigLocations
        {
            get { return new string[] {"assembly://Spring.Messaging.Nms.Integration.Tests/Spring.Messaging.Nms.Core/NmsTemplateTests.xml"}; }
        }

        #endregion
    }
}
