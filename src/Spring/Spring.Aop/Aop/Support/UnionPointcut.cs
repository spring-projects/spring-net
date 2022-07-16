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

using System.Reflection;

#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// A <see cref="Spring.Aop.IPointcut"/> union.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Such pointcut unions are tricky, because one cannot simply <c>OR</c>
	/// the respective <see cref="Spring.Aop.IMethodMatcher"/>s: one has to
	/// ascertain that each <see cref="Spring.Aop.IMethodMatcher"/>'s
	/// <see cref="Spring.Aop.IPointcut.TypeFilter"/> is also satisfied.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
    [Serializable]
    internal class UnionPointcut : IPointcut
	{
		private IPointcut a;
		private IPointcut b;
		private IMethodMatcher mm;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Support.UnionPointcut"/> class.
		/// </summary>
		/// <param name="firstPointcut">The first pointcut.</param>
		/// <param name="secondPointcut">The second pointcut.</param>
		public UnionPointcut(IPointcut firstPointcut, IPointcut secondPointcut)
		{
			this.a = firstPointcut;
			this.b = secondPointcut;
			this.mm = new PointcutUnionMethodMatcher(this);
		}

		/// <summary>
		/// The <see cref="Spring.Aop.ITypeFilter"/> for this pointcut.
		/// </summary>
		/// <value>
		/// The current <see cref="Spring.Aop.ITypeFilter"/>.
		/// </value>
		public ITypeFilter TypeFilter
		{
			get { return TypeFilters.Union(a.TypeFilter, b.TypeFilter); }

		}

		/// <summary>
		/// The <see cref="Spring.Aop.IMethodMatcher"/> for this pointcut.
		/// </summary>
		/// <value>
		/// The current <see cref="Spring.Aop.IMethodMatcher"/>.
		/// </value>
		public IMethodMatcher MethodMatcher
		{
			get { return mm; }
		}

		/// <summary>
		/// Internal method matcher class for union pointcut.
		/// </summary>
		[Serializable]
		private sealed class PointcutUnionMethodMatcher : IMethodMatcher
		{
			private UnionPointcut _enclosingInstance;

			public PointcutUnionMethodMatcher(UnionPointcut enclosingInstance)
			{
				this._enclosingInstance = enclosingInstance;
			}

			public bool IsRuntime
			{
				get
				{
					return _enclosingInstance.a.MethodMatcher.IsRuntime
						|| _enclosingInstance.b.MethodMatcher.IsRuntime;
				}
			}

			public bool Matches(MethodInfo method, Type targetType)
			{
				return (_enclosingInstance.a.TypeFilter.Matches(targetType)
					&& _enclosingInstance.a.MethodMatcher.Matches(method, targetType))
					|| (_enclosingInstance.b.TypeFilter.Matches(targetType)
						&& _enclosingInstance.b.MethodMatcher.Matches(method, targetType));
			}

			public bool Matches(MethodInfo method, Type targetType, object[] args)
			{
				// 2-arg matcher will already have run, so we don't need to do type filtering again...
				return _enclosingInstance.a.MethodMatcher.Matches(method, targetType, args)
					|| _enclosingInstance.b.MethodMatcher.Matches(method, targetType, args);
			}
		}
	}
}
