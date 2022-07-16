#region License

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

#endregion

#region Imports

using System.Reflection;

#endregion

namespace Spring.Core
{
    /// <summary>
    /// Criteria that is satisfied if the number of generic arguments to a given
    /// <see cref="System.Reflection.MethodBase"/> matches an arbitrary number.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This class supports checking the generic arguments count of both
    /// generic methods and constructors.
    /// </p>
    /// </remarks>
    /// <author>Bruno Baia</author>
    public class MethodGenericArgumentsCountCriteria : ICriteria
    {
        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="MethodGenericArgumentsCountCriteria"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This constructor sets the
        /// <see cref="MethodGenericArgumentsCountCriteria.ExpectedGenericArgumentCount"/>
        /// property to zero (0).
        /// </p>
        /// </remarks>
        public MethodGenericArgumentsCountCriteria() : this(0)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="MethodGenericArgumentsCountCriteria"/> class.
        /// </summary>
        /// <param name="expectedGenericArgumentCount">
        /// The number of generic arguments that a <see cref="System.Reflection.MethodInfo"/>
        /// must have to satisfy this criteria.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="expectedGenericArgumentCount"/> is less
        /// than zero.
        /// </exception>
        public MethodGenericArgumentsCountCriteria(int expectedGenericArgumentCount)
        {
            ExpectedGenericArgumentCount = expectedGenericArgumentCount;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The number of generic arguments that a <see cref="System.Reflection.MethodInfo"/>
        /// must have to satisfy this criteria.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// If the supplied value is less than zero.
        /// </exception>
        public int ExpectedGenericArgumentCount
        {
            get { return _count; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "value", value, "Cannot specify a generic argument count of less than zero.");
                }
                _count = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Does the supplied <paramref name="datum"/> satisfy the criteria encapsulated by
        /// this instance?
        /// </summary>
        /// <param name="datum">The datum to be checked by this criteria instance.</param>
        /// <returns>
        /// True if the supplied <paramref name="datum"/> satisfies the criteria encapsulated
        /// by this instance; false if not or the supplied <paramref name="datum"/> is null.
        /// </returns>
        public bool IsSatisfied(object datum)
        {
            bool satisfied = false;
            MethodBase method = datum as MethodBase;
            if (method != null)
            {
                satisfied = method.GetGenericArguments().Length == ExpectedGenericArgumentCount;
            }
            return satisfied;
        }

        #endregion

        #region Fields

        private int _count;

        #endregion
    }
}
