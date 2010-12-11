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
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Aop.Support
{
    /// <summary>
    /// Abstract ObjectFactory-based IPointcutAdvisor that allows for any Advice to be
    /// configured as reference to an Advice object in an ObjectFactory.
    /// </summary>
    /// <remarks>
    /// specifying the name of an advice object instead of the advice object itself
    /// (if running within an ObjectFactory/ApplicationContext increses loose coupling
    /// at initialization time, in order not to initialize the advice object until the
    /// pointcut actually matches.
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack</author>
    public abstract class AbstractObjectFactoryPointcutAdvisor : AbstractPointcutAdvisor, IObjectFactoryAware
    {
        private string adviceObjectName;

        private IObjectFactory objectFactory;

        private IAdvice advice;

        private object adviceMonitor = new object();


        /// <summary>
        /// Gets or sets the name of the advice object that this advisor should refer to.
        /// </summary>
        /// <remarks>An instance of the specified object will be obtained on first access of
        /// this advisor's advice.  This advisor will only ever obtain at most one
        /// single instance of the advice object, caching the instance for the lifetime of
        /// the advisor.</remarks>
        /// <value>The name of the advice object.</value>
        public string AdviceObjectName
        {
            get { return adviceObjectName; }
            set { adviceObjectName = value; }
        }

        #region IObjectFactoryAware Members

        /// <summary>
        /// Callback that supplies the owning factory to an object instance.
        /// </summary>
        /// <value>
        /// Owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// (may not be <see langword="null"/>). The object can immediately
        /// call methods on the factory.
        /// </value>
        /// <remarks>
        /// <p>
        /// Invoked after population of normal object properties but before an init
        /// callback like <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// method or a custom init-method.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of initialization errors.
        /// </exception>
        public IObjectFactory ObjectFactory
        {
            set { objectFactory = value; }
        }

        #endregion

        /// <summary>
        /// Return the advice part of this aspect.
        /// </summary>
        /// <remarks>
        /// <p>
        /// An advice may be an interceptor, a throws advice, before advice,
        /// introduction etc.
        /// </p>
        /// </remarks>
        /// <returns>
        /// The advice that should apply if the pointcut matches.
        /// </returns>
        public override IAdvice Advice
        {
            get
            {
                lock (adviceMonitor)
                {
                    if (advice == null && adviceObjectName != null)
                    {
                        AssertUtils.State(objectFactory != null,
                                          "ObjectFactory must be set to resolve 'adviceObjectName'");
                        advice = objectFactory.GetObject(adviceObjectName, typeof (IAdvice)) as IAdvice;                        
                    }
                }
                return advice;
            }
            set
            {
                advice = value;
            }
        }

        /// <summary>
        /// Describe this Advisor, showing name of advice object.
        /// </summary>
        /// <returns>
        /// Type name and advice object name.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + ": advice object '" + AdviceObjectName + "'";
        }
    }
}