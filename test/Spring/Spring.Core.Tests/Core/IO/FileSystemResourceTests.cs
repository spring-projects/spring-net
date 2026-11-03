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

using NUnit.Framework;

namespace Spring.Core.IO;

/// <summary>
/// Unit tests for the FileSystemResourceTest class.
/// </summary>
/// <author>Rick Evans</author>
[Platform("Win")]
public class FileSystemResourceTests : FileSystemResourceCommonTests
{
    protected override FileSystemResource CreateResourceInstance(string resourceName)
    {
        return new FileSystemResource(resourceName);
    }

    [Test]
    public void LeadingProtocolIsNotTreatedRelative()
    {
        FileSystemResource res = new FileSystemResource(@"file://\\server\share\samples\artfair\");
        FileSystemResource res2 = (FileSystemResource) res.CreateRelative(@"file://./index.html");
        Assert.That(res2.Uri.AbsolutePath, Is.EqualTo(new Uri(Path.Combine(Environment.CurrentDirectory, "index.html")).AbsolutePath));
    }

    [Test]
    public void RelativeUncResourceTooManyBackLevels()
    {
        FileSystemResource res = new FileSystemResource(@"\\server\share\samples\artfair\dummy.txt");
        Assert.Throws<UriFormatException>(() => res.CreateRelative(@"..\..\..\index.html"));
    }

    [Test(Description = "SPRNET-89")]
    public void SPRNET_89_SupportUrlEncodedLocationsWithSpaces()
    {
        string path = Path.GetFullPath("spaced dir");
        Directory.CreateDirectory(path);
        string filePath = Path.Combine(path, "spaced file.txt");
        using (StreamWriter text = File.CreateText(filePath))
        {
            text.WriteLine("hello world");
        }

        FileSystemResource res = new FileSystemResource(new Uri(filePath).AbsoluteUri);
        using (Stream s = res.InputStream)
        {
            using (TextReader reader = new StreamReader(s))
            {
                Assert.That(reader.ReadLine(), Is.EqualTo("hello world"));
            }
        }
    }

    [Test(Description = "http://opensource.atlassian.com/projects/spring/browse/SPRNET-320")]
    public void SupportsSpecialUriCharacter()
    {
        string path = Path.GetFullPath("dir#");
        Directory.CreateDirectory(path);
        string filePath = Path.Combine(path, "file.txt");
        using (StreamWriter text = File.CreateText(filePath))
        {
            text.WriteLine("hello world");
        }

        FileSystemResource res = new FileSystemResource(new Uri(filePath).AbsoluteUri);
        using (Stream s = res.InputStream)
        {
            using (TextReader reader = new StreamReader(s))
            {
                Assert.That(reader.ReadLine(), Is.EqualTo("hello world"));
            }
        }
    }

    [Test]
    public void Resolution_WithProtocolAndSpecialHomeCharacter()
    {
        FileInfo file = CreateFileForTheCurrentDirectory();
        IResource res = new FileSystemResource("file://~/" + TemporaryFileName);
        Assert.That(res.File.FullName, Is.EqualTo(file.FullName),
            "The file name with file://~ must have resolved to a file " +
            "in the current directory of the currently executing domain.");
    }

    [Test]
    public void Resolution_WithProtocolAndSpecialHomeCharacterParentDirectory()
    {
        FileInfo file = new FileInfo(Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemporaryFileName)));
        IResource res = new FileSystemResource("file://~/../" + TemporaryFileName);
        string fileNameOneDirectoryUp = file.Directory.Parent.FullName + "\\" + TemporaryFileName;
        Assert.That(res.File.FullName, Is.EqualTo(fileNameOneDirectoryUp),
            "The file name with file://~/.. must have resolved to a file " +
            "in the parent directory of the currently executing domain.");
    }

    [Test]
    public void CreateRelativeWithParent()
    {
        // use the filename of the declaring assembly as the root...
        string rootpath = GetType().Assembly.Location;
        DirectoryInfo rootdir = new DirectoryInfo(Path.GetDirectoryName(rootpath));
        _testCreateRelative(rootpath, rootdir.Parent.FullName, @"../");
    }

    [Test]
    public void CreateRelativeWithSuperParent()
    {
        // use the filename of the declaring assembly as the root...
        string rootpath = GetType().Assembly.Location;
        DirectoryInfo rootdir = new DirectoryInfo(Path.GetDirectoryName(rootpath));
        _testCreateRelative(rootpath, rootdir.Parent.Parent.FullName, @"..\..\");
    }

    [Test]
    public void CreateRelativeInSameDirectory()
    {
        // use the filename of the declaring assembly as the root...
        string rootpath = GetType().Assembly.Location;
        DirectoryInfo rootdir = new DirectoryInfo(Path.GetDirectoryName(rootpath));
        _testCreateRelative(rootpath, rootdir.FullName, @".\");
    }

    [Test]
    public void CreateRelativeInSubdirectoryDirectory()
    {
        string subdirname = "Stuff";
        // use the filename of the declaring assembly as the root...
        string rootpath = GetType().Assembly.Location;
        DirectoryInfo rootdir = new DirectoryInfo(Path.GetDirectoryName(rootpath));
        DirectoryInfo subdir = rootdir.CreateSubdirectory(subdirname);
        try
        {
            _testCreateRelative(rootpath, subdir.FullName, subdirname + "/");
            // let's get obtuse... specify the parent directory of the subdirectory (i.e. the root directory again)
            _testCreateRelative(rootpath, rootdir.FullName, subdirname + "/../");
        }
        finally
        {
            if (subdir.Exists)
            {
                try
                {
                    subdir.Delete();
                }
                catch (IOException)
                {
                }
            }
        }
    }

    /// <summary>
    /// Helper method...
    /// </summary>
    private void _testCreateRelative(string rootPath, string targetPath, string relativePath)
    {
        string filename = "stuff.txt";
        FileInfo file = new FileInfo(Path.GetFullPath(Path.Combine(targetPath, filename)));
        // create a temporary file in whatever 'targetpath' dir we've been passed...
        StreamWriter writer = file.CreateText();
        // test that the CreateRelative () method works with the supplied 'relativePath'
        IResource resource = new FileSystemResource(rootPath);
        IResource relative = resource.CreateRelative(relativePath + filename);
        Assert.That(relative.Exists, Is.True);
        if (file.Exists)
        {
            try
            {
                writer.Close();
                file.Delete();
            }
            catch (IOException)
            {
            }
        }
    }

    [Test]
    public void RelativeLocalFileSystemResourceWhenNotRelative()
    {
        FileSystemResource res = new FileSystemResource(@"C:\dummy.txt");

        IResource rel0 = res.CreateRelative(@"c:\index.html");
        Assert.That(rel0 is FileSystemResource, Is.True);
        Assert.That(rel0.Description, Is.EqualTo(@"file [c:\index.html]"));
    }

    [Test]
    public void RelativeLocalFileSystemResourceFromRoot()
    {
        FileSystemResource res = new FileSystemResource(@"c:\dummy.txt");

        IResource rel0 = res.CreateRelative(@"\index.html");
        Assert.That(rel0 is FileSystemResource, Is.True);
        Assert.That(rel0.Description, Is.EqualTo(@"file [c:\index.html]"));

        IResource rel1 = res.CreateRelative(@"index.html");
        Assert.That(rel1 is FileSystemResource, Is.True);
        Assert.That(rel1.Description, Is.EqualTo(@"file [c:\index.html]"));

        IResource rel2 = res.CreateRelative(@"samples/artfair/index.html");
        Assert.That(rel2 is FileSystemResource, Is.True);
        Assert.That(rel2.Description, Is.EqualTo(@"file [c:\samples\artfair\index.html]"));

        IResource rel3 = res.CreateRelative(@".\samples\artfair\index.html");
        Assert.That(rel3 is FileSystemResource, Is.True);
        Assert.That(rel3.Description, Is.EqualTo(@"file [c:\samples\artfair\index.html]"));
    }

    [Test]
    public void RelativeLocalFileSystemResourceFromSubfolder()
    {
        FileSystemResource res = new FileSystemResource(@"c:\samples\artfair\dummy.txt");

        IResource rel0 = res.CreateRelative(@"/index.html");
        Assert.That(rel0 is FileSystemResource, Is.True);
        Assert.That(rel0.Description, Is.EqualTo(@"file [c:\index.html]"));

        IResource rel1 = res.CreateRelative(@"index.html");
        Assert.That(rel1 is FileSystemResource, Is.True);
        Assert.That(rel1.Description, Is.EqualTo(@"file [c:\samples\artfair\index.html]"));

        IResource rel2 = res.CreateRelative(@"demo\index.html");
        Assert.That(rel2 is FileSystemResource, Is.True);
        Assert.That(rel2.Description, Is.EqualTo(@"file [c:\samples\artfair\demo\index.html]"));

        IResource rel3 = res.CreateRelative(@"./demo/index.html");
        Assert.That(rel3 is FileSystemResource, Is.True);
        Assert.That(rel3.Description, Is.EqualTo(@"file [c:\samples\artfair\demo\index.html]"));

        IResource rel4 = res.CreateRelative(@"../calculator/index.html");
        Assert.That(rel4 is FileSystemResource, Is.True);
        Assert.That(rel4.Description, Is.EqualTo(@"file [c:\samples\calculator\index.html]"));

        IResource rel5 = res.CreateRelative(@"..\..\index.html");
        Assert.That(rel5 is FileSystemResource, Is.True);
        Assert.That(rel5.Description, Is.EqualTo(@"file [c:\index.html]"));
    }

    [Test]
    public void RelativeUncResourceWhenNotRelative()
    {
        FileSystemResource res = new FileSystemResource(@"\\server\share\dummy.txt");

        IResource rel0 = res.CreateRelative(@"\\server\share\index.html");
        Assert.That(rel0 is FileSystemResource, Is.True);
        Assert.That(rel0.Description, Is.EqualTo(@"file [\\server\share\index.html]"));
    }

    [Test]
    public void RelativeUncResourceFromRoot()
    {
        FileSystemResource res = new FileSystemResource(@"\\server\share\dummy.txt");

        IResource rel0 = res.CreateRelative(@"\index.html");
        Assert.That(rel0 is FileSystemResource, Is.True);
        Assert.That(rel0.Description, Is.EqualTo(@"file [\\server\share\index.html]"));

        IResource rel1 = res.CreateRelative(@"index.html");
        Assert.That(rel1 is FileSystemResource, Is.True);
        Assert.That(rel1.Description, Is.EqualTo(@"file [\\server\share\index.html]"));

        IResource rel2 = res.CreateRelative(@"samples/artfair/index.html");
        Assert.That(rel2 is FileSystemResource, Is.True);
        Assert.That(rel2.Description, Is.EqualTo(@"file [\\server\share\samples\artfair\index.html]"));

        IResource rel3 = res.CreateRelative(@".\samples\artfair\index.html");
        Assert.That(rel3 is FileSystemResource, Is.True);
        Assert.That(rel3.Description, Is.EqualTo(@"file [\\server\share\samples\artfair\index.html]"));
    }

    [Test]
    public void RelativeUncResourceFromSubfolder()
    {
        FileSystemResource res = new FileSystemResource(@"\\server\share\samples\artfair\dummy.txt");

        IResource rel0 = res.CreateRelative(@"/index.html");
        Assert.That(rel0 is FileSystemResource, Is.True);
        Assert.That(rel0.Description, Is.EqualTo(@"file [\\server\share\index.html]"));

        IResource rel1 = res.CreateRelative(@"index.html");
        Assert.That(rel1 is FileSystemResource, Is.True);
        Assert.That(rel1.Description, Is.EqualTo(@"file [\\server\share\samples\artfair\index.html]"));

        IResource rel2 = res.CreateRelative(@"demo\index.html");
        Assert.That(rel2 is FileSystemResource, Is.True);
        Assert.That(rel2.Description, Is.EqualTo(@"file [\\server\share\samples\artfair\demo\index.html]"));

        IResource rel3 = res.CreateRelative(@"./demo/index.html");
        Assert.That(rel3 is FileSystemResource, Is.True);
        Assert.That(rel3.Description, Is.EqualTo(@"file [\\server\share\samples\artfair\demo\index.html]"));

        IResource rel4 = res.CreateRelative(@"../calculator/index.html");
        Assert.That(rel4 is FileSystemResource, Is.True);
        Assert.That(rel4.Description, Is.EqualTo(@"file [\\server\share\samples\calculator\index.html]"));

        IResource rel5 = res.CreateRelative(@"..\..\index.html");
        Assert.That(rel5 is FileSystemResource, Is.True);
        Assert.That(rel5.Description, Is.EqualTo(@"file [\\server\share\index.html]"));
    }
}
