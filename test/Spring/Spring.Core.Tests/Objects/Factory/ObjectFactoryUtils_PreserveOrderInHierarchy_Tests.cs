using NUnit.Framework;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory;

[TestFixture]
public class ObjectFactoryUtils_PreserveOrderInHierarchy_Tests
{
    private readonly Type _expectedtype = typeof(ITestObject);
    private DefaultListableObjectFactory _factory;

    [SetUp]
    public void _TestSetUp()
    {
        DefaultListableObjectFactory parentFactory = new DefaultListableObjectFactory();
        _factory = new DefaultListableObjectFactory(parentFactory);

        RootObjectDefinition rodA = new RootObjectDefinition(_expectedtype);
        RootObjectDefinition rodB = new RootObjectDefinition(_expectedtype);
        RootObjectDefinition rodC = new RootObjectDefinition(_expectedtype);
        RootObjectDefinition rod2A = new RootObjectDefinition(_expectedtype);
        RootObjectDefinition rod2C = new RootObjectDefinition(_expectedtype);

        _factory.RegisterObjectDefinition("objA", rodA);
        _factory.RegisterObjectDefinition("objB", rodB);
        _factory.RegisterObjectDefinition("objC", rodC);

        parentFactory.RegisterObjectDefinition("obj2A", rod2A);
        parentFactory.RegisterObjectDefinition("objB", rodB);
        parentFactory.RegisterObjectDefinition("obj2C", rod2C);
    }

    [Test]
    public void ObjectNamesIncludingAncestorsPreserveOrderOfRegistration()
    {
        var names = ObjectFactoryUtils.ObjectNamesIncludingAncestors(_factory);
        Assert.That(names.Count, Is.EqualTo(5));
        Assert.That(names, Is.EqualTo(new string[] { "objA", "objB", "objC", "obj2A", "obj2C" }));
    }

    [Test]
    public void ObjectNamesForTypeIncludingAncestorsPreserveOrderOfRegistration()
    {
        var names = ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(_factory, _expectedtype);
        Assert.That(names.Count, Is.EqualTo(5));
        Assert.That(names, Is.EqualTo(new string[] { "objA", "objB", "objC", "obj2A", "obj2C" }));
    }

    [Test]
    public void ObjectNamesForTypeIncludingAncestorsPrototypesAndFactoryObjectsPreserveOrderOfRegistration()
    {
        var names = ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(_factory, _expectedtype, false, false);
        Assert.That(names.Count, Is.EqualTo(5));
        Assert.That(names, Is.EqualTo(new string[] { "objA", "objB", "objC", "obj2A", "obj2C" }));
    }
}
