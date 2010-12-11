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

namespace Spring.Globalization
{
    /// <summary>
    /// Interface that should be implemented by all formatters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Formatters assume that source value is a string, and make no assumptions
    /// about the target value's type, which means that <c>Parse</c> method can return
    /// object of any type. 
    /// </para>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public interface IFormatter
    {
        /// <summary>
        /// Formats the specified value.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>Formatted <paramref name="value"/>.</returns>
        string Format(object value);

        /// <summary>
        /// Parses the specified value.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>Parsed <paramref name="value"/>.</returns>
        object Parse(string value);
    }
}