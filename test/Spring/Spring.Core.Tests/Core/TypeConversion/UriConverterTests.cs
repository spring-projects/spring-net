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

using NUnit.Framework;

namespace Spring.Core.TypeConversion;

/// <summary>
/// Unit tests for the UriConverter class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class UriConverterTests
{
    [Test]
    public void CanConvertFrom()
    {
        UriConverter vrt = new UriConverter();
        Assert.That(vrt.CanConvertFrom(typeof(string)), Is.True, "Conversion from a string instance must be supported.");
        Assert.That(vrt.CanConvertFrom(typeof(int)), Is.False);
    }

    [Test]
    public void ConvertFrom()
    {
        UriConverter vrt = new UriConverter();
        object actual = vrt.ConvertFrom("svn://localhost/Spring/trunk/");
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.GetType(), Is.EqualTo(typeof(System.Uri)));
    }

    [Test]
    public void ConvertFromMalformedUriBails()
    {
        try
        {
            UriConverter vrt = new UriConverter();
            object actual = vrt.ConvertFrom("$TheAngelGang");
        }
        catch (Exception ex)
        {
            // check that the inner exception was doe to the malformed URL
            Assert.That(ex.InnerException is UriFormatException, Is.True);
        }
    }

    [Test]
    public void ConvertFromNullReference()
    {
        UriConverter vrt = new UriConverter();
        Assert.Throws<NotSupportedException>(() => vrt.ConvertFrom(null));
    }

    [Test]
    public void ConvertFromNonSupportedOptionBails()
    {
        UriConverter vrt = new UriConverter();
        Assert.Throws<NotSupportedException>(() => vrt.ConvertFrom(12));
    }
}
