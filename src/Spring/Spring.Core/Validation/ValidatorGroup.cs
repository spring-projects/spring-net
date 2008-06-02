#region License

/*
 * Copyright 2002-2004 the original author or authors.
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

using System;
using System.Collections;

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
    public class ValidatorGroup : BaseValidator
    {
        #region Fields

        private IList validators = new ArrayList();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorGroup"/> class.
        /// </summary>
        public ValidatorGroup()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorGroup"/> class.
        /// </summary>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public ValidatorGroup(string when) : this((when != null ? Expression.Parse(when) : null))
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorGroup"/> class.
        /// </summary>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public ValidatorGroup(IExpression when) : base(null, when)
        {}

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the validators.
        /// </summary>
        /// <value>The validators.</value>
        public IList Validators
        {
            get { return validators; }
            set { validators = value; }
        }

        #endregion

        /// <summary>
        /// Validates the specified object.
        /// </summary>
        /// <param name="validationContext">The object to validate.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors"><see cref="ValidationErrors"/> instance to add error messages to.</param>
        /// <returns><c>True</c> if validation was successful, <c>False</c> otherwise.</returns>
        public override bool Validate(object validationContext, IDictionary contextParams, IValidationErrors errors)
        {
            bool valid = true;

            if (EvaluateWhen(validationContext, contextParams))
            {
                foreach (IValidator validator in validators)
                {
                    valid = validator.Validate(validationContext, contextParams, errors) && valid;
                }
                ProcessActions(valid, validationContext, contextParams, errors);
            }

            return valid;
        }

        /// <summary>
        /// Doesn't do anything for validator group as there is no single test.
        /// </summary>
        /// <param name="objectToValidate">Object to validate.</param>
        /// <returns><c>True</c> if specified object is valid, <c>False</c> otherwise.</returns>
        protected override bool Validate(object objectToValidate)
        {
            throw new NotSupportedException("Validator group does not support this method.");
        }
    }
}