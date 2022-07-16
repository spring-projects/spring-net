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

namespace Spring.Aop
{
	/// <summary>
	/// Used to obtain the current "target" of an AOP invocation
	/// </summary>
	/// <remarks>
	/// <p>
	/// This target will be invoked via reflection if no around advice chooses
	/// to end the interceptor chain itself.
	/// </p>
	/// <p>
	/// If an <see cref="Spring.Aop.ITargetSource"/> is <c>"static"</c>, it
	/// will always return the same target, allowing optimizations in the AOP
	/// framework. Dynamic target sources can support pooling, hot swapping etc.
	/// </p>
	/// <p>
	/// Application developers don't usually need to work with target sources
	/// directly: this is an AOP framework interface.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	public interface ITargetSource
	{
        /// <summary>
        /// The <see cref="System.Type"/> of the target object.
        /// </summary>
        Type TargetType { get; }

		/// <summary>
		/// Is the target source static?
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the target source is static.
		/// </value>
		bool IsStatic { get; }

		/// <summary>
		/// Returns the target object.
		/// </summary>
		/// <returns>The target object.</returns>
		/// <exception cref="System.Exception">
		/// If unable to obtain the target object.
		/// </exception>
		object GetTarget();

		/// <summary>
		/// Releases the target object.
		/// </summary>
		/// <param name="target">The target object to release.</param>
		void ReleaseTarget(object target);
	}
}
