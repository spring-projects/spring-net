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

using System.Collections;
using Common.Logging;
using Commons.Collections;
using NVelocity.App;
using NVelocity.Exception;
using NVelocity.Runtime;
using NVelocity.Runtime.Resource.Loader;
using Spring.Context.Support;
using Spring.Core.IO;
using Spring.Util;

namespace Spring.Template.Velocity {
    /// <summary>
    /// Factory that configures a VelocityEngine. Can be used standalone,
    /// but typically you will use VelocityEngineFactoryObject
    /// for preparing a VelocityEngine as bean reference.
    ///
    /// <br/>
    /// The optional "ConfigLocation" property sets the location of the Velocity
    /// properties file, within the current application. Velocity properties can be
    /// overridden via "VelocityProperties", or even completely specified locally,
    /// avoiding the need for an external properties file.
    ///
    /// <br/>
    /// The "ResourceLoaderPath" property can be used to specify the Velocity
    /// resource loader path via Spring's IResource abstraction, possibly relative
    /// to the Spring application context.
    ///
    /// <br/>
    /// If "OverrideLogging" is true (the default), the VelocityEngine will be
    /// configured to log via Commons Logging, that is, using the Spring-provided
    /// CommonsLoggingLogSystem as log system.
    ///
    /// <br/>
    /// The simplest way to use this class is to specify a ResourceLoaderPath
    /// property. the VelocityEngine typically then does not need any further
    /// configuration.
    ///
    /// </summary>
    /// <see cref="CommonsLoggingLogSystem" />
    /// <see cref="VelocityEngineFactoryObject" />
    /// <see cref="CommonsLoggingLogSystem" />
    /// <author>Erez Mazor</author>
    public class VelocityEngineFactory
    {

        /// <summary>
        /// Shared logger instance.
        /// </summary>
        protected static readonly ILog log = LogManager.GetLogger(typeof(VelocityEngineFactory));

        private IResource configLocation;

        private IDictionary<string, object> velocityProperties = new Dictionary<string, object>();

        private IList<string> resourceLoaderPaths = new List<string>();

        private IResourceLoader resourceLoader = new ConfigurableResourceLoader();

        private bool preferFileSystemAccess = true;

        private bool overrideLogging = true;

        /// <summary>
        ///  Set the location of the Velocity config file. Alternatively, you can specify all properties locally.
        /// </summary>
        /// <see cref="VelocityProperties"/>
        /// <see cref="ResourceLoaderPath"/>
        public IResource ConfigLocation {
            set { configLocation = value; }
        }

        /// <summary>
        /// Set local NVelocity properties.
        /// </summary>
        /// <see cref="VelocityProperties"/>
        public IDictionary<string, object> VelocityProperties {
            set { velocityProperties = value; }
        }

        /// <summary>
        /// Single ResourceLoaderPath
        /// </summary>
        /// <see cref="ResourceLoaderPaths"/>
        public string ResourceLoaderPath {
            set { resourceLoaderPaths.Add(value); }
        }

        /// <summary>
        /// Set the Velocity resource loader path via a Spring resource location.
        /// Accepts multiple locations in Velocity's comma-separated path style.
        /// <br/>
        /// When populated via a String, standard URLs like "file:" and "assembly:"
        /// pseudo URLs are supported, as understood by IResourceLoader. Allows for
        /// relative paths when running in an ApplicationContext.
        /// <br/>
        /// Will define a path for the default Velocity resource loader with the name
        /// "file". If the specified resource cannot be resolved to a File,
        /// a generic SpringResourceLoader will be used under the name "spring", without
        /// modification detection.
        /// <br/>
        /// Take notice that resource caching will be enabled in any case. With the file
        /// resource loader, the last-modified timestamp will be checked on access to
        /// detect changes. With SpringResourceLoader, the resource will be throughout
        /// the life time of the application context (for example for class path resources).
        /// <br/>
        /// To specify a modification check interval for files, use Velocity's
        /// standard "file.resource.loader.modificationCheckInterval" property. By default,
        /// the file timestamp is checked on every access (which is surprisingly fast).
        /// Of course, this just applies when loading resources from the file system.
        /// <br/>
        /// To enforce the use of SpringResourceLoader, i.e. to not resolve a path
        /// as file system resource in any case, turn off the "preferFileSystemAccess"
        /// flag. See the latter's documentation for details.
        /// </summary>
        ///  <see cref="ResourceLoader"/>
        ///  <see cref="VelocityProperties"/>
        ///  <see cref="PreferFileSystemAccess"/>
        ///  <see cref="SpringResourceLoader"/>
        ///  <see cref="FileResourceLoader"/>
        public IList<string> ResourceLoaderPaths
        {
            set { resourceLoaderPaths = value; }
        }

