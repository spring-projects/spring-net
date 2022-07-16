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

using Spring.Core.TypeResolution;

#endregion

namespace Spring.Core.TypeConversion
{

	/// <summary>
	/// A custom <see cref="System.ComponentModel.TypeConverter"/> for
	/// runtime type references.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Currently only supports conversion to and from a
    /// <see cref="System.String"/>.
    /// </p>
    /// </remarks>
    /// <author>Rick Evans (.NET)</author>
    public class RuntimeTypeConverter : TypeConverter
    {
        #region Constructor (s) / Destructor
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Core.TypeConversion.RuntimeTypeConverter"/> class.
        /// </summary>
        public RuntimeTypeConverter () {}
        #endregion

        #region Methods
        /// <summary>
        /// Returns whether this converter can convert an object of one
        /// <see cref="System.Type"/> to the <see cref="System.Type"/>
        /// of this converter.
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
        /// Returns whether this converter can convert the object to the specified
        /// <see cref="System.Type"/>.
        /// </summary>
        /// <param name="context">
        /// A <see cref="System.ComponentModel.ITypeDescriptorContext"/>
        /// that provides a format context.
        /// </param>
        /// <param name="destinationType">
        /// A <see cref="System.Type"/> that represents the
        /// <see cref="System.Type"/> you want to convert to.
        /// </param>
        /// <returns>True if the conversion is possible.</returns>
        public override bool CanConvertTo (
            ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof (Type))
            {
                return true;
            }
            return base.CanConvertTo (context, destinationType);
        }

        /// <summary>
        /// Converts the given value to the type of this converter.
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
        /// An <see cref="System.Object"/> that represents the converted value.
        /// </returns>
        public override object ConvertFrom (
            ITypeDescriptorContext context, 
            CultureInfo culture, object value) 
        {
            if (value is string) 
            {
                return TypeResolutionUtils.ResolveType(value as string);
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Converts the given value object to the specified type,
        /// using the specified context and culture information.
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
        /// <param name="destinationType">
        /// The <see cref="System.Type"/> to convert the
        /// <paramref name="value"/> parameter to.
        /// </param>
        /// <returns>
        /// An <see cref="System.Object"/> that represents the converted value.
        /// </returns>
        public override object ConvertTo (
            ITypeDescriptorContext context, 
            CultureInfo culture, object value, Type destinationType) 
        {  
            if (value is Type
                && destinationType == typeof (string)) 
            {
                return ((Type) value).AssemblyQualifiedName;
            }
            return base.ConvertTo (context, culture, value, destinationType);
        }
        #endregion
	}
}
