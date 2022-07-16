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
	/// A filter that restricts the matching of a pointcut or introduction to
	/// a given set of target types.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Can be used as part of a pointcut, or for the entire targeting of an
	/// introduction.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	/// <seealso cref="Spring.Aop.IPointcut"/>
	/// <seealso cref="TrueTypeFilter.True"/>
	public interface ITypeFilter
	{
		/// <summary>
		/// Should the pointcut apply to the supplied
		/// <see cref="System.Type"/>?
		/// </summary>
		/// <param name="type">
		/// The candidate <see cref="System.Type"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the advice should apply to the supplied
		/// <paramref name="type"/>
		/// </returns>
		bool Matches(Type type);
	}
}
