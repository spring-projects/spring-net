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

namespace Spring.Expressions.Processors
{
    /// <summary>
    /// Reverts order of elements in the list
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class ReverseProcessor : ICollectionProcessor
    {
        /// <summary>
        /// Processes a list of source items and returns a result.
        /// </summary>
        /// <param name="source">
        /// The source list to process.
        /// </param>
        /// <param name="args">
        /// An optional processor arguments array.
        /// </param>
        /// <returns>
        /// The processing result.
        /// </returns>
        public object Process(ICollection source, object[] args)
        {
            if (source == null || source.Count == 0)
            {
                return source;
            }

            ArrayList list = new ArrayList(source);
            list.Reverse();

            return list;
        }
    }
}
