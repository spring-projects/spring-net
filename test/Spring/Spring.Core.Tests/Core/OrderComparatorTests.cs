#region License

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

#endregion

#region Imports

using System;

using NUnit.Framework;
using Spring.Util;

#endregion

namespace Spring.Core
{
	/// <summary>
	/// Unit tests for the OrderComparator class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class OrderComparatorTests
    {
        [Test]
        public void OrdersCorrectly ()
        {
            Ordered one = new Ordered (1);
            Ordered fifty = new Ordered (50);
            string max = "Max"; // should be stuck at the end 'cos it doesnt implement IOrdered
            object [] list = new object [] {max, one, fifty};
            Array.Sort (list, new OrderComparator ());
            Assert.AreEqual (one, list [0]);
            Assert.AreEqual (fifty, list [1]);
            Assert.AreEqual (max, list [2]);
        }

        [Test]
        public void DoesntBailWhenFedNulls ()
        {
            Ordered one = new Ordered (1);
            object [] list = new object [] {null, one, null};
            ArrayUtils.Sort(list, new OrderComparator ());
            Assert.AreEqual (one, list [0], "order comparator instance should be first");
            Assert.AreEqual (null, list [1]);
            Assert.AreEqual (null, list [2]);
        }

        internal sealed class Ordered : IOrdered
        {
            public Ordered (int order) 
            {
                _order = order;
            }

            public int Order
            {
                get
                {
                    return _order;
                }
            }

            private int _order;
        }
	}
}
