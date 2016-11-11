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

using System.Text.RegularExpressions;

using Spring.Util;

#endregion

namespace Spring.Core
{
    /// <summary>
    /// A base class for all <see cref="Spring.Core.ICriteria"/>
    /// implementations that are regular expression based.
    /// </summary>
    /// <author>Rick Evans</author>
    public abstract class RegularExpressionCriteria : ICriteria
    {
        #region Constants

        /// <summary>
        /// The default pattern... matches absolutely anything.
        /// </summary>
        protected const string MatchAnyThingPattern = ".*";

        #endregion

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="RegularExpressionCriteria"/> class.
        /// </summary>
        protected RegularExpressionCriteria() : this(RegularExpressionCriteria.MatchAnyThingPattern)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="RegularExpressionCriteria"/> class.
        /// </summary>
        /// <param name="pattern">
        /// The regular expression pattern to be applied.
        /// </param>
        protected RegularExpressionCriteria(string pattern)
        {
            Pattern = pattern;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The regular expression pattern to be applied.
        /// </summary>
        public string Pattern
        {
            get { return _pattern; }
            set
            {
                _pattern = StringUtils.HasText(value) ?
                           value : RegularExpressionCriteria.MatchAnyThingPattern;
                Expression = new Regex(Pattern, Options);
            }
        }

        /// <summary>
        /// The regular expression options to be applied.
        /// </summary>
        public RegexOptions Options
        {
            get { return _options; }
            set { _options = value; }
        }

        /// <summary>
        /// The regular expression to be applied.
        /// </summary>
        public Regex Expression
        {
            get { return _expression; }
            set { _expression = value; }
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
        public abstract bool IsSatisfied(object datum);

        /// <summary>
        /// Convenience method that calls the
        /// <see cref="System.Text.RegularExpressions.Regex.IsMatch(string)"/>
        /// on the supplied <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The input to match against.</param>
        /// <returns>True if the <paramref name="input"/> matches.</returns>
        protected bool IsMatch(string input)
        {
            return Expression.IsMatch(input);
        }

        #endregion

        #region Fields

        private string _pattern;
        private RegexOptions _options;
        private Regex _expression;

        #endregion
    }
}