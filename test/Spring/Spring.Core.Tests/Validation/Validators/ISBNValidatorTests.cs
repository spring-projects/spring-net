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

using NUnit.Framework;

#endregion

namespace Spring.Validation.Validators
{
    /// <summary>
    /// Unit tests for the ISBNValidator class.
    /// </summary>
    /// <author>Goran Milosavljevic</author>
    [TestFixture]
    public sealed class ISBNValidatorTests
    {              
        [Test]
        public void Validate()
        {
            ISBNValidator validator = new ISBNValidator();
            // validate ISBN10
            Assert.IsTrue(validator.Validate("90-70002-34-5", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("1575843013", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("81-7525-766-0", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("1905158793", new ValidationErrors()));

            // validate ISBN13
            Assert.IsTrue(validator.Validate("978-1-905158-79-9", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("978-81-7525-766-5", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("978-90-70002-34-3", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("9789070002343", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("978907000234-3", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("9789070002-34-3", new ValidationErrors()));

            Assert.IsFalse(validator.Validate("9789g70002-34-3", new ValidationErrors()));
            Assert.IsFalse(validator.Validate("a789g70002343", new ValidationErrors()));
            Assert.IsFalse(validator.Validate("978907000234x", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("", new ValidationErrors()));
            Assert.IsTrue(validator.Validate(" ", new ValidationErrors()));
            Assert.IsTrue(validator.Validate(null, new ValidationErrors()));
        }
    }
}
