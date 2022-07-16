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
    /// This validator will be valid when <b>one or more</b> of the validators in the <c>Validators</c>
    /// collection are valid.
    /// </p>
    /// <p>
    /// <c>ValidationErrors</c> property will return a union of all validation error messages
    /// for the contained validators, but only if this validator is not valid (meaning, when none
    /// of the contained validators are valid).
    /// </p>
    /// <p><b>Note</b>, that <see cref="BaseValidatorGroup.FastValidate"/> defaults to <c>true</c> for this validator type!</p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    /// <author>Erich Eichinger</author>
    public class AnyValidatorGroup : BaseValidatorGroup
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AnyValidatorGroup"/> class.
        /// </summary>
        public AnyValidatorGroup()
        {
            this.FastValidate = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnyValidatorGroup"/> class.
        /// </summary>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public AnyValidatorGroup(string when)
            : base(when)
        {
            this.FastValidate = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnyValidatorGroup"/> class.
        /// </summary>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public AnyValidatorGroup(IExpression when)
            : base(when)
        {
            this.FastValidate = true;
        }

        #endregion

        /// <summary>
        /// Validates the specified object.
        /// </summary>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors"><see cref="ValidationErrors"/> instance to add error messages to.</param>
        /// <param name="validationContext">The object to validate.</param>
        /// <returns><c>True</c> if validation was successful, <c>False</c> otherwise.</returns>
        protected override bool ValidateGroup(IDictionary<string, object> contextParams, IValidationErrors errors, object validationContext)
        {
            // capture errors in separate collection to only add them to the error collector in case of errors
            ValidationErrors tmpErrors = new ValidationErrors();
            bool valid = false;
            foreach (IValidator validator in Validators)
            {
                valid = validator.Validate(validationContext, contextParams, tmpErrors) || valid;
                if (valid && FastValidate)
                {
                    break;
                }
            }

            if (!valid)
            {
                errors.MergeErrors(tmpErrors);
            }
            return valid;
        }
    }
}
