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

using System.Text.RegularExpressions;

using Spring.Expressions;
using Spring.Util;

namespace Spring.Validation
{
    /// <summary>
    /// Validates that object matches specified regular expression.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The test expression must evaluate to a <see cref="System.String"/>;
    /// otherwise, an exception is thrown.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class RegularExpressionValidator : BaseSimpleValidator
    {
        #region Fields

        private string expression = string.Empty;
        private bool allowPartialMatching = false;
        private RegexOptions options;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="RegularExpressionValidator"/> class.
        /// </summary>
        public RegularExpressionValidator()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RegularExpressionValidator"/> class.
        /// </summary>
        /// <param name="test">The expression to validate.</param>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        /// <param name="expression">The regular expression to match against.</param>
        public RegularExpressionValidator(string test, string when, string expression)
            : base(test, when)
        {
            AssertUtils.ArgumentHasText(test, "test");
            this.expression = expression;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RegularExpressionValidator"/> class.
        /// </summary>
        /// <param name="test">The expression to validate.</param>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        /// <param name="expression">The regular expression to match against.</param>
        public RegularExpressionValidator(IExpression test, IExpression when, string expression)
            : base(test, when)
        {
            AssertUtils.ArgumentNotNull(test, "test");
            this.expression = expression;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The regular expression <b>text</b> to match against.
        /// </summary>
        /// <value>The regular expression <b>text</b>.</value>
        public string Expression
        {
            get { return expression; }
            set { expression = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to do a partial match instead of a full match.
        /// Default is false.
        /// </summary>
        public bool AllowPartialMatching
        {
            get { return allowPartialMatching; }
            set { allowPartialMatching = value; }
        }

        /// <summary>
        /// The <see cref="RegexOptions"/> for the regular expression evaluation.
        /// </summary>
        /// <value>The regular expression evaluation options.</value>
        /// <seealso cref="RegexOptions"/>
        public RegexOptions Options
        {
            get { return options; }
            set { options = value; }
        }

        #endregion

        /// <summary>
        /// Validates an object.
        /// </summary>
        /// <param name="objectToValidate">Object to validate.</param>
        /// <returns>
        /// <see lang="true"/> if the supplied <paramref name="objectToValidate"/>
        /// object is valid.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="objectToValidate"/> is not a
        /// <see cref="System.String"/>
        /// </exception>
        protected override bool Validate(object objectToValidate)
        {
            string text = objectToValidate as string;

            if (text == null)
            {
                throw new ArgumentException("Test for RegularExpressionValidator must evaluate to a string.");
            }

            if (!StringUtils.HasLength(text))
            {
                return true;
            }

            if (!StringUtils.HasLength(text.Trim()) && !StringUtils.HasLength(expression))
            {
                return false;
            }

            Match match = Regex.Match(text, this.Expression, this.Options);
            if (allowPartialMatching)
            {
                return match.Success;
            }
            else
            {
                return match.Success && match.Index == 0 && match.Length == text.Length;
            }
        }
    }
}
