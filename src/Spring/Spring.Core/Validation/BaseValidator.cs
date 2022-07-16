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

namespace Spring.Validation
{
    /// <summary>
    /// Base class that defines common properties for all validators.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Custom validators should always extend this class instead of
    /// simply implementing <see cref="IValidator"/> interface, in
    /// order to inherit common validator functionality.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    /// <author>Erich Eichinger</author>
    public abstract class BaseValidator : IValidator
    {
        #region Fields

        private IList<IValidationAction> actions = new List<IValidationAction>();

        private IExpression when;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="BaseValidator"/> class.
        /// </summary>
        public BaseValidator()
        {}

        /// <summary>
        /// Creates a new instance of the <see cref="BaseValidator"/> class.
        /// </summary>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public BaseValidator(string when)
            : this((when != null ? Expression.Parse(when) : null))
        {}

        /// <summary>
        /// Creates a new instance of the <see cref="BaseValidator"/> class.
        /// </summary>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public BaseValidator(IExpression when)
        {
            this.when = when;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the expression that determines if this validator should be evaluated.
        /// </summary>
        /// <value>The expression that determines if this validator should be evaluated.</value>
        public IExpression When
        {
            get { return when; }
            set { when = value; }
        }

        /// <summary>
        /// Gets or sets the validation actions.
        /// </summary>
        /// <value>The actions that should be executed after validation.</value>
        public IList<IValidationAction> Actions
        {
            get { return actions; }
            set { actions = value; }
        }

        #endregion

        /// <summary>
        /// Validates the specified object.
        /// </summary>
        /// <param name="validationContext">The object to validate.</param>
        /// <param name="errors"><see cref="ValidationErrors"/> instance to add error messages to.</param>
        /// <returns><c>True</c> if validation was successful, <c>False</c> otherwise.</returns>
        public bool Validate(object validationContext, IValidationErrors errors)
        {
            return Validate(validationContext, null, errors);
        }

        /// <summary>
        /// Validates the specified object.
        /// </summary>
        /// <param name="validationContext">The object to validate.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors"><see cref="ValidationErrors"/> instance to add error messages to.</param>
        /// <returns><c>True</c> if validation was successful, <c>False</c> otherwise.</returns>
        public abstract bool Validate(object validationContext, IDictionary<string, object> contextParams, IValidationErrors errors);

        #region Helper Methods

        /// <summary>
        /// Evaluates when expression.
        /// </summary>
        /// <param name="rootContext">Root context to use for expression evaluation.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <returns><c>True</c> if the condition is true, <c>False</c> otherwise.</returns>
        protected bool EvaluateWhen(object rootContext, IDictionary<string, object> contextParams)
        {
            if (When == null)
            {
                return true;
            }

            return Convert.ToBoolean(When.GetValue(rootContext, contextParams));
        }

        /// <summary>
        /// Processes the error messages.
        /// </summary>
        /// <param name="isValid">Whether validator is valid or not.</param>
        /// <param name="validationContext">Validation context.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors">Validation errors container.</param>
        protected void ProcessActions(bool isValid, object validationContext, IDictionary<string, object> contextParams, IValidationErrors errors)
        {
            if (actions != null && actions.Count > 0)
            {
                foreach (IValidationAction action in actions)
                {
                    action.Execute(isValid, validationContext, contextParams, errors);
                }
            }
        }

        #endregion
    }
}
