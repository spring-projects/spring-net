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
using NUnit.Framework;
using Spring.Objects;

#endregion

namespace Spring.Core.TypeResolution
{
    /// <summary>
    /// Unit tests for the GenericTypeResolver class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class GenericTypeResolverTests : TypeResolverTests
    {
        protected override ITypeResolver GetTypeResolver()
        {
            return new GenericTypeResolver();
        }

        [Test]
        public void ResolveLocalAssemblyGenericType()
        {
            Type t = GetTypeResolver().Resolve("Spring.Objects.TestGenericObject< int, string>");
            Assert.AreEqual(typeof(TestGenericObject<int, string>), t);
        }

        [Test]
        public void ResolveLocalAssemblyGenericTypeDefinition()
        {
            // CLOVER:ON
            Type t = GetTypeResolver().Resolve("Spring.Objects.TestGenericObject< ,>");
            // CLOVER:OFF
            Assert.AreEqual(typeof(TestGenericObject<,>), t);
        }

        [Test]
        public void ResolveLocalAssemblyGenericTypeOpen()
        {
            Assert.Throws<TypeLoadException>(() => GetTypeResolver().Resolve("Spring.Objects.TestGenericObject<int >"));
        }

        [Test]
        public void ResolveGenericTypeWithAssemblyName()
        {
            Type t = GetTypeResolver().Resolve("System.Collections.Generic.Stack<string>, System");
            Assert.AreEqual(typeof(System.Collections.Generic.Stack<string>), t);
        }

        [Test]
        public void ResolveGenericArrayType()
        {
            Type t = GetTypeResolver().Resolve("System.Nullable<[System.Int32, mscorlib]>[,]");
            Assert.AreEqual(typeof(int?[,]), t);
            t = GetTypeResolver().Resolve("System.Nullable`1[int][,]");
            Assert.AreEqual(typeof(int?[,]), t);
        }

        [Test]
        public void ResolveGenericArrayTypeWithAssemblyName()
        {
            Type t = GetTypeResolver().Resolve("System.Nullable<[System.Int32, mscorlib]>[,], mscorlib");
            Assert.AreEqual(typeof(int?[,]), t);
            t = GetTypeResolver().Resolve("System.Nullable<[System.Int32, mscorlib]>[,], mscorlib");
            Assert.AreEqual(typeof(int?[,]), t);
            t = GetTypeResolver().Resolve("System.Nullable`1[[System.Int32, mscorlib]][,], mscorlib");
            Assert.AreEqual(typeof(int?[,]), t);
        }

        [Test]
        public void ResolveAmbiguousGenericTypeWithAssemblyName()
        {
            Assert.Throws<TypeLoadException>(() => GetTypeResolver().Resolve("Spring.Objects.TestGenericObject<System.Collections.Generic.Stack<int>, System, string>"));
        }

        [Test]
        public void ResolveMalformedGenericType()
        {
            Assert.Throws<TypeLoadException>(() => GetTypeResolver().Resolve("Spring.Objects.TestGenericObject<int, <string>>"));
        }

        [Test]
        public void ResolveNestedGenericTypeWithAssemblyName()
        {
            Type t = GetTypeResolver().Resolve("System.Collections.Generic.Stack< Spring.Objects.TestGenericObject<int, string> >, System");
            Assert.AreEqual(typeof(System.Collections.Generic.Stack<Spring.Objects.TestGenericObject<int, string>>), t);
        }

        [Test]
        public void ResolveClrNotationStyleGenericTypeWithAssemblyName()
        {
            Type t = GetTypeResolver().Resolve("System.Collections.Generic.Stack`1[ [Spring.Objects.TestGenericObject`2[int, string], Spring.Core.Tests] ], System");
            Assert.AreEqual(typeof(System.Collections.Generic.Stack<Spring.Objects.TestGenericObject<int, string>>), t);
        }

        [Test]
        public void ResolveNestedQuotedGenericTypeWithAssemblyName()
        {
            Type t = GetTypeResolver().Resolve("System.Collections.Generic.Stack< [Spring.Objects.TestGenericObject<int, string>, Spring.Core.Tests] >, System");
            Assert.AreEqual(typeof(System.Collections.Generic.Stack<Spring.Objects.TestGenericObject<int, string>>), t);
        }
    }
}
