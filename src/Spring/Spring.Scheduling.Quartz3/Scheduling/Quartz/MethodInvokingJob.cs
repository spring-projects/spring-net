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

using System.Reflection;
using Common.Logging;
using Quartz;
using Spring.Objects.Support;

namespace Spring.Scheduling.Quartz
{
    /// <summary> 
    /// Quartz Job implementation that invokes a specified method.
    /// Automatically applied by MethodInvokingJobDetailFactoryObject.
    /// </summary>
    public class MethodInvokingJob : QuartzJobObject
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MethodInvokingJob));
        private MethodInvoker methodInvoker;
        private string errorMessage;

        /// <summary>
        /// Set the MethodInvoker to use.
        /// </summary>
        public virtual MethodInvoker MethodInvoker
        {
            set
            {
                methodInvoker = value ?? throw new ArgumentException("Method invoker cannot be null", nameof(value));
                errorMessage =
                    $"Could not invoke method '{methodInvoker.TargetMethod}' on target object [{methodInvoker.TargetObject}]";
            }
        }

        /// <summary>
        /// Invoke the method via the MethodInvoker.
        /// </summary>
        /// <param name="context"></param>
        protected override Task ExecuteInternal(IJobExecutionContext context)
        {
            if (methodInvoker == null)
            {
                throw new JobExecutionException("Could not execute job when method invoker is null");
            }

            try
            {
                context.Result = methodInvoker.Invoke();
            }
            catch (TargetInvocationException ex)
            {
                logger.Error(errorMessage, ex.GetBaseException());
                if (ex.GetBaseException() is JobExecutionException)
                {
                    // -> JobExecutionException, to be logged at info level by Quartz
                    throw ex.GetBaseException();
                }

                // -> "unhandled exception", to be logged at error level by Quartz
                throw new JobMethodInvocationFailedException(methodInvoker, ex.GetBaseException());
            }
            catch (Exception ex)
            {
                // -> "unhandled exception", to be logged at error level by Quartz
                throw new JobMethodInvocationFailedException(methodInvoker, ex.GetBaseException());
            }

            return Task.FromResult(true);
        }
    }
}
