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

using System.Collections;
using Spring.Expressions;

namespace Spring.Validation
{
    /// <summary>
    /// <see cref="IValidator"/> implementation that supports validating collections.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This validator will be valid only when all of the validators in the <c>Validators</c>
    /// collection are valid for all of the objects in the specified collection.
    /// </p>
    /// <p>
    /// You can specify if you want to validate all of the collection elements regardless of the errors by
    /// setting the <see cref="BaseValidatorGroup.FastValidate"/> property to <c>false</c>.
    /// </p>
    /// <p><b>Note</b>, that <see cref="BaseValidatorGroup.FastValidate"/> defaults to <c>true</c> for this validator type!</p>
    /// <p>
    /// If you set the <c>IncludeElementErrors</c> property to <c>true</c>,
    /// <c>ValidationErrors</c> collection will contain a union of all validation error messages
    /// for the contained validators;
    /// Otherwise it will contain only error messages that were set for this Validator.
    /// </p>
    /// </remarks>
    /// <author>Damjan Tomic</author>
    /// <author>Aleksandar Seovic</author>
    public class CollectionValidator : BaseValidatorGroup
    {
        #region Fields

        private bool includeElementErrors = false;
        private IExpression context;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value that indicates whether to validate all elements of the collection
        /// regardless of the errors.
        /// </summary>
        /// <remarks>This is just an alias for <see cref="BaseValidatorGroup.FastValidate"/> property</remarks>
        public bool ValidateAll
        {
            get { return !base.FastValidate; }
            set { base.FastValidate = !value; }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether to capture all the errors of the specific
        /// elements of the collection
        /// </summary>
        public bool IncludeElementErrors
        {
            get { return includeElementErrors; }
            set { includeElementErrors = value; }
        }


        /// <summary>
        /// Gets or sets the expression that should be used to narrow validation context.
        /// </summary>
        /// <value>The expression that should be used to narrow validation context.</value>
        public IExpression Context
        {
            get { return context; }
            set { context = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionValidator"/> class.
        /// </summary>
        public CollectionValidator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionValidator"/> class.
        /// </summary>
        /// <param name="validateAll">The bool that determines if all elements of the collection should be evaluated.
        /// regardless of the Errors
        /// </param>
        /// <param name="includeElementErrors">The bool that determines whether Validate method should collect
        /// all error messages returned by the item validators</param>
        public CollectionValidator(bool validateAll, bool includeElementErrors)
        {
            this.FastValidate = validateAll;
            this.includeElementErrors = includeElementErrors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionValidator"/> class.
        /// </summary>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        /// <param name="validateAll">The bool that determines if this all elements of the collection should be evaluated.
        /// regardless of the Errors
        /// </param>
        /// <param name="includeElementErrors">The bool that determines whether Validate method should collect
        /// all error messages returned by the item validators</param>
        public CollectionValidator(string when, bool validateAll, bool includeElementErrors)
            : this((when != null ? Expression.Parse(when) : null), validateAll,includeElementErrors)
        {
            this.FastValidate = validateAll;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionValidator"/> class.
        /// </summary>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        /// <param name="validateAll">The bool that determines if this all elements of the collection should be evaluated.
        /// regardless of the Errors
        /// </param>
        /// <param name="includeElementErrors">The bool that determines whether Validate method should collect
        /// all error messages returned by the item validators</param>

        public CollectionValidator(IExpression when, bool validateAll, bool includeElementErrors)
            : base(when)
        {
            this.FastValidate = validateAll;
            this.includeElementErrors = includeElementErrors;
        }

        #endregion

        /// <summary>
        /// Validates the specified collection of objects.
        /// If the <c>IncludeElementErrors</c> property was set to <c>true</c>,
        /// <paramref name="errors"/> collection will contain a union of all validation error messages
        /// for the contained validators;
        /// Otherwise it will contain only error messages that were set for this Validator.
        /// </summary>
        /// <param name="validationContext">The collection to validate.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors"><see cref="ValidationErrors"/> instance to add error messages to.</param>
        /// <returns><c>True</c> if validation was successful, <c>False</c> otherwise.</returns>
        public override bool Validate(object validationContext, IDictionary<string, object> contextParams, IValidationErrors errors)
        {
            if (Context != null)
            {
                validationContext = Context.GetValue(validationContext, contextParams);
            }

            if (!(validationContext is IEnumerable))
            {
                throw new ArgumentException("The type of the object for validation must be subtype of IEnumerable.");
            }

            return base.Validate(validationContext, contextParams, errors);
        }

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
            IEnumerable collectionToValidate = (validationContext is IDictionary
                ? ((IDictionary) validationContext).Values
                : (IEnumerable) validationContext);

            // decide whether to pass new validation errors collection
            //(and discard error messages returned by the item validators)
            // OR to pass validation errors collection that was passed to this method
            //(and collect all error messages returned by the item validators)
            IValidationErrors err = (includeElementErrors)? errors : new ValidationErrors();

            foreach (object objectToValidate in collectionToValidate)
            {
                foreach (IValidator validator in this.Validators)
                {
                    valid = validator.Validate(objectToValidate, contextParams, err) && valid;
                    if (!valid && this.FastValidate)
                    {
                        break;
                    }
                }

                if (!valid && this.FastValidate)
                {
                    break;
                }
            }
            return valid;
        }

    }
}
