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

using Spring.Objects.Support;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Exception that wraps an exception thrown from a target method.
    /// Propagated to the Quartz scheduler from a Job that reflectively invokes
    /// an arbitrary target method.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <seealso cref="MethodInvokingJobDetailFactoryObject" />
    public class JobMethodInvocationFailedException : Exception // TODO, in Java NestedRuntimeException
    {
        /// <summary>
        /// Constructor for JobMethodInvocationFailedException.
        /// </summary>
        /// <param name="methodInvoker">the MethodInvoker used for reflective invocation</param>
        /// <param name="cause">the root cause (as thrown from the target method)</param>
        public JobMethodInvocationFailedException(MethodInvoker methodInvoker, Exception cause) :
            base("Invocation of method '" + methodInvoker.TargetMethod +
                 "' on target class [" + methodInvoker.TargetType + "] failed", cause)
        {
        }
    }
}
