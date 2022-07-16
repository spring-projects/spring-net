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

#region Imports

using System.ComponentModel;
using System.Globalization;

using Spring.Util;

#endregion

namespace Spring.Core.TypeConversion
{
	/// <summary>
	/// Converts a separated <see cref="System.String"/> to a <see cref="System.String"/>
	/// array.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Defaults to using the <c>,</c> (comma) as the list separator. Note that the value
	/// of the current <see cref="System.Globalization.CultureInfo.CurrentCulture"/> is
	/// <b>not</b> used.
	/// </p>
	/// <p>
	/// If you want to provide your own list separator, you can set the value of the
    /// <see cref="Spring.Core.TypeConversion.StringArrayConverter.ListSeparator"/>
	/// property to the value that you want. Please note that this value will be used
	/// for <i>all</i> future conversions in preference to the default list separator.
	/// </p>
	/// <p>
	/// Please note that the individual elements of a string will be passed
	/// through <i>as is</i> (i.e. no conversion or trimming of surrounding
	/// whitespace will be performed).
	/// </p>
	/// <p>
	/// This <see cref="System.ComponentModel.TypeConverter"/> should be
	/// automatically registered with any <see cref="Spring.Objects.IObjectWrapper"/>
	/// implementations.
	/// </p>
	/// </remarks>
	/// <example>
	/// <code language="C#">
	/// public class StringArrayConverterExample 
	/// {     
	///     public static void Main()
	///     {
	///         StringArrayConverter converter = new StringArrayConverter();
	///			
	///			string csvWords = "This,Is,It";
	///			string[] frankBoothWords = converter.ConvertFrom(csvWords);
	///
	///			// the 'frankBoothWords' array will have 3 elements, namely
	///			// "This", "Is", "It".
	///			
	///			// please note that extraneous whitespace is NOT trimmed off
	///			// in the current implementation...
	///			string csv = "  Cogito ,ergo ,sum ";
	///			string[] descartesWords = converter.ConvertFrom(csv);
	///			
	///			// the 'descartesWords' array will have 3 elements, namely
	///			// "  Cogito ", "ergo ", "sum ".
	///			// notice how the whitespace has NOT been trimmed.
	///     }
	/// }
	/// </code>
	/// </example>
	/// <author></author>
	public class StringArrayConverter : TypeConverter
	{
		private const string DefaultListSeparator = ",";

		private string listSeparator = DefaultListSeparator;

		/// <summary>
		/// The value that will be used as the list separator when performing
		/// conversions.
		/// </summary>
		/// <value>
		/// A 'single' string character that will be used as the list separator
		/// when performing conversions.
		/// </value>
		/// <exception cref="System.ArgumentException">
		/// If the supplied value is not <cref lang="null"/> and is an empty
		/// string, or has more than one character.
		/// </exception>
		public string ListSeparator
		{
			get { return this.listSeparator; }
			set
			{
				if (value != null)
				{
					if (value.Length != 1)
					{
						throw new ArgumentException(
							"The 'ListSeparator' must be exactly one character in length.");
					}
					listSeparator = value;
				}
				else
				{
					listSeparator = DefaultListSeparator;
				}
			}
		}

		/// <summary>
		/// Can we convert from a the sourcetype to a <see cref="System.String"/> array?
		/// </summary>
		/// <remarks>
		/// <p>
		/// Currently only supports conversion from a <see cref="System.String"/> instance.
		/// </p>
		/// </remarks>
		/// <param name="context">
		/// A <see cref="System.ComponentModel.ITypeDescriptorContext"/>
		/// that provides a format context.
		/// </param>
		/// <param name="sourceType">
		/// A <see cref="System.Type"/> that represents the
		/// <see cref="System.Type"/> you want to convert from.
		/// </param>
		/// <returns><see langword="true"/> if the conversion is possible.</returns>
		public override bool CanConvertFrom(
			ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		/// <summary>
		/// Convert from a <see cref="System.String"/> value to a
		/// <see cref="System.String"/> array.
		/// </summary>
		/// <param name="context">
		/// A <see cref="System.ComponentModel.ITypeDescriptorContext"/>
		/// that provides a format context.
		/// </param>
		/// <param name="culture">
		/// The <see cref="System.Globalization.CultureInfo"/> to use
		/// as the current culture.
		/// </param>
		/// <param name="value">
		/// The value that is to be converted.
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> array if successful.
		/// </returns>        
		public override object ConvertFrom(
			ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			string values = value as string;
			if (values != null)
			{
				return StringUtils.DelimitedListToStringArray(values, this.ListSeparator);
			}
			return base.ConvertFrom(context, culture, value);
		}
	}
}
