#region License

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

#endregion

#region Imports

using System.Collections.Generic;
using Apache.NMS;

using FakeItEasy;

using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Messaging.Nms.Core;
using Spring.Messaging.Nms.Connections;
using Spring.Messaging.Nms.Listener;
using Spring.Objects;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Messaging.Nms.Config
{
    /// <summary>
    /// This class contains tests for 
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id:$</version>
    [TestFixture]
    public class NmsNamespaceHandlerTests
    {
        private static string DEFAULT_CONNECTION_FACTORY = "ConnectionFactory";
        private static string EXPLICIT_CONNECTION_FACTORY = "testConnectionFactory";
        private IApplicationContext ctx;

        [SetUp]
        public void Setup()
        {
            // WELLKNOWN
            //NamespaceParserRegistry.RegisterParser(typeof(NmsNamespaceParser));
            ctx = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("NmsNamespaceHandlerTests.xml", GetType()));
        }

        [Test]
        public void Registered()
        {
            Assert.IsNotNull(NamespaceParserRegistry.GetParser("http://www.springframework.net/nms"));
        }

        [Test]
        public void ObjectsCreated()
        {
            var containers = ctx.GetObjects<SimpleMessageListenerContainer>();
            Assert.AreEqual(3, containers.Count);
        }

        [Test]
        public void ContainerConfiguration()
        {
            var containers = ctx.GetObjects<SimpleMessageListenerContainer>();
            IConnectionFactory defaultConnectionFactory = (IConnectionFactory) ctx.GetObject(DEFAULT_CONNECTION_FACTORY);
            IConnectionFactory explicitConnectionFactory = (IConnectionFactory) ctx.GetObject(EXPLICIT_CONNECTION_FACTORY);
            

            int defaultConnectionFactoryCount = 0;
		    int explicitConnectionFactoryCount = 0;
            foreach (KeyValuePair<string, SimpleMessageListenerContainer> dictionaryEntry in containers)
            {
                SimpleMessageListenerContainer container = dictionaryEntry.Value;
                if (container.ConnectionFactory.Equals(defaultConnectionFactory))
                {
                    defaultConnectionFactoryCount++;
                }
                else if (container.ConnectionFactory.Equals(explicitConnectionFactory))
                {
                    explicitConnectionFactoryCount++;
                }
            }

            Assert.AreEqual(1, defaultConnectionFactoryCount, "1 container should have the default connectionFactory");
            Assert.AreEqual(2, explicitConnectionFactoryCount, "2 containers should have the explicit connectionFactory");

        }

        [Test]
        public void Listeners()
        {
            TestObject testObject1 = (TestObject) ctx.GetObject("testObject1");
            TestObject testObject2 = (TestObject) ctx.GetObject("testObject2");
            TestMessageListener testObject3 = (TestMessageListener) ctx.GetObject("testObject3");

            Assert.IsNull(testObject1.Name);
            Assert.IsNull(testObject2.Name);
            Assert.IsNull(testObject3.Message);


            ITextMessage message1 = A.Fake<ITextMessage>();
            A.CallTo(() => message1.Text).Returns("Test1");

            IMessageListener listener1 = GetListener("listener1");
            listener1.OnMessage(message1);
            Assert.AreEqual("Test1", testObject1.Name);


            ITextMessage message2 = A.Fake<ITextMessage>();
            A.CallTo(() => message2.Text).Returns("Test1");

            IMessageListener listener2 = GetListener("listener2");
            listener2.OnMessage(message2);
            A.CallTo(() => message2.Text).MustHaveHappened();

            ITextMessage message3 = A.Fake<ITextMessage>();

            //Default naming strategy is to use full type name + # + number
            string className = typeof(SimpleMessageListenerContainer).FullName;
            string targetName = className + ObjectDefinitionReaderUtils.GENERATED_OBJECT_NAME_SEPARATOR + "0";
            IMessageListener listener3 = GetListener(targetName);
            listener3.OnMessage(message3);
            Assert.AreSame(message3, testObject3.Message);
        }

        private IMessageListener GetListener(string containerObjectName)
        {
            SimpleMessageListenerContainer container =
                (SimpleMessageListenerContainer) ctx.GetObject(containerObjectName);
            return (IMessageListener) container.MessageListener;
        }
    }
}