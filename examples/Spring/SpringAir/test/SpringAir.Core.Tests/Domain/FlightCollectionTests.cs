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
using NUnit.Framework;

#endregion

namespace SpringAir.Domain
{
	/// <summary>
	/// Unit tests for the FlightCollection class.
	/// </summary>
	/// <author>Rick Evans</author>
	/// <version>$Id: FlightCollectionTests.cs,v 1.3 2006/01/14 08:37:45 aseovic Exp $</version>
	[TestFixture]
	public sealed class FlightCollectionTests
	{

		[Test]
		public void TypeSafeAdd()
		{
			FlightCollection flights = new FlightCollection();
			flights.Add(new Flight());
			flights.Add(new Flight());
            Assert.AreEqual(2, flights.Count);

		}

        [Test]
        public void TypeSafeIndexer()
        {
            FlightCollection flights = new FlightCollection();
            Flight firstFlight = new Flight();
            firstFlight.FlightNumber = "1";
            flights.Add(firstFlight);
            flights.Add(new Flight());
            Flight f1 = flights[0];
            Assert.IsNotNull(f1);
            Assert.AreEqual("1", f1.FlightNumber);
        }

	}
}