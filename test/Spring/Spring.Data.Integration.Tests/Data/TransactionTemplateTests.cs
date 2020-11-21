#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.Common;
using Spring.Data.Core;
using Spring.Objects;
using Spring.Transaction;
using Spring.Transaction.Support;

#endregion

namespace Spring.Data
{
    /// <summary>
    /// Integration tests for transaction template functionality 
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class TransactionTemplateTests
    {

        private IDbProvider dbProvider;

        private IPlatformTransactionManager transactionManager;

        private IApplicationContext ctx;

        private IAdoOperations adoOperations;

        [SetUp]
        public void SetUp()
        {
            ctx =
                new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/templateTests.xml");
            dbProvider = ctx["DbProvider"] as IDbProvider;
            transactionManager = ctx["transactionManager"] as IPlatformTransactionManager;
            adoOperations = ctx["adoTemplate"] as IAdoOperations;

            ITestObjectManager testObjectManager = ctx["testObjectManager"] as ITestObjectManager;
            testObjectManager.DeleteAllTestObjects();
            testObjectManager.SaveTwoTestObjects(new TestObject("Jack", 10), new TestObject("Jill", 20));
        }

        [TearDown]
        public void TearDown()
        {
            ITestObjectManager testObjectManager = ctx["testObjectManager"] as ITestObjectManager;
            testObjectManager.DeleteTwoTestObjects("Jack", "Jill");
            testObjectManager.DeleteAllTestObjects();
        }


        /// <summary>
        /// Test using ObjectNameAutoProxyCreator for declarative tx mgmt.
        /// </summary>
        /// <remarks>Note asserts to not actually check if same tx is used
        /// for the multiple save and delete operations in TestObjectManager.
        /// Useful for stepping through w/ debugger.
        /// </remarks>
        [Test]
        public void DeclarativeViaAutoProxyCreator()
        {
            ITestObjectManager mgr = ctx["testObjectManager"] as ITestObjectManager;
            TestObjectDao dao = (TestObjectDao)ctx["testObjectDao"];
            PerformOperations(mgr, dao);
        }

        /// <summary>
        /// Test using TransactionProxyFactoryObject for declarative tx mgmt.
        /// </summary>
        /// <remarks>Note asserts to not actually check if same tx is used
        /// for the multiple save and delete operations in TestObjectManager.
        /// Useful for stepping through w/ debugger.
        /// </remarks>
        [Test]
        public void DeclarativeViaTransactionProxyFactoryObject()
        {
            ITestObjectManager mgr = ctx["testObjectManagerTP"] as ITestObjectManager;
            ITestObjectDao dao = (ITestObjectDao)ctx["testObjectDao"];
            PerformOperations(mgr, dao);
        }
        /// <summary>
        /// Test using ProxyFactory with a transaction interceptor for declarative tx mgmt. 
        /// </summary>
        /// <remarks>Note asserts to not actually check if same tx is used
        /// for the multiple save and delete operations in TestObjectManager.
        /// Useful for stepping through w/ debugger.</remarks>
        [Test]
        public void DeclarativeViaProxyFactoryObject()
        {
            ITestObjectManager mgr = ctx["testObjectManagerPF"] as ITestObjectManager;
            TestObjectDao dao = (TestObjectDao)ctx["testObjectDao"];
            PerformOperations(mgr, dao);

        }

        public static void PerformOperations(ITestCoordinator coordinator, ITestObjectDao dao)
        {
            TestObject to1 = new TestObject();
            to1.Name = "Jack";
            to1.Age = 7;
            TestObject to2 = new TestObject();
            to2.Name = "Jill";
            to2.Age = 8;

            coordinator.WorkOn(to1, to2);

            coordinator.TestObjectManager.DeleteTwoTestObjects("Jack", "Jill");
        }

        public static void PerformOperations(ITestObjectManager mgr,
                                       ITestObjectDao dao)
        {
            Assert.IsNotNull(mgr);
            TestObject to1 = new TestObject();
            to1.Name = "Jack";
            to1.Age = 7;
            TestObject to2 = new TestObject();
            to2.Name = "Jill";
            to2.Age = 8;
            mgr.SaveTwoTestObjects(to1, to2);

            TestObject to = dao.FindByName("Jack");
            Assert.IsNotNull(to);

            to = dao.FindByName("Jill");
            Assert.IsNotNull(to);
            Assert.AreEqual("Jill", to.Name);

            mgr.DeleteTwoTestObjects("Jack", "Jill");

            to = dao.FindByName("Jack");
            Assert.IsNull(to);

            to = dao.FindByName("Jill");
            Assert.IsNull(to);
        }

        [Test]
        public void ExecuteTemplate()
        {
            TransactionTemplate tt = new TransactionTemplate(transactionManager);
            object result = tt.Execute(new SimpleTransactionCallback(dbProvider));
            Assert.AreEqual(2, (int)result);
        }

        [Test]
        public void ExecuteTransactionManager()
        {
            DefaultTransactionDefinition def = new DefaultTransactionDefinition();
            def.PropagationBehavior = TransactionPropagation.Required;

            ITransactionStatus status = transactionManager.GetTransaction(def);

            int iCount = 0;
            try
            {
                iCount = (int)adoOperations.ExecuteScalar(CommandType.Text, "SELECT COUNT(*) FROM TestObjects");
                /*
                IAdoCommand cmd = new AdoCommand(dbProvider, CommandType.Text);
                cmd.CommandText = "SELECT COUNT(*) FROM TestObjects";
                iCount = (int)cmd.ExecuteScalar();
                */

                //other AdoCommands can be executed within same tx.
            }
            catch (Exception)
            {
                transactionManager.Rollback(status);
                throw;
            }
            transactionManager.Commit(status);
            Assert.AreEqual(2, iCount);

        }


        private class SimpleTransactionCallback : ITransactionCallback
        {
            private IDbProvider dbProvider;

            public SimpleTransactionCallback(IDbProvider dbp)
            {
                dbProvider = dbp;
            }
            /// <summary>
            /// Gets called by TransactionTemplate.Execute within a 
            /// transaction context.
            /// </summary>
            /// <param name="status">The associated transaction status.</param>
            /// <returns>a result object or <c>null</c></returns>
            public object DoInTransaction(ITransactionStatus status)
            {
                AdoTemplate adoTemplate = new AdoTemplate(dbProvider);
                return adoTemplate.Execute(new TestCommandCallback());
            }
        }

        private class TestCommandCallback : ICommandCallback
        {

            public Object DoInCommand(IDbCommand cmd)
            {
                cmd.CommandText = "SELECT COUNT(*) FROM TestObjects";
                int count = (int)cmd.ExecuteScalar();

                cmd.CommandText = "SELECT COUNT(*) FROM TestObjects";
                count = (int)cmd.ExecuteScalar();

                return count;

            }
        }
    }
}
