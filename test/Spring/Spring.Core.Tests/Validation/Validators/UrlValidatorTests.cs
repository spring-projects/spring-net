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
    /// Unit tests for the UrlValidator class.
    /// </summary>
    /// <author>Goran Milosavljevic</author>
    [TestFixture]
    public sealed class UrlValidatorTests
    {              
        [Test]
        public void Validate()
        {
            UrlValidator validator = new UrlValidator();
            Assert.IsTrue(validator.Validate("http://puzzle.com", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("http://www.com", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("www.1.org", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("ww.1.com", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("http://www.google-com.123.com", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("https://www.google-com.com", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("http://google-com.com", new ValidationErrors()));
            // just to make sure that we are ready for the Japanese market :)
            Assert.IsTrue(validator.Validate("www.amazon.co.jp/C-によるプログラミングWindows-上-Charles-Petzold/dp/4891002921", new ValidationErrors()));

            Assert.IsFalse(validator.Validate("http://.com", new ValidationErrors()));
            Assert.IsFalse(validator.Validate("http://www.google-com.123", new ValidationErrors()));
            Assert.IsFalse(validator.Validate("http://1.1", new ValidationErrors()));
            Assert.IsFalse(validator.Validate("ht:1.1", new ValidationErrors()));
            Assert.IsFalse(validator.Validate("/:www.1.com", new ValidationErrors()));
            Assert.IsTrue(validator.Validate("", new ValidationErrors()));
            Assert.IsTrue(validator.Validate(" ", new ValidationErrors()));             
            Assert.IsTrue(validator.Validate(null, new ValidationErrors()));             
        }
    }
}
