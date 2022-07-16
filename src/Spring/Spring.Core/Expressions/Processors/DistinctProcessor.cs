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

using System.Collections;
using Spring.Collections;

namespace Spring.Expressions.Processors
{
    /// <summary>
    /// Implementation of the distinct processor.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class DistinctProcessor : ICollectionProcessor
    {
        /// <summary>
        /// Returns distinct items from the collection.
        /// </summary>
        /// <param name="source">
        /// The source collection to process.
        /// </param>
        /// <param name="args">
        /// 0: boolean flag specifying whether to include <c>null</c>
        /// in the results or not. Default is false, which means that
        /// <c>null</c> values will not be included in the results.
        /// </param>
        /// <returns>
        /// A collection containing distinct source collection elements.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If there is more than one argument, or if the single optional argument
        /// is not <b>Boolean</b>.
        /// </exception>
        public object Process(ICollection source, object[] args)
        {
            if (source == null)
            {
                return null;
            }

            bool includeNulls = false;
            if (args.Length == 1)
            {
                if (args[0] is bool)
                {
                    includeNulls = (bool) args[0];
                }
                else
                {
                    throw new ArgumentException("distinct() processor argument must be a boolean value.");
                }
            }
            else if (args.Length > 1)
            {
                throw new ArgumentException("Only a single argument can be specified for a distinct() processor.");
            }

            HybridSet set = new HybridSet(source);
            if (!includeNulls)
            {
                set.Remove(null);
            }
            return set;
        }
    }
}
