using System.Data;
using System.Text;
using NUnit.Framework;

namespace Spring.Transaction.Interceptor;

[TestFixture]
public class NoNoRollbackRuleAttributeTests
{
    [Test]
    public void FoundImmediatelyWithString()
    {
        NoRollbackRuleAttribute rr = new NoRollbackRuleAttribute("System.Exception");
        Assert.That(rr.GetDepth(typeof(Exception)) == 0, Is.True);
    }

    [Test]
    public void FoundImmediatelyWithClass()
    {
        NoRollbackRuleAttribute rr = new NoRollbackRuleAttribute(typeof(Exception));
        Assert.That(rr.GetDepth(typeof(Exception)) == 0, Is.True);
    }

    [Test]
    public void NotFound()
    {
        NoRollbackRuleAttribute rr = new NoRollbackRuleAttribute("System.Data.DataException");
        Assert.That(rr.GetDepth(typeof(ApplicationException)) == -1, Is.True);
    }

    [Test]
    public void Ancestry()
    {
        NoRollbackRuleAttribute rr = new NoRollbackRuleAttribute("System.Exception");
        Assert.That(rr.GetDepth(typeof(DataException)) == 2, Is.True);
    }

    [Test]
    public void AlwaysTrue()
    {
        NoRollbackRuleAttribute rr = new NoRollbackRuleAttribute("System.Exception");
        Assert.That(rr.GetDepth(typeof(SystemException)) > 0, Is.True);
        Assert.That(rr.GetDepth(typeof(ApplicationException)) > 0, Is.True);
        Assert.That(rr.GetDepth(typeof(DataException)) > 0, Is.True);
        Assert.That(rr.GetDepth(typeof(TransactionSystemException)) > 0, Is.True);
    }

    [Test]
    public void ConstructorArgMustBeAExceptionClass()
    {
        Assert.Throws<ArgumentException>(() => new NoRollbackRuleAttribute(typeof(StringBuilder)));
    }
}
