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
    /// <see cref="System.Reflection.EventInfo"/> instance matches a
    /// supplied regular expression pattern.
    /// </summary>
    /// <author>Rick Evans</author>
    public class RegularExpressionEventNameCriteria : RegularExpressionCriteria
    {
        #region Constants

        /// <summary>
        /// The default event name pattern... matches pretty much any event name.
        /// </summary>
        private const string MatchAnyEventNamePattern = ".+";

        #endregion

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="RegularExpressionEventNameCriteria"/> class.
        /// </summary>
        public RegularExpressionEventNameCriteria()
            : this(RegularExpressionEventNameCriteria.MatchAnyEventNamePattern)
        {
            Options = RegexOptions.IgnoreCase;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="RegularExpressionEventNameCriteria"/> class.
        /// </summary>
        /// <param name="eventNamePattern">
        /// The pattern that <see cref="System.Reflection.EventInfo"/> names
        /// must match against in order to satisfy this criteria.
        /// </param>
        public RegularExpressionEventNameCriteria(string eventNamePattern)
        {
            Options = RegexOptions.IgnoreCase;
            Pattern = StringUtils.HasText(eventNamePattern) ?
                      eventNamePattern : RegularExpressionEventNameCriteria.MatchAnyEventNamePattern;
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
            EventInfo evt = datum as EventInfo;
            if (evt != null)
            {
                satisfied = IsMatch(evt.Name);
            }
            return satisfied;
        }

        #endregion
    }
}