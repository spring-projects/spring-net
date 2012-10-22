#region Licence

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

using System;
using System.Data.SqlClient;
using System.Transactions;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Objects;

#endregion

namespace Spring.Data
{
    /// <summary>
    /// Simple exercising of the AdoTemplate 
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    [TestFixture]
    public class TestTxIsolationLevelTests
    {
        //investigate
        //1. matching against methods in adotemplate when the pointcut for type should already exclude it?
        //2. is isolaiton level applied correctly
        //   a) TxScope w/ attributes
        //   b) TxScope w/ xml
        //   c) ado  w/ attriutes
        //   d) ado  w/ xml
        private ITestObjectManager testObjectManager;

        [SetUp]
        public void RollbackTestSetup()
        {
            // WELLKNOWN
//            NamespaceParserRegistry.RegisterParser(typeof (DatabaseNamespaceParser));
//            NamespaceParserRegistry.RegisterParser(typeof (TxNamespaceParser));
//            NamespaceParserRegistry.RegisterParser(typeof (AopNamespaceParser));
            IApplicationContext ctx =
                new XmlApplicationContext(
                    "assembly://Spring.Data.Integration.Tests/Spring.Data/TestTxIsolationLevel.xml");
            Assert.IsNotNull(ctx);
            testObjectManager = ctx["testObjectManager"] as ITestObjectManager;
            Assert.IsNotNull(testObjectManager);
        }

        [Test]
        public void DoWork()
        {
            TestObject to1 = new TestObject("joe", 32);
            TestObject to2 = new TestObject("mary", 35);
            testObjectManager.SaveTwoTestObjects(to1, to2);
        }

        [Test]
        public void Dowork2()
        {
            using (TransactionScope txScope = new TransactionScope())
            {
                string updateSql2 = "insert into Debits (DebitAmount) VALUES (222)";
                using (SqlConnection cn2005 = new SqlConnection())
                {
                    cn2005.ConnectionString =
                        @"Data Source=SPRINGQA;Initial Catalog=CreditsAndDebits;User ID=springqa; Password=springqa";
                    SqlCommand cmd = new SqlCommand(updateSql2, cn2005);
                    cn2005.Open();
                    Console.WriteLine("Isolation level = " + System.Transactions.Transaction.Current.IsolationLevel);
                }
            }
        }
    }
}
