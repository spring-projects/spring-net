#region License

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

#endregion

using System;
using System.Collections;
using NUnit.Framework;
using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support
{
    [TestFixture]
    public class ManagedListTests
    {
        [Test]
        public void MergeSunnyDay()
        {
            ManagedList parent = new ManagedList();
            parent.Add("one");
            parent.Add("two");
            ManagedList child = new ManagedList();
            child.Add("three");
            child.MergeEnabled = true;
            IList mergedList = (IList) child.Merge(parent);
            Assert.AreEqual(3, mergedList.Count);
        }

        [Test]
        public void MergeWithNullParent()
        {
            ManagedList child = new ManagedList();
            child.Add("one");
            child.MergeEnabled = true;
            Assert.AreSame(child, child.Merge(null));
        }

        [Test]
        public void MergeNotAllowedWhenMergeNotEnabled()
        {
            ManagedList child = new ManagedList();
            Assert.Throws<InvalidOperationException>(() => child.Merge(null), "Not allowed to merge when the 'MergeEnabled' property is set to 'false'");
        }

        [Test]
        public void MergeWithNonCompatibleParentType()
        {
            ManagedList child = new ManagedList();
            child.Add("one");
            child.MergeEnabled = true;
            Assert.Throws<InvalidOperationException>(() => child.Merge("hello"));
        }

        [Test]
        public void MergeEmptyChild()
        {
            ManagedList parent = new ManagedList();
            parent.Add("one");
            parent.Add("two");
            ManagedList child = new ManagedList();
            child.MergeEnabled = true;
            IList mergedList = (IList) child.Merge(parent);
            Assert.AreEqual(2, mergedList.Count);
        }

        [Test]
        public void MergeChildValueOverrideTheParents()
        {
            //doesn't make much sense in the context of a list...
            ManagedList parent = new ManagedList();
            parent.Add("one");
            parent.Add("two");
            ManagedList child = new ManagedList();
            child.Add("one");
            child.MergeEnabled = true;
            IList mergedList = (IList) child.Merge(parent);
            Assert.AreEqual(3, mergedList.Count);
        }
    }
}