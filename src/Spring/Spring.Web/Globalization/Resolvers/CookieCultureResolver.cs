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
    /// Culture resolver that uses cookie to store culture information.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class CookieCultureResolver : DefaultWebCultureResolver
    {
        private const string CultureKey = "Spring.UserLocale";

        /// <summary>
        /// Resolves the culture from the request.
        /// </summary>
        /// <remarks>
        /// If the culture cookie doesn't exist, this method will return
        /// the value of the 'Accept-Language' request header, or if no
        /// headers are specified, the culture of the current server thread.
        /// </remarks>
        /// <returns>Culture that should be used to render view.</returns>
        public override CultureInfo ResolveCulture()
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[CultureKey];

            if (cookie != null)
            {
                return base.GetCulture(cookie.Value);
            }
            else
            {
                return base.GetDefaultLocale();
            }
        }

        /// <summary>
        /// Sets the culture.
        /// </summary>
        /// <param name="culture">The new culture or <code>null</code> to clear the culture.</param>
        public override void SetCulture(CultureInfo culture)
        {
            HttpCookie cookie = CreateCookie(culture);
            HttpContext.Current.Request.Cookies.Set(cookie);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Creates cookie for the specified culture.
        /// </summary>
        /// <param name="culture">Culture to store in a cookie.</param>
        /// <returns>Created cookie.</returns>
        private HttpCookie CreateCookie(CultureInfo culture)
        {
            HttpCookie cookie = new HttpCookie(CultureKey);
            cookie.Domain = HttpContext.Current.Request.Url.Host;
            cookie.Expires = DateTime.MaxValue;
            cookie.Value = culture.Name;

            return cookie;
        }
    }
}
