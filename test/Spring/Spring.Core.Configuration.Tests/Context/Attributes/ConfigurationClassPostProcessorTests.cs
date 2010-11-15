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

        [SetUp]
        public void _SetUp()
        {
            _ctx = new GenericApplicationContext();

            var builder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(TheConfigurationClass));
            _ctx.RegisterObjectDefinition("whoCares", builder.ObjectDefinition);

            var b2 = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(ConfigurationClassPostProcessor));
            _ctx.RegisterObjectDefinition("ccpp", b2.ObjectDefinition);

            _ctx.Refresh();
        }

        [Test]
        public void Can_Parse_and_Register_Defintions()
        {
            Assert.That(_ctx.ObjectDefinitionCount, Is.EqualTo(4));
        }

        [Test]
        public void Can_Retreive_Actual_Objects_From_Context()
        {
            Assert.That(_ctx[typeof(SingletonParent).Name], Is.TypeOf<SingletonParent>());
            Assert.That(_ctx[typeof(PrototypeChild).Name], Is.TypeOf<PrototypeChild>());
        }

        [Test]
        public void Can_Satisfy_Dependencies_Of_Objects()
        {
            Assert.That(((SingletonParent)_ctx[typeof(SingletonParent).Name]).Child, Is.Not.Null);
        }

        [Test]
        public void Can_Respect_Default_Singleton_Scope()
        {
            var firstObject = (SingletonParent)_ctx[typeof(SingletonParent).Name];
            var secondObject = (SingletonParent)_ctx[typeof(SingletonParent).Name];

            Assert.That(firstObject, Is.SameAs(secondObject));
        }

        [Test]
        public void Can_Respect_Explicit_PrototypeScope()
        {
            var firstObject = (PrototypeChild)_ctx[typeof(PrototypeChild).Name];
            var secondObject = (PrototypeChild)_ctx[typeof(PrototypeChild).Name];

            Assert.That(firstObject, Is.Not.SameAs(secondObject));
        }

    }

    [Configuration]
    public class TheConfigurationClass
    {
        [Definition]
        [Scope(ObjectScope.Prototype)]
        public virtual PrototypeChild PrototypeChild()
        {
            return new PrototypeChild();
        }

        [Definition]
        public virtual SingletonParent SingletonParent()
        {
            return new SingletonParent(PrototypeChild());
        }

    }

    public class SingletonParent
    {
        private PrototypeChild _child;

        public SingletonParent(PrototypeChild child)
        {
            _child = child;
        }

        public PrototypeChild Child
        {
            get
            {
                return _child;
            }
        }

    }

    public class PrototypeChild { }
}
