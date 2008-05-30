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
    /// <version>$Id: Trip.cs,v 1.2 2005/09/27 21:14:28 springboy Exp $</version>
    [Serializable]
    public class Trip
    {
        #region Fields

        private TripMode mode = TripMode.RoundTrip;
        private TripPoint startingFrom = new TripPoint();
        private TripPoint returningFrom = new TripPoint();

        #endregion

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="SpringAir.Domain.Trip"/> class.
        /// </summary>
        public Trip()
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="SpringAir.Domain.Trip"/> class.
        /// </summary>
        /// <param name="mode">
        /// The desired trip mode (one way or return).
        /// </param>
        /// <param name="startingFrom">
        /// The airport, date and time from where and when the journey is to start.
        /// </param>
        /// <param name="returningFrom">
        /// The airport, date and time from where and when the return journey is to start.
        /// </param>
        public Trip(TripMode mode, TripPoint startingFrom, TripPoint returningFrom)
        {
            this.mode = mode;
            this.startingFrom = startingFrom;
            this.returningFrom = returningFrom;
        }

        #endregion

        #region Properties

        public TripMode Mode
        {
            get { return this.mode; }
            set { this.mode = value; }
        }

        public TripPoint StartingFrom
        {
            get { return this.startingFrom; }
            set { this.startingFrom = value; }
        }

        public TripPoint ReturningFrom
        {
            get { return this.returningFrom; }
            set { this.returningFrom = value; }
        }

        #endregion

        /// <summary>
        /// Returns a <see cref="System.String"/> representation of this
        /// <see cref="SpringAir.Domain.Trip"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> representation of this
        /// <see cref="SpringAir.Domain.Trip"/>.
        /// </returns>
        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer
                .Append(Mode).Append(", from ")
                .Append(StartingFrom).Append(" to ")
                .Append(ReturningFrom);
            return buffer.ToString();
        }
    }
}