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
	/// format and parse numbers.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>NumberFormatter</c> uses number-related properties of the
	/// <see cref="NumberFormatInfo"/> to format and parse numbers.
	/// </para>
	/// <para>
	/// This formatter works with both integer and decimal numbers and allows
	/// you to format and parse numbers that conform to <see cref="NumberStyles.Number"/>
	/// number style (leading and trailing white space and/or sign, thousands separator,
	/// decimal point)
	/// </para>
	/// <para>
	/// If you use one of the constructors that accept culture as a parameter
	/// to create an instance of <c>NumberFormatter</c>, default <c>NumberFormatInfo</c>
	/// for the specified culture will be used.
	/// </para>
	/// <para>
	/// You can also use properties exposed by the <c>NumberFormatter</c> in order
	/// to override some of the default number formatting parameters.
	/// </para>
	/// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class NumberFormatter : IFormatter
	{
        private NumberFormatInfo formatInfo;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberFormatter"/> class
        /// using default <see cref="NumberFormatInfo"/> for the current thread's culture.
        /// </summary>
        public NumberFormatter() : this(CultureInfo.CurrentCulture)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberFormatter"/> class
        /// using default <see cref="NumberFormatInfo"/> for the specified culture.
        /// </summary>
        /// <param name="cultureName">The culture name.</param>
        public NumberFormatter(string cultureName) : this(CultureInfo.CreateSpecificCulture(cultureName))
        {}

        /// <summary>
	    /// Initializes a new instance of the <see cref="NumberFormatter"/> class
	    /// using default <see cref="NumberFormatInfo"/> for the specified culture.
	    /// </summary>
	    /// <param name="culture">The culture.</param>
	    public NumberFormatter(CultureInfo culture)
	    {
	        formatInfo = culture.NumberFormat;
	    }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberFormatter"/> class
        /// using specified <see cref="NumberFormatInfo"/>.
        /// </summary>
        /// <param name="formatInfo">
        /// The <see cref="NumberFormatInfo"/> instance that defines how
        /// numbers are formatted and parsed.
        /// </param>
        public NumberFormatter(NumberFormatInfo formatInfo)
        {
            this.formatInfo = formatInfo;
        }

	    #endregion

	    #region Properties

	    /// <summary>
	    /// Gets or sets the number of decimal digits.
	    /// </summary>
	    /// <value>The number of decimal digits.</value>
        /// <seealso cref="NumberFormatInfo.NumberDecimalDigits"/>
        public int DecimalDigits
	    {
	        get { return formatInfo.NumberDecimalDigits; }
	        set { formatInfo.NumberDecimalDigits = value; }
	    }

	    /// <summary>
	    /// Gets or sets the decimal separator.
        /// </summary>
	    /// <value>The decimal separator.</value>
        /// <seealso cref="NumberFormatInfo.NumberDecimalSeparator"/>
        public string DecimalSeparator
	    {
	        get { return formatInfo.NumberDecimalSeparator; }
	        set { formatInfo.NumberDecimalSeparator = value; }
	    }

	    /// <summary>
	    /// Gets or sets the number group sizes.
        /// </summary>
	    /// <value>The number group sizes.</value>
        /// <seealso cref="NumberFormatInfo.NumberGroupSizes"/>
        public int[] GroupSizes
	    {
	        get { return formatInfo.NumberGroupSizes; }
	        set { formatInfo.NumberGroupSizes = value; }
	    }

	    /// <summary>
	    /// Gets or sets the number group separator.
        /// </summary>
	    /// <value>The number group separator.</value>
        /// <seealso cref="NumberFormatInfo.NumberGroupSeparator"/>
        public string GroupSeparator
	    {
	        get { return formatInfo.NumberGroupSeparator; }
	        set { formatInfo.NumberGroupSeparator = value; }
	    }

	    /// <summary>
	    /// Gets or sets the negative pattern.
        /// </summary>
	    /// <value>The number negative pattern.</value>
        /// <seealso cref="NumberFormatInfo.NumberNegativePattern"/>
        public int NegativePattern
	    {
	        get { return formatInfo.NumberNegativePattern; }
	        set { formatInfo.NumberNegativePattern = value; }
	    }

	    #endregion

        /// <summary>
        /// Formats the specified number value.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>Formatted number <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="value"/> is not a number.</exception>
        public string Format(object value)
	    {
            AssertUtils.ArgumentNotNull(value, "value");
            if (!NumberUtils.IsNumber(value))
            {
                throw new ArgumentException("NumberFormatter can only be used to format numbers.");
            }

	        return String.Format(formatInfo, "{0:N}", value);
	    }

        /// <summary>
        /// Parses the specified number value.
        /// </summary>
        /// <param name="value">The number value to parse.</param>
        /// <returns>Parsed number value as a <see cref="Double"/>.</returns>
        public object Parse(string value)
	    {
            if (!StringUtils.HasText(value))
            {
                return 0d;
            }

            NumberStyles numberStyle = NumberStyles.Number;
            if (formatInfo.NumberNegativePattern == 0)
            {
                numberStyle |= NumberStyles.AllowParentheses;
            }

	        return Double.Parse(value, numberStyle, formatInfo);
	    }
	}
}
