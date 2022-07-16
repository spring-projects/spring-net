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
using System.Web;

namespace Spring.Globalization.Resolvers
{
    /// <summary>
    /// Culture resolver that uses request headers to determine culture. If no languages
    /// are specified in the request headers, it returns default culture specifed, and
    /// if no default culture was specifed it returns current culture for the executing
    /// server thread.
    /// </summary>
    /// <remarks>
    /// <b>Note:</b> This culture resolver cannot be used to change the culture
    /// because request headers cannot be modified. In order to change the culture
    /// when using this culture resolver user has to change language settings in
    /// the web browser.
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class RequestCultureResolver : DefaultWebCultureResolver
    {
        /// <summary>
        /// Tries to determine culture from the request headers. If no languages
        /// are specified in the request headers, it returns default culture specifed, and
        /// if no default culture was specifed it returns current culture for the executing
        /// server thread.
        /// </summary>
        /// <returns>Culture that should be used to render view.</returns>
        public override CultureInfo ResolveCulture()
        {
            HttpContext context = HttpContext.Current;

            CultureInfo culture = GetCulture(GetRequestLanguage());
            if (culture != null)
            {
                return culture;
            }

            if (DefaultCulture != null)
            {
                return base.DefaultCulture;
            }

            return Thread.CurrentThread.CurrentUICulture;
        }

        /// <summary>
        /// Not supported for this resolver implementation
        /// </summary>
        /// <param name="culture">The new culture or <code>null</code> to clear the culture.</param>
        public override void SetCulture(CultureInfo culture)
        {
            throw new NotSupportedException("Cannot change HTTP request headers - use a different culture resolution strategy.");
        }
    }
}
