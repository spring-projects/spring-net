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

using System;
using System.Transactions;
using NUnit.Framework;
using Spring.Data.Core;
using Spring.Messaging.Support.Converters;
using Spring.Testing.NUnit;
using Spring.Threading;
using Spring.Transaction;
using Spring.Transaction.Support;
using Spring.Util;

#endregion

namespace Spring.Messaging.Core
{
    /// <summary>
    /// This class contains tests for 
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id:$</version>
    [TestFixture]
    public class MessageQueueTemplateTests : AbstractDependencyInjectionSpringContextTests
    {
       [Test]
        [ExpectedException(typeof (ArgumentException), ExpectedMessage = "DefaultMessageQueueObjectName is required.")]
        public void NoMessageQueueNameSpecified()
        {
            MessageQueueTemplate mqt = new MessageQueueTemplate();
            mqt.AfterPropertiesSet();
        }

        [Test]
        [ExpectedException(typeof (ArgumentException),
            ExpectedMessage = "No object named noqueuename is defined in the Spring container")]
        public void MessageQueueNameNotInContext()
        {
            MessageQueueTemplate q = new MessageQueueTemplate("noqueuename");
            q.ApplicationContext = applicationContext;
            q.AfterPropertiesSet();
        }

        [Test]
        public void MessageQueueCreatedinThreadLocalStorage()
        {
            MessageQueueTemplate q = applicationContext["queue"] as MessageQueueTemplate;
            Assert.IsNotNull(q);
            Assert.AreEqual(q.DefaultMessageQueue, q.MessageQueueFactory.CreateMessageQueue(q.DefaultMessageQueueObjectName));        
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException),
            ExpectedMessage = "No MessageConverter registered. Check configuration of MessageQueueTemplate.")]
        public void MessageConverterNotRegistered()
        {
            MessageQueueTemplate q = applicationContext["queue-noconverter"] as MessageQueueTemplate;
            Assert.IsNotNull(q);
            IMessageConverter c = q.MessageConverter;
        }

        #region Integration Tests - to be moved to another test assembly

        [Test]
        public void SendAndReceiveNonTransactional()
        {
            MessageQueueTemplate q = applicationContext["queue"] as MessageQueueTemplate;
            Assert.IsNotNull(q);
            ReceiveHelloWorld(q,1);
        }

        private static void ReceiveHelloWorld(MessageQueueTemplate q, int index)
        {
            object o = q.ReceiveAndConvert();
            Assert.IsNotNull(o);
            string data = o as string;
            Assert.IsNotNull(data);
            Assert.AreEqual("Hello World " + index, data);
        }

        [Test]
        public void SendNonTxMessageQueueUsingMessageTx()
        {
            MessageQueueTemplate q = applicationContext["queue"] as MessageQueueTemplate;
            Assert.IsNotNull(q);
            SendAndRecieve(q);
        }

        [Test]
        public void SendTxMessageQueueUsingMessageTx()
        {
            MessageQueueTemplate q = applicationContext["txqueue"] as MessageQueueTemplate;
            Assert.IsNotNull(q);
            SendAndRecieve(q);
        }

        [Test]
        public void SendTxMessageQueueUsingTxScope()
        {
            MessageQueueTemplate q = applicationContext["txqueue"] as MessageQueueTemplate;
            Assert.IsNotNull(q);
            SendUsingMessageTxScope(q);
            Receive(q);
        }

        private static void SendAndRecieve(MessageQueueTemplate q)
        {
            SendUsingMessageTx(q);
            Receive(q);
        }

        private static void Receive(MessageQueueTemplate q)
        {
            ReceiveHelloWorld(q,1);
            ReceiveHelloWorld(q,2);
            ReceiveHelloWorld(q,3);
        }

        private static void SendUsingMessageTx(MessageQueueTemplate q)
        {
            IPlatformTransactionManager txManager = new MessageQueueTransactionManager();
            TransactionTemplate transactionTemplate = new TransactionTemplate(txManager);
            transactionTemplate.Execute(delegate(ITransactionStatus status)
                                            {
                                                q.ConvertAndSend("Hello World 1");
                                                q.ConvertAndSend("Hello World 2");
                                                q.ConvertAndSend("Hello World 3");
                                                return null;
                                            });
        }

        private static void SendUsingMessageTxScope(MessageQueueTemplate q)
        {
            IPlatformTransactionManager txManager = new TxScopeTransactionManager();
            TransactionTemplate transactionTemplate = new TransactionTemplate(txManager);
            transactionTemplate.Execute(delegate(ITransactionStatus status)
                                            {
                                                q.ConvertAndSend("Hello World 1");
                                                q.ConvertAndSend("Hello World 2");
                                                q.ConvertAndSend("Hello World 3");
                                                return null;
                                            });
        }

        #endregion

        [Test]
        public void GetAllFromQueue()
        {
            MessageQueueTemplate q = applicationContext["queue"] as MessageQueueTemplate;
            Assert.IsNotNull(q);
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(q.ReceiveAndConvert());
            }
        }
        protected override string[] ConfigLocations
        {
            get { return new string[] {"assembly://Spring.Messaging.Tests/Spring.Messaging.Core/MessageQueueTemplateTests.xml"}; }
        }
    }
}