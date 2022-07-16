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
using Spring.Objects.Factory;
using Spring.Objects.Support;

namespace Spring.Scheduling.Quartz
{
    /// <summary> 
    /// Adapter that implements the Runnable interface as a configurable
    /// method invocation based on Spring's MethodInvoker.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Derives from ArgumentConvertingMethodInvoker, inheriting common
    /// configuration properties from MethodInvoker.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <seealso cref="MethodInvoker" />
    /// <seealso cref="ArgumentConvertingMethodInvoker" />
    public class MethodInvokingRunnable : ArgumentConvertingMethodInvoker, IInitializingObject, IThreadRunnable
    {
        /// <summary>
        /// Logger instance shared by this instance and its sub-class instances.
        /// </summary>
        private readonly ILog logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodInvokingRunnable"/> class.
        /// </summary>
        public MethodInvokingRunnable()
        {
            logger = LogManager.GetLogger(GetType());
        }

        /// <summary>
        /// Logger instance.
        /// </summary>
        protected ILog Logger => logger;

        /// <summary>
        /// Gets the invocation failure message.
        /// </summary>
        /// <value>The invocation failure message.</value>
        protected virtual string InvocationFailureMessage =>
            $"Invocation of method '{TargetMethod}' on target object [{TargetObject}] failed";

        /// <summary>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has injected all of an object's dependencies.
        /// </summary>
        /// <remarks>
        /// 	<p>
        /// This method allows the object instance to perform the kind of
        /// initialization only possible when all of it's dependencies have
        /// been injected (set), and to throw an appropriate exception in the
        /// event of misconfiguration.
        /// </p>
        /// 	<p>
        /// Please do consult the class level documentation for the
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/> interface for a
        /// description of exactly <i>when</i> this method is invoked. In
        /// particular, it is worth noting that the
        /// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>
        /// and <see cref="Spring.Context.IApplicationContextAware"/>
        /// callbacks will have been invoked <i>prior</i> to this method being
        /// called.
        /// </p>
        /// </remarks>
        /// <exception cref="System.Exception">
        /// In the event of misconfiguration (such as the failure to set a
        /// required property) or if initialization fails.
        /// </exception>
        public virtual void AfterPropertiesSet()
        {
            Prepare();
        }

        /// <summary>
        /// This method has to be implemented in order that starting of the thread causes the object's
        /// run method to be called in that separately executing thread.
        /// </summary>
        public virtual Task Run()
        {
            try
            {
                Invoke();
            }
            catch (TargetInvocationException ex)
            {
                logger.Error(InvocationFailureMessage, ex);
                // Do not throw exception, else the main loop of the Timer will stop!
            }
            catch (Exception ex)
            {
                logger.Error(InvocationFailureMessage, ex);
                // Do not throw exception, else the main loop of the Timer will stop!
            }

            return Task.FromResult(true);
        }
    }
}
