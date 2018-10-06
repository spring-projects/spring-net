#region License

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


using System;
using Spring.Transaction.Interceptor;

namespace Spring.Data.NHibernate
{
    public class AccountManager : IAccountManager
    {
        
        private IAccountCreditDao creditDao;
        private IAccountDebitDao debitDao;
        private IAuditDao auditDao;

        private bool throwException = false;
        private bool throwExceptionAtEnd = false;

        public AccountManager()
        {
        }

        public IAccountCreditDao AccountCreditDao
        {
            get { return creditDao; }
            set { creditDao = value; }
        }

        public IAccountDebitDao AccountDebitDao
        {
            get { return debitDao; }
            set { debitDao = value; }
        }

        public IAuditDao AuditDao
        {
            get { return auditDao; }
            set { auditDao = value; }
        }


        public bool ThrowException
        {
            get { return throwException; }
            set { throwException = value; }
        }

        public bool ThrowExceptionAtEnd
        {
            get { return throwExceptionAtEnd; }
            set { throwExceptionAtEnd = value; }
        }


        [Transaction()]
        public void DoTransfer(float creditAmount, float debitAmount)
        {
            creditDao.CreateCredit(creditAmount);
            if (ThrowException)
            {
                throw new ArithmeticException("Couldn't do the math....");
            }
            debitDao.DebitAccount(debitAmount);
            if (AuditDao != null)
            {
                AuditDao.AuditOperation(DateTime.Now.ToString());
            }
            if (ThrowExceptionAtEnd)
            {
                throw new ArgumentException("Almost there...but not quite.");
            }
        }

    }


}
