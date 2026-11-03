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

namespace Spring.Web.Support;

/// <summary>
/// Tests the MediaType class according to http://www.iana.org/assignments/media-types/
/// </summary>
/// <author>Erich Eichinger</author>
[TestFixture]
public class MimeMediaTypeTests
{
    [Test]
    public void CreateInstance()
    {
        Assert.That(new MimeMediaType(), Is.EqualTo(MimeMediaType.Application.Octet));
        MimeMediaType mt = new MimeMediaType("ApPlicatioN");
        Assert.That(mt.ContentType, Is.EqualTo("application"));
        Assert.That(mt.SubType, Is.EqualTo("*"));

        Assert.That(new MimeMediaType("ApPlicatioN", null).SubType, Is.EqualTo("*"));
        Assert.That(new MimeMediaType("ApPlicatioN", "").SubType, Is.EqualTo("*"));
        Assert.That(new MimeMediaType("ApPlicatioN", "bla").SubType, Is.EqualTo("bla"));
    }

    [Test]
    public void ToStringReturnsMimeTypeString()
    {
        Assert.That(new MimeMediaType("ApPlicatioN", "SubType").ToString(), Is.EqualTo("application/subtype"));
    }

    [Test]
    public void Parse()
    {
        MimeMediaType mt = MimeMediaType.Parse("text/html");
        Assert.That(mt.ContentType, Is.EqualTo("text"));
        Assert.That(mt.SubType, Is.EqualTo("html"));

        try
        {
            mt = MimeMediaType.Parse(null);
            Assert.Fail();
        }
        catch (ArgumentNullException)
        {
            /* ok */
        }

        try
        {
            mt = MimeMediaType.Parse("");
            Assert.Fail();
        }
        catch (ArgumentException)
        {
            /* ok */
        }

        try
        {
            mt = MimeMediaType.Parse("aasdfaDF");
            Assert.Fail();
        }
        catch (ArgumentException)
        {
            /* ok */
        }

        try
        {
            mt = MimeMediaType.Parse("*");
            Assert.Fail();
        }
        catch (ArgumentException)
        {
            /* ok */
        }
    }

    [Test]
    public void Equals()
    {
        MimeMediaType mt = MimeMediaType.Parse("text/html");
        Assert.That(mt, Is.EqualTo(MimeMediaType.Text.Html));
        Assert.That(mt, Is.EqualTo(MimeMediaType.Parse("text/html")));

        Assert.That(mt, Is.Not.EqualTo(MimeMediaType.Parse("text/xml")));
    }

    [Test]
    public void GetHashcode()
    {
        MimeMediaType mt = MimeMediaType.Parse("text/html");
        Assert.That(mt.GetHashCode(), Is.EqualTo(MimeMediaType.Text.Html.GetHashCode()));
        Assert.That(mt.GetHashCode(), Is.EqualTo(MimeMediaType.Parse("text/html").GetHashCode()));

        Assert.That(mt.GetHashCode(), Is.Not.EqualTo(MimeMediaType.Parse("text/xml").GetHashCode()));
    }
}
