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

using Spring.Util;

namespace Spring.Core
{
    /// <summary>
    /// Provides methods to support various naming and other conventions used throughout the framework.
    /// Mainly for internal use within the framework.
    /// </summary>
    /// <author>Rob Harrop</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public sealed class Conventions
    {
        /// <summary> Convert <code>String</code>s in attribute name format (lowercase, hyphens separating words)
        /// into property name format (camel-cased). For example, <code>transaction-manager</code> is
        /// converted into <code>transactionManager</code>.
        /// </summary>
        public static string AttributeNameToPropertyName(string attributeName)
        {
            AssertUtils.ArgumentNotNull(attributeName, "attributeName");
            if (attributeName.IndexOf("-") == -1)
            {
                return attributeName;
            }
            char[] chars = attributeName.ToCharArray();
            char[] result = new char[chars.Length - 1]; // not completely accurate but good guess
            int currPos = 0;
            bool upperCaseNext = false;
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (c == '-')
                {
                    upperCaseNext = true;
                }
                else if (upperCaseNext)
                {
                    result[currPos++] = Char.ToUpper(c);
                    upperCaseNext = false;
                }
                else
                {
                    result[currPos++] = c;
                }
            }
            return new String(result, 0, currPos);
        }
    }
}
