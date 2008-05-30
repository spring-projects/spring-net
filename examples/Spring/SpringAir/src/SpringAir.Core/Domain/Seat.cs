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
using SpringAir.Domain;

#endregion

namespace SpringAir.Domain
{
    /// <summary>
    /// A seat 
    /// </summary>
    /// <author>Rick Evans</author>
    /// <version>$Id: Seat.cs,v 1.3 2005/10/08 07:21:15 aseovic Exp $</version>
    [Serializable]
    public sealed class Seat
    {
        private string seatNumber;
        private bool isReserved;
        private CabinClass cabinClass;

        public Seat(string seatNumber, CabinClass cabinClass, bool isReserved)
        {
            #region Sanity Check

            if (seatNumber == null || seatNumber.Trim().Length == 0)
            {
                throw new ArgumentNullException("seatNumber", "The 'seatNumber' argument is required.");
            }

            #endregion

            this.seatNumber = seatNumber;
            this.cabinClass = cabinClass;
            this.isReserved = isReserved;
        }

        public string Number
        {
            get { return seatNumber; }
        }

        public bool IsReserved
        {
            get { return isReserved; }
            set { isReserved = value; }
        }

        public CabinClass CabinClass
        {
            get { return cabinClass; }
        }
    }
}