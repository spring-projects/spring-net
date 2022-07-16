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
	/// Various utility methods relating to the composition of
	/// <see cref="Spring.Aop.IMethodMatcher"/>s.
	/// </summary>
	/// <remarks>
	/// <p>
	/// A method matcher may be evaluated statically (based on method and target
	/// class) or need further evaluation dynamically (based on arguments at
	/// the time of method invocation).
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
    public sealed class MethodMatchers
	{
		/// <summary>
		/// Creates a new <see cref="Spring.Aop.IMethodMatcher"/> that is the
		/// union of the two supplied <see cref="Spring.Aop.IMethodMatcher"/>s.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The newly created matcher will match all the methods that either of the two
		/// supplied matchers would match.
		/// </p>
		/// </remarks>
		/// <param name="firstMatcher">The first method matcher.</param>
		/// <param name="secondMatcher">The second method matcher.</param>
		/// <returns>
		/// A new <see cref="Spring.Aop.IMethodMatcher"/> that is the
		/// union of the two supplied <see cref="Spring.Aop.IMethodMatcher"/>s
		/// </returns>
		public static IMethodMatcher Union(
			IMethodMatcher firstMatcher, IMethodMatcher secondMatcher)
		{
			return new UnionMethodMatcher(firstMatcher, secondMatcher);
		}

		/// <summary>
		/// Creates a new <see cref="Spring.Aop.IMethodMatcher"/> that is the
		/// intersection of the two supplied <see cref="Spring.Aop.IMethodMatcher"/>s.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The newly created matcher will match only those methods that both
		/// of the supplied matchers would match.
		/// </p>
		/// </remarks>
		/// <param name="firstMatcher">The first method matcher.</param>
		/// <param name="secondMatcher">The second method matcher.</param>
		/// <returns>
		/// A new <see cref="Spring.Aop.IMethodMatcher"/> that is the
		/// intersection of the two supplied <see cref="Spring.Aop.IMethodMatcher"/>s
		/// </returns>
		public static IMethodMatcher Intersection(
			IMethodMatcher firstMatcher, IMethodMatcher secondMatcher)
		{
			return new IntersectionMethodMatcher(firstMatcher, secondMatcher);
		}

		#region Inner Class : UnionMethodMatcher

        [Serializable]
        private sealed class UnionMethodMatcher : IMethodMatcher
		{
			private IMethodMatcher a;
			private IMethodMatcher b;

			public UnionMethodMatcher(IMethodMatcher a, IMethodMatcher b)
			{
				this.a = a;
				this.b = b;
			}

			public bool IsRuntime
			{
				get { return a.IsRuntime || b.IsRuntime; }
			}

			public bool Matches(MethodInfo m, Type targetType)
			{
				return a.Matches(m, targetType) || b.Matches(m, targetType);
			}

			public bool Matches(MethodInfo m, Type targetType, object[] args)
			{
				return a.Matches(m, targetType, args) || b.Matches(m, targetType, args);
			}
		}

		#endregion

		#region Inner Class : IntersectionMethodMatcher

        [Serializable]
        private sealed class IntersectionMethodMatcher : IMethodMatcher
		{
			private IMethodMatcher a;
			private IMethodMatcher b;

			public IntersectionMethodMatcher(IMethodMatcher a, IMethodMatcher b)
			{
				this.a = a;
				this.b = b;
			}

			public bool IsRuntime
			{
				get { return a.IsRuntime || b.IsRuntime; }
			}

			public bool Matches(MethodInfo m, Type targetType)
			{
				return a.Matches(m, targetType) && b.Matches(m, targetType);
			}

			public bool Matches(MethodInfo m, Type targetType, object[] args)
			{
				// Because a dynamic intersection may be composed of a static and dynamic part,
				// we must avoid calling the 3-arg matches method on a dynamic matcher, as
				// it will probably be an unsupported operation.
				bool aMatches = a.IsRuntime ? a.Matches(m, targetType, args) : a.Matches(m, targetType);
				bool bMatches = b.IsRuntime ? b.Matches(m, targetType, args) : b.Matches(m, targetType);
				return aMatches && bMatches;
			}
		}

		#endregion

		#region Constructor (s) / Destructor

		// CLOVER:OFF

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Support.MethodMatchers"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a utility class, and as such has no publicly
		/// visible constructors.
		/// </p>
		/// </remarks>
		private MethodMatchers()
		{
		}

		// CLOVER:ON

		#endregion
	}
}
