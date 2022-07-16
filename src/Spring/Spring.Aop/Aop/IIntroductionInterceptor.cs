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

using AopAlliance.Intercept;

#endregion

namespace Spring.Aop
{
	/// <summary>
	/// Subinterface of the AOP Alliance
	/// <see cref="AopAlliance.Intercept.IMethodInterceptor"/> interface that
	/// allows additional interfaces to be implemented by the interceptor, and
	/// available via a proxy using that interceptor.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This is a fundamental AOP concept called <b>introduction</b>.
	/// </p>
	/// <p>
	/// Introductions are often <b>mixins</b>, enabling the building of composite
	/// objects that can achieve many of the goals of multiple inheritance.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	public interface IIntroductionInterceptor : IMethodInterceptor
	{
		/// <summary>
		/// Does this <see cref="Spring.Aop.IIntroductionInterceptor"/>
		/// implement the given interface?
		/// </summary>
		/// <param name="intf">The interface to check.</param>
		/// <returns>
		/// <see langword="true"/> if this
		/// <see cref="Spring.Aop.IIntroductionInterceptor"/>
		/// implements the given interface.
		/// </returns>
		bool ImplementsInterface(Type intf);
	}
}
