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

using System.Globalization;

namespace Spring.Globalization
{
    /// <summary>
    /// Helper class to pick the appropriate culture name for serbian due to changes of the CultureInfo classes
    /// in recent releases of the .NET framework.
    /// </summary>
    /// <remarks>In short, sr-SP-Latn->sr-Latn-CS and sr-SP-Cyrl->sr-Cyrl-CS.  See http://blogs.msdn.com/kierans/archive/2006/08/02/687267.aspx
    /// and http://blogs.msdn.com/shawnste/archive/2006/11/14/problems-compiling-resources-in-net-2-0-apps-after-updates.aspx for
    /// additional information.</remarks>
    /// <author>Mark Pollack</author>
    public class CultureInfoUtils
    {
        private static readonly string srLatn = "sr-SP-Latn";
        private static readonly string srCyrl = "sr-SP-Cyrl";

        static CultureInfoUtils()
        {
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                if (ci.Name.Equals("sr-Latn-CS"))
                {
                    srLatn = "sr-Latn-CS";
                    srCyrl = "sr-Cyrl-CS";
                    break;
                }
            }
        }

        public static string SerbianCyrillicCultureName
        {
            get { return srCyrl; }
        }

        public static string SerbianLatinCultureName
        {
            get { return srLatn; }
        }
    }
}
