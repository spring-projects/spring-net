/*
 * Copyright 2004 the original author or authors.
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

using System.Net;
using NUnit.Framework;

namespace Spring.Core.TypeConversion;

/// <summary>
/// Unit tests for the StreamConverter class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class StreamConverterTests
{
    [Test]
    public void CanConvertFrom()
    {
        StreamConverter vrt = new StreamConverter();
        Assert.That(vrt.CanConvertFrom(typeof(string)),
            Is.True,
            "Conversion from a string instance must be supported.");
        Assert.That(vrt.CanConvertFrom(typeof(int)), Is.False);
    }

    [Test]
    [Explicit] // requires one to be connected to the 'net...
    public void ConvertFrom()
    {
        StreamConverter vrt = new StreamConverter();
        Stream actual = vrt.ConvertFrom("http://www.springframework.net/") as Stream;
        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    [Explicit] // fails if there is a transparent proxy that redirects to error page for non existing URL
    public void ConvertFromValidButNonExistingStreamResource()
    {
        Assert.Throws<WebException>(() => new StreamConverter().ConvertFrom("http://www.aaaabbbbccccddd.com"));
    }

    [Test]
    public void ConvertFromNullReference()
    {
        Assert.Throws<NotSupportedException>(() => new StreamConverter().ConvertFrom(null));
    }

    [Test]
    public void ConvertFromNonSupportedOptionBails()
    {
        Assert.Throws<NotSupportedException>(() => new StreamConverter().ConvertFrom(12));
    }
}
