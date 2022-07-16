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

namespace Spring.Validation.Actions
{
    /// <summary>
    /// Implementation of <see cref="IValidationAction"/> that allows you
    /// to define Spring.NET expressions that should be evaluated after
    /// validation.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class ExpressionAction : BaseValidationAction
    {
        private IExpression onValid;
        private IExpression onInvalid;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionAction"/> class.
        /// </summary>
        public ExpressionAction()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionAction"/> class.
        /// </summary>
        /// <param name="onValid">Expression to execute when validator is valid.</param>
        /// <param name="onInvalid">Expression to execute when validator is not valid.</param>
        public ExpressionAction(string onValid, string onInvalid)
            : this((onValid != null ? Expression.Parse(onValid) : null), (onInvalid != null ? Expression.Parse(onInvalid) : null))
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionAction"/> class.
        /// </summary>
        /// <param name="onValid">Expression to execute when validator is valid.</param>
        /// <param name="onInvalid">Expression to execute when validator is not valid.</param>
        public ExpressionAction(IExpression onValid, IExpression onInvalid)
        {
            this.onValid = onValid;
            this.onInvalid = onInvalid;
        }

        /// <summary>
        /// Gets or sets the expression to execute when validator is valid.
        /// </summary>
        /// <value>The expression to execute when validator is valid.</value>
        public IExpression Valid
        {
            get { return onValid; }
            set { onValid = value; }
        }

        /// <summary>
        /// Gets or sets the expression to execute when validator is not valid.
        /// </summary>
        /// <value>The expression to execute when validator is not valid.</value>
        public IExpression Invalid
        {
            get { return onInvalid; }
            set { onInvalid = value; }
        }

        /// <summary>
        /// Called when associated validator is valid.
        /// </summary>
        /// <param name="validationContext">Validation context.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors">Validation errors container.</param>
        protected override void OnValid(object validationContext, IDictionary<string, object> contextParams, IValidationErrors errors)
        {
            if (Valid != null)
            {
                Valid.GetValue(validationContext, contextParams);
            }
        }

        /// <summary>
        /// Called when associated validator is invalid.
        /// </summary>
        /// <param name="validationContext">Validation context.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors">Validation errors container.</param>
        protected override void OnInvalid(object validationContext, IDictionary<string, object> contextParams, IValidationErrors errors)
        {
            if (Invalid != null)
            {
                Invalid.GetValue(validationContext, contextParams);
            }
        }
    }
}
