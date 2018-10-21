#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using System.Linq;
using NUnit.Framework;
using Spring.Context.Attributes;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Stereotype;
using Spring.Objects.Factory.Attributes;
using ComponentScan.Qualifier;

namespace Spring.Context.Config
{
    [TestFixture]
    public class ComponentScanObjectDefinitionParserTests
    {
        private XmlApplicationContext _applicationContext;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ScanComponentsAndAddToContext()
        {
            var prefix = "ComponentScan.ScanComponentsAndAddToContext.";
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.ComponentScan1.xml", GetType()));
            var objectDefinitionNames = _applicationContext.ObjectFactory.GetObjectDefinitionNames();

            Assert.That(objectDefinitionNames.Count, Is.EqualTo(5+4));
            Assert.That(_applicationContext.GetObject(prefix + "ComponentImpl"), Is.Not.Null);
            Assert.That(_applicationContext.GetObject(prefix + "ServiceImpl"), Is.Not.Null);
            Assert.That(_applicationContext.GetObject(prefix + "RepositoryImpl"), Is.Not.Null);
            Assert.That(_applicationContext.GetObject(prefix + "ControllerImpl"), Is.Not.Null);
            Assert.That(_applicationContext.GetObject(prefix + "ConfigurationImpl"), Is.Not.Null);
        }

        [Test]
        public void ComponentsUseSpecifiedName()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.ComponentScan2.xml", GetType()));
            var objectDefinitionNames = _applicationContext.ObjectFactory.GetObjectDefinitionNames();

            Assert.That(objectDefinitionNames.Count, Is.EqualTo(5 + 4));
            Assert.That(_applicationContext.GetObject("Component"), Is.Not.Null);
            Assert.That(_applicationContext.GetObject("Service"), Is.Not.Null);
            Assert.That(_applicationContext.GetObject("Repository"), Is.Not.Null);
            Assert.That(_applicationContext.GetObject("Controller"), Is.Not.Null);
            Assert.That(_applicationContext.GetObject("Configuration"), Is.Not.Null);
        }

        [Test]
        public void UseSpecifiedObjectNameGenerator()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.ComponentScan3.xml", GetType()));
            var objectDefinitionNames = _applicationContext.ObjectFactory.GetObjectDefinitionNames();

            Assert.That(objectDefinitionNames.Contains("prototype"), Is.True);
        }

        [Test]
        public void UseWrongObjectNameGeneratorTypeString()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.ComponentScan31.xml", GetType()));
            var objectDefinitionNames = _applicationContext.ObjectFactory.GetObjectDefinitionNames();

            Assert.That(objectDefinitionNames.Contains("prototype"), Is.False);
            Assert.That(objectDefinitionNames.Contains("ComponentScan.NameGenerator.Prototype"), Is.True);
        }
        [Test]
        public void ComponentsLazyLoaded()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.ComponentScan4.xml", GetType()));
            var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("LazyInit");

            Assert.That(objectDefinition.IsLazyInit, Is.True);
        }

        [Test]
        public void ComponentsInDifferentScope()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.ComponentScan4.xml", GetType()));
            var singletonDef = _applicationContext.ObjectFactory.GetObjectDefinition("Singleton");
            var prototypeDef = _applicationContext.ObjectFactory.GetObjectDefinition("Prototype");

            Assert.That(singletonDef.IsSingleton, Is.True);
            Assert.That(singletonDef.Scope, Is.EqualTo(ObjectScope.Singleton.ToString().ToLower()));

            Assert.That(prototypeDef.IsSingleton, Is.False);
            Assert.That(prototypeDef.Scope, Is.EqualTo(ObjectScope.Prototype.ToString().ToLower()));
        }

        [Test]
        public void ComponentsUseDefaults()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.ComponentScan5.xml", GetType()));
            var prototypeDef = _applicationContext.ObjectFactory.GetObjectDefinition("Prototype");

            Assert.That(prototypeDef.IsLazyInit, Is.True);
        }

        [Test]
        public void ComponentsUseDefaultAutoWire()
        {
           _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.ComponentScan5.xml", GetType()));
           var prototypeDef = _applicationContext.ObjectFactory.GetObjectDefinition("Prototype");

           Assert.That(prototypeDef.AutowireMode == AutoWiringMode.ByName);
        }

        [Test]
        public void ComponentWithQualifier()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.ComponentScan6.xml", GetType()));
            var objectDef = _applicationContext.ObjectFactory.GetObjectDefinition("Prototype") as ScannedGenericObjectDefinition;

            Assert.That(objectDef.HasQualifier(typeof(QualifierAttribute).Name), Is.True);

            var attr = objectDef.GetQualifier(typeof (QualifierAttribute).Name).GetAttribute(AutowireCandidateQualifier.VALUE_KEY);
            Assert.That(attr, Is.EqualTo("action"));
        }

        [Test]
        public void ComponentWithQualifierAttributes()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.ComponentScan6.xml", GetType()));
            var objectDef = _applicationContext.ObjectFactory.GetObjectDefinition("Attribute") as ScannedGenericObjectDefinition;
            var qualifier = objectDef.GetQualifier(typeof (MyQualifier).Name);

            Assert.That(qualifier, Is.Not.Null);

            var attr = qualifier.GetMetadataAttribute("Foo");
            Assert.That(attr, Is.Not.Null);
            Assert.That(attr.Value, Is.EqualTo("Funny"));
        }

        [Test]
        public void DontRegisterAttributeConfig()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.ComponentScanAttributeConfigFalse.xml", GetType()));
            var objectDefintionNames = _applicationContext.ObjectFactory.GetObjectDefinitionNames();

            Assert.That(objectDefintionNames.Count, Is.EqualTo(0));
            Assert.That(objectDefintionNames.Contains(AttributeConfigUtils.CONFIGURATION_ATTRIBUTE_PROCESSOR_OBJECT_NAME), Is.False);
            Assert.That(objectDefintionNames.Contains(AttributeConfigUtils.AUTOWIRED_ATTRIBUTE_PROCESSOR_OBJECT_NAME), Is.False);
            Assert.That(objectDefintionNames.Contains(AttributeConfigUtils.REQUIRED_ATTRIBUTE_PROCESSOR_OBJECT_NAME), Is.False);
            Assert.That(objectDefintionNames.Contains(AttributeConfigUtils.INITDESTROY_ATTRIBUTE_PROCESSOR_OBJECT_NAME), Is.False);
        }

        [Test]
        public void RegisterAttributeConfig()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.ComponentScanAttributeConfigTrue.xml", GetType()));
            var objectDefintionNames = _applicationContext.ObjectFactory.GetObjectDefinitionNames();

            Assert.That(objectDefintionNames.Count, Is.EqualTo(4));
            Assert.That(objectDefintionNames.Contains(AttributeConfigUtils.CONFIGURATION_ATTRIBUTE_PROCESSOR_OBJECT_NAME), Is.True);
            Assert.That(objectDefintionNames.Contains(AttributeConfigUtils.AUTOWIRED_ATTRIBUTE_PROCESSOR_OBJECT_NAME), Is.True);
            Assert.That(objectDefintionNames.Contains(AttributeConfigUtils.REQUIRED_ATTRIBUTE_PROCESSOR_OBJECT_NAME), Is.True);
            Assert.That(objectDefintionNames.Contains(AttributeConfigUtils.INITDESTROY_ATTRIBUTE_PROCESSOR_OBJECT_NAME), Is.True);
        }

    }
}

