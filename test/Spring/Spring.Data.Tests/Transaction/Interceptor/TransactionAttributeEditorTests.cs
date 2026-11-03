using System.Data;
using NUnit.Framework;

namespace Spring.Transaction.Interceptor;

[TestFixture]
public class TransactionAttributeEditorTests
{
    [Test]
    public void NullTest()
    {
        TransactionAttributeEditor editor = new TransactionAttributeEditor();
        editor.SetAsText(null);
        ITransactionAttribute ta = editor.Value;
        Assert.That(ta, Is.Null);
    }

    [Test]
    public void EmptyStringTest()
    {
        TransactionAttributeEditor editor = new TransactionAttributeEditor();
        editor.SetAsText(String.Empty);
        ITransactionAttribute ta = editor.Value;
        Assert.That(ta, Is.Null);
    }

    [Test]
    public void ValidPropagationCodeOnly()
    {
        TransactionAttributeEditor editor = new TransactionAttributeEditor();
        editor.SetAsText("PROPAGATION_REQUIRED");
        ITransactionAttribute ta = editor.Value;
        Assert.That(ta != null, Is.True);
        Assert.That(ta.PropagationBehavior == TransactionPropagation.Required, Is.True);
        Assert.That(ta.TransactionIsolationLevel == IsolationLevel.ReadCommitted, Is.True);
        Assert.That(ta.ReadOnly, Is.False);
    }

    [Test]
    public void InvalidPropagationCodeOnly()
    {
        TransactionAttributeEditor editor = new TransactionAttributeEditor();
        Assert.Throws<ArgumentException>(() => editor.SetAsText("INVALIDPROPAGATIONCODE"));
    }

    [Test]
    public void ValidPropagationCodeAndIsolationCode()
    {
        TransactionAttributeEditor editor = new TransactionAttributeEditor();
        editor.SetAsText("PROPAGATION_REQUIRED, ISOLATION_READUNCOMMITTED");
        ITransactionAttribute ta = editor.Value;
        Assert.That(ta != null, Is.True);
        Assert.That(ta.PropagationBehavior == TransactionPropagation.Required, Is.True);
        Assert.That(ta.TransactionIsolationLevel == IsolationLevel.ReadUncommitted, Is.True);
    }

    [Test]
    public void ValidPropagationAndIsolationCodeAndInvalidRollbackRule()
    {
        TransactionAttributeEditor editor = new TransactionAttributeEditor();
        Assert.Throws<ArgumentException>(() => editor.SetAsText("PROPAGATION_REQUIRED,ISOLATION_READUNCOMMITTED,XXX"));
    }

#if !NETCOREAPP
    [Test]
    public void ValidPropagationCodeAndIsolationCodeAndRollbackRules1()
    {
        TransactionAttributeEditor editor = new TransactionAttributeEditor( );
        editor.SetAsText( "PROPAGATION_MANDATORY,ISOLATION_REPEATABLEREAD,timeout_10,-DataException,+RemotingException" );
        ITransactionAttribute ta = editor.Value;
        Assert.That(ta, Is.Not.Null);
        Assert.That(ta.PropagationBehavior == TransactionPropagation.Mandatory, Is.True);
        Assert.That(ta.TransactionIsolationLevel == IsolationLevel.RepeatableRead, Is.True);
        Assert.That(ta.TransactionTimeout == 10, Is.True);
        Assert.That(ta.ReadOnly, Is.False);
        Assert.That(ta.RollbackOn(new SystemException()), Is.True);
        // Check for our bizarre customized rollback rules
        Assert.That(ta.RollbackOn(new DataException()), Is.True);
        Assert.That(!ta.RollbackOn(new System.Runtime.Remoting.RemotingException()), Is.True);
    }

