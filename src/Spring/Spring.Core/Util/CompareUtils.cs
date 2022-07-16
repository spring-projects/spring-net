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

#region Imports

#endregion

namespace Spring.Util
{
    /// <summary>
    /// Utility class containing helper methods for object comparison.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class CompareUtils
    {
        /// <summary>Compares two objects.</summary>
        /// <param name="first">First object.</param>
        /// <param name="second">Second object.</param>
        /// <returns>
        /// 0, if objects are equal;
        /// less than zero, if the first object is smaller than the second one;
        /// greater than zero, if the first object is greater than the second one.</returns>
        public static int Compare(object first, object second)
        {
            // anything is greater than null, unless both operands are null
            if (first == null)
            {
                return (second == null ? 0 : -1);
            }
            else if (second == null)
            {
                return 1;
            }

            if (!first.GetType().Equals(second.GetType()))
            {
                if (!CoerceTypes(ref first, ref second))
                {
                    throw new ArgumentException("Cannot compare instances of ["
                        + first.GetType().FullName
                        + "] and ["
                        + second.GetType().FullName
                        + "] because they cannot be coerced to the same type.");
                }
            }

            if (first is IComparable)
            {
                return ((IComparable)first).CompareTo(second);
            }
            else
            {
                throw new ArgumentException("Cannot compare instances of the type ["
                    + first.GetType().FullName
                    + "] because it doesn't implement IComparable");
            }
        }

        private static bool CoerceTypes(ref object left, ref object right)
        {
            if (NumberUtils.IsNumber(left) && NumberUtils.IsNumber(right))
            {
                NumberUtils.CoerceTypes(ref right, ref left);
                return true;
            }
            return false;
        }

    }
}
