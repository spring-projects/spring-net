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

using System.Collections;
using Quartz;
using Quartz.Impl;
using Spring.Context;
using Spring.Objects.Factory;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Convenience subclass of Quartz' JobDetail class that eases properties based
    /// usage.
    /// </summary>
    /// <remarks>
    /// <see cref="IJobDetail" /> itself is already a object but lacks
    /// sensible defaults. This class uses the Spring object name as job name,
    /// and the Quartz default group ("DEFAULT") as job group if not specified.
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <seealso cref="IJobDetail.Key" />
    /// <seealso cref="SchedulerConstants.DefaultGroup" />
    public class JobDetailObject : JobDetailImpl, IObjectNameAware, IApplicationContextAware, IInitializingObject
    {
        private Type actualJobType;
        private string objectName;
        private IApplicationContext applicationContext;
        private string applicationContextJobDataKey;

        /// <summary>
        /// Overridden to support any job class, to allow a custom JobFactory
        /// to adapt the given job class to the Quartz Job interface.
        /// </summary>
        /// <seealso cref="SchedulerFactoryObject.JobFactory" />
        public override Type JobType
        {
            get => actualJobType ?? base.JobType;

            set
            {
                if (value != null && !typeof(IJob).IsAssignableFrom(value))
                {
                    base.JobType = typeof(DelegatingJob);
                    actualJobType = value;
                }
                else
                {
                    base.JobType = value;
                }
            }
        }

        /// <summary>
        /// Register objects in the JobDataMap via a given Map.
        /// </summary>
        /// <remarks>
        /// These objects will be available to this Job only,
        /// in contrast to objects in the SchedulerContext.
        /// <p>
        /// Note: When using persistent Jobs whose JobDetail will be kept in the
        /// database, do not put Spring-managed objects or an ApplicationContext
        /// reference into the JobDataMap but rather into the SchedulerContext.
        /// </p>
        /// </remarks>
        /// <seealso cref="SchedulerFactoryObject.SchedulerContextAsMap" />
        public virtual IDictionary JobDataAsMap
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("Value cannot be null", "value");
                }

                foreach (DictionaryEntry entry in value)
                {
                    JobDataMap.Put((string) entry.Key, entry.Value);
                }
            }
        }

        /// <summary>
        /// Set the name of the object in the object factory that created this object.
        /// </summary>
        /// <value>The name of the object in the factory.</value>
        /// <remarks>
        /// Invoked after population of normal object properties but before an init
        /// callback like <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// method or a custom init-method.
        /// </remarks>
        public virtual string ObjectName
        {
            set => objectName = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Spring.Context.IApplicationContext"/> that this
        /// object runs in.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// <p>
        /// Normally this call will be used to initialize the object.
        /// </p>
        /// <p>
        /// Invoked after population of normal object properties but before an
        /// init callback such as
        /// <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// or a custom init-method. Invoked after the setting of any
        /// <see cref="Spring.Context.IResourceLoaderAware"/>'s
        /// <see cref="Spring.Context.IResourceLoaderAware.ResourceLoader"/>
        /// property.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Context.ApplicationContextException">
        /// In the case of application context initialization errors.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If thrown by any application context methods.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectInitializationException"/>
        public virtual IApplicationContext ApplicationContext
        {
            set => applicationContext = value;
            get => applicationContext;
        }

        /// <summary>
        /// Set the key of an IApplicationContext reference to expose in the JobDataMap,
        /// for example "applicationContext". Default is none.
        /// Only applicable when running in a Spring ApplicationContext.
        /// </summary>
        /// <remarks>
        /// <p>
        /// In case of a QuartzJobObject, the reference will be applied to the Job
        /// instance as object property. An "applicationContext" attribute will correspond
        /// to a "setApplicationContext" method in that scenario.
        /// </p>
        /// <p>
        /// Note that ObjectFactory callback interfaces like IApplicationContextAware
        /// are not automatically applied to Quartz Job instances, because Quartz
        /// itself is responsible for the lifecycle of its Jobs.
        /// </p>
        /// <p>
        /// <b>Note: When using persistent job stores where JobDetail contents will
        /// be kept in the database, do not put an IApplicationContext reference into
        /// the JobDataMap but rather into the SchedulerContext.</b>
        /// </p>
        /// </remarks>
        /// <seealso cref="SchedulerFactoryObject.ApplicationContextSchedulerContextKey" />
        /// <seealso cref="IApplicationContext" />
        public virtual string ApplicationContextJobDataKey
        {
            set => applicationContextJobDataKey = value;
        }

        /// <summary>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has injected all of an object's dependencies.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method allows the object instance to perform the kind of
        /// initialization only possible when all of it's dependencies have
        /// been injected (set), and to throw an appropriate exception in the
        /// event of misconfiguration.
        /// </p>
        /// <p>
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
            if (Name == null)
            {
                Name = objectName;
            }

            if (Group == null)
            {
                Group = SchedulerConstants.DefaultGroup;
            }

            if (applicationContextJobDataKey != null)
            {
                if (applicationContext == null)
                {
                    throw new ArgumentException("JobDetailObject needs to be set up in an IApplicationContext " +
                                                "to be able to handle an 'applicationContextJobDataKey'");
                }

                JobDataMap.Put(applicationContextJobDataKey, applicationContext);
            }
        }
    }
}
