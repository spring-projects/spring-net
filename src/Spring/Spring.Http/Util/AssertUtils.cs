#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using System;
using System.Globalization;

namespace Spring.Util
{
	/// <summary>
	/// Assertion utility methods that simplify things such as argument checks.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Not intended to be used directly by applications.
	/// </p>
	/// </remarks>
	/// <author>Aleksandar Seovic</author>
	/// <author>Erich Eichinger</author>
	internal sealed class AssertUtils
	{
        /// <summary>
		/// Checks the value of the supplied <paramref name="argument"/> and throws an
		/// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/>.
		/// </summary>
		/// <param name="argument">The object to check.</param>
		/// <param name="name">The argument name.</param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="argument"/> is <see langword="null"/>.
		/// </exception>
		internal static void ArgumentNotNull(object argument, string name)
		{
			if (argument == null)
			{
				throw new ArgumentNullException (name,
                    String.Format(CultureInfo.InvariantCulture, "Argument '{0}' cannot be null.", name));
			}
		}

		/// <summary>
		/// Checks the value of the supplied <paramref name="argument"/> and throws an
		/// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/>.
		/// </summary>
		/// <param name="argument">The object to check.</param>
		/// <param name="name">The argument name.</param>
		/// <param name="message">
		/// An arbitrary message that will be passed to any thrown
		/// <see cref="System.ArgumentNullException"/>.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="argument"/> is <see langword="null"/>.
		/// </exception>
        internal static void ArgumentNotNull(object argument, string name, string message)
		{
			if (argument == null)
			{
				throw new ArgumentNullException(name, message);
			}
		}

		/// <summary>
		/// Checks the value of the supplied string <paramref name="argument"/> and throws an
        /// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/> or
		/// contains only whitespace character(s).
		/// </summary>
		/// <param name="argument">The string to check.</param>
		/// <param name="name">The argument name.</param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="argument"/> is <see langword="null"/> or
		/// contains only whitespace character(s).
		/// </exception>
        internal static void ArgumentHasText(string argument, string name)
		{
			if (!StringUtils.HasText(argument))
			{
				throw new ArgumentNullException(name,
					String.Format (CultureInfo.InvariantCulture,
                        "Argument '{0}' cannot be null or resolve to an empty string : '{1}'.", name, argument));
			}
		}

		/// <summary>
		/// Checks the value of the supplied string <paramref name="argument"/> and throws an
		/// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/> or
		/// contains only whitespace character(s).
		/// </summary>
		/// <param name="argument">The string to check.</param>
		/// <param name="name">The argument name.</param>
		/// <param name="message">
		/// An arbitrary message that will be passed to any thrown
		/// <see cref="System.ArgumentNullException"/>.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="argument"/> is <see langword="null"/> or
		/// contains only whitespace character(s).
		/// </exception>
        internal static void ArgumentHasText(string argument, string name, string message)
		{
			if (!StringUtils.HasText(argument))
			{
				throw new ArgumentNullException(name, message);
			}
		}
	}
}