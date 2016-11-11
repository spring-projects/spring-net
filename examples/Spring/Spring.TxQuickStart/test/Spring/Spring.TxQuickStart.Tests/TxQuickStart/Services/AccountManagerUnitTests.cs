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
using NUnit.Framework;
using Spring.TxQuickStart.Dao;

#endregion

namespace Spring.TxQuickStart.Services
{
    /// <summary>
    /// This class contains unit tests for IAccountManager
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id: AccountManagerUnitTests.cs,v 1.1 2007/12/07 02:36:20 markpollack Exp $</version>
    [TestFixture]
    public class AccountManagerUnitTests
    {
        private IAccountManager accountManager;

        [SetUp]
        public void Setup()
        {
            IAccountCreditDao stubCreditDao = new StubAccountCreditDao();
            IAccountDebitDao stubDebitDao = new StubAccountDebitDao();
            accountManager = new AccountManager(stubCreditDao, stubDebitDao);            
        }

        [Test]
        public void TransferBelowMaxAmount()
        {
            accountManager.DoTransfer(217, 217);
        }

        [Test]
        public void TransferAboveMaxAmount()
        {
            Assert.Throws<ArithmeticException>(() => accountManager.DoTransfer(2000000, 200000));
        }       
    }
}