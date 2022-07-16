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
	/// format and parse <see cref="DateTime"/> values.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>DateTimeFormatter</c> uses properties of the
	/// <see cref="DateTimeFormatInfo"/> to format and parse <see cref="DateTime"/> values.
	/// </para>
	/// <para>
	/// If you use one of the constructors that accept culture as a parameter
	/// to create an instance of <c>DateTimeFormatter</c>, default <c>DateTimeFormatInfo</c>
	/// for the specified culture will be used.
	/// </para>
	/// <para>
	/// You can also use properties exposed by the <c>DateTimeFormatter</c> in order
	/// to override some of the default formatting parameters.
	/// </para>
	/// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class DateTimeFormatter : IFormatter
	{
        private DateTimeFormatInfo formatInfo;
        private string format;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeFormatter"/> class
        /// using default <see cref="DateTimeFormatInfo"/> for the current thread's culture.
        /// </summary>
        /// <param name="format">Date/time format string.</param>
        public DateTimeFormatter(string format) : this(format, CultureInfo.CurrentCulture)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeFormatter"/> class
        /// using default <see cref="DateTimeFormatInfo"/> for the specified culture.
        /// </summary>
        /// <param name="format">Date/time format string.</param>
        /// <param name="cultureName">The culture name.</param>
        public DateTimeFormatter(string format, string cultureName) : this(format, CultureInfo.CreateSpecificCulture(cultureName))
        {}

        /// <summary>
	    /// Initializes a new instance of the <see cref="DateTimeFormatter"/> class
	    /// using default <see cref="DateTimeFormatInfo"/> for the specified culture.
	    /// </summary>
	    /// <param name="format">Date/time format string.</param>
	    /// <param name="culture">The culture.</param>
	    public DateTimeFormatter(string format, CultureInfo culture)
	    {
            this.format = format;
	        this.formatInfo = culture.DateTimeFormat;
	    }

	    #endregion

	    /// <summary>
	    /// Formats the specified <see cref="DateTime"/> value.
	    /// </summary>
	    /// <param name="value">The value to format.</param>
	    /// <returns>Formatted <see cref="DateTime"/> value.</returns>
	    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <c>null</c>.</exception>
	    /// <exception cref="ArgumentException">If <paramref name="value"/> is not an instance of <see cref="DateTime"/>.</exception>
	    public string Format(object value)
	    {
            AssertUtils.ArgumentNotNull(value, "value");
            if (!(value is DateTime))
            {
                throw new ArgumentException("DateTimeFormatter can only be used to format instances of [System.DateTime].");
            }

	        return ((DateTime) value).ToString(format, formatInfo);
	    }

	    /// <summary>
	    /// Parses the specified value.
	    /// </summary>
	    /// <param name="value">The string to parse.</param>
	    /// <returns>Parsed <see cref="DateTime"/> value.</returns>
        public object Parse(string value)
	    {
            if (!StringUtils.HasText(value))
            {
                return DateTime.MinValue;
            }

            return DateTime.ParseExact(value, format, formatInfo, DateTimeStyles.AllowWhiteSpaces);
	    }
	}
}
