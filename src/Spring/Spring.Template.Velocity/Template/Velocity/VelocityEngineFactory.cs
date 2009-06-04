#region License

/*
 * Copyright 2002-2009 the original author or authors.
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common.Logging;
using Commons.Collections;
using NVelocity.App;
using NVelocity.Exception;
using NVelocity.Runtime;
using NVelocity.Runtime.Resource.Loader;
using Spring.Context.Support;
using Spring.Core.IO;

namespace Spring.Template.Velocity{
    /// <summary>
    /// Factory that configures a VelocityEngine. Can be used standalone,
    /// but typically you will either use {@link VelocityEngineFactoryBean}
    /// for preparing a VelocityEngine as bean reference.
    ///
    /// <p>The optional "ConfigLocation" property sets the location of the Velocity
    /// properties file, within the current application. Velocity properties can be
    /// overridden via "VelocityProperties", or even completely specified locally,
    /// avoiding the need for an external properties file.
    ///
    /// <p>The "ResourceLoaderPath" property can be used to specify the Velocity
    /// resource loader path via Spring's Resource abstraction, possibly relative
    /// to the Spring application context.
    ///
    /// <p>If "OverrideLogging" is true (the default), the VelocityEngine will be
    /// configured to log via Commons Logging, that is, using the Spring-provided
    /// "CommonsLoggingLogSystem" as log system.
    ///
    /// <p>The simplest way to use this class is to specify a ResourceLoaderPath 
    /// property; the VelocityEngine typically then does not need any further 
    /// configuration.
    ///
    /// </summary>
    /// <see cref="CommonsLoggingLogSystem" />
    /// <see cref="VelocityEngineFactoryObject" />
    /// <see cref="CommonsLoggingLogSystem" />
    /// <author>Erez Mazor (.NET) </author>
    public class VelocityEngineFactory {
        private const char DELIMITER = ',';
        protected static readonly ILog log = LogManager.GetLogger(typeof(VelocityEngineFactory));

        private IResource configLocation;

        private IDictionary<string, object> velocityProperties = new Dictionary<string, object>();

        private String resourceLoaderPath;

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
        /// Set Velocity properties as Map, to allow for non-String values like "ds.resource.loader.instance".
        /// </summary>
        /// <see cref="VelocityProperties"/>
        public IDictionary<string, object> VelocityProperties {
            set { velocityProperties = value; }
        }

        /// <summary>
        /// 
        /// Set the Velocity resource loader path via a Spring resource location.
        /// Accepts multiple locations in Velocity's comma-separated path style.
        /// <p>When populated via a String, standard URLs like "file:" and "assembly:"
        /// pseudo URLs are supported, as understood by ResourceLoader. Allows for
        /// relative paths when running in an ApplicationContext.
        /// <p>Will define a path for the default Velocity resource loader with the name
        /// "file". If the specified resource cannot be resolved to a <code>java.io.File</code>,
        /// a generic SpringResourceLoader will be used under the name "spring", without
        /// modification detection.
        /// <p> Take notice that resource caching will be enabled in any case. With the file 
        /// resource loader, the last-modified timestamp will be checked on access to
        /// detect changes. With SpringResourceLoader, the resource will be cached
        /// forever (for example for class path resources).
        /// <p>To specify a modification check interval for files, use Velocity's
        /// standard "file.resource.loader.modificationCheckInterval" property. By default,
        /// the file timestamp is checked on every access (which is surprisingly fast).
        /// Of course, this just applies when loading resources from the file system.
        /// <p>To enforce the use of SpringResourceLoader, i.e. to not resolve a path
        /// as file system resource in any case, turn off the "preferFileSystemAccess"
        /// flag. See the latter's javadoc for details. 
        /// </summary>
        ///  <see cref="ResourceLoader"/>
        ///  <see cref="VelocityProperties"/>
        ///  <see cref="PreferFileSystemAccess"/>
        ///  <see cref="SpringResourceLoader"/>
        ///  <see cref="FileResourceLoader"/>
        public string ResourceLoaderPath {
            set { resourceLoaderPath = value; }
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
        /// <p>If this is enabled, VelocityEngineFactory will try to resolve the
        /// specified "resourceLoaderPath" as file system resource (which will work
        /// for expanded class path resources and ServletContext resources too).
        /// <p>Default is "true". Turn this off to always load via SpringResourceLoader
        /// (i.e. as stream, without hot detection of template changes), which might
        /// be necessary if some of your templates reside in an expanded classes
        /// directory while others reside in jar files.
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
        ///    Prepare the VelocityEngine instance and return it.
        /// </summary>
        /// <returns>VelocityEngine</returns>
        /// <exception cref="VelocityException" />
        /// <see cref="FillProperties" />
        /// <see cref="InitVelocityResourceLoader" />
        /// <see cref="PostProcessVelocityEngine" />
        /// <see cref="VelocityEngine.Init()" />
        public VelocityEngine CreateVelocityEngine() {
            VelocityEngine velocityEngine = NewVelocityEngine();
            ExtendedProperties props = new ExtendedProperties();
            // Load config file if set.
            if (configLocation != null) {
                if (log.IsInfoEnabled) {
                    log.Info(string.Format("Loading Velocity config from [{0}]", configLocation));
                }
                FillProperties(props, configLocation);
            }

            // Merge local properties if set.
            if (velocityProperties.Count > 0) {
                foreach (KeyValuePair<string, object> pair in velocityProperties) {
                    props.Add(pair.Key, pair.Value);
                }
            }

            // Set a resource loader path, if required.
            if (null != resourceLoaderPath) {
                InitVelocityResourceLoader(velocityEngine, resourceLoaderPath);
            }

            // Log via Commons Logging?
            if (overrideLogging) {
                velocityEngine.SetProperty(RuntimeConstants.RUNTIME_LOG_LOGSYSTEM, new CommonsLoggingLogSystem());
            }

            PostProcessVelocityEngine(velocityEngine);

            try {
                // do not init with extended properties rather set one by one 
                foreach (DictionaryEntry prop in props) {
                    velocityEngine.SetProperty(Convert.ToString(prop.Key), prop.Value);
                }

                // Perform actual initialization.
                velocityEngine.Init();
            } catch (Exception ex) {
                throw new VelocityException(ex.ToString(), ex);
            }

            return velocityEngine;
        }

        /// <summary>
        ///  Return a new VelocityEngine. Subclasses can override this for* 
        /// custom initialization, or for using a mock object for testing. * <p>Called by 
        /// <code>CreateVelocityEngine()</code>
        /// </summary>
        /// <returns>VelocityEngine</returns>
        /// <see cref="CreateVelocityEngine"/>
        protected static VelocityEngine NewVelocityEngine() {
            return new VelocityEngine();
        }

        /// <summary>
        /// Initialize a Velocity resource loader for the given VelocityEngine:* 
        /// either a standard Velocity FileResourceLoader or a SpringResourceLoader.
        /// <p>Called by <code>CreateVelocityEngine()</code>.
        /// </summary>
        /// <param name="velocityEngine">velocityEngine the VelocityEngine to configure</param>
        /// <param name="resourceLoaderFullPath">resourceLoaderPath the path to load Velocity resources from</param>
        /// <see cref="FileResourceLoader"/>
        /// <see cref="SpringResourceLoader"/>
        /// <see cref="InitSpringResourceLoader"/>
        /// <see cref="CreateVelocityEngine"/>
        protected void InitVelocityResourceLoader(VelocityEngine velocityEngine, String resourceLoaderFullPath) {
            if (PreferFileSystemAccess) {
                // Try to load via the file system, fall back to SpringResourceLoader
                // (for hot detection of template changes, if possible).
                try {
                    StringBuilder resolvedPath = new StringBuilder();
                    String[] paths = resourceLoaderFullPath.Split(DELIMITER);
                    for (int i = 0; i < paths.Length; i++) {
                        string path = paths[i];
                        IResource resource = ResourceLoader.GetResource(path);
                        if (log.IsDebugEnabled) {
                            log.Debug(string.Format("Resource loader path [{0}] resolved to file [{1}]", path, resource.File.FullName));
                        }
                        resolvedPath.Append(resource.File.FullName);
                        if (i < paths.Length - 1) {
                            resolvedPath.Append(',');
                        }
                    }
                    velocityEngine.SetProperty(RuntimeConstants.RESOURCE_LOADER, "file");
                    velocityEngine.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_CACHE, "true");
                    velocityEngine.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, resolvedPath.ToString());
                } catch (IOException ex) {
                    if (log.IsDebugEnabled) {
                        log.Error(string.Format("Cannot resolve resource loader path [{0}] to [File]: using SpringResourceLoader", resourceLoaderFullPath), ex);
                    }
                    InitSpringResourceLoader(velocityEngine, resourceLoaderFullPath);
                }
            } else {
                // Always load via SpringResourceLoader
                // (without hot detection of template changes).
                if (log.IsDebugEnabled) {
                    log.Debug("File system access not preferred: using SpringResourceLoader");
                }
                InitSpringResourceLoader(velocityEngine, resourceLoaderFullPath);
            }
        }

        /// <summary>
        ///  Initialize a SpringResourceLoader for the given VelocityEngine.
        /// <p>Called by <code>InitVelocityResourceLoader</code>.
        /// </summary>
        /// <param name="velocityEngine">velocityEngine the VelocityEngine to configure</param>
        /// <param name="resourceLoaderPathString">resourceLoaderPath the path to load Velocity resources from</param>
        /// <see cref="SpringResourceLoader"/>
        /// <see cref="InitVelocityResourceLoader"/>
        protected void InitSpringResourceLoader(VelocityEngine velocityEngine, string resourceLoaderPathString) {
            velocityEngine.SetProperty(
                RuntimeConstants.RESOURCE_LOADER, SpringResourceLoader.NAME);
            velocityEngine.SetProperty(
                SpringResourceLoader.SPRING_RESOURCE_LOADER_CLASS, typeof(SpringResourceLoader).Name);
            velocityEngine.SetProperty(
                SpringResourceLoader.SPRING_RESOURCE_LOADER_CACHE, "true");
            velocityEngine.SetApplicationAttribute(
                SpringResourceLoader.SPRING_RESOURCE_LOADER, ResourceLoader);
            velocityEngine.SetApplicationAttribute(
                SpringResourceLoader.SPRING_RESOURCE_LOADER_PATH, resourceLoaderPathString);
        }

        /// <summary>
        /// To be implemented by subclasses that want to to perform custom
        /// post-processing of the VelocityEngine after this FactoryBean
        /// performed its default configuration (but before VelocityEngine.init)
        /// <p>Called by <code>CreateVelocityEngine()</code>
        /// </summary>
        /// <param name="velocityEngine">velocityEngine the current VelocityEngine</param>
        /// <exception cref="IOException" />
        /// <see cref="CreateVelocityEngine"/>
        /// <see cref="VelocityEngine.Init()"/>
        protected static void PostProcessVelocityEngine(VelocityEngine velocityEngine) {
        }

        /// <summary>
        /// Populates the velocity properties from the given resource
        /// </summary>
        /// <param name="props">ExtendedProperties instance to populate</param>
        /// <param name="resource">The resource from which to load the properties</param>
        private static void FillProperties(ExtendedProperties props, IInputStreamSource resource) {
            try {
                props.Load(resource.InputStream);
            } finally {
                resource.InputStream.Close();
            }
        }
    }
}