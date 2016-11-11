#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

#region Imports

using System.Collections.Generic;
using NUnit.Framework;

using Spring.Context.Support;
using Spring.Expressions;
using Spring.Validation.Actions;

#endregion

namespace Spring.Validation.Validators
{
    /// <summary>
    /// Unit tests for the ConditionValidator class.
    /// </summary>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class ConditionValidatorTests
    {
        [Test]
        public void StraightTrue()
        {
            ConditionValidator validator = new ConditionValidator();
            validator.Test = Expression.Parse("true");
            Assert.IsTrue(validator.Validate(null, new ValidationErrors()));
        }

        [Test]
        public void StraightFalse()
        {
            ConditionValidator validator = new ConditionValidator("false", null);
            Assert.IsFalse(validator.Validate(null, new ValidationErrors()));
        }

        [Test]
        public void TrueScalarExpression()
        {
            Inventor tesla = new Inventor();
            tesla.Name = "Nikola Tesla";

            ConditionValidator validator = new ConditionValidator(Expression.Parse("Name == 'Nikola Tesla'"), null);
            Assert.IsTrue(validator.Validate(tesla, new ValidationErrors()));
        }

        [Test]
        public void FalseScalarExpression()
        {
            Inventor tesla = new Inventor();
            tesla.Name = "Soltan Gris";

            ConditionValidator validator = new ConditionValidator(Expression.Parse("Name == 'Nikola Tesla'"), null);
            validator.Actions = new ErrorMessageAction[] {new ErrorMessageAction("Wrong name", "InventorValidator") };
            IValidationErrors errors = new ValidationErrors();
            Assert.IsFalse(validator.Validate(tesla, errors));
            Assert.IsFalse(errors.IsEmpty);
            IList<string> namedErrors = errors.GetResolvedErrors("InventorValidator", new NullMessageSource());
            Assert.AreEqual(1, namedErrors.Count);
            string error = (string) namedErrors[0];
            Assert.AreEqual("Wrong name", error);
        }

        [Test]
        public void WhenValidatorIsNotEvaluatedBecauseWhenExpressionReturnsFalse()
        {
            ConditionValidator validator = new ConditionValidator();
            validator.Test = Expression.Parse("false");
            validator.When = Expression.Parse("false");
            IValidationErrors errors = new ValidationErrors();

            bool valid = validator.Validate(new object(), null, errors);

            Assert.IsTrue(valid, "Validation should succeed when condition validator is not evaluated.");
            Assert.AreEqual(0, errors.GetErrors("errors").Count);
        }

    }
}