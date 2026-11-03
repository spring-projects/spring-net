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

using System.Collections;
using NUnit.Framework;
using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support;

/// <summary>
/// Integration tests for ManagedDictionary
/// </summary>
/// <author>Erich Eichinger</author>
/// <author>Mark Pollack</author>
[TestFixture]
public class ManagedDictionaryTests
{
    [Test]
    public void MergeSunnyDay()
    {
        ManagedDictionary parent = new ManagedDictionary();
        parent.Add("one", "one");
        parent.Add("two", "two");
        ManagedDictionary child = new ManagedDictionary();
        child.Add("three", "three");
        child.MergeEnabled = true;
        IDictionary mergedList = (IDictionary) child.Merge(parent);
        Assert.That(mergedList.Count, Is.EqualTo(3));
    }

    [Test]
    public void MergeWithNullParent()
    {
        ManagedDictionary child = new ManagedDictionary();
        child.MergeEnabled = true;
        Assert.That(child.Merge(null), Is.SameAs(child));
    }

    [Test]
    public void MergeNotAllowedWhenMergeNotEnabled()
    {
        ManagedDictionary child = new ManagedDictionary();
        Assert.Throws<InvalidOperationException>(() => child.Merge(null), "Not allowed to merge when the 'MergeEnabled' property is set to 'false'");
    }

    [Test]
    public void MergeWithNonCompatibleParentType()
    {
        ManagedDictionary child = new ManagedDictionary();
        child.MergeEnabled = true;
        Assert.Throws<InvalidOperationException>(() => child.Merge("hello"));
    }

    [Test]
    public void MergeEmptyChild()
    {
        ManagedDictionary parent = new ManagedDictionary();
        parent.Add("one", "one");
        parent.Add("two", "two");
        ManagedDictionary child = new ManagedDictionary();
        child.MergeEnabled = true;
        IDictionary mergedMap = (IDictionary) child.Merge(parent);
        Assert.That(mergedMap.Count, Is.EqualTo(2));
    }

    [Test]
    public void MergeChildValueOverrideTheParents()
    {
        ManagedDictionary parent = new ManagedDictionary();
        parent.Add("one", "one");
        parent.Add("two", "two");
        ManagedDictionary child = new ManagedDictionary();
        child.Add("one", "fork");
        child.MergeEnabled = true;
        IDictionary mergedMap = (IDictionary) child.Merge(parent);
        Assert.That(mergedMap.Count, Is.EqualTo(2));
        Assert.That(mergedMap["one"], Is.EqualTo("fork"));
    }

    internal class InternalType
    {
        public string Value = "OK";
    }

    [Test]
    public void ResolvesInternalGenericTypes()
    {
        ManagedDictionary dict2 = new ManagedDictionary();
        dict2.Add("1", "stringValue");
        dict2.KeyTypeName = "int";
        dict2.ValueTypeName = typeof(InternalType).FullName;

        IDictionary resolved = (IDictionary) dict2.Resolve("other", new RootObjectDefinition(typeof(object)), "prop",
            delegate(string name, IObjectDefinition definition, string argumentName, object element)
            {
                if ("stringValue".Equals(element))
                {
                    return new InternalType();
                }

                return element;
            }
        );
        Assert.That(resolved[1].GetType(), Is.EqualTo(typeof(InternalType)));
    }

    [Test]
    public void ResolvesGenericTypeNames()
    {
        ManagedDictionary dict = new ManagedDictionary();
        dict.Add("key", "value");
        dict.KeyTypeName = "string";

        dict.ValueTypeName = "System.Collections.Generic.List<[string]>";
        IDictionary resolved = (IDictionary) dict.Resolve("somename", new RootObjectDefinition(typeof(object)), "prop",
            delegate(string name, IObjectDefinition definition, string argumentName, object element)
            {
                if ("value".Equals(element))
                {
                    return new List<string>();
                }

                return element;
            }
        );
        Assert.That(resolved.Count, Is.EqualTo(1));
        Assert.That(resolved["key"].GetType(), Is.EqualTo(typeof(List<string>)));
    }

    [Test]
    public void ResolvesMergedGenericType()
    {
        ManagedDictionary parent = new ManagedDictionary();
        parent.Add("one", 1);
        parent.Add("two", 2);
        parent.KeyTypeName = "string";
        parent.ValueTypeName = "int";

        ManagedDictionary child = new ManagedDictionary();
        child.MergeEnabled = true;
        child.Add("one", -1);
        child.Add("three", 3);

        ManagedDictionary merged = (ManagedDictionary) child.Merge(parent);

        IDictionary resolved = (IDictionary) merged.Resolve("somename", new RootObjectDefinition(typeof(object)), "prop",
            (name, definition, argumentName, element) => element);

        Assert.That(resolved, Is.InstanceOf<IDictionary<string, int>>());
        Assert.That(resolved.Count, Is.EqualTo(3));
        Assert.That(resolved["two"].GetType(), Is.EqualTo(typeof(int)));
        Assert.That(resolved["one"], Is.EqualTo(-1));
    }
}
