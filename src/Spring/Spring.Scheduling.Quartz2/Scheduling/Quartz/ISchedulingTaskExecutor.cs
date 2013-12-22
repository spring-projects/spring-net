/*
* Copyright 2002-2010 the original author or authors.
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

namespace Spring.Scheduling
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISchedulingTaskExecutor : ITaskExecutor
    {
        /// <summary>
        /// Gets a value indicating whether´this instance prefers short lived tasks.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if prefers short lived tasks; otherwise, <c>false</c>.
        /// </value>
        bool PrefersShortLivedTasks { get; }
    }
}