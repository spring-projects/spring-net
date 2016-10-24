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

using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

#endregion

namespace Spring.Core.TypeConversion
{
    /// <summary>
    /// Unit tests for the RegexConverter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public sealed class RegexConverterTests
    {
        [Test]
        public void ConvertFromNullReference()
        {
            RegexConverter rc = new RegexConverter();
            Assert.Throws<NotSupportedException>(() => rc.ConvertFrom(null));
        }

        [Test]
        public void ConvertFromNonSupportedOptionBails()
        {
            RegexConverter rc = new RegexConverter();
            Assert.Throws<NotSupportedException>(() => rc.ConvertFrom(12));
        }

        [Test]
        public void ConvertFrom()
        {
            RegexConverter rc = new RegexConverter();
            object regex = rc.ConvertFrom("[a-z]");
            Assert.IsNotNull(regex);
            Assert.IsTrue(regex is Regex);
            Assert.IsFalse(((Regex)regex).IsMatch("2"));
        }
    }
}