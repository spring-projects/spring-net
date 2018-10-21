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

using System.Reflection;

namespace Spring.Core
{
    /// <summary>
    /// Convenience class that exposes a signature that matches the
    /// <see cref="System.Reflection.MemberFilter"/> delegate.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Useful when filtering <see cref="System.Type"/> members via the
    /// <see cref="Spring.Core.ICriteria"/> mechanism.
    /// </p>
    /// </remarks>
    /// <author>Rick Evans</author>
    public class CriteriaMemberFilter 
    {
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="CriteriaMemberFilter"/> class.
        /// </summary>
        public CriteriaMemberFilter ()
        {
        }

        /// <summary>
        /// Returns true if the supplied <see cref="System.Reflection.MemberInfo"/> instance
        /// satisfies the supplied <paramref name="filterCriteria"/> (which must be an
        /// <see cref="Spring.Core.ICriteria"/> implementation).
        /// </summary>
        /// <param name="member">
        /// The <see cref="System.Reflection.MemberInfo"/> instance that will be checked to see if
        /// it matches the supplied <paramref name="filterCriteria"/>.
        /// </param>
        /// <param name="filterCriteria">
        /// The criteria against which to filter the supplied
        /// <see cref="System.Reflection.MemberInfo"/> instance.
        /// </param>
        /// <returns>
        /// True if the supplied <see cref="System.Reflection.MemberInfo"/> instance
        /// satisfies the supplied <paramref name="filterCriteria"/> (which must be an
        /// <see cref="Spring.Core.ICriteria"/> implementation); false if not or the
        /// supplied <paramref name="filterCriteria"/> is not an
        /// <see cref="Spring.Core.ICriteria"/> implementation or is null.
        /// </returns>
        public virtual bool FilterMemberByCriteria (MemberInfo member, object filterCriteria) 
        {
            ICriteria criteria = filterCriteria as ICriteria;
            return criteria.IsSatisfied (member);
        }
    }
}