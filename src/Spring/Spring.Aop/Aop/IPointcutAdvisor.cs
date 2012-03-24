#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

namespace Spring.Aop
{
	/// <summary>
	/// Superinterface for all <see cref="Spring.Aop.IAdvisor"/>s that are
	/// driven by a pointcut.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This covers nearly all advisors except introduction advisors, for which
	/// method-level matching does not apply.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	/// <seealso cref="Spring.Aop.IIntroductionAdvisor"/>
	public interface IPointcutAdvisor : IAdvisor
	{
		/// <summary>
		/// The <see cref="Spring.Aop.IPointcut"/> that drives this advisor.
		/// </summary>
		IPointcut Pointcut { get; }
	}
}