        /// <summary>
        /// Set the Spring ResourceLoader to use for loading Velocity template files.
        /// The default is DefaultResourceLoader. Will get overridden by the
        /// ApplicationContext if running in a context.
        ///
        /// </summary>
        /// <see cref="ConfigurableResourceLoader"/>
        /// <see cref="ContextRegistry"/>
        ///
        public IResourceLoader ResourceLoader {
            get { return resourceLoader; }
            set { resourceLoader = value; }
        }

        /// <summary>
        /// Set whether to prefer file system access for template loading.
        /// File system access enables hot detection of template changes.
        /// <br/>
        /// If this is enabled, VelocityEngineFactory will try to resolve the
        /// specified "resourceLoaderPath" as file system resource.
        /// <br/>
        /// Default is "true". Turn this off to always load via SpringResourceLoader
        /// (i.e. as stream, without hot detection of template changes), which might
        /// be necessary if some of your templates reside in a directory while
        /// others reside in assembly files.
        /// </summary>
        /// <see cref="ResourceLoaderPath"/>
        public bool PreferFileSystemAccess {
            get { return preferFileSystemAccess; }
            set { preferFileSystemAccess = value; }
        }

        /// <summary>
        /// Set whether Velocity should log via Commons Logging, i.e. whether Velocity's
        /// log system should be set to CommonsLoggingLogSystem. Default value is true
        /// </summary>
        /// <see cref="CommonsLoggingLogSystem"/>
        public bool OverrideLogging {
            get { return overrideLogging; }
            set { overrideLogging = value; }
        }


        /// <summary>
        ///  Create and initialize the VelocityEngine instance and return it
        /// </summary>
        /// <returns>VelocityEngine</returns>
        /// <exception cref="VelocityException" />
        /// <see cref="FillProperties" />
        /// <see cref="InitVelocityResourceLoader" />
        /// <see cref="PostProcessVelocityEngine" />
        /// <see cref="VelocityEngine.Init()" />
        public VelocityEngine CreateVelocityEngine()
        {
            ExtendedProperties extendedProperties = new ExtendedProperties();
            VelocityEngine velocityEngine = NewVelocityEngine();

            LoadDefaultProperties(extendedProperties);

            // Load config file if set.
            if (configLocation != null) {
                if (log.IsInfoEnabled) {
                    log.Info(string.Format("Loading Velocity config from [{0}]", configLocation));
                }
                FillProperties(extendedProperties, configLocation, false);
            }

            // merge local properties if set.
            if (velocityProperties.Count > 0) {
                foreach (KeyValuePair<string, object> pair in velocityProperties) {
                    extendedProperties.SetProperty(pair.Key, pair.Value);
                }
            }

            // Set a resource loader path, if required.
            if (!preferFileSystemAccess && resourceLoaderPaths.Count == 0) {
                throw new ArgumentException("When using SpringResourceLoader you must provide a path using the ResourceLoaderPath property");
            }

            if (resourceLoaderPaths.Count > 0) {
                InitVelocityResourceLoader(velocityEngine, extendedProperties, resourceLoaderPaths);
            }

            // Log via Commons Logging?
            if (overrideLogging) {
                velocityEngine.SetProperty(RuntimeConstants.RUNTIME_LOG_LOGSYSTEM, new CommonsLoggingLogSystem());
            }

            PostProcessVelocityEngine(velocityEngine);

            try {
                velocityEngine.Init(extendedProperties);
            } catch (Exception ex) {
                throw new VelocityException(ex.ToString(), ex);
            }

            return velocityEngine;
        }

