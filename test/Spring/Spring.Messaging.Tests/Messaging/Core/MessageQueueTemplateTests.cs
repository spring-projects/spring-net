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
using System.Messaging;
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
    /// This class contains tests for MessageQueueTemplate
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class MessageQueueTemplateTests : AbstractDependencyInjectionSpringContextTests
    {
        [SetUp]
        public override void SetUp()
        {
            RecreateMessageQueue(@".\Private$\testqueue", false);
            RecreateMessageQueue(@".\Private$\testtxqueue", true);
            base.SetUp();
        }

        private void RecreateMessageQueue(string path, bool transactional)
        {
            bool defaultCacheEnabled = MessageQueue.EnableConnectionCache;
            MessageQueue.ClearConnectionCache();
            MessageQueue.EnableConnectionCache = false;
            if (MessageQueue.Exists(path))
            {
                MessageQueue queue;
// TODO (EE): delete/create doesn't work for some reason
//                MessageQueue.Delete(path);
//                queue = MessageQueue.Create(path, transactional);
                queue = new MessageQueue(path);
                queue.Purge();
                queue.Dispose();
            }
            else
            {
                MessageQueue.Create(path, transactional).Dispose();
            }
            MessageQueue.ClearConnectionCache();
            MessageQueue.EnableConnectionCache = defaultCacheEnabled; // set to default
        }

#if NET_2_0 || NET_3_0
        [Test]
        public void MessageCreator()
        {
            MessageQueueTemplate mqt = applicationContext["txqueue"] as MessageQueueTemplate;
            Assert.IsNotNull(mqt);        
            string path = @".\Private$\mlptestqueue";
            if (MessageQueue.Exists(path))
            {
                MessageQueue.Delete(path);
            }
            MessageQueue.Create(path, true);
            mqt.MessageQueueFactory.RegisterMessageQueue("newQueueDefinition", delegate
                                                                               {
                                                                                   MessageQueue mq = new MessageQueue();
                                                                                   mq.Path = path;                                                                                                      
                                                                                   // other properties
                                                                                   return mq;
                                                                               });

            Assert.IsTrue(mqt.MessageQueueFactory.ContainsMessageQueue("newQueueDefinition"));

            SendAndReceive("newQueueDefinition",mqt);

            SimpleCreator sc  = new SimpleCreator();
            mqt.MessageQueueFactory.RegisterMessageQueue("fooQueueDefinition", sc.CreateQueue );

        }


#endif
        public class SimpleCreator
        {
            public MessageQueue CreateQueue()
            {
                return new MessageQueue();
            }
        }
  
        
  
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void NoMessageQueueNameSpecified()
        {
            MessageQueueTemplate mqt = new MessageQueueTemplate();
            mqt.AfterPropertiesSet();
        }

        [Test, Ignore("obsolete test, defaultMessageQueueName is used to obtain a queue from MessageQueueFactory")]
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

        #region Integration Tests - to be moved to another test assembly

        [Test]
        public void SendAndReceiveNonTransactional()
        {
            MessageQueueTemplate q = applicationContext["queue"] as MessageQueueTemplate;
            Assert.IsNotNull(q);
            q.ConvertAndSend("Hello World 1");
            ReceiveHelloWorld(null,q,1);
        }

        [Test]
        public void SendAndReceiveNonTransactionalRemotePrivateQueue()
        {
            MessageQueueTemplate q = applicationContext["queueTemplate-remote"] as MessageQueueTemplate;
            Assert.IsNotNull(q);
            q.ConvertAndSend("Hello World 1");
            //ReceiveHelloWorld(null, q, 1);
        }

        private static void ReceiveHelloWorld(string messageQueueObjectName, MessageQueueTemplate q, int index)
        {
            object o = null;
            if (messageQueueObjectName == null)
            {
                o = q.ReceiveAndConvert();
            } else
            {
                o = q.ReceiveAndConvert(messageQueueObjectName);
            }
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
            SendAndReceive(q);
        }

        [Test]
        public void SendTxMessageQueueUsingMessageTx()
        {
            MessageQueueTemplate q = applicationContext["txqueue"] as MessageQueueTemplate;
            Assert.IsNotNull(q);
            SendAndReceive(q);
        }

        [Test]
        public void SendTxMessageQueueUsingTxScope()
        {
            MessageQueueTemplate q = applicationContext["txqueue"] as MessageQueueTemplate;
            Assert.IsNotNull(q);
            SendUsingMessageTxScope(q);
            Receive(null,q);
        }


        private static void SendAndReceive(MessageQueueTemplate q)
        {
            SendAndReceive(null, q);
        }

        private static void SendAndReceive(string messageQueueObjectName, MessageQueueTemplate q)
        {
            SendUsingMessageTx(messageQueueObjectName, q);
            Receive(messageQueueObjectName, q);
        }

        private static void Receive(string messageQueueObjectName, MessageQueueTemplate q)
        {
            ReceiveHelloWorld(messageQueueObjectName, q, 1);
            ReceiveHelloWorld(messageQueueObjectName, q, 2);
            ReceiveHelloWorld(messageQueueObjectName, q, 3);
        }
#if NET_2_0
        private static void SendUsingMessageTx(string messageQueueObjectName, MessageQueueTemplate q)
        {
            IPlatformTransactionManager txManager = new MessageQueueTransactionManager();
            TransactionTemplate transactionTemplate = new TransactionTemplate(txManager);
            transactionTemplate.Execute(delegate(ITransactionStatus status)
                                            {
                                                if (messageQueueObjectName == null)
                                                {
                                                    q.ConvertAndSend("Hello World 1");
                                                    q.ConvertAndSend("Hello World 2");
                                                    q.ConvertAndSend("Hello World 3");
                                                } else
                                                {
                                                    q.ConvertAndSend(messageQueueObjectName, "Hello World 1");
                                                    q.ConvertAndSend(messageQueueObjectName, "Hello World 2");
                                                    q.ConvertAndSend(messageQueueObjectName, "Hello World 3");
                                                }
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
#endif
        #endregion

        protected override string[] ConfigLocations
        {
            get { return new string[] { "assembly://Spring.Messaging.Tests/Spring.Messaging.Core/MessageQueueTemplateTests.xml" }; }
        }


        #region Some simple driver code for debugging
        public void SimpleRemoteConsumption()
        {
//            string connectionWorking = @"FormatName:Direct=OS:MARKT60\Private$\testqueue";

            //TCP:IP doesn't work...
            MessageQueue rmQ = new MessageQueue(@"FormatName:Direct=TCP:192.168.1.105\Private$\testqueue");

            rmQ.Send("Hello Simple");

            rmQ.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

            Message msg = rmQ.Receive();

            Assert.IsNotNull(msg);


        }

        public void GetAllFromQueue()
        {
            MessageQueueTemplate q = applicationContext["queue"] as MessageQueueTemplate;
            Assert.IsNotNull(q);
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(q.ReceiveAndConvert());
            }
        }
        #endregion

    }
}