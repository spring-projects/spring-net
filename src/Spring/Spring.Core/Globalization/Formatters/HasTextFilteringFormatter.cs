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

using Spring.Util;

namespace Spring.Globalization.Formatters
{
    /// <summary>
    /// Replaces input strings with a given default value,
    /// if they are null or contain whitespaces only,
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class HasTextFilteringFormatter : FilteringFormatter
    {
        private readonly string _defaultValue;

        ///<summary>
        /// Creates a new instance of this HasTextFilteringFormatter using null as default value.
        ///</summary>
        ///<param name="underlyingFormatter">an optional underlying formatter</param>
        /// <remarks>
        /// If no underlying formatter is specified, the values
        /// get passed through "as-is" after being filtered
        /// </remarks>
        public HasTextFilteringFormatter(IFormatter underlyingFormatter)
            : this(null, underlyingFormatter)
        {
        }

        ///<summary>
        /// Creates a new instance of this HasTextFilteringFormatter.
        ///</summary>
        /// <param name="defaultValue">the default value to be returned, if input text doesn't contain text</param>
        ///<param name="underlyingFormatter">an optional underlying formatter</param>
        /// <remarks>
        /// If no underlying formatter is specified, the values
        /// get passed through "as-is" after being filtered
        /// </remarks>
        public HasTextFilteringFormatter(string defaultValue, IFormatter underlyingFormatter)
            : base(underlyingFormatter)
        {
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// If value contains no text, it will be replaced by a defaultValue.
        /// </summary>
        protected override string FilterValueToParse(string value)
        {
            if (!StringUtils.HasText(value))
            {
                value = _defaultValue;
            }
            return value;
        }
    }
}
