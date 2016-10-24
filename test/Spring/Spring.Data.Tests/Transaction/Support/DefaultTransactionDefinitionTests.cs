using System;
using System.Data;
using NUnit.Framework;

namespace Spring.Transaction.Support
{
	[TestFixture]
	public class DefaultTransactionDefinitionTests
	{
		[Test]
		public void VanillaTest()
		{
			DefaultTransactionDefinition def = new DefaultTransactionDefinition( TransactionPropagation.NotSupported);
			Assert.IsTrue( def.PropagationBehavior == TransactionPropagation.NotSupported );
			def.PropagationBehavior = TransactionPropagation.Nested;
			Assert.IsTrue( def.PropagationBehavior == TransactionPropagation.Nested );
			def.TransactionIsolationLevel = IsolationLevel.ReadCommitted;
			Assert.IsTrue( def.TransactionIsolationLevel == IsolationLevel.ReadCommitted);
			def.TransactionTimeout = 1000;
			Assert.IsTrue( 1000 == def.TransactionTimeout );
			Assert.IsTrue( false == def.ReadOnly );
			def.ReadOnly = true;
			Assert.IsTrue( def.ReadOnly );
		}
		[Test]
		public void PropogationBehaviorDefault()
		{
			DefaultTransactionDefinition def = new DefaultTransactionDefinition();
			Assert.IsTrue( def.PropagationBehavior == TransactionPropagation.Required );
		}
        [Test]
        public void IsolationLevelDefault()
        {
            DefaultTransactionDefinition def = new DefaultTransactionDefinition();
            Assert.IsTrue(def.TransactionIsolationLevel == IsolationLevel.ReadCommitted);
        }
        [Test]
		public void InvalidTimeout()
		{
			DefaultTransactionDefinition def = new DefaultTransactionDefinition();
			Assert.Throws<ArgumentException>(() => def.TransactionTimeout = -1000);
		}
		[Test]
		public void DefinitionString()
		{
			DefaultTransactionDefinition def = new DefaultTransactionDefinition();
			DefaultTransactionDefinition def2 = new DefaultTransactionDefinition();

			Assert.IsTrue( def.ToString() == def2.ToString());
			def.Equals(def2);
		}
		[Test]
			public void DefinitionStringFilled()
		{
			DefaultTransactionDefinition def = new DefaultTransactionDefinition( TransactionPropagation.Never );
			def.TransactionIsolationLevel = IsolationLevel.Chaos;
			def.TransactionTimeout = 1000;
			def.ReadOnly = true;
			Assert.AreEqual( "PROPAGATION_Never,ISOLATION_Chaos,timeout_1000,readOnly", def.ToString());
			
			DefaultTransactionDefinition def2 = new DefaultTransactionDefinition( TransactionPropagation.Never );
			def2.TransactionIsolationLevel = IsolationLevel.Chaos;
			def2.TransactionTimeout = 1000;
			def2.ReadOnly = true;

			Assert.IsTrue( def2.Equals(def));
			Assert.IsTrue( def.GetHashCode() == def2.GetHashCode());
		}
	}
}
