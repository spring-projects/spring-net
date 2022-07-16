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
using Spring.Util;

namespace Spring.Expressions.Processors
{
    /// <summary>
    /// Implementation of the average aggregator.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class AverageAggregator : ICollectionProcessor
    {
        /// <summary>
        /// Returns the average of the numeric values in the source collection.
        /// </summary>
        /// <param name="source">
        /// The source collection to process.
        /// </param>
        /// <param name="args">
        /// Ignored.
        /// </param>
        /// <returns>
        /// The average of the numeric values in the source collection.
        /// </returns>
        public object Process(ICollection source, object[] args)
        {
            int n = 0;
            object total = 0d;
            foreach (object item in source)
            {
                if (item != null)
                {
                    if (NumberUtils.IsNumber(item))
                    {
                        total = NumberUtils.Add(total, item);
                        n++;
                    }
                    else
                    {
                        throw new ArgumentException("Average can only be calculated for a collection of numeric values.");
                    }
                }
            }

            return NumberUtils.Divide(total, n);
        }
    }
}
