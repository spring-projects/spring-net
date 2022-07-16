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

using System.Globalization;

namespace Spring.Globalization
{
    /// <summary>
    /// Strategy interface for <see cref="System.Globalization.CultureInfo"/>
    /// resolution.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public interface ICultureResolver
    {
        /// <summary>
        /// Resolves the <see cref="System.Globalization.CultureInfo"/>
        /// from some context.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The 'context' is determined by the appropriate implementation class.
        /// An example of such a context might be a thread local bound
        /// <see cref="System.Globalization.CultureInfo"/>, or a
        /// <see cref="System.Globalization.CultureInfo"/> sourced from an HTTP
        /// session.
        /// </p>
        /// </remarks>
        /// <returns>
        /// The <see cref="System.Globalization.CultureInfo"/> that should be used
        /// by the caller.
        ///  </returns>
        CultureInfo ResolveCulture();

        /// <summary>
        /// Sets the <see cref="System.Globalization.CultureInfo"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an optional operation and does not need to be implemented
        /// such that it actually does anything useful (i.e. it can be a no-op).
        /// </p>
        /// </remarks>
        /// <param name="culture">
        /// The new <see cref="System.Globalization.CultureInfo"/> or
        /// <cref lang="null"/> to clear the current <see cref="System.Globalization.CultureInfo"/>.
        /// </param>
        void SetCulture(CultureInfo culture);
    }
}
