#region License

/*
 * Copyright 2002-2009 the original author or authors.
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

using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Core.IO;
using Spring.Data;
using Spring.Data.Common;
using Spring.Data.Core;
using Spring.Transaction.Support;

namespace Spring.Testing.Ado
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class SimpleAdoTestUtilsTests
    {
        private MockRepository mocks;
        private IAdoOperations adoTemplate;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            adoTemplate = (IAdoOperations) mocks.CreateMock(typeof (IAdoOperations));
        }

        [Test]
        public void ExecuteEmptyScript()
        {
            IResource scriptResource = new StringResource("");

            mocks.ReplayAll();

            SimpleAdoTestUtils.ExecuteSqlScript(adoTemplate, scriptResource, false, SimpleAdoTestUtils.BLOCKDELIM_GO_EXP);
            mocks.VerifyAll();
        }

        [Test]
        public void ExecuteSingleStatement()
        {
            IResource scriptResource = new StringResource("statement 1");

            Expect.Call(adoTemplate.ExecuteNonQuery(CommandType.Text, "statement 1")).Return(0);
            mocks.ReplayAll();

            SimpleAdoTestUtils.ExecuteSqlScript(adoTemplate, scriptResource, false, SimpleAdoTestUtils.BLOCKDELIM_GO_EXP);
            mocks.VerifyAll();
        }

        [Test]
        public void ExecuteScriptWithGOBlocks()
        {
            IResource scriptResource = new StringResource("\tstatement 1 \n\n\t GO\t \n   statement 2\nGO");

            Expect.Call(adoTemplate.ExecuteNonQuery(CommandType.Text, "\tstatement 1 \n")).Return(0);
            Expect.Call(adoTemplate.ExecuteNonQuery(CommandType.Text, "\n   statement 2\n")).Return(0);
            mocks.ReplayAll();

            SimpleAdoTestUtils.ExecuteSqlScript(adoTemplate, scriptResource, false, SimpleAdoTestUtils.BLOCKDELIM_GO_EXP);
            mocks.VerifyAll();
        }

        [Test]
        public void ExecuteScriptWithSemicolonSeparatedStatements()
        {
            IResource scriptResource = new StringResource("\tstatement 1  ;\nGO\n   statement 2;");

            Expect.Call(adoTemplate.ExecuteNonQuery(CommandType.Text, "\tstatement 1  ")).Return(0);
            Expect.Call(adoTemplate.ExecuteNonQuery(CommandType.Text, "\nGO\n   statement 2")).Return(0);
            mocks.ReplayAll();

            SimpleAdoTestUtils.ExecuteSqlScript(adoTemplate, scriptResource, false, SimpleAdoTestUtils.BLOCKDELIM_SEMICOLON_EXP);
            mocks.VerifyAll();
        }

        [Test]
        public void ExecuteScriptTransactedSuccess()
        {
            IDbProvider dbProvider = (IDbProvider) mocks.DynamicMock(typeof(IDbProvider));
            IDbConnection dbConnection = (IDbConnection) mocks.CreateMock(typeof (IDbConnection));
            IDbTransaction dbTx = (IDbTransaction) mocks.CreateMock(typeof (IDbTransaction));
            IDbCommand dbCommand = (IDbCommand) mocks.CreateMock(typeof (IDbCommand));
            DefaultTransactionDefinition txDefinition = new DefaultTransactionDefinition();

            Expect.Call(dbProvider.CreateConnection()).Return(dbConnection);
            dbConnection.Open();
            Expect.Call(dbConnection.BeginTransaction(txDefinition.TransactionIsolationLevel)).Return(dbTx);
            Expect.Call(dbProvider.CreateCommand()).Return(dbCommand);
            dbCommand.Connection = dbConnection;
            dbCommand.Transaction = dbTx;
            dbCommand.CommandText = "simple sql cmd";
            dbCommand.CommandType = CommandType.Text;
            Expect.Call(dbCommand.ExecuteNonQuery()).Return(0);
            dbTx.Commit();
            dbCommand.Dispose();
            dbConnection.Dispose();
            mocks.ReplayAll();

            AdoTemplate adoOps = new AdoTemplate(dbProvider);
            IPlatformTransaction tx = SimpleAdoTestUtils.CreateTransaction(dbProvider, txDefinition);

            SimpleAdoTestUtils.ExecuteSqlScript(adoOps, "simple sql cmd");
            tx.Commit();
            tx.Dispose();            
            mocks.VerifyAll();
        }

        [Test]
        public void ExecuteScriptTransactedRollsbackIfNoCommit()
        {
            IDbProvider dbProvider = (IDbProvider) mocks.CreateMock(typeof(IDbProvider));
            IDbConnection dbConnection = (IDbConnection) mocks.CreateMock(typeof (IDbConnection));
            IDbTransaction dbTx = (IDbTransaction) mocks.CreateMock(typeof (IDbTransaction));
            DefaultTransactionDefinition txDefinition = new DefaultTransactionDefinition();

            Expect.Call(dbProvider.CreateConnection()).Return(dbConnection);
            dbConnection.Open();
            Expect.Call(dbConnection.BeginTransaction(txDefinition.TransactionIsolationLevel)).Return(dbTx);
            dbTx.Rollback();
            dbConnection.Dispose();
            mocks.ReplayAll();

            AdoTemplate adoOps = new AdoTemplate(dbProvider);
            IPlatformTransaction tx = SimpleAdoTestUtils.CreateTransaction(dbProvider, txDefinition);
            tx.Dispose();
            mocks.VerifyAll();
        }
    }
}