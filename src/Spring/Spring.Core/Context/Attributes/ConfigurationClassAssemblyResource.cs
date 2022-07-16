#region License

/*
 * Copyright � 2010-2011 the original author or authors.
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

using System.Reflection;
using Spring.Core.IO;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Implementation of the IResource that represents an assembly containing one or more <see cref="ConfigurationClass"/> resources.
    /// </summary>
    public class ConfigurationClassAssemblyResource : IResource
    {
        private readonly string _containingAssemblyFileName;
        private readonly Type _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public ConfigurationClassAssemblyResource(Type type)
        {
            _type = type;
            _containingAssemblyFileName = Assembly.GetAssembly(_type.GetType()).Location;
        }

        #region IResource Members

        /// <summary>
        /// Creates a resource relative to this resource.
        /// </summary>
        /// <param name="relativePath">The path (always resolved as relative to this resource).</param>
        /// <returns>The relative resource.</returns>
        /// <exception cref="T:System.IO.IOException">
        /// If the relative resource could not be created from the supplied
        /// path.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// If the resource does not support the notion of a relative path.
        /// </exception>
        public IResource CreateRelative(string relativePath)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Does this resource represent a handle with an open stream?
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this resource represents a handle with an
        /// open stream.
        /// </value>
        /// <remarks>
        /// 	<p>
        /// If <see langword="true"/>, the <see cref="T:System.IO.Stream"/>
        /// cannot be read multiple times, and must be read and then closed to
        /// avoid resource leaks.
        /// </p>
        /// 	<p>
        /// Will be <see langword="false"/> for all usual resource descriptors.
        /// </p>
        /// </remarks>
        /// <seealso cref="P:Spring.Core.IO.IInputStreamSource.InputStream"/>
        public bool IsOpen
        {
            get { return false; }
        }

        /// <summary>
        /// Returns the <see cref="T:System.Uri"/> handle for this resource.
        /// </summary>
        /// <value>The <see cref="T:System.Uri"/> handle for this resource.</value>
        /// <remarks>
        /// 	<p>
        /// For safety, always check the value of the
        /// <see cref="P:Spring.Core.IO.IResource.Exists"/> property prior to
        /// accessing this property; resources that cannot be exposed as
        /// a <see cref="T:System.Uri"/> will typically return
        /// <see langword="false"/> from a call to the
        /// <see cref="P:Spring.Core.IO.IResource.Exists"/> property.
        /// </p>
        /// </remarks>
        /// <exception cref="T:System.IO.IOException">
        /// If the resource is not available or cannot be exposed as a
        /// <see cref="T:System.Uri"/>.
        /// </exception>
        /// <seealso cref="T:Spring.Core.IO.IResource"/>
        /// <seealso cref="P:Spring.Core.IO.IResource.Exists"/>
        public Uri Uri
        {
            get { return new Uri(_containingAssemblyFileName); }
        }

        /// <summary>
        /// Returns a <see cref="T:System.IO.FileInfo"/> handle for this resource.
        /// </summary>
        /// <value>
        /// The <see cref="T:System.IO.FileInfo"/> handle for this resource.
        /// </value>
        /// <remarks>
        /// 	<p>
        /// For safety, always check the value of the
        /// <see cref="P:Spring.Core.IO.IResource.Exists"/> property prior to
        /// accessing this property; resources that cannot be exposed as
        /// a <see cref="T:System.IO.FileInfo"/> will typically return
        /// <see langword="false"/> from a call to the
        /// <see cref="P:Spring.Core.IO.IResource.Exists"/> property.
        /// </p>
        /// </remarks>
        /// <exception cref="T:System.IO.IOException">
        /// If the resource is not available on a filesystem, or cannot be
        /// exposed as a <see cref="T:System.IO.FileInfo"/> handle.
        /// </exception>
        /// <seealso cref="T:Spring.Core.IO.IResource"/>
        /// <seealso cref="P:Spring.Core.IO.IResource.Exists"/>
        public FileInfo File
        {
            get { return new FileInfo(_containingAssemblyFileName); }
        }

        /// <summary>
        /// Returns a description for this resource.
        /// </summary>
        /// <value>A description for this resource.</value>
        /// <remarks>
        /// 	<p>
        /// The description is typically used for diagnostics and other such
        /// logging when working with the resource.
        /// </p>
        /// 	<p>
        /// Implementations are also encouraged to return this value from their
        /// <see cref="M:System.Object.ToString"/> method.
        /// </p>
        /// </remarks>
        public string Description
        {
            get { return _type.FullName; }
        }

        /// <summary>
        /// Does this resource actually exist in physical form?
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this resource actually exists in physical
        /// form (for example on a filesystem).
        /// </value>
        /// <remarks>
        /// 	<p>
        /// An example of a resource that physically exists would be a
        /// file on a local filesystem. An example of a resource that does not
        /// physically exist would be an in-memory stream.
        /// </p>
        /// </remarks>
        /// <seealso cref="P:Spring.Core.IO.IResource.File"/>
        /// <seealso cref="P:Spring.Core.IO.IResource.Uri"/>
        public bool Exists
        {
            get { return System.IO.File.Exists(_containingAssemblyFileName); }
        }

        /// <summary>
        /// Return an <see cref="T:System.IO.Stream"/> for this resource.
        /// </summary>
        /// <value>An <see cref="T:System.IO.Stream"/>.</value>
        /// <remarks>
        /// 	<note type="caution">
        /// Clients of this interface must be aware that every access of this
        /// property will create a <i>fresh</i>
        /// 		<see cref="T:System.IO.Stream"/>;
        /// it is the responsibility of the calling code to close any such
        /// <see cref="T:System.IO.Stream"/>.
        /// </note>
        /// </remarks>
        /// <exception cref="T:System.IO.IOException">
        /// If the stream could not be opened.
        /// </exception>
        public Stream InputStream
        {
            get { throw new InvalidOperationException(); }
        }

        #endregion
    }
}
