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

#region Imports

using Spring.Util;

#endregion

namespace Spring.Core.IO
{
    /// <summary>
    /// <see cref="Spring.Core.IO.IResource"/> implementation specifically
    /// for resources served up from a web server.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Uses the <c>System.Web.HttpContext.Current.Server.MapPath</c>
    /// method to resolve the file name for a given resource.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class WebResource : FileSystemResource
    {
        private string absolutePath;

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.Core.IO.WebResource"/> class.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the file system resource (on the server).
        /// </param>
        public WebResource(string resourceName)
            : base(resourceName)
        {
        }

        /// <summary>
        /// Gets those characters that are valid path separators for the
        /// resource type.
        /// </summary>
        /// <value>
        /// Those characters that are valid path separators for the resource
        /// type.
        /// </value>
        /// <seealso cref="Spring.Core.IO.AbstractResource.PathSeparatorChars"/>
        protected override char[] PathSeparatorChars
        {
            get { return new char[] {'/'}; }
        }

        /// <summary>
        /// Resolves the <see cref="System.IO.FileInfo"/> handle
        /// for the supplied <paramref name="resourceName"/>.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the file system resource.
        /// </param>
        /// <returns>
        /// The <see cref="System.IO.FileInfo"/> handle for this resource.
        /// </returns>
        protected override FileInfo ResolveFileHandle(string resourceName)
        {
            this.absolutePath = ResolveResourceNameWithoutProtocol(resourceName);
            if(!this.absolutePath.StartsWith("/"))
            {
                string currentPath = VirtualEnvironment.CurrentVirtualFilePath;
                int n = currentPath.LastIndexOfAny(new char[] {'/', '\\'});
                if(n >= 0)
                {
                    currentPath = currentPath.Substring(0, n);
                }
                this.absolutePath = currentPath + '/' + this.absolutePath;
            }

            return new FileInfo(VirtualEnvironment.MapPath(this.absolutePath));
        }

        /// <summary>
        /// Resolves the root location for the supplied <paramref name="resourceName"/>.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the file system resource.
        /// </param>
        /// <returns>
        /// The root location of the resource.
        /// </returns>
        protected override string ResolveRootLocation(string resourceName)
        {
            return string.Empty;
        }

        /// <summary>
        /// Resolves the path for the supplied <paramref name="resourceName"/>.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the file system resource.
        /// </param>
        /// <returns>
        /// The current path of the resource.
        /// </returns>
        protected override string ResolveResourcePath(string resourceName)
        {
            string path = this.absolutePath.TrimStart(PathSeparatorChars);
            int n = path.LastIndexOfAny(PathSeparatorChars);
            if(n > 0)
            {
                path = path.Substring(0, n);
            }
            else
            {
                path = string.Empty;
            }

            return path;
        }

        /// <summary>
        /// Resolves the presence of the
        /// <paramref name="basePathPlaceHolder"/> value
        /// in the supplied <paramref name="resourceName"/> into a path.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the resource.
        /// </param>
        /// <param name="basePathPlaceHolder">
        /// The string that is a placeholder for a base path.
        /// </param>
        /// <returns>
        /// The name of the resource with any <paramref name="basePathPlaceHolder"/>
        /// value having been resolved into an actual path.
        /// </returns>
        protected override string ResolveBasePathPlaceHolder(
            string resourceName, string basePathPlaceHolder)
        {
            if(StringUtils.HasText(resourceName)
               && resourceName.TrimStart().StartsWith(basePathPlaceHolder))
            {
                return resourceName.Replace(basePathPlaceHolder, VirtualEnvironment.ApplicationVirtualPath.TrimEnd('/'));
            }
            return resourceName;
        }

        /// <summary>
        /// Does the supplied <paramref name="resourceName"/> relative ?
        /// </summary>
        /// <param name="resourceName">
        /// The name of the resource to test.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if resource name is relative;
        /// otherwise <see langword="false"/>.
        /// </returns>
        protected override bool IsRelativeResource(string resourceName)
        {
            return (!
                    (resourceName.StartsWith("/") ||
                     resourceName.StartsWith(BasePathPlaceHolder)));
        }

        /// <summary>
        /// Factory Method. Create a new instance of the current resource type using the given resourceName
        /// </summary>
        protected override IResource CreateResourceInstance(string resourceName)
        {
            return new WebResource(resourceName);
        }

        /// <summary>
        /// The ResourceLoader to be used for resolving relative resources
        /// </summary>
        protected override IResourceLoader GetResourceLoader()
        {
            return new ConfigurableResourceLoader(WebUtils.DEFAULT_RESOURCE_PROTOCOL);
        }
    }
}
