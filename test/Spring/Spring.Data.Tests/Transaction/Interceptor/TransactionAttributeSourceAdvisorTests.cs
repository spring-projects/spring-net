using System.Collections.Specialized;
using NUnit.Framework;
using Spring.Util;

namespace Spring.Transaction.Interceptor
{
	[TestFixture]
	public class TransactionAttributeSourceAdvisorTests
	{
		[Test]
		public void Serializability()
		{
			TransactionInterceptor ti = new TransactionInterceptor();
			ti.TransactionAttributes = new NameValueCollection();
			TransactionAttributeSourceAdvisor tas = new TransactionAttributeSourceAdvisor(ti);
			SerializationTestUtils.SerializeAndDeserialize(tas);
		}
	}
}