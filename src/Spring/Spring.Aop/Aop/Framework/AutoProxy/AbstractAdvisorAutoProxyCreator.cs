#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using Common.Logging;

using Spring.Core;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// Abstract IObjectPostProcessor implementation that creates AOP proxies.
    /// This class is completely generic; it contains no special code to handle
    /// any particular aspects, such as pooling aspects.
    /// </summary>
    /// <remarks>
    /// <p>Subclasses must implement the abstract FindCandidateAdvisors() method
    /// to return a list of Advisors applying to any object. Subclasses can also
    /// override the inherited ShouldSkip() method to exclude certain objects
    /// from autoproxying, but they must be careful to invoke the ShouldSkip()
    /// method of this class, which tries to avoid circular reference problems
    /// and infinite loops.</p>
    /// <p>Advisors or advices requiring ordering should implement the Ordered interface.
    /// This class sorts advisors by Ordered order value. Advisors that don't implement
    /// the Ordered interface will be considered to be unordered, and will appear
    /// at the end of the advisor chain in undefined order.</p>
    /// </remarks>
    /// <seealso cref="Spring.Aop.Framework.AutoProxy.AbstractAdvisorAutoProxyCreator.FindCandidateAdvisors"/>
    /// <author>Rod Johnson</author>
    /// <author>Adhari C Mahendra (.NET)</author>
    /// <author>Erich Eichinger</author>
    public abstract class AbstractAdvisorAutoProxyCreator : AbstractAutoProxyCreator
    {
        private readonly ILog Log;
        private IAdvisorRetrievalHelper _advisorRetrievalHelper;

        /// <summary>
        /// Initialize
        /// </summary>
        protected AbstractAdvisorAutoProxyCreator()
        {
            Log = LogManager.GetLogger(this.GetType());
        }

        /// <summary>
        /// We override this method to ensure that all candidate advisors are materialized
        /// under a stack trace including this object. Otherwise, the dependencies won't
        /// be apparent to the circular-reference prevention strategy in AbstractObjectFactory.
        /// </summary>
        public override IObjectFactory ObjectFactory
        {
            set
            {
                if (!(value is IConfigurableListableObjectFactory))
                {
                    throw new InvalidOperationException("Can not use AdvisorAutoProxyCreator without a ConfigurableListableObjectFactory");
                }
                base.ObjectFactory = value;
                InitObjectFactory((IConfigurableListableObjectFactory)value);
            }
        }

        /// <summary>
        /// An new <see cref="IConfigurableListableObjectFactory"/> was set. Initialize this creator instance
        /// according to the specified object factory.
        /// </summary>
        /// <param name="objectFactory"></param>
        protected virtual void InitObjectFactory(IConfigurableListableObjectFactory objectFactory)
        {
            _advisorRetrievalHelper = CreateAdvisorRetrievalHelper(objectFactory);
        }

        /// <summary>
        /// Create the <see cref="IAdvisorRetrievalHelper"/> for retrieving the list of
        /// applicable advisor objects. The default implementation calls back into
        /// <see cref="IsEligibleAdvisorObject"/> thus it usually is sufficient to just
        /// override <see cref="IsEligibleAdvisorObject"/>. Override <see cref="CreateAdvisorRetrievalHelper"/>
        /// only if you know what you are doing!
        /// </summary>
        /// <param name="objectFactory"></param>
        /// <returns></returns>
        protected virtual IAdvisorRetrievalHelper CreateAdvisorRetrievalHelper(IConfigurableListableObjectFactory objectFactory)
        {
            return new ObjectFactoryAdvisorRetrievalHelperAdapter(this, objectFactory);
        }

        /// <summary>
        /// Return whether the given object is to be proxied, what additional
        /// advices (e.g. AOP Alliance interceptors) and advisors to apply.
        /// </summary>
        /// <remarks>
        /// 	<p>The previous targetName of this method was "GetInterceptorAndAdvisorForObject".
        /// It has been renamed in the course of general terminology clarification
        /// in Spring 1.1. An AOP Alliance Interceptor is just a special form of
        /// Advice, so the generic Advice term is preferred now.</p>
        /// 	<p>The third parameter, customTargetSource, is new in Spring 1.1;
        /// add it to existing implementations of this method.</p>
        /// </remarks>
        /// <param name="targetType">the type of the target object</param>
        /// <param name="targetName">the name of the target object</param>
        /// <param name="customTargetSource">targetSource returned by TargetSource property:
        ///   may be ignored. Will be null unless a custom target source is in use.</param>
        /// <returns>
        /// an array of additional interceptors for the particular object;
        /// or an empty array if no additional interceptors but just the common ones;
        /// or null if no proxy at all, not even with the common interceptors.
        /// </returns>
        protected override IList<object> GetAdvicesAndAdvisorsForObject(Type targetType, string targetName, ITargetSource customTargetSource)
        {
            IList<IAdvisor> advisors = FindEligibleAdvisors(targetType, targetName);
            if (advisors.Count == 0)
            {
                return DO_NOT_PROXY;
            }
            return advisors.Cast<object>().ToArray();
        }

        /// <summary>
        /// Find all eligible advices and for autoproxying this class.
        /// </summary>
        /// <param name="targetType">the type of the object to be advised</param>
        /// <param name="targetName">the name of the object to be advised</param>
        /// <returns>
        /// the empty list, not null, if there are no pointcuts or interceptors.
        /// The by-order sorted list of advisors otherwise
        /// </returns>
        protected IList<IAdvisor> FindEligibleAdvisors(Type targetType, string targetName)
        {
            List<IAdvisor> candidateAdvisors = FindCandidateAdvisors(targetType, targetName);
            List<IAdvisor> eligibleAdvisors = FindAdvisorsThatCanApply(candidateAdvisors, targetType, targetName);

            ExtendAdvisors(eligibleAdvisors, targetType, targetName);
            SortAdvisors(eligibleAdvisors);

            return eligibleAdvisors;
        }

        /// <summary>
        /// Find all possible advisor candidates to use in auto-proxying
        /// </summary>
        /// <param name="targetType">the type of the object to be advised</param>
        /// <param name="targetName">the name of the object to be advised</param>
        /// <returns>the list of candidate advisors</returns>
        protected virtual List<IAdvisor> FindCandidateAdvisors(Type targetType, string targetName)
        {
            return _advisorRetrievalHelper.FindAdvisorObjects(targetType, targetName);
        }

        /// <summary>
        /// From the given list of candidate advisors, select the ones that are applicable
        /// to the given target specified by targetType and name.
        /// </summary>
        /// <param name="candidateAdvisors">the list of candidate advisors to date</param>
        /// <param name="targetType">the target object's type</param>
        /// <param name="targetName">the target object's name</param>
        /// <returns>the list of applicable advisors</returns>
        protected virtual List<IAdvisor> FindAdvisorsThatCanApply(List<IAdvisor> candidateAdvisors, Type targetType, string targetName)
        {
            if (candidateAdvisors.Count==0)
            {
                return candidateAdvisors;
            }

            List<IAdvisor> eligibleAdvisors = new List<IAdvisor>();
            foreach(IAdvisor candidate in candidateAdvisors)
            {
                if (candidate is IIntroductionAdvisor && AopUtils.CanApply(candidate, targetType, null))
                {
                    if (logger.IsInfoEnabled)
                    {
                        logger.Info($"Candidate advisor [{candidate}] accepted for targetType [{targetType}]");
                    }
                    eligibleAdvisors.Add(candidate);
                }
            }

            bool hasIntroductions = eligibleAdvisors.Count > 0;
            foreach(IAdvisor candidate in candidateAdvisors)
            {
                if (candidate is IIntroductionAdvisor) continue;

                if (AopUtils.CanApply(candidate, targetType, null, hasIntroductions))
                {
                    if (logger.IsInfoEnabled)
                    {
                        logger.Info($"Candidate advisor [{candidate}] accepted for targetType [{targetType}]");
                    }
                    eligibleAdvisors.Add(candidate);
                }
                else
                {
                    if (logger.IsInfoEnabled)
                    {
                        logger.Info($"Candidate advisor [{candidate}] rejected for targetType [{targetType}]");
                    }
                }
            }
            return eligibleAdvisors;
        }


        /// <summary>
        /// Sorts the advisors.
        /// </summary>
        /// <param name="advisors">The advisors.</param>
        /// <returns></returns>
        protected virtual void SortAdvisors(List<IAdvisor> advisors)
        {
            advisors.Sort(OrderComparator<IAdvisor>.Instance);
        }

        /// <summary>
        /// Extension hook that subclasses can override to add additional advisors for the given object,
        /// given the sorted advisors obtained to date.<br/>
        /// The default implementation does nothing.<br/>
        /// Typically used to add advisors that expose contextual information required by some of the later advisors.
        /// </summary>
        /// <remarks>
        /// The advisor list passed into this method is already reduced to advisors applying to this particular object.
        /// If you want to register additional common advisor candidates, override <see cref="FindCandidateAdvisors"/>.
        /// </remarks>
        /// <param name="advisors">Advisors that have already been identified as applying to a given object</param>
        /// <param name="objectType">the type of the object to be advised</param>
        /// <param name="objectName">the name of the object to be advised</param>
        protected virtual void ExtendAdvisors(IList<IAdvisor> advisors, Type objectType, string objectName)
        {}

        /// <summary>
        /// Whether the given advisor is eligible for the specified target. The default implementation
        /// always returns true.
        /// </summary>
        /// <param name="advisorName">the advisor name</param>
        /// <param name="targetType">the target object's type</param>
        /// <param name="targetName">the target object's name</param>
        protected virtual bool IsEligibleAdvisorObject(string advisorName, Type targetType, string targetName)
        {
            return true;
        }

        private class ObjectFactoryAdvisorRetrievalHelperAdapter : ObjectFactoryAdvisorRetrievalHelper
        {
            private readonly AbstractAdvisorAutoProxyCreator _owner;

            public ObjectFactoryAdvisorRetrievalHelperAdapter(AbstractAdvisorAutoProxyCreator owner, IConfigurableListableObjectFactory owningFactory) : base(owningFactory)
            {
                _owner = owner;
            }

            protected override bool IsEligibleObject(string advisorName, Type objectType, string objectName)
            {
                return  base.IsEligibleObject(advisorName, objectType, objectName)
                        && _owner.IsEligibleAdvisorObject(advisorName, objectType, objectName);
            }
        }
    }
}
