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
using Quartz.Impl;
using Spring.Objects.Factory;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Spring class for accessing a Quartz Scheduler, i.e. for registering jobs,
    /// triggers and listeners on a given <see cref="IScheduler" /> instance.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Marko Lahma (.NET)</author>
    /// <seealso cref="Scheduler" />
    /// <seealso cref="SchedulerName" />
    public class SchedulerAccessorObject : SchedulerAccessor, IObjectFactoryAware, IInitializingObject
    {
        private string schedulerName;
        private IScheduler scheduler;
        private IObjectFactory objectFactory;

        /// <summary>
        /// Specify the Quartz Scheduler to operate on via its scheduler name in the Spring
        /// application context or also in the Quartz {@link org.quartz.impl.SchedulerRepository}.
        /// </summary>
        /// <remarks>
        /// Schedulers can be registered in the repository through custom bootstrapping,
        /// e.g. via the <see cref="StdSchedulerFactory" /> or
        /// <see cref="DirectSchedulerFactory" /> factory classes.
        /// However, in general, it's preferable to use Spring's <see cref="SchedulerFactoryObject" />
        /// which includes the job/trigger/listener capabilities of this accessor as well.    
        /// </remarks>
        public string SchedulerName
        {
            set => schedulerName = value;
        }

        /// <summary>
        /// Return the Quartz Scheduler instance that this accessor operates on.
        /// </summary>
        protected IScheduler Scheduler
        {
            set => scheduler = value;
        }

        /// <summary>
        /// Template method that determines the Scheduler to operate on.
        /// </summary>
        /// <returns></returns>
        protected override IScheduler GetScheduler()
        {
            return scheduler;
        }

        /// <summary>
        /// Return the Quartz Scheduler instance that this accessor operates on.
        /// </summary>
        public IObjectFactory ObjectFactory
        {
            set => objectFactory = value;
        }

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
        public void AfterPropertiesSet()
        {
            if (scheduler == null)
            {
                if (schedulerName != null)
                {
                    scheduler = FindScheduler(schedulerName);
                }
                else
                {
                    throw new InvalidOperationException("No Scheduler specified");
                }
            }

            RegisterListeners();
            RegisterJobsAndTriggers().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Finds the scheduler.
        /// </summary>
        /// <param name="schedulerName">Name of the scheduler.</param>
        /// <returns></returns>
        protected virtual IScheduler FindScheduler(string schedulerName)
        {
            if (objectFactory is IListableObjectFactory lbf)
            {
                var objectNames = lbf.GetObjectNamesForType(typeof(IScheduler));
                foreach (string objectName in objectNames)
                {
                    IScheduler schedulerObject = (IScheduler) lbf.GetObject(objectName);
                    if (schedulerName.Equals(schedulerObject.SchedulerName))
                    {
                        return schedulerObject;
                    }
                }
            }

            IScheduler schedulerInRepo = SchedulerRepository.Instance.Lookup(schedulerName)
                .ConfigureAwait(false).GetAwaiter().GetResult();
            if (schedulerInRepo == null)
            {
                throw new InvalidOperationException("No Scheduler named '" + schedulerName + "' found");
            }

            return schedulerInRepo;
        }
    }
}
