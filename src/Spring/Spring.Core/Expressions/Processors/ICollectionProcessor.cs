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

using System.Collections;

namespace Spring.Expressions.Processors
{
    /// <summary>
    /// Defines an interface that should be implemented
    /// by all collection processors and aggregators.
    /// </summary>
    public interface ICollectionProcessor
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
        object Process(ICollection source, object[] args);
    }
}
