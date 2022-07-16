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
    /// Abstract base class that should be extended by all
    /// validation actions.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This class implements template <c>Execute</c> method
    /// and defines <c>OnValid</c> and <c>OnInvalid</c> methods that
    /// can be overriden
    /// by specific validation actions.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public abstract class BaseValidationAction : IValidationAction
    {
        #region Fields

        private IExpression when;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseValidationAction"/> class.
        /// </summary>
        public BaseValidationAction()
        {}

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

        #endregion

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="isValid">Whether associated validator is valid or not.</param>
        /// <param name="validationContext">Validation context.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors">Validation errors container.</param>
        public virtual void Execute(bool isValid, object validationContext, IDictionary<string, object> contextParams, IValidationErrors errors)
        {
            if (EvaluateWhen(validationContext, contextParams))
            {
                if (isValid)
                {
                    OnValid(validationContext, contextParams, errors);
                }
                else
                {
                    OnInvalid(validationContext, contextParams, errors);
                }
            }
        }

        #region Abstract methods

        // CLOVER:OFF

        /// <summary>
        /// Called when associated validator is valid.
        /// </summary>
        /// <param name="validationContext">Validation context.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors">Validation errors container.</param>
        protected virtual void OnValid(object validationContext, IDictionary<string, object> contextParams, IValidationErrors errors)
        {}

        /// <summary>
        /// Called when associated validator is not valid.
        /// </summary>
        /// <param name="validationContext">Validation context.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors">Validation errors container.</param>
        protected virtual void OnInvalid(object validationContext, IDictionary<string, object> contextParams, IValidationErrors errors)
        {}

        // CLOVER:ON

        #endregion

        #region Helper methods

        /// <summary>
        /// Evaluates 'when' expression.
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

        #endregion
    }
}
