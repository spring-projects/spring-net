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

using System.Reflection;

using Spring.Util;

#endregion

namespace Spring.Core
{
     /// <summary>
    /// Criteria that is satisfied if the method <c>Name</c> of an
    /// <see cref="System.Reflection.MethodInfo"/> instance matches a
    /// supplied string pattern.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Supports the following simple pattern styles: 
    /// "xxx*", "*xxx" and "*xxx*" matches, as well as direct equality.
    /// </para>
    /// </remarks>
    /// <author>Bruno Baia</author>
    public class MethodNameMatchCriteria : ICriteria
    {
        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="MethodNameMatchCriteria"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This constructor sets the
        /// <see cref="MethodNameMatchCriteria.Pattern"/>
        /// property to * (any method name).
        /// </p>
        /// </remarks>
        public MethodNameMatchCriteria() : this("*")
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="MethodNameMatchCriteria"/> class.
        /// </summary>
        /// The pattern that <see cref="System.Reflection.MethodInfo"/> names
        /// must match against in order to satisfy this criteria.
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="pattern"/> is null or resolve to an empty string.
        /// </exception>
        public MethodNameMatchCriteria(string pattern)
        {
            Pattern = pattern;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The number of parameters that a <see cref="System.Reflection.MethodInfo"/>
        /// must have to satisfy this criteria.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied value is null or resolve to an empty string.
        /// </exception>
        public string Pattern
        {
            get { return pattern; }
            set
            {
                AssertUtils.ArgumentHasText(value, "value");
                pattern = value;
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
                satisfied = PatternMatchUtils.SimpleMatch(pattern.ToLower(), method.Name.ToLower());
            }
            return satisfied;
        }

        #endregion

        #region Fields

        private string pattern;

        #endregion
    }
}