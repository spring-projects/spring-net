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

using FakeItEasy;
using NUnit.Framework;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Data.Core;

/// <summary>
/// This class contains tests for TxScopeTransactionManager and will directly a real TransactionScope object
/// but does not access any database
/// </summary>
/// <author>Mark Pollack</author>
[TestFixture]
public class TxScopeTransactionManagerIntegrationTests
{
    [TearDown]
    public void TearDown()
    {
        Assert.That(TransactionSynchronizationManager.ResourceDictionary.Count == 0, Is.True);
        Assert.That(TransactionSynchronizationManager.SynchronizationActive, Is.False);
        Assert.That(TransactionSynchronizationManager.CurrentTransactionName, Is.Null);
        Assert.That(TransactionSynchronizationManager.CurrentTransactionReadOnly, Is.False);
        Assert.That(TransactionSynchronizationManager.CurrentTransactionIsolationLevel, Is.EqualTo(System.Data.IsolationLevel.Unspecified));
        Assert.That(TransactionSynchronizationManager.ActualTransactionActive, Is.False);
    }

    [Test]
    public void Commit()
    {
        TxScopeTransactionManager tm = new TxScopeTransactionManager();
        tm.TransactionSynchronization = TransactionSynchronizationState.Always;

        TransactionTemplate tt = new TransactionTemplate(tm);

        //tt.Name = "txName";

        Assert.That(tm.TransactionSynchronization, Is.EqualTo(TransactionSynchronizationState.Always));
        Assert.That(TransactionSynchronizationManager.SynchronizationActive, Is.False);
        Assert.That(TransactionSynchronizationManager.CurrentTransactionName, Is.Null);
        Assert.That(TransactionSynchronizationManager.CurrentTransactionReadOnly, Is.False);
        tt.Execute(CommitTxDelegate);
        Assert.That(TransactionSynchronizationManager.SynchronizationActive, Is.False);
        Assert.That(TransactionSynchronizationManager.CurrentTransactionName, Is.Null);
        Assert.That(TransactionSynchronizationManager.CurrentTransactionReadOnly, Is.False);
    }

    public object CommitTxDelegate(ITransactionStatus status)
    {
        Assert.That(TransactionSynchronizationManager.SynchronizationActive, Is.True);
        Assert.That(TransactionSynchronizationManager.CurrentTransactionReadOnly, Is.False);

        return null;
    }

    [Test]
    public void TransactionInformation()
    {
        TxScopeTransactionManager tm = new TxScopeTransactionManager();
        tm.TransactionSynchronization = TransactionSynchronizationState.Always;

        TransactionTemplate tt = new TransactionTemplate(tm);
        tt.TransactionIsolationLevel = System.Data.IsolationLevel.ReadUncommitted;
        tt.Execute(TransactionInformationTxDelegate);
    }

    public object TransactionInformationTxDelegate(ITransactionStatus status)
    {
        Assert.That(System.Transactions.Transaction.Current.IsolationLevel,
            Is.EqualTo(System.Transactions.IsolationLevel.ReadUncommitted));

        Assert.That(TransactionSynchronizationManager.CurrentTransactionIsolationLevel,
            Is.EqualTo(System.Data.IsolationLevel.ReadUncommitted));
        return null;
    }

    [Test]
    public void Rollback()
    {
        ITransactionSynchronization sync = A.Fake<ITransactionSynchronization>();

        TxScopeTransactionManager tm = new TxScopeTransactionManager();
        tm.TransactionSynchronization = TransactionSynchronizationState.Always;

        TransactionTemplate tt = new TransactionTemplate(tm);
        tt.TransactionTimeout = 10;
        tt.Name = "txName";

        Assert.That(TransactionSynchronizationManager.SynchronizationActive, Is.False);
        Assert.That(TransactionSynchronizationManager.CurrentTransactionName, Is.Null);
        Assert.That(TransactionSynchronizationManager.CurrentTransactionReadOnly, Is.False);
        tt.Execute(status =>
            {
                Assert.That(TransactionSynchronizationManager.SynchronizationActive, Is.True);
                TransactionSynchronizationManager.RegisterSynchronization(sync);
                Assert.That(TransactionSynchronizationManager.CurrentTransactionName, Is.EqualTo("txName"));
                Assert.That(TransactionSynchronizationManager.CurrentTransactionReadOnly, Is.False);
                status.SetRollbackOnly();
                return null;
            }
        );

        A.CallTo(() => sync.BeforeCompletion()).MustHaveHappenedOnceExactly();
        A.CallTo(() => sync.AfterCompletion(TransactionSynchronizationStatus.Rolledback)).MustHaveHappenedOnceExactly();
    }
}
