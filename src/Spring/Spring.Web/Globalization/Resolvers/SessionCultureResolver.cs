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
    /// Culture resolver that uses HTTP session to store culture information.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class SessionCultureResolver : DefaultWebCultureResolver
    {
        private const string CultureKey = "Spring.UserLocale";

        /// <summary>
        /// Resolves the culture from the request.
        /// </summary>
        /// <remarks>
        /// If culture information doesn't exist in the session, it will be created and its value will 
        /// be set to the value of the 'Accept-Language' request header, or if no
        /// headers are specified to the culture of the current server thread.
        /// </remarks>
        /// <returns>Culture that should be used to render view.</returns>
        public override CultureInfo ResolveCulture()
        {
            CultureInfo culture = SessionCulture;
            if (culture != null)
            {
                return culture;
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
            SessionCulture = culture;
        }

        /// <summary>
        /// Gets/Sets the current session's culture.
        /// </summary>
        protected virtual CultureInfo SessionCulture
        {
            set
            {
                AssertUtils.ArgumentNotNull(value, "SessionCulture");
                HttpContext.Current.Session[CultureKey] = value;
            }
            get
            {
                CultureInfo culture = HttpContext.Current.Session[CultureKey] as CultureInfo;
                return culture;
            }
        }
    }
}