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

using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace Spring.Util
{
    /// <summary>
    /// Unit tests for the UriTemplate class.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Bruno Baia (.NET)</author>
    [TestFixture]
    public class UriTemplateTests
    {
        [Test]
        public void GetVariableNames()
        {
            UriTemplate template = new UriTemplate("http://example.com/hotels/{hotel}/bookings/{booking}");
            string[] variableNames = template.VariableNames;
            Assert.AreEqual(new string[] { "hotel", "booking" }, variableNames, "Invalid variable names");
        }

        [Test]
        public void ExpandVarArgs()
        {
            // absolute
            UriTemplate template = new UriTemplate("http://example.com/hotels/{hotel}/bookings/{booking}");
            Uri result = template.Expand("1", "42");
            Assert.AreEqual(new Uri("http://example.com/hotels/1/bookings/42"), result, "Invalid expanded template");

            // relative
            template = new UriTemplate("/hotels/{hotel}/bookings/{booking}");
            result = template.Expand("1", "42");
            Assert.AreEqual(new Uri("/hotels/1/bookings/42", UriKind.Relative), result, "Invalid expanded template");
        }

        [Test]
        public void MultipleExpandVarArgs()
        {
            UriTemplate template = new UriTemplate("http://example.com/hotels/{hotel}/bookings/{booking}");
            Uri result = template.Expand("2", "21");
            result = template.Expand("1", "42");
            Assert.AreEqual(new Uri("http://example.com/hotels/1/bookings/42"), result, "Invalid expanded template");
        }

        [Test]
        [ExpectedException(
            typeof(ArgumentException),
            ExpectedMessage = "Invalid amount of variables values in 'http://example.com/hotels/{hotel}/bookings/{booking}': expected 2; got 3")]
        public void ExpandVarArgsInvalidAmountVariables()
        {
            UriTemplate template = new UriTemplate("http://example.com/hotels/{hotel}/bookings/{booking}");
            template.Expand("1", "42", "100");
        }

        [Test]
        public void ExpandVarArgsDuplicateVariables()
        {
            UriTemplate template = new UriTemplate("http://example.com/order/{c}/{c}/{c}");
            Assert.AreEqual(new string[] { "c" }, template.VariableNames, "Invalid variable names");
            Uri result = template.Expand("cheeseburger");
            Assert.AreEqual(new Uri("http://example.com/order/cheeseburger/cheeseburger/cheeseburger"), result, "Invalid expanded template");
        }

        [Test]
        public void ExpandDictionary()
        {
            IDictionary<string, string> uriVariables = new Dictionary<String, String>(2);
            uriVariables.Add("booking", "42");
            uriVariables.Add("hotel", "1");

            // absolute
            UriTemplate template = new UriTemplate("http://example.com/hotels/{hotel}/bookings/{booking}");
            Uri result = template.Expand(uriVariables);
            Assert.AreEqual(new Uri("http://example.com/hotels/1/bookings/42"), result, "Invalid expanded template");

            // relative
            template = new UriTemplate("hotels/{hotel}/bookings/{booking}");
            result = template.Expand(uriVariables);
            Assert.AreEqual(new Uri("hotels/1/bookings/42", UriKind.Relative), result, "Invalid expanded template");
        }

        [Test]
        public void MultipleExpandDictionary()
        {
            UriTemplate template = new UriTemplate("http://example.com/hotels/{hotel}/bookings/{booking}");

            IDictionary<string, string> uriVariables = new Dictionary<String, String>(2);
            uriVariables.Add("booking", "21");
            uriVariables.Add("hotel", "2");
            Uri result = template.Expand(uriVariables);

            uriVariables = new Dictionary<String, String>(2);
            uriVariables.Add("booking", "42");
            uriVariables.Add("hotel", "1");
            result = template.Expand(uriVariables);

            Assert.AreEqual(new Uri("http://example.com/hotels/1/bookings/42"), result, "Invalid expanded template");
        }

        [Test]
        [ExpectedException(
            typeof(ArgumentException),
            ExpectedMessage = "Invalid amount of variables values in 'http://example.com/hotels/{hotel}/bookings/{booking}': expected 2; got 1")]
        public void ExpandDictionaryInvalidAmountVariables() 
        {
            IDictionary<string, string> uriVariables = new Dictionary<String, String>(2);
            uriVariables.Add("hotel", "1");

            UriTemplate template = new UriTemplate("http://example.com/hotels/{hotel}/bookings/{booking}");
            template.Expand(uriVariables);
        }

        [Test]
        [ExpectedException(
            typeof(ArgumentException),
            ExpectedMessage = "'uriVariables' dictionary has no value for 'hotel'")]
        public void ExpandDictionaryUnboundVariables() 
        {
            IDictionary<string, string> uriVariables = new Dictionary<String, String>(2);
            uriVariables.Add("booking", "42");
            uriVariables.Add("bar", "1");

            UriTemplate template = new UriTemplate("http://example.com/hotels/{hotel}/bookings/{booking}");
            template.Expand(uriVariables);
        }

        [Test]
        public void ExpandEncoded()
        {
            UriTemplate template = new UriTemplate("http://example.com/hotel list/{hotel}");
            Uri result = template.Expand("Z\u00fcrich");
            Assert.AreEqual(new Uri("http://example.com/hotel%20list/Z%C3%BCrich"), result, "Invalid expanded template");
        }

        [Test]
        public void Matches()
        {
            UriTemplate template = new UriTemplate("http://example.com/hotels/{hotel}/bookings/{booking}/");
            Assert.IsTrue(template.Matches("http://example.com/hotels/1/bookings/42/"), "UriTemplate does not match");
            Assert.IsFalse(template.Matches("hhhhttp://example.com/hotels/1/bookings/42/"), "UriTemplate matches");
            Assert.IsFalse(template.Matches("http://example.com/hotels/1/bookings/42/blabla"), "UriTemplate matches");
            Assert.IsFalse(template.Matches("http://example.com/hotels/bookings/"), "UriTemplate matches");
            Assert.IsFalse(template.Matches(""), "UriTemplate matches");
            Assert.IsFalse(template.Matches(null), "UriTemplate matches");
        }

        [Test]
        public void Match()
        {
            UriTemplate template = new UriTemplate("http://example.com/hotels/{hotel}/bookings/{booking}");
            IDictionary<string, string> result = template.Match("http://example.com/hotels/1/bookings/42");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("1", result["hotel"]);
            Assert.AreEqual("42", result["booking"]);

            result = template.Match("http://example.com/hotels/1/bookings");
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void matchDuplicate()
        {
            UriTemplate template = new UriTemplate("/order/{c}/{c}/{c}");
            IDictionary<string, string> result = template.Match("/order/cheeseburger/cheeseburger/cheeseburger");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("cheeseburger", result["c"]);
        }

        [Test]
        public void MatchMultipleInOneSegment()
        {
            UriTemplate template = new UriTemplate("/{foo}-{bar}");
            IDictionary<string, string> result = template.Match("/12-34");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("12", result["foo"]);
            Assert.AreEqual("34", result["bar"]);
        }

        [Test]
        public void MatchesQueryVariables()
        {
            UriTemplate template = new UriTemplate("/search?q={query}");
            Assert.IsTrue(template.Matches("/search?q=foo"));
        }

        [Test]
        public void MatchesFragments()
        {
            UriTemplate template = new UriTemplate("/search#{fragment}");
            Assert.IsTrue(template.Matches("/search#foo"));

            template = new UriTemplate("/search?query={query}#{fragment}");
            Assert.IsTrue(template.Matches("/search?query=foo#bar"));
        }

        [Test]
        public void ExpandWithAtSign()
        {
            UriTemplate template = new UriTemplate("http://localhost/query={query}");
            Uri uri = template.Expand("foo@bar");
            Assert.AreEqual("http://localhost/query=foo@bar", uri.ToString());
        }
    }
}
