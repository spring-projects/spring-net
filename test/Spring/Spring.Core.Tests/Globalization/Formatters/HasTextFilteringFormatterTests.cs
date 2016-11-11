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

namespace Spring.Globalization.Formatters
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class HasTextFilteringFormatterTests
    {
        [Test]
        public void ReplacesNullAndWhitespacesByDefaultValue()
        {
            string defaultValue = "theDefaultValue";
            HasTextFilteringFormatter fmt = new HasTextFilteringFormatter(defaultValue, null);

            Assert.AreEqual( defaultValue, fmt.Parse(null));
            Assert.AreEqual(defaultValue, fmt.Parse(string.Empty));
            Assert.AreEqual( defaultValue, fmt.Parse("\t \n\r"));
            Assert.AreEqual(" text \n", fmt.Parse(" text \n"));
        }

        [Test]
        public void DoesntAffectFormat()
        {
            string defaultValue = "theDefaultValue";
            HasTextFilteringFormatter fmt = new HasTextFilteringFormatter(defaultValue, null);

            Assert.AreEqual(null, fmt.Format(null));
            Assert.AreEqual(string.Empty, fmt.Format(string.Empty));
            Assert.AreEqual("\t \n\r", fmt.Format("\t \n\r"));
            object o = new object();
            Assert.AreEqual(o.ToString(), fmt.Format(o));            
        }
    }
}