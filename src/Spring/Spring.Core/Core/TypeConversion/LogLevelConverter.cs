/*
 * Copyright 2002-2026 the original author or authors.
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

using System.ComponentModel;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Spring.Core.TypeConversion;

/// <summary>
/// Converter for <see cref="Microsoft.Extensions.Logging.LogLevel"/> instances.
/// </summary>
/// <remarks>
/// In addition to the <see cref="Microsoft.Extensions.Logging.LogLevel"/> member names
/// (<c>Trace</c>, <c>Debug</c>, <c>Information</c>, <c>Warning</c>, <c>Error</c>,
/// <c>Critical</c>, <c>None</c>), the legacy Common.Logging level names used by
/// Spring.NET configurations prior to 3.0 are accepted: <c>All</c> maps to
/// <c>Trace</c>, <c>Info</c> to <c>Information</c>, <c>Warn</c> to <c>Warning</c>,
/// <c>Fatal</c> to <c>Critical</c> and <c>Off</c> to <c>None</c>.
/// Names are matched case-insensitively.
/// </remarks>
public class LogLevelConverter : EnumConverter
{
    private static readonly Dictionary<string, LogLevel> LegacyLevels = new Dictionary<string, LogLevel>(StringComparer.OrdinalIgnoreCase)
    {
        ["All"] = LogLevel.Trace,
        ["Info"] = LogLevel.Information,
        ["Warn"] = LogLevel.Warning,
        ["Fatal"] = LogLevel.Critical,
        ["Off"] = LogLevel.None
    };

    /// <summary>
    /// Creates a new instance of the
    /// <see cref="Spring.Core.TypeConversion.LogLevelConverter"/> class.
    /// </summary>
    public LogLevelConverter() : base(typeof(LogLevel))
    {
    }

    /// <summary>
    /// Convert from a string value to a <see cref="Microsoft.Extensions.Logging.LogLevel"/> instance.
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
    /// A <see cref="Microsoft.Extensions.Logging.LogLevel"/> if successful.
    /// </returns>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string text && LegacyLevels.TryGetValue(text.Trim(), out LogLevel level))
        {
            return level;
        }

        return base.ConvertFrom(context, culture, value);
    }
}
