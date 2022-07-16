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

namespace Spring.Aop
{
	/// <summary>
	/// That part of an <see cref="Spring.Aop.IPointcut"/> that checks whether a
	/// target method is eligible for advice.
	/// </summary>
	/// <remarks>
	/// <p>
	/// An <see cref="Spring.Aop.IMethodMatcher"/> may be evaluated
	/// <b>statically</b> or at runtime (<b>dynamically</b>). Static
	/// matching involves only the method signature and (possibly) any
	/// <see cref="System.Attribute"/>s that have been applied to a method.
	/// Dynamic matching additionally takes into account the actual argument
	/// values passed to a method invocation.
	/// </p>
	/// <p>
	/// If the value of the <see cref="Spring.Aop.IMethodMatcher.IsRuntime"/>
	/// property of an implementation instance returns <see langword="false"/>,
	/// evaluation can be performed statically, and the result will be the same
	/// for all invocations of this method, whatever their arguments. This
	/// means that if the value of the
	/// <see cref="Spring.Aop.IMethodMatcher.IsRuntime"/> is
	/// <see langword="false"/>, the three argument
	/// <see cref="Spring.Aop.IMethodMatcher.Matches(MethodInfo, Type, object[])"/>
	/// method will never be invoked for the lifetime of the
	/// <see cref="Spring.Aop.IMethodMatcher"/>.
	/// </p>
	/// <p>
	/// If an implementation returns <see langword="true"/> in its two argument
	/// <see cref="Spring.Aop.IMethodMatcher.Matches(MethodInfo, Type)"/>
	/// method, and the value of it's
	/// <see cref="Spring.Aop.IMethodMatcher.IsRuntime"/> property is
	/// <see langword="true"/>, the three argument
	/// <see cref="Spring.Aop.IMethodMatcher.Matches(MethodInfo, Type, object[])"/>
	/// method will be invoked <i>immediately before each and every potential
	/// execution of the related advice</i>, to decide whether the advice
	/// should run. All previous advice, such as earlier interceptors in an
	/// interceptor chain, will have run, so any state changes they have
	/// produced in parameters or thread local storage, will be available at
	/// the time of evaluation.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	/// <seealso cref="TrueMethodMatcher"/>
	public interface IMethodMatcher
	{
		/// <summary>
		/// Is this <see cref="Spring.Aop.IMethodMatcher"/> dynamic?
		/// </summary>
		/// <remarks>
		/// <p>
		/// If <see langword="true"/>, the three argument
		/// <see cref="Spring.Aop.IMethodMatcher.Matches(MethodInfo, Type, object[])"/>
		/// method will be invoked if the two argument
		/// <see cref="Spring.Aop.IMethodMatcher.Matches(MethodInfo, Type)"/>
		/// method returns <see langword="true"/>.
		/// </p>
		/// <p>
		/// Note that this property can be checked when an AOP proxy is created,
		/// and implementations need not check the value of this property again
		/// before each method invocation.
		/// </p>
		/// </remarks>
		/// <value>
		/// <see langword="true"/> if this
		/// <see cref="Spring.Aop.IMethodMatcher"/> is dynamic.
		/// </value>
		bool IsRuntime { get; }

		/// <summary>
		/// Does the supplied <paramref name="method"/> satisfy this matcher?
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a static check. If this method invocation returns
		/// <see langword="false"/>,or if the
		/// <see cref="Spring.Aop.IMethodMatcher.IsRuntime"/> property is
		/// <see langword="false"/>, then no runtime check will be made.
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
		bool Matches(MethodInfo method, Type targetType);

		/// <summary>
		/// Is there a runtime (dynamic) match for the supplied
		/// <paramref name="method"/>?
		/// </summary>
		/// <remarks>
		/// <p>
		/// In order for this method to have even been invoked, the supplied
		/// <paramref name="method"/> must have matched
		/// statically. This method is invoked only if the two argument
		/// <see cref="Spring.Aop.IMethodMatcher.Matches(MethodInfo, Type)"/>
		/// method returns <see langword="true"/> for the supplied
		/// <paramref name="method"/> and <paramref name="targetType"/>, and
		/// if the <see cref="Spring.Aop.IMethodMatcher.IsRuntime"/> property
		/// is <see langword="true"/>.
		/// </p>
		/// <p>
		/// Invoked immediately <b>before</b> any potential running of the
		/// advice, and <b>after</b> any advice earlier in the advice chain has
		/// run.
		/// </p>
		/// </remarks>
		/// <param name="method">The candidate method.</param>
		/// <param name="targetType">
		/// The target <see cref="System.Type"/>.
		/// </param>
		/// <param name="args">The arguments to the method</param>
		/// <returns>
		/// <see langword="true"/> if there is a runtime match.</returns>
		bool Matches(MethodInfo method, Type targetType, object[] args);
	}
}
