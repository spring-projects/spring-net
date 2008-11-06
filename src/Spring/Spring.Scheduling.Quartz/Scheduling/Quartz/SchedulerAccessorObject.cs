/*
 * Copyright 2002-2008 the original author or authors.
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

using System;

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
            set { schedulerName = value; }
        }

        /// <summary>
        /// Return the Quartz Scheduler instance that this accessor operates on.
        /// </summary>
        protected IScheduler Scheduler
        {
            set { scheduler = value; }
        }

        protected override IScheduler GetScheduler()
        {
            return scheduler;
        }

        /// <summary>
        /// Return the Quartz Scheduler instance that this accessor operates on.
        /// </summary>
        public IObjectFactory ObjectFactory
        {
            set { objectFactory = value; }
        }


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
            RegisterJobsAndTriggers();
        }

        protected virtual IScheduler FindScheduler(string schedulerName)
        {
            if (objectFactory is IListableObjectFactory)
            {
                IListableObjectFactory lbf = (IListableObjectFactory) objectFactory;
                string[] objectNames = lbf.GetObjectNamesForType(typeof(IScheduler));
                for (int i = 0; i < objectNames.Length; i++)
                {
                    IScheduler schedulerObject = (IScheduler)lbf.GetObject(objectNames[i]);
                    if (schedulerName.Equals(schedulerObject.SchedulerName))
                    {
                        return schedulerObject;
                    }
                }
            }
            IScheduler schedulerInRepo = SchedulerRepository.Instance.Lookup(schedulerName);
            if (schedulerInRepo == null)
            {
                throw new InvalidOperationException("No Scheduler named '" + schedulerName + "' found");
            }
            return schedulerInRepo;
        }

    }
}
