using NUnit.Framework;

using Spring.Testing.NUnit;

namespace Spring.Data.NHibernate.NestedTxSuspension.Integration.Tests
{
    [TestFixture]
    public class TestUsingHibernateTxScopeTransactionManager : AbstractTransactionalSpringContextTests
    {
        #region DI

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
				    "assembly://Spring.Data.NHibernate.NestedTxSuspension.Integration.Tests/Spring.Data.NHibernate.NestedTxSuspension.Integration.Tests/Spring.Configuration.xml",
                    "assembly://Spring.Data.NHibernate.NestedTxSuspension.Integration.Tests/Spring.Data.NHibernate.NestedTxSuspension.Integration.Tests/Spring.NHibernate.xml",     
                    "assembly://Spring.Data.NHibernate.NestedTxSuspension.Integration.Tests/Spring.Data.NHibernate.NestedTxSuspension.Integration.Tests/Spring.HibernateTxScopeTransactionManager.xml"                         
                };
            }
        }

        [Test]
        public void TestSuspendTransactionOnNotSupported()
        {
            Service1.ServiceMethodWithNotSupported1();
        }

        [Test]
        public void TestSuspendTransactionOnNotSupportedWithNestedRequiresNew()
        {
            Service2.ServiceMethodWithNotSupported();
        }

        [Test]
        public void TestSuspendTransactionOnNotSupportedWithNestedRequired()
        {
            Service1.ServiceMethodWithNotSupported3();
        }
    }
}
