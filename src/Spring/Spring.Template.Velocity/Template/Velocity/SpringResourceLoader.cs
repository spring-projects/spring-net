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
using System.IO;
using Common.Logging;
using Commons.Collections;
using NVelocity.Runtime.Resource;
using NVelocity.Runtime.Resource.Loader;
using Spring.Core.IO;

namespace Spring.Template.Velocity
{
    /// <summary>
    /// Velocity ResourceLoader adapter that loads via a Spring IResourceLoader.
    /// Used by VelocityEngineFactory for any resource loader path that cannot
    /// be resolved to a File
    ///
    /// <p>Take notoce that this loader does not allow for modification detection:
    /// Use Velocity's default FileResourceLoader for File resources.
    ///
    /// <p>Expects "spring.resource.loader" and "spring.resource.loader.path"
    /// application attributes in the Velocity runtime: the former of type
    /// <code>IResourceLoader</code>, the latter a String.
    /// </summary>
    /// <see cref="VelocityEngineFactory.ResourceLoaderPath"/>
    /// <see cref="IResourceLoader"/>
    /// <see cref="FileResourceLoader"/>
    /// <author>Erez Mazor (.NET) </author>
    public class SpringResourceLoader : ResourceLoader {
        public static readonly string NAME = "spring";

        public static readonly string SPRING_RESOURCE_LOADER_CLASS = "spring.resource.loader.class";

        public static readonly string SPRING_RESOURCE_LOADER_CACHE = "spring.resource.loader.cache";

        public static readonly string SPRING_RESOURCE_LOADER = "spring.resource.loader";

        public static readonly string SPRING_RESOURCE_LOADER_PATH = "spring.resource.loader.path";

        protected static readonly ILog log = LogManager.GetLogger(typeof(SpringResourceLoader));

        private IResourceLoader resourceLoader;

        private string[] resourceLoaderPaths;


        /// <summary>
        /// Initialize the template loader with a resources class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public override void Init(ExtendedProperties configuration) {
            resourceLoader = (IResourceLoader)rsvc.GetApplicationAttribute(SPRING_RESOURCE_LOADER);
            string resourceLoaderPath = (string)rsvc.GetApplicationAttribute(SPRING_RESOURCE_LOADER_PATH);
            if (resourceLoader == null) {
                throw new ArgumentException("'resourceLoader' application attribute must be present for SpringResourceLoader");
            }
            if (resourceLoaderPath == null) {
                throw new ArgumentException("'resourceLoaderPath' application attribute must be present for SpringResourceLoader");
            }
            resourceLoaderPaths = resourceLoaderPath.Split(',');
            for (int i = 0; i < resourceLoaderPaths.Length; i++) {
                string path = resourceLoaderPaths[i];
                if (!path.EndsWith("/")) {
                    resourceLoaderPaths[i] = path + "/";
                }
            }
            if (log.IsInfoEnabled) {
                log.Info(string.Format("SpringResourceLoader for Velocity: using resource loader [{0}] and resource loader paths {1}", resourceLoader, resourceLoaderPaths));
            }
        }

        /// <summary>
        /// Get the InputStream that the Runtime will parse to create a template.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public override Stream GetResourceStream(string source) {
            if (log.IsDebugEnabled) {
                log.Debug(string.Format("Looking for Velocity resource with name [{0}]", source));
            }
            for (int i = 0; i < resourceLoaderPaths.Length; i++) {
                IResource resource =
                    resourceLoader.GetResource(resourceLoaderPaths[i] + source);
                try {
                    return resource.InputStream;
                } catch (IOException ex) {
                    if (log.IsErrorEnabled) {
                        log.Error(string.Format("Could not find Velocity resource: {0}", resource), ex);
                    }
                }
            }
            throw new ArgumentException(string.Format("Could not find resource [{0}] in Spring resource loader path", source));
        }

        /// <summary>
        ///  Given a template, check to see if the source of InputStream has been modified.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns>
        /// 	<c>true</c> if the source of the InputStream has been modified; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsSourceModified(Resource resource) {
            return false;
        }

        /// <summary>
        /// Get the last modified time of the InputStream source
        /// that was used to create the template. We need the template
        /// here because we have to extract the name of the template
        /// in order to locate the InputStream source.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns></returns>
        public override long GetLastModified(Resource resource) {
            return 0;
        }
    }
}