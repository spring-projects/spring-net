#region License

/*
 * Copyright 2002-2009 the original author or authors.
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

namespace Spring.Validation.Validators
{
    /// <summary>
    /// Validates that the value is valid URL.
    /// </summary>
    /// <author>Goran Milosavljevic</author>
    public class UrlValidator : BaseSimpleValidator
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of the <b>UrlValidator</b> class.
        /// </summary>
        public UrlValidator()
        {
        }

        /// <summary>
        /// Creates a new instance of the <b>UrlValidator</b> class.
        /// </summary>
        /// <param name="test">The expression to validate.</param>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public UrlValidator(string test, string when)
            : base(test, when)
        {
            AssertUtils.ArgumentHasText(test, "test");
        }

        /// <summary>
        /// Creates a new instance of the <b>UrlValidator</b> class.
        /// </summary>
        /// <param name="test">The expression to validate.</param>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public UrlValidator(IExpression test, IExpression when)
            : base(test, when)
        {
            AssertUtils.ArgumentNotNull(test, "test");
        }

        #endregion

        #region BaseValidator methods

        /// <summary>
        /// Validates the supplied <paramref name="objectToValidate"/>.
        /// </summary>
        /// <remarks>
        /// In the case of the <see cref="UrlValidator"/> class,
        /// the test should be a string variable that will be evaluated and the object
        /// obtained as a result of this evaluation will be tested using the URL validation rules.
        /// </remarks>
        /// <param name="objectToValidate">The object to validate.</param>
        /// <returns>
        /// <see lang="true"/> if the supplied <paramref name="objectToValidate"/> is valid.
        /// </returns>
        protected override bool Validate(object objectToValidate)
        {
            string text = objectToValidate as string;
            if (StringUtils.IsNullOrEmpty(text))
            {
                return true;
            }

            Match match = Regex.Match(text, urlCheck);
            return match.Success && match.Index == 0 && match.Length == text.Length;
        }

        #endregion

        #region Data members

        /// <summary>
        /// Regular expression used for validation of object passed to this <see cref="UrlValidator"/>.
        /// </summary>
        private static string urlCheck = "((http|https)://)?[a-z0-9]+([-.]{1}[a-z0-9]+)*.[a-z]{2,5}(([0-9]{1,5})?/.*)?";
        
        #endregion
    }
}
