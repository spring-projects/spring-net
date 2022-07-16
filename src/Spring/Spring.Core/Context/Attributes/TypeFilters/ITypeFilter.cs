#region License

/*
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

#endregion

namespace Spring.Context.Attributes.TypeFilters
{
    /// <summary>
    /// Represents the base interface for all component-scan type filters
    /// </summary>
    public interface ITypeFilter
    {
        /// <summary>
        /// Determine a match based on the given type object.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>true if there is a match; false is there is no match</returns>
        bool Match(Type type);
    }
}
