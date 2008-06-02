#region License

/*
 * Copyright 2002-2007 the original author or authors.
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

using System;
using System.Globalization;
using System.Threading;

#endregion

namespace Spring.Globalization
{
    /// <summary>
    /// Helps setting/resetting current thread cultures.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class CultureTestScope : IDisposable
    {
        [ThreadStatic]
        private static CultureTestScope s_currentScope;

        public static void Set()
        {
            Set("en-GB", "de-DE");
        }

        public static void Set(string culture, string uiCulture)
        {
            s_currentScope = new CultureTestScope(culture, uiCulture);
        }

        public static void Reset()
        {
            CultureTestScope scope = s_currentScope; s_currentScope = null;
            scope.Dispose();
        }

        private readonly CultureInfo _prevCulture;
        private readonly CultureInfo _prevUICulture;

        private CultureTestScope(string culture, string uiCulture)
        {
            this._prevCulture = Thread.CurrentThread.CurrentCulture;
            this._prevUICulture = Thread.CurrentThread.CurrentUICulture;

            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(uiCulture);
        }

        void IDisposable.Dispose()
        {
            this.Dispose();
        }

        private void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = this._prevCulture;
            Thread.CurrentThread.CurrentUICulture = this._prevUICulture;
        }
    }
}