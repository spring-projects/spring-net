#region License

/*
 * Copyright 2002-2011 the original author or authors.
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

namespace Spring.Util
{
    /// <summary>
    /// Miscellaneous <see cref="System.String"/> utility methods.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Mainly for internal use within the framework.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Keith Donald</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <author>Rick Evans (.NET)</author>
    /// <author>Erich Eichinger (.NET)</author>
    internal sealed class StringUtils
    {
        /// <summary>Checks if a string has length.</summary>
        /// <param name="target">
        /// The string to check, may be <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the string has length and is not
        /// <see langword="null"/>.
        /// </returns>
        /// <example>
        /// <code lang="C#">
        /// StringUtils.HasLength(null) = false
        /// StringUtils.HasLength("") = false
        /// StringUtils.HasLength(" ") = true
        /// StringUtils.HasLength("Hello") = true
        /// </code>
        /// </example>
        internal static bool HasLength(string target)
        {
            return (target != null && target.Length > 0);
        }

        /// <summary>
        /// Checks if a <see cref="System.String"/> has text.
        /// </summary>
        /// <remarks>
        /// <p>
        /// More specifically, returns <see langword="true"/> if the string is
        /// not <see langword="null"/>, it's <see cref="String.Length"/> is >
        /// zero <c>(0)</c>, and it has at least one non-whitespace character.
        /// </p>
        /// </remarks>
        /// <param name="target">
        /// The string to check, may be <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="target"/> is not
        /// <see langword="null"/>,
        /// <see cref="String.Length"/> > zero <c>(0)</c>, and does not consist
        /// solely of whitespace.
        /// </returns>
        /// <example>
        /// <code language="C#">
        /// StringUtils.HasText(null) = false
        /// StringUtils.HasText("") = false
        /// StringUtils.HasText(" ") = false
        /// StringUtils.HasText("12345") = true
        /// StringUtils.HasText(" 12345 ") = true
        /// </code>
        /// </example>
        internal static bool HasText(string target)
        {
            if (target == null)
            {
                return false;
            }
            else
            {
                return HasLength(target.Trim());
            }
        }
    }
}