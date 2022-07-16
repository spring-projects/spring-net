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
using Spring.Core.IO;
using Spring.Util;

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Abstract base class for object definition readers.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Provides common properties like the object registry to work on.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	public abstract class AbstractObjectDefinitionReader : IObjectDefinitionReader
	{
		#region Constants

		/// <summary>
		/// The <see cref="Common.Logging.ILog"/> instance for this class (and derived classes).
		/// </summary>
		protected readonly ILog log;

		#endregion

		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinitionReader"/>
		/// class.
		/// </summary>
		/// <param name="registry">
		/// The <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry"/>
		/// instance that this reader works on.
		/// </param>
		/// <remarks>
		/// <p>
		/// This is an <see langword="abstract"/> class, and as such exposes no public constructors.
		/// </p>
		/// </remarks>
		protected AbstractObjectDefinitionReader(IObjectDefinitionRegistry registry)
			: this(registry, AppDomain.CurrentDomain)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinitionReader"/>
		/// class.
		/// </summary>
		/// <param name="registry">
		/// The <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry"/>
		/// instance that this reader works on.
		/// </param>
		/// <param name="domain">
		/// The <see cref="System.AppDomain"/> against which any class names
		/// will be resolved into <see cref="System.Type"/> instances.
		/// </param>
		/// <remarks>
		/// <p>
		/// This is an <see langword="abstract"/> class, and as such exposes no public constructors.
		/// </p>
		/// </remarks>
		protected AbstractObjectDefinitionReader(
			IObjectDefinitionRegistry registry,
			AppDomain domain)
		{
		    log = LogManager.GetLogger(this.GetType());

		    AssertUtils.ArgumentNotNull(registry, "registry", "IObjectDefinitionRegistry must not be null");
			_registry = registry;
			_domain = domain;
            if (registry is IResourceLoader)
            {
                _resourceLoader = registry as IResourceLoader;
            }
            else
            {
                _resourceLoader = new ConfigurableResourceLoader();
            }
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the
		/// <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry"/>
		/// instance that this reader works on.
		/// </summary>
		public IObjectDefinitionRegistry Registry
		{
			get { return _registry; }
		}


        /// <summary>
        /// The <see cref="IObjectNameGenerator"/> to use for anonymous
        /// objects (wihtout explicit object name specified).
        /// </summary>
        /// <value></value>
	    public IObjectNameGenerator ObjectNameGenerator
	    {
	        get { return _objectNameGenerator; }
	        set
	        {
                if (value != null)
                {
                    _objectNameGenerator = value;
                }
                else
                {
                    _objectNameGenerator = new DefaultObjectNameGenerator();
                }
	        }
	    }

	    /// <summary>
		/// The <see cref="System.AppDomain"/> against which any class names
		/// will be resolved into <see cref="System.Type"/> instances.
		/// </summary>
		public AppDomain Domain
		{
			get { return _domain; }
		}


        /// <summary>
        /// Gets or sets the resource loader to use for resource locations.
        /// </summary>
        /// <value>The resource loader.</value>
	    public IResourceLoader ResourceLoader
	    {
	        get { return _resourceLoader; }
	        set { _resourceLoader = value; }
	    }

	    #endregion

		#region Methods

		/// <summary>
		/// Load object definitions from the supplied <paramref name="resource"/>.
		/// </summary>
		/// <param name="resource">
		/// The resource for the object definitions that are to be loaded.
		/// </param>
		/// <returns>
		/// The number of object definitions that were loaded.
		/// </returns>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// In the case of loading or parsing errors.
		/// </exception>
		public abstract int LoadObjectDefinitions(IResource resource);

	    /// <summary>
	    /// Load object definitions from the supplied <paramref name="resources"/>.
	    /// </summary>
	    /// <param name="resources">
	    /// The resources for the object definitions that are to be loaded.
	    /// </param>
	    /// <returns>
	    /// The number of object definitions found
	    /// </returns>
	    /// <exception cref="Spring.Objects.ObjectsException">
	    /// In the case of loading or parsing errors.
	    /// </exception>
	    public int LoadObjectDefinitions(IResource[] resources)
	    {
	        AssertUtils.ArgumentNotNull(resources, "resources");
	        int counter = 0;
	        foreach (IResource resource in resources)
	        {
	            counter += LoadObjectDefinitions(resource);
	        }
	        return counter;
	    }

        /// <summary>
        /// Loads the object definitions from the specified resource location.
        /// </summary>
        /// <param name="location">The resource location, to be loaded with the
        /// IResourceLoader location .</param>
        /// <returns>
        /// The number of object definitions found
        /// </returns>
        public int LoadObjectDefinitions(string location)
	    {
            if (ResourceLoader == null)
            {
                throw new ObjectDefinitionStoreException("Cannot import object definitions from location [" + location +
                                                         "]:" +
                                                         " no ResourceLoader available");
            }
            IResource resource;
            try
            {
                resource = ResourceLoader.GetResource(location);
            } catch (Exception e)
            {
                throw new ObjectDefinitionStoreException("Could not resolve resource location [" + location + "]",e);
            }
            int loadCount = LoadObjectDefinitions(resource);

            if (log.IsDebugEnabled)
            {
                log.Debug("Loaded " + loadCount + " object definitions from location [" + location + "]");
            }
            return loadCount;
	    }


	    /// <summary>
	    /// Loads the object definitions from the specified resource locations.
	    /// </summary>
	    /// <param name="locations">The the resource locations to be loaded with the
	    /// IResourceLoader of this object definition reader.</param>
	    /// <returns>
	    /// The number of object definitions found
	    /// </returns>
	    public int LoadObjectDefinitions(string[] locations)
	    {
            AssertUtils.ArgumentNotNull(locations, "location");
            int counter = 0;
            foreach (string location in locations)
            {
                counter += LoadObjectDefinitions(location);
            }
            return counter;
	    }

	    #endregion

		#region Fields

		private IObjectDefinitionRegistry _registry;
		private AppDomain _domain;
	    private IResourceLoader _resourceLoader;
	    private IObjectNameGenerator _objectNameGenerator = new DefaultObjectNameGenerator();

		#endregion
	}
}
