using NUnit.Framework;
using Spring.Aop.Advice;
using Spring.Context.Support;
using Spring.Objects;
using Spring.Objects.Factory.Xml;

namespace Spring.Aop.Framework.DynamicProxy;

[TestFixture]
public class IsAopProxyTests
{
    private TestObject _target;

    [SetUp]
    public void SetUp()
    {
        _target = new TestObject("Michael", 23);
    }

    [Test]
    public void TargetIsNotAProxy()
    {
        Assert.That(AopUtils.IsAopProxy(_target), Is.False);
        Assert.That(AopUtils.IsInheritanceAopProxy(_target), Is.False);
        Assert.That(AopUtils.IsInheritanceAopProxyType(_target.GetType()), Is.False);
    }

    [Test]
    public void IsCompositionProxy()
    {
        var pf = new ProxyFactory(typeof(ITestObject), new DebugAdvice());
        pf.Target = _target;
        Assert.That(pf.ProxyTargetType, Is.False);

        var proxy = (ITestObject) pf.GetProxy();

        Assert.That(AopUtils.IsCompositionAopProxy(proxy), Is.True);
        Assert.That(AopUtils.IsAopProxy(proxy), Is.True);
        Assert.That(proxy, Is.Not.InstanceOf<TestObject>());
    }

    [Test]
    public void IsDecoratorProxy()
    {
        var pf = new ProxyFactory(new DebugAdvice());
        pf.Target = _target;
        pf.ProxyTargetType = true;

        var proxy = (TestObject) pf.GetProxy();
        Assert.That(AopUtils.IsDecoratorAopProxy(proxy), Is.True);
        Assert.That(AopUtils.IsAopProxy(proxy), Is.True);
    }

    [Test]
    public void IsInheritanceBasedProxyTypeReturnsFalseForNull()
    {
        Assert.That(AopUtils.IsInheritanceAopProxyType(null), Is.False);
    }

    [Test]
    public void IsInheritanceBasedProxyReturnsFalseForNull()
    {
        Assert.That(AopUtils.IsInheritanceAopProxy(null), Is.False);
    }

    [Test]
    public void IsInheritanceBasedProxy()
    {
        using (var ctx = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("IsAopProxyTests.xml", this.GetType())))
        {
            var proxy = (TestObject) ctx["michael"];
            Assert.That(proxy.Name, Is.EqualTo("Michael"));

            Assert.That(AopUtils.IsInheritanceAopProxyType(proxy.GetType()), Is.True);
            Assert.That(AopUtils.IsAopProxyType(proxy.GetType()), Is.True);

            Assert.That(AopUtils.IsInheritanceAopProxy(proxy), Is.True);
            Assert.That(AopUtils.IsAopProxy(proxy), Is.True);
        }
    }
}
