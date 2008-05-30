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

using System;

namespace SpringAir
{
	/// <summary>
	/// Constants for all of the various lookup keys used throughout the
	/// application (to pull objects from the session, request, etc).
	/// </summary>
	/// <remarks>
	/// <p>
	/// Values could be injected into pages that needed them using the
	/// <see cref="Spring.Objects.Factory.Config.FieldRetrievingFactoryObject"/>
	/// (if one wanted to go there).
	/// </p>
	/// </remarks>
	/// <author>Rick Evans</author>
	/// <version>$Id: Constants.cs,v 1.1 2006/11/01 01:14:04 bbaia Exp $</version>
	public sealed class Constants
	{
		/// <summary>
		/// The key used to index into the AppSettings dictionary containing
		/// the path to the Log4Net configuration file.
		/// </summary>
		public const string Log4NetConfigFile = "Log4NetConfigFile";

		/// <summary>
		/// The key under which the suggested flights for a user's trip
		/// search are stored.
		/// </summary>
		public const string SuggestedFlightsKey = "suggestedFlights";

		/// <summary>
		/// The key under which the reservation confirmation for a user's trip
		/// search are stored.
		/// </summary>
		public const string ReservationConfirmationKey = "reservationConfirmation";

		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the <see cref="SpringAir.Constants"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a utility class, and as such exposes no public constructors.
		/// </p>
		/// </remarks>
		private Constants()
		{
		}

		#endregion
	}
}