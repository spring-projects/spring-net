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

#endregion

namespace Spring.Core.TypeConversion
{
	/// <summary>
	/// Converter for <see cref="System.Uri"/> instances.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Mark Pollack (.NET)</author>
	public class UriConverter : TypeConverter
	{
        #region Constructor (s) / Destructor
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Core.TypeConversion.UriConverter"/> class.
        /// </summary>
        public UriConverter () {}
        #endregion

        #region Methods
        /// <summary>
        /// Returns whether this converter can convert an object of one
        /// <see cref="System.Type"/> to a <see cref="System.Uri"/>
        /// </summary>
        /// <remarks>
        /// <p>
        /// Currently only supports conversion from a
        /// <see cref="System.String"/> instance.
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
        /// <returns>True if the conversion is possible.</returns>
        public override bool CanConvertFrom (
            ITypeDescriptorContext context,
            Type sourceType)
        {
            if (sourceType == typeof (string))
            {
                return true;
            }
            return base.CanConvertFrom (context, sourceType);
        }

        /// <summary>
        /// Convert from a string value to a <see cref="System.Uri"/> instance.
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
        /// A <see cref="System.Uri"/> if successful.
        /// </returns>
        public override object ConvertFrom (
            ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    return new Uri(value as string);
                }
                catch (UriFormatException ex)
                {
                    throw new ArgumentException("Malformed URL: ", ex);
                }
            }
            return base.ConvertFrom (context, culture, value);
        }
        #endregion
	}
}
