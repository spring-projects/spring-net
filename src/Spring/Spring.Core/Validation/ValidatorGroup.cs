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

using Spring.Expressions;

namespace Spring.Validation
{
    /// <summary>
    /// <see cref="IValidator"/> implementation that supports grouping of validators.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This validator will be valid only when all of the validators in the <c>Validators</c>
    /// collection are valid.
    /// </p>
    /// <p>
    /// <c>ValidationErrors</c> property will return a union of all validation error messages
    /// for the contained validators.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    /// <author>Erich Eichinger</author>
    public class ValidatorGroup : BaseValidatorGroup
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public ValidatorGroup() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public ValidatorGroup(string when) : base(when)
        {
        }

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public ValidatorGroup(IExpression when) : base(when)
        {
        }

        #endregion

        /// <summary>
        /// Actual implementation how to validate the specified object.
        /// </summary>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors"><see cref="ValidationErrors"/> instance to add error messages to.</param>
        /// <param name="validationContext">The object to validate.</param>
        /// <returns><c>True</c> if validation was successful, <c>False</c> otherwise.</returns>
        protected override bool ValidateGroup(IDictionary<string, object> contextParams, IValidationErrors errors, object validationContext)
        {
            bool valid = true;
            foreach (IValidator validator in this.Validators)
            {
                valid = validator.Validate(validationContext, contextParams, errors) && valid;
                if (!valid && this.FastValidate)
                {
                    break;
                }
            }
            return valid;
        }
    }
}
