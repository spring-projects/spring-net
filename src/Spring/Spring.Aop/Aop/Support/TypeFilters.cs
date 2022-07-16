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

#region Imports

#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// Defines miscellaneous <see cref="System.Type"/> filter operations.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.Net)</author>
	public sealed class TypeFilters
	{
		/// <summary>
		/// Creates a union of two <see cref="System.Type"/> filters.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The filter arising from the union will match all of the
		/// <see cref="System.Type"/> that either of the two supplied filters
		/// would match.
		/// </p>
		/// </remarks>
		/// <param name="first">
		/// The first <see cref="System.Type"/> filter.
		/// </param>
		/// <param name="second">
		/// The second <see cref="System.Type"/> filter.
		/// </param>
		/// <returns>
		/// The union of the supplied <see cref="System.Type"/> filters.
		/// </returns>
		public static ITypeFilter Union(ITypeFilter first, ITypeFilter second)
		{
			return new UnionTypeFilter(new ITypeFilter[] {first, second});
		}

		/// <summary>
		/// Creates the intersection of two <see cref="System.Type"/> filters.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The filter arising from the intersection will match all of the
		/// <see cref="System.Type"/> that both of the two supplied filters
		/// would match.
		/// </p>
		/// </remarks>
		/// <param name="first">
		/// The first <see cref="System.Type"/> filter.
		/// </param>
		/// <param name="second">
		/// The second <see cref="System.Type"/> filter.
		/// </param>
		/// <returns>
		/// The intersection of the supplied <see cref="System.Type"/> filters.
		/// </returns>
		public static ITypeFilter Intersection(ITypeFilter first, ITypeFilter second)
		{
			return new IntersectionTypeFilter(new ITypeFilter[] {first, second});
		}

		/// <summary>
		/// Union class filter implementation.
		/// </summary>
        [Serializable]
        private sealed class UnionTypeFilter : ITypeFilter
		{
			private ITypeFilter[] _filters;

			public UnionTypeFilter(ITypeFilter[] filters)
			{
				_filters = filters;
			}

			public bool Matches(Type type)
			{
				for (int i = 0; i < _filters.Length; i++)
				{
					if (_filters[i].Matches(type))
					{
						return true;
					}
				}
				return false;
			}
		}

		/// <summary>
		/// Intersection <see cref="Spring.Aop.ITypeFilter"/> implementation.
		/// </summary>
        [Serializable]
        private sealed class IntersectionTypeFilter : ITypeFilter
		{
			private ITypeFilter[] _filters;

			public IntersectionTypeFilter(ITypeFilter[] filters)
			{
				_filters = filters;
			}

			public bool Matches(Type type)
			{
				for (int i = 0; i < _filters.Length; i++)
				{
					if (!_filters[i].Matches(type))
					{
						return false;
					}
				}
				return true;
			}
		}

		#region Constructor (s) / Destructor

		// CLOVER:OFF

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Support.TypeFilters"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a utility class, and as such has no publicly visible constructors.
		/// </p>
		/// </remarks>
		private TypeFilters()
		{
		}

		// CLOVER:ON

		#endregion
	}
}
