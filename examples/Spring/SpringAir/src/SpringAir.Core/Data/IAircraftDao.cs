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
using SpringAir.Domain;

#endregion

namespace SpringAir.Data
{
	/// <summary>
	/// DAO interface for the <see cref="SpringAir.Domain.Aircraft"/>
	/// domain class.
	/// </summary>
	/// <author>Rick Evans</author>
	/// <version>$Id: IAircraftDao.cs,v 1.3 2005/12/21 14:36:12 springboy Exp $</version>
	public interface IAircraftDao
	{
		/// <summary>
		/// Gets all of the <see cref="SpringAir.Domain.Aircraft"/> instances
		/// that exist in the system.
		/// </summary>
		/// <returns>
		/// All of the <see cref="SpringAir.Domain.Aircraft"/> instances
		/// that exist in the system; if no
		/// <see cref="SpringAir.Domain.Aircraft"/> can be found an empty
		/// <see cref="System.Collections.IList"/> will be returned (but never
		/// <cref lang="null"/>).
		/// </returns>
		IList GetAllAircraft();

		/// <summary>
		/// Gets the <see cref="SpringAir.Domain.Aircraft"/> uniquely
		/// identified by the supplied <paramref name="id"/>.
		/// </summary>
		/// <param name="id">
		/// The id that uniquely identifies an <see cref="SpringAir.Domain.Aircraft"/>.
		/// </param>
		/// <returns>
		/// The <see cref="SpringAir.Domain.Aircraft"/> uniquely
		/// identified by the supplied <paramref name="id"/>; or <cref lang="null"/>
		/// if no such <see cref="SpringAir.Domain.Aircraft"/> can be found.
		/// </returns>
		Aircraft GetAircraft(long id);
	}
}