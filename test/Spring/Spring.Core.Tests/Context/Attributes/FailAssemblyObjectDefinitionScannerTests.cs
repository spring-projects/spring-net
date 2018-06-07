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
using Spring.Objects.Factory;
using Spring.Objects.Factory.Parsing;
using Spring.Objects.Factory.Support;

namespace Spring.Context.Attributes
{
    [Explicit("Interferes with other fixtures")]
    public class FailAssemblyObjectDefinitionScannerTests
    {
        #region Setup/Teardown

        [SetUp]
        public void _SetUp()
        {
            _scanner = new AssemblyObjectDefinitionScanner();
            _context = new CodeConfigApplicationContext();
        }

        #endregion

        private void ScanForAndRegisterSingleType(Type type)
        {
            _scanner.WithIncludeFilter(t => t.Name == type.Name);
            _scanner.ScanAndRegisterTypes(_context.DefaultListableObjectFactory);
            AttributeConfigUtils.RegisterAttributeConfigProcessors((IObjectDefinitionRegistry)_context.ObjectFactory);
        }

        private CodeConfigApplicationContext _context;
        private AssemblyObjectDefinitionScanner _scanner;

        [Test]
        public void Can_Ignore_Abstract_Configuration_Types()
        {
            ScanForAndRegisterSingleType(typeof(ConfigurationClassThatIsAbstract));
            Assert.That(_context.GetObjectNamesForType(typeof(ConfigurationClassThatIsAbstract)).Count, Is.EqualTo(0), "Abstract Type erroneously registered with the Context.");
        }

        [Test]
        public void Can_Prevent_Methods_With_Parameters()
        {
            ScanForAndRegisterSingleType(typeof(ConfigurationClassWithMethodHavingParameters));
            Assert.Throws<ObjectDefinitionParsingException>(_context.Refresh);
        }

        [Test]
        public void Can_Prevent_Static_Methods()
        {
            ScanForAndRegisterSingleType(typeof(ConfigurationClassWithStaticMethod));
            Assert.Throws<ObjectDefinitionParsingException>(_context.Refresh);
        }

        [Test]
        public void Can_Prevent_Non_Virtual_Methods()
        {
            ScanForAndRegisterSingleType(typeof(ConfigurationClassWithNonVirtualMethod));
            Assert.Throws<ObjectDefinitionParsingException>(_context.Refresh);
        }

        [Test]
        public void Can_Prevent_Sealed_Configuration_Types()
        {
            ScanForAndRegisterSingleType(typeof(ConfigurationClassThatIsSealed));
            Assert.Throws<ObjectDefinitionParsingException>(_context.Refresh);
        }

        [Test]
        public void Can_Prevent_Overloaded_Methods()
        {
            ScanForAndRegisterSingleType(typeof(ConfigurationClassWithOverloadedMethods));
            Assert.Throws<ObjectDefinitionParsingException>(_context.Refresh);
        }

        [Test]
        public void Can_Prevent_Circular_ConfigurationClass_Refereces()
        {
            ScanForAndRegisterSingleType(typeof(FirstConfigurationClassWithCircularReference));

            try
            {
                _context.Refresh();
            }
            catch (ObjectDefinitionStoreException ex)
            {
                Assert.That(ex.InnerException, Is.TypeOf(typeof(ObjectDefinitionParsingException)));
            }
        }

    }

    public class SomeType
    {
    }


    [Configuration]
    public class ConfigurationClassWithNonVirtualMethod
    {
        [ObjectDef]
        public SomeType MethodThatRegistersSomeType()
        {
            return new SomeType();
        }
    }

    [Configuration]
    public class ConfigurationClassWithStaticMethod
    {
        [ObjectDef]
        public static SomeType MethodThatRegistersSomeType()
        {
            return new SomeType();
        }
    }

    [Configuration]
    public class ConfigurationClassWithOverloadedMethods
    {
        [ObjectDef]
        public virtual SomeType MethodThatRegistersSomeType()
        {
            return new SomeType();
        }

        [ObjectDef]
        public virtual SomeType MethodThatRegistersSomeType(int i)
        {
            return new SomeType();
        }
    }

    [Configuration]
    [Import(typeof(SecondConfigurationClassWithCircularReference))]
    public class FirstConfigurationClassWithCircularReference
    {
        [ObjectDef]
        public virtual SomeType MethodThatRegistersSomeType()
        {
            return new SomeType();
        }
    }

    [Configuration]
    [Import(typeof(FirstConfigurationClassWithCircularReference))]
    public class SecondConfigurationClassWithCircularReference
    {
        [ObjectDef]
        public virtual SomeType MethodThatRegistersSomeType()
        {
            return new SomeType();
        }
    }


    [Configuration]
    public class ConfigurationClassWithMethodHavingParameters
    {
        [ObjectDef]
        public virtual SomeType MethodThatRegistersSomeType(int i)
        {
            return new SomeType();
        }
    }


    [Configuration]
    public abstract class ConfigurationClassThatIsAbstract
    {
        [ObjectDef]
        public virtual SomeType MethodThatRegistersSomeType()
        {
            return new SomeType();
        }
    }

    [Configuration]
    public sealed class ConfigurationClassThatIsSealed
    {
        [ObjectDef]
        public SomeType MethodThatRegistersSomeType()
        {
            return new SomeType();
        }
    }
}