#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using Spring.Data.Common;
using Spring.Transaction;

#endregion

namespace Spring.Data
{
	/// <summary>
	/// TODO: 
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	[TestFixture]
	public class DeclarativeTxTests 
	{
        private IDbProvider dbProvider;

        private IPlatformTransactionManager transactionManager;

        private IApplicationContext ctx;

        [SetUp]
        public void SetUp()
        {
            ctx =
                new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/declarativeServices.xml");
            dbProvider = ctx["DbProvider"] as IDbProvider;
            transactionManager = ctx["adoTransactionManager"] as IPlatformTransactionManager;
            
        }

        [Test]
        public void DeclarativeWithAttributes()
        {
            ITestObjectManager mgr = ctx["testObjectManager"] as ITestObjectManager; 
            TestObjectDao dao = (TestObjectDao)ctx["testObjectDao"];
            TransactionTemplateTests.PerformOperations(mgr, dao); 
        }

	}
}
