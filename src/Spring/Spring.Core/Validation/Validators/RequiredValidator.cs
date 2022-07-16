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

using Spring.Expressions;
using Spring.Util;

namespace Spring.Validation
{
    /// <summary>
    /// Validates that required value is not empty.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This validator uses following rules to determine if target value is valid:
    /// <table>
    ///     <tr>
    ///         <th>Target <see cref="System.Type"/></th>
    ///         <th>Valid Value</th>
    ///     </tr>
    ///     <tr>
    ///         <td>A <see cref="System.String"/>.</td>
    ///         <td>Not <see lang="null"/> or an empty string.</td>
    ///     </tr>
    ///     <tr>
    ///         <td>A <see cref="System.DateTime"/>.</td>
    ///         <td>Not <see cref="System.DateTime.MinValue"/> and not <see cref="System.DateTime.MaxValue"/>.</td>
    ///     </tr>
    ///     <tr>
    ///         <td>One of the number types.</td>
    ///         <td>Not zero.</td>
    ///     </tr>
    ///     <tr>
    ///         <td>A <see cref="System.Char"/>.</td>
    ///         <td>Not <see cref="System.Char.MinValue"/> or whitespace.</td>
    ///     </tr>
    ///     <tr>
    ///         <td>Any reference type other than <see cref="System.String"/>.</td>
    ///         <td>Not <see lang="null"/>.</td>
    ///     </tr>
    /// </table>
    /// </p>
    /// <p>
    /// You cannot use this validator to validate any value types other than the ones
    /// specified in the table above.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class RequiredValidator : BaseSimpleValidator
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="RequiredValidator"/> class.
        /// </summary>
        public RequiredValidator()
        {}

        /// <summary>
        /// Creates a new instance of the <see cref="RequiredValidator"/> class.
        /// </summary>
        /// <param name="test">The expression to validate.</param>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public RequiredValidator(string test, string when) : base(test, when)
        {
            AssertUtils.ArgumentHasText(test, "test");
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RequiredValidator"/> class.
        /// </summary>
        /// <param name="test">The expression to validate.</param>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public RequiredValidator(IExpression test, IExpression when) : base(test, when)
        {
            AssertUtils.ArgumentNotNull(test, "test");
        }

        #endregion

        /// <summary>
        /// Validates the supplied <paramref name="objectToValidate"/>.
        /// </summary>
        /// <remarks>
        /// In the case of the <see cref="Spring.Validation.RequiredValidator"/> class,
        /// the test should be a variable expression that will be evaluated and the object
        /// obtained as a result of this evaluation will be tested using the rules described
        /// in the class overview of the <see cref="Spring.Validation.RequiredValidator"/>
        /// class.
        /// </remarks>
        /// <param name="objectToValidate">The object to validate.</param>
        /// <returns>
        /// <see lang="true"/> if the supplied <paramref name="objectToValidate"/> is valid.
        /// </returns>
        protected override bool Validate(object objectToValidate)
        {
            if (objectToValidate is String && StringUtils.IsNullOrEmpty((string) objectToValidate))
            {
                return false;
            }
            else if (objectToValidate is DateTime && (((DateTime) objectToValidate) == DateTime.MinValue || ((DateTime) objectToValidate) == DateTime.MaxValue))
            {
                return false;
            }
            else if (NumberUtils.IsInteger(objectToValidate) && NumberUtils.IsZero(objectToValidate))
            {
                return false;
            }
            else if (objectToValidate is Char && (((char) objectToValidate) == Char.MinValue || Char.IsWhiteSpace((char) objectToValidate)))
            {
                return false;
            }
            else if (NumberUtils.IsDecimal(objectToValidate) && NumberUtils.IsZero(objectToValidate))
            {
                return false;
            }
            return objectToValidate != null;
        }
    }
}
