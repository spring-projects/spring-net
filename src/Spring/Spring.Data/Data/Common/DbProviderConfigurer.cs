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

using Common.Logging;
using Spring.Core;
using Spring.Core.IO;
using Spring.Objects.Factory.Config;

namespace Spring.Data.Common
{
    /// <summary>
    ///
    /// </summary>
    /// <author>Mark Pollack</author>
    public class DbProviderConfigurer : IObjectFactoryPostProcessor, IOrdered
    {
        #region Fields

        private static readonly ILog log = LogManager.GetLogger(typeof(DbProviderConfigurer));

        private int order = Int32.MinValue;

        private IResource providerResource;

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DbProviderConfigurer()
        {
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the provider resource which contains additional IDbProvider definitions.
        /// </summary>
        /// <value>The provider resource.</value>
        public IResource ProviderResource
        {
            get { return providerResource; }
            set { providerResource = value; }
        }

        #endregion

        #region Implementation of IObjectFactoryPostProcessor

        /// <summary>
        /// Modify the application context's internal object factory after its
        /// standard initialization.
        /// </summary>
        /// <param name="factory">The object factory used by the application context.</param>
        /// <remarks>
        /// 	<p>
        /// All object definitions will have been loaded, but no objects will have
        /// been instantiated yet. This allows for overriding or adding properties
        /// even to eager-initializing objects.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        public void PostProcessObjectFactory(IConfigurableListableObjectFactory factory)
        {
            DbProviderFactory.DBPROVIDER_ADDITIONAL_RESOURCE_NAME = providerResource.Uri.AbsoluteUri;
        }

        #endregion

        #region Implementation of IOrdered

        public int Order
        {
            get { return order; }
        }

        #endregion
    }

}
