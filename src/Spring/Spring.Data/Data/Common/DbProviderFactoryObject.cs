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

using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Data.Common
{
    /// <summary>
    /// A <see cref="Spring.Objects.Factory.IFactoryObject"/> implementation that
    /// creates instances of the <see cref="IDbProvider"/> class.
    /// </summary>
    /// <remarks>Typically used as a convenience for retrieving shared
    /// <see cref="IDbProvider"/> in a Spring XML configuration file as compared to
    /// using explict factory method support.
    /// </remarks>
    public class DbProviderFactoryObject : IFactoryObject, IInitializingObject
    {
        #region Fields

        private string provider;
        private string connectionString;
        private IDbProvider dbProvider;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of the <see cref="DbProviderFactoryObject"/> class.
        /// </summary>
        public DbProviderFactoryObject()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the database provider.
        /// </summary>
        /// <value>The name of the database provider.</value>
        public string Provider
        {
            get { return provider; }
            set
            {
                AssertUtils.ArgumentHasText(value, "The 'Provider' property must have a value.");
                provider = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString
        {
            get { return connectionString; }
            set
            {
                AssertUtils.ArgumentHasText(value, "The 'ConnectionString' property must have a value.");
                connectionString = value.Trim();
            }
        }

        #endregion

        #region IFactoryObject Members

        /// <summary>
        /// Return an instance of and IDbProvider as configured by this factory
        /// managed by this factory.
        /// </summary>
        /// <returns>
        /// The same (singleton) instance of the IDbProvider managed by
        /// this factory.
        /// </returns>
        /// <remarks>
        /// 	<note type="caution">
        /// If this method is being called in the context of an enclosing IoC container and
        /// returns <see langword="null"/>, the IoC container will consider this factory
        /// object as not being fully initialized and throw a corresponding (and most
        /// probably fatal) exception.
        /// </note>
        /// </remarks>
        public virtual object GetObject()
        {
            lock (this)
            {
                if (dbProvider == null)
                {
                    ValidateProperties();
                    dbProvider = CreateProviderInstance();
                }

                return dbProvider;
            }
        }

        /// <summary>
        /// Create the actual provider instance as specified by this factory's configuration properties.
        /// </summary>
        /// <returns>the fully configured provider</returns>
        protected virtual IDbProvider CreateProviderInstance()
        {
            IDbProvider providerInstance = DbProviderFactory.GetDbProvider(Provider);
            if (connectionString != null)
            {
                providerInstance.ConnectionString = this.connectionString;
            }
            return providerInstance;
        }

        /// <summary>
        /// Return the type of <see cref="IDbProvider"/>
        /// </summary>
        public virtual Type ObjectType
        {
            get { return typeof (IDbProvider); }
        }

        /// <summary>
        /// Returns true, as the the object managed by this factory is a singleton.
        /// </summary>
        /// <value></value>
        public virtual bool IsSingleton
        {
            get { return true; }
        }

        #endregion

        #region IInitializingObject Memebers

        /// <summary>
        /// Validates that the provider name is specified.
        /// </summary>
        /// <remarks>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has injected all of an object's dependencies.
        /// </remarks>
        /// <exception cref="System.ArgumentException">
        /// In the event of not setting the ProviderName.
        /// </exception>
        public virtual void AfterPropertiesSet()
        {
            ValidateProperties();
        }

        #endregion

        #region Private Memebers

        /// <summary>
        /// Validates the properties.
        /// </summary>
        private void ValidateProperties()
        {
            if (StringUtils.IsNullOrEmpty(Provider))
            {
                throw new ArgumentException("The 'DbProviderName' property has not been set.");
            }
        }

        #endregion
    }
}
