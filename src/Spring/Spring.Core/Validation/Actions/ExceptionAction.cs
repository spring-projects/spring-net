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

using Common.Logging;
using Spring.Expressions;

namespace Spring.Validation.Actions
{
    public class ExceptionAction : BaseValidationAction
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ExceptionAction));
        private IExpression throwsExpression;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionAction"/> class.
        /// </summary>
        public ExceptionAction()
        {
        }

                /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionAction"/> class.
        /// </summary>
        /// <param name="exceptionExpression">Expression that defines the exception to throw when the validator is not valid.</param>
        public ExceptionAction(string exceptionExpression)
            : this((exceptionExpression != null ? Expression.Parse(exceptionExpression) : null))
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionAction"/> class with an expression
        /// that defines the exception to throw.
        /// </summary>
        public ExceptionAction(IExpression throwsExpression)
        {
            this.throwsExpression = throwsExpression;
        }

        /// <summary>
        /// Gets or sets the exception to throw
        /// </summary>
        /// <value>The throws.</value>
        public IExpression ThrowsExpression
        {
            get { return throwsExpression; }
            set { throwsExpression = value; }
        }

        /// <summary>
        /// Called when associated validator is invalid.
        /// </summary>
        /// <param name="validationContext">Validation context.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors">Validation errors container.</param>
        protected override void OnInvalid(object validationContext, IDictionary<string, object> contextParams, IValidationErrors errors)
        {
            if (throwsExpression != null)
            {
                object o = null;
                try
                {
                    o = throwsExpression.GetValue(null, contextParams);
                }
                catch (Exception e)
                {
                    log.Error("Was not able to evaluate action expression [" + throwsExpression + "]", e);
                }
                Exception exception = o as Exception;
                if (exception != null)
                {
                    throw exception;
                }
            }
            throw new ValidationException(errors);
        }

    }
}
