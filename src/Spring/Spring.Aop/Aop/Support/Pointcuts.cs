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
	/// Various <see cref="Spring.Aop.IPointcut"/> related utility methods.
	/// </summary>
	/// <remarks>
	/// <p>
	/// These methods are particularly useful for composing pointcuts
	/// using the union and intersection methods.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
    public sealed class Pointcuts
	{
		/// <summary>
		/// Creates a union of the two supplied pointcuts.
		/// </summary>
		/// <param name="firstPointcut">The first pointcut.</param>
		/// <param name="secondPointcut">The second pointcut.</param>
		/// <returns>
		/// The union of the two supplied pointcuts.
		/// </returns>
		/// <seealso cref="Spring.Aop.Support.UnionPointcut"/>
		public static IPointcut Union(IPointcut firstPointcut, IPointcut secondPointcut)
		{
			return new UnionPointcut(firstPointcut, secondPointcut);
		}

		/// <summary>
		/// Creates an <see cref="Spring.Aop.IPointcut"/> that is the
		/// intersection of the two supplied pointcuts.
		/// </summary>
		/// <param name="firstPointcut">The first pointcut.</param>
		/// <param name="secondPointcut">The second pointcut.</param>
		/// <returns>
		/// An <see cref="Spring.Aop.IPointcut"/> that is the
		/// intersection of the two supplied pointcuts.
		/// </returns>
		public static IPointcut Intersection(IPointcut firstPointcut, IPointcut secondPointcut)
		{
			return new ComposablePointcut(
				firstPointcut.TypeFilter, firstPointcut.MethodMatcher)
				.Intersection(secondPointcut);
		}

		/// <summary>
		/// Performs the least expensive check for a match.
		/// </summary>
		/// <param name="pointcut">
		/// The <see cref="Spring.Aop.IPointcut"/> to be evaluated.
		/// </param>
		/// <param name="method">The candidate method.</param>
		/// <param name="targetType">
		/// The target <see cref="System.Type"/>.
		/// </param>
		/// <param name="args">The arguments to the method</param>
		/// <returns><see langword="true"/> if there is a runtime match.</returns>
		/// <seealso cref="Spring.Aop.IMethodMatcher.Matches(MethodInfo, Type)"/>
		public static bool Matches(
			IPointcut pointcut, MethodInfo method, Type targetType, object[] args)
		{
			if(pointcut != null)
			{
				if (pointcut == TruePointcut.True)
				{
					return true;
				}
				if (pointcut.TypeFilter.Matches(targetType))
				{
					IMethodMatcher mm = pointcut.MethodMatcher;
					if (mm.Matches(method, targetType))
					{
						return mm.IsRuntime ? mm.Matches(method, targetType, args) : true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Are the supplied <see cref="Spring.Aop.IPointcut"/>s equal?
		/// </summary>
		/// <param name="firstPointcut">The first pointcut.</param>
		/// <param name="secondPointcut">The second pointcut.</param>
		/// <returns>
		/// <see langword="true"/> if the supplied <see cref="Spring.Aop.IPointcut"/>s
		/// are equal.
		/// </returns>
		public static bool AreEqual(IPointcut firstPointcut, IPointcut secondPointcut)
		{
			return firstPointcut.TypeFilter == secondPointcut.TypeFilter
				&& firstPointcut.MethodMatcher == secondPointcut.MethodMatcher;
		}

		#region Constructor (s) / Destructor

		// CLOVER:OFF

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Support.Pointcuts"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a utility class, and as such has no publicly
		/// visible constructors.
		/// </p>
		/// </remarks>
		private Pointcuts()
		{
		}

		// CLOVER:ON

		#endregion
	}
}
