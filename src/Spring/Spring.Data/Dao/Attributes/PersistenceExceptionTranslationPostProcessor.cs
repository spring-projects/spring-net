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

using Spring.Aop.Framework;
using Spring.Core;
using Spring.Dao.Support;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Stereotype;
using Spring.Util;

namespace Spring.Dao.Attributes
{
    /// <summary>
    /// Object post-processor that automatically applies persistence exception
    /// translation to any bean that carries the <see cref="RepositoryAttribute"/>
    /// attribute, adding a corresponding <see cref="PersistenceExceptionTranslationAdvisor"/>
    /// to the exposed proxy (either an existing AOP proxy or a newly generated
    /// proxy that implements all of the target's interfaces).
    /// </summary>
    /// <remarks>
    /// <para>Translates native resource exceptions to Spring's <see cref="Spring.Dao.DataAccessException"/>
    /// hierarchy. Autodetects object that implement the <see cref="IPersistenceExceptionTranslator"/>
    /// interface, which are subsequently asked to translate candidate exceptions.
    /// </para>
    /// <para>All of Spring's applicable resource factories implement the 
    /// <code>IPersistenceExceptionTranslator</code> interface out of the box.
    /// As a consequence, all that is usually needed to enable automatic exception
    /// translation is marking all affected objects (such as DAOs) with the
    /// <code>Repository</code> annotation, along with defining this post-processor
    /// in the application context.
    /// </para>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <seealso cref="PersistenceExceptionTranslationAdvisor"/>
    /// <seealso cref="RepositoryAttribute"/>
    /// <seealso cref="DataAccessException"/>
    /// <seealso cref="IPersistenceExceptionTranslator"/>
    public class PersistenceExceptionTranslationPostProcessor : ProxyConfig, IObjectPostProcessor, IObjectFactoryAware, IOrdered
    {
        private Type repositoryAttributeType = typeof(RepositoryAttribute);

        private PersistenceExceptionTranslationAdvisor persistenceExceptionTranslationAdvisor;


        /// <summary>
        /// Sets the type of the repository attribute.  The default required attribute type is the 
        /// <see cref="RepositoryAttribute"/> attirbute.  This setter property exists so that developers
        /// can provide their own (non-Spring-specific) attribute type to indicate that a class has a 
        /// repository role.
        /// </summary>
        /// <value>The desitred type of the repository attribute.</value>
        public Type RepositoryAttributeType
        {
            set
            {
                AssertUtils.ArgumentNotNull(value, "'RepositoryAttributeType' must not be null");
                repositoryAttributeType = value;
            }
        }

        /// <summary>
        /// Callback that supplies the owning factory to an object instance.
        /// </summary>
        public IObjectFactory ObjectFactory
        {
            set
            {
                IListableObjectFactory lof = value as IListableObjectFactory;
                if (lof == null)
                {
                    throw new ArgumentException("Cannot use PersistenceExceptionTranslator autodetection without IListableObjectFactory");
                }
                this.persistenceExceptionTranslationAdvisor =
                    new PersistenceExceptionTranslationAdvisor(lof, this.repositoryAttributeType);  
            }
        }

        public int Order
        {
            get { 
                return Int32.MaxValue; 
                // lowest precidence value
                // This should run after all other post-processors, so that it can just add
                // an advisor to existing proxies rather than double-proxy.
            }
        }

        /// <summary>
        /// Just return the passed in object instance
        /// </summary>
        /// <param name="instance">The new object instance.</param>
        /// <param name="name">The name of the object.</param>
        /// <returns>
        /// The passed in object instance
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        public object PostProcessBeforeInitialization(object instance, string name)
        {
            return instance;
        }

        /// <summary>
        /// Add PersistenceExceptionTranslationAdvice to candidate object if it is a match.
        /// Create AOP proxy if necessary or add advice to existing advice chain.
        /// </summary>
        /// <param name="instance">The new object instance.</param>
        /// <param name="objectName">The name of the object.</param>
        /// <returns>
        /// The object instance to use, wrapped with either the original or a wrapped one.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        public object PostProcessAfterInitialization(object instance, string objectName)
        {
            IAdvised advised = instance as IAdvised;
            Type targetType;
            if (advised != null)
            {
                targetType = advised.TargetSource.TargetType;
            } else
            {
                targetType = instance.GetType();
            }
            if (targetType == null)
            {
                // Can't do much here
                return instance;
            }

            if (AopUtils.CanApply(this.persistenceExceptionTranslationAdvisor, targetType, ReflectionUtils.GetInterfaces(targetType)))                
            {
                if (advised != null)
                {
                    advised.AddAdvisor(this.persistenceExceptionTranslationAdvisor);
                    return instance;
                }
                else
                {
                    ProxyFactory proxyFactory = new ProxyFactory(instance);
                    // copy our properties inherited from ProxyConfig
                    proxyFactory.CopyFrom(this);
                    proxyFactory.AddAdvisor(this.persistenceExceptionTranslationAdvisor);
                    return proxyFactory.GetProxy();
                }
            } else
            {
                return instance;
            }
        }
    }
}
