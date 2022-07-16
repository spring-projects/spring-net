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

namespace Spring.Core
{
	/// <summary>
	/// Interface to be implemented by objects that can return information about
	/// the current call stack.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Useful in AOP (as an expression of the AspectJ <c>cflow</c> concept) but not AOP-specific.
	/// </p>
	/// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Aleksandar Seovic (.Net)</author>
    public interface IControlFlow
	{
        /// <summary>
        /// Detects whether the caller is under the supplied <see cref="System.Type"/>,
        /// according to the current stacktrace.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> to look for.
        /// </param>
        /// <returns>
		/// <see langword="true"/> if the caller is under the supplied <see cref="System.Type"/>.
		/// </returns>
		bool Under(Type type);

		/// <summary>
		/// Detects whether the caller is under the supplied <see cref="System.Type"/>
		/// and <paramref name="methodName"/>, according to the current stacktrace.
		/// </summary>
		/// <param name="type">
		/// The <see cref="System.Type"/> to look for.
		/// </param>
		/// <param name="methodName">The name of the method to look for.</param>
		/// <returns>
		/// <see langword="true"/> if the caller is under the supplied <see cref="System.Type"/>
		/// and <paramref name="methodName"/>.
		/// </returns>
        bool Under(Type type, string methodName);

        /// <summary>
        /// Does the current stack trace contain the supplied <paramref name="token"/>?
        /// </summary>
        /// <param name="token">The token to match against.</param>
        /// <returns>
        /// <see langword="true"/> if the current stack trace contains the supplied
        /// <paramref name="token"/>.
        /// </returns>
		bool UnderToken(string token);
	}
}
