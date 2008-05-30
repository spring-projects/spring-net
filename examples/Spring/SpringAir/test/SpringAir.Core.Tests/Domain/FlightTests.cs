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
    /// Unit tests for the Flight class.
    /// </summary>
    /// <author>Bruno Baia</author>
    /// <version>$Id: FlightTests.cs,v 1.1 2006/01/08 00:39:07 bbaia Exp $</version>
    [TestFixture]
    public sealed class FlightTests
    {
		[Test]
		public void XmlSerialisationTest()
		{
			Flight flightBefore = new Flight(1, "UA 0123", new Airport(), new Airport(), new Aircraft(), DateTime.MinValue, new Cabin[1] {new Cabin(CabinClass.Business, 2, 8, "A")});

			XmlSerializer s = new XmlSerializer(typeof(Flight));
			StringWriter sw = new StringWriter();
			s.Serialize(sw, flightBefore);
			StringReader sr = new StringReader (sw.ToString());
			Flight flightAfter = (Flight) s.Deserialize (sr);

			Assert.AreEqual(flightAfter.Id, 1);
			Assert.AreEqual(flightAfter.FlightNumber, "UA 0123");
			Assert.AreEqual(flightAfter.DepartureTime, DateTime.MinValue);
			Assert.AreEqual(flightBefore.SeatPlan, flightAfter.SeatPlan);
		}
    }
}