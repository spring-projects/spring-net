using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;

namespace Spring.Objects.Factory.Support
{
    [TestFixture]
    public class DelegateInvokingFactoryObjectTests
    {
        [SetUp]
        public void _TestSetUp()
        {
            var context = new GenericApplicationContext();
            ContextRegistry.Clear();
            ContextRegistry.RegisterContext(context);
        }

        private void RegisterDelegatingFactoryWithContext(bool buildSingletonObjectsWhenInvoked)
        {
            var targetBuilder = new ThingThatBuildsTargets();
            var factory = new DelegateInvokingFactoryObject<TargetToBuild>(targetBuilder.BuildTarget, buildSingletonObjectsWhenInvoked);

            Assume.That(ContextRegistry.GetContext(), Is.InstanceOf<IConfigurableApplicationContext>(), "test requires a registered context that implements IConfigurableApplicationContext!");

            var ctx = (IConfigurableApplicationContext)ContextRegistry.GetContext();
            ctx.ObjectFactory.RegisterSingleton("target", factory);
        }

        [Test]
        public void CanReturnSingletonObjects()
        {
            RegisterDelegatingFactoryWithContext(true);

            var targetObject1 = ContextRegistry.GetContext().GetObject<TargetToBuild>();
            var targetObject2 = ContextRegistry.GetContext().GetObject<TargetToBuild>();

            Assert.That(targetObject1, Is.SameAs(targetObject2));
            Assert.That(targetObject1.Counter, Is.EqualTo(targetObject2.Counter));
        }

        [Test]
        public void CanReturnProtoypeObjects()
        {
            RegisterDelegatingFactoryWithContext(false);

            var targetObject1 = ContextRegistry.GetContext().GetObject<TargetToBuild>();
            var targetObject2 = ContextRegistry.GetContext().GetObject<TargetToBuild>();

            Assert.That(targetObject1, Is.Not.SameAs(targetObject2));
            Assert.That(targetObject1.Counter, Is.Not.EqualTo(targetObject2.Counter));
        }

    }

    public class ThingThatBuildsTargets
    {
        //since the IFactoryObject impl that contains this is registered w context as singleton,
        // this counter is effectively static (sort of! <g>) and will be incremented on each call to build the TARGET instance
        private int _counter;

        public TargetToBuild BuildTarget()
        {
            //create and return a new TARGET instance with the incremented counter value to show its working
            return new TargetToBuild(_counter++);
        }
    }

    //the type we're trying to tell the container we want to be in charge of creating
    public class TargetToBuild
    {
        public TargetToBuild(int counter)
        {
            Counter = counter;
        }

        public int Counter { get; private set; }
    }
}