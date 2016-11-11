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
using System.Text.RegularExpressions;

using Spring.Util;

#endregion

namespace Spring.Core
{
    /// <summary>
    /// Criteria that is satisfied if the <c>Name</c> property of an
    /// <see cref="System.Reflection.MethodInfo"/> instance matches a
    /// supplied regular expression pattern.
    /// </summary>
    /// <author>Rick Evans</author>
    public class RegularExpressionMethodNameCriteria : RegularExpressionCriteria
    {
        #region Constants

        /// <summary>
        /// The default method name pattern... matches pretty much any method name.
        /// </summary>
        private const string MatchAnyMethodNamePattern = ".+";

        #endregion

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="RegularExpressionMethodNameCriteria"/> class.
        /// </summary>
        public RegularExpressionMethodNameCriteria()
            : this(RegularExpressionMethodNameCriteria.MatchAnyMethodNamePattern)
        {
            Options = RegexOptions.IgnoreCase;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="RegularExpressionMethodNameCriteria"/> class.
        /// </summary>
        /// <param name="methodNamePattern">
        /// The pattern that <see cref="System.Reflection.MethodInfo"/> names
        /// must match against in order to satisfy this criteria.
        /// </param>
        public RegularExpressionMethodNameCriteria(string methodNamePattern)
        {
            Options = RegexOptions.IgnoreCase;
            Pattern = StringUtils.HasText(methodNamePattern) ?
                      methodNamePattern : RegularExpressionMethodNameCriteria.MatchAnyMethodNamePattern;
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
        public override bool IsSatisfied(object datum)
        {
            bool satisfied = false;
            MethodInfo method = datum as MethodInfo;
            if (method != null)
            {
                satisfied = IsMatch(method.Name);
            }
            return satisfied;
        }

        #endregion
    }
}