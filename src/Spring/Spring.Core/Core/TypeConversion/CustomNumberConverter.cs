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
	/// A custom <see cref="System.ComponentModel.TypeConverter"/> for any
	/// primitive numeric type such as <see cref="System.Int32"/>,
	/// <see cref="System.Single"/>, <see cref="System.Double"/>, etc.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Can use a given <see cref="System.Globalization.NumberFormatInfo"/> for
	/// (locale-specific) parsing and rendering.
	/// </p>
	/// <p>
	/// This is not meant to be used as a system
	/// <see cref="System.ComponentModel.TypeConverter"/> but rather as a
	/// locale-specific number converter within custom controller code, to
	/// parse user-entered number strings into number properties of objects,
	/// and render them in a UI form.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Simon White (.NET)</author>
	public class CustomNumberConverter : TypeConverter
	{
		private Type _type;
		private NumberFormatInfo _nfi;
		private bool _allowEmpty;

		/// <summary>
		/// Creates a new instance of the <see cref="CustomNumberConverter"/>
		/// class.
		/// </summary>
		/// <param name="type">
		/// The primitive numeric <see cref="System.Type"/> to convert to.
		/// </param>
		/// <param name="format">
		/// The <see cref="System.Globalization.NumberFormatInfo"/> to use for
		/// (locale-specific) parsing and rendering
		/// </param>
		/// <param name="allowEmpty">
		/// Is an empty string allowed to be converted? If
		/// <see langword="true"/>, an empty string value will be converted to
		/// numeric 0.</param>
		/// <exception cref="System.ArgumentException">
		/// Id the supplied <paramref name="type"/> is not a primitive
		/// <see cref="System.Type"/>.
		/// </exception>
		/// <seealso cref="System.Type.IsPrimitive"/>
		public CustomNumberConverter(
			Type type, NumberFormatInfo format, bool allowEmpty)
		{
			if (!type.IsPrimitive)
			{
				throw new ArgumentException(
					"Property type must be a primitive type.");
			}
			this._type = type;
			this._nfi = format;
			this._allowEmpty = allowEmpty;
		}

		/// <summary>
		/// Returns whether this converter can convert an object of one
		/// <see cref="System.Type"/> to a <see cref="System.IO.FileInfo"/>
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
		/// <returns>
		/// <see langword="true"/> if the conversion is possible.
		/// </returns>
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
		/// Converts the specified object (a string) to the required primitive
		/// type.
		/// </summary>
		/// <param name="context">
		/// A <see cref="System.ComponentModel.ITypeDescriptorContext"/>
		/// that provides a format context.
		/// </param>
		/// <param name="culture">
		/// The <see cref="System.Globalization.CultureInfo"/> to use
		/// as the current culture. 
		/// </param>
		/// <param name="val">
		/// The value that is to be converted.
		/// </param>
		/// <returns>A primitive representation of the string value.</returns>
		public override object ConvertFrom(
			ITypeDescriptorContext context, CultureInfo culture, object val)
		{
			if (val is string)
			{
				string value = val as string;
				if (!StringUtils.HasText(value)
					&& _allowEmpty)
				{
					value = "0";
				}
				if (_type.Equals(typeof (Int16)))
				{
					return Convert.ToInt16(value, _nfi);
				}
				else if (_type.Equals(typeof (UInt16)))
				{
					return Convert.ToUInt16(value, _nfi);
				}
				else if (_type.Equals(typeof (Int32)))
				{
					return Convert.ToInt32(value, _nfi);
				}
				else if (_type.Equals(typeof (UInt32)))
				{
					return Convert.ToUInt32(value, _nfi);
				}
				else if (_type.Equals(typeof (Int64)))
				{
					return Convert.ToInt64(value, _nfi);
				}
				else if (_type.Equals(typeof (UInt64)))
				{
					return Convert.ToUInt64(value, _nfi);
				}
				else if (_type.Equals(typeof (Single)))
				{
					return Convert.ToSingle(value, _nfi);
				}
				else if (_type.Equals(typeof (Double)))
				{
					return Convert.ToDouble(value, _nfi);
				}
			}
			return base.ConvertFrom(context, culture, val);
		}
	}
}
