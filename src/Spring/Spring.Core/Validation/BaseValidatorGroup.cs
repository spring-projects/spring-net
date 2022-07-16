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

using System.Collections;
using Spring.Expressions;

namespace Spring.Validation
{
    /// <summary>
    /// Base class for composite validators
    /// </summary>
    public abstract class BaseValidatorGroup : BaseValidator
    {
        //                TODO (EE): extend validation schema for "FastValidate"

        private IList validators = new ArrayList();
        private bool fastValidate = false;

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public BaseValidatorGroup()
        {}

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public BaseValidatorGroup(string when)
            : base(when)
        {}

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public BaseValidatorGroup(IExpression when)
            : base(when)
        {}

        /// <summary>
        /// Gets or sets the child validators.
        /// </summary>
        /// <value>The validators.</value>
        public IList Validators
        {
            get { return validators; }
            set { validators = value; }
        }

        /// <summary>
        /// When set <c>true</c>, shortcircuits evaluation.
        /// </summary>
        /// <remarks>
        /// Setting this property true causes the evaluation process to prematurely abort
        /// if the end result is known. Any remaining child validators will not be considered then.
        /// Setting this value false causes implementations to evaluate all child validators, regardless
        /// of the potentially already known result.
        /// </remarks>
        public bool FastValidate
        {
            get { return fastValidate; }
            set { fastValidate = value; }
        }

        /// <summary>
        /// Validates the specified object.
        /// </summary>
        /// <param name="validationContext">The object to validate.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors"><see cref="ValidationErrors"/> instance to add error messages to.</param>
        /// <returns><c>True</c> if validation was successful, <c>False</c> otherwise.</returns>
        public override bool Validate(object validationContext, IDictionary<string, object> contextParams, IValidationErrors errors)
        {
            if (EvaluateWhen(validationContext, contextParams))
            {
                bool valid = ValidateGroup(contextParams, errors, validationContext);
                ProcessActions(valid, validationContext, contextParams, errors);
                return valid;
            }

            return true;
        }

        /// <summary>
        /// Actual implementation how to validate the specified object.
        /// </summary>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors"><see cref="ValidationErrors"/> instance to add error messages to.</param>
        /// <param name="validationContext">The object to validate.</param>
        /// <returns><c>True</c> if validation was successful, <c>False</c> otherwise.</returns>
        protected abstract bool ValidateGroup(IDictionary<string, object> contextParams, IValidationErrors errors, object validationContext);
    }
}
