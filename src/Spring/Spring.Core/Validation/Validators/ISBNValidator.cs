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

using System.Text.RegularExpressions;
using Spring.Expressions;
using Spring.Util;

namespace Spring.Validation.Validators
{
    /// <summary>
    /// Validates that the object is valid ISBN-10 or ISBN-13 value.
    /// </summary>
    /// <author>Goran Milosavljevic</author>
    public class ISBNValidator : BaseSimpleValidator
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of the <b>ISBNValidator</b> class.
        /// </summary>
        public ISBNValidator()
        {}

        /// <summary>
        /// Creates a new instance of the <b>ISBNValidator</b> class.
        /// </summary>
        /// <param name="test">The expression to validate.</param>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public ISBNValidator(string test, string when)
            : base(test, when)
        {
            AssertUtils.ArgumentHasText(test, "test");
        }

        /// <summary>
        /// Creates a new instance of the <b>ISBNValidator</b> class.
        /// </summary>
        /// <param name="test">The expression to validate.</param>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public ISBNValidator(IExpression test, IExpression when)
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
        /// In the case of the <see cref="ISBNValidator"/> class,
        /// the test should be a string variable that will be evaluated and the object
        /// obtained as a result of this evaluation will be tested using the ISBN-10 or
        /// ISBN-13 validation rules.
        /// </remarks>
        /// <param name="objectToValidate">The object to validate.</param>
        /// <returns>
        /// <see lang="true"/> if the supplied <paramref name="objectToValidate"/> is valid ISBN.
        /// </returns>
        protected override bool Validate(object objectToValidate)
        {
            String isbn = objectToValidate as String;
            if (StringUtils.IsNullOrEmpty(isbn))
            {
                return true;
            }

            return IsValid(isbn);
        }

        #endregion

        #region ISBNValidator methods

        /// <summary>
        /// Validates <paramref name="isbn"/> against ISBN-10 or ISBN-13 validation
        /// rules.
        /// </summary>
        /// <param name="isbn">
        /// ISBN string to validate.
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="isbn"/> is a valid ISBN-10 or ISBN-13 code.
        /// </returns>
        private bool IsValid(String isbn)
        {
            String code = (isbn == null ? null : isbn.Trim().Replace("-", "").Replace(" ", ""));

            // check the length
            if ((code == null) || (code.Length < 10 || code.Length>13))
            {
                return false;
            }

            // validate/reformat using regular expression
            Match match;
            String pattern;
            if (code.Length == 10)
            {
                pattern = ISBN10_PATTERN;
            }
            else
            {
                pattern = ISBN13_PATTERN;
            }

            match = Regex.Match(code, pattern);
            return match.Success && match.Index == 0 && match.Length == code.Length;
        }

        #endregion

        #region Data members

        private static readonly String SEP = "(?:\\-|\\s)";
        private static readonly String GROUP = "(\\d{1,5})";
        private static readonly String PUBLISHER = "(\\d{1,7})";
        private static readonly String TITLE = "(\\d{1,6})";

        /// <summary>
        /// ISBN-10 consists of 4 groups of numbers separated by either
        /// dashes (-) or spaces.
        /// </summary>
        /// <remarks>
        /// The first group is 1-5 characters, second 1-7, third 1-6,
        /// and fourth is 1 digit or an X.
        /// </remarks>
        static readonly String ISBN10_PATTERN =  "^(?:(\\d{9}[0-9X])|(?:" + GROUP + SEP + PUBLISHER + SEP + TITLE + SEP + "([0-9X])))$";

        /// <summary>
        /// ISBN-13 consists of 5 groups of numbers separated by either
        /// dashes (-) or spaces.
        /// </summary>
        /// <remarks>
        /// The first group is 978 or 979, the second group is
        /// 1-5 characters, third 1-7, fourth 1-6, and fifth is 1 digit.
        /// </remarks>
        static readonly String ISBN13_PATTERN = "^(978|979)(?:(\\d{10})|(?:" + SEP + GROUP + SEP + PUBLISHER + SEP + TITLE + SEP + "([0-9])))$";

        #endregion
    }
}
