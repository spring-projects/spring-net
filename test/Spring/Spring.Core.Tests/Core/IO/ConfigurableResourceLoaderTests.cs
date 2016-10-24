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
	/// Unit tests for the ConfigurableResourceLoader class.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
	[TestFixture]
	public sealed class ConfigurableResourceLoaderTests
	{
		private ConfigurableResourceLoader loader;

        [SetUp]
        public void SetUp()
        {
            loader = new ConfigurableResourceLoader();
        }

		#region ConfigurableResourceLoader.GetResource Tests

		/// <summary>
		/// Tests that loader correctly loads files specified by absolute name, regardless
		/// of the fact whether protocol name is specified or not.
		/// </summary>
		[Test]
		public void GetAbsoluteFileSystemResource()
		{
			string fileName = Path.GetTempFileName();
			try
			{
				IResource withoutProtocol = loader.GetResource(fileName);
				Assert.IsNotNull(withoutProtocol, "Resource should not be null");
				Assert.IsTrue(withoutProtocol is FileSystemResource, "Expected FileSystemResource");
				Assert.IsTrue(withoutProtocol.Exists, "Resource should exist but it does not");

				IResource withProtocol = loader.GetResource("file:///" + fileName);
				Assert.IsNotNull(withProtocol, "Resource should not be null");
				Assert.IsTrue(withProtocol is FileSystemResource, "Expected FileSystemResource");
				Assert.IsTrue(withProtocol.Exists, "Resource should exist but it does not");
			}
			finally
			{
				new FileInfo(fileName).Delete();
			}
		}

	    [Test]
	    public void GetResourceThatSupportsTheSpecialHomeCharacter()
	    {
	        string filename = "foo.txt";
            FileInfo expectedFile =
                new FileInfo(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)));
            StreamWriter writer = expectedFile.CreateText();
            FileSystemResource res = (FileSystemResource) loader.GetResource("~/" + filename);
            Assert.AreEqual(expectedFile.FullName, res.File.FullName);
            try
            {
                writer.Close();
            }
            catch (IOException)
            {
            }
            try
            {
                expectedFile.Delete();
            }
            catch (IOException)
            {
            }
        }

        [Test]
        public void GetResourceThatSupportsTheSpecialHomeCharacter_WithLeadingWhitespace()
        {
            string filename = "foo.txt";
            FileInfo expectedFile =
                new FileInfo(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)));
            StreamWriter writer = expectedFile.CreateText();
            FileSystemResource res = (FileSystemResource) loader.GetResource("   ~/" + filename);
            Assert.AreEqual(expectedFile.FullName, res.File.FullName);
            try
            {
                writer.Close();
            }
            catch (IOException)
            {
            }
            try
            {
                expectedFile.Delete();
            }
            catch (IOException)
            {
            }
        }

		/// <summary>
		/// Tests that loader correctly loads files specified by relative name, regardless
		/// of the fact whether protocol name is specified or not.
		/// </summary>
		[Test]
		public void GetRelativeFileSystemResource()
		{
			string fileName = "test.tmp";
			FileInfo fi = new FileInfo(fileName);
			FileStream fs = fi.Create();
			fs.Close();

			try
			{
				IResource withoutProtocol = loader.GetResource(fileName);
				Assert.IsNotNull(withoutProtocol, "Resource should not be null");
				Assert.IsTrue(withoutProtocol is FileSystemResource, "Expected FileSystemResource");
				Assert.IsTrue(withoutProtocol.Exists, "Resource should exist but it does not");

				IResource withProtocol = loader.GetResource("file://" + fileName);
				Assert.IsNotNull(withProtocol, "Resource should not be null");
				Assert.IsTrue(withProtocol is FileSystemResource, "Expected FileSystemResource");
				Assert.IsTrue(withProtocol.Exists, "Resource should exist but it does not");
			}
			finally
			{
				fi.Delete();
			}
		}

		/// <summary>
		/// Tests that loader can load UrlResource over HTTP protocol
		/// </summary>
		[Test]
		[Explicit]
		public void GetHttpUrlResource()
		{
			IResource res = loader.GetResource("http://www.springframework.net/license.html");
			Assert.IsNotNull(res, "Resource should not be null");
            Assert.AreEqual(typeof(UrlResource), res.GetType());
		}

		/// <summary>
		/// Tests that loader can load UrlResource over assembly pseudo protocol
		/// </summary>
		[Test]
		public void GetAssemblyResource()
		{
			IResource res = loader.GetResource("assembly://Spring.Core.Tests/Spring/TestResource.txt");
			Assert.IsNotNull(res, "Resource should not be null");
			Assert.AreEqual(typeof(AssemblyResource), res.GetType());
		}

		#endregion

		[Test]
		public void GetResourceForNonMappedProtocol()
		{
            Assert.Throws<UriFormatException>(() => new ConfigurableResourceLoader().GetResource("beep://foo.xml"));
        }
	}
}