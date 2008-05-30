#region License

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

namespace Spring.Core.TypeConversion
{
    /// <summary>
    /// Converter for <see cref="System.TimeSpan"/> instances.
    /// </summary>
    /// <author>Bruno Baia</author>
    /// <version>$Id: TimeSpanConverter.cs,v 1.1 2007/07/31 18:16:08 bbaia Exp $</version>
    public class TimeSpanConverter : System.ComponentModel.TimeSpanConverter
    {
        #region Constants

        private const string DaySpecifier = "d";
        private const string HourSpecifier = "h";
        private const string MinuteSpecifier = "m";
        private const string SecondSpecifier = "s";
        private const string MillisecondSpecifier = "ms";

        #endregion

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Core.TypeConversion.TimeSpanConverter"/> class.
        /// </summary>
        public TimeSpanConverter() { }

        #endregion

        #region Methods

        /// <summary>
        /// Convert from a string value to a <see cref="System.TimeSpan"/> instance.
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
        /// A <see cref="System.TimeSpan"/> if successful. 
        /// </returns>
        public override object ConvertFrom(
            ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string timeSpan = ((string)value).ToLower();
                    int specifierLengh = (timeSpan.EndsWith(MillisecondSpecifier)) ? 2 : 1;
                    int time = int.Parse(timeSpan.Substring(0, timeSpan.Length - specifierLengh));

                    switch (timeSpan.Substring(timeSpan.Length - specifierLengh, specifierLengh))
                    {
                        case MillisecondSpecifier:
                            return TimeSpan.FromMilliseconds((double)time);
                        case SecondSpecifier:
                            return TimeSpan.FromSeconds((double)time);
                        case MinuteSpecifier:
                            return TimeSpan.FromMinutes((double)time);
                        case HourSpecifier:
                            return TimeSpan.FromHours((double)time);
                        case DaySpecifier:
                            return TimeSpan.FromDays((double)time);
                    }
                }
                catch { }
            }
            return base.ConvertFrom(context, culture, value);
        }

        #endregion
    }
}