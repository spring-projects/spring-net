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

using System.Globalization;
using System.Reflection;
using Common.Logging;

#endregion

namespace Spring.Core.IO
{
    /// <summary>
    /// An <see cref="Spring.Core.IO.IResource"/> implementation for
    /// resources stored within assemblies.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This implementation expects any resource name passed to the
    /// constructor to adhere to the following format:
    /// </p>
    /// <p>
    /// assembly://<i>assemblyName</i>/<i>namespace</i>/<i>resourceName</i>
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic (.NET)</author>
    /// <author>Federico Spinazzi (.NET)</author>
    public class AssemblyResource : AbstractResource
    {
        #region Fields

        private Assembly _assembly;
        private string[] _resources;
        private string _resourceName;
        private string _fullResourceName;
        private string _resourceNamespace;
        private string _resourceAssemblyName;
        private static readonly ILog log = LogManager.GetLogger(typeof(AssemblyResource));

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Core.IO.AssemblyResource"/> class.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the assembly resource.
        /// </param>
        /// <exception cref="System.UriFormatException">
        /// If the supplied <paramref name="resourceName"/> did not conform
        /// to the expected format.
        /// </exception>
        /// <exception cref="System.IO.FileLoadException">
        /// If the assembly specified in the supplied
        /// <paramref name="resourceName"/> was loaded twice with two
        /// different evidences.
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// If the assembly specified in the supplied
        /// <paramref name="resourceName"/> could not be found.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// If the caller does not have the required permission to load
        /// the assembly specified in the supplied
        /// <paramref name="resourceName"/>.
        /// </exception>
        /// <see cref="System.Reflection.Assembly.LoadWithPartialName(string)"/>
        public AssemblyResource(string resourceName)
            : base(resourceName)
        {
            string[] info = GetResourceNameWithoutProtocol(resourceName).Split('/');
            if (info.Length != 3)
            {
                throw new UriFormatException(string.Format("Invalid resource name. Name has to be in 'assembly:<assemblyName>/<namespace>/<resourceName>' format:{0}", resourceName));
            }
            this._assembly = Assembly.Load(info[0]);
            if (this._assembly == null)
            {
                throw new FileNotFoundException("Unable to load assembly [" + info[0] + "]");
            }
            this._fullResourceName = resourceName;
            this._resourceAssemblyName = info[0];
            this._resourceNamespace = info[1];
            this._resourceName = String.Format("{0}.{1}", info[1], info[2]);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Return an <see cref="System.IO.Stream"/> for this resource.
        /// </summary>
        /// <value>
        /// An <see cref="System.IO.Stream"/>.
        /// </value>
        /// <exception cref="System.IO.IOException">
        /// If the stream could not be opened.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// If the caller does not have the required permission to load
        /// the underlying assembly's manifest.
        /// </exception>
        /// <seealso cref="Spring.Core.IO.IInputStreamSource"/>
        /// <see cref="System.Reflection.Assembly.GetManifestResourceStream(string)"/> 
        public override Stream InputStream
        {
            get
            {
                Stream stream = _assembly.GetManifestResourceStream(_resourceName);
                if (stream == null)
                {
                    log.Error("Could not load resource with name = [" + _resourceName +
                        "] from assembly + " + _assembly);
                    log.Error("URI specified = [" + this._fullResourceName + "] Spring.NET URI syntax is 'assembly://assemblyName/namespace/resourceName'.");
                    log.Error("Resource name often has the default namespace prefixed, e.g. 'assembly://MyAssembly/MyNamespace/MyNamespace.MyResource.txt'.");
                }
                return stream;
            }
        }

        /// <summary>
        /// Does the embedded resource specified in the value passed to the
        /// constructor exist?
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this resource actually exists in physical
        /// form (for example on a filesystem).
        /// </value>
        /// <seealso cref="Spring.Core.IO.IResource.Exists"/>
        /// <seealso cref="Spring.Core.IO.IResource.File"/>
        /// <see cref="System.Reflection.Assembly.GetManifestResourceNames()"/> 
        public override bool Exists
        {
            get
            {
                if (_resources == null)
                {
                    _resources = _assembly.GetManifestResourceNames();
                    Array.Sort(_resources);
                }
                return (Array.BinarySearch(_resources, _resourceName) >= 0);
            }
        }

        /// <summary>
        /// Does this <see cref="Spring.Core.IO.IResource"/> support relative
        /// resource retrieval?
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation does support relative resource retrieval, and
        /// so will always return <see langword="true"/>.
        /// </p>
        /// </remarks>
        /// <value>
        /// <see langword="true"/> if this
        /// <see cref="Spring.Core.IO.IResource"/> supports relative resource
        /// retrieval.
        /// </value>
        /// <seealso cref="Spring.Core.IO.AbstractResource.SupportsRelativeResources"/>
        protected override bool SupportsRelativeResources
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the root location of the resource (the assembly name in this
        /// case).
        /// </summary>
        /// <value>
        /// The root location of the resource.
        /// </value>
        /// <seealso cref="Spring.Core.IO.AbstractResource.RootLocation"/>
        protected override string RootLocation
        {
            get { return _resourceAssemblyName; }
        }

        /// <summary>
        /// Gets the current path of the resource (the namespace in which the
        /// target resource was embedded in this case).
        /// </summary>
        /// <value>
        /// The current path of the resource.
        /// </value>
        /// <seealso cref="Spring.Core.IO.AbstractResource.ResourcePath"/>
        protected override string ResourcePath
        {
            get { return _resourceNamespace; }
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
            get { return new char[] { '.' }; }
        }

        /// <summary>
        /// Returns a description for this resource.
        /// </summary>
        /// <value>
        /// A description for this resource.
        /// </value>
        /// <seealso cref="Spring.Core.IO.IResource.Description"/>
        public override string Description
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "assembly [{0}], resource [{1}]", _assembly.FullName, _resourceName);
            }
        }

        /// <summary>
        /// Returns the <see cref="System.Uri"/> handle for this resource.
        /// </summary>
        /// <seealso cref="Spring.Core.IO.IResource.Uri"/>
        public override Uri Uri
        {
            get
            {
                return new Uri(_fullResourceName);
            }
        }

        #endregion

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
            return (resourceName.StartsWith("./") ||
                resourceName.StartsWith("/") ||
                resourceName.StartsWith("../") ||
                resourceName.Split('/').Length != 3);
        }
    }
}
