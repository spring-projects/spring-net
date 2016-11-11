#region License

/*
 * Copyright 2004 the original author or authors.
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

using System.Collections;

using NUnit.Framework;

using Spring.Expressions;

#endregion

namespace Spring.Validation
{
    /// <summary>
    /// Unit tests for the ValidatorGroup class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [TestFixture]
    public sealed class ValidatorGroupTests : ValidatorGroup
    {
        [Test]
        public void WhenAllValidatorsReturnFalse()
        {
            ValidatorGroup vg = new ValidatorGroup();
            vg.Validators.Add(new FalseValidator());
            vg.Validators.Add(new FalseValidator());
            vg.Validators.Add(new FalseValidator());

            IValidationErrors errors = new ValidationErrors();
            errors.AddError("existingErrors", new ErrorMessage("error", null));

            bool valid = vg.Validate(new object(), errors);

            Assert.IsFalse(valid, "Validation should fail when all inner validators return false.");
            Assert.AreEqual(3, errors.GetErrors("errors").Count);
            Assert.AreEqual(1, errors.GetErrors("existingErrors").Count);
        }

        [Test]
        public void WhenAllValidatorsReturnFalseFast()
        {
            ValidatorGroup vg = new ValidatorGroup();
            vg.FastValidate = true;
            vg.Validators.Add(new FalseValidator());
            vg.Validators.Add(new FalseValidator());
            vg.Validators.Add(new FalseValidator());

            IValidationErrors errors = new ValidationErrors();
            errors.AddError("existingErrors", new ErrorMessage("error", null));

            bool valid = vg.Validate(new object(), errors);

            Assert.IsFalse(valid, "Validation should fail when all inner validators return false.");
            Assert.AreEqual(1, errors.GetErrors("errors").Count);
            Assert.AreEqual(1, errors.GetErrors("existingErrors").Count);
        }

        [Test]
        public void WhenAllValidatorsReturnTrue()
        {
            ValidatorGroup vg = new ValidatorGroup("true");
            vg.Validators.Add(new TrueValidator());
            vg.Validators.Add(new TrueValidator());
            vg.Validators.Add(new TrueValidator());

            IValidationErrors errors = new ValidationErrors();
            errors.AddError("existingErrors", new ErrorMessage("error", null));

            bool valid = vg.Validate(new object(), errors);

            Assert.IsTrue(valid, "Validation should succeed when all inner validators return true.");
            Assert.AreEqual(0, errors.GetErrors("errors").Count);
            Assert.AreEqual(1, errors.GetErrors("existingErrors").Count);
        }

        [Test]
        public void WhenSingleValidatorReturnsTrue()
        {
            ValidatorGroup vg = new ValidatorGroup(Expression.Parse("true"));
            vg.Validators.Add(new FalseValidator());
            vg.Validators.Add(new TrueValidator());
            vg.Validators.Add(new FalseValidator());

            IValidationErrors errors = new ValidationErrors();
            errors.AddError("existingErrors", new ErrorMessage("error", null));

            bool valid = vg.Validate(new object(), errors);

            Assert.IsFalse(valid, "Validation should fail when single inner validator returns true.");
            Assert.AreEqual(2, errors.GetErrors("errors").Count);
            Assert.AreEqual(1, errors.GetErrors("existingErrors").Count);
        }

        [Test]
        public void WhenGroupIsNotValidatedBecauseWhenExpressionReturnsFalse()
        {
            ValidatorGroup vg = new ValidatorGroup("false");
            IList validators = new ArrayList();
            validators.Add(new FalseValidator());
            validators.Add(new FalseValidator());
            vg.Validators = validators;

            IValidationErrors errors = new ValidationErrors();
            errors.AddError("existingErrors", new ErrorMessage("error", null));

            bool valid = vg.Validate(new object(), errors);

            Assert.IsTrue(valid, "Validation should succeed when group validator is not evaluated.");
            Assert.AreEqual(0, errors.GetErrors("errors").Count);
            Assert.AreEqual(1, errors.GetErrors("existingErrors").Count);
        }
    }
}