#region License

/*
 * Copyright 2004 the original author or authors.
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

#region Imports

using System;
using System.Collections.Generic;
using System.Reflection;

using NUnit.Framework;

using Spring.Objects;

#endregion

namespace Spring.Core.TypeResolution
{
	/// <summary>
    /// Unit tests for the TypeResolutionUtils class.
	/// </summary>
    [TestFixture]
    public sealed class TypeResolutionUtilsTests
    {
        [Test]
        public void ResolveFromAssemblyQualifiedName()
        {
            Type testObjectType = TypeResolutionUtils.ResolveType("Spring.Objects.TestObject, Spring.Core.Tests");
            Assert.IsNotNull(testObjectType);
			Assert.IsTrue(testObjectType.Equals(typeof (TestObject)));
        }

		[Test]
		public void ResolveFromBadAssemblyQualifiedName()
		{
            Assert.Throws<TypeLoadException>(() => TypeResolutionUtils.ResolveType("Spring.Objects.TestObject, Spring.Core.FooTests"));
		}

        [Test]
        public void ResolveFromShortName()
        {
            Type testObjectType = TypeResolutionUtils.ResolveType("Spring.Objects.TestObject");
            Assert.IsNotNull(testObjectType);
			Assert.IsTrue(testObjectType.Equals(typeof (TestObject)));
        }

		[Test]
		public void ResolveFromBadShortName()
		{
            Assert.Throws<TypeLoadException>(() => TypeResolutionUtils.ResolveType("Spring.Objects.FooBarTestObject"));
		}

        [Test]
        public void ResolveInterfaceArrayFromStringArray()
        {
            Type[] expected = new Type[] { typeof(IFoo) };
            string[] input = new string[] { typeof(IFoo).AssemblyQualifiedName };
            IList<Type> actual = TypeResolutionUtils.ResolveInterfaceArray(input);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Length, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
        }

        [Test]
        public void ResolveInterfaceArrayFromStringArrayWithNonInterfaceTypes()
        {
            string[] input = new string[] { GetType().AssemblyQualifiedName };
            Assert.Throws<ArgumentException>(() => TypeResolutionUtils.ResolveInterfaceArray(input));
        }

        [Test]
        public void MethodMatch()
        {
            MethodInfo absquatulateMethod = typeof(TestObject).GetMethod("Absquatulate");
            Assert.IsTrue(TypeResolutionUtils.MethodMatch("*", absquatulateMethod), "Should match '*'");
            Assert.IsTrue(TypeResolutionUtils.MethodMatch("*tulate", absquatulateMethod), "Should match '*tulate'");
            Assert.IsTrue(TypeResolutionUtils.MethodMatch("Absqua*", absquatulateMethod), "Should match 'Absqua*'");
            Assert.IsTrue(TypeResolutionUtils.MethodMatch("*quatul*", absquatulateMethod), "Should match '*quatul*'");
            Assert.IsTrue(TypeResolutionUtils.MethodMatch("Absquatulate", absquatulateMethod), "Should match 'Absquatulate'");
            Assert.IsTrue(TypeResolutionUtils.MethodMatch("Absquatulate()", absquatulateMethod), "Should match 'Absquatulate()'");
            Assert.IsTrue(TypeResolutionUtils.MethodMatch("Absquatulate()", absquatulateMethod), "Should match 'Absquatulate()'");
            Assert.IsFalse(TypeResolutionUtils.MethodMatch("Absquatulate(string)", absquatulateMethod), "Should not match 'Absquatulate(string)'");

            MethodInfo addPeriodicElementMethod = typeof(TestObject).GetMethod("AddPeriodicElement");
            Assert.IsTrue(TypeResolutionUtils.MethodMatch("AddPeriodicElement", addPeriodicElementMethod), "Should match 'AddPeriodicElement'");
            Assert.IsFalse(TypeResolutionUtils.MethodMatch("AddPeriodicElement()", addPeriodicElementMethod), "Should not match 'AddPeriodicElement()'");
            Assert.IsFalse(TypeResolutionUtils.MethodMatch("AddPeriodicElement(string)", addPeriodicElementMethod), "Should not match 'AddPeriodicElement(string)'");
            Assert.IsTrue(TypeResolutionUtils.MethodMatch("AddPeriodicElement(string, string)", addPeriodicElementMethod), "Should match 'AddPeriodicElement(string, string)'");
        }

        #region Helper classes

        internal interface IFoo
        {
            bool Spanglish(string foo, object[] args);
        }

        #endregion
    }
}