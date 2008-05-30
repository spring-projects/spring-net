#region Licence

/*
 * Copyright © 2002-2005 the original author or authors.
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
using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;

#endregion

namespace SpringAir.Domain
{
    /// <summary>
    /// Unit tests for the FlightSuggestions class.
    /// </summary>
    /// <author>Bruno Baia</author>
    /// <version>$Id: FlightSuggestionsTests.cs,v 1.1 2006/01/08 00:38:38 bbaia Exp $</version>
    [TestFixture]
    public sealed class FlightSuggestionsTests
    {
		[Test]
		public void XmlSerialisationTest()
		{
			FlightSuggestions suggestionsBefore = new FlightSuggestions();
			suggestionsBefore.OutboundFlights.Add(new Flight());
			suggestionsBefore.OutboundFlights.Add(new Flight());
			suggestionsBefore.ReturnFlights.Add(new Flight());

			XmlSerializer s = new XmlSerializer(typeof(FlightSuggestions));
			StringWriter sw = new StringWriter();
			s.Serialize(sw, suggestionsBefore);
			StringReader sr = new StringReader (sw.ToString());
			FlightSuggestions suggestionsAfter = (FlightSuggestions) s.Deserialize (sr);

			Assert.AreEqual(2, suggestionsAfter.OutboundFlights.Count);
			Assert.AreEqual(1, suggestionsAfter.ReturnFlights.Count);
		}
    }
}