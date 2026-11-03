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

using System.Collections;
using NUnit.Framework;
using Spring.Expressions;

namespace Spring.Validation;

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

        Assert.That(valid, Is.False, "Validation should fail when all inner validators return false.");
        Assert.That(errors.GetErrors("errors").Count, Is.EqualTo(3));
        Assert.That(errors.GetErrors("existingErrors").Count, Is.EqualTo(1));
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

        Assert.That(valid, Is.False, "Validation should fail when all inner validators return false.");
        Assert.That(errors.GetErrors("errors").Count, Is.EqualTo(1));
        Assert.That(errors.GetErrors("existingErrors").Count, Is.EqualTo(1));
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

        Assert.That(valid, Is.True, "Validation should succeed when all inner validators return true.");
        Assert.That(errors.GetErrors("errors").Count, Is.EqualTo(0));
        Assert.That(errors.GetErrors("existingErrors").Count, Is.EqualTo(1));
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

        Assert.That(valid, Is.False, "Validation should fail when single inner validator returns true.");
        Assert.That(errors.GetErrors("errors").Count, Is.EqualTo(2));
        Assert.That(errors.GetErrors("existingErrors").Count, Is.EqualTo(1));
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

        Assert.That(valid, Is.True, "Validation should succeed when group validator is not evaluated.");
        Assert.That(errors.GetErrors("errors").Count, Is.EqualTo(0));
        Assert.That(errors.GetErrors("existingErrors").Count, Is.EqualTo(1));
    }
}
