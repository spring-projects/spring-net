#region Licence

/*
 * Copyright © 2002-2005 the original author or authors.
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

using System;
using System.ComponentModel;
using System.Globalization;

#endregion

namespace SpringAir.Domain
{
	/// <summary>
	/// ...
	/// </summary>
	/// <author>Keith Donald</author>
	/// <author>Rick Evans (.NET)</author>
	/// <version>$Id: TimeRange.cs,v 1.3 2005/10/09 06:18:45 aseovic Exp $</version>
	[Serializable]
    [TypeConverter(typeof(TimeRange.TimeRangeTypeConverter))]
	public class TimeRange
	{
		private short minHour;
		private short maxHour;

	    public TimeRange()
	    {}

	    public TimeRange(short minHour, short maxHour)
		{
			this.minHour = minHour;
			this.maxHour = maxHour;
		}

		public short MinHour
		{
			get { return this.minHour; }
		}

		public short MaxHour
		{
			get { return this.maxHour; }
		}

		public override bool Equals(Object obj)
		{
			TimeRange rhs = obj as TimeRange;
			return rhs != null
				&& this.minHour == rhs.minHour
				&& this.maxHour == rhs.maxHour;
		}

		public override int GetHashCode()
		{
			return this.minHour.GetHashCode() + this.maxHour.GetHashCode() * 29;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture,
			                     "[{0} - {1}]", this.minHour, this.maxHour);
        }

        #region Inner Class : TimeRangeTypeConverter

        /// <summary>
        /// The default <see cref="SpringAir.Domain.TimeRange"/>
        ///  <see cref="System.ComponentModel.TypeConverter"/> implementation.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Seems reasonable to supply this here, as the
        /// <see cref="SpringAir.Domain.TimeRange"/> class is pretty vanilla, and
        /// one can't see anyone wanting to do a different
        /// <see cref="System.ComponentModel.TypeConverter"/> implementation. This
        /// is, at the very least, a reasonable default.
        /// </p>
        /// </remarks>
        [Serializable]
        public sealed class TimeRangeTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof (string))
                {
                    return true;
                }
                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                string text = value as string;
                if (text != null)
                {
                    text = text.Trim().Trim(new char[] {'[', ']'}).Trim();
                    string[] minMax = text.Split('-');
                    if(minMax.Length != 2)
                    {
                        throw new FormatException();
                    }
                    try
                    {
                        short minHours = short.Parse(minMax[0].Trim());
                        short maxHours = short.Parse(minMax[1].Trim());
                        return new TimeRange(minHours, maxHours);
                    }
                    catch (OverflowException ex)
                    {
                        throw new FormatException("Values out of range.", ex);
                    }
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof (string))
                {
                    return true;
                }
                return base.CanConvertFrom(context, destinationType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
                Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    TimeRange range = (TimeRange) value;
                    // don't want to rely on TimeRange.ToString implementation (even though its the same)...
                    return string.Format(
                        CultureInfo.InvariantCulture,
                        "[{0} - {1}]", range.MinHour, range.MaxHour);
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        #endregion
	}
}