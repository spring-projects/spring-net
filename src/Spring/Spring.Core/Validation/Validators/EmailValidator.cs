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
    /// Perform email validations.
    /// </summary>
    /// <remarks>
    /// <p/>
    /// This implementation is not guaranteed to catch all possible errors in an
    /// email address. For example, an address like nobody@noplace.nowhere will
    /// pass validator, even though there is no TLD "nowhere".
    /// </remarks>
    /// <author>Goran Milosavljevic</author>
    public class EmailValidator : BaseSimpleValidator
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of the <b>EmailValidator</b> class.
        /// </summary>
        public EmailValidator()
        {}

        /// <summary>
        /// Creates a new instance of the <b>EmailValidator</b> class.
        /// </summary>
        /// <param name="test">The expression to validate.</param>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public EmailValidator(string test, string when)
            : base(test, when)
        {
            AssertUtils.ArgumentHasText(test, "test");
        }

        /// <summary>
        /// Creates a new instance of the <b>EmailValidator</b> class.
        /// </summary>
        /// <param name="test">The expression to validate.</param>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public EmailValidator(IExpression test, IExpression when)
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
        /// In the case of the <see cref="EmailValidator"/> class,
        /// the test should be a string variable that will be evaluated and the object
        /// obtained as a result of this evaluation will be checked if it is
        /// a valid e-mail address.
        /// </remarks>
        /// <param name="objectToValidate">The object to validate.</param>
        /// <returns>
        /// <see lang="true"/> if the supplied <paramref name="objectToValidate"/> is valid
        /// e-mail address.
        /// </returns>
        protected override bool Validate(object objectToValidate)
        {
            string text = objectToValidate as string;
            if (StringUtils.IsNullOrEmpty(text))
            {
                return true;
            }

            Match match = Regex.Match(text, emailCheck);
            return match.Success && match.Index == 0 && match.Length == text.Length;
        }

        #endregion

        #region Data members

        /// <summary>
        /// Regular expression used for validation of object passed to this <see cref="EmailValidator"/>.
        /// </summary>
        private static string emailCheck = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

        #endregion
    }
}
