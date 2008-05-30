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
using System.Text;

#endregion

namespace SpringAir.Domain
{
	/// <summary>
	/// ...
	/// </summary>
	/// <author>Keith Donald</author>
	/// <author>Rick Evans (.NET)</author>
	/// <version>$Id: TripPoint.cs,v 1.3 2005/10/08 07:21:15 aseovic Exp $</version>
	[Serializable]
	public class TripPoint
	{
		private string airportCode;
		private DateTime date;
		private TimeRange timeRange = new TimeRange(0, 24);

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="SpringAir.Domain.TripPoint"/> class.
		/// </summary>
		public TripPoint()
		{
		}

		public TripPoint(string airportCode, DateTime date)
		{
			this.airportCode = airportCode;
			this.date = date;
		}

		public TripPoint(string airportCode, DateTime date, TimeRange timeRange)
		{
			this.airportCode = airportCode;
			this.date = date;
			this.timeRange = timeRange;
		}

		public string AirportCode
		{
			get { return this.airportCode; }
			set { this.airportCode = value; }
		}

		public DateTime Date
		{
			get { return this.date; }
			set { this.date = value; }
		}

		public TimeRange TimeRange
		{
			get { return this.timeRange; }
			set { this.timeRange = value; }
		}

        /// <summary>
        /// Returns a <see cref="System.String"/> representation of this
        /// <see cref="SpringAir.Domain.TripPoint"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> representation of this
        /// <see cref="SpringAir.Domain.TripPoint"/>.
        /// </returns>
	    public override string ToString()
	    {
            StringBuilder buffer = new StringBuilder();
            buffer
                .Append("[")
                .Append(AirportCode).Append(" : ")
                .Append(Date).Append(" (")
                .Append(TimeRange).Append(")]");
            return buffer.ToString();
	    }
	}
}