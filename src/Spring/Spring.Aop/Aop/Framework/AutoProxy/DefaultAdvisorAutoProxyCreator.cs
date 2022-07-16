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

#region Imports

using Spring.Objects.Factory;

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
    /// <author>Erich Eichinger (.NET)</author>
    public class DefaultAdvisorAutoProxyCreator : AbstractAdvisorAutoProxyCreator, IObjectNameAware, IInitializingObject
    {
        /// <summary>
        /// Separator between prefix and remainder of object name
        /// </summary>
        public const string Separator = ".";

        private bool usePrefix;
        private string advisorObjectNamePrefix;
        private List<IAdvisor> cachedAdvisors;

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
                    advisorObjectNamePrefix = value + Separator;
                }
            }
        }

        #endregion

        /// <summary>
        /// Find all possible advisor candidates to use in auto-proxying
        /// </summary>
        /// <param name="targetType">the type of the object to be advised</param>
        /// <param name="targetName">the name of the object to be advised</param>
        /// <returns>the list of candidate advisors</returns>
        protected override List<IAdvisor> FindCandidateAdvisors(Type targetType, string targetName)
        {
            if (cachedAdvisors == null) {
                cachedAdvisors = base.FindCandidateAdvisors(targetType, targetName);
            }
            return cachedAdvisors;
        }

        /// <summary>
        /// Whether the given advisor is eligible for the specified target.
        /// </summary>
        /// <param name="advisorName">the advisor name</param>
        /// <param name="targetType">the target object's type</param>
        /// <param name="targetName">the target object's name</param>
        protected override bool IsEligibleAdvisorObject(string advisorName, Type targetType, string targetName)
        {
            return (!usePrefix || advisorName.StartsWith(advisorObjectNamePrefix))
                && base.IsEligibleAdvisorObject(advisorName, targetType, targetName);
        }

        /// <summary>
        /// Validate configuration
        /// </summary>
        public virtual void AfterPropertiesSet()
        {
            // eagerly resolve advisors at this stage already to prevent circular dep problems.
            // TODO (EE): fix instantiation process to make test "AdvisorAutoProxyCreatorCircularReferencesTests" work.
            cachedAdvisors = base.FindCandidateAdvisors(null, null);
        }
    }
}
