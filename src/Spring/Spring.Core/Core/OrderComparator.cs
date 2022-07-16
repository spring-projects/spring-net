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

using System.Collections;

#endregion

namespace Spring.Core
{
    /// <summary>
    /// Comparator implementation for <see cref="Spring.Core.IOrdered"/> objects, sorting by
    /// order value ascending (resp. by priority descending).
    /// </summary>
    /// <remarks>
    /// <p>
    /// Non-<see cref="Spring.Core.IOrdered"/> objects are treated as greatest order values,
    /// thus ending up at the end of a list, in arbitrary order (just like same order values of
    /// <see cref="Spring.Core.IOrdered"/> objects).
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Aleksandar Seovic (.Net)</author>
    [Serializable]
    public class OrderComparator : OrderComparator<object>, IComparer
    {
    }

    /// <summary>
    /// Comparator implementation for <see cref="Spring.Core.IOrdered"/> objects, sorting by
    /// order value ascending (resp. by priority descending).
    /// </summary>
    /// <remarks>
    /// <p>
    /// Non-<see cref="Spring.Core.IOrdered"/> objects are treated as greatest order values,
    /// thus ending up at the end of a list, in arbitrary order (just like same order values of
    /// <see cref="Spring.Core.IOrdered"/> objects).
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Aleksandar Seovic (.Net)</author>
    [Serializable]
    public class OrderComparator<T> : IComparer<T>
    {
        public static readonly IComparer<T> Instance = new OrderComparator<T>();

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than,
        /// equal to or greater than the other.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Uses direct evaluation instead of <see cref="System.Int32.CompareTo(object)"/>
        /// to avoid unnecessary boxing.
        /// </p>
        /// </remarks>
        /// <param name="o1">The first object to compare.</param>
        /// <param name="o2">The second object to compare.</param>
        /// <returns>
        /// -1 if first object is less then second, 1 if it is greater, or 0 if they are equal.
        /// </returns>
        public virtual int Compare(T o1, T o2)
        {
            IOrdered o1lhs = o1 as IOrdered;
            IOrdered o2rhs = o2 as IOrdered;
            int lhs = o1lhs?.Order ?? int.MaxValue;
            int rhs = o2rhs?.Order ?? int.MaxValue;
            if (lhs < rhs)
            {
                return - 1;
            }

            if (lhs > rhs)
            {
                return 1;
            }

            return CompareEqualOrder(o1, o2);
        }

        /// <summary>
        /// Handle the case when both objects have equal sort order priority. By default returns 0,
        /// but may be overriden for handling special cases.
        /// </summary>
        /// <param name="o1">The first object to compare.</param>
        /// <param name="o2">The second object to compare.</param>
        /// <returns>
        /// -1 if first object is less then second, 1 if it is greater, or 0 if they are equal.
        /// </returns>
        protected virtual int CompareEqualOrder(T o1, T o2)
        {
            return 0;
        }
    }
}
