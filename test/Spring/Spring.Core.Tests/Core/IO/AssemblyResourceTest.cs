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

#region Imports

using System;
using System.IO;

using NUnit.Framework;

#endregion

namespace Spring.Core.IO
{
    /// <summary>
    /// Unit tests for AssemblyResource
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <author>Federico Spinazzi</author>
    [TestFixture]
    public class AssemblyResourceTest
    {
        #region SetUp/TearDown

        [SetUp]
        public void SetUp()
        {}

        [TearDown]
        public void TearDown()
        {}

        #endregion

        /// <summary>
        /// Use incorrect format for an assembly resource.  Using
        /// comma delimited instead of '/'.
        /// </summary>
        [Test]
        public void CreateWithMalformedResourceName()
        {
            Assert.Throws<UriFormatException>(() => new AssemblyResource("assembly://Spring.Core.Tests,Spring.TestResource.txt"));
        }

        /// <summary>
        /// Use old format, no longer supported (actually never publicly released)
        /// that used 'dot' notation to seperate the namespace and resource name.
        /// </summary>
        [Test]
        public void CreateWithObsoleteResourceName()
        {
            Assert.Throws<UriFormatException>(() => new AssemblyResource("assembly://Spring.Core.Tests/Spring.TestResource.txt"));
        }

        /// <summary>
        /// Use the correct format but with an invalid assembly name.
        /// </summary>
        [Test]
        public void CreateFromInvalidAssembly()
        {
            Assert.Throws<FileNotFoundException>(() => new AssemblyResource("assembly://Xyz.Invalid.Assembly/Spring/TestResource.txt"));
        }

        /// <summary>
        /// Sunny day scenario that creates IResources and ensures the
        /// correct contents can be read from them.
        /// </summary>
        [Test]
        public void CreateValidAssemblyResource()
        {
            IResource res = new AssemblyResource("assembly://Spring.Core.Tests/Spring/TestResource.txt");
            AssertResourceContent(res, "Spring.TestResource.txt");
            IResource res2 = new AssemblyResource("assembly://Spring.Core.Tests/Spring.Core.IO/TestResource.txt");
            AssertResourceContent(res2, "Spring.Core.IO.TestResource.txt");
        }

        /// <summary>
        /// Use correct assembly name, but incorrect namespace and resource name.
        /// </summary>
        [Test]
        public void CreateInvalidAssemblyResource()
        {
            IResource res = new AssemblyResource("assembly://Spring.Core.Tests/Xyz/InvalidResource.txt");
            Assert.IsFalse(res.Exists, "Exists should return false");
            Assert.IsNull(res.InputStream, "Stream should be null");
        }

        [Test]
        public void CreateRelativeWhenNotRelative()
        {
            IResource res = new AssemblyResource("assembly://Spring.Core.Tests/Spring/TestResource.txt");
            IResource res2 = res.CreateRelative("Spring.Core.Tests/Spring.Core.IO/TestResource.txt");
            AssertResourceContent(res2, "Spring.Core.IO.TestResource.txt");
        }

        /// <summary>
        /// Test creating a resource relative to the location of the first.
        /// The first resource is physically located in the root of the project since
        /// the default namespace of the Spring.Core.Tests project is
        /// 'Spring'.  The notation './IO/TestResource.txt' will navigate
        /// down to the 'Spring.Core.IO' namespace and CreateRelative will
        /// then retrieve the similarly named TestResource.txt located there.
        /// </summary>
        [Test]
        public void CreateRelativeInChildNamespace()
        {
            IResource res = new AssemblyResource("assembly://Spring.Core.Tests/Spring/TestResource.txt");
            IResource res2 = res.CreateRelative("./Core.IO/TestResource.txt");
            AssertResourceContent(res2, "Spring.Core.IO.TestResource.txt");
        }

        /// <summary>
        /// Test creating a resource relative to the location of the first.
        /// The first resource is physically located in the root of the project since
        /// the default namespace of the Spring.Core.Tests project is
        /// 'Spring'.  The notation 'IO/TestResource.txt' will navigate
        /// down to the 'Spring.Core.IO' namespace and CreateRelative will
        /// then retrieve the similarly named TestResource.txt located there.
        /// </summary>
        [Test]
        public void CreateRelativeInChildNamespaceWithoutPrefix()
        {
            IResource res = new AssemblyResource("assembly://Spring.Core.Tests/Spring/TestResource.txt");
            IResource res2 = res.CreateRelative("Core.IO/TestResource.txt");
            AssertResourceContent(res2, "Spring.Core.IO.TestResource.txt");
        }

