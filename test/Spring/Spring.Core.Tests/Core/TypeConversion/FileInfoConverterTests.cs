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

using NUnit.Framework;

namespace Spring.Core.TypeConversion;

/// <summary>
/// Unit tests for the FileInfoConverter class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class FileInfoConverterTests
{
    [Test]
    public void CanConvertFrom()
    {
        FileInfoConverter vrt = new FileInfoConverter();
        Assert.That(vrt.CanConvertFrom(typeof(string)), Is.True, "Conversion from a string instance must be supported.");
        Assert.That(vrt.CanConvertFrom(typeof(int)), Is.False);
    }

    [Test]
    public void ConvertFrom()
    {
        FileInfoConverter vrt = new FileInfoConverter();
        object file = vrt.ConvertFrom("././ManAhShoahDoLoveThoseGrits");
        Assert.That(file, Is.Not.Null);
        Assert.That(file.GetType(), Is.EqualTo(typeof(FileInfo)));
    }

    [Test]
    public void FileConverter()
    {
        FileInfoConverter converter = new FileInfoConverter();
        object file = converter.ConvertFrom("C:/test/myfile.txt");
        Assert.That(file, Is.Not.Null);
        Assert.That(file is FileInfo, Is.True);
        Assert.That(((FileInfo) file).FullName, Is.EqualTo(new FileInfo("C:/test/myfile.txt").FullName));
    }

    [Test]
    public void ConvertFromNullReference()
    {
        FileInfoConverter vrt = new FileInfoConverter();
        Assert.Throws<NotSupportedException>(() => vrt.ConvertFrom(null));
    }

    [Test]
    public void ConvertFromNonSupportedOptionBails()
    {
        FileInfoConverter vrt = new FileInfoConverter();
        Assert.Throws<NotSupportedException>(() => vrt.ConvertFrom(12));
    }
}
