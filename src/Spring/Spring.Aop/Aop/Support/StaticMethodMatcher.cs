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
using System.Runtime.Serialization;

#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// Convenient abstract superclass for static method matchers that don't care
	/// about arguments at runtime.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	[Serializable]
	public abstract class StaticMethodMatcher : IMethodMatcher, IDeserializationCallback
	{
		/// <summary>
		/// Is this <see cref="Spring.Aop.IMethodMatcher"/> dynamic?
		/// </summary>
		/// <remarks>
		/// <p>
		/// Always returns <see langword="false"/>.
		/// </p>
		/// </remarks>
		/// <value>
		/// Always returns <see langword="false"/>.
		/// </value>
		public bool IsRuntime
		{
			get { return false; }
		}

		/// <summary>
		/// Is there a runtime (dynamic) match for the supplied
		/// <paramref name="method"/>?
		/// </summary>
		/// <remarks>
		/// <p>
		/// Always throws a <see cref="System.NotSupportedException"/>. This
		/// method should never be called on a static matcher.
		/// </p>
		/// </remarks>
		/// <param name="method">The candidate method.</param>
		/// <param name="targetType">
		/// The target <see cref="System.Type"/>.
		/// </param>
		/// <param name="args">The arguments to the method</param>
		/// <returns>
		/// Always throws a <see cref="System.NotSupportedException"/>.
		/// </returns>
		/// <exception cref="System.NotSupportedException">
		/// Always.
		/// </exception>
		public bool Matches(MethodInfo method, Type targetType, object[] args)
		{
			throw new NotSupportedException(
				"Illegal IMethodMatcher usage. Cannot call 3-arg Matches method on a static matcher.");
		}

		/// <summary>
		/// Does the supplied <paramref name="method"/> satisfy this matcher?
		/// </summary>
		/// <remarks>
		/// <p>
		/// Must be implemented by a derived class in order to specify matching
		/// rules.
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
		public abstract bool Matches(MethodInfo method, Type targetType);


	    void IDeserializationCallback.OnDeserialization(object sender)
	    {
	        OnDeserialization(sender);
	    }

	    /// <summary>
	    /// Override in case you need to initialized non-serialized fields on deserialization.
	    /// </summary>
	    protected virtual void OnDeserialization(object sender)
	    {
	    }
	}
}
