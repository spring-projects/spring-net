﻿/*
 * Copyright © 2010-2011 the original author or authors.
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

using System.Text.RegularExpressions;

namespace Spring.Context.Attributes.TypeFilters;

/// <summary>
/// A simple filter for matching a fully-qualified class name with a regex
/// </summary>
public class RegexPatternTypeFilter : ITypeFilter
{
    private string _pattern;

    /// <summary>
    /// Creates a type filter with provided pattern
    /// </summary>
    /// <param name="pattern">Regex pattern</param>
    public RegexPatternTypeFilter(string pattern)
    {
        _pattern = pattern;
    }

    /// <summary>
    /// Determine a match based on the given type object.
    /// </summary>
    /// <param name="type">Type to compare against</param>
    /// <returns>true if there is a match; false is there is no match</returns>
    public bool Match(Type type)
    {
        return Regex.IsMatch(type.FullName, _pattern);
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>
    /// A string that represents the current object.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    public override string ToString()
    {
        return string.Format("Pattern: {0}", _pattern);
    }
}