        /// <summary>
        /// Test creating a resource relative to the root of the assembly.
        /// The first resource is physically located in the root of the project since
        /// the default namespace of the Spring.Core.Tests project is
        /// 'Spring'.  The notation '/Spring.Core.IO/TestResource.txt' will navigate
        /// down to the 'Spring.Core.IO' namespace and CreateRelative will
        /// then retrieve the similarly named TestResource.txt located there.
        /// </summary>
        [Test]
        public void CreateRelativeToRoot()
        {
            IResource res = new AssemblyResource("assembly://Spring.Core.Tests/Spring/TestResource.txt");
            IResource res2 = res.CreateRelative("/Spring.Core.IO/TestResource.txt");
            AssertResourceContent(res2, "Spring.Core.IO.TestResource.txt");
        }

        /// <summary>
        /// Test creating a resource relative to the location of the first.
        /// The first resource is physically located in the Spring.Core.IO directory
        /// of the project and corresponds to the namespace 'Spring.Core.IO'.
        /// The notation '../../TestResource.txt' will navigate up to the
        /// root 'Spring' namespace and CreateRelative will then
        /// retrieve the similarly named TestResource.txt located there
        /// </summary>
        [Test]
        public void CreateRelativeInParentNamespace()
        {
            IResource res = new AssemblyResource("assembly://Spring.Core.Tests/Spring.Core.IO/TestResource.txt");
            IResource res2 = res.CreateRelative("../../TestResource.txt");
            AssertResourceContent(res2, "Spring.TestResource.txt");
        }

        /// <summary>
        /// Test creating a resource relative to the location of the first.
        /// The first resource is physically located in the Spring.Core.IO
        /// directory of the project and corresponds to the namespace
        /// 'Spring.Core.IO'.  The notation '../../Objects/Factory/TestResource.txt'
        /// will navigate up to the root 'Spring' namespace and then down
        /// into the 'Spring.Object.Factory' namespace and CreateRelative
        /// will then retrieve the similarly named TestResource.txt located there.
        /// </summary>
        [Test]
        public void CreateRelativeInNotStraightParentNamespace()
        {
            IResource res = new AssemblyResource("assembly://Spring.Core.Tests/Spring.Core.IO/TestResource.txt");
            IResource res2 = res.CreateRelative("../../Objects/Factory/TestResource.txt");
            AssertResourceContent(res2, "Spring.Objects.Factory.TestResource.txt");
        }

        /// <summary>
        /// Test creating a resource relative to the location of the first.
        /// In this case the first resource is an assembly and the second is
        /// uses the file URI.
        /// The file URI used three slashes '///' which is interpreted to
        /// mean the root of where the assembly is located on the file system.
        /// The file 'abstract.xml' is located under Spring.Data in the VS.NET project
        /// but a build-event copies these files under the location
        /// of Spring.Core.Tests.dll.
        /// </summary>
        [Test]
        public void CreateRelativeWithAReferenceToAFileResource()
        {
            IResource res = new AssemblyResource("assembly://Spring.Core.Tests/Spring.Core.IO/TestResource.txt");
            const string path = "Data/Spring/Objects/Factory/Xml/abstract.xml";
            IResource res2 = res.CreateRelative("file://~/" + path);
            using (StreamReader r = File.OpenText(path))
            {
                string content = r.ReadToEnd();
                using (StreamReader reader = new StreamReader(res2.InputStream))
                {
                    Assert.AreEqual(content, reader.ReadToEnd(), "Resource content is not as expected");
                }
            }
        }

        /// <summary>
        /// Try to create a relative resource, but use too many '..' to navigate
        /// past the root namespace, off into la-la land.
        /// </summary>
        [Test]
        public void TooMuchParentNamespacesAbove()
        {
            IResource res = new AssemblyResource("assembly://Spring.Core.Tests/Spring.Core.IO/TestResource.txt");
            Assert.Throws<UriFormatException>(() => res.CreateRelative("../../../../TestResource.txt"));
        }

        /// <summary>
        /// Utility method to compare a resource that contains a single string with
        /// an exemplar.
        /// </summary>
        /// <param name="res">The resource to read a line from</param>
        /// <param name="expectedContent">the expected value of the line.</param>
        private void AssertResourceContent(IResource res, string expectedContent)
        {
            Assert.IsTrue(res.Exists);
            using (StreamReader reader = new StreamReader(res.InputStream))
            {
                Assert.AreEqual(expectedContent, reader.ReadLine(), "Resource content is not as expected");
            }
        }
    }
}