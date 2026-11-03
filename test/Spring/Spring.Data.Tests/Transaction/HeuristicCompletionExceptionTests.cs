using NUnit.Framework;

namespace Spring.Transaction;

[TestFixture]
public class HeuristicCompletionExceptionTests
{
    [Test]
    public void TransactionOutcomeStateGetter()
    {
        HeuristicCompletionException ex = new HeuristicCompletionException(TransactionOutcomeState.Unknown, new Exception());
        Assert.That(TransactionOutcomeState.Unknown == ex.OutcomeState, Is.True);
    }

    [Test]
    public void TransactionOutcomeStateGetterCommittted()
    {
        HeuristicCompletionException ex = new HeuristicCompletionException(TransactionOutcomeState.Committed, new Exception());
        Assert.That(TransactionOutcomeState.Committed == ex.OutcomeState, Is.True);
    }

    [Test]
    public void TransactionOutcomeStateGetterMixed()
    {
        HeuristicCompletionException ex = new HeuristicCompletionException(TransactionOutcomeState.Mixed, new Exception());
        Assert.That(TransactionOutcomeState.Mixed == ex.OutcomeState, Is.True);
    }

    [Test]
    public void TransactionOutcomeStateGetterRolledback()
    {
        HeuristicCompletionException ex = new HeuristicCompletionException(TransactionOutcomeState.Rolledback, new Exception());
        Assert.That(TransactionOutcomeState.Rolledback == ex.OutcomeState, Is.True);
    }
}
