#region License

// /*
//  * Copyright 2018 the original author or authors.
//  *
//  * Licensed under the Apache License, Version 2.0 (the "License");
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  *      http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */

#endregion

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Spring;

public static class LogManager
{
    /// <summary>
    /// Gets or sets the current log provider based on logger factory.
    /// </summary>
    public static ILoggerFactory LoggerFactory { get; set; }

    public static ILogger GetLogger(string category) => LoggerFactory?.CreateLogger(category) ?? NullLogger.Instance;
    public static ILogger GetLogger(Type type) => LoggerFactory?.CreateLogger(type) ?? NullLogger.Instance;
    public static ILogger<T> GetLogger<T>() => LoggerFactory?.CreateLogger<T>() ?? NullLogger<T>.Instance;
}
