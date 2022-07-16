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
    /// Base class that defines common properties for all single validators.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Custom single validators should always extend this class instead of
    /// simply implementing <see cref="IValidator"/> interface, in
    /// order to inherit common validator functionality.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    /// <author>Erich Eichinger</author>
    public abstract class BaseSimpleValidator : BaseValidator
    {
        private IExpression test;

        /// <summary>
        /// Gets or sets the test expression.
        /// </summary>
        /// <value>The test expression.</value>
        public IExpression Test
        {
            get { return test; }
            set { test = value; }
        }

        /// <summary>
        /// Creates a new instance of the validator without any <see cref="Test"/>
        /// and <see cref="BaseValidator.When"/> criteria
        /// </summary>
        public BaseSimpleValidator()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BaseValidator"/> class.
        /// </summary>
        /// <param name="test">The expression to validate.</param>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public BaseSimpleValidator(string test, string when)
            : base( when)
        {
            this.test = (test != null ? Expression.Parse(test) : null);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BaseValidator"/> class.
        /// </summary>
        /// <param name="test">The expression to validate.</param>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        public BaseSimpleValidator(IExpression test, IExpression when):base(when)
        {
            this.test = test;
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
            bool valid = true;

            if (EvaluateWhen(validationContext, contextParams))
            {
                valid = Validate(EvaluateTest(validationContext, contextParams));
                ProcessActions(valid, validationContext, contextParams, errors);
            }

            return valid;
        }

        /// <summary>
        /// Validates test object.
        /// </summary>
        /// <param name="objectToValidate">Object to validate.</param>
        /// <returns><c>True</c> if specified object is valid, <c>False</c> otherwise.</returns>
        protected abstract bool Validate(object objectToValidate);

        /// <summary>
        /// Evaluates test expression.
        /// </summary>
        /// <param name="rootContext">Root context to use for expression evaluation.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <returns>Result of the test expression evaluation, or validation context if test is <c>null</c>.</returns>
        protected object EvaluateTest(object rootContext, IDictionary<string, object> contextParams)
        {
            if (Test == null)
            {
                return rootContext;
            }
            return Test.GetValue(rootContext, contextParams);
        }
    }
}
