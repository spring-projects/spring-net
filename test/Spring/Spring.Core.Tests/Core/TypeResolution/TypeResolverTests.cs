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

#region Imports

using System;
using System.Data;
using NUnit.Framework;
using Spring.Objects;

#endregion

namespace Spring.Core.TypeResolution
{
    /// <summary>
    /// Unit tests for the TypeResolver class.
    /// </summary>
    /// <author>Rick Evans</author>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class TypeResolverTests
    {
        protected virtual ITypeResolver GetTypeResolver()
        {
            return new TypeResolver();
        }

        [Test]
        public void ResolveLocalAssemblyType()
        {
            Type t = GetTypeResolver().Resolve("Spring.Objects.TestObject");
            Assert.AreEqual(typeof (TestObject), t);
        }

        [Test]
        public void ResolveWithPartialAssemblyName()
        {
            Type t = GetTypeResolver().Resolve("System.Data.IDbConnection, System.Data");
            Assert.AreEqual(typeof (IDbConnection), t);
        }

        /// <summary>
        /// Tests that the resolve method throws the correct exception
        /// when supplied a load of old rubbish as a type name.
        /// </summary>
        [Test]
        public void ResolveWithNonExistentTypeName()
        {
            Assert.Throws<TypeLoadException>(() => GetTypeResolver().Resolve("RaskolnikovsDilemma, System.StPetersburg"));
        }

        [Test]
        public void ResolveBadArgs()
        {
            Assert.Throws<TypeLoadException>(() => GetTypeResolver().Resolve(null));
        }

        [Test]
        public void ResolveLocalAssemblyTypeWithFullAssemblyQualifiedName()
        {
            Type t = GetTypeResolver().Resolve(typeof(TestObject).AssemblyQualifiedName);
            Assert.AreEqual(typeof (TestObject), t);
        }

        [Test]
        public void LoadTypeFromSystemAssemblySpecifyingOnlyTheAssemblyDisplayName()
        {
            string stringType = typeof(string).FullName + ", System";
            Assert.Throws<TypeLoadException>(() => GetTypeResolver().Resolve(stringType));
        }

        [Test]
        public void LoadTypeFromSystemAssemblySpecifyingTheFullAssemblyName()
        {
            string stringType = typeof(string).AssemblyQualifiedName;
            Type t = GetTypeResolver().Resolve(stringType);
            Assert.AreEqual(typeof(string), t);
        }
    }
}