    [Test]
    public void ValidPropagationCodeAndIsolationCodeAndRollbackRules2()
    {
        TransactionAttributeEditor editor = new TransactionAttributeEditor( );
        editor.SetAsText( "+DataException,readOnly,ISOLATION_READCOMMITTED,-RemotingException,PROPAGATION_SUPPORTS" );
        ITransactionAttribute ta = editor.Value;
        Assert.That(ta, Is.Not.Null);
        Assert.That(ta.PropagationBehavior == TransactionPropagation.Supports, Is.True);
        Assert.That(ta.TransactionIsolationLevel == IsolationLevel.ReadCommitted, Is.True);
        Assert.That(ta.TransactionTimeout == -1, Is.True);
        Assert.That(ta.ReadOnly, Is.True);
        Assert.That(ta.RollbackOn(new SystemException()), Is.True);
        // Check for our bizarre customized rollback rules
        Assert.That(ta.RollbackOn(new DataException()), Is.False);
        Assert.That(ta.RollbackOn(new System.Runtime.Remoting.RemotingException()), Is.True);
    }
#endif

    [Test]
    public void DefaultTransactionAttributeToString()
    {
        DefaultTransactionAttribute source = new DefaultTransactionAttribute();
        source.PropagationBehavior = TransactionPropagation.Supports;
        source.TransactionIsolationLevel = IsolationLevel.RepeatableRead;
        source.TransactionTimeout = 10;
        source.ReadOnly = true;

        TransactionAttributeEditor editor = new TransactionAttributeEditor();
        editor.SetAsText(source.ToString());
        ITransactionAttribute ta = editor.Value;
        Assert.That(ta, Is.EqualTo(source));
        Assert.That(ta.PropagationBehavior, Is.EqualTo(TransactionPropagation.Supports));
        Assert.That(ta.TransactionIsolationLevel, Is.EqualTo(IsolationLevel.RepeatableRead));
        Assert.That(ta.TransactionTimeout, Is.EqualTo(10));
        Assert.That(ta.ReadOnly, Is.True);
        Assert.That(ta.RollbackOn(new SystemException()), Is.True);
        //mlp 3/17 changed rollback to rollback on all exceptions.
        Assert.That(ta.RollbackOn(new ApplicationException()), Is.True);

        source.TransactionTimeout = 9;
        Assert.That(ta == source, Is.False);
        source.TransactionTimeout = 10;
        Assert.That(source, Is.EqualTo(ta));
    }

    [Test]
    public void RuleBasedTransactionAttributeToString()
    {
        RuleBasedTransactionAttribute source = new RuleBasedTransactionAttribute();
        source.PropagationBehavior = TransactionPropagation.Supports;
        source.TransactionIsolationLevel = IsolationLevel.RepeatableRead;
        source.TransactionTimeout = 10;
        source.ReadOnly = true;
        source.AddRollbackRule(new RollbackRuleAttribute("ArgumentException"));
        source.AddRollbackRule(new NoRollbackRuleAttribute("IllegalTransactionStateException"));

        TransactionAttributeEditor editor = new TransactionAttributeEditor();
        editor.SetAsText(source.ToString());
        ITransactionAttribute ta = editor.Value;
        Assert.That(ta, Is.EqualTo(source));
        Assert.That(ta.PropagationBehavior, Is.EqualTo(TransactionPropagation.Supports));
        Assert.That(ta.TransactionIsolationLevel, Is.EqualTo(IsolationLevel.RepeatableRead));
        Assert.That(ta.TransactionTimeout, Is.EqualTo(10));
        Assert.That(ta.ReadOnly, Is.True);
        Assert.That(ta.RollbackOn(new ArgumentException()), Is.True);
        Assert.That(ta.RollbackOn(new IllegalTransactionStateException()), Is.False);

        source.ClearRollbackRules();
        Assert.That(ta == source, Is.False);
        source.AddRollbackRule(new RollbackRuleAttribute("ArgumentException"));
        source.AddRollbackRule(new NoRollbackRuleAttribute("IllegalTransactionStateException"));
        Assert.That(source, Is.EqualTo(ta));
    }
}
