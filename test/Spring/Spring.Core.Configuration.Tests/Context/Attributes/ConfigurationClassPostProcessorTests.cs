using System;
using NUnit.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory.Support;
using Spring.Context.Attributes;

namespace Spring.Context.Annotation
{
    [TestFixture]
    public class ConfigurationClassPostProcessorTests
    {
        private GenericApplicationContext _ctx;

        private ConfigurationClassPostProcessor _postProcessor;

        [SetUp]
        public void _SetUp()
        {
            _ctx = new GenericApplicationContext();

            var builder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(ObjectDefinitions));
            _ctx.RegisterObjectDefinition("whoCares", builder.ObjectDefinition);

            _postProcessor = new ConfigurationClassPostProcessor();
            _postProcessor.PostProcessObjectDefinitionRegistry(_ctx);
        }

        [Test]
        public void CanRegisterDefintions()
        {
            Assert.That(_ctx.ObjectDefinitionCount, Is.EqualTo(3));
        }

        [Test]
        public void CanRetreiveActualObjectsFromContext()
        {
            Assert.That(_ctx[typeof(Parent).Name], Is.TypeOf<Parent>());
            Assert.That(_ctx[typeof(Child).Name], Is.TypeOf<Child>());
        }

        [Test]
        public void CanSatisfyDependenciesOfObjects()
        {
            Assert.That(((Parent)_ctx[typeof(Parent).Name]).Child, Is.Not.Null);
        }

    }

    [Configuration]
    public class ObjectDefinitions
    {
        [Definition]
        public Parent Parent()
        {
            return new Parent(Child());
        }

        [Definition]
        public Child Child()
        {
            return new Child();
        }
    }

    public class Parent
    {
        private Child _child;

        public Parent(Child child)
        {
            _child = child;
        }

        public Child Child
        {
            get
            {
                return _child;
            }
        }

    }
    public class Child { }
}
