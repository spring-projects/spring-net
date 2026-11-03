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

using System.Text;
using NUnit.Framework;
using Spring.Util;

namespace Spring.Core.IO;

/// <summary>
/// </summary>
/// <author>Erich Eichinger</author>
[TestFixture]
public class EncodedResourceTests
{
    [Test]
    public void HashcodeIsCalculatedUsingResourceOnly()
    {
        StringResource testResource = new StringResource("test");
        EncodedResource er1 = new EncodedResource(testResource, Encoding.ASCII, true);
        EncodedResource er2 = new EncodedResource(testResource, Encoding.UTF8, false);

        Assert.That(er1.GetHashCode(), Is.EqualTo(testResource.GetHashCode()));
        Assert.That(er2.GetHashCode(), Is.EqualTo(er1.GetHashCode()));
    }

    [Test]
    public void OpensReaderWithDefaults()
    {
        EncodedResource r = new EncodedResource(new StringResource("test"));
        StreamReader reader = (StreamReader) r.OpenReader();
        Assert.That(reader.CurrentEncoding.EncodingName, Is.EqualTo(Encoding.UTF8.EncodingName));
        Assert.That(reader.ReadToEnd(), Is.EqualTo("test"));
    }

    [Test]
    public void OpensReaderWithAutoDetectEncoding()
    {
        string expected = "test";
        Encoding utf32 = new UTF32Encoding(false, true);
        byte[] resourceData = GetBytes(expected, utf32);
        resourceData = (byte[]) ArrayUtils.Concat(utf32.GetPreamble(), resourceData);
        EncodedResource r = new EncodedResource(new InputStreamResource(new MemoryStream(resourceData), "description"), Encoding.UTF8, true);
        StreamReader reader = (StreamReader) r.OpenReader();
        Assert.That(reader.CurrentEncoding.EncodingName, Is.EqualTo(Encoding.UTF8.EncodingName));
        string actual = reader.ReadToEnd();
        Assert.That(actual, Is.EqualTo("\uFEFF" + expected));
        // interestingly the line below is *not* true!
        //            Assert.That(actual, Is.EqualTo(utf32.GetString(resourceData)));
        Assert.That(reader.CurrentEncoding, Is.EqualTo(utf32));
    }

    [Test]
    public void OpensReaderWithoutAutoDetectEncoding()
    {
        string expected = "test";
        Encoding utf32 = new UTF32Encoding(false, true);
        byte[] resourceData = GetBytes(expected, utf32);
        EncodedResource r = new EncodedResource(new InputStreamResource(new MemoryStream(resourceData), "description"), Encoding.UTF8, false);
        StreamReader reader = (StreamReader) r.OpenReader();
        Assert.That(reader.CurrentEncoding.EncodingName, Is.EqualTo(Encoding.UTF8.EncodingName));
        string actual = reader.ReadToEnd();
        //            Assert.That(actual, Is.EqualTo("\uFFFD\uFFFD\0\0t\0\0\0e\0\0\0s\0\0\0t\0\0\0"));
        Assert.That(actual, Is.EqualTo(Encoding.UTF8.GetString(resourceData)));
        Assert.That(reader.CurrentEncoding.EncodingName, Is.EqualTo(Encoding.UTF8.EncodingName));
    }

    /// <summary>
    /// Returns the text bytes including the encoding's preamble (<see cref="Encoding.GetPreamble"/>), if any.
    /// </summary>
    private byte[] GetBytes(string text, Encoding encoding)
    {
        byte[] resourceData = encoding.GetBytes(text);
        resourceData = (byte[]) ArrayUtils.Concat(encoding.GetPreamble(), resourceData);
        return resourceData;
    }
}
