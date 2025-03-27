#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using NUnit.Framework;
using Spring.Objects;

#endregion

namespace Spring.Core.TypeResolution
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class TypeAssemblyHolderTests
    {
        [Test]
        public void CanTakeQualifiedType()
        {
            Type testType = typeof(TestObject);
            TypeAssemblyHolder tah = new TypeAssemblyHolder(testType.AssemblyQualifiedName);
            Assert.IsTrue(tah.IsAssemblyQualified);
            Assert.AreEqual(testType.FullName, tah.TypeName);
            Assert.AreEqual(testType.Assembly.FullName, tah.AssemblyName);
        }

        [Test]
        public void CanTakeUnqualifiedType()
        {
            Type testType = typeof(TestObject);
            TypeAssemblyHolder tah = new TypeAssemblyHolder(testType.FullName);
            Assert.IsFalse(tah.IsAssemblyQualified);
            Assert.AreEqual(testType.FullName, tah.TypeName);
            Assert.AreEqual(null, tah.AssemblyName);
        }

        [Test]
        public void CanTakeUnqualifiedGenericType()
        {
            Type testType = typeof(TestGenericObject<int, string>);
            TypeAssemblyHolder tah = new TypeAssemblyHolder(testType.FullName);
            Assert.IsFalse(tah.IsAssemblyQualified);
            Assert.AreEqual(testType.FullName, tah.TypeName);
            Assert.AreEqual(null, tah.AssemblyName);
        }

        [Test]
        public void CanTakeQualifiedGenericType()
        {
            Type testType = typeof(TestGenericObject<int, string>);
            TypeAssemblyHolder tah = new TypeAssemblyHolder(testType.AssemblyQualifiedName);
            Assert.IsTrue(tah.IsAssemblyQualified);
            Assert.AreEqual(testType.FullName, tah.TypeName);
            Assert.AreEqual(testType.Assembly.FullName, tah.AssemblyName);
        }
    }
}
