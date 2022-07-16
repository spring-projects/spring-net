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

using Spring.Util;

namespace Spring.Globalization.Formatters
{
	/// <summary>
	/// Implementation of <see cref="IFormatter"/> that can be used to
	/// format and parse integer numbers.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This formatter allows you to format and parse numbers that conform
	/// to <see cref="NumberStyles.Integer"/> number style (leading and trailing
	/// white space, leading sign).
	/// </para>
	/// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class IntegerFormatter : IFormatter
	{
        private string format = "{0:D}";

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerFormatter"/> class,
        /// using default format string of '{0:D}'.
        /// </summary>
        public IntegerFormatter()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerFormatter"/> class,
        /// using specified format string.
        /// </summary>
        public IntegerFormatter(string format)
        {
            this.format = format;
        }

        #endregion

        /// <summary>
        /// Formats the specified integer value.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>Formatted integer number.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="value"/> is not an integer number.</exception>
        public string Format(object value)
	    {
            AssertUtils.ArgumentNotNull(value, "value");
            if (!NumberUtils.IsInteger(value))
            {
                throw new ArgumentException("IntegerFormatter can only be used to format integer numbers.");
            }

	        return String.Format(format, value);
	    }

        /// <summary>
        /// Parses the specified integer value.
        /// </summary>
        /// <param name="value">The integer value to parse.</param>
        /// <returns>Parsed number value as a <see cref="Int32"/>.</returns>
        public object Parse(string value)
	    {
            if (!StringUtils.HasText(value))
            {
                return 0;
            }
	        return Int32.Parse(value, NumberStyles.Integer);
	    }
	}
}
