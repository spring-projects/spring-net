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
using System.Drawing;
using System.Globalization;

using Spring.Util;

#endregion

namespace Spring.Core.TypeConversion
{
    /// <summary>
    /// Converter for <see cref="System.Drawing.Color"/> from a comma separated
    /// list of RBG values.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Please note that this class does <b>not</b> implement converting
    /// to a comma separated list of RBG values from a
    /// <see cref="System.Drawing.Color"/>.
    /// </p>
    /// </remarks>
    /// <author>Federico Spinazzi</author>
    public sealed class RGBColorConverter : TypeConverter
    {
        private const char ArgbSeparator = ',';

        private const string DefaultAlpha = "255";

        /// <summary>
        /// Returns whether this converter can convert an object of one
        /// <see cref="System.Type"/> to a 
        /// <see cref="System.Drawing.Color"/>.
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
        /// <returns><see langword="true"/> if the conversion is possible.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof (string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts the specified object (a string) a <see cref="System.Drawing.Color"/>
        /// instance.
        /// </summary>
        /// <param name="context">
        /// A <see cref="System.ComponentModel.ITypeDescriptorContext"/>
        /// that provides a format context.
        /// </param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> to use
        /// as the current culture: currently ignored.
        /// </param>
        /// <param name="value">
        /// The value that is to be converted, in "R,G,B", "A,R,G,B", or 
        /// symbolic color name (System.Drawing.KnownColor if on .NET Full Framework).
        /// </param>
        /// <returns>
        /// A <see cref="System.Drawing.Color"/> representation of the string value.
        /// </returns>
        /// <exception cref="System.FormatException">
        /// If the input string is not in a supported format, or is not one of the
        /// predefined system colors (System.Drawing.KnownColor if on .NET Full Framework).
        /// </exception>
        public override object ConvertFrom(
            ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string input = value as string;
            if (StringUtils.HasText(input))
            {
                if (input.IndexOf(ArgbSeparator) > -1)
                {
                    string[] colorSplit = input.Split(ArgbSeparator);
                    if (colorSplit.Length == 3)
                    {
                        return FromRgb(colorSplit);
                    }
                    else if (colorSplit.Length == 4)
                    {
                        return FromArgb(colorSplit);
                    }
                    else
                    {
                        throw new FormatException(
                            "Input string is not in the correct format : '" + input + "'.");
                    }
                }
                return FromName(input);
            }
            return base.ConvertFrom(context, culture, value);
        }

        private static object FromRgb(string[] rgb)
        {
            return GetColorFrom(DefaultAlpha, rgb[0], rgb[1], rgb[2]);
        }

        private static object FromArgb(string[] argb)
        {
            return GetColorFrom(argb[0], argb[1], argb[2], argb[3]);
        }

        private static Color FromName(string name)
        {
#if NETSTANDARD
            throw new NotSupportedException("FromName is not supported under .NET Core");
#else
            try
            {
                KnownColor color = (KnownColor) Enum.Parse(typeof (KnownColor), name);
                return Color.FromKnownColor(color);
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    "Input string is not a known system color : '" + name + "'.", ex);
            }
#endif
        }

        private static Color GetColorFrom(string alpha, string red, string green, string blue)
        {
            try
            {
                return Color.FromArgb(Int32.Parse(alpha), Int32.Parse(red), Int32.Parse(green), Int32.Parse(blue));
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    "Bad color format.", ex);
            }
        }
    }
}
