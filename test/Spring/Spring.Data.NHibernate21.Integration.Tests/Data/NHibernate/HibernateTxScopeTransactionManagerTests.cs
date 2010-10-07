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
        #region Fields
        private IDbProvider dbProvider;

        private IPlatformTransactionManager transactionManager;

        private IApplicationContext ctx;
        #endregion

        #region Constants

        /// <summary>
        /// The shared <see cref="log4net.ILog"/> instance for this class (and derived classes). 
        /// </summary>
        protected static readonly ILog log =
            LogManager.GetLogger(typeof(TemplateTests));

        //// force Spring.Data.NHibernate to be preloaded by runtime
        //private Type TLocalSessionFactoryObject = typeof(LocalSessionFactoryObject);

        #endregion

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

        private static void ExecuteSql(IDbConnection conn, string sql)
        {
            IDbCommand cmd;
            cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
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
    }
}
