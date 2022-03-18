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

using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

#endregion

namespace Spring.Core.IO
{
    /// <summary>
    /// Common Unit tests for all FileSystemResource derived classes.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public abstract class FileSystemResourceCommonTests
    {
        protected const string TemporaryFileName = "temp.file";

        protected abstract FileSystemResource CreateResourceInstance(string resourceName);

        /// <summary>
        /// Creates a FileInfo instance representing the original location of the given assembly
        /// </summary>
        /// <remarks>
        /// Use this instead of the "Assembly.Location" property to get the original location before shadow copying!
        /// </remarks>
        protected static FileInfo GetAssemblyLocation(Assembly assembly)
        {
            return new FileInfo(new Uri(assembly.Location).LocalPath);
        }

        protected static FileInfo CreateFileForTheCurrentDirectory()
        {
            return new FileInfo(Path.GetFullPath(
                                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemporaryFileName)));
        }

        [Test]
        public void CreateFileSystemResourceWithPathName()
        {
            FileSystemResource fileSystemResource = CreateResourceInstance(TemporaryFileName);
            Assert.AreEqual(TemporaryFileName, fileSystemResource.File.Name);
        }

        [Test]
        public void FileSystemResourceExists()
        {
            FileInfo file = GetAssemblyLocation(Assembly.GetExecutingAssembly());
            FileSystemResource fileSystemResource = CreateResourceInstance("~/" + file.Name);
            Assert.IsTrue(fileSystemResource.Exists);
        }

        [Test]
        public void FileSystemResourceNotExists()
        {
            Assert.IsFalse(CreateResourceInstance("asdfasfadf").Exists);
        }

        [Test]
        public void FileSystemResourceOpenNonExistanceFile()
        {
            FileSystemResource fileSystemResource = CreateResourceInstance(TemporaryFileName);
            Stream inputStream;
            Assert.Throws<FileNotFoundException>(() => inputStream = fileSystemResource.InputStream);
        }

        [Test]
        public void FileSystemResourceValidInputStream()
        {
            FileInfo file = GetAssemblyLocation(Assembly.GetExecutingAssembly());
            FileSystemResource fileSystemResource = CreateResourceInstance("~/" + file.Name);
            using(Stream inputStream = fileSystemResource.InputStream)
            {
                Assert.IsNotNull(inputStream);
            }
        }

        [Test]
        public void FileSystemResourceGivesOpenedInputStream()
        {
            FileInfo file = GetAssemblyLocation(Assembly.GetExecutingAssembly());
            FileSystemResource fileSystemResource = CreateResourceInstance("~/" + file.Name);
            using(Stream inputStream = fileSystemResource.InputStream)
            {
                Assert.IsTrue(inputStream.CanRead);
            }
        }

        [Test]
        public void GetDescription()
        {
            FileSystemResource fileSystemResource = CreateResourceInstance(TemporaryFileName);
            string expectedDescription = "file [" + fileSystemResource.File.FullName + "]";
            Assert.AreEqual(expectedDescription, fileSystemResource.Description);
        }

        [Test]
        public void GetURL()
        {
            FileSystemResource fileSystemResource = CreateResourceInstance(TemporaryFileName);
            Assert.IsNotNull(fileSystemResource.Uri);
        }

        /// <summary>
        /// Even though the 'root' resource points to a nonexistent subdirectory, surfing 'up' to the parent
        /// via a relative path should still work...
        /// </summary>
        [Test]
        public void CreateRelativeFromNonExistentOriginalResource()
        {
            // a suitable subdirectory of total pish
            IResource resource = CreateResourceInstance("dork/muller.venken");
            Assert.IsFalse(resource.Exists,
                           "This test needs to feed off of a base resource that explicitly *doesn't* exist; but the resource seems to exist anyway.");
            IResource relative =
                resource.CreateRelative("../" + GetAssemblyLocation(Assembly.GetExecutingAssembly()).Name);
            Assert.IsTrue(relative.Exists);
        }

        [Test]
        public void CreateRelativeResourceIsEqualToOriginalAfterBouncingUpAndDownTheDirectoryTree()
        {
            IResource resource = new FileSystemResource(GetAssemblyLocation(Assembly.GetExecutingAssembly()).FullName);
            IResource relative =
                resource.CreateRelative("foo/bar/../../" + GetAssemblyLocation(Assembly.GetExecutingAssembly()).Name);
            Assert.IsTrue(relative.Exists);
            Assert.IsTrue(resource.Equals(relative));
        }

        [Test]
        public void CreateRelativeWithNullRelativePath()
        {
            IResource resource = CreateResourceInstance(".");
            Assert.Throws<ArgumentNullException>(() => resource.CreateRelative(null));
        }

        [Test]
        public void CreateRelativeWithEmptyRelativePath()
        {
            IResource resource = CreateResourceInstance("boba.licious");
            IResource relative = resource.CreateRelative(string.Empty);
            Assert.IsFalse(relative.Exists);
        }

        [Test]
        public void RelativeResourceWhenNotRelative()
        {
            IResource res = CreateResourceInstance("dummy.txt");
            IResource res2 = CreateResourceInstance("/index.html");

            IResource rel0 = res.CreateRelative("/index.html");
            Assert.IsTrue(rel0 is FileSystemResource);
            Assert.AreEqual(res2.File.FullName, rel0.File.FullName);
        }

