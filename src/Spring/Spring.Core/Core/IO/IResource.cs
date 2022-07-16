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

using System.ComponentModel;

#endregion

namespace Spring.Core.IO
{
	/// <summary>
    /// The central abstraction for Spring.NET's access to resources such as
    /// <see cref="System.IO.Stream"/>s.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This interface encapsulates a resource descriptor that abstracts away
	/// from the underlying type of resource; possible resource types include
	/// files, memory streams, and databases (this list is not exhaustive).
	/// </p>
	/// <p>
	/// A <see cref="System.IO.Stream"/> can definitely be opened and accessed
	/// for every such resource; if the resource exists in a physical form (for
	/// example, the resource is not an in-memory stream or one that has been
	/// extracted from an assembly or ZIP file), a <see cref="System.Uri"/> or
	/// <see cref="System.IO.FileInfo"/> can also be accessed. The actual
	/// behavior is implementation-specific.
	/// </p>
	/// <p>
	/// This interface, when used in tandem with the
	/// <see cref="IResourceLoader"/> interface, forms the backbone of
	/// Spring.NET's resource handling. Third party extensions or libraries
	/// that want to integrate external resources with Spring.NET's IoC
	/// container are encouraged expose such resources via this abstraction.
	/// </p>
	/// <p>
	/// Interfaces cannot obviously mandate implementation, but derived classes
	/// are <b>strongly</b> encouraged to expose a constructor that takes a
	/// single <see cref="System.String"/> as it's sole argument (see example).
	/// Exposing such a constructor will make your custom
	/// <see cref="Spring.Core.IO.IResource"/> implementation integrate nicely
	/// with the <see cref="Spring.Core.IO.ConfigurableResourceLoader"/> class.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	/// <seealso cref="Spring.Core.IO.IResourceLoader"/>
	/// <seealso cref="Spring.Core.IO.ConfigurableResourceLoader"/>
    [TypeConverter(typeof(ResourceConverter))]
	public interface IResource : IInputStreamSource
	{
		/// <summary>
		/// Does this resource represent a handle with an open stream?
		/// </summary>
		/// <remarks>
		/// <p>
		/// If <see langword="true"/>, the <see cref="System.IO.Stream"/>
		/// cannot be read multiple times, and must be read and then closed to
		/// avoid resource leaks.
		/// </p>
		/// <p>
		/// Will be <see langword="false"/> for all usual resource descriptors.
		/// </p>
		/// </remarks>
		/// <value>
		/// <see langword="true"/> if this resource represents a handle with an
		/// open stream.
		/// </value>
		/// <seealso cref="Spring.Core.IO.IInputStreamSource.InputStream"/>
		bool IsOpen { get; }

		/// <summary>
		/// Returns the <see cref="System.Uri"/> handle for this resource.
		/// </summary>
		/// <remarks>
		/// <p>
		/// For safety, always check the value of the
		/// <see cref="Spring.Core.IO.IResource.Exists"/> property prior to
		/// accessing this property; resources that cannot be exposed as 
		/// a <see cref="System.Uri"/> will typically return
		/// <see langword="false"/> from a call to the
		/// <see cref="Spring.Core.IO.IResource.Exists"/> property.
		/// </p>
		/// </remarks>
		/// <value>
		/// The <see cref="System.Uri"/> handle for this resource.
		/// </value>
		/// <exception cref="System.IO.IOException">
		/// If the resource is not available or cannot be exposed as a
		/// <see cref="System.Uri"/>.
		/// </exception>
		/// <seealso cref="Spring.Core.IO.IResource"/>
		/// <seealso cref="Spring.Core.IO.IResource.Exists"/>
		Uri Uri { get; }

		/// <summary>
		/// Returns a <see cref="System.IO.FileInfo"/> handle for this resource.
		/// </summary>
		/// <remarks>
		/// <p>
		/// For safety, always check the value of the
		/// <see cref="Spring.Core.IO.IResource.Exists"/> property prior to
		/// accessing this property; resources that cannot be exposed as 
		/// a <see cref="System.IO.FileInfo"/> will typically return
		/// <see langword="false"/> from a call to the
		/// <see cref="Spring.Core.IO.IResource.Exists"/> property.
		/// </p>
		/// </remarks>
		/// <value>
		/// The <see cref="System.IO.FileInfo"/> handle for this resource.
		/// </value>
		/// <exception cref="System.IO.IOException">
		/// If the resource is not available on a filesystem, or cannot be
		/// exposed as a <see cref="System.IO.FileInfo"/> handle.
		/// </exception>
		/// <seealso cref="Spring.Core.IO.IResource"/>
		/// <seealso cref="Spring.Core.IO.IResource.Exists"/>
		FileInfo File { get; }

		/// <summary>
		/// Returns a description for this resource.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The description is typically used for diagnostics and other such
		/// logging when working with the resource.
		/// </p>
		/// <p>
		/// Implementations are also encouraged to return this value from their
		/// <see cref="System.Object.ToString()"/> method.
		/// </p>
		/// </remarks>
		/// <value>
		/// A description for this resource.
		/// </value>
		string Description { get; }

		/// <summary>
		/// Does this resource actually exist in physical form?
		/// </summary>
		/// <remarks>
		/// <p>
		/// An example of a resource that physically exists would be a
		/// file on a local filesystem. An example of a resource that does not
		/// physically exist would be an in-memory stream.
		/// </p>
		/// </remarks>
		/// <value>
		/// <see langword="true"/> if this resource actually exists in physical
		/// form (for example on a filesystem).
		/// </value>
		/// <seealso cref="Spring.Core.IO.IResource.File"/>
		/// <seealso cref="Spring.Core.IO.IResource.Uri"/>
		bool Exists { get; }

		/// <summary>
		/// Creates a resource relative to this resource.
		/// </summary>
		/// <param name="relativePath">
		/// The path (always resolved as relative to this resource).
		/// </param>
		/// <returns>
		/// The relative resource.
		/// </returns>
		/// <exception cref="System.IO.IOException">
		/// If the relative resource could not be created from the supplied
		/// path.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// If the resource does not support the notion of a relative path.
		/// </exception>
		IResource CreateRelative(string relativePath);
	}
}
