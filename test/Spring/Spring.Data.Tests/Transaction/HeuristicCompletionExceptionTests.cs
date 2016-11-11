using System;

using NUnit.Framework;

namespace Spring.Transaction
{
	[TestFixture]
	public class HeuristicCompletionExceptionTests
	{
		[Test]
		public void TransactionOutcomeStateGetter() 
		{
			HeuristicCompletionException ex = new HeuristicCompletionException( TransactionOutcomeState.Unknown, new Exception() );
			Assert.IsTrue( TransactionOutcomeState.Unknown == ex.OutcomeState );
		}	
		[Test]
		public void TransactionOutcomeStateGetterCommittted() 
		{
			HeuristicCompletionException ex = new HeuristicCompletionException( TransactionOutcomeState.Committed, new Exception() );
			Assert.IsTrue( TransactionOutcomeState.Committed == ex.OutcomeState );
		}
		[Test]
		public void TransactionOutcomeStateGetterMixed() 
		{
			HeuristicCompletionException ex = new HeuristicCompletionException( TransactionOutcomeState.Mixed, new Exception() );
			Assert.IsTrue( TransactionOutcomeState.Mixed == ex.OutcomeState );
		}
		[Test]
		public void TransactionOutcomeStateGetterRolledback() 
		{
			HeuristicCompletionException ex = new HeuristicCompletionException( TransactionOutcomeState.Rolledback, new Exception() );
			Assert.IsTrue( TransactionOutcomeState.Rolledback == ex.OutcomeState );
		}
	}
}
