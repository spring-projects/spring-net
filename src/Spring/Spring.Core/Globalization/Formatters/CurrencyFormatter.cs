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
	/// format and parse currency values.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>CurrencyFormatter</c> uses currency related properties of the
	/// <see cref="NumberFormatInfo"/> to format and parse currency values.
	/// </para>
	/// <para>
	/// If you use one of the constructors that accept culture as a parameter
	/// to create an instance of <c>CurrencyFormatter</c>, default <c>NumberFormatInfo</c>
	/// for the specified culture will be used.
	/// </para>
	/// <para>
	/// You can also use properties exposed by the <c>CurrencyFormatter</c> in order
	/// to override some of the default currency formatting parameters.
	/// </para>
	/// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class CurrencyFormatter : IFormatter
	{
        private NumberFormatInfo formatInfo;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyFormatter"/> class
        /// using default <see cref="NumberFormatInfo"/> for the current thread's culture.
        /// </summary>
        public CurrencyFormatter() : this(CultureInfo.CurrentCulture)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyFormatter"/> class
        /// using default <see cref="NumberFormatInfo"/> for the specified culture.
        /// </summary>
        /// <param name="cultureName">The culture name.</param>
        public CurrencyFormatter(string cultureName) : this(CultureInfo.CreateSpecificCulture(cultureName))
        {}

        /// <summary>
	    /// Initializes a new instance of the <see cref="CurrencyFormatter"/> class
	    /// using default <see cref="NumberFormatInfo"/> for the specified culture.
	    /// </summary>
	    /// <param name="culture">The culture.</param>
	    public CurrencyFormatter(CultureInfo culture)
	    {
	        formatInfo = culture.NumberFormat;
	    }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyFormatter"/> class
        /// using specified <see cref="NumberFormatInfo"/>.
        /// </summary>
        /// <param name="formatInfo">
        /// The <see cref="NumberFormatInfo"/> instance that defines how
        /// currency values are formatted.
        /// </param>
        public CurrencyFormatter(NumberFormatInfo formatInfo)
        {
            this.formatInfo = formatInfo;
        }

	    #endregion

	    #region Properties

	    /// <summary>
	    /// Gets or sets the currency decimal digits.
	    /// </summary>
	    /// <value>The currency decimal digits.</value>
        /// <seealso cref="NumberFormatInfo.CurrencyDecimalDigits"/>
        public int DecimalDigits
	    {
	        get { return formatInfo.CurrencyDecimalDigits; }
	        set { formatInfo.CurrencyDecimalDigits = value; }
	    }

	    /// <summary>
	    /// Gets or sets the currency decimal separator.
        /// </summary>
	    /// <value>The currency decimal separator.</value>
        /// <seealso cref="NumberFormatInfo.CurrencyDecimalSeparator"/>
        public string DecimalSeparator
	    {
	        get { return formatInfo.CurrencyDecimalSeparator; }
	        set { formatInfo.CurrencyDecimalSeparator = value; }
	    }

	    /// <summary>
	    /// Gets or sets the currency group sizes.
        /// </summary>
	    /// <value>The currency group sizes.</value>
        /// <seealso cref="NumberFormatInfo.CurrencyGroupSizes"/>
        public int[] GroupSizes
	    {
	        get { return formatInfo.CurrencyGroupSizes; }
	        set { formatInfo.CurrencyGroupSizes = value; }
	    }

	    /// <summary>
	    /// Gets or sets the currency group separator.
        /// </summary>
	    /// <value>The currency group separator.</value>
        /// <seealso cref="NumberFormatInfo.CurrencyGroupSeparator"/>
        public string GroupSeparator
	    {
	        get { return formatInfo.CurrencyGroupSeparator; }
	        set { formatInfo.CurrencyGroupSeparator = value; }
	    }

	    /// <summary>
	    /// Gets or sets the currency symbol.
        /// </summary>
	    /// <value>The currency symbol.</value>
        /// <seealso cref="NumberFormatInfo.CurrencySymbol"/>
        public string CurrencySymbol
	    {
	        get { return formatInfo.CurrencySymbol; }
	        set { formatInfo.CurrencySymbol = value; }
	    }

	    /// <summary>
	    /// Gets or sets the currency negative pattern.
        /// </summary>
	    /// <value>The currency negative pattern.</value>
        /// <seealso cref="NumberFormatInfo.CurrencyNegativePattern"/>
        public int NegativePattern
	    {
	        get { return formatInfo.CurrencyNegativePattern; }
	        set { formatInfo.CurrencyNegativePattern = value; }
	    }

	    /// <summary>
	    /// Gets or sets the currency positive pattern.
        /// </summary>
	    /// <value>The currency positive pattern.</value>
        /// <seealso cref="NumberFormatInfo.CurrencyPositivePattern"/>
        public int PositivePattern
	    {
	        get { return formatInfo.CurrencyPositivePattern; }
	        set { formatInfo.CurrencyPositivePattern = value; }
	    }

	    #endregion

	    /// <summary>
	    /// Formats the specified currency value.
	    /// </summary>
	    /// <param name="value">The value to format.</param>
	    /// <returns>Formatted currency <paramref name="value"/>.</returns>
	    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <c>null</c>.</exception>
	    /// <exception cref="ArgumentException">If <paramref name="value"/> is not a number.</exception>
	    public string Format(object value)
	    {
            AssertUtils.ArgumentNotNull(value, "value");
            if (!NumberUtils.IsNumber(value))
            {
                throw new ArgumentException("CurrencyFormatter can only be used to format numbers.");
            }

	        return String.Format(formatInfo, "{0:C}", value);
	    }

	    /// <summary>
	    /// Parses the specified currency value.
	    /// </summary>
	    /// <param name="value">The currency value to parse.</param>
	    /// <returns>Parsed currency value as a <see cref="Double"/>.</returns>
        public object Parse(string value)
	    {
            if (!StringUtils.HasText(value))
            {
                return 0d;
            }

            return Double.Parse(value, NumberStyles.Currency, formatInfo);
	    }
	}
}
