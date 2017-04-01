using System;
using System.Collections;

using FakeItEasy;

using NUnit.Framework;

using Spring.Core;
using Spring.Util;

namespace Spring.Transaction.Support
{
    [TestFixture]
    public class TransactionSynchronizationManagerTests
    {
        [SetUp]
        public void Init()
        {
            if (TransactionSynchronizationManager.SynchronizationActive)
            {
                TransactionSynchronizationManager.ClearSynchronization();
            }
        }

        [TearDown]
        public void Destory()
        {
            if (TransactionSynchronizationManager.SynchronizationActive)
            {
                TransactionSynchronizationManager.ClearSynchronization();
            }
        }

        [Test]
        public void SynchronizationsInvalid()
        {
            IList temp;
            Assert.Throws<InvalidOperationException>(() => temp = TransactionSynchronizationManager.Synchronizations);
        }

        [Test]
        public void InitSynchronizationsInvalid()
        {
            TransactionSynchronizationManager.InitSynchronization();
            Assert.Throws<InvalidOperationException>(() => TransactionSynchronizationManager.InitSynchronization());
        }

        [Test]
        public void SynchronizationsClearInvalid()
        {
            Assert.Throws<InvalidOperationException>(() => TransactionSynchronizationManager.ClearSynchronization());
        }

        [Test]
        public void RegisterSyncsInvalid()
        {
            Assert.Throws<InvalidOperationException>(() => TransactionSynchronizationManager.RegisterSynchronization(A.Fake<ITransactionSynchronization>()));
        }

        [Test]
        public void SynchronizationsLifeCycle()
        {
            TransactionSynchronizationManager.InitSynchronization();
            IList syncs = TransactionSynchronizationManager.Synchronizations;
            Assert.AreEqual(0, syncs.Count);
            TransactionSynchronizationManager.RegisterSynchronization(A.Fake<ITransactionSynchronization>());
            syncs = TransactionSynchronizationManager.Synchronizations;
            Assert.AreEqual(1, syncs.Count);
            TransactionSynchronizationManager.ClearSynchronization();
        }

        [Test(Description = "http://jira.springframework.org/browse/SPRNET-1160")]
        public void SynchronizationsExecuteInOrderOfRegistration()
        {
            TransactionSynchronizationManager.InitSynchronization();

            try
            {
                // expect syncs to be run A, B, C, D, E since all have order '1'
                TransactionSynchronizationManager.RegisterSynchronization(new Sync("A", 1));
                TransactionSynchronizationManager.RegisterSynchronization(new Sync("B", 1));
                TransactionSynchronizationManager.RegisterSynchronization(new Sync("C", 1));
                TransactionSynchronizationManager.RegisterSynchronization(new Sync("D", 1));
                TransactionSynchronizationManager.RegisterSynchronization(new Sync("E", 1));

                // simulate what APTM does
                Sync[] syncs = (Sync[]) CollectionUtils.ToArray(TransactionSynchronizationManager.Synchronizations, typeof(Sync));
                Assert.AreEqual("A", syncs[0].Name);
                Assert.AreEqual("B", syncs[1].Name);
                Assert.AreEqual("C", syncs[2].Name);
                Assert.AreEqual("D", syncs[3].Name);
                Assert.AreEqual("E", syncs[4].Name);
            }
            finally
            {
                TransactionSynchronizationManager.ClearSynchronization();
            }
        }

        private class Sync : TransactionSynchronizationAdapter, IOrdered
        {
            private readonly string name;
            private readonly int order;

            public Sync(string name, int order)
            {
                this.name = name;
                this.order = order;
            }

            public int Order
            {
                get { return order; }
            }

            public string Name
            {
                get { return name; }
            }

            public override string ToString()
            {
                return name;
            }
        }
    }
}