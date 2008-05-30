#region License

/*
 * Copyright 2002-2007 the original author or authors.
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

using Spring.Context;
using Spring.Context.Support;

using Spring.Northwind.Service;

#endregion

namespace Spring.Northwind.IntegrationTests
{
    [TestFixture]
    public class FulfillmentServiceTests
    {
        private IApplicationContext ctx;
        
        [SetUp]
        public void SetUp()
        {
            ctx = ContextRegistry.GetContext();
        }

        /// <summary>
        /// The transaction proxy is created by direct
        /// configuration of a TransactionProxyFactoryObject
        /// </summary>
        [Test,Explicit( "choose DeclarativeServicesTxProxyFactoryDriven.xml in Services.xml before running this test" )]
        public void ProcessCustomerViaTxProxyFactoryObject()
        {
            IFulfillmentService s = ctx["FulfillmentServiceUsingTxPFO"] as IFulfillmentService;
            s.ProcessCustomer("BERGS");
            //assertions....
        }
        
        /// <summary>
        /// The transaction proxy is created through use of TransactionAttributeSourceAdvisor
        /// that identified classes/methods that have the [Transaction] attribute
        /// </summary>
        [Test]
        //[Explicit( "choose DeclarativeServicesAttributeDriven.xml in Services.xml before running this test" )]
        public void ProcessCustomer()
        {
            IFulfillmentService s = ctx["FulfillmentService"] as IFulfillmentService;
            s.ProcessCustomer("BERGS");
            //assertions....
        }
    }
}
