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

#region Imports

using System;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using NUnit.Framework;
using Spring.Data.Common;
using Spring.Data.Core;
using Spring.Transaction;
using Spring.Transaction.Support;

#endregion

namespace Spring.Data
{
    /// <summary>
    /// This class contains tests for nesting transaction scopes.
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class NestedTxScopeTests
    {
        


        [Test]
        public void TxTemplate()
        {
            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            dbProvider.ConnectionString = @"Data Source=SPRINGQA;Initial Catalog=CreditsAndDebits;User ID=springqa; Password=springqa";
            //IPlatformTransactionManager tm = new ServiceDomainPlatformTransactionManager();
            //IPlatformTransactionManager tm = new TxScopeTransactionManager();
            IPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            AdoTemplate adoTemplate = new AdoTemplate(dbProvider);

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.Required;
            tt.Execute(status =>
                           {
                               if (System.Transactions.Transaction.Current != null) Console.WriteLine("tx 1 id = " + System.Transactions.Transaction.Current.TransactionInformation.LocalIdentifier);
                               Console.WriteLine("tx 1 'IsNewTransaction' = " + status.IsNewTransaction);
                               adoTemplate.ExecuteNonQuery(CommandType.Text, "insert into Credits (CreditAmount) VALUES (@amount)", "amount", DbType.Decimal, 0,444);
                               TransactionTemplate tt2 = new TransactionTemplate(tm);
                               tt2.PropagationBehavior = TransactionPropagation.RequiresNew;
                               
                               tt2.Execute(status2 =>
                                               {
                                                   if (System.Transactions.Transaction.Current != null) Console.WriteLine("tx 2 = " + System.Transactions.Transaction.Current.TransactionInformation.LocalIdentifier);
                                                   Console.WriteLine("tx 2 'IsNewTransaction' = " + status2.IsNewTransaction);
                                                   adoTemplate.ExecuteNonQuery(CommandType.Text, "insert into dbo.Debits (DebitAmount) VALUES (@amount)", "amount", DbType.Decimal, 0,555);
                                                   //throw new ArithmeticException("can't do the math.");
                                                   status2.SetRollbackOnly();
                                                   return null;
                                               });
                               
                               if (System.Transactions.Transaction.Current != null) Console.WriteLine("tx id1 = " + System.Transactions.Transaction.Current.TransactionInformation.LocalIdentifier);                               
                               return null;
                           });
        }

        [Test]
        public void TxScope()
        {
            Method1();
        }
        private void Method1()
        {
            string updateSql1 = "insert into Credits (CreditAmount) VALUES (333)";
            using (TransactionScope ts =
                new TransactionScope(TransactionScopeOption.Required))
            {
                Console.WriteLine("tx id1 = " +
                  System.Transactions.Transaction.Current.TransactionInformation.LocalIdentifier);
                using (SqlConnection cn2005 = new SqlConnection())
                {
                    cn2005.ConnectionString =
                        @"Data Source=SPRINGQA;Initial Catalog=CreditsAndDebits;User ID=springqa; Password=springqa";
                    SqlCommand cmd = new SqlCommand(updateSql1, cn2005);
                    cn2005.Open();
                    cmd.ExecuteNonQuery();
                }
                Method2();
                Console.WriteLine("tx id1 = " +
                  System.Transactions.Transaction.Current.TransactionInformation.LocalIdentifier);
                ts.Complete();
            }
        }

        private void Method2()
        {
            bool rollback = true;
            string updateSql2 = "insert into Debits (DebitAmount) VALUES (222)";
            using (TransactionScope ts =
                new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                Console.WriteLine("tx id2 = " +
  System.Transactions.Transaction.Current.TransactionInformation.LocalIdentifier);
                using (SqlConnection cn2005 = new SqlConnection())
                {
                    cn2005.ConnectionString = @"Data Source=SPRINGQA;Initial Catalog=CreditsAndDebits;User ID=springqa; Password=springqa";
                    SqlCommand cmd = new SqlCommand(updateSql2, cn2005);
                    cn2005.Open();
                    cmd.ExecuteNonQuery();
                }
                if (rollback)
                {
                    System.Transactions.Transaction.Current.Rollback();
                }
                else
                {
                    ts.Complete(); 
                }
                Console.WriteLine("end of 2nd data access operation");
                
            }
        }

        [Test]
        public void UnwantedPromotion()
        {
            using (TransactionScope ts = new TransactionScope())
            {
                InnerMethod();
                InnerMethod();
                ts.Complete();
            }
        }

        private void InnerMethod()
        {
            string updateSql2 = "insert into Debits (DebitAmount) VALUES (222)";

            using (SqlConnection cn2005 = new SqlConnection())
            {
                cn2005.ConnectionString = @"Data Source=SPRINGQA;Initial Catalog=CreditsAndDebits;User ID=springqa; Password=springqa";
                SqlCommand cmd = new SqlCommand(updateSql2, cn2005);
                cn2005.Open();
                cmd.ExecuteNonQuery();

                #region logging
                Console.WriteLine("TransactionSynchronizationManager.CurrentTransactionIsolationLevel = " +
                  TransactionSynchronizationManager.CurrentTransactionIsolationLevel);
                Console.WriteLine("System.Transactions.Transaction.Current.IsolationLevel = " + System.Transactions.Transaction.Current.IsolationLevel);
                string name = TransactionSynchronizationManager.CurrentTransactionName;
                bool read = TransactionSynchronizationManager.CurrentTransactionReadOnly;
                #endregion
            }
        }
    }
}
