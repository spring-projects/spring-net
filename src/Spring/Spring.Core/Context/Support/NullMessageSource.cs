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

using System.Globalization;

namespace Spring.Context.Support
{
    /// <summary>
    /// An <see cref="Spring.Context.IMessageSource"/> that doesn't do a whole lot.
    /// </summary>
    /// <remarks>
    /// <p>
    /// <see cref="Spring.Context.Support.NullMessageSource"/> is an implementation of
    /// the <c>NullObject</c> pattern. It should be used in those situations where a
    /// <see cref="Spring.Context.IMessageSource"/> needs to be passed (say to a
    /// method) but where the resolution of messages is not required.
    /// </p>
    /// <p>
    /// There should not (typically) be a need to instantiate instances of this class;
    /// <see cref="Spring.Context.Support.NullMessageSource"/> does not maintan any state
    /// and the <see cref="Spring.Context.Support.NullMessageSource.Null"/> instance is
    /// thus safe to pass around.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public sealed class NullMessageSource : AbstractMessageSource
    {
        /// <summary>
        /// The canonical instance of the
        /// <see cref="Spring.Context.Support.NullMessageSource"/> class.
        /// </summary>
        public static readonly NullMessageSource Null = new NullMessageSource();

        /// <summary>
        /// Creates a new instance of the <see cref="NullMessageSource"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Consider using <see cref="Spring.Context.Support.NullMessageSource.Null"/>
        /// instead.
        /// </p>
        /// </remarks>
        public NullMessageSource()
        {}

        /// <summary>
        /// Simply returns the supplied message <paramref name="code"/> as-is.
        /// </summary>
        /// <param name="code">The code of the message to resolve.</param>
        /// <param name="cultureInfo">
        /// The <see cref="System.Globalization.CultureInfo"/> to resolve the
        /// code for.
        /// </param>
        /// <returns>
        /// The supplied message <paramref name="code"/> as-is.
        /// </returns>
        protected override string ResolveMessage(string code, CultureInfo cultureInfo)
        {
            return code;
        }

        /// <summary>
        /// Always returns <see lang="null"/>.
        /// </summary>
        /// <param name="code">The code of the object to resolve.</param>
        /// <param name="cultureInfo">
        /// The <see cref="System.Globalization.CultureInfo"/> to resolve the
        /// code for.
        /// </param>
        /// <returns>
        /// <see lang="null"/> (always).
        /// </returns>
        protected override object ResolveObject(string code, CultureInfo cultureInfo)
        {
            return null;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="value">
        /// An object that contains the property values to be applied.
        /// </param>
        /// <param name="objectName">
        /// The base name of the object to use for key lookup.
        /// </param>
        /// <param name="cultureInfo">
        /// The <see cref="System.Globalization.CultureInfo"/> with which the
        /// resource is associated.
        /// </param>
        protected override void ApplyResourcesToObject(
            object value, string objectName, CultureInfo cultureInfo)
        {}
    }
}
