using NUnit.Framework;

using Spring.Data.NHibernate.Generic;
using Spring.Testing.NUnit;

namespace Spring.Data.NHibernate.TxPromotion.Integration.Tests
{
    [TestFixture]
    public class TestUsingHibernateTxScopeTransactionManager : AbstractTransactionalSpringContextTests
    {
        #region DI

        public Generic.HibernateTemplate HibernateTemplate { get; set; }
        public IService1 Service1 { get; set; }
        public IService2 Service2 { get; set; }

        #endregion

        protected override string[] ConfigLocations
        {
            get
            {
                return new[]
                {
                    "config://spring/objects",
				    "assembly://Spring.Data.NHibernate.TxPromotion.Integration.Tests/Spring.Data.NHibernate.TxPromotion.Integration.Tests/Spring.Configuration.xml",
                    "assembly://Spring.Data.NHibernate.TxPromotion.Integration.Tests/Spring.Data.NHibernate.TxPromotion.Integration.Tests/Spring.NHibernate.xml",     
                    "assembly://Spring.Data.NHibernate.TxPromotion.Integration.Tests/Spring.Data.NHibernate.TxPromotion.Integration.Tests/Spring.HibernateTxScopeTransactionManager.xml"                         
                };
            }
        }

        [Test(Description = "This is failing test case in my unit test with original NH TX Scope Manager, but not Custom TX Scope Manager, Not Supported -> Not Supported")]
        public void TestSuspendTransactionOnNotSupported()
        {
            Service1.ServiceMethodWithNotSupported1();
        }

        [Test(Description = "This is also failing with NH TX Scope manager, but not with Custom TX Scope Manager, Not Supported -> Requires New")]
        public void TestSuspendTransactionOnNotSupportedWithNestedRequiresNew()
        {
            Service2.ServiceMethodWithNotSupported();
        }

        [Test(Description = "This is failing test case in my unit test with Custom TX Scope Manager (this is the original bug reported), Not Supported -> Not Supported -> Required")]
        public void TestSuspendTransactionOnNotSupportedWithNestedRequired()
        {
            Service1.ServiceMethodWithNotSupported3();
        }
    }
}
