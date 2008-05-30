#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

#if NET_2_0

#region Imports

using System;
using System.Data;

using NUnit.Framework;
using Spring.Objects;

#endregion

namespace Spring.Core.TypeResolution
{
    /// <summary>
    /// Unit tests for the GenericTypeResolver class.
    /// </summary>
    /// <author>Bruno Baia</author>
    /// <version>$Id: GenericTypeResolverTests.cs,v 1.2 2007/08/16 05:42:33 markpollack Exp $</version>
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
            Assert.AreEqual(typeof(TestGenericObject<int,string>), t);
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
        [ExpectedException(typeof(TypeLoadException))]
        public void ResolveLocalAssemblyGenericTypeOpen()
        {
            GetTypeResolver().Resolve("Spring.Objects.TestGenericObject<int >");
        }

        [Test]
        public void ResolveGenericTypeWithAssemblyName()
        {
            Type t = GetTypeResolver().Resolve("System.Collections.Generic.Stack<string>, System");
            Assert.AreEqual(typeof(System.Collections.Generic.Stack<string>), t);
        }

        [Test]
        [ExpectedException(typeof(TypeLoadException))]
        public void ResolveAmbiguousGenericTypeWithAssemblyName()
        {
            Type t = GetTypeResolver().Resolve("Spring.Objects.TestGenericObject<System.Collections.Generic.Stack<int>, System, string>");
        }

        [Test]
        [ExpectedException(typeof(TypeLoadException))]
        public void ResolveMalformedGenericType()
        {
            Type t = GetTypeResolver().Resolve("Spring.Objects.TestGenericObject<int, <string>>");
        }
    }
}

#endif
