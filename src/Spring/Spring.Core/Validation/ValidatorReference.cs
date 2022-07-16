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
using Spring.Objects.Factory;

namespace Spring.Validation
{
    /// <summary>
    /// Represents a reference to an externally defined validator object
    /// </summary>
    /// <remarks>
    /// <p>
    /// This class allows validation groups to reference validators that
    /// are defined outside of the group itself.
    /// </p>
    /// <p>
    /// It also allows users to narrow the context for the referenced validator
    /// by specifying value for the <c>Context</c> property.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class ValidatorReference : IValidator, IObjectFactoryAware
    {
        #region Fields

        private IObjectFactory objectFactory;

        private string name;
        private IExpression context;
        private IExpression when;
        private IValidator validator;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ValidatorReference"/> class.
        /// </summary>
        public ValidatorReference()
        {}

        /// <summary>
        /// Creates a new instance of the <see cref="ValidatorReference"/> class.
        /// </summary>
        /// <param name="when">
        /// The expression that determines if this validator should be evaluated.
        /// </param>
        public ValidatorReference(string when)
            : this((when != null ? Expression.Parse(when) : null))
        {}

        /// <summary>
        /// Creates a new instance of the <see cref="ValidatorReference"/> class.
        /// </summary>
        /// <param name="when">
        /// The expression that determines if this validator should be evaluated.
        /// </param>
        public ValidatorReference(IExpression when)
        {
            this.when = when;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the referenced validator.
        /// </summary>
        /// <value>The name of the referenced validator.</value>
        public string Name
        {
            get { return name; }
            set { name = value; }
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
        public bool Validate(object validationContext, IDictionary<string, object> contextParams, IValidationErrors errors)
        {
            bool valid = true;

            if (When == null ||
                Convert.ToBoolean(When.GetValue(validationContext, contextParams)))
            {
                if (Context != null)
                {
                    validationContext = Context.GetValue(validationContext, contextParams);
                }
                if (validator == null)
                {
                    validator = (IValidator)objectFactory.GetObject(Name);
                }
                valid = validator.Validate(validationContext, contextParams, errors);
            }

            return valid;
        }

        /// <summary>
        /// Callback that supplies the owning factory to an object instance.
        /// </summary>
        /// <value>
        /// Owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// (may not be <see langword="null"/>). The object can immediately
        /// call methods on the factory.
        /// </value>
        /// <remarks>
        /// <p>
        /// Invoked after population of normal object properties but before an init
        /// callback like <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// method or a custom init-method.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of initialization errors.
        /// </exception>
        public IObjectFactory ObjectFactory
        {
            set { objectFactory = value; }
        }
    }
}
