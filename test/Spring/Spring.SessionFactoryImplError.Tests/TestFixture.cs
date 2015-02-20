using NUnit.Framework;

using Spring.Data.NHibernate.Generic;
using Spring.Testing.NUnit;

namespace Spring.SessionFactoryImplError.Tests
{
    [TestFixture]
    public class TestFixture : AbstractTransactionalSpringContextTests
    {
        #region DI

        public HibernateTemplate HibernateTemplate { get; set; }
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
				    "assembly://Spring.SessionFactoryImplError.Tests/Spring.SessionFactoryImplError.Tests/Spring.Configuration.xml",
                    "assembly://Spring.SessionFactoryImplError.Tests/Spring.SessionFactoryImplError.Tests/Spring.NHibernate.xml"                    
                };
            }
        }

        [Test(Description = "This is failing test case in my unit test, Not Supported -> Not Supported")]
        public void TestSuspendTransactionOnNotSupported()
        {           
            Service1.ServiceMethod1();
        }

        [Test(Description = "This is also failing, Not Supported -> Requires New")]
        public void TestSuspendTransactionOnRequiresNew()
        {          
            Service2.ServiceMethod1();
        }
    }
}
