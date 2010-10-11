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

using System;
using System.Data;
using log4net;
using log4net.Config;
using NHibernate;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.Common;
using Spring.Data.Support;
using Spring.Transaction;
using Spring.Transaction.Support;
using Spring.Transaction.Interceptor;
using System.Transactions;
using System.Collections.Generic;

#endregion

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// Use of Hibernate Template against database.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    [TestFixture]
    public class HibernateTxScopeTransactionManagerTests
    {
        /// <summary>
        /// The shared <see cref="log4net.ILog"/> instance for this class (and derived classes). 
        /// </summary>
        protected static readonly ILog log =
            LogManager.GetLogger(typeof(HibernateTxScopeTransactionManagerTests));

        private IApplicationContext ctx;

        private IDbProvider dbProvider;

        private IPlatformTransactionManager transactionManager;

        [Test]
        public void CanProperlyReleaseConnectionsWhenTransactionsAreRolledBack()
        {
            for (int i = 0; i < 200; i++)
            {
                try
                {
                    DoSave(true);
                }
                catch (Exception)
                {
                }
            }
        }

        [Test]
        [Ignore("Test harness left in place to demonstrate the leaking connection issue present in NH2.1 and later")]
        public void DoesNotLeakConnection()
        {
            DoPreventConnectionLeaksPreventionPattern();
        }

        [Test]
        [Ignore("Test harness left in place to demonstrate the leaking connection issue present in NH2.1 and later")]
        public void LeaksConnection()
        {
            DoConnectionLeakingAntiPattern();
        }

        [SetUp]
        public void SetUp()
        {
            //NamespaceParserRegistry.RegisterParser(typeof(DatabaseNamespaceParser));
            BasicConfigurator.Configure();
            string assemblyName = GetType().Assembly.GetName().Name;
            ctx = new XmlApplicationContext("assembly://" + assemblyName + "/Spring.Data.NHibernate/HibernateTxScopeTransactionManagerTests.xml");

            dbProvider = ctx["DbProvider"] as IDbProvider;
            transactionManager = ctx["transactionManager"] as IPlatformTransactionManager;
            CleanupDatabase(dbProvider.CreateConnection());
        }

        private static void CleanupDatabase(IDbConnection conn)
        {
            conn.Open();
            using (conn)
            {
                ExecuteSql(conn, "delete credits");
                ExecuteSql(conn, "delete debits");
                ExecuteSql(conn, "delete TestObjects");
                ExecuteSql(conn, "insert TestObjects(Age,Name) Values(5, 'Gabriel')");
            }
        }

        /// <summary>
        /// this method will fail the test as it follows the anti-pattern of failing to use the (mandatory) NH Transaction to wrap the session call
        /// </summary>
        private void DoConnectionLeakingAntiPattern()
        {
            //this counter must be larger than the 'Max Pool Size' setting in the connection string for this test to demonstrate the issue
            int counter = 200;

            for (int i = 0; i < counter; i++)
            {

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    using (ISession session = ((ISessionFactory)ctx["SessionFactory"]).OpenSession())
                    {
                        IList<TestObject> to = session.CreateCriteria<TestObject>().List<TestObject>();
                    }

                    //because scope.Complete() is never called, the Transaction is rolled back and this results in the orpahned connections
                }

            }

        }

        /// <summary>
        /// this method demonstrates the proper pattern to follow w NH 2.1 and later, mandating the use of an NH transaction wrapping the session
        /// </summary>
        private void DoPreventConnectionLeaksPreventionPattern()
        {
            //this counter must be larger than the 'Max Pool Size' setting in the connection string for this test to demonstrate the issue
            int counter = 200;

            for (int i = 0; i < counter; i++)
            {

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    using (ISession session = ((ISessionFactory)ctx["SessionFactory"]).OpenSession())
                    {
                        using (ITransaction tx = session.BeginTransaction())
                        {
                            try
                            {
                                IList<TestObject> to = session.CreateCriteria<TestObject>().List<TestObject>();
                                throw new Exception("this exception simulates something going wrong!");

                            }
                            catch (Exception)
                            {
                                tx.Rollback();
                            }
                        }
                    }

                    //because scope.Complete() is never called, the Transaction is rolled back but the presence of the NH transaction and its
                    // associated Rollback() call will enable NH to properly release the connection
                }
            }
        }

        [Transaction]
        private void DoSave(bool simulateException)
        {
            using (TransactionScope tx = new TransactionScope())
            {
                ISession s = ((ISessionFactory)ctx["SessionFactory"]).OpenSession();

                TestObject to = new TestObject();
                to.Name = "George";
                to.Age = 33;

                if (simulateException) { throw new Exception("Simulated Failure in Save Operation."); }

                s.Save(to);

                tx.Complete();
            }

        }

        private static void ExecuteSql(IDbConnection conn, string sql)
        {
            IDbCommand cmd;
            cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

    }
}
