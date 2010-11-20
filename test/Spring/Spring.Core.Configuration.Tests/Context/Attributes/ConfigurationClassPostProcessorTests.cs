using System;
using NUnit.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory.Support;
using Spring.Context.Attributes;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Xml;

namespace Spring.Context.Attributes
{
    [TestFixture]
    public class ConfigurationClassPostProcessorTests
    {
        private GenericApplicationContext _ctx;

        [SetUp]
        public void _SetUp()
        {
            _ctx = new GenericApplicationContext();

            var configDefinitionBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(TheConfigurationClass));
            _ctx.RegisterObjectDefinition(configDefinitionBuilder.ObjectDefinition.ObjectTypeName, configDefinitionBuilder.ObjectDefinition);

            var postProcessorDefintionBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(ConfigurationClassPostProcessor));
            _ctx.RegisterObjectDefinition(postProcessorDefintionBuilder.ObjectDefinition.ObjectTypeName, postProcessorDefintionBuilder.ObjectDefinition);

            Assert.That(_ctx.ObjectDefinitionCount, Is.EqualTo(2));

            _ctx.Refresh();
        }

        [Test]
        public void Can_Assign_Init_And_Destroy_Methods()
        {
            IObjectDefinition def = _ctx.GetObjectDefinition(typeof(ObjectWithInitAndDestroyMethods).Name);

            Assert.That(def, Is.Not.Null);
            Assert.That(def.InitMethodName, Is.EqualTo("CallToInit"));
            Assert.That(def.DestroyMethodName, Is.EqualTo("CallToDestroy"));
        }

        [Test]
        public void Can_Import_Configurations_From_Additional_Classes()
        {
            Assert.That(_ctx.GetObject(typeof(AnImportedType).Name), Is.Not.Null);
        }

        [Test]
        public void Can_Respect_Assigned_Aliases()
        {
            var firstObject = _ctx["TheFirstAlias"];
            var secondObject = _ctx["TheSecondAlias"];
            Assert.That(firstObject, Is.InstanceOf<ObjectWithAnAlias>());
            Assert.That(secondObject, Is.InstanceOf<ObjectWithAnAlias>());
        }

        [Test]
        public void Can_Respect_Assigned_Name()
        {
            var result = _ctx["TheName"];
            Assert.That(result, Is.InstanceOf<SingleNamedObject>());
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

        [Test]
        public void Can_Respect_Lazy_Attribute()
        {
            Assert.That(_ctx.GetObjectDefinition(typeof(ImplicitLazyInitObject).Name).IsLazyInit, Is.True);
            Assert.That(_ctx.GetObjectDefinition(typeof(ExplicitLazyInitObject).Name).IsLazyInit, Is.True);
            Assert.That(_ctx.GetObjectDefinition(typeof(ExplicitNonLazyInitObject).Name).IsLazyInit, Is.False);
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
        public void Can_Respect_Imported_Resources()
        {
            Assert.That(_ctx["xmlRegisteredObject"], Is.Not.Null);
        }
    }

    public class ObjectWithInitAndDestroyMethods
    {
        public void CallToDestroy() { }
        public void CallToInit() { }
    }




    [Configuration]
    [ImportResource("assembly://Spring.Core.Configuration.Tests/Spring.Context.Attributes/ObjectDefinitions.xml", DefinitionReader = typeof(XmlObjectDefinitionReader))]
    public class TheImportedConfigurationClass
    {
        [Definition]
        public virtual AnImportedType AnImportedType()
        {
            return new AnImportedType();
        }
    }

    [Configuration]
    [Import(new Type[] { typeof(TheImportedConfigurationClass) })]
    public class TheConfigurationClass
    {
        [Definition(Names = "TheName")]
        public virtual SingleNamedObject NamedObject()
        {
            return new SingleNamedObject();
        }

        [Definition(DestroyMethod = "CallToDestroy", InitMethod = "CallToInit")]
        public virtual ObjectWithInitAndDestroyMethods ObjectWithInitAndDestroyMethods()
        {
            return new ObjectWithInitAndDestroyMethods();
        }

        [Definition(Names = "TheFirstAlias,TheSecondAlias")]
        public virtual ObjectWithAnAlias ObjectWithAnAlias()
        {
            return new ObjectWithAnAlias();
        }

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

        [Definition]
        [Lazy]
        public virtual ImplicitLazyInitObject ImplicitLazyInitObject()
        {
            return new ImplicitLazyInitObject();
        }

        [Definition]
        [Lazy(true)]
        public virtual ExplicitLazyInitObject ExplicitLazyInitObject()
        {
            return new ExplicitLazyInitObject();
        }

        [Definition]
        [Lazy(false)]
        public virtual ExplicitNonLazyInitObject ExplicitNonLazyInitObject()
        {
            return new ExplicitNonLazyInitObject();
        }

    }

    public class TypeRegisteredInXml { }

    public class AnImportedType { }

    public class ImplicitLazyInitObject { }

    public class ExplicitLazyInitObject { }

    public class ExplicitNonLazyInitObject { }

    public class ObjectWithAnAlias { }

    public class SingleNamedObject { }

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
