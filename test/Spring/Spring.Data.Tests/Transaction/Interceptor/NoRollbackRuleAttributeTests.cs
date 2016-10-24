using System;
using System.Data;
using System.Text;
using NUnit.Framework;

namespace Spring.Transaction.Interceptor
{
	[TestFixture]
	public class NoNoRollbackRuleAttributeTests
	{
	    [Test]
	    public void FoundImmediatelyWithString()
	    {
	        NoRollbackRuleAttribute rr = new NoRollbackRuleAttribute("System.Exception");
	        Assert.IsTrue(rr.GetDepth(typeof(Exception)) == 0);
	    }

	    [Test]
	    public void FoundImmediatelyWithClass()
	    {
	        NoRollbackRuleAttribute rr = new NoRollbackRuleAttribute(typeof(Exception));
	        Assert.IsTrue(rr.GetDepth(typeof(Exception)) == 0);
	    }

	    [Test]
	    public void NotFound()
	    {
	        NoRollbackRuleAttribute rr = new NoRollbackRuleAttribute("System.Data.DataException");
	        Assert.IsTrue(rr.GetDepth(typeof(ApplicationException)) == -1);
	    }

	    [Test]
	    public void Ancestry()
	    {
	        NoRollbackRuleAttribute rr = new NoRollbackRuleAttribute("System.Exception");
	        Assert.IsTrue(rr.GetDepth(typeof(DataException)) == 2);
	    }

	    [Test]
	    public void AlwaysTrue()
	    {
	        NoRollbackRuleAttribute rr = new NoRollbackRuleAttribute("System.Exception");
	        Assert.IsTrue(rr.GetDepth(typeof(SystemException)) > 0);
	        Assert.IsTrue(rr.GetDepth(typeof(ApplicationException)) > 0);
	        Assert.IsTrue(rr.GetDepth(typeof(DataException)) > 0);
	        Assert.IsTrue(rr.GetDepth(typeof(TransactionSystemException)) > 0);
	    }

	    [Test]
	    public void ConstructorArgMustBeAExceptionClass()
	    {
	        Assert.Throws<ArgumentException>(() => new NoRollbackRuleAttribute(typeof(StringBuilder)));
	    }
	}
}
