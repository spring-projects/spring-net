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

using System.Collections.Specialized;
using NUnit.Framework;
using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support;

/// <summary>
/// Integration tests for ManagedNameValueCollectionTests
/// </summary>
/// <author>Mark Pollack</author>
[TestFixture]
public class ManagedNameValueCollectionTests
{
    [Test]
    public void MergeSunnyDay()
    {
        ManagedNameValueCollection parent = new ManagedNameValueCollection();
        parent.Add("one", "one");
        parent.Add("two", "two");
        ManagedNameValueCollection child = new ManagedNameValueCollection();
        child.Add("three", "three");
        child.MergeEnabled = true;
        NameValueCollection mergedList = (NameValueCollection) child.Merge(parent);
        Assert.That(mergedList.Count, Is.EqualTo(3));
    }

    [Test]
    public void MergeWithNullParent()
    {
        ManagedNameValueCollection child = new ManagedNameValueCollection();
        child.MergeEnabled = true;
        Assert.That(child.Merge(null), Is.SameAs(child));
    }

    [Test]
    public void MergeNotAllowedWhenMergeNotEnabled()
    {
        ManagedNameValueCollection child = new ManagedNameValueCollection();
        Assert.Throws<InvalidOperationException>(() => child.Merge(null), "Not allowed to merge when the 'MergeEnabled' property is set to 'false'");
    }

    [Test]
    public void MergeWithNonCompatibleParentType()
    {
        ManagedNameValueCollection child = new ManagedNameValueCollection();
        child.MergeEnabled = true;
        Assert.Throws<InvalidOperationException>(() => child.Merge("hello"));
    }

    [Test]
    public void MergeEmptyChild()
    {
        ManagedNameValueCollection parent = new ManagedNameValueCollection();
        parent.Add("one", "one");
        parent.Add("two", "two");
        ManagedNameValueCollection child = new ManagedNameValueCollection();
        child.MergeEnabled = true;
        NameValueCollection mergedMap = (NameValueCollection) child.Merge(parent);
        Assert.That(mergedMap.Count, Is.EqualTo(2));
    }

    [Test]
    public void MergeChildValueOverrideTheParents()
    {
        ManagedNameValueCollection parent = new ManagedNameValueCollection();
        parent.Add("one", "one");
        parent.Add("two", "two");
        ManagedNameValueCollection child = new ManagedNameValueCollection();
        child.Add("one", "fork");
        child.MergeEnabled = true;
        NameValueCollection mergedMap = (NameValueCollection) child.Merge(parent);
        Assert.That(mergedMap.Count, Is.EqualTo(2));
        Assert.That(mergedMap["one"], Is.EqualTo("fork"));
    }
}
