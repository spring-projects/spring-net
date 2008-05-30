using System;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Dao;
using Spring.Objects;

namespace Spring.Data
{
	/// <summary>
	/// Summary description for AdoTemplateTests.
	/// </summary>
	[TestFixture]
	public class AdoDaoTests
	{
		public AdoDaoTests()
		{
		}	    	    

	    [Test]
	    public void SimpleCreate()
	    {
            IApplicationContext ctx =
                new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/nativeAdoTests.xml");
            Assert.IsNotNull(ctx);
            ITestObjectDao dao = (ITestObjectDao)ctx["testObjectDao"];
            Assert.IsNotNull(dao);
            dao.Create("John", 44);
	    }
        [Test]
        public void SimpleDao()
        {
            IApplicationContext ctx =
                new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/templateTests.xml");
            Assert.IsNotNull(ctx);
            TestObjectDao dao = (TestObjectDao)ctx["testObjectDao"];
            Assert.IsNotNull(dao);
            Assert.AreEqual(10, dao.GetCount());
            
            Assert.AreEqual(3, dao.GetCount(33));
            Assert.AreEqual(3, dao.GetCountByAltMethod(33));
            Assert.AreEqual(3, dao.GetCountByCommandSetter(33));            
            Assert.AreEqual(2, dao.GetCount(33,"George"));
            
            
        }

        [Test]
        public void SimpleDao2()
        {
            IApplicationContext ctx =
                new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/templateTests.xml");
            Assert.IsNotNull(ctx);
            TestObjectDao dao = (TestObjectDao)ctx["testObjectDao"];
            Assert.IsNotNull(dao);
            Assert.AreEqual(1, dao.GetCountByDelegate());
        }

        [Test]
        public void DaoOperations()
        {
            IApplicationContext ctx =
                new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/templateTests.xml");
            Assert.IsNotNull(ctx);
            TestObjectDao dao = (TestObjectDao)ctx["testObjectDao"];
            dao.Create("George", 33);
            TestObject to = dao.FindByName("George");
            Assert.IsNotNull(to);
            Assert.AreEqual("George", to.Name);
            Assert.AreEqual(33, to.Age);

            to.Age=34;
            dao.Update(to);

            TestObject to2 = dao.FindByName("George");
            Assert.AreEqual(34, to2.Age);

            dao.Delete("George");

            TestObject to3 = dao.FindByName("George");
            Assert.IsNull(to3);



        }
	}
}