        [Test]
        public void RelativeResourceFromRoot()
        {
            FileSystemResource res = CreateResourceInstance(@"/dummy.txt");
            FileSystemResource res2;

            IResource rel0 = res.CreateRelative("/index.html");
            Assert.IsTrue(rel0 is FileSystemResource);
            res2 = CreateResourceInstance("/index.html");
            Assert.AreEqual(res2.File.FullName, rel0.File.FullName);

            IResource rel1 = res.CreateRelative(@"index.html");
            Assert.IsTrue(rel1 is FileSystemResource);
            res2 = CreateResourceInstance("/index.html");
            Assert.AreEqual(res2.File.FullName, rel1.File.FullName);

            IResource rel2 = res.CreateRelative(@"samples/artfair/index.html");
            Assert.IsTrue(rel2 is FileSystemResource);
            res2 = CreateResourceInstance("/samples/artfair/index.html");
            Assert.AreEqual(res2.File.FullName, rel2.File.FullName);

            IResource rel3 = res.CreateRelative(@"./samples/artfair/index.html");
            Assert.IsTrue(rel3 is FileSystemResource);
            res2 = CreateResourceInstance("/samples/artfair/index.html");
            Assert.AreEqual(res2.File.FullName, rel3.File.FullName);
        }

        [Test]
        public void RelativeResourceFromSubfolder()
        {
            FileSystemResource res = CreateResourceInstance(@"/samples/artfair/dummy.txt");
            FileSystemResource resExpected;

            IResource rel0 = res.CreateRelative(@"/index.html");
            Assert.IsTrue(rel0 is FileSystemResource);
            resExpected = CreateResourceInstance("/index.html");
            Assert.AreEqual(resExpected.File.FullName, rel0.File.FullName);

            IResource rel1 = res.CreateRelative(@"index.html");
            Assert.IsTrue(rel1 is FileSystemResource);
            resExpected = CreateResourceInstance("/samples/artfair/index.html");
            Assert.AreEqual(resExpected.File.FullName, rel1.File.FullName);

            IResource rel2 = res.CreateRelative(@"demo\index.html");
            Assert.IsTrue(rel2 is FileSystemResource);
            resExpected = CreateResourceInstance("/samples/artfair/demo/index.html");
            Assert.AreEqual(resExpected.File.FullName, rel2.File.FullName);

            IResource rel3 = res.CreateRelative(@"./demo/index.html");
            Assert.IsTrue(rel3 is FileSystemResource);
            resExpected = CreateResourceInstance("/samples/artfair/demo/index.html");
            Assert.AreEqual(resExpected.File.FullName, rel3.File.FullName);

            IResource rel4 = res.CreateRelative(@"../calculator/index.html");
            Assert.IsTrue(rel4 is FileSystemResource);
            resExpected = CreateResourceInstance("/samples/calculator/index.html");
            Assert.AreEqual(resExpected.File.FullName, rel4.File.FullName);

            IResource rel5 = res.CreateRelative(@"..\..\index.html");
            resExpected = CreateResourceInstance("/index.html");
            Assert.AreEqual(resExpected.File.FullName, rel5.File.FullName);
        }

        [Test]
        public void RelativeResourceTooManyBackLevels()
        {
            FileSystemResource res = CreateResourceInstance("/samples/artfair/dummy.txt");
            Assert.Throws<UriFormatException>(() => res.CreateRelative("../../../index.html"));
        }

        [Test]
        public void SupportsAndResolvesTheSpecialHomeCharacter_SunnyDay()
        {
            FileInfo file =
                new FileInfo(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemporaryFileName)));
            StreamWriter writer = file.CreateText();
            FileSystemResource res = CreateResourceInstance("~/" + TemporaryFileName);
            Assert.IsTrue(res.File.Exists);
            Assert.AreEqual(file.FullName.ToLower(), res.File.FullName.ToLower());
            try
            {
                writer.Close();
            }
            catch(IOException)
            {}
            try
            {
                file.Delete();
            }
            catch(IOException)
            {}
        }

        [Test]
        [Ignore("problematic between framework versions")]
        public void SupportsAndResolvesTheSpecialHomeCharacter_SunnyDayEvenWithLeadingWhitespace()
        {
            FileInfo file =
                new FileInfo(Path.GetFullPath(
                                 Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemporaryFileName)));
            StreamWriter writer = file.CreateText();
            FileSystemResource res = CreateResourceInstance("    ~/" + TemporaryFileName);
            Assert.AreEqual(file.FullName.ToLower(), res.File.FullName.ToLower());
            try
            {
                writer.Close();
            }
            catch(IOException)
            {}
            try
            {
                file.Delete();
            }
            catch(IOException)
            {}
        }

        [Test]
        public void SupportsAndResolvesTheSpecialHomeCharacter_OnlyIfSpecialHomeCharacterIsFirstCharacter()
        {
            FileSystemResource res = CreateResourceInstance("foo~.txt");
            // must not have replaced ~; its only valid at the start of a resource name...
            Assert.AreEqual("foo~.txt", res.File.Name);
        }

        [Test]
        public void Resolution_PlainVanilla()
        {
            FileInfo file = CreateFileForTheCurrentDirectory();
            IResource res = CreateResourceInstance(TemporaryFileName);
            Assert.AreEqual(file.FullName.ToLower(), res.File.FullName.ToLower(),
                            "The bare file name all by itself must have resolved to a file in the current " +
                            "directory of the currently executing domain.");
        }

        [Test]
        public void Resolution_WithSpecialHomeCharacter()
        {
            FileInfo file = CreateFileForTheCurrentDirectory();
            IResource res = CreateResourceInstance("~/" + TemporaryFileName);
            Assert.AreEqual(file.FullName.ToLower(), res.File.FullName.ToLower(),
                            "The file name with ~/ must have resolved to a file " +
                            "in the current directory of the currently executing domain.");
        }
    }
}