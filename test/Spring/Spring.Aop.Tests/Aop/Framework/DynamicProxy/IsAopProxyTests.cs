using NUnit.Framework;
using Spring.Aop.Advice;
using Spring.Context.Support;
using Spring.Objects;
using Spring.Objects.Factory.Xml;

namespace Spring.Aop.Framework.DynamicProxy
{
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
            Assert.False(AopUtils.IsAopProxy(_target));
            Assert.False(AopUtils.IsInheritanceAopProxy(_target));
            Assert.False(AopUtils.IsInheritanceAopProxyType(_target.GetType()));
        }

        [Test]
        public void IsCompositionProxy()
        {
            var pf = new ProxyFactory(typeof(ITestObject), new DebugAdvice());
            pf.Target = _target;
            Assert.False(pf.ProxyTargetType);

            var proxy = (ITestObject)pf.GetProxy();

            Assert.True(AopUtils.IsCompositionAopProxy(proxy));
            Assert.True(AopUtils.IsAopProxy(proxy));
            Assert.IsNotInstanceOf<TestObject>(proxy);
        }

        [Test]
        public void IsDecoratorProxy()
        {
            var pf = new ProxyFactory(new DebugAdvice());
            pf.Target = _target;
            pf.ProxyTargetType = true;

            var proxy = (TestObject)pf.GetProxy();
            Assert.True(AopUtils.IsDecoratorAopProxy(proxy));
            Assert.True(AopUtils.IsAopProxy(proxy));
        }

        [Test]
        public void IsInheritanceBasedProxyTypeReturnsFalseForNull()
        {
            Assert.False(AopUtils.IsInheritanceAopProxyType(null));
        }
        
        [Test]
        public void IsInheritanceBasedProxyReturnsFalseForNull()
        {
            Assert.False(AopUtils.IsInheritanceAopProxy(null));
        }

        [Test]
        public void IsInheritanceBasedProxy()
        {
            using (var ctx = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("IsAopProxyTests.xml", this.GetType())))
            {
                var proxy = (TestObject)ctx["michael"];
                Assert.AreEqual("Michael", proxy.Name);
                
                Assert.True(AopUtils.IsInheritanceAopProxyType(proxy.GetType()));
                Assert.True(AopUtils.IsAopProxyType(proxy.GetType()));

                Assert.True(AopUtils.IsInheritanceAopProxy(proxy));
                Assert.True(AopUtils.IsAopProxy(proxy));
            }
        }
    }
}