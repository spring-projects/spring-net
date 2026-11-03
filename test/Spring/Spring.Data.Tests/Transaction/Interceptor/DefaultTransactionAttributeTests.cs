using NUnit.Framework;

namespace Spring.Transaction.Interceptor;

[TestFixture]
public class DefaultTransactionAttributeTests
{
    [Test]
    public void RollbackOnTests()
    {
        DefaultTransactionAttribute dta = new DefaultTransactionAttribute();
        Assert.That(dta.RollbackOn(new SystemException()), Is.True);
        //mlp 3/17 changed rollback to rollback on all exceptions.
        Assert.That(dta.RollbackOn(new TransactionSystemException()), Is.True);
    }

    [Test]
    public void ToStringTests()
    {
        DefaultTransactionAttribute dta = new DefaultTransactionAttribute();
        Assert.That(dta.ToString(), Is.EqualTo("PROPAGATION_Required,ISOLATION_ReadCommitted,-System.Exception"));
    }
}
