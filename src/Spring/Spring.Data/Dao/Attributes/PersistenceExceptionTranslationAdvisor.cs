#region License

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

#endregion

using AopAlliance.Aop;
using Spring.Aop;
using Spring.Aop.Support;
using Spring.Dao.Support;
using Spring.Objects.Factory;

namespace Spring.Dao.Attributes
{
    /// <summary>
    /// Spring AOP exception translation aspect for use at Repository or DAO layer level.
    /// Translates native persistence exceptions into Spring's DataAccessException hierarchy,
    /// based on a given PersistenceExceptionTranslator.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack  (.NET)</author>
    public class PersistenceExceptionTranslationAdvisor : AbstractPointcutAdvisor
    {
        private PersistenceExceptionTranslationInterceptor advice;

	    private AttributeMatchingPointcut pointcut;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceExceptionTranslationAdvisor"/> class.
        /// </summary>
        /// <param name="persistenceExceptionTranslator">The persistence exception translator to use.</param>
        /// <param name="repositoryAttributeType">Type of the repository attribute to check for.</param>
        public PersistenceExceptionTranslationAdvisor(IPersistenceExceptionTranslator persistenceExceptionTranslator,
            Type repositoryAttributeType)
        {
            this.advice = new PersistenceExceptionTranslationInterceptor(persistenceExceptionTranslator);
            this.pointcut = new AttributeMatchingPointcut(repositoryAttributeType, true);
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceExceptionTranslationAdvisor"/> class.
        /// </summary>
        /// <param name="objectFactory">The object factory to obtain all IPersistenceExceptionTranslators from.</param>
        /// <param name="repositoryAttributeType">Type of the repository attribute to check for.</param>
        public PersistenceExceptionTranslationAdvisor(IListableObjectFactory objectFactory, Type repositoryAttributeType)
        {
            this.advice = new PersistenceExceptionTranslationInterceptor(objectFactory);
            this.pointcut = new AttributeMatchingPointcut(repositoryAttributeType, true);
        }

        /// <summary>
        /// Return the advice part of this aspect.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// 	<p>
        /// An advice may be an interceptor, a throws advice, before advice,
        /// introduction etc.
        /// </p>
        /// </remarks>
        /// <returns>
        /// The advice that should apply if the pointcut matches.
        /// </returns>
        public override IAdvice Advice
        {
            get { return this.advice; }
            set {  }
        }

        /// <summary>
        /// The <see cref="Spring.Aop.IPointcut"/> that drives this advisor.
        /// </summary>
        /// <value></value>
        public override IPointcut Pointcut
        {
            get { return this.pointcut;  }
            set { }
        }
    }
}
