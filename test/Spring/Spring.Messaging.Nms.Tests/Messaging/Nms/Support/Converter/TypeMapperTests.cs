#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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
using System.Collections;
using NUnit.Framework;
using Spring.Objects;

namespace Spring.Messaging.Nms.Support.Converter
{
    /// <summary>
    /// Test the TypeMapper
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class TypeMapperTests
    {
        private TypeMapper tm;

        [SetUp]
        public void SetUp()
        {
            tm = new TypeMapper();
        }

        [Test]
        public void ConfigurationNamespaceTests()
        {
            tm.DefaultAssemblyName = "Spring.Objects";
            Assert.Throws<ArgumentException>(() => tm.AfterPropertiesSet());
        }

        [Test]
        public void FromTypeTestsForDictionary()
        {            
            Assert.AreEqual("Hashtable", tm.FromType(typeof(Hashtable)));
        }

        [Test]
        public void ToTypeForDictionary()
        {
            Assert.AreEqual(typeof (Hashtable), tm.ToType("Hashtable"));
        }

        [Test]
        public void FromTypeForNonRegisteredType()
        {
            Assert.AreEqual("TestObject", tm.FromType(typeof(TestObject)));
        }

        [Test]
        public void ToTypeForNonRegisteredTypeSettingDefaults()
        {
            tm.DefaultNamespace = "Spring.Objects";
            tm.DefaultAssemblyName = "Spring.Core.Tests";
            Type resolvedTyped = tm.ToType("TestObject");
            Assert.AreEqual(typeof(TestObject), resolvedTyped);
        }

        [Test]
        public void ToTypeForUnresolvableType()
        {
            Assert.Throws<TypeLoadException>(() => tm.ToType("TestObject"));
        }

        [Test]
        public void MarhsalUsingAssemblyQualifiedName()
        {
            tm.UseAssemblyQualifiedName = true;
            string typeAsString = tm.FromType(typeof (TestObject));
            Type resolvedTyped = tm.ToType(typeAsString);
            Assert.AreEqual(typeof(TestObject), resolvedTyped);
        }

        [Test]
        public void UsingTypeMappings()
        {
            tm.IdTypeMapping.Add("1", typeof (TestObject));
            tm.AfterPropertiesSet();
            string typeAsString = tm.FromType(typeof (TestObject));
            Assert.AreEqual("1", typeAsString);
            Type resolvedType = tm.ToType("1");
            Assert.AreEqual(typeof(TestObject), resolvedType);
        }
    }
}