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
    /// Criteria that is satisfied if the number of parameters to a given
    /// <see cref="System.Reflection.MethodBase"/> matches an arbitrary number.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This class supports checking the parameter count of both methods and
    /// constructors.
    /// </p>
    /// <p>
    /// Default parameters, etc need to taken into account.
    /// </p>
    /// </remarks>
    /// <author>Rick Evans</author>
    public class MethodParametersCountCriteria : ICriteria
    {
        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="MethodParametersCountCriteria"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This constructor sets the
        /// <see cref="MethodParametersCountCriteria.ExpectedParameterCount"/>
        /// property to zero (0).
        /// </p>
        /// </remarks>
        public MethodParametersCountCriteria() : this(0)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="MethodParametersCountCriteria"/> class.
        /// </summary>
        /// <param name="expectedParameterCount">
        /// The number of parameters that a <see cref="System.Reflection.MethodInfo"/>
        /// must have to satisfy this criteria.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="expectedParameterCount"/> is less
        /// than zero.
        /// </exception>
        public MethodParametersCountCriteria(int expectedParameterCount)
        {
            ExpectedParameterCount = expectedParameterCount;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The number of parameters that a <see cref="System.Reflection.MethodInfo"/>
        /// must have to satisfy this criteria.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// If the supplied value is less than zero.
        /// </exception>
        public int ExpectedParameterCount
        {
            get { return _count; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "value", value, "Cannot specify a parameter count of less than zero.");
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
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == ExpectedParameterCount)
                {
                    satisfied = true;
                }
                else if ((parameters.Length > 0) && (ExpectedParameterCount >= parameters.Length-1))
                {
                    ParameterInfo lastParameter = parameters[parameters.Length - 1];
                    satisfied = lastParameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
                }
            }
            return satisfied;
        }

        #endregion

        #region Fields

        private int _count;

        #endregion
    }
}
