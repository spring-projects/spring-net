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

namespace SpringAir.Domain
{
	/// <summary>
	/// Base class for those domain objects that are persistent.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This is a pragmatic concession to the persistence layer. This base
	/// class and its single property are not part of the 'pure' domain
	/// but it just makes persistence so much easier if each persistable class
	/// has a unique identifier (especially on a greenfield project such as
	/// this).
	/// </p>
	/// </remarks>
	/// <author>Rick Evans</author>
	/// <version>$Id: Entity.cs,v 1.3 2005/12/21 16:06:35 springboy Exp $</version>
	[Serializable]
	public abstract class Entity
	{
		/// <summary>
		/// Identifies an <see cref="SpringAir.Domain.Entity"/> that is 'new'.
		/// </summary>
		/// <remarks>
		/// <p>
		/// A 'new' <see cref="SpringAir.Domain.Entity"/> is one that has not
		/// yet been persisted.
		/// </p>
		/// </remarks>
		public const long Transient = long.MinValue;

		private long id = Transient;

		/// <summary>
		/// Creates a new instance of the <see cref="Entity"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an <see langword="abstract"/> class, and as such exposes
		/// no publicly visible constructors.
		/// </p>
		/// </remarks>
		protected Entity()
		{
		}

		/// <summary>
		/// Creates a new instance of the <see cref="Entity"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an <see langword="abstract"/> class, and as such exposes
		/// no publicly visible constructors.
		/// </p>
		/// </remarks>
		/// <param name="id">
		/// The number that uniquely identifies this instance.
		/// </param>
		protected Entity(long id)
		{
			this.id = id;
		}

		/// <summary>
		/// The number that uniquely identifies this instance.
		/// </summary>
		public long Id
		{
			get { return id; }
			set { id = value; }
		}

		/// <summary>
		/// Is this instance transient?
		/// </summary>
		/// <remarks>
		/// <p>
		/// That is, does it exist in permanent storage?
		/// </p>
		/// </remarks>
		/// <value>
		/// <see lang="true"/> if this instance is transient.
		/// </value>
		public bool IsTransient
		{
			get { return id == Transient; }
		}
	}
}