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

using System;
using NUnit.Framework;
using Spring.Context.Attributes;
using Spring.Context.Attributes.TypeFilters;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;
using Spring.Context.Support;


namespace Spring.Context.Config
{
    [TestFixture]
    public class ComponentScanObjectDefinitionParserTypeFilterTests
    {
        private IApplicationContext _applicationContext;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void IncludeRegExpressionFilter()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.TypeScannerTestRegExInclude.xml", GetType()));

            Assert.That(_applicationContext.GetObjectDefinitionNames().Count, Is.EqualTo(6));
            Assert.That(_applicationContext.GetObject("SomeIncludeType1"), Is.Not.Null);
            Assert.That(() => { _applicationContext.GetObject("SomeExcludeType"); }, Throws.Exception.TypeOf<NoSuchObjectDefinitionException>());
        }

        [Test]
        public void IncludeMultipleRegExpressionFilter()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.TypeScannerTestRegExInclude2.xml", GetType()));

            Assert.That(_applicationContext.GetObjectDefinitionNames().Count, Is.EqualTo(8));
            Assert.That(_applicationContext.GetObject("SomeIncludeType1"), Is.Not.Null);
            Assert.That(_applicationContext.GetObject("SomeIncludeType2"), Is.Not.Null);
            Assert.That(() => { _applicationContext.GetObject("SomeExcludeType"); }, Throws.Exception.TypeOf<NoSuchObjectDefinitionException>());
        }

        [Test]
        public void ExcludeRegExpressionFilter()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.TypeScannerTestRegExExclude.xml", GetType()));

            Assert.That(_applicationContext.GetObjectDefinitionNames().Count, Is.EqualTo(8));
            Assert.That(_applicationContext.GetObject("SomeIncludeType1"), Is.Not.Null);
            Assert.That(() => { _applicationContext.GetObject("SomeExcludeType"); }, Throws.Exception.TypeOf<NoSuchObjectDefinitionException>());
        }

        [Test]
        public void IncludeAttributeExpressionFilter()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.TypeScannerTestAttributeInclude.xml", GetType()));

            Assert.That(_applicationContext.GetObjectDefinitionNames().Count, Is.EqualTo(6));
            Assert.That(_applicationContext.GetObject("SomeIncludeType2"), Is.Not.Null);
            Assert.That(() => { _applicationContext.GetObject("SomeExcludeType"); }, Throws.Exception.TypeOf<NoSuchObjectDefinitionException>());
        }

        [Test]
        public void ExcludeAttributeExpressionFilter()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.TypeScannerTestAttributeExclude.xml", GetType()));

            Assert.That(_applicationContext.GetObjectDefinitionNames().Count, Is.EqualTo(8));
            Assert.That(_applicationContext.GetObject("SomeIncludeType2"), Is.Not.Null);
            Assert.That(_applicationContext.GetObject("SomeExcludeType"), Is.Not.Null);
            Assert.That(() => { _applicationContext.GetObject("SomeIncludeType1"); }, Throws.Exception.TypeOf<NoSuchObjectDefinitionException>());
        }

        [Test]
        public void IncludeAssignableExpressionFilter()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.TypeScannerTestAssignableInclude.xml", GetType()));

            Assert.That(_applicationContext.GetObjectDefinitionNames().Count, Is.EqualTo(8));
            Assert.That(_applicationContext.GetObject("SomeIncludeType1"), Is.Not.Null);
            Assert.That(_applicationContext.GetObject("SomeIncludeType2"), Is.Not.Null);
            Assert.That(() => { _applicationContext.GetObject("SomeExcludeType"); }, Throws.Exception.TypeOf<NoSuchObjectDefinitionException>());
        }

        [Test]
        public void ExcludeAssignableExpressionFilter()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.TypeScannerTestAssignableExclude.xml", GetType()));

            Assert.That(_applicationContext.GetObjectDefinitionNames().Count, Is.EqualTo(8));
            Assert.That(_applicationContext.GetObject("SomeIncludeType1"), Is.Not.Null);
            Assert.That(_applicationContext.GetObject("SomeExcludeType"), Is.Not.Null);
            Assert.That(() => { _applicationContext.GetObject("SomeIncludeType2"); }, Throws.Exception.TypeOf<NoSuchObjectDefinitionException>());
        }

        [Test]
        public void IncludeCustomExpressionFilter()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.TypeScannerTestCustomInclude.xml", GetType()));

            Assert.That(_applicationContext.GetObjectDefinitionNames().Count, Is.EqualTo(6));
            Assert.That(_applicationContext.GetObject("SomeIncludeType1"), Is.Not.Null);
            Assert.That(() => { _applicationContext.GetObject("SomeIncludeType2"); }, Throws.Exception.TypeOf<NoSuchObjectDefinitionException>());
        }

        [Test]
        public void ExcludeCustomExpressionFilter()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("ConfigFiles.TypeScannerTestCustomExclude.xml", GetType()));

            Assert.That(_applicationContext.GetObjectDefinitionNames().Count, Is.EqualTo(8));
            Assert.That(_applicationContext.GetObject("SomeIncludeType2"), Is.Not.Null);
            Assert.That(_applicationContext.GetObject("SomeExcludeType"), Is.Not.Null);
            Assert.That(() => { _applicationContext.GetObject("SomeIncludeType1"); }, Throws.Exception.TypeOf<NoSuchObjectDefinitionException>());
        }

    }
}

namespace XmlAssemblyTypeScanner.Test.Include1
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DoNotIncludeAttribute : Attribute
    {
    }

    [Configuration]
    [DoNotInclude]
    public class SomeIncludeConfiguration1 : IFunny
    {
        [ObjectDef]
        public virtual SomeIncludeType1 SomeIncludeType1()
        {
            return new SomeIncludeType1();           
        }
    }

    public class SomeIncludeType1
    {
    }

    public interface IFunny
    {}


    public class TestFilter : ITypeFilter
    {
        public bool Match(Type type)
        {
            return type.Name.Equals("SomeIncludeConfiguration1");
        }
    }

}

namespace XmlAssemblyTypeScanner.Test.Include2
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DoIncludeAttribute : Attribute
    {
    }

    [Configuration]
    [DoInclude]
    public class SomeIncludeConfiguration2 : FunnyAbstract
    {
        public override void Test() { }

        [ObjectDef]
        public virtual SomeIncludeType2 SomeIncludeType2()
        {
            return new SomeIncludeType2();
        }
    }

    public class SomeIncludeType2
    {
    }

    public abstract class FunnyAbstract
    {
        public abstract void Test();
    }
}

namespace XmlAssemblyTypeScanner.Test.Include
{
    [Configuration]
    public class SomeExcludeConfiguration3
    {
        
        [ObjectDef]
        public virtual SomeExcludeType SomeExcludeType()
        {
            return new SomeExcludeType();
        }
    }

    public class SomeExcludeType
    {
    }
}
