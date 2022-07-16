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
    /// Implementation of the sort processor.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class SortProcessor : ICollectionProcessor
    {
        /// <summary>
        /// Sorts the source collection.
        /// </summary>
        /// <remarks>
        /// Please not that this processor requires that collection elements
        /// are of a uniform type and that they implement <see cref="IComparable"/>
        /// interface.
        /// <p/>
        /// If you want to perform custom sorting based on element properties
        /// you should consider using <see cref="OrderByProcessor"/> instead.
        /// </remarks>
        /// <param name="source">
        /// The source collection to sort.
        /// </param>
        /// <param name="args">
        /// Ignored.
        /// </param>
        /// <returns>
        /// An array containing sorted collection elements.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If <paramref name="source"/> collection is not empty and it is
        /// neither <see cref="IList"/> nor <see cref="ISet"/>.
        /// </exception>
        public object Process(ICollection source, object[] args)
        {
            if (source == null || source.Count == 0)
            {
                return source;
            }

            bool sortAscending = true;
            if (args != null && args.Length == 1 && args[0] is bool)
            {
                sortAscending = (bool) args[0];
            }

            ArrayList list = new ArrayList(source);
            list.Sort();
            if (!sortAscending)
            {
                list.Reverse();
            }

            Type elementType = DetermineElementType(list);
            return list.ToArray(elementType);
        }

        private Type DetermineElementType(IList list)
        {
            for(int i=0;i<list.Count;i++)
            {
                object element = list[i];
                if (element != null) return element.GetType();
            }
            return typeof (object);
        }
    }
}
