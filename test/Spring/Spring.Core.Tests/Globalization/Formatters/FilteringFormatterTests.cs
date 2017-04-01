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

using FakeItEasy;

using NUnit.Framework;

namespace Spring.Globalization.Formatters
{
    /// <summary>
    ///
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class FilteringFormatterTests
    {
        public class TestFilteringFormatter : FilteringFormatter
        {
            public TestFilteringFormatter(IFormatter underlyingFormatter) : base(underlyingFormatter)
            {
            }

            protected override string FilterValueToParse(string value)
            {
                return this.DoFilterValueToParse(value);
            }

            public virtual string DoFilterValueToParse(string value)
            {
                return base.FilterValueToParse(value);
            }

            protected override object FilterValueToFormat(object value)
            {
                return this.DoFilterValueToFormat(value);
            }

            public virtual object DoFilterValueToFormat(object value)
            {
                return base.FilterValueToFormat(value);
            }

        }

        [Test]
        public void FiltersOnParseAndFormat()
        {
            IFormatter underlyingFormatter = A.Fake<IFormatter>();
            TestFilteringFormatter formatter = A.Fake<TestFilteringFormatter>(x => x
                .CallsBaseMethods()
                .WithArgumentsForConstructor(new[] {underlyingFormatter}));

            string inputText = "inputText";
            string filteredInputText = "filteredInputText";
            object outputValue = new object();
            object filteredOutputValue = new object();

            A.CallTo(() => formatter.DoFilterValueToParse(inputText)).Returns(filteredInputText);
            A.CallTo(() => underlyingFormatter.Parse(filteredInputText)).Returns(outputValue);

            A.CallTo(() => formatter.DoFilterValueToFormat(outputValue)).Returns(filteredOutputValue);
            A.CallTo(() => underlyingFormatter.Format(filteredOutputValue)).Returns(inputText);

            Assert.AreSame(outputValue, formatter.Parse(inputText));
            Assert.AreEqual(inputText, formatter.Format(outputValue));
        }
    }
}