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

using NUnit.Framework;
using Spring.Collections;
using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support
{
    [TestFixture]
    public class ManagedSetTests
    {
        [Test]
        public void MergeSunnyDay()
        {
            ManagedSet parent = new ManagedSet();
            parent.Add("one");
            parent.Add("two");
            ManagedSet child = new ManagedSet();
            child.Add("three");
            child.MergeEnabled = true;
            ISet mergedList = (ISet) child.Merge(parent);
            Assert.AreEqual(3, mergedList.Count);
        }

        [Test]
        public void MergeWithNullParent()
        {
            ManagedSet child = new ManagedSet();
            child.Add("one");
            child.MergeEnabled = true;
            Assert.AreSame(child, child.Merge(null));
        }

        [Test]
        public void MergeNotAllowedWhenMergeNotEnabled()
        {
            ManagedSet child = new ManagedSet();
            Assert.Throws<InvalidOperationException>(() => child.Merge(null), "Not allowed to merge when the 'MergeEnabled' property is set to 'false'");
        }

        [Test]
        public void MergeWithNonCompatibleParentType()
        {
            ManagedSet child = new ManagedSet();
            child.Add("one");
            child.MergeEnabled = true;
            Assert.Throws<InvalidOperationException>(() => child.Merge("hello"));
        }

        [Test]
        public void MergeEmptyChild()
        {
            ManagedSet parent = new ManagedSet();
            parent.Add("one");
            parent.Add("two");
            ManagedSet child = new ManagedSet();
            child.MergeEnabled = true;
            ISet mergedSet = (ISet) child.Merge(parent);
            Assert.AreEqual(2, mergedSet.Count);
        }

        [Test]
        public void MergeChildValueOverrideTheParents()
        {
            ManagedSet parent = new ManagedSet();
            parent.Add("one");
            parent.Add("two");
            ManagedSet child = new ManagedSet();
            child.Add("one");
            child.MergeEnabled = true;
            ISet mergedList = (ISet) child.Merge(parent);
            Assert.AreEqual(2, mergedList.Count);
        }
    }
}