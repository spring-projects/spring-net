using System.Data;
using NUnit.Framework;

namespace Spring.Transaction.Support;

[TestFixture]
public class DefaultTransactionDefinitionTests
{
    [Test]
    public void VanillaTest()
    {
        DefaultTransactionDefinition def = new DefaultTransactionDefinition(TransactionPropagation.NotSupported);
        Assert.That(def.PropagationBehavior == TransactionPropagation.NotSupported, Is.True);
        def.PropagationBehavior = TransactionPropagation.Nested;
        Assert.That(def.PropagationBehavior == TransactionPropagation.Nested, Is.True);
        def.TransactionIsolationLevel = IsolationLevel.ReadCommitted;
        Assert.That(def.TransactionIsolationLevel == IsolationLevel.ReadCommitted, Is.True);
        def.TransactionTimeout = 1000;
        Assert.That(1000 == def.TransactionTimeout, Is.True);
        Assert.That(false == def.ReadOnly, Is.True);
        def.ReadOnly = true;
        Assert.That(def.ReadOnly, Is.True);
    }

    [Test]
    public void PropogationBehaviorDefault()
    {
        DefaultTransactionDefinition def = new DefaultTransactionDefinition();
        Assert.That(def.PropagationBehavior == TransactionPropagation.Required, Is.True);
    }

    [Test]
    public void IsolationLevelDefault()
    {
        DefaultTransactionDefinition def = new DefaultTransactionDefinition();
        Assert.That(def.TransactionIsolationLevel == IsolationLevel.ReadCommitted, Is.True);
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

        Assert.That(def.ToString() == def2.ToString(), Is.True);
        def.Equals(def2);
    }

    [Test]
    public void DefinitionStringFilled()
    {
        DefaultTransactionDefinition def = new DefaultTransactionDefinition(TransactionPropagation.Never);
        def.TransactionIsolationLevel = IsolationLevel.Chaos;
        def.TransactionTimeout = 1000;
        def.ReadOnly = true;
        Assert.That(def.ToString(), Is.EqualTo("PROPAGATION_Never,ISOLATION_Chaos,timeout_1000,readOnly"));

        DefaultTransactionDefinition def2 = new DefaultTransactionDefinition(TransactionPropagation.Never);
        def2.TransactionIsolationLevel = IsolationLevel.Chaos;
        def2.TransactionTimeout = 1000;
        def2.ReadOnly = true;

        Assert.That(def2.Equals(def), Is.True);
        Assert.That(def.GetHashCode() == def2.GetHashCode(), Is.True);
    }
}
