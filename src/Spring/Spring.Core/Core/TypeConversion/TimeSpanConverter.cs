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
    #region Specifier parsers

    using TimeSpanNullable = Nullable<TimeSpan>;

    /// <summary>
    /// Base parser for <see cref="TimeSpanConverter"/> custom specifiers.
    /// </summary>
    abstract class SpecifierParser
    {
        const RegexOptions ParsingOptions = RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.IgnoreCase;

        /// <summary>
        /// Specifier
        /// </summary>
        public abstract string Specifier { get; }

        /// <summary>
        /// Convert int value to a Timespan based on the specifier
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract TimeSpan Parse(int value);

        /// <summary>
        /// Check if the string contains the specifier and
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TimeSpanNullable Match(string value)
        {
            string regex = @"^(\d+)" + Specifier + "$";
            Match match = Regex.Match(value, regex, ParsingOptions);

            if (!match.Success) return new TimeSpanNullable();

            return new TimeSpanNullable(Parse(int.Parse(match.Groups[1].Value)));
        }

    }

    /// <summary>
    /// Recognize 10d as ten days
    /// </summary>
    class DaySpecifier: SpecifierParser
    {
        /// <summary>
        /// Day specifier: d
        /// </summary>
        public override string Specifier
        {
            get { return "d"; }
        }

        /// <summary>
        /// Parse value as days
        /// </summary>
        /// <param name="value">Timespan in days</param>
        /// <returns></returns>
        public override TimeSpan Parse(int value)
        {
            return TimeSpan.FromDays(value);
        }
    }

    /// <summary>
    /// Recognize 10h as ten hours
    /// </summary>
    class HourSpecifier : SpecifierParser
    {
        /// <summary>
        /// Hour specifier: h
        /// </summary>
        public override string Specifier
        {
            get { return "h"; }
        }

        /// <summary>
        /// Parse value as hours
        /// </summary>
        /// <param name="value">Timespan in hours</param>
        /// <returns></returns>
        public override TimeSpan Parse(int value)
        {
            return TimeSpan.FromHours(value);
        }
    }

    /// <summary>
    /// Recognize 10m as ten minutes
    /// </summary>
    class MinuteSpecifier : SpecifierParser
    {
        /// <summary>
        /// Minute specifier: m
        /// </summary>
        public override string Specifier
        {
            get { return "m"; }
        }

        /// <summary>
        /// Parse value as minutes
        /// </summary>
        /// <param name="value">Timespan in minutes</param>
        /// <returns></returns>
        public override TimeSpan Parse(int value)
        {
            return TimeSpan.FromMinutes(value);
        }
    }

    /// <summary>
    /// Recognize 10s as ten seconds
    /// </summary>
    class SecondSpecifier : SpecifierParser
    {
        /// <summary>
        /// Second specifier: s
        /// </summary>
        public override string Specifier
        {
            get { return "s"; }
        }

        /// <summary>
        /// Parse value as seconds
        /// </summary>
        /// <param name="value">Timespan in seconds</param>
        /// <returns></returns>
        public override TimeSpan Parse(int value)
        {
            return TimeSpan.FromSeconds(value);
        }
    }

    /// <summary>
    /// Recognize 10ms as ten milliseconds
    /// </summary>
    class MillisecondSpecifier : SpecifierParser
    {
        /// <summary>
        /// Millisecond specifier: ms
        /// </summary>
        public override string Specifier
        {
            get { return "ms"; }
        }

        /// <summary>
        /// Parse value as milliseconds
        /// </summary>
        /// <param name="value">Timespan in milliseconds</param>
        /// <returns></returns>
        public override TimeSpan Parse(int value)
        {
            return TimeSpan.FromMilliseconds(value);
        }
    }

    #endregion

    /// <summary>
    /// Converter for <see cref="System.TimeSpan"/> instances.
    /// </summary>
    /// <author>Bruno Baia</author>
    /// <author>Roberto Paterlini</author>
    public class TimeSpanConverter : System.ComponentModel.TimeSpanConverter
    {
        #region Constants

        static readonly SpecifierParser[] Specifiers = {
                                                  new DaySpecifier(),
                                                  new HourSpecifier(),
                                                  new MinuteSpecifier(),
                                                  new SecondSpecifier(),
                                                  new MillisecondSpecifier()
                                              };

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
            string stringValue = value as string;
            if (stringValue!=null)
            {
                try
                {
                    stringValue = stringValue.Trim();

                    foreach (SpecifierParser specifierParser in Specifiers)
                    {
                        TimeSpanNullable res = specifierParser.Match(stringValue);
                        if (res.HasValue) return res.Value;
                    }
                }
                catch { }
            }
            return base.ConvertFrom(context, culture, value);
        }

        #endregion
    }
}
