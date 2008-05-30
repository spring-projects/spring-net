using System;
using System.Collections;
using System.Data;
using NUnit.Framework;
using Spring.Collections;

namespace Spring.Transaction.Interceptor
{
	[TestFixture]
	public class RuleBasedTransactionAttributeTests
	{
		[Test]
			public void DefaultRule()
		{
			RuleBasedTransactionAttribute rta = new RuleBasedTransactionAttribute();
			Assert.IsTrue(rta.RollbackOn(new SystemException()));
            //mlp 3/17 changed rollback to rollback on all exceptions.
			Assert.IsTrue(rta.RollbackOn(new ApplicationException()));
			Assert.IsTrue( rta.RollbackOn(new TransactionSystemException()));
		}
		[Test]
			public void RuleForRollbackOnApplicationException()
		{
			IList list = new LinkedList();
			list.Add(new RollbackRuleAttribute("Spring.Transaction.TransactionSystemException"));
			RuleBasedTransactionAttribute rta = new RuleBasedTransactionAttribute( TransactionPropagation.Required, list );

			Assert.IsTrue( rta.RollbackOn(new SystemException()));
            //mlp 3/17 changed rollback to rollback on all exceptions.
			Assert.IsTrue( rta.RollbackOn(new ApplicationException()));
			Assert.IsTrue(( rta.RollbackOn( new TransactionSystemException())));
		}
		[Test]
			public void RuleForCommitOnUnchecked()
		{
			IList list = new LinkedList();
			list.Add( new NoRollbackRuleAttribute("System.SystemException"));
			list.Add(new RollbackRuleAttribute("Spring.Transaction.TransactionSystemException"));

			RuleBasedTransactionAttribute rta = new RuleBasedTransactionAttribute( TransactionPropagation.Required, list );
			Assert.IsFalse( rta.RollbackOn(new SystemException()));
			Assert.IsTrue( rta.RollbackOn(new TransactionSystemException()));
		}
		[Test]
			public void RuleForSelectiveRollbackOnCheckedWithString()
		{
			IList list = new LinkedList();
			list.Add( new RollbackRuleAttribute("Spring.Transaction.TransactionSystemException"));
			RuleBasedTransactionAttribute rta = new RuleBasedTransactionAttribute( TransactionPropagation.Required, list );
			ruleForSelectionRollbackOnChecked( rta );
		}
		[Test]
			public void RuleForSelectiveRollbackOnCheckedWithClass()
		{
			IList list = new LinkedList();
			list.Add( new RollbackRuleAttribute(typeof(TransactionSystemException)));
			RuleBasedTransactionAttribute rta = new RuleBasedTransactionAttribute( TransactionPropagation.Required, list );
			ruleForSelectionRollbackOnChecked( rta );
		}
		private void ruleForSelectionRollbackOnChecked( RuleBasedTransactionAttribute rta )
		{
			Assert.IsTrue(rta.RollbackOn(new SystemException()));
			Assert.IsTrue( rta.RollbackOn(new TransactionSystemException()));
		}
		[Test]
			public void RuleForCommitOnSubclassOfChecked()
		{
			IList list = new LinkedList();
			list.Add( new RollbackRuleAttribute("System.Data.DataException"));
			list.Add( new NoRollbackRuleAttribute("Spring.Transaction.TransactionSystemException"));
			RuleBasedTransactionAttribute rta = new RuleBasedTransactionAttribute( TransactionPropagation.Required, list );
			
			Assert.IsTrue( rta.RollbackOn(new SystemException()));
			Assert.IsFalse( rta.RollbackOn(new TransactionSystemException()));																		 
		}
		[Test]
			public void RollbackNever()
		{
			IList list = new LinkedList();
			list.Add( new NoRollbackRuleAttribute("System.Exception"));
			RuleBasedTransactionAttribute rta = new RuleBasedTransactionAttribute( TransactionPropagation.Required, list );

			Assert.IsFalse(rta.RollbackOn(new SystemException()));
			Assert.IsFalse(rta.RollbackOn(new DataException()));
			Assert.IsFalse(rta.RollbackOn(new ApplicationException()));
			Assert.IsFalse(rta.RollbackOn(new TransactionSystemException()));
		}
	}
}
