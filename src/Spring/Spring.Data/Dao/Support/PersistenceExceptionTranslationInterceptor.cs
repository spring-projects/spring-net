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

using AopAlliance.Intercept;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Dao.Support
{
    /// <summary>
    /// AOP MethodInterceptor that provides persistence exception translation
    /// based on a given PersistenceExceptionTranslator.
    /// </summary>
    /// <remarks>
    /// Delegates to the given <see cref="IPersistenceExceptionTranslator"/> to translate
    /// an Exception thrown into Spring's DataAccessException hierarchy
    /// (if appropriate).
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class PersistenceExceptionTranslationInterceptor : IMethodInterceptor, IObjectFactoryAware, IInitializingObject
    {
        private IPersistenceExceptionTranslator persistenceExceptionTranslator;


        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceExceptionTranslationInterceptor"/> class.
        /// Needs to be configured with a PersistenceExceptionTranslator afterwards.
        /// </summary>
        public PersistenceExceptionTranslationInterceptor()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceExceptionTranslationInterceptor"/> class for the
        /// given IPersistenceExceptionTranslator
        /// </summary>
        /// <param name="persistenceExceptionTranslator">The persistence exception translator to use.</param>
        public PersistenceExceptionTranslationInterceptor(IPersistenceExceptionTranslator persistenceExceptionTranslator)
        {
            this.persistenceExceptionTranslator = persistenceExceptionTranslator;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceExceptionTranslationInterceptor"/> class, autodetecting
        /// IPersistenceExceptionTranslators in the given object factory.
        /// </summary>
        /// <param name="objectFactory">The object factory to obtain all IPersistenceExceptionTranslators from.</param>
        public PersistenceExceptionTranslationInterceptor(IListableObjectFactory objectFactory)
        {
            this.persistenceExceptionTranslator = DetectPersistenceExceptionTranslators(objectFactory);
        }


        /// <summary>
        /// Sets the persistence exception translator.  The default is to autodetect all IPersistenceExceptionTranslators
        /// in the containing object factory, using them in a chain.
        /// </summary>
        /// <value>The persistence exception translator.</value>
        public IPersistenceExceptionTranslator PersistenceExceptionTranslator
        {
            set
            {
                AssertUtils.ArgumentNotNull(value, "IPersistenceExceptionTranslator must not be null");
                persistenceExceptionTranslator = value;
            }
        }


        /// <summary>
        /// Callback that supplies the owning factory to an object instance.
        /// </summary>
        /// <value>
        /// Owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// (may not be <see langword="null"/>). The object can immediately
        /// call methods on the factory.
        /// </value>
        /// <remarks>
        /// 	<p>
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
            set
            {
                if (this.persistenceExceptionTranslator == null)
                {
                    // No explicit exception translator specified - perform autodetection.
                    IListableObjectFactory owningFactory = value as IListableObjectFactory;
                    if (owningFactory == null)
                    {
                        throw new ArgumentException("Cannot use IPersistenceExceptionTranslator autodetection without IListableObjectFactory");
                    }
                    this.persistenceExceptionTranslator = DetectPersistenceExceptionTranslators(owningFactory);
                }
            }
        }

        /// <summary>
        /// Ensures that the property PersistenceExceptionTranslator has been set.
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has injected all of an object's dependencies.
        /// </summary>
        /// <exception cref="System.Exception">
        /// In the event of misconfiguration (such as the failure to set a
        /// required property) or if initialization fails.
        /// </exception>
        public void AfterPropertiesSet()
        {
            if (this.persistenceExceptionTranslator == null)
            {
                throw new ArgumentException("Property 'PersistenceExceptionTranslator' is required");
            }
        }

        /// <summary>
        /// Detects the petsistence exception translators in the given object factory.
        /// </summary>
        /// <param name="objectFactory">The object factory for obtaining all IPersistenceExceptionTranslators.</param>
        /// <returns>A chained IPersistenceExceptionTranslator, combining all PersistenceExceptionTranslators found in the factory
        /// </returns>
        /// <seealso cref="ChainedPersistenceExceptionTranslator"/>
        protected IPersistenceExceptionTranslator DetectPersistenceExceptionTranslators(IListableObjectFactory objectFactory)
        {
            // Find all translators, being careful not to activate FactoryObjects.
            IDictionary<string, object> pets =
                ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(objectFactory,
                                                                   typeof (IPersistenceExceptionTranslator), false,
                                                                   false);
            if (pets.Count == 0)
            {
                throw new InvalidOperationException("No persistence exception translators found in container. Cannot perform exception translation.");
            }

            ChainedPersistenceExceptionTranslator cpet = new ChainedPersistenceExceptionTranslator();
            foreach (KeyValuePair<string, object> pet in pets)
            {
                cpet.AddTranslator((IPersistenceExceptionTranslator)pet.Value);
            }
            return cpet;
        }

        /// <summary>
        /// Return a translated exception if this is appropriate, otherwise rethrow the original exception.
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        public object Invoke(IMethodInvocation invocation)
        {
            try
            {
                return invocation.Proceed();
            } catch (Exception ex)
            {
                DataAccessException dex = DataAccessUtils.TranslateIfNecessary(ex, persistenceExceptionTranslator);
                if (dex == null)
                {
                    throw;
                } else
                {
                    throw dex;
                }
            }
        }
    }
}
