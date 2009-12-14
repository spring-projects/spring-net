using System.Collections;
using AopAlliance.Intercept;
using NUnit.Framework;
using Spring.Aop.Support;
using Spring.Context.Support;
using Spring.Objects;
using Spring.Objects.Factory.Support;

namespace Spring.Aop.Framework.AutoProxy
{
    [TestFixture]
    public class DefaultAdvisorAutoProxyCreatorTests
    {
        private class CapturingAdvice : IMethodInterceptor
        {
            public readonly ArrayList CapturedCalls = new ArrayList();

            public object Invoke(IMethodInvocation invocation)
            {
                CapturedCalls.Add(invocation);
                return invocation.Proceed();
            }
        }

        public interface ITestObjectFactoryObject
        {
            ITestObject CreateTestObject();
        }

        public class TestObjectFactoryObject : ITestObjectFactoryObject
        {
            public ITestObject CreateTestObject()
            {
                return new TestObject("TheName", 10);
            }
        }

        [Test]
        public void CanProxyFactoryMethodProducts()
        {
            GenericApplicationContext ctx = new GenericApplicationContext();
            ctx.ObjectFactory.AddObjectPostProcessor(new DefaultAdvisorAutoProxyCreator());

            CapturingAdvice capturingAdvice = new CapturingAdvice();
            ctx.ObjectFactory.RegisterSingleton("logging", new DefaultPointcutAdvisor(TruePointcut.True, capturingAdvice));

            // register "factory" object 
            RootObjectDefinition rod;
            rod = new RootObjectDefinition(typeof(TestObjectFactoryObject));
            ctx.ObjectFactory.RegisterObjectDefinition("test", rod);

            // register product, referencing the factory object
            rod = new RootObjectDefinition(typeof(ITestObject));
            rod.FactoryObjectName = "test";
            rod.FactoryMethodName = "CreateTestObject";
            ctx.ObjectFactory.RegisterObjectDefinition("testProduct", rod);

            ctx.Refresh();

            ITestObjectFactoryObject fo = (ITestObjectFactoryObject) ctx.GetObject("test");
            Assert.IsTrue( AopUtils.IsAopProxy(fo) );
            Assert.AreEqual("CreateTestObject", ((IMethodInvocation)capturingAdvice.CapturedCalls[0]).Method.Name);

            capturingAdvice.CapturedCalls.Clear();
            ITestObject to = (ITestObject)ctx.GetObject("testProduct");
            Assert.IsTrue( AopUtils.IsAopProxy(to) );
            Assert.AreEqual("TheName", to.Name);
            Assert.AreEqual("get_Name", ((IMethodInvocation)capturingAdvice.CapturedCalls[0]).Method.Name);
        }
    }
}