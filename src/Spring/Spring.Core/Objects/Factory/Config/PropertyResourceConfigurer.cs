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

using System.Collections.Specialized;
using System.Globalization;
using Common.Logging;
using Spring.Core;
using Spring.Core.IO;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Allows for the configuration of individual object property values from
    /// a .NET .config file.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Useful for custom .NET .config files targetted at system administrators
    /// that override object properties configured in the application context.
    /// </p>
    /// <p>
    /// Two concrete implementations are provided in the Spring.NET core library:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <see cref="Spring.Objects.Factory.Config.PropertyOverrideConfigurer"/>
    /// for <c>&lt;add key="placeholderKey" value="..."/&gt;</c> style
    /// overriding (pushing values from a .NET .config file into object
    /// definitions).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="Spring.Objects.Factory.Config.PropertyPlaceholderConfigurer"/>
    /// for replacing "${...}" placeholders (pulling values from a .NET .config
    /// file into object definitions).
    /// </description>
    /// </item>
    /// </list>
    /// </p>
    /// <p>
    /// Please refer to the API documentation for the concrete implementations
    /// listed above for example usage.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Simon White (.NET)</author>
    /// <seealso cref="Spring.Objects.Factory.Config.PropertyOverrideConfigurer"/>
    /// <seealso cref="Spring.Objects.Factory.Config.PropertyPlaceholderConfigurer"/>
    [Serializable]
    public abstract class PropertyResourceConfigurer
        : IObjectFactoryPostProcessor, IOrdered
    {
        /// <summary>
        /// The default configuration section name to use if none is explictly supplied.
        /// </summary>
        /// <seealso cref="Spring.Objects.Factory.Config.PropertyResourceConfigurer.ConfigSections"/>
        public static readonly string DefaultConfigSectionName = "spring-config";

        #region Fields

        private readonly ILog _log;

        private int _order = Int32.MaxValue; // default: same as non-Ordered
        private NameValueCollection _defaultProperties;
        private IResource[] _locations;
        private string[] _configSections;
        private bool _ignoreResourceNotFound = false;
        private bool _lastLocationOverrides = true;

        #endregion

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Config.PropertyResourceConfigurer"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes no
        /// public constructors.
        /// </p>
        /// </remarks>
        protected PropertyResourceConfigurer()
        {
            _log = LogManager.GetLogger(this.GetType());
        }

        #endregion

        #region Properties

        /// <summary>
        /// The policy for resolving conflicting property overrides from
        /// several resources.
        /// </summary>
        /// <remarks>
        /// <p>
        /// When merging conflicting property overrides from several resources,
        /// should append an override with the same key be appended to the
        /// current value, or should the property override from the last resource
        /// processed override previous values?
        /// </p>
        /// <p>
        /// The default value is <see langword="true"/>; i.e. a property
        /// override from the last resource to be processed overrides previous
        /// values.
        /// </p>
        /// </remarks>
        /// <value>
        /// <see langword="true"/> if the property override from the last resource
        /// processed overrides previous values.
        /// </value>
        public bool LastLocationOverrides
        {
            set { _lastLocationOverrides = value; }
        }

        /// <summary>
        /// Return the order value of this object, where a higher value means greater in
        /// terms of sorting.
        /// </summary>
        /// <returns>The order value.</returns>
        /// <seealso cref="Spring.Core.IOrdered.Order"/>
        public int Order
        {
            get { return _order; }
            set { _order = value; }
        }

        /// <summary>
        /// The default properties to be applied.
        /// </summary>
        /// <remarks>
        /// <p>
        /// These are to be considered defaults, to be overridden by values
        /// loaded from other resources.
        /// </p>
        /// </remarks>
        public NameValueCollection Properties
        {
            get { return _defaultProperties; }
            set { _defaultProperties = value; }
        }

        /// <summary>
        /// The location of the .NET .config file that contains the property
        /// overrides that are to be applied.
        /// </summary>
        public IResource Location
        {
            set { _locations = new IResource[] {value}; }
        }

        /// <summary>
        /// The locations of the .NET .config files containing the property
        /// overrides that are to be applied.
        /// </summary>
        public IResource[] Locations
        {
            set { _locations = value; }
        }

        /// <summary>
        /// The configuration sections to look for within the .config files.
        /// </summary>
        /// <seealso cref="Spring.Objects.Factory.Config.PropertyResourceConfigurer.Location"/>
        /// <seealso cref="Spring.Objects.Factory.Config.PropertyResourceConfigurer.Locations"/>
        public string[] ConfigSections
        {
            get
            {
                if (_configSections == null
                    || _configSections.Length == 0)
                {
                    _configSections = new string[] {DefaultConfigSectionName};
                }
                return _configSections;
            }
            set { _configSections = value; }
        }

        /// <summary>
        /// Should a failure to find a .config file be ignored?
        /// </summary>
        /// <remarks>
        /// <p>
        /// <see langword="true"/> is only appropriate if the .config file is
        /// completely optional. The default is <see langword="false"/>.
        /// </p>
        /// </remarks>
        /// <value>
        /// <see langword="true"/> if a failure to find a .config file is to be
        /// ignored.
        /// </value>
        public bool IgnoreResourceNotFound
        {
            set { _ignoreResourceNotFound = value; }
        }

        #endregion

        /// <summary>
        /// Modify the application context's internal object factory after its
        /// standard initialization.
        /// </summary>
        /// <param name="factory">
        /// The object factory used by the application context.
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor.PostProcessObjectFactory(IConfigurableListableObjectFactory)"/>
        public void PostProcessObjectFactory(IConfigurableListableObjectFactory factory)
        {
            try
            {
                NameValueCollection properties = new NameValueCollection();
                InitializeWithDefaultProperties(properties);
                LoadProperties(properties);
                ProcessProperties(factory, properties);
            }
            catch (Exception ex)
            {
                if (typeof(ObjectsException).IsInstanceOfType(ex))
                {
                    throw;
                }
                else
                {
                    throw new ObjectsException(
                        "Errored while postprocessing an object factory.", ex);
                }
            }
        }

        /// <summary>
        /// Loads properties from the configuration sections
        /// specified in <see cref="ConfigSections"/> into <paramref name="properties"/>.
        /// </summary>
        /// <param name="properties">The <see cref="NameValueCollection"/> instance to be filled with properties.</param>
        protected virtual void LoadProperties(NameValueCollection properties)
        {
            string[] configSections = ConfigSections;
            if (_locations != null)
            {
                ValidateConfigSections(configSections);
                bool usingMultipleConfigSections = configSections.Length > 1;
                int sectionNameIndex = 0;
                foreach (IResource resource in _locations)
                {
                    #region Instrumentation

                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug(string.Format(
                                       CultureInfo.InvariantCulture,
                                       "Loading configuration from '{0}'.", resource));
                    }

                    #endregion

                    string sectionName = configSections[sectionNameIndex];
                    if (resource is ConfigSectionResource)
                    {
                        ConfigurationReader.PopulateFromAppConfig(
                            properties, sectionName, _lastLocationOverrides);
                    }
                    else
                    {
                        if (resource.Exists)
                        {
                            ConfigurationReader.Read(
                                resource, sectionName, properties, _lastLocationOverrides);
                        }
                        else
                        {
                            string errorMessage = "Could not load configuration from " + resource;
                            if (_ignoreResourceNotFound)
                            {
                                #region Instrumentation

                                if (_log.IsWarnEnabled)
                                {
                                    _log.Warn(errorMessage);
                                }

                                #endregion
                            }
                            else
                            {
                                throw new ObjectInitializationException(errorMessage);
                            }
                        }
                    }
                    if (usingMultipleConfigSections)
                    {
                        ++sectionNameIndex;
                    }
                }
            }
            else
            {
                foreach (string sectionName in configSections)
                {
                    ConfigurationReader.PopulateFromAppConfig(
                        properties, sectionName, _lastLocationOverrides);
                }
            }
        }

        /// <summary>
        /// Apply the given properties to the supplied
        /// <see cref="Spring.Objects.Factory.Config.IConfigurableListableObjectFactory"/>.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="Spring.Objects.Factory.Config.IConfigurableListableObjectFactory"/>
        /// used by the application context.
        /// </param>
        /// <param name="props">The properties to apply.</param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If an error occured.
        /// </exception>
        protected abstract void ProcessProperties(
            IConfigurableListableObjectFactory factory,
            NameValueCollection props);

        /// <summary>
        /// Validates the supplied <paramref name="configSections"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Basically, if external locations are specified, ensure that either
        /// one or a like number of config sections are also specified.
        /// </p>
        /// </remarks>
        /// <param name="configSections">
        /// The <paramref name="configSections"/> to be validated.
        /// </param>
        private void ValidateConfigSections(string[] configSections)
        {
            if (_locations.Length != configSections.Length)
            {
                // if only one config section is specified for all locations that's cool...
                if (configSections.Length != 1)
                {
                    throw new ObjectInitializationException(
                        "Invalid number of config sections specified.");
                }
            }
        }

        /// <summary>
        /// Simply initializes the supplied <paramref name="properties"/>
        /// collection with this instances default <see cref="Properties"/>
        /// (if any).
        /// </summary>
        /// <param name="properties">
        /// The collection to be so initialized.
        /// </param>
        private void InitializeWithDefaultProperties(NameValueCollection properties)
        {
            if (Properties != null)
            {
                properties.Add(Properties);
            }
        }
    }
}
