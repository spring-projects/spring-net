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

using System.Text.RegularExpressions;
using NUnit.Framework;
using Spring.Expressions;

namespace Spring.Validation.Validators;

/// <summary>
/// Unit tests for the RegularExpressionValidator class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class RegularExpressionValidatorTests
{
    [Test]
    public void WithNonString()
    {
        RegularExpressionValidator validator = new RegularExpressionValidator();
        Assert.Throws<ArgumentException>(() => validator.Validate(this, new ValidationErrors()));
    }

    [Test]
    public void WithNull()
    {
        RegularExpressionValidator validator = new RegularExpressionValidator();
        Assert.Throws<ArgumentException>(() => validator.Validate(null, new ValidationErrors()));
    }

    [Test]
    public void EmptyStringValidatesToTrue()
    {
        RegularExpressionValidator validator = new RegularExpressionValidator();
        Assert.That(validator.Validate(string.Empty, new ValidationErrors()), Is.True);
    }

    [Test]
    public void WhitespaceStringDoesntEvaluateToTrueByDefault()
    {
        RegularExpressionValidator validator = new RegularExpressionValidator();
        Assert.That(validator.Validate("   ", new ValidationErrors()), Is.False);
    }

    [Test]
    public void WhitespaceStringOnlyValidatesToTrueWhenGivenMatchingRegex()
    {
        RegularExpressionValidator validator = new RegularExpressionValidator();
        validator.Expression = @"\s*";
        Assert.That(validator.Validate("   ", new ValidationErrors()), Is.True);
    }

    [Test]
    public void CaseSensitiveStringMatching()
    {
        RegularExpressionValidator validator = new RegularExpressionValidator("ToString()", "true", @"[A-Z][a-z]*");
        Assert.That(validator.Validate("Aleksandar", new ValidationErrors()), Is.True);
        Assert.That(validator.Validate("ALEKSANDAR", new ValidationErrors()), Is.False);
        Assert.That(validator.Validate("aleksandar", new ValidationErrors()), Is.False);
    }

    [Test]
    public void CaseInsensitiveStringMatching()
    {
        RegularExpressionValidator validator = new RegularExpressionValidator("ToString()", "true", @"[A-Z][a-z]*");
        validator.Options = RegexOptions.IgnoreCase;
        Assert.That(validator.Validate("Aleksandar", new ValidationErrors()), Is.True);
        Assert.That(validator.Validate("ALEKSANDAR", new ValidationErrors()), Is.True);
        Assert.That(validator.Validate("aleksandar", new ValidationErrors()), Is.True);
    }

    [Test]
    public void SunnyDayFailure_Invalid()
    {
        RegularExpressionValidator validator = new RegularExpressionValidator(Expression.Parse("'ljwdf87cwbh'"), Expression.Parse("true"), @"((\d{1,2}\.\d{1,3}\.\d{1,3}\.\d{1,3}))");
        Assert.That(validator.Validate("ljwdf87cwbh", new ValidationErrors()), Is.False);
    }

    [Test]
    public void SunnyDay_Valid()
    {
        RegularExpressionValidator validator = new RegularExpressionValidator();
        validator.Expression = @"((\d{1,2}\.\d{1,3}\.\d{1,3}\.\d{1,3}))";
        Assert.That(validator.Validate("11.222.333.444", new ValidationErrors()), Is.True);
    }

    [Test]
    public void WhenValidatorIsNotEvaluatedBecauseWhenExpressionReturnsFalse()
    {
        RegularExpressionValidator validator = new RegularExpressionValidator("'ljwdf87cwbh'", "false", @"((\d{1,2}\.\d{1,3}\.\d{1,3}\.\d{1,3}))");
        bool valid = validator.Validate(null, new ValidationErrors());
        Assert.That(valid, Is.True, "Validation should succeed when regex validator is not evaluated.");
    }

    [Test]
    public void AllowPartialMatching()
    {
        RegularExpressionValidator validator = new RegularExpressionValidator();
        validator.Expression = "[A-Za-z]";
        validator.AllowPartialMatching = true;
        Assert.That(validator.Validate("123a456", new ValidationErrors()), Is.True);
    }
}
