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

namespace Spring.Globalization.Formatters
{
    /// <summary>
    /// Provides base functionality for filtering values before they actually get parsed/formatted.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public abstract class FilteringFormatter : IFormatter
    {
        private readonly IFormatter _underlyingFormatter;

        ///<summary>
        /// Creates a new instance of this FilteringFormatter.
        ///</summary>
        ///<param name="underlyingFormatter">an optional underlying formatter</param>
        /// <remarks>
        /// If no underlying formatter is specified, the values
        /// get passed through "as-is" after being filtered
        /// </remarks>
        public FilteringFormatter(IFormatter underlyingFormatter)
        {
            _underlyingFormatter = underlyingFormatter;
        }

        /// <summary>
        /// Parses the specified value.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>Parsed <paramref name="value"/>.</returns>
        public object Parse(string value)
        {
            value = FilterValueToParse(value);

            if (_underlyingFormatter != null)
            {
                return _underlyingFormatter.Parse(value);
            }

            return value;
        }

        /// <summary>
        /// Formats the specified value.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>Formatted <paramref name="value"/>.</returns>
        public string Format(object value)
        {
            value = FilterValueToFormat(value);

            if (_underlyingFormatter != null)
            {
                return _underlyingFormatter.Format(value);
            }

            return (value != null) ? value.ToString() : null;
        }

        /// <summary>
        /// Allows to rewrite a value before it gets parsed by the underlying formatter
        /// </summary>
        protected virtual string FilterValueToParse(string value)
        {
            return value;
        }

        /// <summary>
        /// Allows to change a value before it gets formatted by the underlying formatter
        /// </summary>
        protected virtual object FilterValueToFormat(object value)
        {
            return value;
        }
    }
}
