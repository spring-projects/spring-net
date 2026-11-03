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

using NUnit.Framework;

namespace Spring.Core.IO;

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
            Assert.That(withoutProtocol, Is.Not.Null, "Resource should not be null");
            Assert.That(withoutProtocol is FileSystemResource, Is.True, "Expected FileSystemResource");
            Assert.That(withoutProtocol.Exists, Is.True, "Resource should exist but it does not");

            IResource withProtocol = loader.GetResource("file:///" + fileName);
            Assert.That(withProtocol, Is.Not.Null, "Resource should not be null");
            Assert.That(withProtocol is FileSystemResource, Is.True, "Expected FileSystemResource");
            Assert.That(withProtocol.Exists, Is.True, "Resource should exist but it does not");
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
        Assert.That(res.File.FullName, Is.EqualTo(expectedFile.FullName));
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
        Assert.That(res.File.FullName, Is.EqualTo(expectedFile.FullName));
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
            Assert.That(withoutProtocol, Is.Not.Null, "Resource should not be null");
            Assert.That(withoutProtocol is FileSystemResource, Is.True, "Expected FileSystemResource");
            Assert.That(withoutProtocol.Exists, Is.True, "Resource should exist but it does not");

            IResource withProtocol = loader.GetResource("file://" + fileName);
            Assert.That(withProtocol, Is.Not.Null, "Resource should not be null");
            Assert.That(withProtocol is FileSystemResource, Is.True, "Expected FileSystemResource");
            Assert.That(withProtocol.Exists, Is.True, "Resource should exist but it does not");
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
        Assert.That(res, Is.Not.Null, "Resource should not be null");
        Assert.That(res.GetType(), Is.EqualTo(typeof(UrlResource)));
    }

    /// <summary>
    /// Tests that loader can load UrlResource over assembly pseudo protocol
    /// </summary>
    [Test]
    public void GetAssemblyResource()
    {
        IResource res = loader.GetResource("assembly://Spring.Core.Tests/Spring/TestResource.txt");
        Assert.That(res, Is.Not.Null, "Resource should not be null");
        Assert.That(res.GetType(), Is.EqualTo(typeof(AssemblyResource)));
    }

    [Test]
    public void GetResourceForNonMappedProtocol()
    {
        Assert.Throws<UriFormatException>(() => new ConfigurableResourceLoader().GetResource("beep://foo.xml"));
    }
}