namespace ComponentScan.ScanComponentsAndAddToContext
{
    public interface IFoo
    {
    }

    [Component]
    public class ComponentImpl : IFoo
    {
    }

    [Service]
    public class ServiceImpl : IFoo
    {
    }

    [Repository]
    public class RepositoryImpl : IFoo
    {
    }

    [Controller]
    public class ControllerImpl : IFoo
    {
    }

    [Configuration]
    public class ConfigurationImpl : IFoo
    {
    }
}

namespace ComponentScan.ComponentsUseSpecifiedName
{
    public interface IFoo
    {
    }

    [Component("Component")]
    public class ComponentImpl : IFoo
    {
    }

    [Service("Service")]
    public class ServiceImpl : IFoo
    {
    }

    [Repository("Repository")]
    public class RepositoryImpl : IFoo
    {
    }

    [Controller("Controller")]
    public class ControllerImpl : IFoo
    {
    }

    [Configuration("Configuration")]
    public class ConfigurationImpl : IFoo
    {
    }
}

namespace ComponentScan.ComponentsAttributeLoad
{
    public interface IFoo
    {
    }

    [Component("LazyInit")]
    [Lazy]
    public class LazyImpl : IFoo
    {
    }

    [Component("Singleton")]
    [Scope(ObjectScope.Singleton)]
    public class SingletonImpl : IFoo
    {
    }

    [Component("Prototype")]
    [Scope(ObjectScope.Prototype)]
    public class PrototypeImpl : IFoo
    {
    }
}

namespace ComponentScan.ComponentsUseDefaults
{
    public interface IFoo
    {
    }
    
    [Component("Prototype")]
    public class PrototypeImpl : IFoo
    {
    }
}

namespace ComponentScan.Qualifier
{
    public interface IFoo
    {
    }

    public class MyQualifier : QualifierAttribute
    {
        public string Foo { get; set; }
    }

    [Component("Prototype")]
    [Qualifier("action")]
    public class PrototypeImpl : IFoo
    {
    }

    [Component("Attribute")]
    [MyQualifier(Foo="Funny")]
    public class QualifierAttributeImpl : IFoo
    {
    }

}

namespace ComponentScan.NameGenerator
{
    public interface IFoo
    {
    }

    public class MyGenerator : IObjectNameGenerator
    {
        public string GenerateObjectName(IObjectDefinition definition, IObjectDefinitionRegistry registry)
        {
            string typeName = definition.ObjectType.Name;
            return typeName.ToLower();
        }
    }

    [Component]
    public class Prototype : IFoo
    {
    }
}
