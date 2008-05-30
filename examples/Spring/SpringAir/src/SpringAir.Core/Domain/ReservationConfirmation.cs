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

#endregion

namespace SpringAir.Domain
{
	/// <summary>
	/// ...
	/// </summary>
	/// <author>Keith Donald</author>
	/// <author>Rick Evans (.NET)</author>
	/// <version>$Id: ReservationConfirmation.cs,v 1.4 2005/12/13 00:42:50 bbaia Exp $</version>
	[Serializable]
	public class ReservationConfirmation
	{
		#region Fields

		private string confirmationNumber;
		private Reservation reservation;

		#endregion

		#region Constructor (s) / Destructor

	    public ReservationConfirmation()
	    {}

	    public ReservationConfirmation(
			string confirmationNumber, Reservation reservation)
		{
			this.confirmationNumber = confirmationNumber;
			this.reservation = reservation;
		}

		#endregion

		#region Properties

		public string ConfirmationNumber
		{
			get { return this.confirmationNumber; }
			set { this.confirmationNumber = value; }
		}

		public Reservation Reservation
		{
			get { return this.reservation; }
			set { this.reservation = value; }
		}

		#endregion

		#region Methods

		public override bool Equals(object obj)
		{
			ReservationConfirmation rhs = obj as ReservationConfirmation;
			return rhs != null &&
				this.confirmationNumber.Equals(rhs.confirmationNumber);
		}

		public override int GetHashCode()
		{
			return this.confirmationNumber.GetHashCode();
		}

		#endregion
	}
}