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
using System.Text.RegularExpressions;

#endregion

namespace Spring.Core.TypeConversion
{
    /// <summary>
    /// Converts string representation of a regular expression into an instance of <see cref="Regex"/>.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class RegexConverter : TypeConverter
    {
        /// <summary>
        /// Can we convert from the sourcetype to a <see cref="Regex"/>?
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
            return (sourceType == typeof(string));
        }

        /// <summary>
        /// Convert from a <see cref="System.String"/> value to an
        /// <see cref="Regex"/> instance.
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
        /// A <see cref="Regex"/> if successful.
        /// </returns>        
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return new Regex(value as string);
            }
            else
            {
                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}
