using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Objects;
using Spring.Testing.NUnit;

namespace Spring.Data;

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
        Assert.That(dao.GetCount(), Is.EqualTo(0));
        dao.Create("John", 44);
        Assert.That(dao.GetCountByDelegate(), Is.EqualTo(1));
        dao.Create("Mary", 44);
        dao.Create("Steve", 44);
        dao.Create("George", 33);
        Assert.That(dao.GetCount(), Is.EqualTo(4), "GetCount()");
        Assert.That(dao.GetCount(33), Is.EqualTo(3), "GetCount(33)");
        Assert.That(dao.GetCountByAltMethod(33), Is.EqualTo(3), "GetCountByAltMethod(33)");
        Assert.That(dao.GetCountByCommandSetter(33), Is.EqualTo(3), "GetCountByCommandSetter(33)");
        Assert.That(dao.GetCount(32, "George"), Is.EqualTo(1), "GetCount(32, 'George')");

        TestObject to = dao.FindByName("George");
        Assert.That(to, Is.Not.Null);
        Assert.That(to.Name, Is.EqualTo("George"));
        Assert.That(to.Age, Is.EqualTo(33));

        to.Age = 34;
        dao.Update(to);

        TestObject to2 = dao.FindByName("George");
        Assert.That(to2.Age, Is.EqualTo(34));

        dao.Delete("George");

        TestObject to3 = dao.FindByName("George");
        Assert.That(to3, Is.Null);
    }

    [Ignore("Sanity-Check tests intended for verification of base-class behavior only")]
    [Test]
    public void SimpleCreate()
    {
        IApplicationContext ctx =
            new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/nativeAdoTests.xml");
        Assert.That(ctx, Is.Not.Null);
        ITestObjectDao dao = (ITestObjectDao) ctx["testObjectDao"];
        Assert.That(dao, Is.Not.Null);
        dao.Create("John", 44);
    }

    [Ignore("Sanity-Check tests intended for verification of base-class behavior only")]
    [Test]
    public void SimpleDao()
    {
        IApplicationContext ctx =
            new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/templateTests.xml");
        Assert.That(ctx, Is.Not.Null);
        TestObjectDao dao = (TestObjectDao) ctx["testObjectDao"];
        Assert.That(dao, Is.Not.Null);
        Assert.That(dao.GetCount(), Is.EqualTo(10));

        Assert.That(dao.GetCount(33), Is.EqualTo(3));
        Assert.That(dao.GetCountByAltMethod(33), Is.EqualTo(3));
        Assert.That(dao.GetCountByCommandSetter(33), Is.EqualTo(3));
        Assert.That(dao.GetCount(33, "George"), Is.EqualTo(2));
    }

    [Ignore("Sanity-Check tests intended for verification of base-class behavior only")]
    [Test]
    public void SimpleDao2()
    {
        IApplicationContext ctx =
            new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/templateTests.xml");
        Assert.That(ctx, Is.Not.Null);
        TestObjectDao dao = (TestObjectDao) ctx["testObjectDao"];
        Assert.That(dao, Is.Not.Null);
        Assert.That(dao.GetCountByDelegate(), Is.EqualTo(1));
    }

    [Ignore("Sanity-Check tests intended for verification of base-class behavior only")]
    [Test]
    public void DaoOperations()
    {
        IApplicationContext ctx =
            new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/templateTests.xml");
        Assert.That(ctx, Is.Not.Null);
        TestObjectDao dao = (TestObjectDao) ctx["testObjectDao"];
        dao.Create("George", 33);
        TestObject to = dao.FindByName("George");
        Assert.That(to, Is.Not.Null);
        Assert.That(to.Name, Is.EqualTo("George"));
        Assert.That(to.Age, Is.EqualTo(33));

        to.Age = 34;
        dao.Update(to);

        TestObject to2 = dao.FindByName("George");
        Assert.That(to2.Age, Is.EqualTo(34));

        dao.Delete("George");

        TestObject to3 = dao.FindByName("George");
        Assert.That(to3, Is.Null);
    }

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
}