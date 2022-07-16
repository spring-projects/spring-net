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

namespace Spring.Objects.Support
{
	/// <summary>
	/// Definition for sorting object instances by a property.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Simon White (.NET)</author>
	public interface ISortDefinition
	{
		/// <summary>
		/// The name of the property to sort by.
		/// </summary>
		string Property
		{
			get;
		}

		/// <summary>
		/// Whether upper and lower case in string values should be ignored.
		/// </summary>
		/// <value>
		/// True if the sorting should be performed in a case-insensitive fashion.
		/// </value>
		bool IgnoreCase
		{
			get;
		}

		/// <summary>
		/// If the sorting should be ascending or descending.
		/// </summary>
		/// <value>
		/// True if the sorting should be in the ascending order.
		/// </value>
		bool Ascending
		{
			get;
		}
	}
}
