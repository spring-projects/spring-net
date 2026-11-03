using System.Data;
using NUnit.Framework;

namespace Spring.Transaction.Interceptor;

[TestFixture]
public class RuleBasedTransactionAttributeTests
{
    [Test]
    public void DefaultRule()
    {
        var rta = new RuleBasedTransactionAttribute();
        Assert.That(rta.RollbackOn(new SystemException()), Is.True);
        //mlp 3/17 changed rollback to rollback on all exceptions.
        Assert.That(rta.RollbackOn(new ApplicationException()), Is.True);
        Assert.That(rta.RollbackOn(new TransactionSystemException()), Is.True);
    }

    [Test]
    public void RuleForRollbackOnApplicationException()
    {
        var list = new List<RollbackRuleAttribute>();
        list.Add(new RollbackRuleAttribute("Spring.Transaction.TransactionSystemException"));
        RuleBasedTransactionAttribute
            rta = new RuleBasedTransactionAttribute(TransactionPropagation.Required, list);

        Assert.That(rta.RollbackOn(new SystemException()), Is.True);
        //mlp 3/17 changed rollback to rollback on all exceptions.
        Assert.That(rta.RollbackOn(new ApplicationException()), Is.True);
        Assert.That((rta.RollbackOn(new TransactionSystemException())), Is.True);
    }

    [Test]
    public void RuleForCommitOnUnchecked()
    {
        var list = new List<RollbackRuleAttribute>();
        list.Add(new NoRollbackRuleAttribute("System.SystemException"));
        list.Add(new RollbackRuleAttribute("Spring.Transaction.TransactionSystemException"));

        var rta = new RuleBasedTransactionAttribute(TransactionPropagation.Required, list);
        Assert.That(rta.RollbackOn(new SystemException()), Is.False);
        Assert.That(rta.RollbackOn(new TransactionSystemException()), Is.True);
    }

    [Test]
    public void RuleForSelectiveRollbackOnCheckedWithString()
    {
        IList<RollbackRuleAttribute> list = new List<RollbackRuleAttribute>();
        list.Add(new RollbackRuleAttribute("Spring.Transaction.TransactionSystemException"));
        var rta = new RuleBasedTransactionAttribute(TransactionPropagation.Required, list);
        ruleForSelectionRollbackOnChecked(rta);
    }

    [Test]
    public void RuleForSelectiveRollbackOnCheckedWithClass()
    {
        var list = new List<RollbackRuleAttribute>();
        list.Add(new RollbackRuleAttribute(typeof(TransactionSystemException)));
        var rta = new RuleBasedTransactionAttribute(TransactionPropagation.Required, list);
        ruleForSelectionRollbackOnChecked(rta);
    }

    private void ruleForSelectionRollbackOnChecked(RuleBasedTransactionAttribute rta)
    {
        Assert.That(rta.RollbackOn(new SystemException()), Is.True);
        Assert.That(rta.RollbackOn(new TransactionSystemException()), Is.True);
    }

    [Test]
    public void RuleForCommitOnSubclassOfChecked()
    {
        var list = new List<RollbackRuleAttribute>();
        list.Add(new RollbackRuleAttribute("System.Data.DataException"));
        list.Add(new NoRollbackRuleAttribute("Spring.Transaction.TransactionSystemException"));
        var rta = new RuleBasedTransactionAttribute(TransactionPropagation.Required, list);

        Assert.That(rta.RollbackOn(new SystemException()), Is.True);
        Assert.That(rta.RollbackOn(new TransactionSystemException()), Is.False);
    }

    [Test]
    public void RollbackNever()
    {
        var list = new List<RollbackRuleAttribute>();
        list.Add(new NoRollbackRuleAttribute("System.Exception"));
        var rta = new RuleBasedTransactionAttribute(TransactionPropagation.Required, list);

        Assert.That(rta.RollbackOn(new SystemException()), Is.False);
        Assert.That(rta.RollbackOn(new DataException()), Is.False);
        Assert.That(rta.RollbackOn(new ApplicationException()), Is.False);
        Assert.That(rta.RollbackOn(new TransactionSystemException()), Is.False);
    }
}
