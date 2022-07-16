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
	/// Convenient class for building up pointcuts.
	/// </summary>
	/// <remarks>
	/// <p>
	/// All methods return a <see cref="Spring.Aop.Support.ComposablePointcut"/>
	/// instance, which facilitates the following concise usage pattern...
	/// </p>
	/// <code language="C#">
	/// IPointcut pointcut = new ComposablePointcut()
	///			.Union(typeFilter)
	///				.Intersection(methodMatcher)
	///					.Intersection(pointcut);
	/// </code>
	/// <p>
	/// There is no <c>Union()</c> method on this class. Use the
	/// <see cref="Spring.Aop.Support.Pointcuts.Union"/> method for such functionality.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	[Serializable]
    public class ComposablePointcut : IPointcut
	{
		private ITypeFilter _typeFilter;
		private IMethodMatcher _methodMatcher;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Support.ComposablePointcut"/> class
		/// that matches all the methods on all <see cref="System.Type"/>s.
		/// </summary>
		public ComposablePointcut()
		{
			_typeFilter = TrueTypeFilter.True;
			_methodMatcher = TrueMethodMatcher.True;
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Support.ComposablePointcut"/> class
		/// that uses the supplied <paramref name="typeFilter"/> and
		/// <paramref name="methodMatcher"/>.
		/// </summary>
		/// <param name="typeFilter">
		/// The type filter to use.
		/// </param>
		/// <param name="methodMatcher">
		/// The method matcher to use.
		/// </param>
		public ComposablePointcut(ITypeFilter typeFilter, IMethodMatcher methodMatcher)
		{
			_typeFilter = typeFilter;
			_methodMatcher = methodMatcher;
		}

		/// <summary>
		/// The <see cref="Spring.Aop.ITypeFilter"/> for this pointcut.
		/// </summary>
		/// <value>
		/// The current <see cref="Spring.Aop.ITypeFilter"/>.
		/// </value>
		public virtual ITypeFilter TypeFilter
		{
			get { return _typeFilter; }
		}

		/// <summary>
		/// The <see cref="Spring.Aop.IMethodMatcher"/> for this pointcut.
		/// </summary>
		/// <value>
		/// The current <see cref="Spring.Aop.IMethodMatcher"/>.
		/// </value>
		public virtual IMethodMatcher MethodMatcher
		{
			get { return _methodMatcher; }
		}

		/// <summary>
		/// Changes the current type filter to be the union of the existing filter and the
		/// supplied <paramref name="filter"/>.
		/// </summary>
		/// <param name="filter">The filter to union with.</param>
		/// <returns>
		/// The union of the existing filter and the supplied <paramref name="filter"/>.
		/// </returns>
		public virtual ComposablePointcut Union(ITypeFilter filter)
		{
			_typeFilter = TypeFilters.Union(_typeFilter, filter);
			return this;
		}

		/// <summary>
		/// Changes the current type filter to be the intersection of the existing filter
		/// and the supplied <paramref name="filter"/>.
		/// </summary>
		/// <param name="filter">The filter to diff against.</param>
		/// <returns>
		/// The intersection of the existing filter and the supplied <paramref name="filter"/>.
		/// </returns>
		public virtual ComposablePointcut Intersection(ITypeFilter filter)
		{
			_typeFilter = TypeFilters.Intersection(_typeFilter, filter);
			return this;
		}

		/// <summary>
		/// Changes the current method matcher to be the union of the existing matcher and the
		/// supplied <paramref name="matcher"/>.
		/// </summary>
		/// <param name="matcher">The matcher to union with.</param>
		/// <returns>
		/// The union of the existing matcher and the supplied <paramref name="matcher"/>.
		/// </returns>
		public virtual ComposablePointcut Union(IMethodMatcher matcher)
		{
			_methodMatcher = MethodMatchers.Union(_methodMatcher, matcher);
			return this;
		}

		/// <summary>
		/// Changes the current method matcher to be the intersection of the existing matcher
		/// and the supplied <paramref name="matcher"/>.
		/// </summary>
		/// <param name="matcher">The matcher to diff against.</param>
		/// <returns>
		/// The intersection of the existing matcher and the supplied <paramref name="matcher"/>.
		/// </returns>
		public virtual ComposablePointcut Intersection(IMethodMatcher matcher)
		{
			_methodMatcher = MethodMatchers.Intersection(_methodMatcher, matcher);
			return this;
		}

		/// <summary>
		/// Changes current pointcut to intersection of the current and supplied pointcut
		/// </summary>
		/// <param name="other">pointcut to diff against</param>
		/// <returns>updated pointcut</returns>
		public virtual ComposablePointcut Intersection(IPointcut other)
		{
			_typeFilter = TypeFilters.Intersection(_typeFilter, other.TypeFilter);
			_methodMatcher = MethodMatchers.Intersection(_methodMatcher, other.MethodMatcher);
			return this;
		}
	}
}
