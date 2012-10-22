using System;
using NUnit.Framework;

namespace Spring.Transaction.Interceptor
{
	[TestFixture]
	public class DefaultTransactionAttributeTests
	{
		[Test]
		public void RollbackOnTests()
		{
			DefaultTransactionAttribute dta = new DefaultTransactionAttribute();
			Assert.IsTrue( dta.RollbackOn( new SystemException()));
            //mlp 3/17 changed rollback to rollback on all exceptions.
			Assert.IsTrue( dta.RollbackOn( new TransactionSystemException()));
		}
		[Test]
		public void ToStringTests()
		{
			DefaultTransactionAttribute dta = new DefaultTransactionAttribute();
			Assert.AreEqual( "PROPAGATION_Required,ISOLATION_Unspecified,-System.Exception", dta.ToString());
		}
	}
}
