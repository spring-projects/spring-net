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
	/// Convenient abstract superclass for dynamic method matchers that do
	/// care about arguments at runtime.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	[Serializable]
	public abstract class DynamicMethodMatcher : IMethodMatcher
	{
		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Support.DynamicMethodMatcher"/>
		/// class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an <see langword="abstract"/> class, and as such exposes no
		/// public constructors.
		/// </p>
		/// </remarks>
		protected DynamicMethodMatcher()
		{
		}

		#endregion

		/// <summary>
		/// Is this <see cref="Spring.Aop.IMethodMatcher"/> dynamic?
		/// </summary>
		/// <value>
		/// Always returns <see langword="true"/>, to specify that this is a
		/// dynamic matcher.
		/// </value>
		public virtual bool IsRuntime
		{
			get { return true; }
		}

		/// <summary>
		/// Does the supplied <paramref name="method"/> satisfy this matcher?
		/// </summary>
		/// <remarks>
		/// <p>
		/// Derived classes can override this method to add preconditions for
		/// dynamic matching.
		/// </p>
		/// <p>
		/// This implementation always returns <see langword="true"/>.
		/// </p>
		/// </remarks>
		/// <param name="method">The candidate method.</param>
		/// <param name="targetType">
		/// The target <see cref="System.Type"/> (may be <see langword="null"/>,
		/// in which case the candidate <see cref="System.Type"/> must be taken
		/// to be the <paramref name="method"/>'s declaring class).
		/// </param>
		/// <returns>
		/// <see langword="true"/> if this this method matches statically.
		/// </returns>
		public virtual bool Matches(MethodInfo method, Type targetType)
		{
			return true;
		}

		/// <summary>
		/// Is there a runtime (dynamic) match for the supplied
		/// <paramref name="method"/>?
		/// </summary>
		/// <remarks>
		/// <p>
		/// Must be overriden by derived classes to provide criteria for dynamic matching.
		/// </p>
		/// </remarks>
		/// <param name="method">The candidate method.</param>
		/// <param name="targetType">
		/// The target <see cref="System.Type"/>.
		/// </param>
		/// <param name="args">The arguments to the method</param>
		/// <returns>
		/// <see langword="true"/> if there is a runtime match.</returns>
		public abstract bool Matches(MethodInfo method, Type targetType, object[] args);
	}
}
