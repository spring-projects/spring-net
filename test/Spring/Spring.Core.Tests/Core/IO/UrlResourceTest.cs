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
/// Unit tests for the UrlResource class.
/// </summary>
[TestFixture]
public sealed class UrlResourceTests
{
    private const string FILE_PROTOCOL_PREFIX = "file:///";

    [Test]
    public void CreateUrlResourceWithGivenPath()
    {
        UrlResource urlResource = new UrlResource(FILE_PROTOCOL_PREFIX + "C:/temp");
        Assert.That(urlResource.Uri.AbsolutePath, Is.EqualTo("C:/temp"));
    }

    [Test]
    public void CreateInvalidUrlResource()
    {
        string uri = null;
        Assert.Throws<ArgumentNullException>(() => new UrlResource(uri));
    }

    [Test]
    [Platform("Win")]
    public void GetValidFileInfo()
    {
        UrlResource urlResource = new UrlResource(FILE_PROTOCOL_PREFIX + "C:/temp");
        Assert.That(urlResource.File.FullName, Is.EqualTo("C:\\temp"));
    }

    [Test, Explicit]
    public void ExistsValidHttp()
    {
        UrlResource urlResource = new UrlResource("http://www.springframework.net/");
        Assert.That(urlResource.Exists, Is.True);
    }

    [Test]
    public void GetInvalidFileInfo()
    {
        UrlResource urlResource = new UrlResource("http://www.springframework.net/");
        FileInfo file;
        Assert.Throws<FileNotFoundException>(() => file = urlResource.File);
    }

    [Test]
    public void GetInvalidFileInfoWithOddPort()
    {
        UrlResource urlResource = new UrlResource("http://www.springframework.net:76/");
        FileInfo temp;
        Assert.Throws<FileNotFoundException>(() => temp = urlResource.File);
    }

    [Test]
    public void GetDescription()
    {
        UrlResource urlResource = new UrlResource(FILE_PROTOCOL_PREFIX + "C:/temp");
        Assert.That(urlResource.Description, Is.EqualTo("URL [file:///C:/temp]"));
    }

    [Test]
    public void GetValidInputStreamForFileProtocol()
    {
        string fileName = Path.GetTempFileName();
        FileStream fs = File.Create(fileName);
        fs.Close();
        using (Stream inputStream = new UrlResource(FILE_PROTOCOL_PREFIX + fileName).InputStream)
        {
            Assert.That(inputStream.CanRead, Is.True);
        }
    }

    [Test]
    public void RelativeResourceFromRoot()
    {
        UrlResource res = new UrlResource("http://www.springframework.net/documentation.html");

        IResource rel0 = res.CreateRelative("/index.html");
        Assert.That(rel0 is UrlResource, Is.True);
        Assert.That(rel0.Description, Is.EqualTo("URL [http://www.springframework.net/index.html]"));

        IResource rel1 = res.CreateRelative("index.html");
        Assert.That(rel1 is UrlResource, Is.True);
        Assert.That(rel1.Description, Is.EqualTo("URL [http://www.springframework.net/index.html]"));

        IResource rel2 = res.CreateRelative("samples/artfair/index.html");
        Assert.That(rel2 is UrlResource, Is.True);
        Assert.That(rel2.Description, Is.EqualTo("URL [http://www.springframework.net/samples/artfair/index.html]"));

        IResource rel3 = res.CreateRelative("./samples/artfair/index.html");
        Assert.That(rel3 is UrlResource, Is.True);
        Assert.That(rel3.Description, Is.EqualTo("URL [http://www.springframework.net/samples/artfair/index.html]"));
    }

    [Test]
    public void RelativeResourceFromSubfolder()
    {
        UrlResource res = new UrlResource("http://www.springframework.net/samples/artfair/download.html");

        IResource rel0 = res.CreateRelative("/index.html");
        Assert.That(rel0 is UrlResource, Is.True);
        Assert.That(rel0.Description, Is.EqualTo("URL [http://www.springframework.net/index.html]"));

        IResource rel1 = res.CreateRelative("index.html");
        Assert.That(rel1 is UrlResource, Is.True);
        Assert.That(rel1.Description, Is.EqualTo("URL [http://www.springframework.net/samples/artfair/index.html]"));

        IResource rel2 = res.CreateRelative("demo/index.html");
        Assert.That(rel2 is UrlResource, Is.True);
        Assert.That(rel2.Description, Is.EqualTo("URL [http://www.springframework.net/samples/artfair/demo/index.html]"));

        IResource rel3 = res.CreateRelative("./demo/index.html");
        Assert.That(rel3 is UrlResource, Is.True);
        Assert.That(rel3.Description, Is.EqualTo("URL [http://www.springframework.net/samples/artfair/demo/index.html]"));

        IResource rel4 = res.CreateRelative("../calculator/index.html");
        Assert.That(rel4 is UrlResource, Is.True);
        Assert.That(rel4.Description, Is.EqualTo("URL [http://www.springframework.net/samples/calculator/index.html]"));

        IResource rel5 = res.CreateRelative("../../index.html");
        Assert.That(rel5 is UrlResource, Is.True);
        Assert.That(rel5.Description, Is.EqualTo("URL [http://www.springframework.net/index.html]"));
    }

    [Test]
    public void RelativeResourceTooManyBackLevels()
    {
        UrlResource res = new UrlResource("http://www.springframework.net/samples/artfair/download.html");
        Assert.Throws<UriFormatException>(() => res.CreateRelative("../../../index.html"));
    }
}
