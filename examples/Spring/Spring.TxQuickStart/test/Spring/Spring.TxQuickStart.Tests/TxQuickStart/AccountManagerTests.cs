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

using System;
using System.Data;
using NUnit.Framework;
using Spring.Aop.Config;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.Common;
using Spring.Data.Config;
using Spring.Data.Core;
using Spring.Objects.Factory.Xml;
using Spring.Transaction.Config;
using Spring.TxQuickStart.Services;

namespace Spring.TxQuickStart
{
    [TestFixture]
	public class AccountManagerTests 
	{
        private AdoTemplate adoTemplateCredit;
        private AdoTemplate adoTemplateDebit;

        private IAccountManager accountManager;

        [SetUp]
        public void SetUp()
        {
            // Configure Spring programmatically
            NamespaceParserRegistry.RegisterParser(typeof(DatabaseNamespaceParser));
            NamespaceParserRegistry.RegisterParser(typeof(TxNamespaceParser));
            NamespaceParserRegistry.RegisterParser(typeof(AopNamespaceParser));
            IApplicationContext context = CreateContextFromXml();
            accountManager = context["accountManager"] as IAccountManager;  
            CleanDb(context);
        }

        private static IApplicationContext CreateContextFromXml()
        {
            return new XmlApplicationContext(
            // use for demoing ado tx manager
                "assembly://Spring.TxQuickStart.Tests/Spring.TxQuickStart/system-test-local-config.xml" 
            // use for demoing TransactionScope tx manager
//                "assembly://Spring.TxQuickStart.Tests/Spring.TxQuickStart/system-test-dtc-config.xml" 
                );
        }

        [Test]
        public void TransferBelowMaxAmount()
        {
            accountManager.DoTransfer(217, 217);

            //asserts to read from db...

            int numCreditRecords = (int)adoTemplateCredit.ExecuteScalar(CommandType.Text, "select count(*) from Credits");
            int numDebitRecords =  (int)adoTemplateDebit.ExecuteScalar(CommandType.Text, "select count(*) from Debits");
            Assert.AreEqual(1, numCreditRecords);
            Assert.AreEqual(1, numDebitRecords);
        }

        [Test]
        public void TransferAboveMaxAmount()
        {
            try
            {
                accountManager.DoTransfer(2000000, 200000);
                Assert.Fail("Should have thrown Arithmethic Exception");
            } catch (ArithmeticException)
            {
                int numCreditRecords = (int)adoTemplateCredit.ExecuteScalar(CommandType.Text, "select count(*) from Credits");
                int numDebitRecords = (int)adoTemplateDebit.ExecuteScalar(CommandType.Text, "select count(*) from Debits");
                Assert.AreEqual(0, numCreditRecords);
                Assert.AreEqual(0, numDebitRecords);
            }
            
        }


        // Run the following test only if you have changed to the alternate implementation
        // of the method DoTransfer in AccountManager that specifies the NoRollbackFor property.
        [Test]   
        [Ignore("Change impl of AccountManager as shown before running this test")]
        public void TransferAboveMaxAmountNoRollbackFor()
        {
            try
            {
                accountManager.DoTransfer(2000000, 2000000);
                Assert.Fail("Should have thrown Arithmethic Exception");
            } catch (ArithmeticException) {
                int numCreditRecords = (int)adoTemplateCredit.ExecuteScalar(CommandType.Text, "select count(*) from Credits");
                int numDebitRecords = (int)adoTemplateDebit.ExecuteScalar(CommandType.Text, "select count(*) from Debits");
                Assert.AreEqual(1, numCreditRecords);
                Assert.AreEqual(0, numDebitRecords);
            }
        }


        private void CleanDb(IApplicationContext context)
        {
            IDbProvider dbProvider = (IDbProvider)context["DebitDbProvider"];
            adoTemplateDebit = new AdoTemplate(dbProvider);
            adoTemplateDebit.ExecuteNonQuery(CommandType.Text, "truncate table Debits");

            dbProvider = (IDbProvider)context["CreditDbProvider"];
            adoTemplateCredit = new AdoTemplate(dbProvider);
            adoTemplateCredit.ExecuteNonQuery(CommandType.Text, "truncate table Credits");

        }
	}
}
