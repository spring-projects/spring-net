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

using Quartz;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Extension of the MethodInvokingJob, implementing the StatefulJob interface.
    /// Quartz checks whether or not jobs are stateful and if so,
    /// won't let jobs interfere with each other.
    /// </summary>
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class StatefulMethodInvokingJob : MethodInvokingJob
    {
        // No implementation, just an addition of the tag interface StatefulJob
        // in order to allow stateful method invoking jobs.
    }
}