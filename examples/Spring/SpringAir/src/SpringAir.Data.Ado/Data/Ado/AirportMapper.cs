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
using System.Data;
using Spring.Data;
using SpringAir.Domain;

#endregion

namespace SpringAir.Data.Ado
{
	/// <summary>
	/// Maps a single row from an <see cref="System.Data.SqlClient.SqlDataReader"/>
	/// to an <see cref="SpringAir.Domain.Airport"/> instance.
	/// </summary>
	/// <author>Rick Evans</author>
	/// <version>$Id: AirportMapper.cs,v 1.1 2006/11/28 06:42:26 markpollack Exp $</version>
	public class AirportMapper : IRowMapper
	{
		/// <summary>
		/// Maps values stripped from the supplied <paramref name="results"/>
		/// object into an <see cref="SpringAir.Domain.Airport"/> instance.
		/// </summary>
		/// <param name="results">
		/// The <see cref="System.Data.IDataReader"/> containing the result values.
		/// </param>
		/// <param name="rowNumber">
		/// The number of the current row.
		/// </param>
		/// <returns>
		/// An <see cref="SpringAir.Domain.Airport"/> instance containing the results
		/// of the mapping.
		/// </returns>
		public object MapRow(IDataReader results, int rowNumber)
		{
			long id = results.GetInt32(0);
			string code = results.GetString(1);
			string city = results.GetString(2);
			string description = results.GetString(3);
			return new Airport(id, code, city, description);
		}
	}
}