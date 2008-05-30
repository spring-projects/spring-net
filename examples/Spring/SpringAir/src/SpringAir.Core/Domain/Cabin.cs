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

#endregion

namespace SpringAir.Domain
{
    /// <summary>
    /// An aircraft cabin. Models seat map for the cabin. 
    /// </summary>
    /// <author>Rick Evans</author>
    /// <version>$Id: Cabin.cs,v 1.2 2005/12/13 00:42:50 bbaia Exp $</version>
    [Serializable]
    public sealed class Cabin
    {
        private CabinClass cabinClass;
        private int startRow, endRow;
        private string seatLetters;
        private IDictionary seats = new Hashtable();
        private int availableSeats = 0;

        #region Constructors

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="SpringAir.Domain.Cabin"/> class.
		public Cabin()
		{}
		
        public Cabin(Cabin cabin)
            : this(cabin.cabinClass, cabin.startRow, cabin.endRow, cabin.seatLetters, new ArrayList())
        {}

        public Cabin(CabinClass cabinClass, int startRow, int endRow, string seatLetters)
            : this(cabinClass, startRow, endRow, seatLetters, new ArrayList())
        {}

        public Cabin(CabinClass cabinClass, int startRow, int endRow, string seatLetters, IList reservedSeats)
        {
            this.cabinClass = cabinClass;
            this.startRow = startRow;
            this.endRow = endRow;
            this.seatLetters = seatLetters;

            for (int i = startRow; i <= endRow; i++)
            {
                foreach (char c in seatLetters)
                {
                    string seatNumber = i.ToString() + c;
                    bool isReserved = reservedSeats.Contains(seatNumber);
                    seats.Add(seatNumber, new Seat(seatNumber, cabinClass, isReserved));

                    if (!isReserved)
                    {
                        availableSeats++;
                    }
                }
            }
        }

        #endregion

        #region Properties

        public CabinClass CabinClass
        {
            get { return cabinClass; }
        }

        public int SeatCount
        {
            get { return seats.Count; }
        }

        public int AvailableSeats
        {
            get { return availableSeats; }
        }

        public string SeatPlan
        {
            get { return "(" + AvailableSeats + "/" + SeatCount + ")"; }
        }

        #endregion

        public bool IsReserved(string seatNumber)
        {
            Seat seat = (Seat) seats[seatNumber];
            if (seat != null)
            {
                return seat.IsReserved;
            }
            throw new ArgumentException("Specified seat does not exist in this cabin.", "seatNumber");
        }

        public string ReserveSeat(string seatNumber)
        {
            Seat seat = (Seat) seats[seatNumber];

            if (seat == null)
            {
                throw new ArgumentException("Specified seat does not exist in this cabin.", "seatNumber");
            }
            if (seat.IsReserved)
            {
                return ReserveSeat();
            }

            seat.IsReserved = true;
            availableSeats--;
            return seat.Number;
        }

        public string ReserveSeat()
        {
            foreach (Seat seat in seats)
            {
                if (!seat.IsReserved)
                {
                    seat.IsReserved = true;
                    availableSeats--;
                    return seat.Number;
                }
            }
            return null;
        }

        public override string ToString()
        {
            return cabinClass.ToString() + " " + SeatPlan;
        }

    }
}