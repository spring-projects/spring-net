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

namespace Spring.Util;

/// <summary>
/// Unit tests for Spring.Util.Properties.
/// </summary>
[TestFixture]
public class PropertiesTests
{
    [Test]
    public void Instantiation()
    {
        Properties root = new Properties();
        root.Add("foo", "this");
        root.Add("bar", "is");
        Properties props = new Properties(root);
        props.SetProperty("myPropertyKey", "myPropertyValue");
        Assert.That(props.Count, Is.EqualTo(3));
        Assert.That(props.GetProperty("foo"), Is.EqualTo("this"));
        Assert.That(props.GetProperty("bar"), Is.EqualTo("is"));
        Assert.That(props.GetProperty("myPropertyKey"), Is.EqualTo("myPropertyValue"));
    }

    [Test]
    public void GetPropertyWithDefault()
    {
        Properties props = new Properties();
        props.Add("foo", "this");
        props.Add("bar", "is");
        Assert.That(props.GetProperty("foo"), Is.EqualTo("this"));
        Assert.That(props.GetProperty("bar"), Is.EqualTo("is"));
        Assert.That(props.GetProperty("baz", "it"), Is.EqualTo("it"));
    }

    [Test]
    public void Remove()
    {
        Properties props = new Properties();
        props.Add("foo", "this");
        props.Add("bar", "is");
        Assert.That(props.Count, Is.EqualTo(2));
        props.Remove("foo");
        Assert.That(props.Count, Is.EqualTo(1));
        Assert.That(props.ContainsKey("foo"), Is.False);
    }

    [Test]
    public void Store()
    {
        Properties props = new Properties();
        props.Add("foo", "this");
        props.Add("bar", "is");
        props.Add("baz", "it");
        FileInfo file = new FileInfo("properties.test");
        try
        {
            // write 'em out with the specified header...
            using (Stream cout = file.OpenWrite())
            {
                props.Store(cout, "My Properties");
            }
        }
        finally
        {
            try
            {
                file.Delete();
            }
            catch (IOException)
            {
            }
        }
    }

    [Test]
    public void ListAndLoad()
    {
        Properties props = new Properties();
        props.Add("foo", "this");
        props.Add("bar", "is");
        props.Add("baz", "it");
        FileInfo file = new FileInfo("properties.test");
        try
        {
            // write 'em out...
            using (Stream cout = file.OpenWrite())
            {
                props.List(cout);
            }

            // read 'em back in...
            using (Stream cin = file.OpenRead())
            {
                props = new Properties();
                props.Load(cin);
                Assert.That(props.Count, Is.EqualTo(3));
                Assert.That(props.GetProperty("foo"), Is.EqualTo("this"));
                Assert.That(props.GetProperty("bar"), Is.EqualTo("is"));
                Assert.That(props.GetProperty("baz", "it"), Is.EqualTo("it"));
            }
        }
        finally
        {
            try
            {
                file.Delete();
            }
            catch (IOException)
            {
            }
        }
    }

    [Test]
    public void SimpleProperties()
    {
        string input = "key1=value1\r\nkey2:value2\r\n\r\n# a comment line\r\n   leadingspace : true";
        Stream s = new MemoryStream(Encoding.ASCII.GetBytes(input));
        Properties props = new Properties();
        props.Load(s);

        Assert.That("value1".Equals(props["key1"]), Is.True);
        Assert.That("value2".Equals(props["key2"]), Is.True);
        Assert.That("true".Equals(props["leadingspace"]), Is.True);
    }

    [Test]
    public void WhitespaceProperties()
    {
        string input = "key1 =\t\nkey2:\nkey3";

        Stream s = new MemoryStream(Encoding.ASCII.GetBytes(input));
        Properties props = new Properties();
        props.Load(s);

        Assert.That(props["key1"], Is.Empty, "key1 should have empty value");
        Assert.That(props["key2"], Is.Empty, "key2 should have empty value");
        Assert.That(props.ContainsKey("key3"), Is.True);
        Assert.That(props["key3"], Is.Null);
    }

    [Test]
    public void Continuation()
    {
        string input = "continued = this is a long value element \\\r\nthat uses continuation \\\r\n    xxx";
        Stream s = new MemoryStream(Encoding.ASCII.GetBytes(input));
        Properties props = new Properties();
        props.Load(s);

        Assert.That("this is a long value element that uses continuation xxx".Equals(props["continued"]), Is.True);
    }

    [Test]
    public void SeperatorEscapedWithinKey()
    {
        string input = "\\" + ":key:newvalue";
        Stream s = new MemoryStream(Encoding.ASCII.GetBytes(input));
        Properties props = new Properties();
        props.Load(s);

        Assert.That("newvalue".Equals(props[":key"]), Is.True);
    }

    [Test]
    public void EscapedCharactersInValue()
    {
        string input = "escaped=test\\ttest";
        Stream s = new MemoryStream(Encoding.ASCII.GetBytes(input));
        Properties props = new Properties();
        props.Load(s);

        Assert.That("test\ttest".Equals(props["escaped"]), Is.True);
    }
}
