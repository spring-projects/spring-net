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
	/// <c>PercentFormatter</c> uses percent-related properties of the
	/// <see cref="NumberFormatInfo"/> to format and parse percentages.
	/// </para>
	/// <para>
	/// If you use one of the constructors that accept culture as a parameter
	/// to create an instance of <c>PercentFormatter</c>, default <c>NumberFormatInfo</c>
	/// for the specified culture will be used.
	/// </para>
	/// <para>
	/// You can also use properties exposed by the <c>PercentFormatter</c> in order
	/// to override some of the default number formatting parameters.
	/// </para>
	/// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class PercentFormatter : IFormatter
	{
        private static int[] positivePatterns = new int[] {3, 1, 0};
        private static int[] negativePatterns = new int[] {8, 5, 1};

        private NumberFormatInfo formatInfo;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PercentFormatter"/> class
        /// using default <see cref="NumberFormatInfo"/> for the current thread's culture.
        /// </summary>
        public PercentFormatter() : this(CultureInfo.CurrentCulture)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PercentFormatter"/> class
        /// using default <see cref="NumberFormatInfo"/> for the specified culture.
        /// </summary>
        /// <param name="cultureName">The culture name.</param>
        public PercentFormatter(string cultureName) : this(CultureInfo.CreateSpecificCulture(cultureName))
        {}

        /// <summary>
	    /// Initializes a new instance of the <see cref="PercentFormatter"/> class
	    /// using default <see cref="NumberFormatInfo"/> for the specified culture.
	    /// </summary>
	    /// <param name="culture">The culture.</param>
	    public PercentFormatter(CultureInfo culture)
	    {
	        formatInfo = culture.NumberFormat;
	    }

        /// <summary>
        /// Initializes a new instance of the <see cref="PercentFormatter"/> class
        /// using specified <see cref="NumberFormatInfo"/>.
        /// </summary>
        /// <param name="formatInfo">
        /// The <see cref="NumberFormatInfo"/> instance that defines how
        /// numbers are formatted and parsed.
        /// </param>
        public PercentFormatter(NumberFormatInfo formatInfo)
        {
            this.formatInfo = formatInfo;
        }

	    #endregion

	    #region Properties

	    /// <summary>
	    /// Gets or sets the number of decimal digits.
	    /// </summary>
	    /// <value>The number of decimal digits.</value>
        /// <seealso cref="NumberFormatInfo.PercentDecimalDigits"/>
        public int DecimalDigits
	    {
	        get { return formatInfo.PercentDecimalDigits; }
	        set { formatInfo.PercentDecimalDigits = value; }
	    }

	    /// <summary>
	    /// Gets or sets the decimal separator.
        /// </summary>
	    /// <value>The decimal separator.</value>
        /// <seealso cref="NumberFormatInfo.PercentDecimalSeparator"/>
        public string DecimalSeparator
	    {
	        get { return formatInfo.PercentDecimalSeparator; }
	        set { formatInfo.PercentDecimalSeparator = value; }
	    }

	    /// <summary>
	    /// Gets or sets the percent group sizes.
        /// </summary>
	    /// <value>The percent group sizes.</value>
        /// <seealso cref="NumberFormatInfo.PercentGroupSizes"/>
        public int[] GroupSizes
	    {
	        get { return formatInfo.PercentGroupSizes; }
	        set { formatInfo.PercentGroupSizes = value; }
	    }

	    /// <summary>
	    /// Gets or sets the percent group separator.
        /// </summary>
	    /// <value>The percent group separator.</value>
        /// <seealso cref="NumberFormatInfo.PercentGroupSeparator"/>
        public string GroupSeparator
	    {
	        get { return formatInfo.PercentGroupSeparator; }
	        set { formatInfo.PercentGroupSeparator = value; }
	    }

	    /// <summary>
	    /// Gets or sets the negative pattern.
        /// </summary>
	    /// <value>The percent negative pattern.</value>
        /// <seealso cref="NumberFormatInfo.PercentNegativePattern"/>
        public int NegativePattern
	    {
	        get { return formatInfo.PercentNegativePattern; }
	        set { formatInfo.PercentNegativePattern = value; }
	    }

        /// <summary>
        /// Gets or sets the positive pattern.
        /// </summary>
        /// <value>The percent positive pattern.</value>
        /// <seealso cref="NumberFormatInfo.PercentPositivePattern"/>
        public int PositivePattern
        {
            get { return formatInfo.PercentPositivePattern; }
            set { formatInfo.PercentPositivePattern = value; }
        }

        /// <summary>
        /// Gets or sets the percent symbol.
        /// </summary>
        /// <value>The percent symbol.</value>
        /// <seealso cref="NumberFormatInfo.PercentSymbol"/>
        public string PercentSymbol
	    {
	        get { return formatInfo.PercentSymbol; }
	        set { formatInfo.PercentSymbol = value; }
	    }

        /// <summary>
        /// Gets or sets the per mille symbol.
        /// </summary>
        /// <value>The per mille symbol.</value>
        /// <seealso cref="NumberFormatInfo.PerMilleSymbol"/>
        public string PerMilleSymbol
	    {
	        get { return formatInfo.PerMilleSymbol; }
	        set { formatInfo.PerMilleSymbol = value; }
	    }

	    #endregion

        /// <summary>
        /// Formats the specified percentage value.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>Formatted percentage.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="value"/> is not a number.</exception>
        public string Format(object value)
	    {
            AssertUtils.ArgumentNotNull(value, "value");
            if (!NumberUtils.IsNumber(value))
            {
                throw new ArgumentException("PercentFormatter can only be used to format numbers.");
            }

	        return String.Format(formatInfo, "{0:P}", value);
	    }

        /// <summary>
        /// Parses the specified percentage value.
        /// </summary>
        /// <param name="value">The percentage value to parse.</param>
        /// <returns>Parsed percentage value as a <see cref="Double"/>.</returns>
        public object Parse(string value)
	    {
            if (!StringUtils.HasText(value))
            {
                return 0d;
            }

            // there is no percentage parser in .NET, so we use currency parser to achieve the goal
            NumberFormatInfo fi = (NumberFormatInfo) formatInfo.Clone();
            fi.CurrencyDecimalDigits = formatInfo.PercentDecimalDigits;
            fi.CurrencyDecimalSeparator = formatInfo.PercentDecimalSeparator;
            fi.CurrencyGroupSeparator = formatInfo.PercentGroupSeparator;
            fi.CurrencyGroupSizes = formatInfo.PercentGroupSizes;
            fi.CurrencyNegativePattern = negativePatterns[formatInfo.PercentNegativePattern];
            fi.CurrencyPositivePattern = positivePatterns[formatInfo.PercentPositivePattern];
            fi.CurrencySymbol = formatInfo.PercentSymbol;

	        return Double.Parse(value, NumberStyles.Currency, fi) / 100;
	    }
	}
}
