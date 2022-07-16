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

using System.Reflection;

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Permits the (re)implementation of an arbitrary method on a Spring.NET
	/// IoC container managed object.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Encapsulates the notion of the Method-Injection form of Dependency
	/// Injection.
	/// </p>
	/// <p>
	/// Methods that are dependency injected with implementations of this
	/// interface may be (but need not be) <see lang="abstract"/>, in which
	/// case the container will create a concrete subclass of the
	/// <see lang="abstract"/> class prior to instantiation.
	/// </p>
	/// <p>
	/// Do <b>not</b> use this mechanism as a means of AOP. See the reference
	/// manual for examples of appropriate usages of this interface.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Rick Evans (.NET)</author>
	public interface IMethodReplacer
	{
		/// <summary>
		/// Reimplement the supplied <paramref name="method"/>.
		/// </summary>
		/// <param name="target">
		/// The instance whose <paramref name="method"/> is to be
		/// (re)implemented.
		/// </param>
		/// <param name="method">
		/// The method that is to be (re)implemented.
		/// </param>
		/// <param name="arguments">The target method's arguments.</param>
		/// <returns>
		/// The result of the (re)implementation of the method call.
		/// </returns>
		object Implement(object target, MethodInfo method, object[] arguments);
	}
}
