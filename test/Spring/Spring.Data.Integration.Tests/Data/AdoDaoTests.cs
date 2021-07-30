using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Objects;
using Spring.Testing.NUnit;

namespace Spring.Data
{
	/// <summary>
	/// Test AdoTemplate based DAO object.
	/// </summary>
	[TestFixture]
    public class AdoDaoTests : AbstractTransactionalSpringContextTests
	{

	    private TestObjectDao dao;

	    public TestObjectDao TestObjectDao
	    {
	        get { return dao; }
	        set { dao = value; }
	    }
        
        [Test]
        public void ExerciseDao()
        {
            Assert.AreEqual(0, dao.GetCount());
            dao.Create("John", 44);
            Assert.AreEqual(1, dao.GetCountByDelegate());
            dao.Create("Mary", 44);
            dao.Create("Steve", 44);
            dao.Create("George", 33);
            Assert.AreEqual(4, dao.GetCount(), "GetCount()");
            Assert.AreEqual(3, dao.GetCount(33), "GetCount(33)");
            Assert.AreEqual(3, dao.GetCountByAltMethod(33), "GetCountByAltMethod(33)");
            Assert.AreEqual(3, dao.GetCountByCommandSetter(33), "GetCountByCommandSetter(33)");
            Assert.AreEqual(1, dao.GetCount(32, "George"), "GetCount(32, 'George')");

            TestObject to = dao.FindByName("George");
            Assert.IsNotNull(to);
            Assert.AreEqual("George", to.Name);
            Assert.AreEqual(33, to.Age);

            to.Age = 34;
            dao.Update(to);

            TestObject to2 = dao.FindByName("George");
            Assert.AreEqual(34, to2.Age);

            dao.Delete("George");

            TestObject to3 = dao.FindByName("George");
            Assert.IsNull(to3);

        }

        [Ignore("Sanity-Check tests intended for verification of base-class behavior only")]
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

        [Ignore("Sanity-Check tests intended for verification of base-class behavior only")]
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

        [Ignore("Sanity-Check tests intended for verification of base-class behavior only")]
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

        [Ignore("Sanity-Check tests intended for verification of base-class behavior only")]
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

	    #region Overrides of AbstractDependencyInjectionSpringContextTests

	    /// <summary>
	    /// Subclasses must implement this property to return the locations of their
	    /// config files. A plain path will be treated as a file system location.
	    /// </summary>
	    /// <value>An array of config locations</value>
	    protected override string[] ConfigLocations
	    {
            get
            {
                return new string[] { "assembly://Spring.Data.Integration.Tests/Spring.Data/templateTests.xml" };
            }
	    }

        protected override void OnSetUp()
        {
            TestObjectDao.Cleanup();
            base.OnSetUp();
        }

        #endregion
	}
}
