/*
 * Copyright © 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;

namespace Spring.Core
{
    /// <summary>
    /// A <see cref="Spring.Core.ICriteria"/> implementation that represents
    /// a composed collection of <see cref="Spring.Core.ICriteria"/> instances.
    /// </summary>
    public class ComposedCriteria : ICriteria
    {
        private readonly List<ICriteria> _criteria;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="ComposedCriteria"/> class.
        /// </summary>
        public ComposedCriteria() : this(null)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="ComposedCriteria"/> class.
        /// </summary>
        /// <param name="criteria">
        /// A user-defined (child) criteria that will be composed into this instance.
        /// </param>
        public ComposedCriteria(ICriteria criteria)
        {
            _criteria = new List<ICriteria>();
            Add(criteria);
        }

        /// <inheritdoc />
        public virtual bool IsSatisfied(object datum)
        {
            for (var i = 0; i < _criteria.Count; i++)
            {
                if (!_criteria[i].IsSatisfied(datum))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds the supplied <parameref name="criteria"/> into the criteria
        /// composed within this instance.
        /// </summary>
        /// <param name="criteria">
        /// The <see cref="Spring.Core.ICriteria"/> to be added.
        /// </param>
        public void Add(ICriteria criteria)
        {
            if (criteria != null)
            {
                _criteria.Add(criteria);
            }
        }

        /// <summary>
        /// The list of <see cref="Spring.Core.ICriteria"/> composing this
        /// instance.
        /// </summary>
        protected IReadOnlyList<ICriteria> Criteria => _criteria;
    }
}