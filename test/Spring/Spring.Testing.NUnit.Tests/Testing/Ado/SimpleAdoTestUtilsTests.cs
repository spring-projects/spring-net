#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using FakeItEasy;

using NUnit.Framework;

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
        private IAdoOperations adoTemplate;

        [SetUp]
        public void SetUp()
        {
            adoTemplate = A.Fake<IAdoOperations>();
        }

        [Test]
        public void ExecuteEmptyScript()
        {
            IResource scriptResource = new StringResource("");

            SimpleAdoTestUtils.ExecuteSqlScript(adoTemplate, scriptResource, false, SimpleAdoTestUtils.BLOCKDELIM_GO_EXP);
        }

        [Test]
        public void ExecuteSingleStatement()
        {
            IResource scriptResource = new StringResource("statement 1");

            SimpleAdoTestUtils.ExecuteSqlScript(adoTemplate, scriptResource, false, SimpleAdoTestUtils.BLOCKDELIM_GO_EXP);
            A.CallTo(() => adoTemplate.ExecuteNonQuery(CommandType.Text, "statement 1")).MustHaveHappened();
        }

        [Test]
        public void ExecuteScriptWithMissingSeparatorOnLastBlock()
        {
            IResource scriptResource = new StringResource("\tstatement 1 \n\n\t GO\t \n   statement 2");

            SimpleAdoTestUtils.ExecuteSqlScript(adoTemplate, scriptResource, false, SimpleAdoTestUtils.BLOCKDELIM_GO_EXP);

            A.CallTo(() => adoTemplate.ExecuteNonQuery(CommandType.Text, "\tstatement 1 \n")).MustHaveHappened();
            A.CallTo(() => adoTemplate.ExecuteNonQuery(CommandType.Text, "\n   statement 2")). MustHaveHappened();

        }

        [Test]
        public void ExecuteScriptWithGOBlocks()
        {
            IResource scriptResource = new StringResource("\tstatement 1 \n\n\t GO\t \n   statement 2\nGO");

            A.CallTo(() => adoTemplate.ExecuteNonQuery(CommandType.Text, "\tstatement 1 \n")).Returns(0);
            A.CallTo(() => adoTemplate.ExecuteNonQuery(CommandType.Text, "\n   statement 2\n")).Returns(0);

            SimpleAdoTestUtils.ExecuteSqlScript(adoTemplate, scriptResource, false, SimpleAdoTestUtils.BLOCKDELIM_GO_EXP);
        }

        [Test]
        public void ExecuteScriptWithSemicolonSeparatedStatements()
        {
            IResource scriptResource = new StringResource("\tstatement 1  ;\nGO\n   statement 2;");

            A.CallTo(() => adoTemplate.ExecuteNonQuery(CommandType.Text, "\tstatement 1  ")).Returns(0);
            A.CallTo(() => adoTemplate.ExecuteNonQuery(CommandType.Text, "\nGO\n   statement 2")).Returns(0);

            SimpleAdoTestUtils.ExecuteSqlScript(adoTemplate, scriptResource, false, SimpleAdoTestUtils.BLOCKDELIM_SEMICOLON_EXP);
        }

        [Test]
        public void ExecuteScriptTransactedSuccess()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection dbConnection = A.Fake<IDbConnection>();
            IDbTransaction dbTx = A.Fake<IDbTransaction>();
            IDbCommand dbCommand = A.Fake<IDbCommand>();
            DefaultTransactionDefinition txDefinition = new DefaultTransactionDefinition();

            A.CallTo(() => dbProvider.CreateConnection()).Returns(dbConnection);
            A.CallTo(() => dbConnection.BeginTransaction(txDefinition.TransactionIsolationLevel)).Returns(dbTx);
            A.CallTo(() => dbProvider.CreateCommand()).Returns(dbCommand);
            A.CallTo(() => dbCommand.ExecuteNonQuery()).Returns(0);

            AdoTemplate adoOps = new AdoTemplate(dbProvider);
            IPlatformTransaction tx = SimpleAdoTestUtils.CreateTransaction(dbProvider, txDefinition);

            SimpleAdoTestUtils.ExecuteSqlScript(adoOps, "simple sql cmd");
            tx.Commit();
            tx.Dispose();

            A.CallTo(() => dbConnection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => dbTx.Commit()).MustHaveHappenedOnceExactly();
            A.CallTo(() => dbCommand.Dispose()).MustHaveHappenedOnceExactly();
            A.CallTo(() => dbConnection.Dispose()).MustHaveHappenedOnceExactly();

            A.CallToSet(() => dbCommand.Connection).WhenArgumentsMatch(x => x[0] == dbConnection).MustHaveHappenedOnceExactly();
            A.CallToSet(() => dbCommand.Transaction).WhenArgumentsMatch(x => x[0] == dbTx).MustHaveHappenedOnceExactly();
            A.CallToSet(() => dbCommand.CommandText).WhenArgumentsMatch(x => (string) x[0] == "simple sql cmd").MustHaveHappenedOnceExactly();
            A.CallToSet(() => dbCommand.CommandType).WhenArgumentsMatch(x => (CommandType) x[0] == CommandType.Text).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void ExecuteScriptTransactedRollsbackIfNoCommit()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection dbConnection = A.Fake<IDbConnection>();
            IDbTransaction dbTx = A.Fake<IDbTransaction>();
            DefaultTransactionDefinition txDefinition = new DefaultTransactionDefinition();

            A.CallTo(() => dbProvider.CreateConnection()).Returns(dbConnection);
            A.CallTo(() => dbConnection.BeginTransaction(txDefinition.TransactionIsolationLevel)).Returns(dbTx);

            AdoTemplate adoOps = new AdoTemplate(dbProvider);
            IPlatformTransaction tx = SimpleAdoTestUtils.CreateTransaction(dbProvider, txDefinition);
            tx.Dispose();

            A.CallTo(() => dbConnection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => dbTx.Rollback()).MustHaveHappenedOnceExactly();
            A.CallTo(() => dbConnection.Dispose()).MustHaveHappenedOnceExactly();
        }
    }
}