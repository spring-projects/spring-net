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

#region Importsusing System;

using System.ComponentModel;
using System.Globalization;
using Spring.Util;

#endregion

namespace Spring.Core.TypeConversion
{
    /// <summary>
    /// Converts between instances of <see cref="UniqueKey"/> and their string representations.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class UniqueKeyConverter : TypeConverter
    {
        /// <summary>
        /// Can we convert from the sourcetype to a <see cref="UniqueKey"/>?
        /// </summary>
        /// <remarks>
        /// <p>
        /// Currently only supports conversion from a <see cref="System.String"/> instance.
        /// </p>
        /// </remarks>
        /// <param name="context">
        /// A <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="sourceType">
        /// A <see cref="System.Type"/> that represents the <see cref="System.Type"/> you want to convert from.
        /// </param>
        /// <returns><see langword="true"/> if the conversion is possible.</returns>
        public override bool CanConvertFrom(
            ITypeDescriptorContext context, Type sourceType)
        {
            return (typeof(string)==sourceType || typeof(UniqueKey)==sourceType);
        }

        /// <summary>
        /// Convert from a <see cref="System.String"/> value to an <see cref="UniqueKey"/> instance.
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
        /// A <see cref="UniqueKey"/> if successful, <see langword="null"/> otherwise.
        /// </returns>
        ///<exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null) return null;

            if (!CanConvertFrom(context, value.GetType()))
            {
                throw new NotSupportedException(string.Format("Conversion from value of type '{0}' is not supported", value.GetType().FullName));
            }

            if (value is string)
            {
                return new UniqueKey(value as string);
            }
            else if (value is UniqueKey)
            {
                return value;
            }

            throw new NotSupportedException(string.Format("Cannot convert from value of type '{0}' to '{1}'", value.GetType().FullName, typeof(UniqueKey).FullName));
        }

        ///<summary>
        ///Returns whether this converter can convert the object to the specified type, using the specified context.
        ///</summary>
        ///<param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context. </param>
        ///<param name="destinationType">A <see cref="T:System.Type"></see> that represents the type you want to convert to. </param>
        ///<returns>
        ///true if this converter can perform the conversion; otherwise, false.
        ///</returns>
        /// <remarks>
        /// At the moment only conversion to string is supported.
        /// </remarks>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (typeof(string)==destinationType || typeof(UniqueKey)==destinationType);
        }

        ///<summary>
        ///Converts the given value object to the specified type, using the specified context and culture information.
        ///</summary>
        ///
        ///<returns>
        ///An <see cref="T:System.Object"></see> that represents the converted value.
        ///</returns>
        ///
        ///<param name="culture">A <see cref="T:System.Globalization.CultureInfo"></see>. If null is passed, the current culture is assumed. </param>
        ///<param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context. </param>
        ///<param name="destinationType">The <see cref="T:System.Type"></see> to convert the value parameter to. </param>
        ///<param name="value">The <see cref="T:System.Object"></see> to convert. </param>
        ///<exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
        ///<exception cref="T:System.ArgumentNullException">The destinationType parameter is null. </exception>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            // check destinationType
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType", "destinationType must not be null");
            }
            if (!CanConvertTo(context, destinationType))
            {
                throw new NotSupportedException(string.Format("conversion to destinationType '{0}' is not supported.", destinationType.FullName));
            }

            // value must either be null or a UniqueKey
            if ( (value != null) && (typeof(UniqueKey) != value.GetType()) )
            {
                throw new NotSupportedException(string.Format("value is not of type '{0}'", typeof(UniqueKey).FullName));
            }

            if (value == null) return null;

            if (typeof(string)==destinationType)
            {
                return (value).ToString();
            }
            else if (typeof(UniqueKey)==destinationType)
            {
                return value;
            }

            throw new NotSupportedException(string.Format("destinationType must be '{0}' but was '{1}'", typeof(string).FullName, destinationType));
        }

    }
}
