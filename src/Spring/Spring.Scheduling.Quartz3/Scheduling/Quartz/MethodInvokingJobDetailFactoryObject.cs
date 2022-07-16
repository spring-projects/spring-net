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
using Spring.Objects.Factory.Config;
using Spring.Objects.Support;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// IFactoryObject that exposes a JobDetail object that delegates job execution
    /// to a specified (static or non-static) method. Avoids the need to implement
    /// a one-line Quartz Job that just invokes an existing service method.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Derived from ArgumentConverting MethodInvoker to share common properties and behavior
    /// with MethodInvokingFactoryObject.
    /// </p>
    ///  <p>
    /// Supports both concurrently running jobs and non-currently running
    /// ones through the "concurrent" property. Jobs created by this
    /// MethodInvokingJobDetailFactoryObject are by default volatile and durable
    /// (according to Quartz terminology).
    /// </p>
    /// <p><b>NOTE: JobDetails created via this FactoryObject are <i>not</i>
    /// serializable and thus not suitable for persistent job stores.</b>
    /// You need to implement your own Quartz Job as a thin wrapper for each case
    /// where you want a persistent job to delegate to a specific service method.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Alef Arendsen</author>
    /// <seealso cref="Concurrent" />
    /// <seealso cref="MethodInvokingFactoryObject" />
    public class MethodInvokingJobDetailFactoryObject : ArgumentConvertingMethodInvoker,
        IObjectFactoryAware,
        IFactoryObject,
        IObjectNameAware,
        IInitializingObject
    {
        private string name;
        private string group;
        private bool concurrent = true;
        private string[] jobListenerNames;
        private string targetObjectName;
        private string objectName;
        private JobDetailImpl jobDetail;
        private IObjectFactory objectFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodInvokingJobDetailFactoryObject"/> class.
        /// </summary>
        public MethodInvokingJobDetailFactoryObject()
        {
            group = SchedulerConstants.DefaultGroup;
        }

        /// <summary>
        /// Set the name of the job.
        /// Default is the object name of this FactoryObject.
        /// </summary>
        /// <seealso cref="Name" />
        public virtual string Name
        {
            set => name = value;
        }

        /// <summary>
        /// Set the group of the job.
        /// Default is the default group of the Scheduler.
        /// </summary>
        /// <seealso cref="Group" />
        /// <seealso cref="SchedulerConstants.DefaultGroup" />
        public virtual string Group
        {
            set => group = value;
        }

        /// <summary>
        /// Specify whether or not multiple jobs should be run in a concurrent
        /// fashion. The behavior when one does not want concurrent jobs to be
        /// executed is realized through adding the <see cref="DisallowConcurrentExecutionAttribute" /> attribute.
        /// More information on stateful versus stateless jobs can be found
        /// <a href="http://www.opensymphony.com/quartz/tutorial.html#jobsMore">here</a>.
        /// <p>
        /// The default setting is to run jobs concurrently.
        /// </p>
        /// </summary>
        public virtual bool Concurrent
        {
            set => concurrent = value;
        }

        /// <summary>
        /// Gets the job detail.
        /// </summary>
        /// <value>The job detail.</value>
        protected IJobDetail JobDetail => jobDetail;

        /// <summary>
        /// Set a list of JobListener names for this job, referring to
        /// non-global JobListeners registered with the Scheduler.
        /// </summary>
        /// <remarks>
        /// A JobListener name always refers to the name returned
        /// by the JobListener implementation.
        /// </remarks>
        /// <seealso cref="SchedulerAccessor.JobListeners" />
        /// <seealso cref="IJobListener.Name" />
        public virtual string[] JobListenerNames
        {
            set => jobListenerNames = value;
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
        /// Set the name of the target object in the Spring object factory.
        /// </summary>
        /// <remarks>
        /// This is an alternative to specifying TargetObject
        /// allowing for non-singleton objects to be invoked. Note that specified
        /// "TargetObject" and "TargetType" values will
        /// override the corresponding effect of this "TargetObjectName" setting
        ///(i.e. statically pre-define the object type or even the target object).
        /// </remarks>
        public string TargetObjectName
        {
            set => targetObjectName = value;
        }

        /// <summary>
        /// Sets the object factory.
        /// </summary>
        /// <value>The object factory.</value>
        public IObjectFactory ObjectFactory
        {
            set => objectFactory = value;
        }

        /// <summary>
        /// Return an instance (possibly shared or independent) of the object
        /// managed by this factory.
        /// </summary>
        /// <returns>
        /// An instance (possibly shared or independent) of the object managed by
        /// this factory.
        /// </returns>
        /// <remarks>
        /// <note type="caution">
        /// If this method is being called in the context of an enclosing IoC container and
        /// returns <see langword="null"/>, the IoC container will consider this factory
        /// object as not being fully initialized and throw a corresponding (and most
        /// probably fatal) exception.
        /// </note>
        /// </remarks>
        public virtual object GetObject()
        {
            return jobDetail;
        }

        /// <summary>
        /// Return the <see cref="System.Type"/> of object that this
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
        /// <see langword="null"/> if not known in advance.
        /// </summary>
        /// <value></value>
        public virtual Type ObjectType
        {
            get
            {
                if (targetObjectName != null)
                {
                    if (objectFactory == null)
                    {
                        throw new InvalidOperationException("ObjectFactory must be set when using 'TargetObjectName'");
                    }

                    return objectFactory.GetType(targetObjectName);
                }

                return typeof(IJobDetail);
            }
        }

        /// <summary>
        /// Is the object managed by this factory a singleton or a prototype?
        /// </summary>
        /// <value></value>
        public virtual bool IsSingleton => true;

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
            Prepare();

            // Use specific name if given, else fall back to object name.
            string jobDetailName = name ?? objectName;

            // Consider the concurrent flag to choose between stateful and stateless job.
            Type jobType = (concurrent ? typeof(MethodInvokingJob) : typeof(StatefulMethodInvokingJob));

            // Build JobDetail instance.
            jobDetail = new JobDetailImpl(jobDetailName, group, jobType);
            jobDetail.JobDataMap.Put("methodInvoker", this);
            jobDetail.Durable = true;

            // job listeners through configuration are no longer supported
            if (jobListenerNames != null && jobListenerNames.Length > 0)
            {
                throw new InvalidOperationException("Non-global IJobListeners not supported on Quartz 2 - " +
                                                    "manually register a Matcher against the Quartz ListenerManager instead");
            }

            PostProcessJobDetail(jobDetail);
        }

        /// <summary>
        /// Callback for post-processing the JobDetail to be exposed by this FactoryObject.
        /// <p>
        /// The default implementation is empty. Can be overridden in subclasses.
        /// </p>
        /// </summary>
        /// <param name="detail">the JobDetail prepared by this FactoryObject</param>
        protected virtual void PostProcessJobDetail(IJobDetail detail)
        {
        }
    }
}
