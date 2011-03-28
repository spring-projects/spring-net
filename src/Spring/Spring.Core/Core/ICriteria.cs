#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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



#endregion

namespace Spring.Core
{
    /// <summary>
    /// The criteria for an arbitrary filter.
    /// </summary>
    /// <author>Rick Evans</author>
    public interface ICriteria 
    {
        /// <summary>
        /// Does the supplied <paramref name="datum"/> satisfy the criteria
        /// encapsulated by this instance?
        /// </summary>
        /// <param name="datum">
        /// The datum to be checked by this criteria instance.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the supplied <paramref name="datum"/>
        /// satisfies the criteria encapsulated by this instance;
        /// <see langword="false"/> if not, or the supplied
        /// <paramref name="datum"/> is <see langword="null"/>.
        /// </returns>
        bool IsSatisfied (object datum);
    }
}
