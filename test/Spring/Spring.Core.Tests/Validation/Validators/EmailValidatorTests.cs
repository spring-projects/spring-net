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

#region Imports

using NUnit.Framework;

#endregion

namespace Spring.Validation.Validators
{
    /// <summary>
    /// Unit tests for the EmailValidator class.
    /// </summary>
    /// <author>Goran Milosavljevic</author>
    [TestFixture]
    public sealed class EmailValidatorTests
    {              
        [Test]
        public void Validate()
        {
            EmailValidator validator = new EmailValidator();
            Assert.IsTrue(validator.Validate("goran@eu.s4hc.com", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("goran.milosavljevic@s4hc.com", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("g.m.m@web_ask.com", new ValidationErrors()));

            Assert.IsFalse(validator.Validate("@eu.s4hc.com", new ValidationErrors()));
            Assert.IsFalse(validator.Validate("g @s4hc.com", new ValidationErrors()));
            Assert.IsFalse(validator.Validate("g&@s", new ValidationErrors()));
            Assert.IsFalse(validator.Validate("goran@s", new ValidationErrors()));
            Assert.IsFalse(validator.Validate("goran@@", new ValidationErrors()));             
            Assert.IsFalse(validator.Validate("goran@eu s4hc.com", new ValidationErrors()));             
            Assert.IsTrue(validator.Validate(" ", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("", new ValidationErrors()));             
            Assert.IsTrue(validator.Validate(null, new ValidationErrors()));             
        }
    }
}
