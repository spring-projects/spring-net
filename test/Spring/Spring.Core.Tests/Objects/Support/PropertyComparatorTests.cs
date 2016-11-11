#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

#region Imports

using System;

using NUnit.Framework;
using Spring.Core;

#endregion

namespace Spring.Objects.Support
{
    /// <summary>
    /// Unit tests for the PropertyComparator class.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The sort algorithm for arrays changed between versions 1.1 and 2.0 of the .NET
    /// Framework. Hence there are two batches of tests in this suite, one for versions
    /// 1.1 and 2.0 of the .NET Framework respectively.
    /// </p>
    /// </remarks>
    /// <author>Rick Evans</author>
    /// <author>Michael Loll</author>
    [TestFixture]
    public sealed class PropertyComparatorTests
    {
        [Test]
        public void InstantiationWithNullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new PropertyComparator(null));
        }

        [Test]
        public void Compare()
        {
            ISortDefinition definition = new MutableSortDefinition("Age", false, true);

            PropertyComparator cmp = new PropertyComparator(definition);
            TestObject one = new TestObject("Rick", 19);
            TestObject two = new TestObject("Balzac", 205);
            int actual = cmp.Compare(one, two);
            Assert.IsTrue(actual < 0); // 19 is less than 205
        }

        [Test]
        public void CompareNestedProperty()
        {
            ISortDefinition definition = new MutableSortDefinition("Lawyer.Company", true, true);

            PropertyComparator cmp = new PropertyComparator(definition);
            TestObject one = new TestObject("Rick", 19);
            one.Lawyer = new NestedTestObject("Gizajoab");
            TestObject two = new TestObject("Balzac", 205);
            two.Lawyer = new NestedTestObject("Wallpaperer");
            int actual = cmp.Compare(one, two);
            Assert.IsTrue(actual < 0); // Gizajoab is less than Wallpaperer
        }

        [Test]
        public void CompareWithNullProperty()
        {
            ISortDefinition definition = new MutableSortDefinition("Lawyer.Company", true, true);

            PropertyComparator cmp = new PropertyComparator(definition);
            TestObject one = new TestObject("Rick", 19);
            one.Lawyer = new NestedTestObject("Gizajoab");
            TestObject two = new TestObject("Balzac", 205);
            // no Lawyer.Company property set on object two...
            int actual = cmp.Compare(one, two);
            Assert.IsTrue(actual > 0); // Gizajoab is greater than null
        }

        [Test]
        public void CompareWithNonExistantProperty()
        {
            ISortDefinition definition = new MutableSortDefinition("Deborahs.Banjo", true, true);

            PropertyComparator cmp = new PropertyComparator(definition);
            TestObject one = new TestObject("Rick", 19);
            TestObject two = new TestObject("Balzac", 205);
            Assert.Throws<InvalidPropertyException>(() => cmp.Compare(one, two));
        }

        [Test]
        public void Sort()
        {
            ISortDefinition definition = new MutableSortDefinition("NAmE", true, true);

            TestObject one = new TestObject("Rick", 19);
            TestObject two = new TestObject("Balzac", 205);
            TestObject three = new TestObject("Jenny", 89);
            TestObject[] actual = new TestObject[] {one, two, three};
            TestObject[] expected = new TestObject[] {two /*Balzac*/, three /*Jenny*/, one /*Rick*/};
            PropertyComparator.Sort(actual, definition);
            for (int i = 0; i < actual.Length; ++i)
            {
                Assert.AreEqual(expected[i].Name, actual[i].Name);
            }
        }

        [Test]
        public void SortInDescendingOrder()
        {
            ISortDefinition definition = new MutableSortDefinition("NAmE", true, false);

            TestObject one = new TestObject("Rick", 19);
            TestObject two = new TestObject("Balzac", 205);
            TestObject three = new TestObject("Jenny", 89);
            TestObject[] actual = new TestObject[] {one, two, three};
            // Rick comes after Jenny comes after Balzac (descending sort order)...
            TestObject[] expected = new TestObject[] {one /*Rick*/, three /*Jenny*/, two /*Balzac*/};
            PropertyComparator.Sort(actual, definition);
            for (int i = 0; i < actual.Length; ++i)
            {
                Assert.AreEqual(expected[i].Name, actual[i].Name);
            }
        }

        [Test]
//        [Ignore("Sort ordering is not preserved (unstable) with equal elements (c.f. System.Array.Sort (Array, IComparer)))")]
        public void OrderingIsUnperturbedWithEqualProps()
        {
            ISortDefinition definition = new MutableSortDefinition("Age", false, true);

            TestObject one = new TestObject("Rick", 19);
            TestObject two = new TestObject("Balzac", 19);
            TestObject three = new TestObject("Jenny", 19);
            TestObject[] actual = new TestObject[] {one, two, three};
            TestObject[] expected = new TestObject[] {one /*Rick*/, two /*Balzac*/, three /*Jenny*/};
            PropertyComparator.Sort(actual, definition);
            for (int i = 0; i < actual.Length; ++i)
            {
                Assert.AreEqual(expected[i].Name, actual[i].Name);
            }
        }

        [Test]
        public void SortWithNullDefinition()
        {
            Assert.Throws<ArgumentNullException>(() => PropertyComparator.Sort(Type.EmptyTypes, null));
        }

        [Test]
        public void CompareWithNullArguments()
        {
            ISortDefinition definition = new MutableSortDefinition("Nails", false, true);

            Funk one = new Funk(1, "long");
            PropertyComparator cmp = new PropertyComparator(definition);
            Assert.AreEqual(0, cmp.Compare(null, null));
            // nulls are always last (i.e. greater than)
            Assert.AreEqual(1, cmp.Compare(null, one));
            // any non-null instance comes before null (i.e. less than).
            Assert.AreEqual(-1, cmp.Compare(one, null));
        }

        /// <summary>
        /// Validates that a PropertyComparator can return its internal ISortDefinition
        /// to a consumer for use.
        /// </summary>
        [Test(Description = "SPRNET-171")]
        public void CanGetSortDefinition()
        {
            ISortDefinition definition = new MutableSortDefinition("Age", false, true);

            PropertyComparator cmp = new PropertyComparator(definition);
            ISortDefinition sortDefinition = cmp.SortDefinition;

            Assert.AreSame(definition, sortDefinition);
            Assert.AreEqual(definition.Property, sortDefinition.Property);
            Assert.AreEqual(definition.Ascending, sortDefinition.Ascending);
            Assert.AreEqual(definition.IgnoreCase, sortDefinition.IgnoreCase);
        }

        internal sealed class Funk
        {
            public Funk(int id, string nails)
            {
                _id = id;
                _nails = nails;
            }

            private string _nails;
            private int _id;

            public string Nails
            {
                get { return _nails; }
            }

            public int Id
            {
                get { return _id; }
                set { _id = value; }
            }
        }

    }
}