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
using System.Collections;
using NUnit.Framework;

#endregion

namespace SpringAir.Domain
{
    /// <summary>
    /// Unit tests for the ReservationConfirmation class.
    /// </summary>
    /// <author>Rick Evans</author>
    /// <version>$Id: ReservationConfirmationTests.cs,v 1.7 2005/12/13 00:48:00 bbaia Exp $</version>
    [TestFixture]
    public sealed class ReservationConfirmationTests
    {
        private const string reservationConfirmationNumber = "JX726617H";

        [Test]
        public void EqualsSunnyDay()
        {
            ReservationConfirmation lhs = new ReservationConfirmation(
                reservationConfirmationNumber, new Reservation(null, new Itinerary(1, 1000.0m, null)));
            ReservationConfirmation rhs = new ReservationConfirmation(
                reservationConfirmationNumber, new Reservation(null, new Itinerary(1, 1000.0m, null)));
            Assert.IsTrue(lhs.Equals(rhs));
        }

        [Test]
        public void EqualsNull()
        {
            ReservationConfirmation lhs = new ReservationConfirmation(
                reservationConfirmationNumber, new Reservation(null, new Itinerary(1, 1000.0m, null)));
            Assert.IsFalse(lhs.Equals(null));
        }

        [Test]
        public void EqualsAnotherTypeOfObject()
        {
            ReservationConfirmation lhs = new ReservationConfirmation(
                reservationConfirmationNumber, new Reservation(null, new Itinerary(1, 1000.0m, null)));
            Assert.IsFalse(lhs.Equals(12));
        }
    }
}