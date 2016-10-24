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

using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

using Spring.Expressions;

#endregion

namespace Spring.Validation.Validators
{
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
			Assert.IsTrue(validator.Validate(string.Empty, new ValidationErrors()));
		}

		[Test]
		public void WhitespaceStringDoesntEvaluateToTrueByDefault()
		{
			RegularExpressionValidator validator = new RegularExpressionValidator();
			Assert.IsFalse(validator.Validate("   ", new ValidationErrors()));
		}

		[Test]
		public void WhitespaceStringOnlyValidatesToTrueWhenGivenMatchingRegex()
		{
			RegularExpressionValidator validator = new RegularExpressionValidator();
			validator.Expression = @"\s*";
			Assert.IsTrue(validator.Validate("   ", new ValidationErrors()));
		}

        [Test]
        public void CaseSensitiveStringMatching()
        {
            RegularExpressionValidator validator = new RegularExpressionValidator("ToString()", "true", @"[A-Z][a-z]*");
            Assert.IsTrue(validator.Validate("Aleksandar", new ValidationErrors()));
            Assert.IsFalse(validator.Validate("ALEKSANDAR", new ValidationErrors()));
            Assert.IsFalse(validator.Validate("aleksandar", new ValidationErrors()));
        }

        [Test]
        public void CaseInsensitiveStringMatching()
        {
        	RegularExpressionValidator validator = new RegularExpressionValidator("ToString()", "true", @"[A-Z][a-z]*");
            validator.Options = RegexOptions.IgnoreCase;
            Assert.IsTrue(validator.Validate("Aleksandar", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("ALEKSANDAR", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("aleksandar", new ValidationErrors()));
        }

		[Test]
		public void SunnyDayFailure_Invalid()
		{
			RegularExpressionValidator validator = new RegularExpressionValidator(Expression.Parse("'ljwdf87cwbh'"), Expression.Parse("true"), @"((\d{1,2}\.\d{1,3}\.\d{1,3}\.\d{1,3}))");
			Assert.IsFalse(validator.Validate("ljwdf87cwbh", new ValidationErrors()));
		}

		[Test]
		public void SunnyDay_Valid()
		{
			RegularExpressionValidator validator = new RegularExpressionValidator();
			validator.Expression = @"((\d{1,2}\.\d{1,3}\.\d{1,3}\.\d{1,3}))";
			Assert.IsTrue(validator.Validate("11.222.333.444", new ValidationErrors()));
		}

        [Test]
        public void WhenValidatorIsNotEvaluatedBecauseWhenExpressionReturnsFalse()
        {
        	RegularExpressionValidator validator = new RegularExpressionValidator("'ljwdf87cwbh'", "false", @"((\d{1,2}\.\d{1,3}\.\d{1,3}\.\d{1,3}))");
            bool valid = validator.Validate(null, new ValidationErrors());
            Assert.IsTrue(valid, "Validation should succeed when regex validator is not evaluated.");
        }

        [Test]
        public void AllowPartialMatching()
        {
            RegularExpressionValidator validator = new RegularExpressionValidator();
            validator.Expression = "[A-Za-z]";
            validator.AllowPartialMatching = true;
            Assert.True(validator.Validate("123a456", new ValidationErrors()));
        }
	}
}