        /// <summary>
        /// This is to overcome an issue with the current NVelocity library, it seems the
        /// default runetime properties/directives (nvelocity.properties and directive.properties
        /// files) are not being properly located in  the library at load time. A jira should
        /// be filed but for now we attempt to do this on our own. Particularly our
        /// concern here is with several required properties which I don't want
        /// to require users to re-defined. e.g.,:
        /// <br/>
        ///
        /// Pre-requisites:<br/>
        /// resource.manager.class=NVelocity.Runtime.Resource.ResourceManagerImpl <br/>
        /// directive.manager=NVelocity.Runtime.Directive.DirectiveManager <br/>
        /// runtime.introspector.uberspect=NVelocity.Util.Introspection.UberspectImpl <br/>
        /// </summary>
        private static void LoadDefaultProperties(ExtendedProperties extendedProperties) {
            IResource defaultRuntimeProperties = new AssemblyResource("assembly://NVelocity/NVelocity.Runtime.Defaults/nvelocity.properties");
            IResource defaultRuntimeDirectives = new AssemblyResource("assembly://NVelocity/NVelocity.Runtime.Defaults/directive.properties");
            FillProperties(extendedProperties, defaultRuntimeProperties, true);
            FillProperties(extendedProperties, defaultRuntimeDirectives, true);
        }

        /// <summary>
        ///  Return a new VelocityEngine. Subclasses can override this for
        /// custom initialization, or for using a mock object for testing. <br/>
        /// Called by CreateVelocityEngine()
        /// </summary>
        /// <returns>VelocityEngine instance (non-configured)</returns>
        /// <see cref="CreateVelocityEngine"/>
        protected static VelocityEngine NewVelocityEngine()
        {
            return new VelocityEngine();
        }

        /// <summary>
        /// Initialize a Velocity resource loader for the given VelocityEngine:
        /// either a standard Velocity FileResourceLoader or a SpringResourceLoader.
        /// <br/>Called by <code>CreateVelocityEngine()</code>.
        /// </summary>
        /// <param name="velocityEngine">velocityEngine the VelocityEngine to configure</param>
        /// <param name="extendedProperties"></param>
        /// <param name="paths">paths the path list to load Velocity resources from</param>
        /// <see cref="FileResourceLoader"/>
        /// <see cref="SpringResourceLoader"/>
        /// <see cref="InitSpringResourceLoader"/>
        /// <see cref="CreateVelocityEngine"/>
        protected void InitVelocityResourceLoader(VelocityEngine velocityEngine, ExtendedProperties extendedProperties, IList<string> paths)
        {

            if (PreferFileSystemAccess) {
                // Try to load via the file system, fall back to SpringResourceLoader
                // (for hot detection of template changes, if possible).
                IList<string> resolvedPaths = new List<string>();
                try {
                    foreach (string path in paths) {
                        IResource resource = ResourceLoader.GetResource(path);
                        resolvedPaths.Add(resource.File.FullName);
                    }

                    extendedProperties.SetProperty(RuntimeConstants.RESOURCE_LOADER, VelocityConstants.File);
                    extendedProperties.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH,
                        StringUtils.CollectionToCommaDelimitedString(resolvedPaths));
                } catch (IOException ex) {
                    if (log.IsDebugEnabled) {
                        log.Error(string.Format("Cannot resolve resource loader path [{0}] to [File]: using SpringResourceLoader",
                            StringUtils.CollectionToCommaDelimitedString(resolvedPaths)), ex);
                    }

                    InitSpringResourceLoader(velocityEngine, extendedProperties, StringUtils.CollectionToCommaDelimitedString(paths));
                }
            } else {
                // Always load via SpringResourceLoader (without hot detection of template changes).
                if (log.IsDebugEnabled) {
                    log.Debug("File system access not preferred: using SpringResourceLoader");
                }
                InitSpringResourceLoader(velocityEngine, extendedProperties, StringUtils.CollectionToCommaDelimitedString(paths));
            }
        }

