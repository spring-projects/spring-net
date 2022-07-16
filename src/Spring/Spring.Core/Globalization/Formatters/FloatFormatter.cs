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
	/// format and parse floating point numbers.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This formatter allows you to format and parse numbers that conform
	/// to <see cref="NumberStyles.Float"/> number style (leading and trailing
	/// white space, leading sign, decimal point, exponent).
	/// </para>
	/// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class FloatFormatter : IFormatter
	{
        /// <summary>
        /// Default format string.
        /// </summary>
        public const string DefaultFormat = "{0:F}";

        private string format;
		private NumberFormatInfo numberInfo;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatFormatter"/> class,
        /// using default format string of '{0:F}' and current thread's culture.
        /// </summary>
        public FloatFormatter() : this(DefaultFormat, CultureInfo.CurrentCulture)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatFormatter"/> class,
        /// using specified format string and current thread's culture.
        /// </summary>
        /// <param name="format">The format string.</param>
        public FloatFormatter(string format) : this(format, CultureInfo.CurrentCulture)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatFormatter"/> class,
        /// using default format string of '{0:F}' and specified culture.
        /// </summary>
        /// <param name="culture">The culture.</param>
        public FloatFormatter(CultureInfo culture) : this(DefaultFormat, culture)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatFormatter"/> class,
        /// using specified format string and current thread's culture.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="cultureName">The culture name.</param>
        public FloatFormatter(string format, string cultureName) : this(format, CultureInfo.CreateSpecificCulture(cultureName))
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatFormatter"/> class,
        /// using specified format string and culture.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="culture">The culture.</param>
        public FloatFormatter(string format, CultureInfo culture)
        {
            this.format = format;
            this.numberInfo = culture.NumberFormat;
        }

	    #endregion

        /// <summary>
        /// Formats the specified float value.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>Formatted floating point number.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="value"/> is not a number.</exception>
        public string Format(object value)
	    {
            AssertUtils.ArgumentNotNull(value, "value");
            if (!NumberUtils.IsNumber(value))
            {
                throw new ArgumentException("FloatFormatter can only be used to format numbers.");
            }

	        return String.Format(numberInfo, format, value);
	    }

        /// <summary>
        /// Parses the specified float value.
        /// </summary>
        /// <param name="value">The float value to parse.</param>
        /// <returns>Parsed float value as a <see cref="Double"/>.</returns>
        public object Parse(string value)
	    {
            if (!StringUtils.HasText(value))
            {
                return 0d;
            }
	        return Double.Parse(value, NumberStyles.Float, numberInfo);
	    }
	}
}
