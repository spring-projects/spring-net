#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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

using System.Collections;
using NUnit.Framework;

using Spring.Messaging.Ems.Common;
using Spring.Testing.NUnit;

#endregion

namespace Spring.Messaging.Ems.Core
{
    [TestFixture]
    public class EmsTemplateTests : AbstractDependencyInjectionSpringContextTests
    {
        protected IConnectionFactory emsConnectionFactory;

        protected IConnectionFactory connectionFactory;

        //This is the 'raw' TIBCO type
        protected ConnectionFactory jndiEmsConnectionFactory;

        protected IConnectionFactory cachingJndiConnectionFactory;

        protected EmsTemplate emsTemplate;

        protected SimpleGateway simpleGateway;


        private Admin admin;

        private string queueName = "INT_TEST_QUEUE";

        private Hashtable env = new Hashtable();

        private LookupContext lookupContext;

        /// <summary>
        /// Default constructor for EmsTemplateTests.
        /// </summary>
        public EmsTemplateTests()
        {
            this.PopulateProtectedVariables = true;

            env.Add(LookupContext.PROVIDER_URL, "tibjmsnaming://localhost:7222");
            env.Add(LookupContext.SECURITY_PRINCIPAL, "admin");
            env.Add(LookupContext.SECURITY_CREDENTIALS, "");
            lookupContext = new LookupContext(env);
        }

        protected override void OnSetUp()
        {
            admin = new Admin("tcp://localhost:7222", "admin", "");
            Destination destination = null;
            try
            {
                destination = (Destination) lookupContext.Lookup(queueName);
                if (destination != null) admin.DestroyQueue(queueName);
            } catch (NameNotFoundException)
            {}                        
            admin.CreateQueue(new QueueInfo(queueName));
            admin.BindQueue(queueName, queueName);
            admin.PurgeQueue(queueName);
        }

     


        [Test]
        public void ConvertAndSend()
        {
            Assert.NotNull(emsConnectionFactory);
            Assert.NotNull(connectionFactory);
            Assert.NotNull(jndiEmsConnectionFactory);       
            Assert.NotNull(cachingJndiConnectionFactory);
            Assert.NotNull(emsTemplate);

            string msgText = "Hello World";

            //Use with destination set at runtime
            emsTemplate.ConvertAndSend("APP.TESTING", msgText);
            AssertRecievedHelloWorldMessage(msgText, emsTemplate.ReceiveAndConvert("APP.TESTING"));

            //Now using default destination set via property
            emsTemplate.DefaultDestinationName = "APP.TESTING";
            emsTemplate.ConvertAndSend(msgText);
            AssertRecievedHelloWorldMessage(msgText, emsTemplate.ReceiveAndConvert());
            
            //Now using destination oject
            Destination destination = (Destination)lookupContext.Lookup(queueName);

            emsTemplate.ConvertAndSend(destination, msgText);
            AssertRecievedHelloWorldMessage(msgText, emsTemplate.ReceiveAndConvert(destination));
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
            get { return new string[] { "assembly://Spring.Messaging.Ems.Integration.Tests/Spring.Messaging.Ems.Core/EmsTemplateTests.xml" }; }
        }

        #endregion
    }
}