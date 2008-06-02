#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

#region Imports

using System;
using System.Collections;
using Spring.Aop.Framework.DynamicProxy;
using Spring.Core;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;

#endregion

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// Abstract IOBjectPostProcessor implementation that creates AOP proxies.
    /// This class is completely generic; it contains no special code to handle
    /// any particular aspects, such as pooling aspects.
    /// </summary>
    /// <remarks>
    /// <p>Subclasses must implement the abstract findCandidateAdvisors() method
    /// to return a list of Advisors applying to any object. Subclasses can also
    /// override the inherited shouldSkip() method to exclude certain objects
    /// from autoproxying, but they must be careful to invoke the shouldSkip()
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
    public abstract class AbstractAdvisorAutoProxyCreator : AbstractAutoProxyCreator
    {
        /// <summary>
        /// We override this method to ensure that all candidate advisors are materialized
        /// under a stack trace including this object. Otherwise, the dependencies won't
        /// be apparent to the circular-reference prevention strategy in AbstractObjectFactory.
        /// </summary>
        public override IObjectFactory ObjectFactory
        {
            //TODO investigate override...
            set
            {
                base.ObjectFactory = value;
                if (!(value is IConfigurableListableObjectFactory))
                {
                    throw new InvalidOperationException(
                        "Can not use AdvisorAutoProxyCreator without a ConfigurableListableObjectFactory");
                }
            }
            get { return base.ObjectFactory; }
        }

        /// <summary>
        /// Return whether the given object is to be proxied, what additional
        /// advices (e.g. AOP Alliance interceptors) and advisors to apply.
        /// </summary>
        /// <param name="objType">the new object instance</param>
        /// <param name="name">the name of the object</param>
        /// <param name="customTargetSource">targetSource returned by TargetSource property:
        /// may be ignored. Will be null unless a custom target source is in use.</param>
        /// <returns>
        /// an array of additional interceptors for the particular object;
        /// or an empty array if no additional interceptors but just the common ones;
        /// or null if no proxy at all, not even with the common interceptors.
        /// </returns>
        /// <remarks>
        /// 	<p>The previous name of this method was "GetInterceptorAndAdvisorForObject".
        /// It has been renamed in the course of general terminology clarification
        /// in Spring 1.1. An AOP Alliance Interceptor is just a special form of
        /// Advice, so the generic Advice term is preferred now.</p>
        /// 	<p>The third parameter, customTargetSource, is new in Spring 1.1;
        /// add it to existing implementations of this method.</p>
        /// </remarks>
        protected override object[] GetAdvicesAndAdvisorsForObject(Type objType, string name, ITargetSource customTargetSource)
        {
            IList advisors = FindEligibleAdvisors(objType);
            if (advisors.Count == 0)
            {
                return DO_NOT_PROXY;
            }
            advisors = SortAdvisors(advisors);
            if (advisors is ArrayList)
                return ((ArrayList) advisors).ToArray();
            else
            {
                return advisors as object[];
            }
        }

        /// <summary>
        /// Find all eligible advices and for autoproxying this class.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>the empty list, not null, if there are no pointcuts or interceptors</returns>
        protected IList FindEligibleAdvisors(Type type)
        {
            IList candidateAdvisors = FindCandidateAdvisors();
            IList eligibleAdvisors = new ArrayList();
            for (int i = 0; i < candidateAdvisors.Count; i++)
            {
                IAdvisor candidate = (IAdvisor) candidateAdvisors[i];
                if (AopUtils.CanApply(candidate, type, null))
                {
                    eligibleAdvisors.Add(candidate);
                    if (logger.IsInfoEnabled)
                    {
                        logger.Info(string.Format("Candidate advisor [{0}] accepted for type [{1}]", candidate, type.ToString()));
                    }
                }
                else
                {
                    if (logger.IsInfoEnabled)
                    {
                        logger.Info(string.Format("Candidate advisor [{0}] rejected for type [{1}]", candidate, type.ToString()));
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
        protected IList SortAdvisors(IList advisors)
        {
            if (advisors is ArrayList)
                ((ArrayList) advisors).Sort(new OrderComparator());
            else if (advisors is Array)
                Array.Sort((Array) advisors, new OrderComparator());
            return advisors;
        }

        /// <summary>
        /// Find all candidate advisors to use in auto-proxying.
        /// </summary>
        /// <returns>list of Advisors</returns>
        protected abstract IList FindCandidateAdvisors();
    }
}