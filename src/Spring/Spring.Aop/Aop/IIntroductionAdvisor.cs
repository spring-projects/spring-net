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
	/// Superinterface for advisors that perform one or more AOP
	/// <b>introductions</b>.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This interface cannot be implemented directly; subinterfaces must
	/// provide the advice type implementing the introduction.
	/// </p>
	/// <p>
	/// Introduction is the implementation of additional interfaces (not
	/// implemented by a target) via AOP advice.
	/// </p>
	/// </remarks>
	/// <seealso cref="Spring.Aop.IIntroductionInterceptor"/>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	public interface IIntroductionAdvisor : IAdvisor
	{
		/// <summary>
		/// Returns the filter determining which target classes this
		/// introduction should apply to.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is the <see cref="System.Type"/> part of a pointcut.
		/// Be advised that method matching doesn't make sense in the context
		/// of introductions.
		/// </p>
		/// </remarks>
		/// <value>
		/// The filter determining which target classes this introduction
		/// should apply to.
		/// </value>
		ITypeFilter TypeFilter { get; }

		/// <summary>
		/// Gets the interfaces introduced by this
		/// <see cref="Spring.Aop.IAdvisor"/>.
		/// </summary>
		/// <value>
		/// The interfaces introduced by this
		/// <see cref="Spring.Aop.IAdvisor"/>.
		/// </value>
		Type[] Interfaces { get; }

		/// <summary>
		/// Can the advised interfaces be implemented by the introduction
		/// advice?
		/// </summary>
		/// <remarks>
		/// <p>
		/// Invoked <b>before</b> adding an
		/// <seealso cref="Spring.Aop.IIntroductionAdvisor"/>.
		/// </p>
		/// </remarks>
		/// <exception cref="System.ArgumentException">
		/// If the advised interfaces cannot be implemented by the introduction
		/// advice.
		/// </exception>
		/// <seealso cref="Spring.Aop.IIntroductionAdvisor.Interfaces"/>
		void ValidateInterfaces();
	}
}
