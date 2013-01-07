#region License

/*
 * Copyright © 2010-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using NUnit.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Xml;

namespace Spring.Context.Attributes
{

    public abstract class AbstractConfigurationClassPostProcessorTests
    {
        protected AbstractApplicationContext _ctx;

        [SetUp]
        public void _SetUp()
        {
            SingletonParent.InstanceCount = 0;
            SingletonChild.InstanceCount = 0;
            PrototypeParent.InstanceCount = 0;
            PrototypeChild.InstanceCount = 0;
            CreateApplicationContext();
        }


        protected abstract void CreateApplicationContext();


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
            var firstObject = (SingletonChild)_ctx[typeof(SingletonChild).Name];
            var secondObject = (SingletonChild)_ctx[typeof(SingletonChild).Name];

            Assert.That(SingletonChild.InstanceCount, Is.EqualTo(1));
            Assert.That(firstObject, Is.SameAs(secondObject));
        }

        [Test]
        public void Can_Respect_Default_Singleton_Scope_With_Explicit_Prototype_Dependency()
        {
            var firstObject = (SingletonParent)_ctx[typeof(SingletonParent).Name];
            var secondObject = (SingletonParent)_ctx[typeof(SingletonParent).Name];

            Assert.That(SingletonParent.InstanceCount, Is.EqualTo(1));
            //Assert.That(PrototypeChild.InstanceCount, Is.EqualTo(2));  // Requires scoped proxies
            Assert.That(firstObject, Is.SameAs(secondObject));
        }

        [Test]
        public void Can_Respect_Explicit_Prototype_Scope()
        {
            var firstObject = (PrototypeChild)_ctx[typeof(PrototypeChild).Name];
            var secondObject = (PrototypeChild)_ctx[typeof(PrototypeChild).Name];

            Assert.That(PrototypeChild.InstanceCount, Is.EqualTo(3)); // One instance used by SingletonParent
            Assert.That(firstObject, Is.Not.SameAs(secondObject));
        }

        [Test]
        public void Can_Respect_Explicit_Prototype_Scope_With_Default_Singleton_Dependency()
        {
            var firstObject = (PrototypeParent)_ctx[typeof(PrototypeParent).Name];
            var secondObject = (PrototypeParent)_ctx[typeof(PrototypeParent).Name];

            Assert.That(PrototypeParent.InstanceCount, Is.EqualTo(2));
            Assert.That(SingletonChild.InstanceCount, Is.EqualTo(1));
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
    [ImportResource("assembly://Spring.Core.Tests/Spring.Context.Attributes/ObjectDefinitions.xml", DefinitionReader = typeof(XmlObjectDefinitionReader))]
    [ImportResource("assembly://Spring.Core.Tests/Spring.Context.Attributes/ObjectDefinitionsTwo.xml")]
    public class TheImportedConfigurationClass
    {
        [ObjectDef]
        public virtual AnImportedType AnImportedType()
        {
            return new AnImportedType();
        }
    }

    [Configuration]
    [Import(typeof(TheImportedConfigurationClass))]
    public class TheConfigurationClass
    {
        [ObjectDef(Names = "TheName")]
        public virtual SingleNamedObject NamedObject()
        {
            return new SingleNamedObject();
        }

        [ObjectDef(DestroyMethod = "CallToDestroy", InitMethod = "CallToInit")]
        public virtual ObjectWithInitAndDestroyMethods ObjectWithInitAndDestroyMethods()
        {
            return new ObjectWithInitAndDestroyMethods();
        }

        [ObjectDef(Names = "TheFirstAlias,TheSecondAlias")]
        public virtual ObjectWithAnAlias ObjectWithAnAlias()
        {
            return new ObjectWithAnAlias();
        }

        [ObjectDef]
        [Scope(ObjectScope.Prototype)]
        public virtual PrototypeParent PrototypeParent()
        {
            return new PrototypeParent(SingletonChild());
        }

        [ObjectDef]
        [Scope(ObjectScope.Prototype)]
        public virtual PrototypeChild PrototypeChild()
        {
            return new PrototypeChild();
        }

        [ObjectDef]
        public virtual SingletonParent SingletonParent()
        {
            return new SingletonParent(PrototypeChild());
        }

        [ObjectDef]
        public virtual SingletonChild SingletonChild()
        {
            return new SingletonChild();
        }

        [ObjectDef]
        [Lazy]
        public virtual ImplicitLazyInitObject ImplicitLazyInitObject()
        {
            return new ImplicitLazyInitObject();
        }

        [ObjectDef]
        [Lazy(true)]
        public virtual ExplicitLazyInitObject ExplicitLazyInitObject()
        {
            return new ExplicitLazyInitObject();
        }

        [ObjectDef]
        [Lazy(false)]
        public virtual ExplicitNonLazyInitObject ExplicitNonLazyInitObject()
        {
            return new ExplicitNonLazyInitObject();
        }

    }


    [Configuration]
    public class DerivedConfiguration : BaseConfigurationClass
    {
        [ObjectDef]
        public virtual TestObject DerivedDefinition()
        {
            return new TestObject(BaseDefinition());
        }
    }

    public class BaseConfigurationClass
    {
        [ObjectDef]
        public virtual string BaseDefinition()
        {
            return Guid.NewGuid().ToString();
        }
    }

    public class TypeRegisteredInXml { }
    
    public class TypeRegisteredInXmlTwo { }
    
    public class AnImportedType { }

    public class ImplicitLazyInitObject { }

    public class ExplicitLazyInitObject { }

    public class ExplicitNonLazyInitObject { }

    public class ObjectWithAnAlias { }

    public class SingleNamedObject { }

    public class SingletonParent
    {
        public static int InstanceCount = 0;
        private PrototypeChild _child;

        public SingletonParent(PrototypeChild child)
        {
            InstanceCount++;
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

    public class SingletonChild
    {
        public static int InstanceCount = 0;

        public SingletonChild()
        {
            InstanceCount++;
        }
    }

    public class PrototypeParent
    {
        public static int InstanceCount = 0;
        private SingletonChild _child;

        public PrototypeParent(SingletonChild child)
        {
            InstanceCount++;
            _child = child;
        }

        public SingletonChild Child
        {
            get
            {
                return _child;
            }
        }
    }

    public class PrototypeChild
    {
        public static int InstanceCount = 0;

        public PrototypeChild()
        {
            InstanceCount++;
        }
    }

    public class TestObject
    {
        private readonly string _value;

        public TestObject(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }
}
