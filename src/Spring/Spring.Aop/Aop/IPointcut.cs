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
	/// Spring.NET's core pointcut abstraction.
	/// </summary>
	/// <remarks>
	/// <p>
	/// A pointcut is composed of <see cref="Spring.Aop.ITypeFilter"/>s and
	/// <see cref="Spring.Aop.IMethodMatcher"/>s. Both these basic terms and an
	/// <see cref="Spring.Aop.IPointcut"/> itself can be combined to build up
	/// sophisticated combinations.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	public interface IPointcut
	{
		/// <summary>
		/// The <see cref="Spring.Aop.ITypeFilter"/> for this pointcut.
		/// </summary>
		/// <value>
		/// The current <see cref="Spring.Aop.ITypeFilter"/>.
		/// </value>
		ITypeFilter TypeFilter { get; }

		/// <summary>
		/// The <see cref="Spring.Aop.IMethodMatcher"/> for this pointcut.
		/// </summary>
		/// <value>
		/// The current <see cref="Spring.Aop.IMethodMatcher"/>.
		/// </value>
		IMethodMatcher MethodMatcher { get; }
	}
}