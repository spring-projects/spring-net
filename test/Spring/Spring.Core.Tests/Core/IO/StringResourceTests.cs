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

using System.Text;
using NUnit.Framework;

namespace Spring.Core.IO;

/// <summary>
///
/// </summary>
/// <author>Erich Eichinger</author>
[TestFixture]
public class StringResourceTests
{
    [Test]
    public void EnsureDefaults()
    {
        Encoding enc = Encoding.Default;
        string FOO_CONTENT = "foo";
        string FOO_DESCRIPTION = "foo description";

        StringResource r = new StringResource(FOO_CONTENT);
        Assert.That(r.Content, Is.EqualTo(FOO_CONTENT));
        Assert.That(r.Encoding, Is.EqualTo(enc));
        Assert.That(r.Description, Is.Empty);

        enc = new UTF7Encoding();
        r = new StringResource(FOO_CONTENT, enc, FOO_DESCRIPTION);
        Assert.That(r.Content, Is.EqualTo(FOO_CONTENT));
        Assert.That(r.Encoding, Is.EqualTo(enc));
        Assert.That(r.Description, Is.EqualTo(FOO_DESCRIPTION));
    }

    [Test]
    public void ReturnsCorrectEncodedStream()
    {
        string FOO_CONTENT = "foo\u4567";
        StringResource r = new StringResource(FOO_CONTENT, Encoding.GetEncoding("utf-16"));
        Assert.That(r.Content, Is.EqualTo(FOO_CONTENT));
        Stream istm = r.InputStream;
        Assert.That(istm.CanRead, Is.True);

        byte[] chars = new byte[istm.Length];
        istm.Read(chars, 0, chars.Length);
        istm.Close();
        string result = Encoding.GetEncoding("utf-16").GetString(chars);
        Assert.That(result, Is.EqualTo(FOO_CONTENT));
    }

    [Test]
    public void DoesntSupportRelativeResources()
    {
        StringResource r = new StringResource(string.Empty);
        Assert.Throws<NotSupportedException>(() => r.CreateRelative("foo"));
    }

    [Test]
    public void AcceptsNullContent()
    {
        Encoding utf7 = new UTF7Encoding();
        StringResource r = new StringResource(null, utf7);
        Assert.That(r.Content, Is.Empty);
        Stream stm = r.InputStream;
        Assert.That(stm.CanRead, Is.True);
        Assert.That(stm, Is.Not.Null);
        Assert.That(stm.Length, Is.EqualTo(0));
        stm.Close();
    }

    [Test]
    public void AlwaysExists()
    {
        StringResource r = new StringResource(null);
        Assert.That(r.Exists, Is.True);
        r = new StringResource(string.Empty);
        Assert.That(r.Exists, Is.True);
        r = new StringResource("foo");
        Assert.That(r.Exists, Is.True);
    }
}
