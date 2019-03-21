#region License

/*
 * Copyright 2004 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region Imports

using NUnit.Framework;

using Spring.Expressions;

#endregion

namespace Spring.Validation.Validators
{
    /// <summary>
    /// Unit tests for the RequiredValidator class.
    /// </summary>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class RequiredValidatorTests
    {
        [Test]
        public void WithNull()
        {
            RequiredValidator validator = new RequiredValidator();
            validator.Test = Expression.Parse("null");
            IValidationErrors errors = new ValidationErrors();
            Assert.IsFalse(validator.Validate(null, errors));
        }

        [Test]
        public void WithZeroNumber()
        {
            RequiredValidator validator = new RequiredValidator("0", null);
            IValidationErrors errors = new ValidationErrors();
            Assert.IsFalse(validator.Validate(null, errors));
        }

        [Test]
        public void WithPositiveNumber()
        {
            RequiredValidator validator = new RequiredValidator(Expression.Parse("100"), null);
            IValidationErrors errors = new ValidationErrors();
            Assert.IsTrue(validator.Validate(null, errors));
        }

        [Test]
        public void WithNegativeNumber()
        {
            RequiredValidator validator = new RequiredValidator();
            validator.Test = Expression.Parse("-100");
            IValidationErrors errors = new ValidationErrors();
            Assert.IsTrue(validator.Validate(null, errors));
        }

        [Test]
        public void WithEmptyString()
        {
            RequiredValidator validator = new RequiredValidator("''", null);
            IValidationErrors errors = new ValidationErrors();
            Assert.IsFalse(validator.Validate(null, errors));
        }

        [Test]
        public void WithWhitespaceOnlyString()
        {
            RequiredValidator validator = new RequiredValidator(Expression.Parse("'    '"), null);
            IValidationErrors errors = new ValidationErrors();
            Assert.IsFalse(validator.Validate(null, errors));
        }

        [Test]
        public void WithKosherString()
        {
            RequiredValidator validator = new RequiredValidator();
            validator.Test = Expression.Parse("'some non-empty string'");
            IValidationErrors errors = new ValidationErrors();
            Assert.IsTrue(validator.Validate(null, errors));
        }

        [Test]
        public void WithKosherDate()
        {
            RequiredValidator validator = new RequiredValidator("DateTime.Today", null);
            IValidationErrors errors = new ValidationErrors();
            Assert.IsTrue(validator.Validate(null, errors));
        }

        [Test]
        public void WithMinDate()
        {
            RequiredValidator validator = new RequiredValidator(Expression.Parse("DateTime.MinValue"), null);
            IValidationErrors errors = new ValidationErrors();
            Assert.IsFalse(validator.Validate(null, errors));
        }

        [Test]
        public void WithMaxDate()
        {
            RequiredValidator validator = new RequiredValidator();
            validator.Test = Expression.Parse("DateTime.MaxValue");
            IValidationErrors errors = new ValidationErrors();
            Assert.IsFalse(validator.Validate(null, errors));
        }

        [Test]
        public void WithZeroFloat()
        {
            RequiredValidator validator = new RequiredValidator("0.00F", null);
            IValidationErrors errors = new ValidationErrors();
            Assert.IsFalse(validator.Validate(null, errors));
        }

        [Test]
        public void WithKosherFloat()
        {
            RequiredValidator validator = new RequiredValidator(Expression.Parse("5.25F"), null);
            IValidationErrors errors = new ValidationErrors();
            Assert.IsTrue(validator.Validate(null, errors));
        }

        [Test]
        public void WithZeroDouble()
        {
            RequiredValidator validator = new RequiredValidator("0.00D", null);
            IValidationErrors errors = new ValidationErrors();
            Assert.IsFalse(validator.Validate(null, errors));
        }

        [Test]
        public void WithKosherDouble()
        {
            RequiredValidator validator = new RequiredValidator("5.25D", null);
            IValidationErrors errors = new ValidationErrors();
            Assert.IsTrue(validator.Validate(null, errors));
        }

        [Test]
        public void WithMinChar()
        {
            RequiredValidator validator = new RequiredValidator();
            validator.Test = Expression.Parse("char.MinValue");
            IValidationErrors errors = new ValidationErrors();
            Assert.IsFalse(validator.Validate(null, errors));
        }

        [Test]
        public void WithWhitespaceChar()
        {
            RequiredValidator validator = new RequiredValidator();
            validator.Test = Expression.Parse("' '.ToCharArray()[0]");
            IValidationErrors errors = new ValidationErrors();
            Assert.IsFalse(validator.Validate(null, errors));
        }

        [Test]
        public void WithKosherChar()
        {
            RequiredValidator validator = new RequiredValidator();
            validator.Test = Expression.Parse("'xyz'.ToCharArray()[1]");
            IValidationErrors errors = new ValidationErrors();
            Assert.IsTrue(validator.Validate(null, errors));
        }

        [Test]
        public void WhenValidatorIsNotEvaluatedBecauseWhenExpressionReturnsFalse()
        {
            RequiredValidator validator = new RequiredValidator("DateTime.MinValue", "false");
            IValidationErrors errors = new ValidationErrors();

            bool valid = validator.Validate(new object(), null, errors);

            Assert.IsTrue(valid, "Validation should succeed when required validator is not evaluated.");
            Assert.AreEqual(0, errors.GetErrors("errors").Count);
        }

    }
}