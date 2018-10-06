using NUnit.Framework;

using Spring.Testing.NUnit;

namespace Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests
{
    [TestFixture]
    public class HibernateTxScopeTransactionManagerNestedTransactionSuspensionTests : AbstractTransactionalSpringContextTests
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
				    "assembly://Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests/Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests/Spring.Configuration.xml",
                    "assembly://Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests/Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests/Spring.NHibernate.xml",     
                    "assembly://Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests/Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests/Spring.HibernateTxScopeTransactionManager.xml"                         
                };
            }
        }

        [Test]
        public void CanSuspendTransactionOnNotSupported()
        {
            Service1.ServiceMethodWithNotSupported1();
        }

        [Test]
        public void CanSuspendTransactionOnNotSupportedWithNestedRequiresNew()
        {
            Service2.ServiceMethodWithNotSupported();
        }

        [Test]
        public void CanSuspendTransactionOnNotSupportedWithNestedRequired()
        {
            Service1.ServiceMethodWithNotSupported3();
        }
    }
}
