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
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;

#endregion

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// ObjectPostProcessor implementation that creates AOP proxies based on all candidate
    /// Advisors in the current IObjectFactory. This class is completely generic; it contains
    /// no special code to handle any particular aspects, such as pooling aspects.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Adhari C Mahendra (.NET)</author>
    public class DefaultAdvisorAutoProxyCreator : AbstractAdvisorAutoProxyCreator, IObjectNameAware, IInitializingObject
    {

        /// <summary>
        /// Separator between prefix and remainder of object name
        /// </summary>
        public static readonly string SEPARATOR = ".";
        private bool usePrefix;
        private string advisorObjectNamePrefix;
        private IList advisors;

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to exclude
        /// advisors with a certain prefix.
        /// </summary>
        /// <value><c>true</c> if [use prefix]; otherwise, <c>false</c>.</value>
        public bool UsePrefix
        {
            get { return usePrefix; }
            set { usePrefix = value; }
        }

        /// <summary>
        /// Set the prefix for object names that will cause them to be included for
        /// auto-proxying by this object. This prefix should be set to avoid circular
        /// references. Default value is the object name of this object + a dot.
        /// </summary>
        /// <value>The advisor object name prefix.</value>
        public string AdvisorObjectNamePrefix
        {
            get { return advisorObjectNamePrefix; }
            set { advisorObjectNamePrefix = value; }
        }

        #endregion
        /// <summary>
        /// Find all candidate advices to use in auto proxying.
        /// </summary>
        /// <returns>list of Advice</returns>
        protected override IList FindCandidateAdvisors()
        {
            if (advisors == null)
            {
                throw new InvalidOperationException("Must not be called before AfterPropertiesSet()");
            }
            if (logger.IsDebugEnabled)
            {
                logger.Debug(string.Format("returning available advisors"));
            }
            return advisors;
        }

        private IList InstantiateCandidateAdvisors()
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(string.Format("instantiating available advisors"));
            }

            //This is ensured in AbstractAdvisorAutoProxyCreator.  Will be more type safe once sync with Spring Java 2.x
            IConfigurableListableObjectFactory owningFactory = ObjectFactory as IConfigurableListableObjectFactory;
            if (owningFactory == null)
            {
                throw new InvalidOperationException("Cannot use DefaultAdvisorAutoProxyCreator without a IListableObjectFactory");
            }

            ArrayList candidateAdvisors = new ArrayList();

            string[] advisorNames = ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(
                owningFactory, typeof(IAdvisor), true, false);
            for (int i = 0; i < advisorNames.Length; i++)
            {
                string name = advisorNames[i];
                if ( (!usePrefix || name.StartsWith(advisorObjectNamePrefix)) && !owningFactory.IsCurrentlyInCreation(name))
                {
                    try
                    {
                        IAdvisor advisor = (IAdvisor) owningFactory.GetObject(name);
                        candidateAdvisors.Add(advisor);
                    } catch (ObjectCreationException ex)
                    {
                        Exception rootEx = ex.GetBaseException();

                        
                        if (rootEx is ObjectCurrentlyInCreationException)
                        {
                            ObjectCurrentlyInCreationException oce = (ObjectCurrentlyInCreationException) rootEx;
                            if (owningFactory.IsCurrentlyInCreation(oce.ObjectName))
                            {
                                if (logger.IsDebugEnabled)
                                {
                                    logger.Debug(string.Format("Ignoring currently created advisor '{0}': exception message = {1}",
                                        name, ex.Message));
                                }
                                continue;
                            }                                                      
                        }
                        throw;
                    }
                }
            }

            string[] aspectNames = ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(
                owningFactory, typeof(IAdvisors), true, false);
            for (int i = 0; i < aspectNames.Length; i++)
            {
                string name = aspectNames[i];
                if (!usePrefix || name.StartsWith(advisorObjectNamePrefix))
                {
                    IAdvisors advisors = (IAdvisors)owningFactory.GetObject(name);
                    candidateAdvisors.AddRange(advisors.Advisors);
                }
            }

            return candidateAdvisors;
        }



        /// <summary>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has injected all of an object's dependencies.
        /// </summary>
        public void AfterPropertiesSet()
        {
            advisors = InstantiateCandidateAdvisors();
        }

        #region IObjectNameAware Members

        /// <summary>
        /// Set the name of the object in the object factory that created this object.
        /// </summary>
        /// <value>The name of the object in the factory.</value>
        /// <remarks>
        /// 	<p>
        /// Invoked after population of normal object properties but before an init
        /// callback like <see cref="T:Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="M:Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// method or a custom init-method.
        /// </p>
        /// </remarks>
        public string ObjectName
        {
            set
            {
                // If no infrastructure object name prefix has been set, override it.
                if (advisorObjectNamePrefix == null)
                {
                    advisorObjectNamePrefix = value + SEPARATOR;
                }
            }
        }

        #endregion
    }
}