        /// <summary>
        ///  Initialize a SpringResourceLoader for the given VelocityEngine.
        /// <br/>Called by <code>InitVelocityResourceLoader</code>.
        ///
        /// <b>Important</b>: the NVeloctity ResourceLoaderFactory.getLoader
        /// method replaces ';' with ',' when attempting to construct our resource
        /// loader. The name on the SPRING_RESOURCE_LOADER_CLASS property
        /// has to be in the form of "ClassFullName; AssemblyName" in replacement
        /// of the tranditional "ClassFullName, AssemblyName" to work.
        /// </summary>
        /// <param name="velocityEngine">velocityEngine the VelocityEngine to configure</param>
        /// <param name="extendedProperties"></param>
        /// <param name="resourceLoaderPathString">resourceLoaderPath the path to load Velocity resources from</param>
        /// <see cref="SpringResourceLoader"/>
        /// <see cref="InitVelocityResourceLoader"/>
        protected void InitSpringResourceLoader(VelocityEngine velocityEngine, ExtendedProperties extendedProperties, string resourceLoaderPathString)
        {
            extendedProperties.SetProperty(RuntimeConstants.RESOURCE_LOADER, SpringResourceLoader.NAME);
            Type springResourceLoaderType = typeof(SpringResourceLoader);
            string springResourceLoaderTypeName = springResourceLoaderType.FullName + "; " + springResourceLoaderType.Assembly.GetName().Name;
            extendedProperties.SetProperty(SpringResourceLoader.SPRING_RESOURCE_LOADER_CLASS, springResourceLoaderTypeName);
            velocityEngine.SetApplicationAttribute(SpringResourceLoader.SPRING_RESOURCE_LOADER, ResourceLoader);
            velocityEngine.SetApplicationAttribute(SpringResourceLoader.SPRING_RESOURCE_LOADER_PATH, resourceLoaderPathString);
        }

        /// <summary>
        /// To be implemented by subclasses that want to to perform custom
        /// post-processing of the VelocityEngine after this FactoryObject
        /// performed its default configuration (but before VelocityEngine.init)
        /// <br/>
        /// Called by CreateVelocityEngine
        /// </summary>
        /// <param name="velocityEngine">velocityEngine the current VelocityEngine</param>
        /// <exception cref="IOException" />
        /// <see cref="CreateVelocityEngine"/>
        /// <see cref="VelocityEngine.Init()"/>
        protected virtual void PostProcessVelocityEngine(VelocityEngine velocityEngine)
        {
        }

        /// <summary>
        /// Populates the velocity properties from the given resource
        /// </summary>
        /// <param name="extendedProperties">ExtendedProperties instance to populate</param>
        /// <param name="resource">The resource from which to load the properties</param>
        /// <param name="append">A flag indicated weather the properties loaded from the resource should be appended or replaced in the extendedProperties</param>
        private static void FillProperties(ExtendedProperties extendedProperties, IInputStreamSource resource, bool append) {
            try {
                if (append) {
                    extendedProperties.Load(resource.InputStream);
                } else {
                    ExtendedProperties overrides = new ExtendedProperties();
                    overrides.Load(resource.InputStream);
                    foreach (DictionaryEntry entry in overrides) {
                        extendedProperties.SetProperty(Convert.ToString(entry.Key), entry.Value);
                    }
                }
            } finally {
                resource.InputStream.Close();
            }
        }
    }
}
