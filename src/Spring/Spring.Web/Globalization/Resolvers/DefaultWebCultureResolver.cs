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
using Spring.Util;

namespace Spring.Globalization.Resolvers
{
    /// <summary>
    /// Default culture resolver for web applications. Contains some common utility methods for web culture resolvers.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class DefaultWebCultureResolver : DefaultCultureResolver
    {
        /// <summary>
        /// Returns default culture. If <see cref="DefaultCultureResolver.DefaultCulture"/> property is not set,
        /// it tries to get culture from the request headers
        /// and falls back to a current thread's culture if no headers are available.
        /// </summary>
        /// <returns>Default culture to use.</returns>
        protected override CultureInfo GetDefaultLocale()
        {
            if (DefaultCulture != null)
            {
                return base.DefaultCulture;
            }

            CultureInfo culture = GetCulture(GetRequestLanguage());
            if (culture != null)
            {
                return culture;
            }

            return Thread.CurrentThread.CurrentUICulture;
        }

        /// <summary>
        /// Extracts the users favorite language from "accept-language" header of the current request.
        /// </summary>
        /// <returns>a language string if any or <c>null</c>, if no languages have been sent with the request</returns>
        protected virtual string GetRequestLanguage()
        {
            HttpContext context = HttpContext.Current;
            if (context != null && context.Request != null && ArrayUtils.HasLength(context.Request.UserLanguages))
            {
                return context.Request.UserLanguages[0];
            }
            return null;
        }

        /// <summary>
        /// Resolves a culture by name.
        /// </summary>
        /// <param name="cultureName">the name of the culture to get</param>
        /// <returns>a (possible neutral!) <see cref="CultureInfo"/> or <c>null</c>, if culture could not be resolved</returns>
        public CultureInfo GetCulture(string cultureName)
        {
            try { return new CultureInfo(cultureName.Split(';')[0]); } catch { }
            return null;
        }

        /// <summary>
        /// Resolves the culture from the context.
        /// </summary>
        /// <returns>Culture that should be used to render view.</returns>
        public override CultureInfo ResolveCulture()
        {
            return GetDefaultLocale();
        }

        /// <summary>
        /// Not supported for this implementation.
        /// </summary>
        /// <param name="culture">The new culture or <code>null</code> to clear the culture.</param>
        public override void SetCulture(CultureInfo culture)
        {
            throw new NotSupportedException("Cannot change a default culture in a web application - use a different culture resolution strategy.");
        }
    }
}
