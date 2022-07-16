#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using Spring.Util;

namespace Spring.Objects.Support
{
	/// <summary>
	/// Mutable implementation of the
	/// <see cref="Spring.Objects.Support.ISortDefinition"/> interface that
    /// supports toggling the ascending value on setting the same property again.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Jean-Pierre Pawlak</author>
	/// <author>Simon White (.NET)</author>
	[Serializable]
	public class MutableSortDefinition : ISortDefinition
	{
		private string _property = string.Empty;
		private bool _ignoreCase = true;
		private bool _ascending = true;
		private bool _toggleAscendingOnProperty = false;

		#region Properties
		private bool ToggleAscendingOnProperty
		{
			get
			{
				return _toggleAscendingOnProperty;
			}
			set
			{
				this._toggleAscendingOnProperty = value;
			}
		}
		#endregion

		#region ISortDefinition Properties
        /// <summary>
        /// The name of the property to sort by.
        /// </summary>
		public string Property
		{
			get
			{
				return _property;
			}
			set
			{
				if (!StringUtils.HasText (value))
				{
					_property = string.Empty;
				}
				else
				{
					// implicit toggling of ascending?
					if (ToggleAscendingOnProperty)
					{
						if (value.Equals(_property))
						{
							_ascending = !_ascending;
						}
					}
					_property = value;
				}
			}
		}

        /// <summary>
        /// Whether upper and lower case in string values should be ignored.
        /// </summary>
        /// <value>
        /// True if the sorting should be performed in a case-insensitive fashion.
        /// </value>
		public bool IgnoreCase
		{
			get
			{
				return _ignoreCase;
			}
		}

        /// <summary>
        /// If the sorting should be ascending or descending.
        /// </summary>
        /// <value>
        /// True if the sorting should be in the ascending order.
        /// </value>
		public bool Ascending
		{
			get
			{
				return _ascending;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Support.MutableSortDefinition"/> class.
		/// </summary>
		public MutableSortDefinition ()
		{
		}

		/// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Support.MutableSortDefinition"/> class using
        /// the specified <see cref="Spring.Objects.Support.ISortDefinition"/>.
		/// </summary>
		/// <param name="source">
		/// The <see cref="Spring.Objects.Support.ISortDefinition"/> to use
		/// as a source for initial property values.
		/// </param>
		public MutableSortDefinition (ISortDefinition source)
		{
			this._property = source.Property;
			this._ignoreCase = source.IgnoreCase;
			this._ascending = source.Ascending;
		}

		/// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Support.MutableSortDefinition"/> class.
		/// </summary>
		/// <param name="name">
		/// The name of the property to sort by.
		/// </param>
		/// <param name="ignoreCase">
		/// Whether upper and lower case in string values should be ignored.
		/// </param>
		/// <param name="ascending">
		/// Whether or not the sorting should be ascending or descending.
		/// </param>
		public MutableSortDefinition(string name, bool ignoreCase, bool ascending)
		{
			this._property = name;
			this._ignoreCase = ignoreCase;
			this._ascending = ascending;
		}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Support.MutableSortDefinition"/> class.
        /// </summary>
		/// <param name="toggleAscendingOnSameProperty">
		/// Whether or not the
		/// <see cref="Spring.Objects.Support.MutableSortDefinition.Ascending"/>
		/// property should be toggled if the same name is set on the
		/// <see cref="Spring.Objects.Support.MutableSortDefinition.Property"/>
		/// property.
		/// </param>
		public MutableSortDefinition (bool toggleAscendingOnSameProperty)
		{
			this.ToggleAscendingOnProperty = toggleAscendingOnSameProperty;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Overrides the default <see cref="System.Object.Equals(object)"/> method
		/// </summary>
		/// <param name="obj">
		/// The object to test against this instance for equality.
		/// </param>
		/// <returns>
		/// True if the supplied <paramref name="obj"/> is equal to this instance.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (!(obj is ISortDefinition))
			{
				return false;
			}
			ISortDefinition sd = (ISortDefinition) obj;
			return (this.Property.Equals(sd.Property) &&
					this.Ascending == sd.Ascending && this.IgnoreCase == sd.IgnoreCase);
		}

		/// <summary>
		/// Overrides the default <see cref="System.Object.GetHashCode"/> method.
		/// </summary>
		/// <returns>The hashcode for this instance.</returns>
		public override int GetHashCode()
		{
			int result = 0;
			result = this.Property.GetHashCode();
			result = 29 * result + (this.IgnoreCase ? 1 : 0);
			result = 29 * result + (this.Ascending ? 1 : 0);
			return result;
		}
		#endregion
	}
}
